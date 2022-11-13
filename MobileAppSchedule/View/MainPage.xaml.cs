using MobileAppSchedule.ViewModel;
using Xamarin.Forms;

namespace MobileAppSchedule.View
{
    public partial class MainPage : ContentPage
    {

        private MainViewModel _mainViewModel;
        public MainPage()
        {
            InitializeComponent();
            BindingContext = _mainViewModel = new MainViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _mainViewModel.OnAppearing();
        }
    }
}
