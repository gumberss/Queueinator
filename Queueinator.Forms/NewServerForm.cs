using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

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

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            if (txtServer.Text != "")
            {

                await _mediator.Send(new TryConnectCommand()
                {
                    
                });

                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Retry;
            }
        }
    }
}
