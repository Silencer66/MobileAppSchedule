using System.Threading.Tasks;
using Xamarin.Forms;

namespace MobileAppSchedule.Services
{
    public class PageService : IPageService
    {
        private Page _mainPage
        {
            get
            {
                return Application.Current.MainPage as Page;
            }
        }

        public async Task PushAsync(Page page)
        {
            await _mainPage.Navigation.PushAsync(page);
        }

        public async Task PushModalAsync(Page page)
        {
            await _mainPage.Navigation.PushModalAsync(page);
        }

        public async Task<Page> PopAsync()
        {
            return await _mainPage.Navigation.PopAsync();
        }

        public async Task<bool> DisplayAlert(string title, string message, string ok, string cancle)
        {
            return await _mainPage.DisplayAlert(title, message, ok, cancle);
        }

        public async Task DisplayAlert(string title, string message, string ok)
        {
            await _mainPage.DisplayAlert(title, message, ok);
        }
    }
}
