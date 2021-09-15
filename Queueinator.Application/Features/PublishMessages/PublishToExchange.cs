using MediatR;
using Queueinator.Application.Infra.Requests;
using Queueinator.Domain.RabbitMq;
using Queueinator.Domain.Utils;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Queueinator.Application.Features.PublishMessages
{
    public class PublishToExchangeCommand : IRequest<Result<bool, BusinessException>>
    {
        public Server Server { get; set; }
        public string VHost { get; set; }
        public string Exchange { get; set; }

        public QueueMessage Message { get; set; }
    }

    public class PublishToExchangeHandler : IRequestHandler<PublishToExchangeCommand, Result<bool, BusinessException>>
    {
        public async Task<Result<bool, BusinessException>> Handle(PublishToExchangeCommand request, CancellationToken cancellationToken)
        {
            var host = request.VHost == "/" ? "%2f" : request.VHost;

            await Result.Try(async () =>
            {
                var body = JsonSerializer.Serialize(request.Message.Clone());

                return  await RabbitRequestMessageCreator.Send("POST", $"api/exchanges/{host}/{request.Exchange}/publish", request.Server, body);
            });

            return true;
        }
    }
}
