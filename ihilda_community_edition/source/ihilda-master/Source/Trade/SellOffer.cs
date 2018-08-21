using System;
using RippleLibSharp.Transactions;

namespace IhildaWallet
{
	public class SellOffer : AutomatedOrder
	{
		public SellOffer () {

		}

		public SellOffer (TradePair tp)
		{
			this.SetFromTradePair (tp);
		}

		public void SetFromTradePair ( TradePair tp ) {
			if (tp == null) {
				// todo debug
				return;
			}

			// When you are selling, you are getting base currency
			// receiving counter

			this.taker_pays = tp.Currency_Counter.DeepCopy();
			this.taker_gets = tp.Currency_Base.DeepCopy();
		}

	}
}

