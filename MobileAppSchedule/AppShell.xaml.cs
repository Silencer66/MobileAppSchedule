using MobileAppSchedule.View;
using Xamarin.Forms;

namespace MobileAppSchedule
{
    public partial class AppShell : Shell   
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(MainPage),typeof(MainPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(FavouritesGroupsPage), typeof(FavouritesGroupsPage));
        }
    }
}