using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Queueinator.Application.Features.TryConnect;
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

        public VirtualHost VirtualHost { get; private set; }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            if (txtServer.Text != "")
            {

                var virtualHost = await _mediator.Send(new TryConnectCommand()
                {
                    Server = txtServer.Text,
                    Port = txtPort.Text,
                    User = txtUser.Text,
                    Password = txtPassword.Text
                });

                if (virtualHost.IsFailure)
                {
                    MessageBox.Show(virtualHost.Error.ToString());
                    DialogResult = DialogResult.Retry;
                }
                else
                {
                    VirtualHost = virtualHost.Value;
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
