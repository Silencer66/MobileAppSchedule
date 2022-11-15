using System.IO;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using MobileAppSchedule.Model.ScheduleModel;
using Xamarin.Essentials;
using DayOfWeek = MobileAppSchedule.Model.ScheduleModel.DayOfWeek;
using MobileAppSchedule.Model.University;
using MvvmHelpers;
using MvvmHelpers.Commands;
using BaseViewModel = MobileAppSchedule.ViewModel.Base.BaseViewModel;

namespace MobileAppSchedule.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        #region Properties

        #region EmptyView
        public string EmptyView  => "Список пуст! Выбери группу";
        #endregion

        public ObservableRangeCollection<Discipline> Disciplines { get; set; }

        public ObservableRangeCollection<DayOfWeek> Weekday { get; set; }

        #region GroupName
        private string _groupName;
        public string GroupName
        {
            get => _groupName;
            set => Set<string>(ref _groupName, value);
        }
        #endregion

        #region CurrentDay
        public Discipline CurrentDay { get; set; }
        #endregion

        #region Schedule
        private Schedule _schedule;
        public Schedule Schedule
        {
            get => _schedule;
            set
            {
                Set(ref _schedule, value);

                GroupName = _schedule.GroupName;

                Weekday.Clear();
                Weekday.AddRange(_schedule.Weekday);

                //на понедельник (переопределить индексатор)
                Disciplines.Clear();
                Disciplines.AddRange(_schedule.Weekday[0].Disciplines);
                
            }
        }
        #endregion


        #endregion


        #region Commands

        public AsyncCommand RefreshCommand { get; }

        #endregion

        public MainViewModel()
        {
            #region Fields

            Title = "Расписание";
            Disciplines = new ObservableRangeCollection<Discipline>();
            Weekday = new ObservableRangeCollection<DayOfWeek>();
            _schedule = new Schedule();

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

        public void OnAppearing()
        {
            //загружаем список из памяти и распаршиваем в объекты 
            object name = "";
            App.Current.Properties.TryGetValue("group_name", out name);
            if (name == null || name.ToString() != GroupName)
            {
                var path = FileSystem.CacheDirectory;
                var fullpath = Path.Combine(path, "mobileschedule_schedule.txt");

                if (File.Exists(fullpath))
                {
                    var source = File.ReadAllText(fullpath);
                    var domParser = new HtmlParser();
                    var document = Task.Run(() => domParser.ParseDocumentAsync(source).GetAwaiter().GetResult()).Result;
                    UniversityParser parser = new UniversityParser(new Schedule());

                    Schedule = parser.ParseSchedule(document);
                    Title = "Расписание " + Schedule.GroupName;
                }
            }
        }
    }
}
