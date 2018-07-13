using System;
using System.Collections.Generic;

namespace RippleLibSharp.Result
{
	public class AccountCurrenciesResult
	{

#pragma warning disable IDE1006 // Naming Styles
		public string ledger_hash { get; set; }

		public int ledger_index { get; set; }
		public string[] receive_currencies { get; set; }
		public string[] send_currencies { get; set; }
		public bool validated { get; set; }
#pragma warning restore IDE1006 // Naming Styles

	}
}

