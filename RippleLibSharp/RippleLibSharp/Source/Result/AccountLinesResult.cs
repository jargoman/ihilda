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

		public RippleCurrency[] GetBalancesAsRippleCurrencies () {

			RippleCurrency[] ret = new RippleCurrency[lines.Length];


			for (int i = 0; i < lines.Length; i++) {
				
				decimal amount = lines[i].GetBalanceAsDecimal ();
				RippleAddress issuer = lines[i].account;
				string currencyStr = lines[i].currency;
					

				var cur = new RippleCurrency (
					
					amount,
					issuer,
					currencyStr

				);

				ret [i] = cur;

			}

			return ret;

		}

		public List<RippleCurrency> GetBalanceAsCurrency ( String currency ) {

			if (currency == null) {
				throw new ArgumentNullException ();
			}

			List< RippleCurrency > list = null;

			foreach (TrustLine line in lines) {

				if (line.currency.Equals(currency)) {
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

			return list;

		}


		public RippleCurrency GetBalanceAsCurrency ( String currency, String issuer ) {

			if (currency == null || issuer == null) {
				throw new ArgumentNullException ();
			}



			foreach (TrustLine line in lines) {

				if (line.currency.Equals(currency)) {
					decimal amount = line.GetBalanceAsDecimal ();



					if ( !(issuer.Equals( line?.account )) ) {
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

			return null;

		}
	}
}

