using System;
using RippleLibSharp.Transactions;

namespace IhildaWallet
{
	public class OrderFilledRule
	{
		
		/*
		public OrderFilledRule ()
		{


		}
		*/

		public string Account {
			get;
			set;
		}

		public bool IsActive {
			get;
			set;
		}

		public RippleCurrency BoughtCurrency {
			get;
			set;
		}

		public RippleCurrency SoldCurrency {
			get;
			set;
		}

		public ProfitStrategy RefillMod {
			get;
			set;
		}



		public bool DetermineMatch (Offer o) {

			if (o == null) {
				throw new NullReferenceException ("OrderFilledRule can not match with offer o = null");
			}

			if (BoughtCurrency != null) {

				if ( BoughtCurrency.currency != null && !BoughtCurrency.currency.Trim().Equals("")) {
					if (!BoughtCurrency.currency.Equals(o.TakerPays.currency)) {
						return false;
					}
				}

				if ( BoughtCurrency.issuer != null && !BoughtCurrency.issuer.Trim().Equals("")) {
					if (!BoughtCurrency.issuer.Equals(o.TakerPays.issuer)) {
						return false;
					}

				}

			}

			if (SoldCurrency != null) {
				if (SoldCurrency.currency != null && !SoldCurrency.currency.Trim().Equals("")) {
					if (!SoldCurrency.currency.Equals(o.TakerGets.currency)) {
						return false;
					}
				}

				if (SoldCurrency.issuer != null && !SoldCurrency.issuer.Trim().Equals("")) {
					if (!SoldCurrency.issuer.Equals(o.TakerGets.issuer)) {
						return false;
					}
				}
			}


			return true;

		}

	}
}

