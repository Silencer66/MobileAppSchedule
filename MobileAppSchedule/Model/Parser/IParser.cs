using AngleSharp.Html.Dom;

namespace MobileAppSchedule.Model.Parser
{
    interface IParser<T> where T : class
    {
        T Parse(IHtmlDocument document);
    }
}
