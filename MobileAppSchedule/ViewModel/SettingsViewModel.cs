using System.Collections.Generic;
using MobileAppSchedule.Model.ScheduleModel;
using MobileAppSchedule.ViewModel.Base;

namespace MobileAppSchedule.ViewModel
{
    internal class SettingsViewModel : BaseViewModel
    {
        private readonly Model.Model _model;
        private Schedule _schedule;

        #region Свойства
        public IList<string> GroupNames { get; set; }

        private int _groupIndex;
        public int GroupIndex
        {
            get => _groupIndex;
            set
            {
                Set(ref _groupIndex, value);
                //Здесь нужно запарсить сайт 
            }
        }
        #endregion

        #region Команды

        #endregion
        public SettingsViewModel(Model.Model model)
        {
            _model = model;
            GroupNames = model.GroupNames;

            #region Команды
            #endregion
        }
    }
}
