using System.Collections.Generic;
using MobileAppSchedule.Model.ScheduleModel;
using MvvmHelpers.Commands;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using MobileAppSchedule.Model.University;
using MobileAppSchedule.Model.Worker;
using MvvmHelpers;
using BaseViewModel = MobileAppSchedule.ViewModel.Base.BaseViewModel;

namespace MobileAppSchedule.ViewModel
{
    internal class FavouritesGroupsViewModel : BaseViewModel
    {
        readonly string _urlForSchedule = "https://www.madi.ru/tplan/tasks/tableFiller.php";
        private List<string> rowGroups;

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
                    var path = FileSystem.CacheDirectory;
                    var fullpath = Path.Combine(path, "mobileschedule_groups.txt");
                    var json = File.ReadAllText(fullpath);
                    GroupNames = JsonConvert.DeserializeObject<List<Group>>(json);
                }
                var letter = _textSearchBar.ToUpper();
                GroupNames = new List<Group>(GroupNames.Where(x => x.GroupName.ToUpperInvariant().Contains(letter)));
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

        public List<Group> GroupNames { get; set; }

        #region FavouritesGroups
        private ObservableRangeCollection<Group> _favouritesGroups;
        public ObservableRangeCollection<Group> FavouritesGroups
        {
            get => _favouritesGroups;
            set
            {
                Set(ref _favouritesGroups, value);
            }
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
                //Навигация на другую страницу с передачей имени группы параметром
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

        public ICommand RemoveFavouriteCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        public FavouritesGroupsViewModel()
        {
            #region Commands
            RefreshCommand = new Command(Refresh);
            RemoveFavouriteCommand = new Command(RemoveFavourite);
            #endregion

            #region Fields
            Title = "Расписание";
            GroupNames = new List<Group>();
            FavouritesGroups = new ObservableRangeCollection<Group>();
            #endregion
        }

        #region Methods

        public void OnAppearing()
        {
            object name = "";
            App.Current.Properties.TryGetValue("group_name", out name);
            if (name != null && !string.IsNullOrEmpty(name.ToString()))
                Title = "Расписание " + App.Current.Properties["group_name"];
            Refresh();
        }

        private void RemoveFavourite(object arg)
        {
            var path = FileSystem.CacheDirectory;
            string groupname = arg.ToString();

            GroupNames.Where(a => a.GroupName == groupname).FirstOrDefault().IsFavorite =
                !GroupNames.Where(a => a.GroupName == groupname).FirstOrDefault().IsFavorite;

            FavouritesGroups.Where(a => a.GroupName == groupname).FirstOrDefault().IsFavorite =
                !FavouritesGroups.Where(a => a.GroupName == groupname).FirstOrDefault().IsFavorite;

            if (GroupNames.Where(a => a.GroupName == groupname).FirstOrDefault().IsFavorite == true)
            {
                GroupNames.Where(a => a.GroupName == groupname).FirstOrDefault().FavouriteImage = "filled_star.png";
                FavouritesGroups.Where(a => a.GroupName == groupname).FirstOrDefault().FavouriteImage = "filled_star.png";
            }
            else
            {
                GroupNames.Where(a => a.GroupName == groupname).FirstOrDefault().FavouriteImage = "star.png";
                FavouritesGroups.Where(a => a.GroupName == groupname).FirstOrDefault().FavouriteImage = "star.png";
            }

            var favouritesGroups = GroupNames.Where(a => a.IsFavorite == true).ToList();
            var json_favourites = JsonConvert.SerializeObject(favouritesGroups);
            var fullpath_favourites_groups = Path.Combine(path, "mobileschedule_favourite_groups.txt");
            File.WriteAllText(fullpath_favourites_groups, json_favourites);

            var json = JsonConvert.SerializeObject(GroupNames);
            var fullpath_groupnames = Path.Combine(path, "mobileschedule_groups.txt");
            File.WriteAllText(fullpath_groupnames, json);
        }

        private void Refresh()
        {
            IsBusy = true;
            var path = FileSystem.CacheDirectory;
            var fullpath_favourites_groups = Path.Combine(path, "mobileschedule_favourite_groups.txt");
            if (File.Exists(fullpath_favourites_groups)) //Однократно загружаем список групп c сайта
            {
                var json_favourites = File.ReadAllText(fullpath_favourites_groups);
                FavouritesGroups = JsonConvert.DeserializeObject<ObservableRangeCollection<Group>>(json_favourites);
            }

            var fullpath_groupnames = Path.Combine(path, "mobileschedule_groups.txt");
            if (File.Exists(fullpath_groupnames))
            {
                var json_groupnames = File.ReadAllText(fullpath_groupnames);
                GroupNames = JsonConvert.DeserializeObject<List<Group>>(json_groupnames);
            }
            IsBusy = false;
        }


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
        #endregion
    }
}
