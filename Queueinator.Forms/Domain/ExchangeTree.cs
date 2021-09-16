using Queueinator.Domain.RabbitMq;
using System.Windows.Forms;

namespace Queueinator.Forms.Domain
{
    public class ExchangeTree
    {
        public HostExchange Exchange { get; }

        public TreeNode Node { get; }

        public HostTree Host { get; }

        public ExchangeTree(HostExchange exchange, TreeNode node, HostTree host)
        {
            Exchange = exchange;
            Node = node;
            Host = host;
        }
    }
}
