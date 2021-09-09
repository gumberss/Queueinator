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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Queueinator.Forms.Controls
{
    public partial class QueueControl : UserControl
    {
        private readonly IMediator _mediator;
        private readonly QueueTree _queueTree;

        private Dictionary<String, MessageTree> _messages = new Dictionary<string, MessageTree>();

        public QueueControl(IMediator mediator, QueueTree queueTree)
        {
            InitializeComponent();
            _mediator = mediator;
            _queueTree = queueTree;

            LoadMessages().ConfigureAwait(false);
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

            foreach (var message in messages.Value)
            {
                try
                {
                    messagesGrid.Rows.Add(message.Properties.Id, message.Bytes, message.Payload);
                }
                catch (Exception ex)
                {

                }

            }
        }
    }
}
