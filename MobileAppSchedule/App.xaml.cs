using MobileAppSchedule.View;
using Xamarin.Forms;

namespace MobileAppSchedule
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new SchedulePage();
        }

        protected override void OnStart()
        {

        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
