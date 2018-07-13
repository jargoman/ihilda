using System;
using System.Collections.Generic;
using System.Linq;
using RippleLibSharp.Transactions;

namespace RippleLibSharp.Transactions
{
	public class OrderChange : Offer
	{
		/*
		public OrderChange ()
		{
		}
		*/

		//public String Account { get; set; }

		public string BuyOrderChange {
			get;
			set;
		}

		public string SellOrderChange {
			get;
			set;
		}


		public RippleCurrency TakerGetFee {
			get;
			set;
		}

		public RippleCurrency TakerPaysFee {
			get;
			set;
		}



		/*public static void calculateFees (RippleCurrency full, IEnumerable<OrderChange> changes)
		 {

			full.amount = Math.Abs((Decimal)full.amount);
			var getsorders = from change in changes where change.taker_gets.isSameCurrency(full) select change;

			Decimal takerGetsSum;
			if (getsorders.Count() == 1) {
				takerGetsSum = full.amount;
			} else {
				takerGetsSum = getsorders.ToList().Sum (x => x.taker_gets.amount);
			}

			Decimal totalGetsFee = full.amount - takerGetsSum;


			foreach ( OrderChange oc in getsorders ) {
				Decimal ratio = ((Decimal)oc.taker_gets.amount) / takerGetsSum; // not ratio of gets/pays, rather ratio of this order from totalt
				RippleCurrency rc = oc.taker_gets.deepCopy();
				rc.amount = Math.Round(totalGetsFee * ratio, 15);
				oc.TakerGetFee = rc;
			}


			// repeat of above only for takerpays
			var paysorders = from change in changes where change.taker_pays.isSameCurrency(full) select change;
			Decimal takerPaysSum;
			if (paysorders.Count() == 1) {
				takerPaysSum = full.amount;
			} else {
				takerPaysSum = paysorders.ToList ().Sum (x => (Decimal)x.taker_pays.amount);
			}
				
			Decimal totalPaysFee = full.amount - takerPaysSum;

			foreach ( OrderChange oc in paysorders ) {
				Decimal ratio = (Decimal)oc.taker_pays.amount / takerPaysSum;
				RippleCurrency rc = oc.taker_pays.deepCopy ();
				rc.amount = Math.Round( totalPaysFee * ratio, 15);
				oc.TakerPaysFee = rc;
			}
		}
		*/

	}
}

