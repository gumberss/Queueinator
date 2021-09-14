using MediatR;
using Queueinator.Application.Infra.Requests;
using Queueinator.Domain.RabbitMq;
using Queueinator.Domain.Utils;
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
        public async Task<Result<bool, BusinessException>> Handle(PurgeQueueCommand request, CancellationToken cancellationToken)
        {
            var host = request.VHost == "/" ? "%2f" : request.VHost;

            await Result.Try(RabbitRequestMessageCreator.Send("DELETE", $"api/queues/{host}/{request.QueueName}/contents", request.Server));

            return true;
        }
    }
}