using MediatR;
using Queueinator.Application.Infra.Requests;
using Queueinator.Domain.RabbitMq;
using Queueinator.Domain.Utils;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Queueinator.Application.Features.LoadMessages
{
    //https://www.rabbitmq.com/uri-spec.html
    //https://stackoverflow.com/questions/43332913/can-i-iterate-through-queues-using-the-rabbitmq-net-client
    //https://stackoverflow.com/questions/33119611/how-to-make-rabbitmq-api-calls-with-vhost
    //https://rawcdn.githack.com/rabbitmq/rabbitmq-management/v3.8.0/priv/www/api/index.html

    public class LoadMessagesQuery : IRequest<Result<QueueMessage[], BusinessException>>
    {
        public Server Server { get; set; }

        public string HostName { get; set; }
        public string QueueName { get; set; }

        public int CountMessages { get; set; }
    }
    public class LoadMessagesHandler : IRequestHandler<LoadMessagesQuery, Result<QueueMessage[], BusinessException>>
    {
        public async Task<Result<QueueMessage[], BusinessException>> Handle(LoadMessagesQuery request, CancellationToken cancellationToken)
        {
            var messages = await Result.Try(async () => {

                var host = request.HostName == "/" ? "%2f" : request.HostName;

                var body = @"{""count"":10000,""ackmode"":""ack_requeue_true"",""encoding"":""auto"",""truncate"":500000}";

                var data = await RabbitRequestMessageCreator.Send("POST", $"api/queues/{host}/{request.QueueName}/get", request.Server, body);

                var messages = JsonSerializer.Deserialize<QueueMessage[]>(data);

                return messages;
            });

            return messages;
        }
    }
}