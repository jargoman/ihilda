using System;
using System.Collections.Generic;

namespace IhildaWallet
{
	public class AddressCacheObject
	{
		/*
		public AddressCacheObject ()
		{
		}
		*/

#pragma warning disable RECS0122 // Initializing field with default value is redundant

		public TrustLineTableRow [] line = null;

		public Dictionary <String, Decimal> cash = null;  
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		public String[] GetCurrencies () {
			

			var keys = cash.Keys;
			String[] currencies = new string[keys.Count];

			int x = 0;
			foreach (String k in keys) {
				currencies [x++] = k;
			}

			return currencies;
		}

	}
}

