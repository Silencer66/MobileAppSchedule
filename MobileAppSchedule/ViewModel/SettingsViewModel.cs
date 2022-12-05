using MobileAppSchedule.Model.ScheduleModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using AngleSharp.Html.Parser;
using MobileAppSchedule.Model.University;
using MobileAppSchedule.Model.Worker;
using MvvmHelpers;
using Group = MobileAppSchedule.Model.ScheduleModel.Group;
using Path = System.IO.Path;
using Newtonsoft.Json;
using Xamarin.Forms;
using BaseViewModel = MobileAppSchedule.ViewModel.Base.BaseViewModel;

namespace MobileAppSchedule.ViewModel
{
    internal class SettingsViewModel : BaseViewModel
    {
        #region Fields

        private List<string> rowGroups;
        private List<Group> _groupNamesNotForView;
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
                    GroupNamesForView.Clear();
                    var path = FileSystem.CacheDirectory;
                    var fullpath = Path.Combine(path, "mobileschedule_groups.txt");
                    var json = File.ReadAllText(fullpath);
                    GroupNamesForView.AddRange(_groupNamesNotForView);
                }
                var letter = _textSearchBar.ToUpper();
                GroupNamesForView = new ObservableRangeCollection<Group>(_groupNamesNotForView.Where(x => x.GroupName.ToUpperInvariant().Contains(letter)));
            }
        }
        #endregion

        #region GroupNames
        private ObservableRangeCollection<Group> _groupNamesForView;
        public ObservableRangeCollection<Group> GroupNamesForView
        {
            get => _groupNamesForView;
            set
            {
                Set(ref _groupNamesForView, value);
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
                        GetScheduleAsync();
                    }
                }
            }
        }
        #endregion

        #endregion

        #region Commands

        #region AddToFavouriteCommand
        public ICommand AddToFavouriteCommand { get; }
        #endregion

        #region RefreshCommand
        public MvvmHelpers.Commands.AsyncCommand RefreshCommand { get; }
        #endregion

        #endregion

        public SettingsViewModel()
        {
            #region Fields
            Schedule = new Schedule();
            GroupNamesForView = new ObservableRangeCollection<Group>();
            _groupNamesNotForView = new List<Group>();
            rowGroups = new List<string>();
            Title = "Расписание";
            #endregion

            #region Commands

            RefreshCommand = new MvvmHelpers.Commands.AsyncCommand(Refresh);
            AddToFavouriteCommand = new Command(AddToFavourite);

            #endregion
        }

        private void AddToFavourite(object arg)
        {
            var path = FileSystem.CacheDirectory;
            string groupname = arg.ToString();

            _groupNamesNotForView.Where(a => a.GroupName == groupname).FirstOrDefault().IsFavorite =
                !_groupNamesNotForView.Where(a => a.GroupName == groupname).FirstOrDefault().IsFavorite;

            if (_groupNamesNotForView.Where(a => a.GroupName == groupname).FirstOrDefault().IsFavorite == true)
            {
                GroupNamesForView.Where(a => a.GroupName == groupname).FirstOrDefault().FavouriteImage = "filled_star.png";
                _groupNamesNotForView.Where(a => a.GroupName == groupname).FirstOrDefault().FavouriteImage = "filled_star.png";
            }
            else
            {
                _groupNamesNotForView.Where(a => a.GroupName == groupname).FirstOrDefault().FavouriteImage = "star.png";
                GroupNamesForView.Where(a => a.GroupName == groupname).FirstOrDefault().FavouriteImage = "star.png";
            }
            
            var FavouritesGroups = _groupNamesNotForView.Where(a => a.IsFavorite == true).ToList();
            var json_favourites = JsonConvert.SerializeObject(FavouritesGroups);
            var fullpath_for_favourites = Path.Combine(path, "mobileschedule_favourite_groups.txt");
            File.WriteAllText(fullpath_for_favourites, json_favourites);

            var json = JsonConvert.SerializeObject(_groupNamesNotForView);
            var fullpath_for_groups = Path.Combine(path, "mobileschedule_groups.txt");
            File.WriteAllText(fullpath_for_groups, json);
        }

        private async Task Refresh()
        {
            await UpdateGroupsAsync();
        }

        private async Task UpdateGroupsAsync()
        {
            
            IsBusy = true;
            _groupNamesNotForView.Clear();
            GroupNamesForView.Clear();
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
                await App.Current.SavePropertiesAsync();
            }
            else //Загружаем список из памяти
            {

                var path = FileSystem.CacheDirectory;
                var fullpath = Path.Combine(path, "mobileschedule_groups.txt");
                if (File.Exists(fullpath))
                {
                    var json = File.ReadAllText(fullpath);
                    _groupNamesNotForView = JsonConvert.DeserializeObject<List<Group>>(json);
                    GroupNamesForView = JsonConvert.DeserializeObject<ObservableRangeCollection<Group>>(json);
                    IsBusy = false;
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Расписание", "Пожалуйста обновите страницу", "Закрыть");
                    App.Current.Properties.Remove("group_list");
                    await App.Current.SavePropertiesAsync();
                }
            }
        }

        /// <summary> Парсит расписание по выбранной группе </summary>
        private async void GetScheduleAsync()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await App.Current.MainPage.DisplayAlert("Расписание", "Невозможно загрузить расписание выбранной группы.\nПроверьте подключение к интернету", "Закрыть");
                return;
            }

            var path = FileSystem.CacheDirectory;
            var fullpath = Path.Combine(path, "mobileschedule_row_groups.txt");
            var json = File.ReadAllText(fullpath);
            rowGroups = JsonConvert.DeserializeObject<List<string>>(json);

            UniversitySettings settings = new UniversitySettings(rowGroups, url: _urlForSchedule);
            UniversityParser parser = new UniversityParser(Schedule);
            ParserWorker<Schedule> worker = new ParserWorker<Schedule>(parser, settings);

            worker.OnGroupSchedule += OnNewSchedule;
            await worker.LoadScheduleByGroupName(rowGroups.Where(a => a.Contains(_selectedGroup.GroupName)).FirstOrDefault());
        }


        public void OnAppearing()
        {
            object name = "";
            App.Current.Properties.TryGetValue("group_name", out name);
            if (name != null && !string.IsNullOrEmpty(name.ToString()))
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

            App.Current.Properties["group_name"] = SelectedGroup.GroupName;
            App.Current.SavePropertiesAsync();
        }

        private async void OnListGroups(object obj, string source)
        {
            var path = FileSystem.CacheDirectory;
            var fullpath = Path.Combine(path, "mobileschedule_groups.txt");

            UniversityParser parser = new UniversityParser();
            var domParser = new HtmlParser();
            var document = await domParser.ParseDocumentAsync(source);

            rowGroups = parser.ParseGroupNames(document);
            var fullpath_for_row_groups = Path.Combine(path, "mobileschedule_row_groups.txt");
            var json_row_groups = JsonConvert.SerializeObject(rowGroups);
            File.WriteAllText(fullpath_for_row_groups, json_row_groups);

            GetGroupNames(rowGroups);
            var json = JsonConvert.SerializeObject(_groupNamesNotForView);
            File.WriteAllText(fullpath, json);
            IsBusy = false;
        }

        /// <summary> Преобразует список групп к нормальному виду </summary>
        /// <param name="names"></param>
        private void GetGroupNames(List<string> names)
        {
            foreach (var groupName in names)
            {
                GroupNamesForView.Add(new Group(Regex.Matches(groupName, @"[\d\s\w]*")[6].ToString(), false, "star.png"));
                _groupNamesNotForView.Add(new Group(Regex.Matches(groupName, @"[\d\s\w]*")[6].ToString(), false, "star.png"));
            }
        }
    }
}