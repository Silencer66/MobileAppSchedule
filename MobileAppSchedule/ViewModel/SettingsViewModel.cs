using MobileAppSchedule.Model.ScheduleModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using MobileAppSchedule.ViewModel.Base;
using System.Threading.Tasks;
using Xamarin.Essentials;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using MobileAppSchedule.Model.University;
using MobileAppSchedule.Model.Worker;
using System.Linq;
using System.Net.Http;
using System.Net;
using System;

namespace MobileAppSchedule.ViewModel
{
    internal class SettingsViewModel : BaseViewModel
    {
        #region Fields

        private List<string> rowGroups;
        readonly string url = "https://www.madi.ru/tplan/tasks/task3,7_fastview.php";

        #endregion

        #region Свойства
        public ObservableCollection<string> GroupNames { get; set; }

        #region Schedule

        private Schedule _schedule;

        public Schedule Schedule
        {
            get => _schedule;
            set => Set(ref _schedule, value);
        }
        #endregion

        #region SelectedGroup
        private string _selectedGroup;
        /// <summary> Выбранная пользователем группа </summary>
        public string SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                Set(ref _selectedGroup, value);

                object name = "";
                App.Current.Properties.TryGetValue("group_name", out name);
                if (name == null || name.ToString() != _selectedGroup)
                {
                    App.Current.Properties["group_name"] = _selectedGroup;
                    App.Current.SavePropertiesAsync().GetAwaiter();

                    IsBusy = true;

                    GetSchedule();
                }
            }
        }
        #endregion

        #endregion

        #region Команды

        #endregion
        public SettingsViewModel()
        {
            #region Properties
            Schedule = new Schedule();
            GroupNames = new ObservableCollection<string>();
            #endregion


            #region Команды
            #endregion
        }

        /// <summary> Преобразует список групп к нормальному виду </summary>
        /// <param name="names"></param>
        private void GetGroupNames(List<string> names)
        {
            foreach (var groupName in names)
            {
                //var group = Regex.Matches(groupName, @"[\d\s\w]*")[6];
                GroupNames.Add(Regex.Matches(groupName, @"[\d\s\w]*")[6].ToString());
            }
        }
        /// <summary> Скачивает с сайта список всех групп </summary>
        /// <param name="httpClient"></param>
        /// <returns>Возвращает список в сыром виде</returns>
        private async Task<List<string>> GetRowGroups(HttpClient httpClient)
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
            //var array = sourceString.Split("value=\"", StringSplitOptions.None).ToList();
            var array = new Regex("value=\"").Split(sourceString).ToList();
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


        
        /// <summary> Парсит расписание по выбранной группе </summary>
        async void GetSchedule()
        {
            UniversitySettings settings = new UniversitySettings(rowGroups);
            UniversityParser parser = new UniversityParser(_schedule);

            ParserWorker<Schedule> worker = new ParserWorker<Schedule>(parser, settings);

            worker.OnComplete += OnCompleteParse;
            worker.OnGroupSchedule += OnNewDataParse;

            await worker.LoadDataByGroupName(GroupNames.IndexOf(_selectedGroup));
            //Task.Run(() => worker.LoadDataByGroupName(GroupNames.IndexOf(_selectedGroup)).GetAwaiter());
        }


        private void OnNewDataParse(object obj, Schedule schedule)
        {
            _schedule = schedule;
        }
        private void OnCompleteParse(object obj)
        {
            IsBusy = false;
        }
        public void OnAppearing()
        {
            if (GroupNames.Count > 0)
                return;

            IsBusy = true;
            //Однократно загружаем список групп c сайта
            if (!App.Current.Properties.ContainsKey("group_list"))
            {
                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    DisplayAlert()
                    return;
                }
                rowGroups = Task.Run(() => GetRowGroups(new HttpClient()).GetAwaiter().GetResult()).Result;
                App.Current.Properties["group_list"] = "exists";
                App.Current.SavePropertiesAsync().GetAwaiter();
            }
            else //Загружаем список из памяти
            {
                var path = FileSystem.CacheDirectory;
                var fullpath = Path.Combine(path, "mobileschedule_groups.txt");
                rowGroups = ParseGroupNames(File.ReadAllText(fullpath));
            }
            GetGroupNames(rowGroups);
            IsBusy = false;
        }
    }
}
