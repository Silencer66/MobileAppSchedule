using System.Threading.Tasks;
using System.Windows.Input;
using MobileAppSchedule.Services;
using MobileAppSchedule.View;
using MobileAppSchedule.ViewModel.Base;
using Xamarin.Forms;

namespace MobileAppSchedule.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        public string GroupName { get; set; } = "2бАСУ-3";
        public ICommand SettingsCommand { get; set; }
        private IPageService _pageService;

        public MainViewModel(IPageService pageService)
        {
            _pageService = pageService;
            SettingsCommand = new Command(async () => await ToSettingsPage());
        }

        private async Task ToSettingsPage()
        {
            await _pageService.PushAsync(new SettingsPage());
        }
    }
}
