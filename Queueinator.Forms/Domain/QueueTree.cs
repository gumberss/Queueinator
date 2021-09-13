using Queueinator.Domain.RabbitMq;
using System.Windows.Forms;

namespace Queueinator.Forms.Domain
{
    public class QueueTree 
    {
        public HostQueue Queue { get; }

        public TreeNode Node { get; }

        public HostTree Host { get; }

        public QueueTree(HostQueue queue, TreeNode node, HostTree host)
        {
            Queue = queue;
            Node = node;
            Host = host;
        }
    }
}
