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
using System.Linq;
using MobileAppSchedule.Model;
using Command = MvvmHelpers.Commands.Command;

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

        #region EmptyView

        private string _emptyView;
        public string EmptyView
        {
            get => _emptyView;
            set 
            {
                Set(ref _emptyView, value);
            }
        }

        #endregion

        #region Disciplines
        public MvvmHelpers.ObservableRangeCollection<Discipline> Disciplines { get; set; }

        #endregion

        #region Weekday
        public MyObservableCollection<DayOfWeek> Weekday { get; set; }

        #endregion

        #region GroupName
        private string _groupName;
        public string GroupName
        {
            get => _groupName;
            set => Set<string>(ref _groupName, value);
        }
        #endregion

        #region CurrentDay
        public DayOfWeek CurrentDay { get; set; }
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
                var currentDayOfWeek = CurrentPageCalendar.TodayDate.DayOfWeek;
                ChangeCurrentDayOfWeek(currentDayOfWeek);
                if (CurrentDay == null)
                {
                    EmptyView = "На этот день занятий нет!";
                    return;
                }
                Disciplines.AddRange(Weekday[CurrentDay].Disciplines);
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
            #region Commands
            ChangeDateSelectionCommand = new MvvmHelpers.Commands.Command<DateTime>(ChangeDateSelection);
            CurrentPageCalendarChangedCommand = new Command(CurrentPageCalendarChanged);
            #endregion

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
            Weekday = new MyObservableCollection<DayOfWeek>();
            Disciplines = new MvvmHelpers.ObservableRangeCollection<Discipline>();


            CurrentPageCalendar.AutoRows = false;
            CurrentPageCalendar.Rows = 5;
            CurrentPageCalendar.SelectionType = SelectionType.Single;
            
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
            if (IsBusy == false)
            {
                IsBusy = true;
                //загружаем список из памяти и распаршиваем в объекты 
                object name = "";
                App.Current.Properties.TryGetValue("group_name", out name);
                if (name == null)
                {
                    EmptyView = "Расписание пусто, пожалуйста выберите группу";
                    IsBusy = false;
                    return;
                }

                if (name.ToString() != GroupName)
                {
                    var path = FileSystem.CacheDirectory;
                    var fullpath = Path.Combine(path, "mobileschedule_schedule.txt");

                    if (File.Exists(fullpath))
                    {
                        var source = File.ReadAllText(fullpath);
                        var domParser = new HtmlParser();

                        var document = await domParser.ParseDocumentAsync(source);
                        UniversityParser parser = new UniversityParser(new Schedule());

                        Schedule = parser.ParseSchedule(document);
                        Title = "Расписание " + Schedule.GroupName;
                    }
                    
                }
            }
            IsBusy = false;
        }

        public Task OnAppearing()
        {
            return Refresh();
        }

        /// <summary>Возникате при изменении даты в календаре</summary>
        /// <param name="DateTime"></param>
        public void ChangeDateSelection(DateTime DateTime)
        {
            CurrentPageCalendar.SelectedDates.Clear();
            CurrentPageCalendar.ChangeDateSelection(DateTime);

            //реализуем смену расписания
            var currnetDayOfWeek = CurrentPageCalendar.SelectedDates[0].DayOfWeek;
            
            ChangeCurrentDayOfWeek(currnetDayOfWeek);
            Disciplines.Clear();
            if (CurrentDay == null)
            {
                EmptyView = "На сегодня не занятий!";
                return;
            }
            Disciplines.AddRange(Weekday[CurrentDay].Disciplines);
        }

        private void ChangeCurrentDayOfWeek(System.DayOfWeek currentDayOfWeek)
        {
            switch (currentDayOfWeek)
            {
                case System.DayOfWeek.Monday:
                    CurrentDay = Weekday.Where(a => a.NameOfDay == "Понедельник").FirstOrDefault();
                    break;
                case System.DayOfWeek.Tuesday:
                    CurrentDay = Weekday.Where(a => a.NameOfDay == "Вторник").FirstOrDefault();
                    break;
                case System.DayOfWeek.Wednesday:
                    CurrentDay = Weekday.Where(a => a.NameOfDay == "Среда").FirstOrDefault();
                    break;
                case System.DayOfWeek.Thursday:
                    CurrentDay = Weekday.Where(a => a.NameOfDay == "Четверг").FirstOrDefault();
                    break;
                case System.DayOfWeek.Friday:
                    CurrentDay = Weekday.Where(a => a.NameOfDay == "Пятница").FirstOrDefault();
                    break;
                case System.DayOfWeek.Saturday:
                    CurrentDay = Weekday.Where(a => a.NameOfDay == "Суббота").FirstOrDefault();
                    break;
                case System.DayOfWeek.Sunday:
                    CurrentDay = Weekday.Where(a => a.NameOfDay == "Воскресенье").FirstOrDefault();
                    break;
            }
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
