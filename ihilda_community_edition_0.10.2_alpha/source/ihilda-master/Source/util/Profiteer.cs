using System;
using RippleLibSharp.Keys;
using System.Collections.Generic;
using RippleLibSharp.Transactions;

namespace IhildaWallet
{
	public static class Profiteer
	{
		/*
		
		public Profiteer (RippleAddress account)
		{
			
		}
		*/



		public static AutomatedOrder[] GetBuyBacks (OrderChange[] ords) {


			List<AutomatedOrder> lst = new List<AutomatedOrder> ();
			foreach ( OrderChange o in ords) {

				AutomatedOrder off = new AutomatedOrder {

					//Decimal price = off.TakerGets.getPriceAt (off.TakerPays);

					Account = o.Account,

					TakerGets = o.TakerPays / 1.007m,
					TakerPays = o.TakerGets * 1.007m
				};

				lst.Add (off);
			}

			return lst.ToArray ();
		}

		public static AutomatedOrder[] GetBuyBacks ( IEnumerable<AutomatedOrder> ords) {
			List<AutomatedOrder> lst = new List<AutomatedOrder> ();
			foreach ( AutomatedOrder o in ords) {

				AutomatedOrder off = new AutomatedOrder {

					//Decimal price = off.TakerGets.getPriceAt (off.TakerPays);

					Account = o.Account,

					TakerGets = o.TakerPays / 1.007m,
					TakerPays = o.TakerGets * 1.007m
				};

				lst.Add (off);
			}

			return lst.ToArray ();
		}

		public static AutomatedOrder GetBuyBack (AutomatedOrder off, ProfitStrategy strategy) {

			AutomatedOrder ao = new AutomatedOrder {
				Account = off.Account,
				TakerGets = off.TakerPays / strategy.Pay_Less,
				TakerPays = off.TakerGets * strategy.Get_More
			};

			return ao;

		}

	}
}

