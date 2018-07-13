using System;
using Gtk;

namespace IhildaWallet
{
	public class OrderBookLabel : Label
	{
		public OrderBookLabel (int x, int y)
		{
			y_pos = y;
			x_pos = x;
		}

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public int? x_pos = null;

		public int? y_pos = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant
		//OrderBookTableWidget obtw = null;
	}
}

