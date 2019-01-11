using System;
using System.Threading;

namespace Ripple.Testing.Utils
{
    public class NonTestThreadException : Exception
    {
        public NonTestThreadException(Exception innerException) :
            base("Exception in non test thread", innerException)
        {
        }

        public static void ThrowInOtherThread(Exception wrappedException)
        {
            var e = new NonTestThreadException(wrappedException);
            new Thread(() => { throw e; }).Start();
        }
    }
}