using System;
using RippleLibSharp.Transactions;

namespace RippleLibSharp.Result
{
	public class BookOfferResult
	{
		/*
		public BookOfferResult ()
		{
		}
		*/


#pragma warning disable IDE1006 // Naming Styles

		public int ledger_current_index { get; set; }

		public Offer[] offers { get; set; }
		public bool validated { get; set; }
	
#pragma warning restore IDE1006 // Naming Styles

	}


}

