using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Auth0.Windows
{
    public partial class BrowserAuthenticationForm : Form
    {
        public Uri StartUrl { get; set; }
        public Uri EndUrl { get; set; }

        static readonly char[] AmpersandChars = { '&' };
        static readonly char[] EqualsChars = { '=' };

        public event EventHandler<AuthenticatorCompletedEventArgs> Completed;
        public event EventHandler<AuthenticatorErrorEventArgs> Error;

        private readonly WebBrowser _loadingBrowser = new WebBrowser();  
  
        public BrowserAuthenticationForm(Uri startUrl, Uri endUrl)
        {
            InitializeComponent();
            browser.Hide();
            _loadingBrowser.Show();
            _loadingBrowser.DocumentText = "<!DOCTYPE html><html lang=en><head><meta charset=utf-8><style type=text/css>html,body{overflow: hidden;background-color:#000;color:#FFF;margin:0}#loading{width:100%;height:100%;background:url(\"https://s3.amazonaws.com/assets.auth0.com/loading.gif\") no-repeat center center #fff;position:fixed;opacity:.9}</style></head><body><div id=loading></div></body></html>";
            _loadingBrowser.Dock = DockStyle.Fill;
            BrowserPanel.Controls.Add(_loadingBrowser);

            StartUrl = startUrl;
            EndUrl = endUrl;
        }

        private void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (BrowserPanel.Controls.ContainsKey("Loading") && e.Url.PathAndQuery.Contains("rpc.html"))
            {
                _loadingBrowser.Hide();
                browser.Show();                
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
                Close();
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
                
                Close();
            }
        }

        protected virtual void OnCompleted(IDictionary<string, string> fragment)
        {
            if (Completed != null)
                Completed(this, new AuthenticatorCompletedEventArgs(fragment));
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
            return url.Host == EndUrl.Host && url.LocalPath == EndUrl.LocalPath;
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
            browser.Navigate(StartUrl.AbsoluteUri);
            ShowDialog(owner);
        }

        private void UpdateStatus(string message)
        {
            if (message == "")
            {
                _loadingBrowser.Hide();
                browser.Show();
            }
            else
            {
                _loadingBrowser.Show();
                browser.Hide();
            }
            LabelStatus.Text = message;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            OnError("The operation was canceled by the user.");
            Close();
        }

        private const int cGrip = 16;      // Grip size
        private const int cCaption = 25;   // Caption bar height;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84)
            {  // Trap WM_NCHITTEST
                var pos = new Point(m.LParam.ToInt32() & 0xffff, m.LParam.ToInt32() >> 16);
                pos = PointToClient(pos);
                if (pos.Y < cCaption)
                {
                    m.Result = (IntPtr)2;  // HTCAPTION
                    return;
                }
                if (pos.X >= ClientSize.Width - cGrip && pos.Y >= ClientSize.Height - cGrip)
                {
                    m.Result = (IntPtr)17; // HTBOTTOMRIGHT
                    return;
                }
            }
            base.WndProc(ref m);
        }

        public class AuthenticatorCompletedEventArgs : EventArgs
        {
            public bool IsAuthenticated { get { return Response.Count > 0; } }
            public IDictionary<string, string> Response { get; private set; }

            public AuthenticatorCompletedEventArgs(IDictionary<string, string> response)
            {
                Response = response;
            }
        }

        public class AuthenticatorErrorEventArgs : EventArgs
        {
            public string Message { get; private set; }

            public Exception Exception { get; private set; }

            public AuthenticatorErrorEventArgs(string message)
            {
                Message = message;
            }
    
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
            WindowState = WindowState == FormWindowState.Normal ? FormWindowState.Maximized : FormWindowState.Normal;
            ToggleFullScreen.Text = WindowState == FormWindowState.Maximized ? "Exit Full Screen" : "Full Screen";
        }
    }
}
