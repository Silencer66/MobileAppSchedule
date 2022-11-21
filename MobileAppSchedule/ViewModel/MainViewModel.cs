using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using AngleSharp.Html.Parser;
using MobileAppSchedule.Model.ScheduleModel;
using Xamarin.Essentials;
using DayOfWeek = MobileAppSchedule.Model.ScheduleModel.DayOfWeek;
using MobileAppSchedule.Model.University;
using MvvmHelpers.Commands;
using BaseViewModel = MobileAppSchedule.ViewModel.Base.BaseViewModel;
using XCalendar.Core.Models;
using XCalendar.Core.Enums;
using System;

namespace MobileAppSchedule.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        #region Properties

        public Calendar<CalendarDay> FirstPageCalendar { get; set; } = new Calendar<CalendarDay>() { SelectionAction = SelectionAction.Modify, SelectionType = SelectionType.Single };
        public Calendar<CalendarDay> SecondPageCalendar { get; set; } = new Calendar<CalendarDay>() { SelectionAction = SelectionAction.Modify, SelectionType = SelectionType.Single };
        public Calendar<CalendarDay> ThirdPageCalendar { get; set; } = new Calendar<CalendarDay>() { SelectionAction = SelectionAction.Modify, SelectionType = SelectionType.Single };
        public int CurrentPageCalendarPosition { get; set; } = 1;
        public Calendar<CalendarDay> CurrentPageCalendar
        {
            get
            {
                return Calendars[CurrentPageCalendarPosition];
            }
        }
        public MvvmHelpers.ObservableRangeCollection<Calendar<CalendarDay>> Calendars { get; set; } = new MvvmHelpers.ObservableRangeCollection<Calendar<CalendarDay>>();

        public MvvmHelpers.ObservableRangeCollection<Discipline> Disciplines { get; set; }

        public MvvmHelpers.ObservableRangeCollection<DayOfWeek> Weekday { get; set; }

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
        public ICommand ChangeDateSelectionCommand { get; set; }
        public ICommand CurrentPageCalendarChangedCommand { get; set; }

        #region RefreshCommand
        public AsyncCommand RefreshCommand { get; }
        #endregion

        #endregion

        public MainViewModel()
        {
            
            ChangeDateSelectionCommand = new Command<DateTime>(ChangeDateSelection);
            CurrentPageCalendarChangedCommand = new Command(CurrentPageCalendarChanged);

            SecondPageCalendar.SelectedDates = FirstPageCalendar.SelectedDates;
            SecondPageCalendar.DayNamesOrder = FirstPageCalendar.DayNamesOrder;
            SecondPageCalendar.CustomDayNamesOrder = FirstPageCalendar.CustomDayNamesOrder;

            ThirdPageCalendar.SelectedDates = FirstPageCalendar.SelectedDates;
            ThirdPageCalendar.DayNamesOrder = FirstPageCalendar.DayNamesOrder;
            ThirdPageCalendar.CustomDayNamesOrder = FirstPageCalendar.CustomDayNamesOrder;

            Calendars.Add(FirstPageCalendar);
            Calendars.Add(SecondPageCalendar);
            Calendars.Add(ThirdPageCalendar);

            UpdateCalendarPages();



            #region Fields
            Title = "Расписание";
            _schedule = new Schedule();
            Weekday = new MvvmHelpers.ObservableRangeCollection<DayOfWeek>();
            Disciplines = new MvvmHelpers.ObservableRangeCollection<Discipline>();
            #endregion

            #region Commands



            #region RefreshCommand
            RefreshCommand = new AsyncCommand(Refresh);
            #endregion

            #endregion
        }

        #region Methods

        async Task Refresh()
        {
            IsBusy = true;

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

                    ///
                    var document = await domParser.ParseDocumentAsync(source);
                    UniversityParser parser = new UniversityParser(new Schedule());

                    Schedule = parser.ParseSchedule(document);
                    Title = "Расписание " + Schedule.GroupName;
                }
            }

            IsBusy = false;
        }

        public async void OnAppearing()
        {
            await RefreshCommand.ExecuteAsync();
        }



        public void ChangeDateSelection(DateTime DateTime)
        {
            CurrentPageCalendar.ChangeDateSelection(DateTime);
        }
        public void CurrentPageCalendarChanged()
        {
            UpdateCalendarPages();
        }
        public void UpdateCalendarPages()
        {
            DateTime CurrentPageCalendarPreviousNavigatedDate = CurrentPageCalendar.NavigateDateTime(CurrentPageCalendar.NavigatedDate, CurrentPageCalendar.NavigationLowerBound, CurrentPageCalendar.NavigationUpperBound, CurrentPageCalendar.BackwardsNavigationAmount, CurrentPageCalendar.NavigationLoopMode, CurrentPageCalendar.NavigationTimeUnit, CurrentPageCalendar.StartOfWeek);
            DateTime CurrentPageCalendarNextNavigatedDate = CurrentPageCalendar.NavigateDateTime(CurrentPageCalendar.NavigatedDate, CurrentPageCalendar.NavigationLowerBound, CurrentPageCalendar.NavigationUpperBound, CurrentPageCalendar.ForwardsNavigationAmount, CurrentPageCalendar.NavigationLoopMode, CurrentPageCalendar.NavigationTimeUnit, CurrentPageCalendar.StartOfWeek);

            if (CurrentPageCalendar == FirstPageCalendar)
            {
                SecondPageCalendar.NavigatedDate = CurrentPageCalendarNextNavigatedDate;
                ThirdPageCalendar.NavigatedDate = CurrentPageCalendarPreviousNavigatedDate;
            }
            else if (CurrentPageCalendar == SecondPageCalendar)
            {
                FirstPageCalendar.NavigatedDate = CurrentPageCalendarPreviousNavigatedDate;
                ThirdPageCalendar.NavigatedDate = CurrentPageCalendarNextNavigatedDate;
            }
            else if (CurrentPageCalendar == ThirdPageCalendar)
            {
                FirstPageCalendar.NavigatedDate = CurrentPageCalendarNextNavigatedDate;
                SecondPageCalendar.NavigatedDate = CurrentPageCalendarPreviousNavigatedDate;
            }
        }
        #endregion

    }
}
