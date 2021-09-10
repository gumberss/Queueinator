using Queueinator.Domain.RabbitMq;
using System.Windows.Forms;

namespace Queueinator.Forms.Domain
{
    public class MessageTree
    {
        public QueueMessage Message { get; }

        public QueueTree Queue { get; }

        public MessageTree(QueueMessage message, QueueTree queue)
        {
            Message = message;
            Queue = queue;
        }
    }
}
