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

                auth.Error += (o, e) => tcs.TrySetResult(new Auth0Result {Status = Auth0Status.Failed});
                auth.Cancelled += (o, e) => tcs.TrySetResult(new Auth0Result { Status = Auth0Status.Cancelled });

                auth.Completed += (o, e) =>
                {
                    if (!e.IsAuthenticated)
                    {
                        tcs.TrySetResult(new Auth0Result { Status = Auth0Status.Cancelled });
                        return;
                    }

                    tcs.TrySetResult(new Auth0Result { ResponseData = e.Response, Status = Auth0Status.Success});
                };

                auth.ShowUI(owner);

                return tcs.Task;

        }
    }
}
