﻿using MediatR;
using Queueinator.Domain.RabbitMq;
using Queueinator.Domain.Utils;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Queueinator.Application.Features.Connections
{
    public class ConnectCommand : IRequest<Result<Server, BusinessException>>
    {
        public string Server { get; set; }
        public string Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }

    public class ConnectHandler : IRequestHandler<ConnectCommand, Result<Server, BusinessException>>
    {
        public async Task<Result<Server, BusinessException>> Handle(ConnectCommand request, CancellationToken cancellationToken)
        {
            //https://www.rabbitmq.com/uri-spec.html
            //https://stackoverflow.com/questions/43332913/can-i-iterate-through-queues-using-the-rabbitmq-net-client
            //https://stackoverflow.com/questions/33119611/how-to-make-rabbitmq-api-calls-with-vhost
            //https://rawcdn.githack.com/rabbitmq/rabbitmq-management/v3.8.0/priv/www/api/index.html

            var url = $"http://{request.Server}:{request.Port}/api/vhosts";

            using (var httpClient = new HttpClient())
            {
                using (var htttpRequest = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{request.User}:{request.Password}"));
                    htttpRequest.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

                    var response = await httpClient.SendAsync(htttpRequest);

                    var data = await response.Content.ReadAsStringAsync();

                    var virtualHosts = JsonSerializer.Deserialize<VirtualHost[]>(data);

                    var connection = new Server()
                    {
                        Name = request.Server,
                        Port = request.Port,
                        User = request.User,
                        Password = request.Password,
                    };

                    connection.AddHosts(virtualHosts.ToList());

                    return connection;
                }
            }
        }
    }
}
