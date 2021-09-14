using MediatR;
using Queueinator.Application.Features.LoadQueues;
using Queueinator.Application.Features.PurgeQueue;
using Queueinator.Domain.RabbitMq;
using Queueinator.Forms.Controls;
using Queueinator.Forms.Domain;
using Queueinator.Forms.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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
            serverTreeView.AfterCollapse += On_after_colapse_treeView;
            serverTreeView.AfterExpand += On_after_expand_treeView;

            tabControl.MouseDown += On_tabControl_MouseDown;
            this.FormClosed += On_FormClosed;
            _newServerForm = newServerForm;
            _mediator = mediator;

            serverTreeView.ImageList = new ImageList();
            serverTreeView.ImageList.Images.Add(Image.FromFile("Images/closed_folder.png"));
            serverTreeView.ImageList.Images.Add(Image.FromFile("Images/opened_folder.png"));
            serverTreeView.ImageList.Images.Add(Image.FromFile("Images/messages.png"));
            serverTreeView.ImageList.Images.Add(Image.FromFile("Images/message.png"));

            LoadServers();
        }

        private void On_FormClosed(object sender, FormClosedEventArgs e) => SaveServers();

        private void SaveServers()
        {
            var data = JsonSerializer.Serialize(_servers
                .Select(x => x.Value.Server)
                .Where(x => x.CanSave));

            if (!Directory.Exists("./db")) Directory.CreateDirectory("./db");

            File.WriteAllText("./db/servers.json", data);
        }

        private void LoadServers()
        {
            if (File.Exists("./db/servers.json"))
            {
                var data = File.ReadAllText("./db/servers.json");

                var servers = JsonSerializer.Deserialize<IEnumerable<Server>>(data);

                foreach (var server in servers) LoadServerOnTreeView(server, serverTreeView);
            }
        }

        private void On_after_colapse_treeView(object sender, TreeViewEventArgs e)
        {
            e.Node.ImageIndex = 0;
            e.Node.SelectedImageIndex = 0;
        }

        private void On_after_expand_treeView(object sender, TreeViewEventArgs e)
        {
            e.Node.ImageIndex = 1;
            e.Node.SelectedImageIndex = 1;
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

                LoadServerOnTreeView(newServer, serverTreeView);

            }
            else if (dialogResult == DialogResult.Retry)
            {
                ConnectToAServer(newServerPopup);
            }
            else if (dialogResult == DialogResult.Cancel)
            {

            }
        }

        private void LoadServerOnTreeView(Server newServer, TreeView treeView)
        {
            if (_servers.ContainsKey(newServer.Name)) return;

            var serverNode = treeView.Nodes.Add(newServer.Name, newServer.Name);

            var serverTree = new ServerTree(newServer, serverNode);
            _servers.Add(newServer.Name, serverTree);

            foreach (var host in newServer.Hosts)
            {
                if (_virtualHosts.ContainsKey($"{newServer.Name}:{host.Name}")) continue;

                var hostNode = serverNode.Nodes.Add($"{newServer.Name}:{host.Name}", host.Name);

                _virtualHosts.Add($"{newServer.Name}:{host.Name}", new HostTree(host, hostNode, serverTree));
            }
        }

        private async void On_TreeViewNode_DoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (_virtualHosts.ContainsKey($"{e.Node.Name}"))
            {
                var host = _virtualHosts[e.Node.Name];
                var serverNode = _servers[host.Node.Parent.Name];

                var queues = await _mediator.Send(new LoadQueuesQuery()
                {
                    Server = serverNode.Server,
                    VHost = host.Host.Name
                });

                if (queues.IsFailure)
                {
                    MessageBox.Show("Falha ao carregar as filas");
                    return;
                }

                new LoadTreeNodesService().LoadNode(
                    host.Node,
                    queues.Value.ToList(),
                    (queue, queueNode) => new QueueTree(queue, queueNode, host),
                    (key, queues, isLeaf) => isLeaf ? $"{key} ({queues.First().MessagesReadyCount}) {queues.First().State}" : $"{key} ({queues.Sum(x => x.MessagesReadyCount)})",
                    (key, lastQueue) => $"{host.FullName()}:{lastQueue.Name}",
                    ref _queues,
                    lastNode =>
                    {
                        lastNode.ImageIndex = 2;
                        lastNode.SelectedImageIndex = 2;
                        lastNode.ContextMenuStrip = CreateContextMenuForQueues(lastNode.Name);
                    }
                );

                host.Node.Expand();
            }
        }

        public ContextMenuStrip CreateContextMenuForQueues(String name)
        {
            ContextMenuStrip cms = new ContextMenuStrip()
            {
                ShowCheckMargin = false,
                ImageList = new ImageList(),

            };
            cms.ImageList.Images.Add(Image.FromFile("Images/trash.png"));

            var toolStripItem = new ToolStripMenuItem()
            {
                Text = "Purge",
                Name = name,
                ImageIndex = 0,
            };

            toolStripItem.Click += On_Purge_Queue;

            cms.Items.Add(toolStripItem);

            return cms;
        }

        private void On_tabControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Middle) return;

            for (int i = 0; i < tabControl.TabPages.Count; i++)
            {
                if (this.tabControl.GetTabRect(i).Contains(e.Location))
                {
                    this.tabControl.TabPages.RemoveAt(i);
                    return;
                }
            }
        }

        private void On_TreeViewNode_Click(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (_queues.ContainsKey(e.Node.Name))
            {
                TabPage page = GetPageByText(e.Node.Name);

                if (page is null)
                {
                    page = new TabPage(e.Node.Name);
                    page.Name = e.Node.Name;
                    tabControl.TabPages.Add(page);
                    page.Controls.Add(new QueueControl(_mediator, _queues[e.Node.Name]) { Name = e.Node.Name });
                }

                tabControl.SelectTab(page);
            }
        }

        private TabPage GetPageByText(String text)
        {
            TabPage page = null;

            foreach (TabPage existentPage in tabControl.TabPages)
            {
                if (existentPage.Text == text)
                {
                    page = existentPage;
                    break;
                }
            }

            return page;
        }

        private void On_Purge_Queue(object sender, EventArgs e)
        {
            if ((sender is ToolStripItem toolStrip))
            {
                PurgeQueue(toolStrip.Name).ConfigureAwait(false);
            }
        }

        private async Task PurgeQueue(String queueName)
        {
            var queue = _queues[queueName];

            if (queue is not null)
            {
                var server = queue.Host.Server.Server;
                var host = queue.Host.Host;

                var result = await _mediator.Send(new PurgeQueueCommand
                {
                    Server = server,
                    VHost = host.Name,
                    QueueName = queue.Queue.Name,
                });

                if (result.IsFailure)
                {
                    MessageBox.Show("Erro", result.Error.Message);
                    return;
                }

                var queuePurgedPage = GetPageByText(queue.Node.Name);

                if (queuePurgedPage is null) return;

                var queueControls = queuePurgedPage.Controls
                    .Find(queue.Node.Name, false)
                    .OfType<QueueControl>()
                    .ToList();

                queueControls.ForEach(x => x.ReloadMessages().ConfigureAwait(false));
            }
        }
    }
}
