using System;

namespace IhildaWallet
{
	public partial class OrderBookWindow : Gtk.Window
	{
		public OrderBookWindow (RippleWallet rippleWallet) :
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();

			if (orderbookwidget1 == null) {
				orderbookwidget1 = new OrderBookWidget ();
				orderbookwidget1.Show ();
				vbox3.Add (orderbookwidget1);
			}

			this.orderbookwidget1.SetRippleWallet (rippleWallet);
		}

		public void SetTradePair (TradePair tp) {

			string title = "Orderbook for "
					+ (tp?.Currency_Base?.currency ?? "")
					+ "/"
					+ (tp?.Currency_Counter?.currency ?? "");

			Gtk.Application.Invoke ( delegate {
				this.Title = title;

			});

			orderbookwidget1.SetTradePair (tp);
			orderbookwidget1.ResyncNetwork(new System.Threading.CancellationToken());
		}
	}
}

