using System;

namespace Auth0.Windows
{
    public class AuthenticationErrorException : Exception
    {
        public AuthenticationErrorException()
        {
        }

        public AuthenticationErrorException(string message)
            : base(message)
        {
        }
    }
}