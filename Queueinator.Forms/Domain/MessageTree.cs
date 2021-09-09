using Queueinator.Domain.RabbitMq;
using System.Windows.Forms;

namespace Queueinator.Forms.Domain
{
    public class MessageTree
    {
        public QueueMessage Message { get; }

        public TreeNode Node { get; }

        public HostQueue Queue { get; }

        public MessageTree(QueueMessage message, TreeNode node, HostQueue queue)
        {
            Message = message;
            Node = node;
            Queue = queue;
        }
    }
}
