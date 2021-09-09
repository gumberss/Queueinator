using Queueinator.Domain.RabbitMq;
using System.Windows.Forms;

namespace Queueinator.Forms.Domain
{
    public class QueueTree
    {
        public HostQueue Host { get; set; }

        public TreeNode Node { get; set; }

        public QueueTree(HostQueue host, TreeNode node)
        {
            Host = host;
            Node = node;
        }
    }
}
