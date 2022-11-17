namespace MobileAppSchedule.Model.ScheduleModel
{
    internal class Group
    {
        public string GroupName { get; set; }
        public bool IsFavorite { get; set; }

        public Group(string groupName, bool isFavorite)
        {
            GroupName = groupName;
            IsFavorite = isFavorite;
        }
    }
}
