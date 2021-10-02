using MediatR;
using Queueinator.Application.Features.DeleteMessages;
using Queueinator.Application.Features.LoadExchanges;
using Queueinator.Application.Features.LoadQueues;
using Queueinator.Application.Features.PublishMessages;
using Queueinator.Application.Features.PurgeQueue;
using Queueinator.Domain.RabbitMq;
using Queueinator.Domain.Utils;
using Queueinator.Forms.Controls;
using Queueinator.Forms.Domain;
using Queueinator.Forms.Domain.Enums;
using Queueinator.Forms.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Queueinator.Forms
{
    public partial class MainScreen : Form
    {
        private readonly NewServerForm _newServerForm;
        private readonly IMediator _mediator;

        public MainScreen(IMediator mediator, NewServerForm newServerForm)
        {
            InitializeComponent();
            Load += (object sender, EventArgs e) =>
            {
                this.SetStyle(ControlStyles.DoubleBuffer, true);

                int style = NativeWindowAPI.GetWindowLong(this.Handle, NativeWindowAPI.GWL_EXSTYLE);
                style |= NativeWindowAPI.WS_EX_COMPOSITE;
                NativeWindowAPI.SetWindowLong(this.Handle, NativeWindowAPI.GWL_EXSTYLE, style);
            };

            _newServerForm = newServerForm;
            _mediator = mediator;

            tsAddServer.Click += tsAddServer_Click;
            tsRemoveServer.Click += tsRemoveServer_Click;
            serverTreeView.NodeMouseDoubleClick += On_TreeViewNode_DoubleClick;
            serverTreeView.NodeMouseClick += On_TreeViewNode_Click;
            serverTreeView.AfterCollapse += On_after_colapse_treeView;
            serverTreeView.AfterExpand += On_after_expand_treeView;

            tabControl.MouseDown += On_tabControl_MouseDown;
            this.FormClosed += On_FormClosed;

            serverTreeView.ImageList = new ImageList();
            serverTreeView.ImageList.Images.Add(Image.FromFile("Images/closed_folder.png"));
            serverTreeView.ImageList.Images.Add(Image.FromFile("Images/opened_folder.png"));
            serverTreeView.ImageList.Images.Add(Image.FromFile("Images/messages.png"));
            serverTreeView.ImageList.Images.Add(Image.FromFile("Images/message.png"));
            serverTreeView.ImageList.Images.Add(Image.FromFile("Images/broadcast.png"));

            serverTreeView.AllowDrop = true;
            serverTreeView.DragOver += On_server_tree_view_drag_over;
            serverTreeView.DragDrop += On_server_tree_view_drag_drop;
            LoadServers();

        }

        private async void On_server_tree_view_drag_drop(object sender, DragEventArgs e)
        {
            var name = typeof(MoveMessageData).FullName;

            if (e.Data.GetDataPresent(name))
            {
                var moveMessageData = e.Data.GetData(name) as MoveMessageData;

                var selectedItemToDropPosition = serverTreeView.PointToClient(new Point(e.X, e.Y));

                var selectedItem = serverTreeView.GetNodeAt(selectedItemToDropPosition);

                if (selectedItem is null) return;

                Result<bool, BusinessException> moveMessagesResult = default;

                if (_queues.ContainsKey(selectedItem.Name))
                {
                    var queue = _queues[selectedItem.Name];

                    moveMessagesResult = await _mediator.Send(new PublishToQueueCommand()
                    {
                        Server = queue.Host.Server.Server,
                        Queue = queue.Queue.Name,
                        Messages = moveMessageData.Messages.Select(x => x.Message),
                        VHost = queue.Host.Host.Name
                    });
                }
                else if (_exchanges.ContainsKey(selectedItem.Name))
                {
                    var exchange = _exchanges[selectedItem.Name];

                    moveMessagesResult = await _mediator.Send(new PublishToExchangeCommand()
                    {
                        Server = exchange.Host.Server.Server,
                        Exchange = exchange.Exchange,
                        Messages = moveMessageData.Messages.Select(x => x.Message),
                        VHost = exchange.Host.Host.Name
                    });
                }

                if (moveMessageData.PostAction == OnMoveEnum.RemoveFromSource)
                {
                    if (moveMessagesResult != default && moveMessagesResult.IsSuccess && moveMessagesResult)
                    {
                        var movedMessages = moveMessageData.Messages.Select(x => x.Message);
                        var queue = moveMessageData.Messages.First().Queue;

                        var deleteResult = await _mediator.Send(new DeleteMessagesCommand()
                        {
                            Server = queue.Host.Server.Server,
                            Queue = queue.Queue.Name,
                            VHost = queue.Host.Host.Name,
                            Messages = movedMessages.ToList()
                        });

                        if (deleteResult.IsFailure)
                        {
                            MessageBox.Show("A mensagem foi postada, mas nao foi possível remover da fila atual");
                            return;
                        }

                        ReloadTab(queue);
                    }
                    else if (moveMessagesResult != default && (moveMessagesResult.IsFailure || !moveMessagesResult))
                    {
                        MessageBox.Show("Falha ao publicar as mensagens");
                    }
                }
            }
        }

        private void On_server_tree_view_drag_over(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
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

        private void tsRemoveServer_Click(object sender, EventArgs e)
        {
            var serverName = serverTreeView.SelectedNode.Name;
            if (_servers.ContainsKey(serverName))
            {
                var server = _servers[serverName];

                var hosts = _virtualHosts.Values.Where(x => x.Server == server);

                foreach (TabPage tab in tabControl.TabPages)
                {
                    if (hosts.Any(x => tab.Name.StartsWith(x.FullName())))
                        this.tabControl.TabPages.Remove(tab);
                }

                foreach (var host in hosts)
                {
                    _virtualHosts.Remove(host.FullName());
                }
                _servers.Remove(serverName);


                var removeQueues = _queues.Where(x => x.Value.Host.Server.Server == server.Server).ToList();

                removeQueues.ForEach(x => _queues.Remove(x.Key));

                var removeExchanges = _exchanges.Where(x => x.Value.Host.Server.Server == server.Server).ToList();
                removeExchanges.ForEach(x => _exchanges.Remove(x.Key));

                RemoveNodes(new[] { server.Node });
            }
        }

        private void RemoveNodes(IEnumerable<TreeNode> nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (node is null) continue;

                if (node.Nodes.Count > 0)
                    RemoveNodes(node.Nodes.Cast<TreeNode>());

                node.Remove();
            }
        }

        Dictionary<String, HostTree> _virtualHosts = new Dictionary<string, HostTree>();
        Dictionary<String, ServerTree> _servers = new Dictionary<string, ServerTree>();
        Dictionary<String, QueueTree> _queues = new Dictionary<string, QueueTree>();
        Dictionary<String, ExchangeTree> _exchanges = new Dictionary<string, ExchangeTree>();
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

                var hostTree = new HostTree(host, hostNode, serverTree);

                hostTree.QueuesNode.ContextMenuStrip = CreateContextMenuForGroupQueues(hostTree);

                _virtualHosts.Add($"{newServer.Name}:{host.Name}", hostTree);
            }
        }

        private async void On_TreeViewNode_DoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Parent is null || !_virtualHosts.ContainsKey(e.Node.Parent.Name)) return;

            if (e.Node.Name == "Queues")
            {
                var host = _virtualHosts[e.Node.Parent.Name];
                var server = host.Server;

                await LoadQueues(host, server);
            }
            else if (e.Node.Name == "Exchanges")
            {
                var host = _virtualHosts[e.Node.Parent.Name];
                var server = host.Server;

                await LoadExchanges(host, server);
            }
        }

        private async Task LoadExchanges(HostTree host, ServerTree server)
        {
            var exchanges = await _mediator.Send(new LoadExchangesQuery()
            {
                Server = server.Server,
                VHost = host.Host.Name
            });

            if (exchanges.IsFailure)
            {
                MessageBox.Show("Falha ao carregar as filas");
                return;
            }

            new LoadTreeNodesService().LoadNode(
                host.ExchangeNode,
                exchanges.Value.ToList(),
                (exchange, exchangeNode) => new ExchangeTree(exchange, exchangeNode, host),
                (key, exchange, isLeaf) => isLeaf ? $"{key} ({exchange.First().Type})" : $"{key}",
                (key, lastExchange) => $"Exchange:{host.FullName()}:{lastExchange.Name}",
                ref _exchanges,
                (lastNode, element, oldTree) =>
                {
                    lastNode.ImageIndex = 4;
                    lastNode.SelectedImageIndex = 4;
                }
            );

            host.ExchangeNode.Expand();
        }


        private async Task LoadQueues(HostTree host, ServerTree server)
        {
            var queues = await _mediator.Send(new LoadQueuesQuery()
            {
                Server = server.Server,
                VHost = host.Host.Name
            });

            if (queues.IsFailure)
            {
                MessageBox.Show("Falha ao carregar as filas");
                return;
            }

            List<TreeNode> changed = new List<TreeNode>();

            try
            {

                serverTreeView.BeginUpdate();
                new LoadTreeNodesService().LoadNode(
                    host.QueuesNode,
                    queues.Value.ToList(),
                    (queue, queueNode) => new QueueTree(queue, queueNode, host),
                    (key, queues, isLeaf) => isLeaf ? $"{key} ({queues.First().MessagesReadyCount}) {queues.First().State}" : $"{key} ({queues.Sum(x => x.MessagesReadyCount)})",
                    (key, lastQueue) => $"Queue:{host.FullName()}:{lastQueue.Name}",
                    ref _queues,
                    (lastNode, element, oldTree) =>
                    {
                        lastNode.ImageIndex = 2;
                        lastNode.SelectedImageIndex = 2;
                        lastNode.ContextMenuStrip = CreateContextMenuForQueues(lastNode.Name);

                        if (oldTree is not null)
                        {
                            Color color = Color.White;

                            if (oldTree.Queue.MessagesCount > element.MessagesCount)
                                color = Color.LightCoral;
                            else if (oldTree.Queue.MessagesCount < element.MessagesCount)
                                color = Color.LightGreen;

                            if (color != Color.White)
                            {
                                changed.AddRange(ChangeTreeBackColor(lastNode, color, false, changed));
                            }
                        }
                    }
                );
            }
            finally
            {
                serverTreeView.EndUpdate();
            }

            host.QueuesNode.Expand();
        }

        private List<TreeNode> GetAllRelatedNodes(IEnumerable<TreeNode> changed)
        {
            var allNodes = new List<TreeNode>();

            foreach (var item in changed)
            {
                foreach (TreeNode parent in item.Nodes)
                {
                    allNodes.AddRange(GetAllRelatedNodes(new[] { parent }));
                }

                allNodes.Add(item);
            }

            return allNodes;
        }

        private async Task SoftBlink(TreeNode node, Color c1, Color c2, short CycleTime_ms, bool BkClr)
        {
            var sw = new Stopwatch(); sw.Start();
            short halfCycle = (short)Math.Round(CycleTime_ms * 0.5);

            while (sw.ElapsedMilliseconds < CycleTime_ms)
            {
                await Task.Delay(1);
                var n = sw.ElapsedMilliseconds % CycleTime_ms;
                var per = (double)Math.Abs(n - halfCycle) / halfCycle;
                var red = (short)Math.Round((c2.R - c1.R) * per) + c1.R;
                var grn = (short)Math.Round((c2.G - c1.G) * per) + c1.G;
                var blw = (short)Math.Round((c2.B - c1.B) * per) + c1.B;
                var clr = Color.FromArgb(red, grn, blw);
                if (BkClr) node.BackColor = clr;
            }
        }

        private List<TreeNode> ChangeTreeBackColor(TreeNode node, Color color, bool reset, List<TreeNode> changed)
        {
            if (node == null) return new List<TreeNode>();

            if (changed.Contains(node)) return new List<TreeNode>();

            SoftBlink(node, color, Color.White, 2000, true).ConfigureAwait(false);

            return new[] { node }.Concat(ChangeTreeBackColor(node.Parent, color, reset, changed)).ToList();
        }

        public ContextMenuStrip CreateContextMenuForGroupQueues(HostTree host)
        {
            ContextMenuStrip cms = new ContextMenuStrip()
            {
                ShowCheckMargin = false,
                ImageList = new ImageList(),

            };

            var toolStripItem = new ToolStripMenuItem()
            {
                Text = "Refresh Interval",
                Checked = host.Refreshing
            };

            toolStripItem.Click += async (object o, EventArgs e) =>
            {
                host.Refreshing = !host.Refreshing;

                while (host.Refreshing)
                {
                    LoadQueues(host, host.Server).ConfigureAwait(false);
                    await Task.Delay(5000);
                }
            };

            cms.Items.Add(toolStripItem);

            return cms;
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
                    var queue = _queues[e.Node.Name];
                    page.Text = $"Queue: {queue.Queue.Name}";
                    tabControl.TabPages.Add(page);
                    page.Controls.Add(new QueueControl(_mediator, queue) { Name = e.Node.Name });
                }

                tabControl.SelectTab(page);
            }
            else if (_exchanges.ContainsKey(e.Node.Name))
            {
                TabPage page = GetPageByText(e.Node.Name);

                if (page is null)
                {
                    page = new TabPage(e.Node.Name);
                    page.Name = e.Node.Name;
                    var exchange = _exchanges[e.Node.Name];
                    page.Text = $"Exchange: {exchange.Exchange.Name}";
                    tabControl.TabPages.Add(page);
                    page.Controls.Add(new ExchangeControl(_mediator, exchange) { Name = e.Node.Name });
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

                ReloadTab(queue);
            }
        }

        private void ReloadTab(QueueTree queue)
        {
            var tab = GetPageByText(queue.Node.Name);

            if (tab is null) return;

            var queueControls = tab.Controls
                .Find(queue.Node.Name, false)
                .OfType<QueueControl>()
                .ToList();

            queueControls.ForEach(x => x.ReloadMessages().ConfigureAwait(false));
        }
    }
}
