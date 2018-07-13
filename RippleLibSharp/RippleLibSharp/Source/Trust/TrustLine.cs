using System;
using RippleLibSharp.Keys;
using RippleLibSharp.Util;

namespace RippleLibSharp.Trust
{
	public class TrustLine
	{


		public TrustLine ()
		{

			this.account = null;
			this.balance = null;
			this.currency = null;
			this.limit = null;
			this.limit_peer = null;

			this.quality_in = 0L;
			this.quality_out = 0L;
		}


		public TrustLine (String account, String balance, String currency, String limit, String limit_peer, ulong quality_in, ulong quality_out)
		{
			this.account = (RippleAddress)account;
			this.balance = balance;
			this.currency = currency;
			this.limit = limit;
			this.limit_peer = limit_peer;

			this.quality_in = quality_in;
			this.quality_out = quality_out;
		}

		public TrustLine (RippleAddress account, String balance, String currency, String limit, String limit_peer, ulong quality_in, ulong quality_out)
		{
			this.account = account;
			this.balance = balance;
			this.currency = currency;
			this.limit = limit;
			this.limit_peer = limit_peer;

			this.quality_in = quality_in;
			this.quality_out = quality_out;
		}





		public decimal GetBalanceAsDecimal ()
		{

#if DEBUG
			String method_sig = clsstr + nameof (GetBalanceAsDecimal) +  DebugRippleLibSharp.both_parentheses;
#endif

			try {
				//return Convert.ToDouble(this.balance);
				if (this.balance == null) {
#if DEBUG
					if (DebugRippleLibSharp.TrustLine) {
						Logging.WriteLog (method_sig + "this.balance  == null");
					}
#endif
					return 0;
				}


				decimal d = Convert.ToDecimal (this.balance);
#if DEBUG
				if (DebugRippleLibSharp.TrustLine) {
					Logging.WriteLog (method_sig + " returning " + d.ToString ());
				}
#endif
				return d;
			} catch (Exception e) {
#if DEBUG
				Logging.WriteLog (method_sig + "Error in TrustLine, Exception thrown converting string to double. String balance == " + this.balance ?? "null");
				Logging.WriteLog (e.Message);
				Logging.ReportException (method_sig, e);
#endif
				return 0;
			}
		}

#pragma warning disable IDE1006 // Naming Styles
		public string account { get; set; }


		public String balance { get; set; }

		public String currency { get; set; }

		public String limit { get; set; }

		public String limit_peer { get; set; }

		public ulong quality_in { get; set; }

		public ulong quality_out { get; set; }

		public bool? no_ripple { get; set; }
		public bool? no_ripple_peer { get; set; }
#pragma warning restore IDE1006 // Naming Styles
		/*
		public int quality_in { get; set; }
		public int quality_out { get; set; }
		*/

#if DEBUG
		private static readonly String clsstr = nameof (TrustLine) + DebugRippleLibSharp.colon;
#endif
	}
}

