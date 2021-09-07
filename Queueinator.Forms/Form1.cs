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
            _newServerForm = newServerForm;
        }

        private void tsAddServer_Click(object sender, EventArgs e)
        {
            ConnectToAServer(_newServerForm);
        }

        private static void ConnectToAServer(NewServerForm newServerPopup)
        {
            var dialogResult = newServerPopup.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
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
