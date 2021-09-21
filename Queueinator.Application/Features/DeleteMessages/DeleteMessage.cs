using MediatR;
using Queueinator.Domain.RabbitMq;
using Queueinator.Domain.Utils;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Queueinator.Application.Features.DeleteMessages
{
    public class DeleteMessagesCommand : IRequest<Result<bool, BusinessException>>
    {
        public Server Server { get; set; }
        public string VHost { get; set; }
        public string Queue { get; set; }

        public IEnumerable<QueueMessage> Messages { get; set; }
    }

    public class LoadExchangesHandler : IRequestHandler<DeleteMessagesCommand, Result<bool, BusinessException>>
    {
        public async Task<Result<bool, BusinessException>> Handle(DeleteMessagesCommand request, CancellationToken cancellationToken)
        {
            var server = request.Server;

            return await Result.Try(async () =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = server.Name,
                    Password = server.Password,
                    UserName = server.User,
                    VirtualHost = request.VHost
                };

                var messagesToDeleteIds = request.Messages.Select(x => x.Properties.Id).ToList();

                List<string> messagesChecked = new List<string>();
                List<Guid> deletedMessages = new List<Guid>(request.Messages.Count());
                bool inLoop = false;

                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var consumer = new EventingBasicConsumer(channel);
                    var consumerTag = channel.BasicConsume(request.Queue, false, consumer);

                    consumer.Received += (ch, ea) =>
                    {
                        if (messagesChecked.Contains(ea.BasicProperties.MessageId))
                        {
                            inLoop = true;
                            //loop
                            channel.BasicCancel(consumerTag);
                        }

                        messagesChecked.Add(ea.BasicProperties.MessageId);

                        if (!Guid.TryParse(ea.BasicProperties.MessageId, out Guid currentMessageId)) return;

                        if (!messagesToDeleteIds.Contains(currentMessageId)) return;

                        channel.BasicAck(ea.DeliveryTag, false);

                        deletedMessages.Add(currentMessageId);
                    };

                    var startProcessDate = DateTime.Now;
                    var timeout = TimeSpan.FromSeconds(30);

                    while (!inLoop && deletedMessages.Count != messagesToDeleteIds.Count && !Timeout(startProcessDate, timeout))
                    {
                        await Task.Delay(10);
                    }
                }

                if (inLoop) return false;

                return true;
            });
        }

        private static bool Timeout(DateTime startProcessDate, TimeSpan timeout)
        {
            return startProcessDate.Add(timeout) < DateTime.Now;
        }
    }
}