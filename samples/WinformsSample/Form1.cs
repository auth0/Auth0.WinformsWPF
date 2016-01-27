using Auth0.Windows;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinformsSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private Auth0Client auth0 = new Auth0Client(ConfigurationManager.AppSettings["auth0:Domain"],
                                                    ConfigurationManager.AppSettings["auth0:ClientId"]);

        private void LoginWithWidgetButton_Click(object sender, EventArgs e)
        {
            
            auth0.LoginAsync(this).ContinueWith(t =>
            {
                if (t.IsFaulted)
                    this.UserProfileTextBox.Text = t.Exception.InnerException.ToString();
                else
                    this.UserProfileTextBox.Text = t.Result.Profile.ToString();
            },
            TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void logoutButton_Click(object sender, EventArgs e)
        {
            auth0.Logout();
            MessageBox.Show("Logout was succesful");
        }
    }
}
