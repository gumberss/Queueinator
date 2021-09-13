using Queueinator.Domain.RabbitMq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Queueinator.Application.Infra.Requests
{
    public static class RabbitRequestMessageCreator
    {
        //https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
        private static HttpClient _client = new HttpClient();

        public async static Task<String> Send(String method, String path, Server server, String body = "")
        {
            var url = $"http://{server.Name}:{server.Port}/{path}";

            var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{server.User}:{server.Password}"));

            using var httpMessage = new HttpRequestMessage(new HttpMethod(method), url);

            httpMessage.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");
            httpMessage.Content = new StringContent(body);

            var response = await _client.SendAsync(httpMessage);

            return await response.Content.ReadAsStringAsync();
        }
    }
}
