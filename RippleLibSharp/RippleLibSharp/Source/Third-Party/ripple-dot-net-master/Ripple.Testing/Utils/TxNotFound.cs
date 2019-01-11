using System;

namespace Ripple.Testing.Utils
{
    public class TxNotFound : Exception
    {
        public TxNotFound(string txFoundWithNoMeta) : base(txFoundWithNoMeta)
        {
        }
    }
}