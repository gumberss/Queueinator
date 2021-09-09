using Queueinator.Domain.RabbitMq;
using System.Windows.Forms;

namespace Queueinator.Forms.Domain
{
    public class HostTree
    {
        public VirtualHost Host { get; }

        public TreeNode Node { get; }

        public ServerTree Server { get; }

        public HostTree(VirtualHost host, TreeNode node, ServerTree server)
        {
            Host = host;
            Node = node;
            Server = server;
        }
    }
}
