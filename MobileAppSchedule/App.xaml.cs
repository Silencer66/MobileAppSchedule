using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using AngleSharp.Html.Parser;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MobileAppSchedule.Model.ScheduleModel;
using MobileAppSchedule.Services;
using MobileAppSchedule.View;
using MobileAppSchedule.ViewModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MobileAppSchedule
{
    public partial class App : Application
    {
        readonly string url = "https://www.madi.ru/tplan/tasks/task3,7_fastview.php";
        private IPageService _pageService;
        private Schedule _schedule;
        private List<string> _groupNames;
        private Model.Model _model;

        public App()
        {
            _pageService = new PageService();
            _schedule = new Schedule();
            _groupNames = new List<string>();
            InitializeComponent();
        }

        protected override void OnStart()
        {
            //Однократно загружаем список групп c сайта
            if (!Properties.ContainsKey("group_list"))
            {
                _groupNames = Task.Run(() => GetGroupNames(new HttpClient()).GetAwaiter().GetResult()).Result;
                Properties["group_list"] = "exists";
                Current.SavePropertiesAsync().GetAwaiter();
            }
            else //Загружаем список из памяти
            {
                var path = FileSystem.CacheDirectory;
                var fullpath = Path.Combine(path, "mobileschedule_groups.txt");
                _groupNames = ParseGroupNames(File.ReadAllText(fullpath));
            }
            _model = new Model.Model(_schedule, _groupNames, _pageService);
            MainPage = new NavigationPage(new MainPage()
            {
                BindingContext = new MainViewModel(_model)
            });
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        private async Task<List<string>> GetGroupNames(HttpClient httpClient)
        {
            var response = await httpClient.GetAsync(url);
            if (response == null || response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Ответ пустой");
            }
            var responseBody = await response.Content.ReadAsStringAsync();

            var domParser = new HtmlParser();
            var document = await domParser.ParseDocumentAsync(responseBody);
            var sourceString = document.Source.Text;

            //записываем спаршеную строку в файл
            var path = FileSystem.CacheDirectory;
            var fullpath = Path.Combine(path, "mobileschedule_groups.txt");
            File.WriteAllText(fullpath, sourceString);

            return ParseGroupNames(sourceString);
        }

        private List<string> ParseGroupNames(string sourceString)
        {
            var array = sourceString.Split("value=\"", StringSplitOptions.None).ToList();
            array.RemoveAt(0);
            Regex rg = new Regex(">([\\s\\S]+?)<");

            var resultList = new List<string>();
            foreach (var item in array)
            {
                var res_string = "tab=7&gp_name=";
                string id = item.Substring(0, 4);
                string name = rg.Match(item).ToString().TrimEnd(new char[] { '<' }).TrimStart(new char[] { '>' });

                res_string += name + "&gp_id=" + id;
                resultList.Add(res_string);
            }
            return resultList;
        }
    }
}
