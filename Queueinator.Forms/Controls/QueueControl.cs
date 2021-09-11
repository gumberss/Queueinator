using MediatR;
using Queueinator.Application.Features.LoadMessages;
using Queueinator.Forms.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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

            messagesGrid.SelectionChanged += On_Change_Row_selection;

            LoadMessages().ConfigureAwait(false);
        }

        private void On_Change_Row_selection(object sender, EventArgs e)
        {
            if (messagesGrid.SelectedCells.Count == 0) return;

            var selectedRow = messagesGrid.Rows[messagesGrid.SelectedCells[0].RowIndex];

            var id = selectedRow.Cells[0].Value;

            if (id is null) return;

            var message = _messages[(Guid)id];

            txtMessage.Text = message.Message.Payload;

            var payload = message.Message.Payload;

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

            var messages = await _mediator.Send(new LoadMessagesCommand()
            {
                HostName = _queueTree.Queue.VirtualHostName,
                Server = server.Name,
                Password = server.Password,
                Port = server.Port,
                User = server.User,
                QueueName = _queueTree.Queue.Name
            });

            if (messages.IsFailure)
            {
                MessageBox.Show("Falha ao carregar as mensagens");
                return;
            }
            messagesGrid.Columns.Add("Id", "Id");
            messagesGrid.Columns.Add("Bytes", "Bytes");
            messagesGrid.Columns.Add("Payload", "Payload");
            messagesGrid.Columns[2].Width = 500;

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
                    row.Cells.Add(new DataGridViewTextBoxCell(){ Value = message.Properties.Id});
                    row.Cells.Add(new DataGridViewTextBoxCell() { Value = message.Bytes });
                    row.Cells.Add(new DataGridViewTextBoxCell() { Value = message.Payload });
                    messagesGrid.Rows.Add(row);
            }
                catch (Exception ex)
            {

            }

        }
    }
}
}
