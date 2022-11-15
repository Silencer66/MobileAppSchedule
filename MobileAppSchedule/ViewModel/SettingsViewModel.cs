using MobileAppSchedule.Model.ScheduleModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using MobileAppSchedule.ViewModel.Base;
using System.Threading.Tasks;
using Xamarin.Essentials;
using AngleSharp.Html.Parser;
using MobileAppSchedule.Model.University;
using MobileAppSchedule.Model.Worker;
using System.Net.Http;
using System.Net;
using System;
using MvvmHelpers.Commands;
using Path = System.IO.Path;

namespace MobileAppSchedule.ViewModel
{
    internal class SettingsViewModel : BaseViewModel
    {
        #region Fields

        private List<string> rowGroups;
        readonly string url = "https://www.madi.ru/tplan/tasks/task3,7_fastview.php";

        #endregion

        #region Properties

        #region GroupNames
        public ObservableCollection<string> GroupNames { get; set; }
        #endregion

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

                    GetScheduleAsync();
                }
            }
        }
        #endregion

        #endregion

        #region Commands

        private AsyncCommand RefreshCommand { get; }
        #endregion

        public SettingsViewModel()
        {
            #region Fields
            Schedule = new Schedule();
            GroupNames = new ObservableCollection<string>();
            rowGroups = new List<string>();
            Title = "Расписание";
            #endregion

            #region Commands

            RefreshCommand = new AsyncCommand(Refresh);

            #endregion
        }

        async Task Refresh()
        {
            IsBusy = true;

            OnAppearing();

            IsBusy = false;
        }

        /// <summary> Парсит расписание по выбранной группе </summary>
        private async void GetScheduleAsync()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                App.Current.MainPage.DisplayAlert("Расписание", "Невозможно загрузить расписание выбранной группы.\nПроверьте подключение к интернету", "Закрыть");
                IsBusy = false;
                return;
            }

            UniversitySettings settings = new UniversitySettings(rowGroups);
            UniversityParser parser = new UniversityParser(_schedule);
            ParserWorker<Schedule> worker = new ParserWorker<Schedule>(parser, settings);

            worker.OnGroupSchedule += OnNewSchedule;
            await worker.LoadScheduleByGroupName(GroupNames.IndexOf(_selectedGroup));
        }
        
        
        public async void OnAppearing()
        {
            if (GroupNames.Count > 0)
                return;

            IsBusy = true;
            //Однократно загружаем список групп c сайта
            if (!App.Current.Properties.ContainsKey("group_list"))
            {
                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    App.Current.MainPage.DisplayAlert("Расписание", "Не удается загрузить список групп.\nПроверьте подключение к интернету", "Закрыть");
                    IsBusy = false;
                    return;
                }

                UniversityParser parser = new UniversityParser(Schedule);
                UniversitySettings settings = new UniversitySettings(rowGroups);

                ParserWorker<Schedule> worker = new ParserWorker<Schedule>(parser, settings);
                worker.OnGroups += OnListGroups;

                await worker.LoadGroupNames();

                App.Current.Properties["group_list"] = "exists";
                App.Current.SavePropertiesAsync().GetAwaiter();
            }
            else //Загружаем список из памяти
            {
                var path = FileSystem.CacheDirectory;
                var fullpath = Path.Combine(path, "mobileschedule_groups.txt");

                OnListGroups(this, File.ReadAllText(fullpath));
            }
        }

        private void OnNewSchedule(object obj, string source)
        {
            var path = FileSystem.CacheDirectory;
            var fullpath = Path.Combine(path, "mobileschedule_schedule.txt");
            File.WriteAllText(fullpath, source);
            Title = "Расписание " + SelectedGroup;
            IsBusy = false;
        }

        private async void OnListGroups(object obj, string source)
        {
            UniversityParser parser = new UniversityParser();

            var domParser = new HtmlParser();
            var document = await domParser.ParseDocumentAsync(source);

            rowGroups = parser.ParseGroupNames(document);
            GetGroupNames(rowGroups);
            IsBusy = false;
        }

        /// <summary> Преобразует список групп к нормальному виду </summary>
        /// <param name="names"></param>
        private void GetGroupNames(List<string> names)
        {
            foreach (var groupName in names)
                GroupNames.Add(Regex.Matches(groupName, @"[\d\s\w]*")[6].ToString());
        }
    }
}
