
namespace Queueinator.Forms.Controls
{
    partial class ExchangeControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbExchangeDetails = new System.Windows.Forms.TabControl();
            this.tbSource = new System.Windows.Forms.TabPage();
            this.tbDestination = new System.Windows.Forms.TabPage();
            this.tbExchangeDetails.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbExchangeDetails
            // 
            this.tbExchangeDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbExchangeDetails.Controls.Add(this.tbSource);
            this.tbExchangeDetails.Controls.Add(this.tbDestination);
            this.tbExchangeDetails.Location = new System.Drawing.Point(0, 397);
            this.tbExchangeDetails.Name = "tbExchangeDetails";
            this.tbExchangeDetails.SelectedIndex = 0;
            this.tbExchangeDetails.Size = new System.Drawing.Size(1095, 391);
            this.tbExchangeDetails.TabIndex = 3;
            // 
            // tbSource
            // 
            this.tbSource.Location = new System.Drawing.Point(4, 24);
            this.tbSource.Name = "tbSource";
            this.tbSource.Padding = new System.Windows.Forms.Padding(3);
            this.tbSource.Size = new System.Drawing.Size(1087, 363);
            this.tbSource.TabIndex = 0;
            this.tbSource.Text = "tabPage1";
            this.tbSource.UseVisualStyleBackColor = true;
            // 
            // tbDestination
            // 
            this.tbDestination.Location = new System.Drawing.Point(4, 24);
            this.tbDestination.Name = "tbDestination";
            this.tbDestination.Padding = new System.Windows.Forms.Padding(3);
            this.tbDestination.Size = new System.Drawing.Size(1087, 363);
            this.tbDestination.TabIndex = 1;
            this.tbDestination.Text = "tabPage2";
            this.tbDestination.UseVisualStyleBackColor = true;
            // 
            // ExchangeControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tbExchangeDetails);
            this.Name = "ExchangeControl";
            this.Size = new System.Drawing.Size(1095, 788);
            this.tbExchangeDetails.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tbExchangeDetails;
        private System.Windows.Forms.TabPage tbSource;
        private System.Windows.Forms.TabPage tbDestination;
    }
}
