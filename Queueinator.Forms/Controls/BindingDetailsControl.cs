using Queueinator.Domain.RabbitMq;
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
    public partial class BindingDetailsControl : UserControl
    {
        private readonly ExchangeTree _exchange;
        private readonly List<ExchangeBinding> _bindings;

        public BindingDetailsControl(ExchangeTree exchange, List<ExchangeBinding> bindings)
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;

            _exchange = exchange;
            _bindings = bindings;

            dgBindings.SelectionChanged += On_Change_Row_selection;

            FillDataSource();
        }

        public void FillDataSource()
        {
            dgBindings.Columns.Add("Source", "Source");
            dgBindings.Columns.Add("Destination", "Destination");
            dgBindings.Columns.Add("RoutingKey", "RoutingKey");

            foreach (DataGridViewColumn column in dgBindings.Columns)
            {
                column.Width = 170;
            }

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

        private void On_Change_Row_selection(object sender, EventArgs e)
        {
            if (dgBindings.SelectedCells.Count == 0) return;

            //var selectedRow = dgBindings.Rows[];

            var rowIndex = dgBindings.SelectedCells[0].RowIndex;

            if (_bindings.Count <= rowIndex) return;

            var binding = _bindings[rowIndex];

            txtDetails.Text = JsonSerializer.Serialize(binding, new JsonSerializerOptions
            {
                WriteIndented = true,
            });
        }
    }
}
