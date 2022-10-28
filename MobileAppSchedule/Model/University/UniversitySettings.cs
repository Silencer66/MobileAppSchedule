using System.Collections.Generic;
using MobileAppSchedule.Model.Parser;

namespace MobileAppSchedule.Model.University
{
    internal class UniversitySettings : IParserSettings
    {
        public string BaseUrl { get; set; } = "https://www.madi.ru/tplan/tasks/tableFiller.php";
        public List<string> GroupNames { get; set; }
        public string ContentType { get; set; } = "application/x-www-form-urlencoded";

        public UniversitySettings(List<string> groupNames)
        {
            GroupNames = groupNames;
        }
    }
}
