using Queueinator.Domain.RabbitMq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Queueinator.Forms
{
    public partial class Form1 : Form
    {
        private NewServerForm _newServerForm;

        public Form1(NewServerForm newServerForm)
        {
            InitializeComponent();

            tsAddServer.Click += tsAddServer_Click;
            _newServerForm = newServerForm;
        }

        private void tsAddServer_Click(object sender, EventArgs e)
        {
            ConnectToAServer(_newServerForm);
        }

        Dictionary<String, VirtualHost> _virtualHosts = new Dictionary<string, VirtualHost>();

        private void ConnectToAServer(NewServerForm newServerPopup)
        {
            var dialogResult = newServerPopup.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                var virtualHost = newServerPopup.VirtualHost;

                _virtualHosts.Add(virtualHost.Name, virtualHost);

                treeViewQueues.Nodes.Add(virtualHost.Name);

                MessageBox.Show("Connected");
            }
            else if(dialogResult == DialogResult.Retry)
            {
                ConnectToAServer(newServerPopup);
            }
            else if(dialogResult == DialogResult.Cancel)
            {

            }
        }
    }
}
