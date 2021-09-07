using MediatR;
using Queueinator.Domain.Connections;
using Queueinator.Domain.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace Queueinator.Application.Features.TryConnect
{
    public class TryConnectCommand : IRequest<Result<Connection, BusinessException>>
    {
        public string Server { get; set; }
        public string Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }

    public class TryConnectHandler : IRequestHandler<TryConnectCommand, Result<Connection, BusinessException>>
    {
        public Task<Result<Connection, BusinessException>> Handle(TryConnectCommand request, CancellationToken cancellationToken)
        {
            //https://www.rabbitmq.com/uri-spec.html
            //https://stackoverflow.com/questions/43332913/can-i-iterate-through-queues-using-the-rabbitmq-net-client
            //https://stackoverflow.com/questions/33119611/how-to-make-rabbitmq-api-calls-with-vhost
            //https://rawcdn.githack.com/rabbitmq/rabbitmq-management/v3.8.0/priv/www/api/index.html
            throw new System.NotImplementedException();
        }
    }
}
