using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
//using RippleLibSharp.Network;
using IhildaWallet.Networking;
using RippleLibSharp.Commands.Subscriptions;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;
using RippleLibSharp.Util;
using Gtk;

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


			
	    		
			

			if (ProgramVariables.darkmode) {
				label26.Markup = "<b><span fgcolor=\"chartreuse\" font_size=\"xx-large\">Buy Order Bids</span></b>";

			}
			
		}

		public void InitRefreshTask ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (InitRefreshTask) + DebugRippleLibSharp.both_parentheses;
#endif

			Task.Factory.StartNew ( () => {

				TradePair tp = _tradePair;

				if (tp == null) {
#if DEBUG
					if (DebugIhildaWallet.OrderBookWidget) {
						Logging.WriteLog (method_sig + "tp == null");
					}
#endif
					// todo the lines below
					//if (this.buyorderbooktablewidget != null) {
					//	this.buyorderbooktablewidget.clearTable();
					//}

					//if (this.sellorderbooktablewidget != null) {
					//	this.sellorderbooktablewidget.clearTable();
					//}



					SetInfoBar ("<span foreground=\"red\">Can't sync. Null tradepair</span>");
					return;
				}

				RippleCurrency cur_base = tp.Currency_Base;
				if (cur_base == null) {
#if DEBUG
					if (DebugIhildaWallet.OrderBookWidget) {
						Logging.WriteLog (method_sig + "cur_base == null, returning");
					}
#endif


					SetInfoBar ("<span foreground=\"red\">Can't sync. Null base currency</span>");
					return;
				}

				RippleCurrency counter_currency = tp.Currency_Counter;
				if (counter_currency == null) {
#if DEBUG
					if (DebugIhildaWallet.OrderBookWidget) {
						Logging.WriteLog (method_sig + "counter_currency == null, returning");
					}
#endif


					SetInfoBar ("<span foreground=\"red\">Can't sync. Null currency</span>");
					return;
				}



				var token = tokenSource.Token;

				while (!token.IsCancellationRequested) {
					try {

						for (int i = 0; i < (this.ledgerDelay ?? 1); i++) {
							WaitHandle.WaitAny (new WaitHandle [] {
								LedgerTracker.LedgerResetEvent,
			    					token.WaitHandle
							}, 8000);
						}
						//await Task.Delay (6000, tokenSource.Token);
						ResyncNetwork (token);
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
			if (!ProgramVariables.showPopUps) {
				return;

			}

			if (tradePair == null) {
				return;
			}

			if (!ProgramVariables.showPopUps) {
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

			string [] askTitles = { "<b><u>Ask Price</u></b>", "<b><u>Size " + (tp?.Currency_Base?.currency ?? "") + "</u></b>", "<b><u>Sum " + (tp?.Currency_Base?.currency ?? "") + "</u></b>" };
			string [] bidTitles = { "<b><u>Sum " + (tp?.Currency_Counter?.currency ?? "") + "</u></b>", "<b><u>Size" + (tp?.Currency_Counter?.currency ?? "") + "</u></b>", "<b><u>Bid Price</u></b>" };


			orderbooktablewidget1.SetTitle ( bidTitles );

			orderbooktablewidget2.SetTitle ( askTitles );



			this.SetToolTips (tp);

			Gtk.Application.Invoke (delegate {
				this.label27.Hide ();
				this.label27.Markup = message;


			});
		}


		private TradePair _tradePair = null;

		public void ResyncNetwork (CancellationToken token)
		{
#if DEBUG
			String method_sig = clsstr + nameof (ResyncNetwork) + DebugRippleLibSharp.both_parentheses;
#endif



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
			if (ni == null) {


				SetInfoBar ("<span foreground=\"red\">Network error</span>");
				return;
			}

			if (!ni.IsConnected ()) {

				SetInfoBar ("<span foreground=\"red\">Not connected to network</span>");
				return;
			}

			if (limit == null) {


				SetInfoBar ("<span foreground=\"orange\">For better performance set the limit for the number of orders using options</span>");

			} else if (limit > 50) {

				SetInfoBar ("<span foreground=\"orange\">For better performance reduce the number of displayed orders using options</span>");
			}


			TradePair tp = _tradePair;
			RippleCurrency cur_base = tp.Currency_Base;
			RippleCurrency counter_currency = tp.Currency_Counter;

			Task<Response<BookOfferResult>> buyTask =
			limit == null ?
				RippleLibSharp.Commands.Stipulate.BookOffers.GetResult (counter_currency, cur_base, ni, token)
	   			: RippleLibSharp.Commands.Stipulate.BookOffers.GetResult (counter_currency, cur_base, limit + 1, ni, token);
			;

			var btask = buyTask.ContinueWith ((arg) => {

				Offer [] buys = null;
				Offer [] res = arg?.Result?.result?.offers;
				if (res == null) {
					// TODO

					orderbooktablewidget2.SetErrorBar ("<span foreground=\"red\">Network returned no bids</span>");

					return;
				}

				if (limit != null) {
					buys = new Offer [(int)limit];




					Array.Copy (res, buys, (int)limit);
				} else {
					buys = res;
				}

				IEnumerable<AutomatedOrder> buyoffers = AutomatedOrder.ConvertFromIEnumerableOrder (buys);
				this.orderbooktablewidget1.SetBids (buyoffers.ToArray ());  // .ToArray()
											    //buyoffers.
			});


			Task<Response<BookOfferResult>> sellTask =
			limit == null ?
				RippleLibSharp.Commands.Stipulate.BookOffers.GetResult (cur_base, counter_currency, ni, token)
	   	 		: RippleLibSharp.Commands.Stipulate.BookOffers.GetResult (cur_base, counter_currency, limit + 1, ni, token);

			var stask = sellTask.ContinueWith ((arg) => {

				Offer [] sells = null;
				var res = arg?.Result?.result?.offers;
				if (res == null) {


					orderbooktablewidget1.SetErrorBar ("<span foreground=\"red\">Network returned no asks</span>");
					return;
				}

				if (limit != null) {
					sells = new Offer [(int)limit];




					Array.Copy (res, sells, (int)limit);

				} else {
					sells = res;
				}

				IEnumerable<AutomatedOrder> selloffers = AutomatedOrder.ConvertFromIEnumerableOrder (sells);
				this.orderbooktablewidget2.SetAsks (selloffers.ToArray ());  // .ToArray()
			});






			const string refreshBids = "Syncing bids";
			const string refreshAsks = "Syncing asks";
			const int countMax = 10;

			string refBids = default(string);
			string refAsks = default(string);

			int count = 0;
			do {


				if (btask.IsCompleted) {

				} else if (btask.IsFaulted) {
					orderbooktablewidget1.SetErrorBar ("<span foreground=\"green\">syncing bids faulted</span>");
				} else if (btask.IsCanceled) {
					orderbooktablewidget1.SetErrorBar ("<span foreground=\"green\">syncing bids canceled</span>");
				} else {

					if (count % 5 == 0) {
						refBids = refreshBids;
					} else {
						refBids += ".";
					}

					orderbooktablewidget1.SetInfoBar ("<span foreground=\"green\">" + refBids + "</span>");

					if (count > countMax) {
						orderbooktablewidget1.SetErrorBar ("<span foreground=\"red\">Syncing bids is taking longer than usual</span>");
					}
				}



				if (stask.IsCompleted) {

				} else if (stask.IsFaulted) {
					orderbooktablewidget2.SetErrorBar ("<span foreground=\"red\">Syncing asks faulted</span>");
				} else if (stask.IsCanceled) {
					orderbooktablewidget2.SetErrorBar ("<span foreground=\"red\">Syncing asks canceled</span>");
				} else {
					if (count % 5 == 0) {
						refAsks = refreshAsks;
					} else {
						refAsks += ".";
					}

					orderbooktablewidget2.SetInfoBar ("<span foreground=\"green\">" + refAsks + "</span>");

					if (count > countMax) {
						orderbooktablewidget2.SetErrorBar ("<span foreground=\"red\">Syncing asks is taking longer than usual</span>");
					}
				}

				Task.WaitAll (new Task [] { btask, stask }, 1000, token);

				count++;

				if (count > countMax * 10) {
					count = 0;

					// TODO what to do about this
					if (autoRefresh) {
						return;
					}

				}

			}
			while (
			    (!btask.IsCompleted && !btask.IsFaulted && !btask.IsCanceled)
			    || (!stask.IsCompleted && !stask.IsFaulted && !stask.IsCanceled)
		    	); 
				





#if DEBUG

			if (DebugIhildaWallet.OrderBookWidget) {
				Logging.WriteLog(method_sig + "done waiting");
				//Logging.writeLog(method_sig + "e.Message = " + Debug.toAssertString(e.Message));
			}
#endif
				
			
			

			



	    		
			
			

		}

		private void SetInfoBar (string text)
		{
			Application.Invoke ( delegate {

				infoLabel.Markup = text;
			});
		}

		public uint? limit = null;
		public uint? ledgerDelay = null;
		public bool autoRefresh = false;

#if DEBUG
		private static readonly string clsstr = nameof (OrderBookWidget) + DebugRippleLibSharp.colon;
#endif

	}
}

