using MobileAppSchedule.Model.ScheduleModel;
using MobileAppSchedule.Services;
using System.Collections.Generic;

namespace MobileAppSchedule.Model
{
    public class Model
    {
        public Schedule Schedule { get; set; }
        public List<string> GroupNames { get; set; }
        public IPageService PageService { get; set; }

        public Model(Schedule schedule, List<string> groupNames, IPageService pageService)
        {
            Schedule = schedule;
            GroupNames = groupNames;
            PageService = pageService;
        }
    }
}
