using System;
using System.Linq;
using System.Collections.Generic;
using RippleLibSharp.Trust;
using RippleLibSharp.Transactions;
using RippleLibSharp.Keys;


namespace RippleLibSharp.Result
{
	public class AccountLinesResult
	{
		/*
		public AccountLinesResult ()
		{
		}
		*/

#pragma warning disable IDE1006 // Naming Styles

		public string account { get; set; }

		public TrustLine[] lines { get; set; }


		public Marker marker {
			get;
			set;
		}

#pragma warning restore IDE1006 // Naming Styles

		public IEnumerable<RippleCurrency> GetBalancesAsRippleCurrencies () {

			IEnumerable<RippleCurrency> retMe = null;
			if (!Configuration.Config.PreferLinq) {
				RippleCurrency [] ret = new RippleCurrency [lines.Length];


				for (int i = 0; i < lines.Length; i++) {

					decimal amount = lines [i].GetBalanceAsDecimal ();
					RippleAddress issuer = lines [i].account;
					string currencyStr = lines [i].currency;
					string lim = lines [i].limit;


					var cur = new RippleCurrency (

						amount,
						issuer,
						currencyStr


					) {
						SelfLimit = lim
					};

					ret [i] = cur;

					retMe = ret;

				}
			} else {

				var ret = lines.Select ((TrustLine line) => {
					decimal amount = line.GetBalanceAsDecimal ();
					RippleAddress issuer = line.account;
					string currencyStr = line.currency;
					string lim = line.limit;


					var cur = new RippleCurrency (

						amount,
						issuer,
						currencyStr


					) {
						SelfLimit = lim
					};

					return cur;
				});

				retMe = ret;
			}
			return retMe;
			

		}

		public IEnumerable<RippleCurrency> GetBalanceAsCurrency ( String currency ) {

			if (currency == null) {
				throw new ArgumentNullException ();
			}

			IEnumerable<RippleCurrency> ret = null;

			if (!Configuration.Config.PreferLinq) {
				List<RippleCurrency> list = null;

				foreach (TrustLine line in lines) {

					if (line.currency.Equals (currency)) {
						decimal amount = line.GetBalanceAsDecimal ();
						RippleAddress issuer = line.account;
						string currencyStr = line.currency;

						if (list == null) {
							list = new List<RippleCurrency> ();
						}

						var cur = new RippleCurrency (

							amount,
							issuer,
							currencyStr

						);

						list.Add (cur);


					}

				}

				ret = list;

			} else {
				var v = from TrustLine line in lines where line.currency.Equals (currency) select

				     //RippleAddress issuer = line.account;
				     //string currencyStr = line.currency;


				     new RippleCurrency (

					     line.GetBalanceAsDecimal (),
					     line.account,
					     line.currency

				     )
			    ;

				ret = v;
			}
			return ret;

		}


		public RippleCurrency GetBalanceAsCurrency (String currency, String issuer) {

			if (currency == null || issuer == null) {
				throw new ArgumentNullException ();
			}


			if (!Configuration.Config.PreferLinq) {
				foreach (TrustLine line in lines) {

					if (line.currency.Equals (currency)) {
						decimal amount = line.GetBalanceAsDecimal ();



						if (!(issuer.Equals (line?.account))) {
							continue;
						}

						var cur = new RippleCurrency (

							amount,
							issuer,
							currency

						);

						return cur;

					}

				}
			} else {
				var v = from TrustLine line in lines
					where line.currency.Equals (currency)
					where issuer.Equals (line?.account)
					select new RippleCurrency (
						line.GetBalanceAsDecimal (),
						issuer,
						currency
						);

				return v.FirstOrDefault ();
			}

			return null;
		}
	}
}

