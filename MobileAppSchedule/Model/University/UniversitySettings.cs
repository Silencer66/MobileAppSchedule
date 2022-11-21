using System.Collections.Generic;
using MobileAppSchedule.Model.Parser;

namespace MobileAppSchedule.Model.University
{
    internal class UniversitySettings : IParserSettings
    {
        public string BaseUrl { get; set; }
        public List<string> GroupNames { get; set; }
        public string ContentType { get; set; } = "application/x-www-form-urlencoded";

        public UniversitySettings(List<string> groupNames, string url)
        {
            GroupNames = groupNames;
            BaseUrl = url;
        }
    }
}
