using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Auth0.Windows
{
    public class Auth0Broker
    {
        public static Task<Auth0Result> AuthenticateAsync(IWin32Window owner, Uri startUri, Uri endUri)
        {
                var tcs = new TaskCompletionSource<Auth0Result>();

               var auth = new BrowserAuthenticationForm(startUri, endUri);

                auth.Error += (o, e) =>
                {
                    //var ex = e.Exception ?? new UnauthorizedAccessException(e.Message);
                    tcs.TrySetResult(new Auth0Result() { Status = Auth0Status.Failed });
                };

                auth.Completed += (o, e) =>
                {
                    if (!e.IsAuthenticated)
                    {
                        tcs.TrySetResult(new Auth0Result { Status = Auth0Status.Cancelled });
                    }
                    else
                    {
                        /*if (this.State != e.Account.State)
                        {
                            tcs.TrySetException(new UnauthorizedAccessException("State does not match"));
                        }
                        else*/
                        {
                            tcs.TrySetResult(new Auth0Result() { ResponseData = e.Response, Status = Auth0Status.Success});
                        }
                    }
                };

                auth.ShowUI(owner);

                return tcs.Task;

        }
    }
}
