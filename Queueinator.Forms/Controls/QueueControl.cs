﻿using MediatR;
using Queueinator.Application.Features.LoadMessages;
using Queueinator.Application.Features.PublishMessages;
using Queueinator.Forms.Domain;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace Queueinator.Forms.Controls
{
    public partial class QueueControl : UserControl
    {
        private readonly IMediator _mediator;
        private readonly QueueTree _queueTree;
        private Dictionary<Guid, MessageTree> _messages = new Dictionary<Guid, MessageTree>();

        public QueueControl(IMediator mediator, QueueTree queueTree)
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;

            _mediator = mediator;
            _queueTree = queueTree;

            tabControl1.TabPages[0].Text = "Payload";
            tabControl1.TabPages[1].Text = "Details";

            messagesGrid.Columns.Add("Id", "Id");
            messagesGrid.Columns.Add("Bytes", "Bytes");
            messagesGrid.Columns.Add("Payload", "Payload");
            messagesGrid.Columns[2].Width = 500;

            messagesGrid.CellMouseDown += On_Cell_Mouse_Down;
            messagesGrid.SelectionChanged += On_Change_Row_selection;

            messagesGrid.AllowDrop = true;

            LoadMessages().ConfigureAwait(false);

            btnReload.Click += On_reload_messages_clicked;
        }

        private void On_Cell_Mouse_Down(object sender, DataGridViewCellMouseEventArgs e)
        {
            List<MessageTree> selectedMessages = new List<MessageTree>();

            foreach (DataGridViewCell cell in messagesGrid.SelectedCells)
            {
                var row = messagesGrid.Rows[cell.RowIndex];

                var messageId = row.Cells[0].Value?.ToString();

                if (messageId is not null)
                {
                    selectedMessages.Add(_messages[Guid.Parse(messageId)]);
                }
            }

            DoDragDrop(selectedMessages, DragDropEffects.Move);
        }

        private void On_drag_item(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var a = e.Item;
            }
        }
        private void On_Change_Row_selection(object sender, EventArgs e)
        {
            if (messagesGrid.SelectedCells.Count == 0) return;

            var selectedRow = messagesGrid.Rows[messagesGrid.SelectedCells[0].RowIndex];

            var id = selectedRow.Cells[0].Value;

            if (id is null) return;

            var message = _messages[(Guid)id];

            var payload = message.Message.Payload;

            txtMessage.Text = payload;

            message.Message.Payload = "Tab Payload";

            txtDetails.Text = JsonSerializer.Serialize(message.Message, new JsonSerializerOptions
            {
                WriteIndented = true,
            });

            message.Message.Payload = payload;
        }

        private async Task LoadMessages()
        {
            var server = _queueTree.Host.Server.Server;

            var messages = await _mediator.Send(new LoadMessagesQuery()
            {
                HostName = _queueTree.Queue.VirtualHostName,
                Server = server,
                QueueName = _queueTree.Queue.Name
            });

            if (messages.IsFailure)
            {
                MessageBox.Show("Falha ao carregar as mensagens");
                return;
            }

            foreach (var message in messages.Value)
            {
                try
                {
                    var messageTree = new MessageTree(message, _queueTree);
                    _messages.Add(message.Properties.Id, messageTree);

                    var row = new DataGridViewRow()
                    {
                        ReadOnly = true,
                    };
                    row.Cells.Add(new DataGridViewTextBoxCell() { Value = message.Properties.Id });
                    row.Cells.Add(new DataGridViewTextBoxCell() { Value = message.Bytes });
                    row.Cells.Add(new DataGridViewTextBoxCell() { Value = message.Payload });
                    //row.ContextMenuStrip = CreateContextMenuForMessages(message.Properties.Id.ToString());

                    messagesGrid.ContextMenuStrip = CreateContextMenuForMessages(message.Properties.Id.ToString());
                    messagesGrid.Rows.Add(row);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
            }
        }

        public ContextMenuStrip CreateContextMenuForMessages(String name)
        {
            ContextMenuStrip cms = new ContextMenuStrip()
            {
                ImageList = new ImageList(),
            };

            cms.ImageList.Images.Add(Image.FromFile("./Images/send.png"));

            var toolStripItem = new ToolStripMenuItem()
            {
                Text = "Publish",
                Name = name,
                ImageIndex = 0
            };

            var exchange = new ToolStripMenuItem()
            {
                Text = "Exchange",
                Name = name,
            };

            exchange.Click += On_Publish_message;

            toolStripItem.DropDownItems.Add(exchange);

            cms.Items.Add(toolStripItem);

            return cms;
        }

        private void On_Publish_message(object sender, EventArgs e)
        {
            if ((sender is ToolStripItem))
            {
                var rows = new List<DataGridViewRow>(messagesGrid.SelectedCells.Count);

                foreach (DataGridViewCell cell in messagesGrid.SelectedCells)
                {
                    if (!rows.Contains(cell.OwningRow))
                        rows.Add(cell.OwningRow);
                }

                foreach (var row in rows)
                {
                    var messageIdObj = row.Cells[0].Value;
                    if (messageIdObj is not null)
                    {
                        var messageId = Guid.Parse(messageIdObj.ToString());
                        PublishMessage(_messages[messageId]).ConfigureAwait(false);
                    }

                }

                ReloadMessages().ConfigureAwait(false);
            }
        }

        private async Task PublishMessage(MessageTree messageTree)
        {
            var result = await _mediator.Send(new PublishToExchangeCommand()
            {
                Server = messageTree.Queue.Host.Server.Server,
                Exchange = messageTree.Message.Exchange,
                VHost = messageTree.Queue.Host.Host.Name,
                Message = messageTree.Message
            });

            if (result.IsFailure)
                MessageBox.Show("It was not possible to publish the message", "Error");
        }

        private void On_reload_messages_clicked(object sender, EventArgs e)
        {
            ReloadMessages().ConfigureAwait(false);
        }

        public async Task ReloadMessages()
        {
            messagesGrid.Rows.Clear();
            _messages.Clear();

            await LoadMessages();
        }
    }
}
