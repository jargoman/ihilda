using System;

namespace Ripple.Testing.Utils
{
    public class UnexpectedEngineResultException : Exception
    {
        public UnexpectedEngineResultException(string s) : base(s)
        {
        }

        public UnexpectedEngineResultException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}