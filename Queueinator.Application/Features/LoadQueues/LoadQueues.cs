using MediatR;
using Queueinator.Application.Infra.Requests;
using Queueinator.Domain.RabbitMq;
using Queueinator.Domain.Utils;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Queueinator.Application.Features.LoadQueues
{
    public class LoadQueuesCommand : IRequest<Result<HostQueue[], BusinessException>>
    {
        public Server Server { get; set; }
        public string VHost { get; set; }
    }

    public class LoadQueuesHandler : IRequestHandler<LoadQueuesCommand, Result<HostQueue[], BusinessException>>
    {
        public async Task<Result<HostQueue[], BusinessException>> Handle(LoadQueuesCommand request, CancellationToken cancellationToken)
        {
            //https://www.rabbitmq.com/uri-spec.html
            //https://stackoverflow.com/questions/43332913/can-i-iterate-through-queues-using-the-rabbitmq-net-client
            //https://stackoverflow.com/questions/33119611/how-to-make-rabbitmq-api-calls-with-vhost
            //https://rawcdn.githack.com/rabbitmq/rabbitmq-management/v3.8.0/priv/www/api/index.html

            var data = await Result.Try(async () =>
            {
                var host = request.VHost == "/" ? "%2f" : request.VHost;

                var data = await RabbitRequestMessageCreator.Send("GET", $"api/queues/{host}", request.Server);

                var virtualHosts = JsonSerializer.Deserialize<HostQueue[]>(data);

                return virtualHosts;
            });

            return data;
        }
    }
}
