using MvvmHelpers;
using System.Linq;

namespace MobileAppSchedule.Model
{
    internal class MyObservableCollection<DayOfWeek> : ObservableRangeCollection<DayOfWeek>
    {   
        public DayOfWeek this[DayOfWeek day]
        {
            get
            {
                return this.Where(a => a.Equals(day)).FirstOrDefault();
            }
            set
            {
                this[day] = value;
            }
        }
    }
}
