using MobileAppSchedule.Model.ScheduleModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MobileAppSchedule.ViewModel.Base;
using System.Threading.Tasks;
using Xamarin.Essentials;
using AngleSharp.Html.Parser;
using MobileAppSchedule.Model.University;
using MobileAppSchedule.Model.Worker;
using Group = MobileAppSchedule.Model.ScheduleModel.Group;
using Path = System.IO.Path;

namespace MobileAppSchedule.ViewModel
{
    internal class SettingsViewModel : BaseViewModel
    {
        #region Fields

        private List<string> rowGroups;
        readonly string _urlForGroupNames = "https://www.madi.ru/tplan/tasks/task3,7_fastview.php";
        readonly string _urlForSchedule = "https://www.madi.ru/tplan/tasks/tableFiller.php";

        #endregion

        #region Properties

        #region TextSearchBar
        private string _textSearchBar;
        public string TextSearchBar
        {
            get => _textSearchBar;
            set
            {
                Set(ref _textSearchBar, value);
                if (string.IsNullOrEmpty(_textSearchBar))
                {
                    GroupNames.Clear();
                    GetGroupNames(rowGroups);
                }

                var letter = _textSearchBar.ToUpper();
                GroupNames = new ObservableCollection<Group>(GroupNames.Where(x => x.GroupName.ToUpperInvariant().Contains(letter)));
            }
        }
        #endregion



        #region GroupNames

        private ObservableCollection<Group> _groupNames;
        public ObservableCollection<Group> GroupNames
        {
            get => _groupNames;
            set
            {
                Set(ref _groupNames, value);
            }
        }
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
        private Group _selectedGroup;
        /// <summary> Выбранная пользователем группа </summary>
        public Group SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                Set(ref _selectedGroup, value);
                if (_selectedGroup != null)
                {
                    object name = "";
                    App.Current.Properties.TryGetValue("group_name", out name);
                    if (name == null || name.ToString() != _selectedGroup.GroupName)
                    {
                        App.Current.Properties["group_name"] = _selectedGroup.GroupName;
                        App.Current.SavePropertiesAsync();

                        IsBusy = true;

                        GetScheduleAsync();
                    }
                }
            }
        }
        #endregion

        #endregion

        #region Commands

        #region RefreshCommand
        public MvvmHelpers.Commands.AsyncCommand RefreshCommand { get; }
        #endregion

        #endregion

        public SettingsViewModel()
        {
            #region Fields
            Schedule = new Schedule();
            GroupNames = new ObservableCollection<Group>();
            rowGroups = new List<string>();
            Title = "Расписание";
            #endregion

            #region Commands

            RefreshCommand = new MvvmHelpers.Commands.AsyncCommand(Refresh);

            #endregion
        }

        private async Task Refresh()
        {
            if (GroupNames.Count > 0)
            {
                IsBusy = true;
                IsBusy = false;
                return;
            }

            if (!App.Current.Properties.ContainsKey("group_list")) //Однократно загружаем список групп c сайта
            {
                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    await App.Current.MainPage.DisplayAlert("Расписание", "Не удается загрузить список групп.\nПроверьте подключение к интернету", "Закрыть");
                    IsBusy = false;
                    return;
                }

                UniversityParser parser = new UniversityParser(Schedule);
                UniversitySettings settings = new UniversitySettings(rowGroups, url: _urlForGroupNames);

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
                if (File.Exists(fullpath))
                {
                    OnListGroups(this, File.ReadAllText(fullpath));
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Расписание", "Пожалуйста обновите страницу", "Закрыть");
                    App.Current.Properties.Remove("group_list");
                    App.Current.SavePropertiesAsync().GetAwaiter();
                    IsBusy = false;
                }
            }
        }

        /// <summary> Парсит расписание по выбранной группе </summary>
        private async void GetScheduleAsync()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await App.Current.MainPage.DisplayAlert("Расписание", "Невозможно загрузить расписание выбранной группы.\nПроверьте подключение к интернету", "Закрыть");
                IsBusy = false;
                return;
            }

            UniversitySettings settings = new UniversitySettings(rowGroups, url: _urlForSchedule);
            UniversityParser parser = new UniversityParser(_schedule);
            ParserWorker<Schedule> worker = new ParserWorker<Schedule>(parser, settings);

            worker.OnGroupSchedule += OnNewSchedule;
            await worker.LoadScheduleByGroupName(rowGroups.Where(a => a.Contains(_selectedGroup.GroupName)).FirstOrDefault());
        }


        public void OnAppearing()
        {
            object name = "";
            App.Current.Properties.TryGetValue("group_name", out name);
            if (name != null && string.IsNullOrEmpty(name.ToString()))
                Title = "Расписание " + App.Current.Properties["group_name"];
            IsBusy = true;
        }

        private void OnNewSchedule(object obj, string source)
        {
            var path = FileSystem.CacheDirectory;
            var fullpath = Path.Combine(path, "mobileschedule_schedule.txt");
            File.WriteAllText(fullpath, source);
            Title = "Расписание " + SelectedGroup.GroupName;
            TextSearchBar = "";
            IsBusy = false;
        }

        private async void OnListGroups(object obj, string source)
        {

            var path = FileSystem.CacheDirectory;
            var fullpath = Path.Combine(path, "mobileschedule_groups.txt");
            File.WriteAllText(fullpath, source);
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
                GroupNames.Add(new Group(Regex.Matches(groupName, @"[\d\s\w]*")[6].ToString(), false));
        }
    }
}