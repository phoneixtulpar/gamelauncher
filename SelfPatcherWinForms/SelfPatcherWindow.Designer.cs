namespace SelfPatcherWindowsForms
{
	partial class SelfPatcherWindow
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.closeButton = new System.Windows.Forms.Button();
            this.label = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // closeButton
            // 
            this.closeButton.Enabled = false;
            this.closeButton.Location = new System.Drawing.Point(112, 89);
            this.closeButton.Margin = new System.Windows.Forms.Padding(2);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(91, 32);
            this.closeButton.TabIndex = 0;
            this.closeButton.Text = "OK";
            this.closeButton.UseMnemonic = false;
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Visible = false;
            // 
            // label
            // 
            this.label.BackColor = System.Drawing.Color.Transparent;
            this.label.Dock = System.Windows.Forms.DockStyle.Top;
            this.label.ForeColor = System.Drawing.SystemColors.Window;
            this.label.Location = new System.Drawing.Point(26, 8);
            this.label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(258, 50);
            this.label.TabIndex = 1;
            this.label.Text = "Updating the launcher, please wait";
            this.label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(28, 61);
            this.progressBar.Margin = new System.Windows.Forms.Padding(2);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(254, 15);
            this.progressBar.TabIndex = 2;
            // 
            // SelfPatcherWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlText;
            this.ClientSize = new System.Drawing.Size(310, 141);
            this.ControlBox = false;
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.label);
            this.Controls.Add(this.closeButton);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(326, 180);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(326, 180);
            this.Name = "SelfPatcherWindow";
            this.Padding = new System.Windows.Forms.Padding(26, 8, 26, 8);
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = " Updating...";
            this.TopMost = true;
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.Label label;
		private System.Windows.Forms.ProgressBar progressBar;
	}
}

