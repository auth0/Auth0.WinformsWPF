using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Auth0.Windows
{
    public partial class BrowserAuthenticationForm : Form
    {
        public Uri StartUrl { get; set; }
        public Uri EndUrl { get; set; }

        static char[] AmpersandChars = new char[] { '&' };
        static char[] EqualsChars = new char[] { '=' };

        public event EventHandler<AuthenticatorCompletedEventArgs> Completed;
        public event EventHandler<AuthenticatorErrorEventArgs> Error;

        private WebBrowser loadingBrowser = new WebBrowser();  
  
        public BrowserAuthenticationForm(Uri startUrl, Uri endUrl)
        {
            InitializeComponent();
            this.browser.Hide();
            this.loadingBrowser.Show();
            this.loadingBrowser.DocumentText = "<!DOCTYPE html><html lang=en><head><meta charset=utf-8><style type=text/css>html,body{overflow: hidden;background-color:#000;color:#FFF;margin:0}#loading{width:100%;height:100%;background:url(\"https://s3.amazonaws.com/assets.auth0.com/loading.gif\") no-repeat center center #fff;position:fixed;opacity:.9}</style></head><body><div id=loading></div></body></html>";
            this.loadingBrowser.Dock = DockStyle.Fill;
            this.BrowserPanel.Controls.Add(this.loadingBrowser);

            this.StartUrl = startUrl;
            this.EndUrl = endUrl;
        }

        private void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (this.BrowserPanel.Controls.ContainsKey("Loading") && e.Url.PathAndQuery.Contains("rpc.html"))
            {
                this.loadingBrowser.Hide();
                this.browser.Show();                
            }

            UpdateStatus("");
            var url = e.Url;
            var fragment = FormDecode(url.Fragment);
            var all = new Dictionary<string, string>(FormDecode(url.Query));
            foreach (var kv in fragment)
                all[kv.Key] = kv.Value;

            //
            // Check for errors
            //
            if (all.ContainsKey("error"))
            {
                var description = all["error"];
                if (all.ContainsKey("error_description"))
                {
                    description = all["error_description"];
                }
                OnError(description);
                this.Close();
                return;
            }

            //
            // Watch for the redirect
            //
            if (UrlMatchesRedirect(url))
            {
                if (fragment == null || fragment.Keys.Count == 1)
                {
                    OnError("The response is too large and Internet Explorer does not support it. Try using scope=openid instead or remove attributes with rules.");
                }
                else
                {
                    OnCompleted(fragment);
                }
                
                this.Close();
            }
        }

        protected virtual void OnCompleted(IDictionary<string, string> fragment)
        {
            if (Completed != null)
                Completed(this, new AuthenticatorCompletedEventArgs(new Auth0User(fragment)));
        }

        protected virtual void OnError(string error)
        {
            if (Error != null)
                Error(this, new AuthenticatorErrorEventArgs(error));
        }

        protected virtual void OnError(Exception ex)
        {
            if (Error != null)
                Error(this, new AuthenticatorErrorEventArgs(ex));
        }

        private bool UrlMatchesRedirect(Uri url)
        {
            return url.Host == this.EndUrl.Host && url.LocalPath == this.EndUrl.LocalPath;
        }

        private static IDictionary<string, string> FormDecode(string encodedString)
        {
            var inputs = new Dictionary<string, string>();

            if (encodedString.StartsWith("?") || encodedString.StartsWith("#"))
            {
                encodedString = encodedString.Substring(1);
            }

            var parts = encodedString.Split(AmpersandChars);
            foreach (var p in parts)
            {
                var kv = p.Split(EqualsChars);
                var k = Uri.UnescapeDataString(kv[0]);
                var v = kv.Length > 1 ? Uri.UnescapeDataString(kv[1]) : "";
                inputs[k] = v;
            }

            return inputs;
        }

        public void ShowUI(IWin32Window owner)
        {
            // Always clear the cache to ensure previous logged in user is not in browser cache.
            // If we do not clear the cache, and a user has signed in before, then that user will display
            WebBrowserHelpers.ClearCache();

            this.browser.Navigate(this.StartUrl.AbsoluteUri);
            this.ShowDialog(owner);
        }

        private void UpdateStatus(string message)
        {
            if (message == "")
            {
                this.loadingBrowser.Hide();
                this.browser.Show();
            }
            else
            {
                this.loadingBrowser.Show();
                this.browser.Hide();
            }
            this.LabelStatus.Text = message;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.OnError("The operation was canceled by the user.");
            this.Close();
        }

        private const int cGrip = 16;      // Grip size
        private const int cCaption = 25;   // Caption bar height;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84)
            {  // Trap WM_NCHITTEST
                Point pos = new Point(m.LParam.ToInt32() & 0xffff, m.LParam.ToInt32() >> 16);
                pos = this.PointToClient(pos);
                if (pos.Y < cCaption)
                {
                    m.Result = (IntPtr)2;  // HTCAPTION
                    return;
                }
                if (pos.X >= this.ClientSize.Width - cGrip && pos.Y >= this.ClientSize.Height - cGrip)
                {
                    m.Result = (IntPtr)17; // HTBOTTOMRIGHT
                    return;
                }
            }
            base.WndProc(ref m);
        }

        public class AuthenticatorCompletedEventArgs : EventArgs
        {
            /// <summary>
            /// Whether the authentication succeeded and there is a valid <see cref="Account"/>.
            /// </summary>
            /// <value>
            /// <see langword="true"/> if the user is authenticated; otherwise, <see langword="false"/>.
            /// </value>
            public bool IsAuthenticated { get { return Account != null; } }

            /// <summary>
            /// Gets the account created that represents this authentication.
            /// </summary>
            /// <value>
            /// The account.
            /// </value>
            public Auth0User Account { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Xamarin.Auth.AuthenticatorCompletedEventArgs"/> class.
            /// </summary>
            /// <param name='account'>
            /// The account created or <see langword="null"/> if authentication failed or was canceled.
            /// </param>
            public AuthenticatorCompletedEventArgs(Auth0User account)
            {
                Account = account;
            }
        }

        public class AuthenticatorErrorEventArgs : EventArgs
        {
            /// <summary>
            /// Gets a message describing the error.
            /// </summary>
            /// <value>
            /// The message.
            /// </value>
            public string Message { get; private set; }

            /// <summary>
            /// Gets the exception that signaled the error if there was one.
            /// </summary>
            /// <value>
            /// The exception or <see langword="null"/>.
            /// </value>
            public Exception Exception { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Xamarin.Auth.AuthenticatorErrorEventArgs"/> class
            /// with a message but no exception.
            /// </summary>
            /// <param name='message'>
            /// A message describing the error.
            /// </param>
            public AuthenticatorErrorEventArgs(string message)
            {
                Message = message;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Xamarin.Auth.AuthenticatorErrorEventArgs"/> class with an exception.
            /// </summary>
            /// <param name='exception'>
            /// The exception signaling the error. The message of this object is retrieved from this exception or
            /// its inner exceptions.
            /// </param>
            public AuthenticatorErrorEventArgs(Exception exception)
            {
                Message = exception.ToString();
                Exception = exception;
            }
        }

        private void browser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            UpdateStatus("Loading...");
        }

        private void ToggleFullScreen_Click(object sender, EventArgs e)
        {
            this.WindowState = this.WindowState == FormWindowState.Normal ? FormWindowState.Maximized : FormWindowState.Normal;
            this.ToggleFullScreen.Text = this.WindowState == FormWindowState.Maximized ? "Exit Full Screen" : "Full Screen";
        }
    }
}
