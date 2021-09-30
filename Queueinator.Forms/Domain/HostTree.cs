using Queueinator.Domain.RabbitMq;
using System;
using System.Windows.Forms;

namespace Queueinator.Forms.Domain
{
    public class HostTree
    {
        public VirtualHost Host { get; }

        public TreeNode Node { get; }

        public ServerTree Server { get; }

        public TreeNode QueuesNode { get; }

        public TreeNode ExchangeNode { get; }
        public bool Refreshing { get;  set; }

        public string FullName()
        {
            return $"{Server.Server.Name}:{this.Host.Name}";
        }

        public HostTree(VirtualHost host, TreeNode node, ServerTree server)
        {
            Host = host;
            Node = node;
            Server = server;
            
            QueuesNode = new TreeNode("Queues") { Name = "Queues" };

            node.Nodes.Add(QueuesNode);

            ExchangeNode = new TreeNode("Exchanges") { Name = "Exchanges" };
            node.Nodes.Add(ExchangeNode);
        }

       
    }
}
