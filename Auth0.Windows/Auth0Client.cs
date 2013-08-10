using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Windows.Forms;

namespace Auth0.Windows
{
    /// <summary>
    /// A simple client to Authenticate Users with Auth0.
    /// </summary>
    public partial class Auth0Client
    {
        private const string AuthorizeUrl = "https://{0}/authorize?client_id={1}&scope={4}&redirect_uri={2}&response_type=token&connection={3}";
        private const string LoginWidgetUrl = "https://{0}/login/?client={1}&scope={3}&redirect_uri={2}&response_type=token";
        private const string ResourceOwnerEndpoint = "https://{0}/oauth/ro";
        private const string DefaultCallback = "https://{0}/mobile";

        private readonly string subDomain;
        private readonly string clientId;
        private readonly string clientSecret;

        internal string State { get; set; }

        public Auth0Client(string subDomain, string clientId, string clientSecret)
        {
            this.subDomain = subDomain.Contains('.') ? subDomain : subDomain + ".auth0.com";
            this.clientId = clientId;
            this.clientSecret = clientSecret;
        }

        public Auth0User CurrentUser { get; private set; }

        public string CallbackUrl
        {
            get
            {
                return string.Format(DefaultCallback, this.subDomain);
            }
        }

        /// <summary>
        /// Login a user into an Auth0 application by showing an embedded browser window either showing the widget or skipping it by passing a connection name
        /// </summary>
        /// <param name="owner">The owner window</param>
        /// <param name="connection">Optional connection name to bypass the login widget</param>
        /// <param name="scope">Optional. Scope indicating what attributes are needed. "openid" to just get the user_id or "openid profile" to get back everything.
        /// <remarks>When using openid profile if the user has many attributes the token might get big and the embedded browser (Internet Explorer) won't be able to parse a large URL</remarks>
        /// </param>
        /// <returns>Returns a Task of Auth0User</returns>
        public Task<Auth0User> LoginAsync(IWin32Window owner, string connection = "", string scope = "openid profile")
        {
            var tcs = new TaskCompletionSource<Auth0User>();
            var auth = this.GetAuthenticator(connection, scope);

            auth.Error += (o, e) =>
            {
                var ex = e.Exception ?? new UnauthorizedAccessException(e.Message);
                tcs.TrySetException(ex);
            };

            auth.Completed += (o, e) =>
            {
                if (!e.IsAuthenticated)
                {
                    tcs.TrySetCanceled();
                }
                else
                {
                    if (this.State != e.Account.State)
                    {
                        tcs.TrySetException(new UnauthorizedAccessException("State does not match"));
                    }
                    else
                    {
                        this.CurrentUser = e.Account;
                        tcs.TrySetResult(this.CurrentUser);
                    }
                }
            };

            auth.ShowUI(owner);

            return tcs.Task;
        }

        /// <summary>
        ///  Log a user into an Auth0 application given an user name and password.
        /// </summary>
        /// <returns>Task that will complete when the user has finished authentication.</returns>
        /// <param name="connection" type="string">The name of the connection to use in Auth0. Connection defines an Identity Provider.</param>
        /// <param name="userName" type="string">User name.</param>
        /// <param name="password type="string"">User password.</param>
        /// <param name="scope">Optional. Scope indicating what attributes are needed. "openid" to just get the user id or "openid profile" to get back everything.
        /// </param>
        public Task<Auth0User> LoginAsync(string connection, string userName, string password, string scope = "openid profile")
        {
            var endpoint = string.Format(ResourceOwnerEndpoint, this.subDomain);
            var parameters = new Dictionary<string, string> 
			{
				{ "client_id", this.clientId },
				{ "client_secret", this.clientSecret },
				{ "connection", connection },
				{ "username", userName },
				{ "password", password },
				{ "grant_type", "password" },
				{ "scope", scope }
			};

            var request = new HttpClient();
            return request.PostAsync(new Uri(endpoint), new FormUrlEncodedContent(parameters)).ContinueWith(t =>
            {
                try
                {
                    t.Result.EnsureSuccessStatusCode();
                    var text = t.Result.Content.ReadAsStringAsync().Result;
                    var data = JObject.Parse(text).ToObject<Dictionary<string, string>>();

                    if (data.ContainsKey("error"))
                    {
                        throw new UnauthorizedAccessException("Error authenticating: " + data["error"]);
                    }
                    else if (data.ContainsKey("access_token"))
                    {
                        this.SetupCurrentUser(data);
                    }
                    else
                    {
                        throw new UnauthorizedAccessException("Expected access_token in access token response, but did not receive one.");
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                return this.CurrentUser;
            });
        }

        /// <summary>
        /// Log a user out of a Auth0 application.
        /// </summary>
        public void Logout()
        {
            this.CurrentUser = null;
        }

        private void SetupCurrentUser(IDictionary<string, string> accountProperties)
        {
            this.CurrentUser = new Auth0User(accountProperties);
        }

        private BrowserAuthenticationForm GetAuthenticator(string connection, string scope)
        {
            // Generate state to include in startUri
            var chars = new char[16];
            var rand = new Random();
            for (var i = 0; i < chars.Length; i++)
            {
                chars[i] = (char)rand.Next((int)'a', (int)'z' + 1);
            }

            var redirectUri = this.CallbackUrl;
            var authorizeUri = !string.IsNullOrWhiteSpace(connection) ?
                string.Format(AuthorizeUrl, subDomain, clientId, Uri.EscapeDataString(redirectUri), connection, Uri.EscapeDataString(scope)) :
                string.Format(LoginWidgetUrl, subDomain, clientId, Uri.EscapeDataString(redirectUri), Uri.EscapeDataString(scope));

            this.State = new string(chars);
            var startUri = new Uri(authorizeUri + "&state=" + this.State);
            var endUri = new Uri(redirectUri);

            var auth = new BrowserAuthenticationForm(startUri, endUri);

            return auth;
        }
    }
}