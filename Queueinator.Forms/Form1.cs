﻿using MediatR;
using Queueinator.Application.Features.LoadQueues;
using Queueinator.Domain.RabbitMq;
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
            serverTreeView.NodeMouseDoubleClick += LoadQueues_TreeNodeDoubleClick;
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

                _servers.Add(newServer.Name, new ServerTree(newServer, serverNode));

                foreach (var host in newServer.Hosts)
                {
                    if (_servers.ContainsKey(host.Name)) continue;

                    var hostNode = serverNode.Nodes.Add(host.Name, host.Name);

                    _virtualHosts.Add(host.Name, new HostTree(host, hostNode));
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

        private async void LoadQueues_TreeNodeDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
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

                LoadNode(host.Node, queues.Value.ToList());
            }else if (_queues.ContainsKey(e.Node.Name))
            {
            }
        }

        private void LoadNode(TreeNode parentNode, List<HostQueue> queues, int depth = 0)
        {
            if (!queues.Any()) return;

            char queueNameSeparator = '-';

            var groupedQueues = queues.GroupBy(x => x.Name.Split(queueNameSeparator)[depth]);

            foreach (var item in groupedQueues)
            {
                var lastNodeItems = item.Where(x => x.Name.Count(y => y == queueNameSeparator) == depth);

                var newItems = item.Except(lastNodeItems).ToList();

                if (!newItems.Any())
                {
                    var lastQueue = lastNodeItems.First();
                    var queueNode = AddNode(parentNode, lastQueue.Name, $"{item.Key} ({lastQueue.MessagesReadyCount}) {lastQueue.State}" );
                    _queues.Add(queueNode.Name, new QueueTree(lastQueue, queueNode));
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
