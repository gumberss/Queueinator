using MediatR;
using Queueinator.Application.Features.LoadBindings;
using Queueinator.Domain.RabbitMq;
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
    public partial class ExchangeControl : UserControl
    {
        private readonly IMediator _mediator;
        private readonly ExchangeTree _exchange;

        public ExchangeControl(IMediator mediator, ExchangeTree exchange)
        {
            InitializeComponent();

            this.Dock = DockStyle.Fill;

            tbSource.Text = "Source Bindings";
            tbDestination.Text = "Destination Bindings";

            _mediator = mediator;
            _exchange = exchange;

            LoadData().ConfigureAwait(false);
        }

        private async Task LoadData()
        {
            var bindings = await _mediator.Send(new LoadBindingsQuery()
            {
                Exchange = _exchange.Exchange,
                Server = _exchange.Server.Server
            });

            if (bindings.IsFailure)
            {
                MessageBox.Show($"An error ocured: {bindings.Error}");
                return;
            }

            tbSource.Controls.Add(new BindingDetailsControl(_exchange, bindings.Value.FromExchange));
            tbDestination.Controls.Add(new BindingDetailsControl(_exchange, bindings.Value.ToExchange));
        }

    }
}
