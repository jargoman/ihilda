using System;
using Ripple.Core.Types;
using Ripple.TxSigning;

namespace Ripple.Testing.Utils
{
    public class TxConfigurator
    {
        public Action<StObject> PreSubmit;
        public Action<SignedTx> AfterSigning;
        public static implicit operator TxConfigurator(Action<StObject> a)
        {
            return new TxConfigurator { PreSubmit = a };
        }
        public static implicit operator TxConfigurator(Action<SignedTx> a)
        {
            return new TxConfigurator { AfterSigning = a };
        }
    }
}