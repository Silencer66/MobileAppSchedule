using System.Threading.Tasks;
using System.Windows.Input;
using MobileAppSchedule.Model.ScheduleModel;
using MobileAppSchedule.Services;
using MobileAppSchedule.View;
using MobileAppSchedule.ViewModel.Base;
using Xamarin.Forms;

namespace MobileAppSchedule.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        private readonly Model.Model _model;
        private IPageService _pageService;
        private Schedule _schedule;
        #region Свойства

        #region GroupName
        private string _groupName;
        public string GroupName
        {
            get => _groupName;
            set => Set<string>(ref _groupName, value);
        }

        #endregion


        #endregion


        #region Команды

        #region SettingsCommand
        public ICommand SettingsCommand { get; set; }
        private async Task ToSettingsPage()
        {
            await _pageService.PushAsync(new SettingsPage()
            {
                BindingContext = new SettingsViewModel(_model)
            });
        }
        #endregion


        #endregion

        public MainViewModel(Model.Model model)
        {
            _model = model;
            _schedule = model.Schedule;
            GroupName = _schedule.GroupName;
            _pageService = model.PageService;

            #region Команды
            SettingsCommand = new Command(async () => await ToSettingsPage());
            #endregion
        }
    }
}
