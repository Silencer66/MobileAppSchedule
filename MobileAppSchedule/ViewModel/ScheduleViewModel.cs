using System.Threading.Tasks;
using System.Windows.Input;
using MobileAppSchedule.View;
using Xamarin.Forms;

namespace MobileAppSchedule.ViewModel
{
    internal class ScheduleViewModel
    {
        public INavigation Navigation;
        public string GroupName { get; set; } = "2бАСУ-3";

        public ICommand SettingsCommand { get; set; }

        public ScheduleViewModel()
        {
            SettingsCommand = new Command(async()=> await ToSettingsPage());
        }

        private async Task ToSettingsPage()
        {
            await Navigation.PushAsync(new MainPage());
        }
    }
}
