using System;
using System.Collections.Generic;
using RippleLibSharp.Transactions;

namespace RippleLibSharp.Result
{
	public class AccountOffersResult
	{
		/*
		public AccountOffersResult ()
		{
			
		}
		*/




#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private object _ledger_current_index = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant



#pragma warning disable IDE1006 // Naming Styles
		public string account { get; set; }


		public object ledger_current_index { 
			get {
				return _ledger_current_index;
			}
			set {
				_ledger_current_index = value;

				if (value is int) {
					ledger_current_index_int = (int)value;
				}

				if (value is string) {
					ledger_current_index_string = value.ToString ();
				}
			}
		}

		public int ledger_current_index_int {
			get;
			set;
		}

		public string ledger_current_index_string {
			get;
			set;

		}

		public int limit {
			get;
			set;
		}
		public Marker marker { get; set; }

		public Offer[] offers { get; set; }
		public bool validated { get; set; }
#pragma warning restore IDE1006 // Naming Styles
	}
}

