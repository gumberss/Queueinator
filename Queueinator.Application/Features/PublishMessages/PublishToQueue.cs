using MediatR;
using Queueinator.Application.Infra.Requests;
using Queueinator.Domain.RabbitMq;
using Queueinator.Domain.Utils;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Queueinator.Application.Features.PublishMessages
{
    public class PublishToQueueCommand : IRequest<Result<bool, BusinessException>>
    {
        public Server Server { get; set; }
        public string VHost { get; set; }
        public string Queue { get; set; }

        public IEnumerable<QueueMessage> Messages { get; set; }
    }

    public class PublishToQueueHandler : IRequestHandler<PublishToQueueCommand, Result<bool, BusinessException>>
    {
        public async Task<Result<bool, BusinessException>> Handle(PublishToQueueCommand request, CancellationToken cancellationToken)
        {
            //var host = request.VHost == "/" ? "%2f" : request.VHost;

            var server = request.Server;

            await Result.Try(() =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = server.Name,
                    Password = server.Password,
                    UserName = server.User,
                    VirtualHost = request.VHost
                };

                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(request.Queue, true, false, false, null);

                    var batch = channel.CreateBasicPublishBatch();
                    
                    foreach (var message in request.Messages)
                    {
                        var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(message.Payload));

                        var properties = channel.CreateBasicProperties();
                        properties.MessageId = Guid.NewGuid().ToString();
                        batch.Add("", request.Queue, false, properties, body);
                    }
                    
                    batch.Publish();
                }
            });

            return true;
        }

    }
}
