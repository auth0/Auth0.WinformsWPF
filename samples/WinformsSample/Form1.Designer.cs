namespace WinformsSample
{
    partial class Form1
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
            this.LoginWithWidgetButton = new System.Windows.Forms.Button();
            this.UserProfileTextBox = new System.Windows.Forms.TextBox();
            this.logoutButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LoginWithWidgetButton
            // 
            this.LoginWithWidgetButton.Location = new System.Drawing.Point(12, 12);
            this.LoginWithWidgetButton.Name = "LoginWithWidgetButton";
            this.LoginWithWidgetButton.Size = new System.Drawing.Size(75, 23);
            this.LoginWithWidgetButton.TabIndex = 0;
            this.LoginWithWidgetButton.Text = "Login";
            this.LoginWithWidgetButton.UseVisualStyleBackColor = true;
            this.LoginWithWidgetButton.Click += new System.EventHandler(this.LoginWithWidgetButton_Click);
            // 
            // UserProfileTextBox
            // 
            this.UserProfileTextBox.Location = new System.Drawing.Point(13, 52);
            this.UserProfileTextBox.Multiline = true;
            this.UserProfileTextBox.Name = "UserProfileTextBox";
            this.UserProfileTextBox.Size = new System.Drawing.Size(524, 318);
            this.UserProfileTextBox.TabIndex = 1;
            // 
            // logoutButton
            // 
            this.logoutButton.Location = new System.Drawing.Point(108, 12);
            this.logoutButton.Name = "logoutButton";
            this.logoutButton.Size = new System.Drawing.Size(75, 23);
            this.logoutButton.TabIndex = 2;
            this.logoutButton.Text = "Logout";
            this.logoutButton.UseVisualStyleBackColor = true;
            this.logoutButton.Click += new System.EventHandler(this.logoutButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 382);
            this.Controls.Add(this.logoutButton);
            this.Controls.Add(this.UserProfileTextBox);
            this.Controls.Add(this.LoginWithWidgetButton);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button LoginWithWidgetButton;
        private System.Windows.Forms.TextBox UserProfileTextBox;
        private System.Windows.Forms.Button logoutButton;
    }
}

