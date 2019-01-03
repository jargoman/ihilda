using System;
using System.Threading.Tasks;

using System.Collections.Generic;
using Codeplex.Data;
using RippleLibSharp.Network;
using RippleLibSharp.Transactions;
using RippleLibSharp.Result;
using RippleLibSharp.Commands.Accounts;
//using RippleLibSharp.Network;
using IhildaWallet.Networking;
using System.Linq;
using RippleLibSharp.Util;
using System.Threading;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderBookWidget : Gtk.Bin
	{
		public OrderBookWidget ()
		{

#if DEBUG
			string method_sig = clsstr + nameof (OrderBookWidget) + DebugRippleLibSharp.both_parentheses;
#endif


			this.Build ();

			if (orderbooktablewidget1 == null) {
				orderbooktablewidget1 = new OrderBookTableWidget ();
				orderbooktablewidget1.Show ();
				vbox3.Add (this.orderbooktablewidget1);

			}

			if (orderbooktablewidget2 == null) {
				orderbooktablewidget2 = new OrderBookTableWidget ();
				orderbooktablewidget2.Show ();
				vbox4.Add (this.orderbooktablewidget2);
			}


			orderbooktablewidget1.SetTitle(OrderBookTableWidget.bidTitles);

			orderbooktablewidget2.SetTitle(OrderBookTableWidget.askTitles);


			Task.Factory.StartNew (async () => {

				while (!tokenSource.IsCancellationRequested) {
					try {
						await Task.Delay (6000, tokenSource.Token);
						ResyncNetwork (tokenSource.Token);
					} catch (Exception e) {
#if DEBUG
						if (DebugIhildaWallet.OrderBookWidget) {
							Logging.ReportException (method_sig, e);
						}
#endif
					}
				}
			}
			);

			
		}

		public void SetToolTips (TradePair tradePair)
		{
			if (!Program.showPopUps) {
				return;

			}

			if (tradePair == null) {
				return;
			}

			if (!Program.showPopUps) {
				return;
			}

			var bidmess = 
				"list of bid orders to buy " + 
				tradePair.Currency_Base.currency +
				" with " +
				tradePair.Currency_Counter.currency;

			var askmess =
				"list of ask orders selling " +
				tradePair.Currency_Base.currency +
				" for " +
				tradePair.Currency_Counter.currency;

			Gtk.Application.Invoke (
				delegate {
					label26.TooltipMarkup = bidmess;
					label25.TooltipMarkup = askmess;

				}
			);
		}

		~OrderBookWidget ()
		{
			tokenSource?.Cancel ();  // = false;
		}

		private CancellationTokenSource tokenSource = new CancellationTokenSource();


		public void SetRippleWallet (RippleWallet rippleWallet)
		{
			orderbooktablewidget1.SetRippleWallet (rippleWallet);
			orderbooktablewidget2.SetRippleWallet (rippleWallet);

			orderbooktablewidget1.CreateTable ();
			orderbooktablewidget2.CreateTable ();
		}

		public void SetTradePair (TradePair tp)
		{

			_tradePair = tp;

			this.orderbooktablewidget1._TradePair = tp;
			this.orderbooktablewidget2._TradePair = tp;

			string message = "<b><u>Orderbook for "
					+ (tp?.Currency_Base?.currency ?? "")
					+ "/"
					+ (tp?.Currency_Counter?.currency ?? "")
					+ " </u></b>";

			this.SetToolTips (tp);

			Gtk.Application.Invoke (delegate {
				this.label27.Hide ();
				this.label27.Markup = message;


			});
		}


		private TradePair _tradePair = null;

		public void ResyncNetwork ( CancellationToken token )
		{
#if DEBUG
			String method_sig = clsstr + nameof (ResyncNetwork) + DebugRippleLibSharp.both_parentheses;
#endif

			TradePair tp = _tradePair;

			if (tp == null) {
#if DEBUG
				if (DebugIhildaWallet.OrderBookWidget) {
					Logging.WriteLog(method_sig + "tp == null");
				}
#endif
				// todo the lines below
				//if (this.buyorderbooktablewidget != null) {
				//	this.buyorderbooktablewidget.clearTable();
				//}

				//if (this.sellorderbooktablewidget != null) {
				//	this.sellorderbooktablewidget.clearTable();
				//}
				return;
			}

			RippleCurrency cur_base = tp.Currency_Base;
			if (cur_base == null) {
#if DEBUG
				if (DebugIhildaWallet.OrderBookWidget) {
					Logging.WriteLog(method_sig + "cur_base == null, returning");
				}
#endif
				return;
			}

			RippleCurrency counter_currency = tp.Currency_Counter;
			if (counter_currency == null) {
#if DEBUG
				if (DebugIhildaWallet.OrderBookWidget) {
					Logging.WriteLog(method_sig + "counter_currency == null, returning");
				}
#endif
				return;
			}
				
			/*
			Object gets = cur_base.getAnonObjectWithoutAmount();//counter_currency.getAnonObjectWithoutAmount();
			Object pays = counter_currency.getAnonObjectWithoutAmount();

			String com = "book_offers";

			Object o = new {
				command = com,
				taker_gets = gets,

				taker_pays = pays,
				id = jsid1
			};
			*/
			NetworkInterface ni = NetworkController.GetNetworkInterfaceGuiThread ();

			Task< Response<BookOfferResult>> buyTask = RippleLibSharp.Commands.Stipulate.BookOffers.GetResult (counter_currency, cur_base, ni, token);
			Task< Response<BookOfferResult>> sellTask = RippleLibSharp.Commands.Stipulate.BookOffers.GetResult (cur_base, counter_currency, ni, token);


			if (true) {

			}
			Task.WaitAll ( new Task[] { buyTask, sellTask }, token );

			//buyTask.Wait ();
			//sellTask.Wait ();

#if DEBUG

			if (DebugIhildaWallet.OrderBookWidget) {
				Logging.WriteLog(method_sig + "done waiting");
				//Logging.writeLog(method_sig + "e.Message = " + Debug.toAssertString(e.Message));
			}
#endif
				
			Offer[] buys = buyTask?.Result?.result?.offers;
			Offer[] sells = sellTask?.Result?.result?.offers;

			//string account = WalletManager.selectedWallet.getStoredReceiveAddress ();

			IEnumerable<AutomatedOrder> buyoffers = AutomatedOrder.ConvertFromIEnumerableOrder (  buys);
			IEnumerable<AutomatedOrder> selloffers = AutomatedOrder.ConvertFromIEnumerableOrder ( sells);

				//d.result.offers;
			//System.Double id = d.id.handle_bar;

#if DEBUG
			if (DebugIhildaWallet.OrderBookWidget) {
				Logging.WriteLog(method_sig + "end for");
				//Logging.writeLog("id type =" + id.GetType().ToString());
				//Logging.writeLog("id = " + id.ToString());
			}
#endif





			
			this.orderbooktablewidget1.SetBids(buyoffers.ToArray());  // .ToArray()
			this.orderbooktablewidget2.SetAsk(selloffers.ToArray());  // .ToArray()

		}

#if DEBUG
		private static readonly string clsstr = nameof (OrderBookWidget) + DebugRippleLibSharp.colon;
#endif

	}
}

