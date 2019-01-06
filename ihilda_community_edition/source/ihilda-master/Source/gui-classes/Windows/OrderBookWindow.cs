using System.Threading;
using System.Threading.Tasks;
using Gtk;
using RippleLibSharp.Commands.Subscriptions;

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

			progressbar1.PulseStep = 0.1;

			var opts = OrderBookOptions.LoadOrderBookOptions ();

			if (opts.AutoRefresh) {
				hbox1.Hide ();
			}


			orderbookwidget1.limit = opts.Limit;
			orderbookwidget1.ledgerDelay = opts.LedgerDelay;
			orderbookwidget1.autoRefresh = opts.AutoRefresh;



			button316.Clicked += delegate {
				Task.Run ( delegate {

					ResyncNetworkManual (new CancellationToken ());

				});

			};
		}

		public void SetTradePair (TradePair tp) {

			string title = "Orderbook for "
					+ (tp?.Currency_Base?.currency ?? "")
					+ "/"
					+ (tp?.Currency_Counter?.currency ?? "");

			Gtk.Application.Invoke ( delegate {
				this.Title = title;

			});

			//orderbookwidget1.limit = 10;
			orderbookwidget1.SetTradePair (tp);


			Task.Run ( delegate {

				if (orderbookwidget1.autoRefresh) {
					ResyncNetWorkAuto (new CancellationToken ());
				} else {
					ResyncNetworkManual (new CancellationToken ());
				}
				

			});


		}


		public void ResyncNetWorkAuto (CancellationToken token)
		{


			while (true) {
				var task = Task.Run (delegate {
					orderbookwidget1.ResyncNetwork (token);

				});

				for (int i = 0; i < orderbookwidget1.ledgerDelay; i++) {
					LedgerTracker.LedgerResetEvent.WaitOne ();
				}
				
			}

	    		/*
			return;
			
			*/    

		}

		public void ResyncNetworkManual (CancellationToken token)
		{

			var task = Task.Run (delegate {
				orderbookwidget1.ResyncNetwork (token);

			});

			while (!token.IsCancellationRequested && task != null && !task.IsCanceled && !task.IsCompleted && !task.IsFaulted) {
				//Application.Invoke ((sender, e) => progressbar1.Pulse ());
				Application.Invoke (delegate {

					progressbar1.Pulse ();
				});
				task.Wait (1000);
			}

			Application.Invoke (delegate {

				progressbar1.Fraction = 0;
			});
		}


	}
}

