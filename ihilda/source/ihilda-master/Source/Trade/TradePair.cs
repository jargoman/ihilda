using System;

using System.Text;
using System.Threading.Tasks;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;
using RippleLibSharp.Util;
using RippleLibSharp.Network;
namespace IhildaWallet
{
	public class TradePair
	{
		public TradePair ()
		{
			

		}

		public TradePair (RippleCurrency bass, RippleCurrency counter) {
			this.Currency_Base = bass;
			this.Currency_Counter = counter;
		}



		public TradePair (String bass, String counter) 
		{
			if (RippleCurrency.NativeCurrency.Equals (bass)) {
				Currency_Base = new RippleCurrency (0m);
			} else {
				Currency_Base = new RippleCurrency (0m, null, bass) {
					currency = bass
				};
			}

			if (RippleCurrency.NativeCurrency.Equals (counter)) {
				Currency_Counter = new RippleCurrency (0m);
			} else {
				Currency_Counter = new RippleCurrency (0m, null, counter) {
					// probably unneeded
					currency = counter
				};
			}
		}

		public RippleCurrency Currency_Base { get; set; }
		public RippleCurrency Currency_Counter {get; set; }

		public TradePair DeepCopy () {
			TradePair tp = new TradePair {
				Currency_Base = this.Currency_Base.DeepCopy (),
				Currency_Counter = this.Currency_Counter.DeepCopy ()
			};

			return tp;
		}

		public void UpdateBalances (string account, NetworkInterface networkInterface)
		{

			this.Currency_Base.UpdateBalance (account, networkInterface);
			this.Currency_Counter.UpdateBalance (account, networkInterface);

		}

		public String ToHumanString () {


			return string.Format("{0}/{1}", Currency_Base?.currency ?? "",  Currency_Counter?.currency ?? "");
		}

		public bool HasRequirements () {
			#if DEBUG
			string method_sig = clsstr + nameof (HasRequirements) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.TradePair) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
			#endif
			if (this.Currency_Base == null) {
				#if DEBUG
				if (DebugIhildaWallet.TradePair) {
					Logging.WriteLog (method_sig + "tradePair.currency_base == null, returning false\n");
				}
				#endif

				return false;
			}
			if (this.Currency_Base.currency == null) {
				#if DEBUG
				if (DebugIhildaWallet.TradePair) {
					Logging.WriteLog (method_sig + "tradePair.currency_base.currency == null, returning false\n");
				}
				#endif
				return false;
			}
			if (this.Currency_Counter == null) {
				#if DEBUG
				if (DebugIhildaWallet.TradePair) {
					Logging.WriteLog (method_sig + "tradePair.currency_counter == null, returning false\n");
				}
				#endif
				return false;
			}
			if (this.Currency_Counter.currency == null) {
				#if DEBUG
				if (DebugIhildaWallet.TradePair) {
					Logging.WriteLog (method_sig + "tradePair.currency_counter.currency == null, returning false\n");
				}
				#endif
				return false;
			}

			#if DEBUG
			if (DebugIhildaWallet.TradePair) {
				Logging.WriteLog (method_sig + "all requirements met returning true\n");
			}
			#endif
			return true;
		}

		public override string ToString ()
		{
			//  TODO separate Debug code from release
			#if DEBUG
			return 
			string.Format ("[TradePair: currency_base={0}, currency_counter={1}]", DebugIhildaWallet.ToAssertString(Currency_Base), DebugIhildaWallet.ToAssertString(Currency_Counter));
			#else
			return "tradePair";
			#endif

		}

		public string DetermineKey  () {
			// we could use the issuer strings or use a hash instead

			return DetermineDictKeyFromCurrencies (this.Currency_Base, this.Currency_Counter);
		}

		public static string DetermineDictKeyFromCurrencies (RippleCurrency cbase, RippleCurrency ccounter) {
			StringBuilder sb = new StringBuilder ();
			string b = cbase?.ToIssuerString () ?? "";
			sb.Append (b);

			sb.Append ("/");

			string c = ccounter == null ? "" : ccounter.ToIssuerString ();
			sb.Append (c);
			return sb.ToString();
		}

#if DEBUG
		private const string clsstr = nameof (TradePair) + DebugRippleLibSharp.colon;
#endif

		private static bool allowExtraSymbols = true;
		public static TradePair FromString ( String parseme)
		{
			#if DEBUG
			String method_sig = clsstr + nameof (FromString) + DebugRippleLibSharp.left_parentheses + nameof (parseme) + DebugRippleLibSharp.equals + DebugIhildaWallet.ToAssertString(parseme) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.TradePair) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
			#endif

			String[] pair = allowExtraSymbols ? parseme.Split ('/') : parseme.Split ('/', '\\', ':', '|', '-', '&', '+');
			if (pair.Length != 2) {
				// todo not a valid keypair
				#if DEBUG
				if (DebugIhildaWallet.TradePair) {
					Logging.WriteLog(method_sig + "pair.Length != 2, user entered incorrect keypair value"); 
				}
				#endif
				return null;
			}



			TradePair tp = new TradePair ( pair[0], pair[1] );

			return tp;


		}
	}
}

