using MediatR;
using Queueinator.Domain.RabbitMq;
using Queueinator.Domain.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Queueinator.Application.Features.LoadQueues
{
    public class LoadQueuesCommand : IRequest<Result<IEnumerable<HostQueue>, BusinessException>>
    {
        public string Server { get; set; }
        public string Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string VHost { get; set; }
    }

    public class LoadQueuesHandler : IRequestHandler<LoadQueuesCommand, Result<IEnumerable<HostQueue>, BusinessException>>
    {
        public async Task<Result<IEnumerable<HostQueue>, BusinessException>> Handle(LoadQueuesCommand request, CancellationToken cancellationToken)
        {
            //https://www.rabbitmq.com/uri-spec.html
            //https://stackoverflow.com/questions/43332913/can-i-iterate-through-queues-using-the-rabbitmq-net-client
            //https://stackoverflow.com/questions/33119611/how-to-make-rabbitmq-api-calls-with-vhost
            //https://rawcdn.githack.com/rabbitmq/rabbitmq-management/v3.8.0/priv/www/api/index.html

            var host = request.VHost == "/" ? "%2f" : request.VHost;

            var url = $"http://{request.Server}:{request.Port}/api/queues/{host}";

            using (var httpClient = new HttpClient())
            {
                using (var htttpRequest = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{request.User}:{request.Password}"));
                    htttpRequest.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

                    var response = await httpClient.SendAsync(htttpRequest);

                    var data = await response.Content.ReadAsStringAsync();

                    var virtualHosts = JsonSerializer.Deserialize<HostQueue[]>(data);

                    return virtualHosts;
                }
            }
        }
    }
}
