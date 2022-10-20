using MobileAppSchedule.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileAppSchedule.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            BindingContext = new SettingsViewModel();
        }
    }
}