using System;
using System.Collections.Generic;

namespace IhildaWallet
{
	public class ScamAddress
	{
		public ScamAddress ()
		{
		}

		public string Name { get; set; }

		public string Address { get; set; }

		public string Link { get; set; }

		public string Description { get; set; }

		public static List<ScamAddress> knownScammers = new List<ScamAddress> () {

			new ScamAddress () {
				Name = "Bitfinex",
				Address = "rLW9gnQo7BQhU6igk5keqYnH3TVrCxGRzm",
				Description =
				"Bitfinex is a well known exchange shrouded with controversy. " +
				"Bitfinex is banned from doing business in the united states due to fraudulent business practices. " +
				"The exchange is also known for \"No longer supporting\" coins which is code for stealing your coins"
			},

	    		new ScamAddress () {
				Name = ""
			    }
		};
	}
}
