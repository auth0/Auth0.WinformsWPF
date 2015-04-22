using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Auth0.Windows
{

    /// <summary>
    /// A simple client to Authenticate Users with Auth0.
    /// </summary>
    public class Auth0Client
    {
        private const string AuthorizeUrl =
            "https://{0}/authorize?client_id={1}&redirect_uri={2}&response_type=token&connection={3}&scope={4}";

        private const string LoginWidgetUrl =
            "https://{0}/login/?client={1}&redirect_uri={2}&response_type=token&scope={3}";

        private const string ParamQueryString = "&{0}={1}";
        private const string ResourceOwnerEndpoint = "https://{0}/oauth/ro";
        private const string DelegationEndpoint = "https://{0}/delegation";
        private const string UserInfoEndpoint = "https://{0}/userinfo?access_token={1}";
        private const string DefaultCallback = "https://{0}/mobile";

        private readonly string _domain;
        private readonly string _clientId;

        private static readonly string[] ReservedAuthParams =
        {
            "state",
            "access_token",
            "scope",
            "protocol",
            "device",
            "request_id",
            "connection_scopes",
            "nonce",
            "offline_mode"
        };


        public Auth0Client(string domain, string clientId)
        {
            _domain = domain;
            _clientId = clientId;
            DeviceIdProvider = new Device();
        }

        public Auth0User CurrentUser { get; private set; }

        public string CallbackUrl
        {
            get { return string.Format(DefaultCallback, _domain); }
        }

        /// <summary>
        /// The component used to generate the device's unique id
        /// </summary>
        public IDeviceIdProvider DeviceIdProvider { get; set; }

        /// <summary>
        /// Login a user into an Auth0 application. Attempts to do a background login, but if unsuccessful shows an embedded browser window either showing the widget or skipping it by passing a connection name
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="connection">Optional connection name to bypass the login widget</param>
        /// <param name="withRefreshToken">true to include the refresh_token in the response, false (default) otherwise.
        /// The refresh_token allows you to renew the id_token indefinitely (does not expire) unless specifically revoked.</param>
        /// <param name="scope">Optional scope, either 'openid' or 'openid profile'</param>
        /// <param name="authParams">Additional parameters to forward to Auth0 or to the IdP (like login_hint).</param>
        /// <returns>Returns a Task of Auth0User</returns>
        public async Task<Auth0User> LoginAsync(IWin32Window owner, string connection = "", bool withRefreshToken = false, string scope = "openid", IDictionary<string, string> authParams = null)
        {
            scope = IncreaseScopeWithOfflineAccess(withRefreshToken, scope);

            var tcs = new TaskCompletionSource<Auth0User>();

            var auth = await GetAuthenticatorAsync(owner, connection, scope, authParams);
        
            if (auth.Status == Auth0Status.Success)
            {
                var tokens = auth.ResponseData;
                if (tokens != null)
                {
                    SetupCurrentUser(tokens);
                    tcs.TrySetResult(CurrentUser);
                }
                else
                {
                    throw new AuthenticationErrorException();
                }
            }
            else if (auth.Status == Auth0Status.Cancelled)
            {
                throw new AuthenticationCancelException();
            }

            return CurrentUser;
        }

        /// <summary>
        ///  Log a user into an Auth0 application given an user name and password.
        /// </summary>
        /// <returns>Task that will complete when the user has finished authentication.</returns>
        /// <param name="connection" type="string">The name of the connection to use in Auth0. Connection defines an Identity Provider.</param>
        /// <param name="userName" type="string">User name.</param>
        /// <param name="password" type="string">User password.</param>
        /// <param name="withRefreshToken">true to include the refresh_token in the response, false otherwise.
        /// The refresh_token allows you to renew the id_token indefinitely (does not expire) unless specifically revoked.</param>
        /// <param name="scope">Scope.</param>
        public async Task<Auth0User> LoginAsync(string connection, string userName, string password, bool withRefreshToken = false, string scope = "openid")
        {
            scope = IncreaseScopeWithOfflineAccess(withRefreshToken, scope);

            var endpoint = string.Format(ResourceOwnerEndpoint, _domain);
            var parameters = new Dictionary<string, string>
            {
                {"client_id", _clientId},
                {"connection", connection},
                {"username", userName},
                {"password", password},
                {"grant_type", "password"},
                {"scope", scope}
            };

            if (scope.Contains("offline_access"))
            {
                var deviceId = Uri.EscapeDataString(DeviceIdProvider.GetDeviceId());
                parameters.Add("device", deviceId);
            }

            var request = new HttpClient();
            var result = await request.PostAsync(new Uri(endpoint), new FormUrlEncodedContent(parameters));

            result.EnsureSuccessStatusCode();
            var text = result.Content.ReadAsStringAsync().Result;
            var data = JObject.Parse(text).ToObject<Dictionary<string, string>>();

            if (data.ContainsKey("error"))
            {
                throw new UnauthorizedAccessException("Error authenticating: " + data["error"]);
            }
            if (data.ContainsKey("access_token"))
            {
                SetupCurrentUser(data);
            }
            else
            {
                throw new UnauthorizedAccessException("Expected access_token in access token response, but did not receive one.");
            }

            return CurrentUser;
        }

        /// <summary>
        /// Renews the idToken (JWT)
        /// </summary>
        /// <returns>The refreshed token.</returns>
        /// <param name="refreshToken">The refresh token</param>
        /// <param name="options">Additional parameters.</param>
        public async Task<JObject> RefreshToken(string refreshToken = "", Dictionary<string, string> options = null)
        {
            var emptyToken = string.IsNullOrEmpty(refreshToken);
            if (emptyToken && (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.RefreshToken)))
            {
                throw new InvalidOperationException(
                    "The current user's refresh_token could not be retrieved and no refresh_token was provided as parameter");
            }

            return await GetDelegationToken(
                api: "app",
                refreshToken: emptyToken ? CurrentUser.RefreshToken : refreshToken,
                options: options);
        }

        /// <summary>
        /// Verifies if the jwt for the current user has expired.
        /// </summary>
        /// <returns>true if the token has expired, false otherwise.</returns>
        /// <remarks>Must be logged in before invoking.</remarks>
        public bool HasTokenExpired()
        {
            if (string.IsNullOrEmpty(CurrentUser.IdToken))
            {
                throw new InvalidOperationException("You need to login first.");
            }

            return TokenValidator.HasExpired(CurrentUser.IdToken);
        }

        /// <summary>
        /// Renews the idToken (JWT)
        /// </summary>
        /// <returns>The refreshed token.</returns>
        /// <remarks>The JWT must not have expired.</remarks>
        /// <param name="options">Additional parameters.</param>
        public Task<JObject> RenewIdToken(Dictionary<string, string> options = null)
        {
            if (string.IsNullOrEmpty(CurrentUser.IdToken))
            {
                throw new InvalidOperationException("You need to login first.");
            }

            options = options ?? new Dictionary<string, string>();

            if (!options.ContainsKey("scope"))
            {
                options["scope"] = "passthrough";
            }

            return GetDelegationToken("app", CurrentUser.IdToken, options: options);
        }

        /// <summary>
        /// Get a delegation token
        /// </summary>
        /// <returns>Delegation token result.</returns>
        /// <param name="api">The type of the API to be used.</param>
        /// <param name="idToken">The string representing the JWT. Useful only if not expired.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="targetClientId">The clientId of the target application for which to obtain a delegation token.</param>
        /// <param name="options">Additional parameters.</param>
        public Task<JObject> GetDelegationToken(string api = "", string idToken = "", string refreshToken = "", string targetClientId = "", Dictionary<string, string> options = null)
        {
            if (!(string.IsNullOrEmpty(idToken) || string.IsNullOrEmpty(refreshToken)))
            {
                throw new InvalidOperationException(
                    "You must provide either the idToken parameter or the refreshToken parameter, not both.");
            }

            if (string.IsNullOrEmpty(idToken) && string.IsNullOrEmpty(refreshToken))
            {
                if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.IdToken))
                {
                    throw new InvalidOperationException(
                    "You need to login first or specify a value for idToken or refreshToken parameter.");
                }

                idToken = CurrentUser.IdToken;
            }

            options = options ?? new Dictionary<string, string>();
            options["id_token"] = idToken;
            options["api_type"] = api;
            options["refresh_token"] = refreshToken;

            var endpoint = string.Format(DelegationEndpoint, _domain);
            var parameters = new Dictionary<string, string>
            {
                {"grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"},
                {"target", targetClientId},
                {"client_id", _clientId}
            };

            // custom parameters
            foreach (var option in options)
            {
                if (!parameters.ContainsKey(option.Key))
                    parameters.Add(option.Key, option.Value);
            }

            var request = new HttpClient();
            return request.PostAsync(new Uri(endpoint), new FormUrlEncodedContent(parameters)).ContinueWith(t =>
            {
                var text = t.Result.Content.ReadAsStringAsync().Result;
                return JObject.Parse(text);
            });
        }

        /// <summary>
        /// Log a user out of a Auth0 application.
        /// </summary>
        public void Logout()
        {
            CurrentUser = null;
        }

        private static string IncreaseScopeWithOfflineAccess(bool withRefreshToken, string scope)
        {
            if (withRefreshToken && !scope.Contains("offline_access"))
            {
                scope += " offline_access";
            }

            return scope;
        }

        private void SetupCurrentUser(IDictionary<string, string> accountProperties)
        {
            var endpoint = string.Format(UserInfoEndpoint, _domain, accountProperties["access_token"]);
            var request = new HttpClient();

            request.GetAsync(new Uri(endpoint)).ContinueWith(t =>
            {
                try
                {
                    t.Result.EnsureSuccessStatusCode();
                    var profileString = t.Result.Content.ReadAsStringAsync().Result;
                    accountProperties.Add("profile", profileString);
                }
                finally
                {
                    CurrentUser = new Auth0User(accountProperties);
                }
            })
                .Wait();
        }


        private async Task<Auth0Result> GetAuthenticatorAsync(IWin32Window owner, string connection, string scope, IEnumerable<KeyValuePair<string, string>> authParams)
        {
            // Generate state to include in startUri
            var chars = new char[16];
            var rand = new Random();
            for (var i = 0; i < chars.Length; i++)
            {
                chars[i] = (char)rand.Next('a', 'z' + 1);
            }

            // Encode scope value
            scope = WebUtility.UrlEncode(scope);

            var redirectUri = CallbackUrl;
            var authorizeUri = !string.IsNullOrWhiteSpace(connection)
                ? string.Format(AuthorizeUrl, _domain, _clientId, Uri.EscapeDataString(redirectUri),
                    connection,
                    scope)
                : string.Format(LoginWidgetUrl, _domain, _clientId, Uri.EscapeDataString(redirectUri), scope);

            if (scope != null && scope.Contains("offline_access"))
            {
                var deviceId = Uri.EscapeDataString(DeviceIdProvider.GetDeviceId());
                authorizeUri += string.Format("&device={0}", deviceId);
            }

            // Add custom auth params to the request.
            if (authParams != null)
            {
                foreach (var authParam in authParams.Where(a => !ReservedAuthParams.Contains(a.Key)))
                    authorizeUri += String.Format(ParamQueryString, authParam.Key, authParam.Value);
            }

            var state = new string(chars);
            var startUri = new Uri(authorizeUri + "&state=" + state);
            var endUri = new Uri(redirectUri);

            return await Auth0Broker.AuthenticateAsync(owner, startUri, endUri);
        }

        /*
        private static bool RequireDevice(string scope)
        {
            return !String.IsNullOrEmpty(scope) && scope.Contains("offline_access");
        }*/
         
        ///// <summary>
        ///// After authenticating the result will be: https://callback#id_token=1234&access_token=12345&...
        ///// </summary>
        ///// <param name="result"></param>
        ///// <returns></returns>
        //private static Dictionary<string, string> ParseResult(string result)
        //{
        //    if (String.IsNullOrEmpty(result) || !result.Contains("#"))
        //        return null;

        //    var tokens = new Dictionary<string, string>();

        //    foreach (var tokenPart in result.Split('#')[1].Split('&'))
        //    {
        //        var tokenKeyValue = tokenPart.Split('=');
        //        tokens.Add(tokenKeyValue[0], tokenKeyValue[1]);
        //    }

        //    return tokens;
        //}
    }
}