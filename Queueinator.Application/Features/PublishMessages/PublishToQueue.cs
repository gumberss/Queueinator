using MediatR;
using Queueinator.Application.Infra.Requests;
using Queueinator.Domain.RabbitMq;
using Queueinator.Domain.Utils;
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

        public QueueMessage Message { get; set; }
    }

    public class PublishToQueueHandler : IRequestHandler<PublishToQueueCommand, Result<bool, BusinessException>>
    {
        public async Task<Result<bool, BusinessException>> Handle(PublishToQueueCommand request, CancellationToken cancellationToken)
        {
            var host = request.VHost == "/" ? "%2f" : request.VHost;

            //await Result.Try(async () =>
            //{
            //    var body = JsonSerializer.Serialize(request.Message.Clone());

            //    return await RabbitRequestMessageCreator.Send("POST", $"api/exchanges/{host}/{request.Queue}/publish", request.Server, body);
            //});

            return true;
        }
    }
}
