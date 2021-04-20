using System;
using RippleLibSharp.Transactions;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public class BuyOffer : AutomatedOrder
	{
		public BuyOffer ()
		{
		}

		public BuyOffer (TradePair tp)
		{
			SetFromTradePair(tp);
		}


		public void SetFromTradePair ( TradePair tp ) {
			#if DEBUG
			string method_sig = clsstr + nameof (SetFromTradePair) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.BuyOffer) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
			#endif
			if (tp == null) {
				// todo debug
				#if DEBUG
				if (DebugIhildaWallet.BuyOffer) {
					Logging.WriteLog (method_sig + "tp == null");
				}
				#endif
				return;
			}

			// When you are buying the base, you are giving currency
			// receiving counter

			this.taker_gets = tp.Currency_Counter.DeepCopy();
			this.taker_pays = tp.Currency_Base.DeepCopy();
		}

		#if DEBUG
		private static readonly string clsstr = nameof(BuyOffer) + DebugRippleLibSharp.colon;
		#endif
	}
}

