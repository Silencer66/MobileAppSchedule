using MobileAppSchedule.ViewModel.Base;

namespace MobileAppSchedule.Model.ScheduleModel
{
    internal class Group : BaseViewModel
    {
        public string GroupName { get; set; }

        private bool _isFavourite;
        public bool IsFavorite
        {
            get => _isFavourite;
            set
            {
                Set(ref _isFavourite, value);
            }
        }

        private string _favouriteImage;
        public string FavouriteImage
        {
            get => _favouriteImage;
            set
            {
                Set(ref _favouriteImage, value);
            }
        }


        public Group(string groupName, bool isFavorite, string favouriteImage)
        {
            GroupName = groupName;
            IsFavorite = isFavorite;
            FavouriteImage = favouriteImage;
        }
    }
}
