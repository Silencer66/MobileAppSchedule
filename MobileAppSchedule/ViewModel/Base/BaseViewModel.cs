﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace MobileAppSchedule.ViewModel.Base
{
    internal abstract class BaseViewModel:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) 
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
