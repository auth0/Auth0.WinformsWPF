namespace Auth0.Windows
{
    partial class BrowserAuthenticationForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.ToggleFullScreen = new System.Windows.Forms.Button();
            this.LabelStatus = new System.Windows.Forms.Label();
            this.BrowserPanel = new System.Windows.Forms.Panel();
            this.browser = new System.Windows.Forms.WebBrowser();
            this.CancelLoginButton = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.BrowserPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.ToggleFullScreen);
            this.panel1.Controls.Add(this.LabelStatus);
            this.panel1.Controls.Add(this.BrowserPanel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.ForeColor = System.Drawing.Color.Silver;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(484, 600);
            this.panel1.TabIndex = 0;
            // 
            // ToggleFullScreen
            // 
            this.ToggleFullScreen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ToggleFullScreen.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ToggleFullScreen.ForeColor = System.Drawing.Color.Black;
            this.ToggleFullScreen.Location = new System.Drawing.Point(380, 7);
            this.ToggleFullScreen.Name = "ToggleFullScreen";
            this.ToggleFullScreen.Size = new System.Drawing.Size(97, 21);
            this.ToggleFullScreen.TabIndex = 2;
            this.ToggleFullScreen.Text = "Full Screen";
            this.ToggleFullScreen.UseVisualStyleBackColor = true;
            this.ToggleFullScreen.Click += new System.EventHandler(this.ToggleFullScreen_Click);
            // 
            // LabelStatus
            // 
            this.LabelStatus.AutoSize = true;
            this.LabelStatus.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelStatus.ForeColor = System.Drawing.Color.Black;
            this.LabelStatus.Location = new System.Drawing.Point(68, 10);
            this.LabelStatus.Name = "LabelStatus";
            this.LabelStatus.Size = new System.Drawing.Size(58, 13);
            this.LabelStatus.TabIndex = 3;
            this.LabelStatus.Text = "Loading...";
            // 
            // BrowserPanel
            // 
            this.BrowserPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowserPanel.AutoSize = true;
            this.BrowserPanel.Controls.Add(this.browser);
            this.BrowserPanel.Location = new System.Drawing.Point(0, 34);
            this.BrowserPanel.Name = "BrowserPanel";
            this.BrowserPanel.Size = new System.Drawing.Size(484, 565);
            this.BrowserPanel.TabIndex = 2;
            // 
            // browser
            // 
            this.browser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browser.Location = new System.Drawing.Point(0, 0);
            this.browser.MinimumSize = new System.Drawing.Size(20, 20);
            this.browser.Name = "browser";
            this.browser.Size = new System.Drawing.Size(484, 565);
            this.browser.TabIndex = 0;
            this.browser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.browser_DocumentCompleted);
            this.browser.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.browser_Navigating);
            // 
            // CancelLoginButton
            // 
            this.CancelLoginButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelLoginButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CancelLoginButton.Location = new System.Drawing.Point(6, 7);
            this.CancelLoginButton.Name = "CancelLoginButton";
            this.CancelLoginButton.Size = new System.Drawing.Size(57, 21);
            this.CancelLoginButton.TabIndex = 1;
            this.CancelLoginButton.Text = "Cancel";
            this.CancelLoginButton.UseVisualStyleBackColor = true;
            this.CancelLoginButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // BrowserAuthenticationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelLoginButton;
            this.ClientSize = new System.Drawing.Size(484, 600);
            this.Controls.Add(this.CancelLoginButton);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimizeBox = false;
            this.Name = "BrowserAuthenticationForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.BrowserPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.WebBrowser browser;
        private System.Windows.Forms.Button CancelLoginButton;
        private System.Windows.Forms.Panel BrowserPanel;
        private System.Windows.Forms.Label LabelStatus;
        private System.Windows.Forms.Button ToggleFullScreen;

    }
}