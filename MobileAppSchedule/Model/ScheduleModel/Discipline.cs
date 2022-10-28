namespace MobileAppSchedule.Model.ScheduleModel
{
    public class Discipline
    {
        /// <summary> Период </summary>
        public string Period { get; set; }

        /// <summary> Наименование дисциплины </summary>
        public string NameOfDiscipline { get; set; }

        /// <summary> Вид занятия(лекция/семинар) </summary>
        public string TypeDescipline { get; set; }

        /// <summary> Периодичность занятий </summary>
        public string Frequency { get; set; }

        /// <summary> Аудитория </summary>
        public string AudienceNumber { get; set; }

        /// <summary> Преподаватель </summary>
        public string NameOfTeacher { get; set; }
    }
}
