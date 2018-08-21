using System;
using System.Collections.Generic;
using RippleLibSharp.Transactions;

using RippleLibSharp;

using RippleLibSharp.Result;

namespace IhildaWallet
{
	public class TxSummary : IRawJsonInterface
	{
		/*
		public TxSummary ()
		{
		}
		*/



		public string Tx_Type {

			get;
			set;
		}

		public string Tx_id {
			get;
			set;
		}

		public string Tx_Summary_Buy {
			get;
			set;
		}

		public string Meta_Summary_Buy {
			get;
			set;
		}

		public string Tx_Summary_Sell {
			get;
			set;
		}

		public string Meta_Summary_Sell {
			get;
			set;
		}

		public string RawJSON {
			get;
			set;
		}

		public uint Time {
			get;
			set;
		}

		public OrderChange [] Changes {
			get;
			set;
		}



		/*

		public Offer[] GetBuyBackOffers (Decimal profit) {
			Offer[] offrs =  new Offer[Changes.Length];
			int x = 0;
			foreach (Offer off in Changes) {
				offrs [x].taker_pays = off.taker_gets * profit;
				offrs [x].taker_gets = off.taker_pays;
				x++;
			}

			return offrs;
		}
		*/

		private Offer[] Condense ( Offer[] offers ) {
			LinkedList<Offer> r = new LinkedList<Offer> ();

			for (int i = 0; i < offers.Length; i++) {
				Offer o = offers [i].Copy ();

				foreach ( Offer p in r) {
					if ( p.taker_gets.IsSameCurrency( offers [i].taker_gets ) && p.taker_pays.IsSameCurrency( offers [i].taker_pays )) {
						continue; // note this exits the inner not outer loop
					}
				}

				for ( int j = i + 1; j < offers.Length; j++ ) {
					if (!o.taker_gets.IsSameCurrency(offers[j].taker_gets) || !o.taker_pays.IsSameCurrency(offers[j].taker_pays)) {
						continue;
					}

					r.AddLast (o);
				}

			}

			Offer[] rr = new Offer[r.Count];
			r.CopyTo (rr, 0);
			return rr;

		}

	}
}

