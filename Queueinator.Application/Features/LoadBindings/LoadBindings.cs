using MediatR;
using Queueinator.Application.Infra.Requests;
using Queueinator.Domain.RabbitMq;
using Queueinator.Domain.Utils;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Queueinator.Application.Features.LoadBindings
{
    public class LoadBindingsQuery : IRequest<Result<ExchangeBindingsGrouper, BusinessException>>
    {
        public Server Server { get; set; }

        public HostExchange Exchange { get; set; }
    }

    public class LoadBindingsHandler : IRequestHandler<LoadBindingsQuery, Result<ExchangeBindingsGrouper, BusinessException>>
    {
        public async Task<Result<ExchangeBindingsGrouper, BusinessException>> Handle(LoadBindingsQuery request, CancellationToken cancellationToken)
        {
            var exchange = request.Exchange;

            var data = await Result.Try(() =>
            {
                var host = exchange.Host == "/" ? "%2f" : exchange.Host;

                var dataSourceTask = RabbitRequestMessageCreator.Send("GET", $"api/exchanges/{host}/{exchange.Name}/bindings/source", request.Server);

                var dataDestinationTask = RabbitRequestMessageCreator.Send("GET", $"api/exchanges/{host}/{exchange.Name}/bindings/destination", request.Server);

                Task.WaitAll(dataSourceTask, dataDestinationTask);

                var dataSource = JsonSerializer.Deserialize<ExchangeBinding[]>(dataSourceTask.Result);

                var dataDestination = JsonSerializer.Deserialize<ExchangeBinding[]>(dataDestinationTask.Result);

                return new ExchangeBindingsGrouper(dataSource.ToList(), dataDestination.ToList());
            });

            return data;
        }
    }
}

