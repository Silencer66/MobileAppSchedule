using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using MobileAppSchedule.Model.ScheduleModel;
using MobileAppSchedule.ViewModel.Base;

namespace MobileAppSchedule.ViewModel
{
    internal class SettingsViewModel : BaseViewModel
    {
        private readonly Model.Model _model;
        private Schedule _schedule;

        #region Свойства
        public ObservableCollection<string> GroupNames { get; set; }

        private string _selectedGroup;
        public string SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                Set(ref _selectedGroup, value);
                //Здесь нужно запарсить сайт

                object name = "";
                App.Current.Properties.TryGetValue("group_name", out name);
                if (name == null || name.ToString() != _selectedGroup)
                {
                    //парсим и меняем значение в словаре
                    App.Current.Properties["group_name"] = _selectedGroup;
                    App.Current.SavePropertiesAsync().GetAwaiter();
                }

            }
        }
        #endregion

        #region Команды

        #endregion
        public SettingsViewModel(Model.Model model)
        {
            _model = model;
            GroupNames = new ObservableCollection<string>();
            GetGroupNames(model.GroupNames);

            #region Команды
            #endregion
        }

        private void GetGroupNames(List<string> names)
        {
            foreach (var groupName in names)
            {
                //var group = Regex.Matches(groupName, @"[\d\s\w]*")[6];
                GroupNames.Add(Regex.Matches(groupName, @"[\d\s\w]*")[6].ToString());
            }
        }
    }
}
