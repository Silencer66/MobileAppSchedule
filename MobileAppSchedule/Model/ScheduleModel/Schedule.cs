using System.Collections.Generic;

namespace MobileAppSchedule.Model.ScheduleModel
{
    public class Schedule
    {
        /// <summary> Имя группы </summary>
        public string GroupName { get; set; }

        /// <summary> Список дней недели </summary>
        public List<DayOfWeek> Weekday { get; set; }
        public Schedule()
        {
            Weekday = new List<DayOfWeek>();
        }

        public override bool Equals(object obj)
        {
            Schedule other = obj as Schedule;
            return (other != null && other.GroupName.Equals(GroupName));
        }
    }
}
