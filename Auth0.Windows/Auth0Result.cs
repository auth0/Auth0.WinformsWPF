using System.Collections.Generic;

namespace Auth0.Windows
{
    public class Auth0Result
    {
        public Auth0Status Status { get; set; }
        public IDictionary<string, string> ResponseData { get; set; }
    }
}
