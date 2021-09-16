using MediatR;
using Queueinator.Application.Infra.Requests;
using Queueinator.Domain.RabbitMq;
using Queueinator.Domain.Utils;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Queueinator.Application.Features.LoadExchanges
{
    public class LoadExchangesQuery : IRequest<Result<HostExchange[], BusinessException>>
    {
        public Server Server { get; set; }
        public string VHost { get; set; }
    }

    public class LoadExchangesHandler : IRequestHandler<LoadExchangesQuery, Result<HostExchange[], BusinessException>>
    {
        public async Task<Result<HostExchange[], BusinessException>> Handle(LoadExchangesQuery request, CancellationToken cancellationToken)
        {
            var data = await Result.Try(async () =>
            {
                var host = request.VHost == "/" ? "%2f" : request.VHost;

                var data = await RabbitRequestMessageCreator.Send("GET", $"api/exchanges/{host}", request.Server);

                var virtualHosts = JsonSerializer.Deserialize<HostExchange[]>(data);

                return virtualHosts;
            });

            return data;
        }
    }
}
