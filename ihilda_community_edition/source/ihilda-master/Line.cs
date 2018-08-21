using System;

namespace IhildaWallet
{
	public class Line
	{
		public Line (dynamic d)
		{
			this.account = (String)d.account;
			this.balance = (String)d.balance;
			this.currency = (String)d.currency;
			this.limit = (String)d.limit;
			this.limit_peer = (String)d.limit_peer;

			this.quality_in = (double)d.quality_in;
			this.quality_out = (double)d.quality_out;
		}

		public Object getTableXIndex (uint x) {
			switch (x) {

			case 0:
				return this.account;
				//break;

			case 1:
				return this.balance;
				//break;

			case 2:
				return this.currency;
				//break;

			case 3:
				return this.limit;
				//break;
			case 4:
				return this.limit_peer;
				//break;
			case 5:
				return this.quality_in;
				//break;
			case 6:
				return this.quality_out;
				//break;
			default:
				Logging.write ("Line.getTableXIndex index out of bounds\n");
				return null;
				//break;
			}


		}

		public double getBalanceAsDouble () {

			try {
				return Convert.ToDouble(this.balance);
			}

			catch (Exception e) {
				Logging.write ("Error in Class Line, Exception thrown converting string to double");
				Logging.write (e.Message);
				return 0;
			}
		}

		public static String[] getTableTitleRow () {
			if (titles == null) {
				titles = new string[] { "Account","Balance","Currency","Limit","Limit Peer","Quality In","Quality Out" };


			}
			return titles;
		}



		private static String[] titles = null;

		public String account = null;

		public String balance = null;

		public String currency = null;

		public String limit = null;

		public String limit_peer = null;

		public double quality_in = 0;

		public double quality_out = 0;

		public static uint numColumns = 7;
	}
}

