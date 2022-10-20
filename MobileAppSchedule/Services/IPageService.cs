﻿using System.Threading.Tasks;
using Xamarin.Forms;

namespace MobileAppSchedule.Services
{
    internal interface IPageService
    {
        Task PushAsync(Page page);
        Task PushModalAsync(Page page);
        Task<Page> PopAsync();
        Task<bool> DisplayAlert(string title, string message, string ok, string cancle);
        Task DisplayAlert(string title, string message, string ok);
    }
}