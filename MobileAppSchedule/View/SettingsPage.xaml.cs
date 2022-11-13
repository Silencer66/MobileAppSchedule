using MobileAppSchedule.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileAppSchedule.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        private SettingsViewModel _settingsViewModel;
        public SettingsPage()
        {
            InitializeComponent();
            BindingContext  = _settingsViewModel = new SettingsViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _settingsViewModel.OnAppearing();
        }
    }
}