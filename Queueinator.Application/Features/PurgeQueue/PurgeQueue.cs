using MediatR;
using Queueinator.Application.Infra.Requests;
using Queueinator.Domain.RabbitMq;
using Queueinator.Domain.Utils;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Queueinator.Application.Features.PurgeQueue
{
    public class PurgeQueueCommand : IRequest<Result<bool, BusinessException>>
    {
        public Server Server { get; set; }

        public string VHost { get; set; }
        public string QueueName { get; set; }
    }

    public class LoadQueuesHandler : IRequestHandler<PurgeQueueCommand, Result<bool, BusinessException>>
    {
        ///api/queues/vhost/name/contents
        public async Task<Result<bool, BusinessException>> Handle(PurgeQueueCommand request, CancellationToken cancellationToken)
        {
            //https://www.rabbitmq.com/uri-spec.html
            //https://stackoverflow.com/questions/43332913/can-i-iterate-through-queues-using-the-rabbitmq-net-client
            //https://stackoverflow.com/questions/33119611/how-to-make-rabbitmq-api-calls-with-vhost
            //https://rawcdn.githack.com/rabbitmq/rabbitmq-management/v3.8.0/priv/www/api/index.html

            var host = request.VHost == "/" ? "%2f" : request.VHost;

            await Result.Try(RabbitRequestMessageCreator.Send("DELETE", $"api/queues/{host}/{request.QueueName}/contents", request.Server));

            return true;
        }
    }
}