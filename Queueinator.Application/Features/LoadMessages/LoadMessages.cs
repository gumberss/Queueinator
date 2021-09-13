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

namespace Queueinator.Application.Features.LoadMessages
{
    public class LoadMessagesCommand : IRequest<Result<IEnumerable<QueueMessage>, BusinessException>>
    {
        public Server Server { get; set; }

        public string HostName { get; set; }
        public string QueueName { get; set; }

        public int CountMessages { get; set; }
    }
    public class LoadMessagesHandler : IRequestHandler<LoadMessagesCommand, Result<IEnumerable<QueueMessage>, BusinessException>>
    {
        public async Task<Result<IEnumerable<QueueMessage>, BusinessException>> Handle(LoadMessagesCommand request, CancellationToken cancellationToken)
        {
            //https://www.rabbitmq.com/uri-spec.html
            //https://stackoverflow.com/questions/43332913/can-i-iterate-through-queues-using-the-rabbitmq-net-client
            //https://stackoverflow.com/questions/33119611/how-to-make-rabbitmq-api-calls-with-vhost
            //https://rawcdn.githack.com/rabbitmq/rabbitmq-management/v3.8.0/priv/www/api/index.html

            var host = request.HostName == "/" ? "%2f" : request.HostName;

            var url = $"http://{request.Server.Name}:{request.Server.Port}/api/queues/{host}/{request.QueueName}/get";

            using (var httpClient = new HttpClient())
            {
                using (var htttpRequest = new HttpRequestMessage(new HttpMethod("POST"), url))
                {
                    var body = @"{""count"":10000,""ackmode"":""ack_requeue_true"",""encoding"":""auto"",""truncate"":500000}";

                    var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{request.Server.User}:{request.Server.Password}"));
                    htttpRequest.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");
                    htttpRequest.Content = new StringContent(body);

                    var response = await httpClient.SendAsync(htttpRequest);

                    var data = await response.Content.ReadAsStringAsync();
                    
                    var messages = JsonSerializer.Deserialize<QueueMessage[]>(data);
                    
                    return messages;
                }
            }
        }
    }
}
