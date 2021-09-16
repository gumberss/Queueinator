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
    public class PublishToExchangeCommand : IRequest<Result<bool, BusinessException>>
    {
        public Server Server { get; set; }
        public string VHost { get; set; }
        public HostExchange Exchange { get; set; }

        public IEnumerable<QueueMessage> Messages { get; set; }
    }

    public class PublishToExchangeHandler : IRequestHandler<PublishToExchangeCommand, Result<bool, BusinessException>>
    {
        public async Task<Result<bool, BusinessException>> Handle(PublishToExchangeCommand request, CancellationToken cancellationToken)
        {
            var server = request.Server;
            var exchange = request.Exchange;

            var result = await Result.Try(() =>
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
                    channel.ExchangeDeclare(exchange.Name, exchange.Type, exchange.Durable, exchange.AutoDelete);

                    var batch = channel.CreateBasicPublishBatch();

                    foreach (var message in request.Messages)
                    {
                        var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(message.Payload));

                        var properties = channel.CreateBasicProperties();
                        properties.MessageId = Guid.NewGuid().ToString();
                        properties.DeliveryMode = message.Properties.DeliveryMode;
                        //headers??
                        
                        batch.Add(exchange.Name, message.RoutingKey, false, properties, body);
                    }

                    batch.Publish();
                }
            });

            if (result.IsFailure) return result.Error;

            return true;
        }
    }
}
