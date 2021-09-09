using MediatR;
using Queueinator.Application.Features.LoadQueues;
using Queueinator.Domain.RabbitMq;
using Queueinator.Forms.Controls;
using Queueinator.Forms.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Queueinator.Forms
{
    public partial class Form1 : Form
    {
        private NewServerForm _newServerForm;
        private IMediator _mediator;

        public Form1(IMediator mediator, NewServerForm newServerForm)
        {
            InitializeComponent();

            tsAddServer.Click += tsAddServer_Click;
            serverTreeView.NodeMouseDoubleClick += On_TreeViewNode_DoubleClick;
            serverTreeView.NodeMouseClick += On_TreeViewNode_Click;
            _newServerForm = newServerForm;
            _mediator = mediator;


        }

        private void tsAddServer_Click(object sender, EventArgs e)
        {
            ConnectToAServer(_newServerForm);
        }

        Dictionary<String, HostTree> _virtualHosts = new Dictionary<string, HostTree>();
        Dictionary<String, ServerTree> _servers = new Dictionary<string, ServerTree>();
        Dictionary<String, QueueTree> _queues = new Dictionary<string, QueueTree>();

        private void ConnectToAServer(NewServerForm newServerPopup)
        {
            var dialogResult = newServerPopup.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                var newServer = newServerPopup.Server;

                if (_servers.ContainsKey(newServer.Name)) return;

                var serverNode = serverTreeView.Nodes.Add(newServer.Name, newServer.Name);

                var serverTree = new ServerTree(newServer, serverNode);
                _servers.Add(newServer.Name, serverTree);

                foreach (var host in newServer.Hosts)
                {
                    if (_servers.ContainsKey(host.Name)) continue;

                    var hostNode = serverNode.Nodes.Add(host.Name, host.Name);

                    _virtualHosts.Add(host.Name, new HostTree(host, hostNode, serverTree));
                }

            }
            else if (dialogResult == DialogResult.Retry)
            {
                ConnectToAServer(newServerPopup);
            }
            else if (dialogResult == DialogResult.Cancel)
            {

            }
        }

        private async void On_TreeViewNode_DoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (_virtualHosts.ContainsKey(e.Node.Name))
            {
                var host = _virtualHosts[e.Node.Name];
                var serverNode = _servers[host.Node.Parent.Name];

                var queues = await _mediator.Send(new LoadQueuesCommand()
                {
                    Password = serverNode.Server.Password,
                    Server = serverNode.Server.Name,
                    Port = serverNode.Server.Port,
                    User = serverNode.Server.User,
                    VHost = host.Host.Name
                });

                if (queues.IsFailure)
                {
                    MessageBox.Show("Falha ao carregar as filas");
                    return;
                }

                //clear(node and _queues), then load

                LoadNode(host.Node, queues.Value.ToList());
            }
        }

        private void On_TreeViewNode_Click(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (_queues.ContainsKey(e.Node.Name))
            {
                var page = new TabPage(e.Node.Name);
                tabControl.TabPages.Add(page);
                page.Controls.Add(new QueueControl(_mediator, _queues[e.Node.Name]));
                //var messages = _mediator.Send(new LoadMessages());
            }
        }


        private void LoadNode(TreeNode parentNode, List<HostQueue> queues, int depth = 0)
        {

            if (!queues.Any()) return;

            char[] queueNameSeparator = new[] { '-', '.' };

            var groupedQueues = queues.GroupBy(x => x.Name.Split(queueNameSeparator)[depth]);

            foreach (var item in groupedQueues)
            {
                var lastNodeItems = item.Where(x => x.Name.Count(y => queueNameSeparator.Contains(y)) == depth);

                var newItems = item.Except(lastNodeItems).ToList();

                if (!newItems.Any())
                {
                    var lastQueue = lastNodeItems.First();
                    var queueNode = AddNode(parentNode, lastQueue.Name, $"{item.Key} ({lastQueue.MessagesReadyCount}) {lastQueue.State}");

                    if (!_queues.ContainsKey(queueNode.Name))
                        _queues.Add(queueNode.Name, new QueueTree(lastQueue, queueNode, _virtualHosts[lastQueue.VirtualHostName]));
                }
                else
                {
                    var text = $"{item.Key} ({item.Sum(x => x.MessagesReadyCount)})";

                    var newParent = AddNode(parentNode, item.Key, text);

                    LoadNode(newParent, newItems, depth + 1);
                }
            }
        }

        private TreeNode AddNode(TreeNode node, string name, string text)
        {
            return node.Nodes.Add(name, text);
        }


    }
}
