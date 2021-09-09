using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Queueinator.Application.Features.Connections;
using Queueinator.Domain.RabbitMq;

namespace Queueinator.Forms
{
    public partial class NewServerForm : Form
    {
        private IMediator _mediator;

        public NewServerForm(IMediator mediator)
        {
            InitializeComponent();
            _mediator = mediator;
        }

        public Server Server { get; private set; }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            if (txtServer.Text != "")
            {
                var server = await _mediator.Send(new ConnectCommand()
                {
                    Server = txtServer.Text,
                    Port = txtPort.Text,
                    User = txtUser.Text,
                    Password = txtPassword.Text
                });

                if (server.IsFailure)
                {
                    MessageBox.Show(server.Error.ToString());
                    DialogResult = DialogResult.Retry;
                }
                else
                {
                    Server = server.Value;
                    DialogResult = DialogResult.OK;
                }
            }
            else
            {
                DialogResult = DialogResult.Retry;
            }
        }
    }
}
