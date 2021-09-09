using Queueinator.Domain.RabbitMq;
using System.Windows.Forms;

namespace Queueinator.Forms.Domain
{
    public class ServerTree
    {
        public Server Server { get; set; }

        public TreeNode Node { get; set; }

        public ServerTree(Server server, TreeNode node)
        {
            Server = server;
            Node = node;
        }
    }
}
