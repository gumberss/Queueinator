using MediatR;
using Queueinator.Domain.Utils;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Queueinator.Application.Features.PurgeQueue
{
    public class PurgeQueueCommand : IRequest<Result<bool, BusinessException>>
    {
        public string Server { get; set; }
        public string Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string VHost { get; set; }
        public string QueueName { get; set; }
    }

    public class LoadQueuesHandler : IRequestHandler<PurgeQueueCommand, Result<bool, BusinessException>>
    {
        ///api/queues/vhost/name/contents
        public async Task<Result<bool, BusinessException>> Handle(PurgeQueueCommand request, CancellationToken cancellationToken)
        {
            //https://www.rabbitmq.com/uri-spec.html
            //https://stackoverflow.com/questions/43332913/can-i-iterate-through-queues-using-the-rabbitmq-net-client
            //https://stackoverflow.com/questions/33119611/how-to-make-rabbitmq-api-calls-with-vhost
            //https://rawcdn.githack.com/rabbitmq/rabbitmq-management/v3.8.0/priv/www/api/index.html

            var host = request.VHost == "/" ? "%2f" : request.VHost;

            var url = $"http://{request.Server}:{request.Port}/api/queues/{host}/{request.QueueName}/contents";

            using (var httpClient = new HttpClient())
            {
                using (var htttpRequest = new HttpRequestMessage(new HttpMethod("DELETE"), url))
                {
                    var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{request.User}:{request.Password}"));
                    htttpRequest.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");
                    try
                    {
                        var response = await httpClient.SendAsync(htttpRequest);

                        var data = await response.Content.ReadAsStringAsync();

                    }
                    catch (Exception ex)
                    {
                        return new BusinessException(System.Net.HttpStatusCode.InternalServerError, ex.Message);
                    }


                    return true;
                }
            }
        }
    }
}
