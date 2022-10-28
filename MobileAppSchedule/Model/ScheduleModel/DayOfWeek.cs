using System.Collections.Generic;

namespace MobileAppSchedule.Model.ScheduleModel
{
    public class DayOfWeek
    {
        public string NameOfDay { get; set; }
        /// <summary> Список занятий </summary>
        public List<Discipline> Disciplines = new List<Discipline>();
    }
}
