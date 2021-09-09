using Queueinator.Domain.RabbitMq;
using System.Windows.Forms;

namespace Queueinator.Forms.Domain
{
    public class HostTree
    {
        public VirtualHost Host { get; set; }

        public TreeNode Node { get; set; }

        public HostTree(VirtualHost host, TreeNode node)
        {
            Host = host;
            Node = node;
        }
    }
}
