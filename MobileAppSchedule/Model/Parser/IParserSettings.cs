using System.Collections.Generic;

namespace MobileAppSchedule.Model.Parser
{
    interface IParserSettings
    {
        string BaseUrl { get; set; }

        List<string> GroupNames { get; set; }

        string ContentType { get; set; }
    }
}
