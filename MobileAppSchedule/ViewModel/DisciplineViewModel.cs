using MobileAppSchedule.Model.ScheduleModel;
using MobileAppSchedule.ViewModel.Base;

namespace MobileAppSchedule.ViewModel
{
    internal class DisciplineViewModel : BaseViewModel
    {
        private readonly Discipline _discipline;

        /// <summary> Период </summary>
        public string Period => _discipline.Period;

        /// <summary> Наименование дисциплины </summary>
        public string NameOfDiscipline =>_discipline.NameOfDiscipline;

        /// <summary> Вид занятия(лекция/семинар) </summary>
        public string TypeDescipline =>_discipline.TypeDescipline;

        /// <summary> Периодичность занятий </summary>
        public string Frequency =>_discipline.Frequency;

        /// <summary> Аудитория </summary>
        public string AudienceNumber =>_discipline.AudienceNumber;

        /// <summary> Преподаватель </summary>
        public string NameOfTeacher => _discipline.NameOfTeacher;

        public DisciplineViewModel(Discipline discipline)
        {
            _discipline = discipline;
        }
    }
}
