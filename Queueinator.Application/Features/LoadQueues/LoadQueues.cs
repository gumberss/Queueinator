using MediatR;
using Queueinator.Application.Infra.Requests;
using Queueinator.Domain.RabbitMq;
using Queueinator.Domain.Utils;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Queueinator.Application.Features.LoadQueues
{
    public class LoadQueuesQuery : IRequest<Result<HostQueue[], BusinessException>>
    {
        public Server Server { get; set; }
        public string VHost { get; set; }
    }

    public class LoadQueuesHandler : IRequestHandler<LoadQueuesQuery, Result<HostQueue[], BusinessException>>
    {
        public async Task<Result<HostQueue[], BusinessException>> Handle(LoadQueuesQuery request, CancellationToken cancellationToken)
        {
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
