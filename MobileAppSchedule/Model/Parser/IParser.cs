using System.Collections.Generic;
using AngleSharp.Html.Dom;

namespace MobileAppSchedule.Model.Parser
{
    interface IParser<T> where T : class
    {
        T ParseSchedule(IHtmlDocument document);
        List<string> ParseGroupNames(IHtmlDocument document);
    }
}
