using System;
using RippleLibSharp.Transactions;

namespace IhildaWallet
{
	public class Sentiment
	{
		public Sentiment ()
		{
		}

		public String Match {
			get;
			set;
		}

		public string Rating {
			get;
			set;
		}

	}


	public enum SentimentRatingEnum {
		Mooning = 6,
		Very_Bullish = 5,
		Bullish = 4,
		Neutral = 3,
		Bearish = 2,
		Very_Bearish = 1,
		Trash = 0

	}
}
