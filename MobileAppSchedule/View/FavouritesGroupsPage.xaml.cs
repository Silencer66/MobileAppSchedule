using MobileAppSchedule.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileAppSchedule.View
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class FavouritesGroupsPage : ContentPage
    {
        private FavouritesGroupsViewModel _favouritesGroupsViewModel;
		public FavouritesGroupsPage ()
		{
			InitializeComponent ();
			_favouritesGroupsViewModel= new FavouritesGroupsViewModel ();
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _favouritesGroupsViewModel.OnAppearing();
        }
    }
}