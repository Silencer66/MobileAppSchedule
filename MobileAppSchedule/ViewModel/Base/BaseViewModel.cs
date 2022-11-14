using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace MobileAppSchedule.ViewModel.Base
{
    internal abstract class BaseViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #region Свойства

        #region Title

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                Set(ref _title, value);
            }
        }
        #endregion

        #region IsBusy
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                Set(ref _isBusy, value);
            }
        }
        #endregion

        #endregion
    }
}
