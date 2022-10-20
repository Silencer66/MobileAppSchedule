using MobileAppSchedule.Services;
using MobileAppSchedule.ViewModel;
using Xamarin.Forms;

namespace MobileAppSchedule.View
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new MainViewModel(new PageService());
        }
    }
}
