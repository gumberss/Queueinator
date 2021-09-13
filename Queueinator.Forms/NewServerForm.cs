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
using Queueinator.Forms.Extensions;

namespace Queueinator.Forms
{
    public partial class NewServerForm : Form
    {
        private IMediator _mediator;

        public NewServerForm(IMediator mediator)
        {
            InitializeComponent();
            _mediator = mediator;

            txtServer.PlaceholderText = "localhost";
            txtPort.PlaceholderText = "15672";
            txtUser.PlaceholderText = "guest";
            txtPassword.PlaceholderText = "guest";
        }

        public Server Server { get; private set; }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                var serverData = new Server
                {
                    Name = txtServer.Text.Default(txtServer.PlaceholderText),
                    Port = txtPort.Text.Default(txtPort.PlaceholderText),
                    User = txtUser.Text.Default(txtUser.PlaceholderText),
                    Password = txtPassword.Text.Default(txtPassword.PlaceholderText)
                };

                var server = await _mediator.Send(new ConnectCommand()
                {
                    Server = serverData
                });

                if (server.IsFailure)
                {
                    MessageBox.Show(server.Error.ToString());
                    DialogResult = DialogResult.Retry;
                }
                else
                {
                    Server = server.Value;
                    Server.CanSave = cbCanSave.Checked;
                    DialogResult = DialogResult.OK;
                    cbCanSave.Checked = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                DialogResult = DialogResult.Retry;
            }

        }
    }
}
