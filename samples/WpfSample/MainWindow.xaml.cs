using Auth0.Windows;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace WpfSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Auth0Client auth0 = new Auth0Client(ConfigurationManager.AppSettings["auth0:Domain"], 
                                                    ConfigurationManager.AppSettings["auth0:ClientId"]);

        private void LoginWithWidget_Click(object sender, RoutedEventArgs e)
        {
            auth0.LoginAsync(new WindowWrapper(new WindowInteropHelper(this).Handle)).ContinueWith(t =>
            {
                if (t.IsFaulted)
                    this.textBox1.Text = t.Exception.InnerException.ToString();
                else
                    this.textBox1.Text = t.Result.Profile.ToString();
            },
            TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void LoginWithConnection_Click(object sender, RoutedEventArgs e)
        {
            auth0.LoginAsync(new WindowWrapper(new WindowInteropHelper(this).Handle), connection: ConnectionTextBox.Text).ContinueWith(t =>
            {
                if (t.IsFaulted)
                    this.textBox1.Text = t.Exception.InnerException.ToString();
                else
                    this.textBox1.Text = t.Result.Profile.ToString();
            },
            TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void LoginUserPassButton_Click(object sender, RoutedEventArgs e)
        {
            auth0.LoginAsync(connection: DbConnectionNameTextBox.Text, userName: UserTextBox.Text, password: PasswordTextBox.Password).ContinueWith(t =>
            {
                if (t.IsFaulted) 
                    this.textBox1.Text = t.Exception.InnerException.ToString();
                else
                    this.textBox1.Text = t.Result.Profile.ToString();
            },
            TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            auth0.Logout();

            this.textBox1.Text = "Logged out...";
        }
    }
}
