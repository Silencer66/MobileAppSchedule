using System.Collections.Generic;

namespace MobileAppSchedule.Model.ScheduleModel
{
    public class Schedule
    {
        /// <summary> Имя группы </summary>
        public string GroupName { get; set; }

        /// <summary> Список дней недели </summary>
        public List<DayOfWeek> DaysOfWeek { get; set; }
        public Schedule()
        {
            DaysOfWeek = new List<DayOfWeek>();
        }
    }
}
