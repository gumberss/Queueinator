using MediatR;
using Queueinator.Application.Infra.Requests;
using Queueinator.Domain.RabbitMq;
using Queueinator.Domain.Utils;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Queueinator.Application.Features.Connections
{
    public class ConnectCommand : IRequest<Result<Server, BusinessException>>
    {
        public Server Server { get; set; }
    }

    public class ConnectHandler : IRequestHandler<ConnectCommand, Result<Server, BusinessException>>
    {
        public async Task<Result<Server, BusinessException>> Handle(ConnectCommand request, CancellationToken cancellationToken)
        {
            var connection = await Result.Try(async () =>
            {
                var data = await RabbitRequestMessageCreator.Send("GET", "api/vhosts", request.Server);

                var virtualHosts = JsonSerializer.Deserialize<VirtualHost[]>(data);

                var connection = new Server()
                {
                    Name = request.Server.Name,
                    Port = request.Server.Port,
                    User = request.Server.User,
                    Password = request.Server.Password,
                };

                connection.AddHosts(virtualHosts.ToList());

                return connection;
            });

            return connection;
        }
    }
}
