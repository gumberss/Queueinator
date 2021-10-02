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
    public partial class BindingDetailsControl : UserControl
    {
        private readonly ExchangeTree _exchange;
        private readonly List<ExchangeBinding> _bindings;

        public BindingDetailsControl(ExchangeTree exchange, List<ExchangeBinding> bindings)
        {
            InitializeComponent();

            _exchange = exchange;
            _bindings = bindings;
        }

        public void FillDataSource()
        {
            foreach (var binding in _bindings)
            {
                try
                {
                    var row = new DataGridViewRow()
                    {
                        ReadOnly = true,
                    };
                    row.Cells.Add(new DataGridViewTextBoxCell() { Value = binding.Source });
                    row.Cells.Add(new DataGridViewTextBoxCell() { Value = binding.Destination });
                    row.Cells.Add(new DataGridViewTextBoxCell() { Value = binding.RoutingKey });
                    dgBindings.Rows.Add(row);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
            }
        }
    }
}
