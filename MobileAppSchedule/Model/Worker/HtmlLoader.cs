using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MobileAppSchedule.Model.Parser;

namespace MobileAppSchedule.Model.Worker
{
    class HtmlLoader
    {
        readonly HttpClient client;
        readonly string url;
        readonly string contentType;

        public HtmlLoader(IParserSettings settings)
        {
            client = new HttpClient();
            url = settings.BaseUrl;
            contentType = settings.ContentType;
        }

        public async Task<string> GetScheduleByGroupName(string payload)
        {

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                HttpContent content = new StringContent(payload,
                    Encoding.UTF8,
                    contentType
                );
                request.Content = content;

                var response = await client.SendAsync(request);
                if (response == null || response.StatusCode != HttpStatusCode.OK)
                {
                    return "Ответ пустой";
                }
                var responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
        }
    }
}
