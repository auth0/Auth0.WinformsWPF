using System;

namespace Auth0.Windows
{
    public class AuthenticationCancelException : Exception
    {
        public AuthenticationCancelException()
        {
        }

        public AuthenticationCancelException(string message)
            : base(message)
        {
        }
    }
}