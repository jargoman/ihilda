using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Gtk;
using Codeplex.Data;
using RippleLibSharp.Transactions;
using RippleLibSharp.Binary;
using RippleLibSharp.Util;
using IhildaWallet.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class OrderBookTableWidget : Gtk.Bin
	{
		public OrderBookTableWidget ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (OrderBookTableWidget) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.OrderBookTableWidget) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			this.Build ();

#if DEBUG
			if (DebugIhildaWallet.OrderBookTableWidget) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}
#endif



		}

		OrderBookLabel [,] labels = new OrderBookLabel [3, MAX_ORDERS + 1]; // + 1 for the title
		Table table = null;
		Label infoBarLabel = null;

		public void CreateTable ()
		{


#if DEBUG
			String method_sig = clsstr + nameof (CreateTable) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.OrderBookTableWidget) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
#endif

			VBox vbox = new VBox (false, 2);



			infoBarLabel = new Label ("");
			infoBarLabel.Show ();

			table = new Table ((uint)(MAX_ORDERS + 1u), 3u, false);
			table.Show ();
			
			vbox.Add (infoBarLabel);
			vbox.Add (table);


			//Label[,] 
			labels = new OrderBookLabel [3, MAX_ORDERS + 1];

			RippleWallet rw = _rippleWallet;


			//Logging.write("aaauh");
			for (int x = 0; x < 3; x++) {
				for (int y = 0; y < MAX_ORDERS + 1; y++) {

					OrderBookLabel orderbookLabel = new OrderBookLabel (x, y);
					orderbookLabel.Show ();
					orderbookLabel.SetAlignment (0.0f, 0.0f);
					orderbookLabel.SetPadding (2, 1);

					EventBox eb = new EventBox ();

					eb.Show ();

					// if y == 0 then it's a title and requires no events 
					if (y != 0) {
						eb.ButtonReleaseEvent += delegate (object o, ButtonReleaseEventArgs args) {
							Offer [] offs = null;
							lock (orderLock) {
								offs = offers;
							}




#if DEBUG

							string event_sig = 
								method_sig 
								+ nameof (OrderBookLabel) 
								+ DebugRippleLibSharp.space_char 
								+ nameof (ButtonReleaseEvent) 
								+ DebugRippleLibSharp.colon;


							if (DebugIhildaWallet.OrderBookTableWidget) {
								Logging.WriteLog (event_sig + DebugRippleLibSharp.begin);
							}

#endif

							if (!(o is EventBox)) {
								throw new ArgumentException ();
							}

							EventBox ebox = (EventBox)o;



							OrderBookLabel obl = (OrderBookLabel)eb.Child; //orderbookLabel; //o as OrderBookLabel;
							if (obl == null) {
#if DEBUG
								if (DebugIhildaWallet.OrderBookTableWidget) {
									Logging.WriteLog (event_sig + "obl == null, returning");
								}
#endif
								return;
							}

							if (obl.y_pos == null) {
#if DEBUG
								if (DebugIhildaWallet.OrderBookTableWidget) {
									Logging.WriteLog (event_sig + "obl.y_pos == null, returning");
								}
#endif
								return;
							}

							if (offs == null) {
#if DEBUG
								if (DebugIhildaWallet.OrderBookTableWidget) {
									Logging.WriteLog (event_sig + "offers == null, returning");
								}
#endif
								return;
							}



							Offer off = offs [(int)obl.y_pos - 1]; // - 1 because with arrays 1 is zero. 
							IEnumerable<Offer> takes = offs.Take ((int)obl.y_pos); // take function name is coinsidence take is inherited from array and has nothing tto do wit orders


							if (off == null) {
								return;
							}





							if (args.Event.Button == 1) {


								TakeOffers (takes);

								return;
							}

							Gtk.Menu menu = new Menu ();

							MenuItem mainBuyMenu = new MenuItem (
								Program.darkmode ?
								"<span fgcolor=\"chartreuse\" font_size=\"xx-large\"><b>Buy</b></span>":
								"<span fgcolor=\"green\" font_size=\"xx-large\"><b>Buy</b></span>"
								);

							Label label = (Label)mainBuyMenu.Child;
							label.UseMarkup = true;

							mainBuyMenu.Show ();

							menu.Add (mainBuyMenu);


							Gtk.MenuItem mainSellMenu = new MenuItem (Program.darkmode ? "<span fgcolor=\"#FFAABB\" font_size=\"xx - large\"><b>Sell</b></span>"  : "<span fgcolor=\"red\" font_size=\"xx-large\"><b>Sell</b></span>");
							Label labe = (Label)mainSellMenu.Child;
							labe.UseMarkup = true;

							mainSellMenu.Show ();
							menu.Add (mainSellMenu);
							 
							Menu buyMenu = new Menu ();
							buyMenu.Show ();
							Menu sellMenu = new Menu ();
							sellMenu.Show ();

							mainBuyMenu.Submenu = buyMenu;
							mainSellMenu.Submenu = sellMenu;

							#region take
							StringBuilder takeMessage = new StringBuilder ();
							takeMessage.Append ("<b>Take ");
							takeMessage.Append (takes.Count ().ToString ());
							takeMessage.Append (" Orders</b>");

							Gtk.MenuItem take = new MenuItem (takeMessage.ToString ());
							take.Show ();

							if (offerType == OrderEnum.ASK) {

								buyMenu.Add (take);
							} else {
								sellMenu.Add (take);
							}


							Gtk.Label menulabel = (Label)take.Child;
							menulabel.UseMarkup = true;

							take.ButtonPressEvent += (object sender, ButtonPressEventArgs e) => {


#if DEBUG
								if (DebugIhildaWallet.OrderBookTableWidget) {
									Logging.WriteLog ("take selected");
								}
#endif

								TakeOffers (takes);

							};

							#endregion

							#region buy
							Gtk.MenuItem buy = new MenuItem (
								Program.darkmode ?
								"<b>Prepare a <span fgcolor=\"chartreuse\">buy</span> order at this price</b>" :
								"<b>Prepare a <span fgcolor=\"green\">buy</span> order at this price</b>"
								);
							buy.Show ();
							buyMenu.Add (buy);

							menulabel = (Label)buy.Child;
							menulabel.UseMarkup = true;


							buy.ButtonPressEvent += ( object obj, ButtonPressEventArgs buttonPressEventArgs) => {
#if DEBUG
								if (DebugIhildaWallet.OrderBookTableWidget) {
									Logging.WriteLog ("buy selected");
								}
#endif

								TradeWindow tradeWindow = ShowTradeWindow ( /*off,*/ rw);
								if (offerType == OrderEnum.ASK) {
									AutomatedOrder automatedOrder = new AutomatedOrder (off) {
										taker_gets = off?.taker_pays?.DeepCopy (),
										taker_pays = off?.taker_gets?.DeepCopy ()
									};
									tradeWindow.SetBuyOffer (automatedOrder);
								} else {
									tradeWindow.SetBuyOffer (off);
								}


							};

							#endregion

							#region cassbuy

							Gtk.MenuItem cassbuy = new MenuItem (
								Program.darkmode ?
								"<b>Cascade <span fgcolor=\"chartreuse\">buy</span> orders beginning at this price</b>" :
								"<b>Cascade <span fgcolor=\"green\">buy</span> orders beginning at this price</b>"
								);

							cassbuy.Show ();
							buyMenu.Add (cassbuy);

							menulabel = (Label)cassbuy.Child;
							menulabel.UseMarkup = true;



							cassbuy.ButtonPressEvent += (object obj, ButtonPressEventArgs buttonPressEventArgs) => {
#if DEBUG
								if (DebugIhildaWallet.OrderBookTableWidget) {
									Logging.WriteLog ("cassbuy selected");
								}
#endif

								TradeWindow tradeWindow = ShowTradeWindow ( /*off ,*/ rw);


								if (offerType == OrderEnum.ASK) {
									AutomatedOrder automatedOrder = new AutomatedOrder (off);
									automatedOrder.taker_gets = off.taker_pays.DeepCopy ();
									automatedOrder.taker_pays = off.taker_gets.DeepCopy ();
									tradeWindow.InitCascadedBuyOffer (automatedOrder);
								} else {
									tradeWindow.InitCascadedBuyOffer (off);
								}


								//tradeWindow.InitiateCascade (off, offerType);
							};
								

							#endregion


							#region autobuy

							Gtk.MenuItem autobuy = new MenuItem (
								Program.darkmode ?
								"<b>Prepare an automated <span fgcolor=\"chartreuse\">buy</span> order at this price</b>" :
								"<b>Prepare an automated <span fgcolor=\"green\">buy</span> order at this price</b>"
								);
							autobuy.Show ();
							buyMenu.Add (autobuy);

							menulabel = (Label)autobuy.Child;
							menulabel.UseMarkup = true;

							autobuy.ButtonPressEvent += (object obj, ButtonPressEventArgs buttonPressEventArgs) => {
#if DEBUG
								if (DebugIhildaWallet.OrderBookTableWidget) {
									Logging.WriteLog ("autobuy selected");
								}
#endif

								TradeWindow tradeWindow = ShowTradeWindow ( /*off,*/ rw);

								if (offerType == OrderEnum.ASK) {
									AutomatedOrder automatedOrder = new AutomatedOrder (off) {
										taker_gets = off?.taker_pays?.DeepCopy (),
										taker_pays = off?.taker_gets?.DeepCopy ()
									};
									tradeWindow.SetAutomatedBuyOffer (automatedOrder);
								} else {
									tradeWindow.SetAutomatedBuyOffer (off);
								}

								//tradeWindow.SetAutomatedOffer (off, offerType);


							};

							#endregion


							#region sell
							Gtk.MenuItem sell = new MenuItem (Program.darkmode ? "<b>Prepare a <span fgcolor=\"#FFAABB\">sell</span> order at this price</b>" : "<b>Prepare a <span fgcolor=\"red\">sell</span> order at this price</b>");
							sell.Show ();
							sellMenu.Add (sell);

							menulabel = (Label)sell.Child;
							menulabel.UseMarkup = true;

							sell.ButtonPressEvent += (object obj, ButtonPressEventArgs buttonPressEventArgs) => {
#if DEBUG
								if (DebugIhildaWallet.OrderBookTableWidget) {
									Logging.WriteLog ("sell selected");
								}
#endif

								TradeWindow tradeWindow = ShowTradeWindow (/*off,*/ rw);
								if (offerType == OrderEnum.BID) {
									AutomatedOrder automatedOrder = new AutomatedOrder (off);
									automatedOrder.taker_gets = off.taker_pays.DeepCopy ();
									automatedOrder.taker_pays = off.taker_gets.DeepCopy ();
									tradeWindow.SetSellOffer (automatedOrder);
								} else {
									tradeWindow.SetSellOffer (off);
								}


							};
							#endregion


							#region casssell

							Gtk.MenuItem casssell = new MenuItem (Program.darkmode ? "<b>Cascade <span fgcolor=\"#FFAABB\">sell</span> orders begining at this price</b>" : "<b>Cascade <span fgcolor=\"red\">sell</span> orders begining at this price</b>");
							casssell.Show ();
							sellMenu.Add (casssell);

							menulabel = (Label)casssell.Child;
							menulabel.UseMarkup = true;

							casssell.ButtonPressEvent += (object obj, ButtonPressEventArgs buttonPressEventArgs) => {
#if DEBUG
								if (DebugIhildaWallet.OrderBookTableWidget) {
									Logging.WriteLog ("casssell selected");
								}
#endif

								TradeWindow tradeWindow = ShowTradeWindow (/*off,*/ rw);
								if (offerType == OrderEnum.BID) {
									AutomatedOrder automatedOrder = new AutomatedOrder (off) {
										taker_gets = off?.taker_pays?.DeepCopy (),
										taker_pays = off?.taker_gets?.DeepCopy ()
									};
									tradeWindow.InitCascadedSellOffer (automatedOrder);
								} else {
									tradeWindow.InitCascadedSellOffer (off);
								}

								//tradeWindow.InitiateCascade (off, offerType);
								//tradeWindow.InitCascadedAskOffer (off);

							};
							#endregion

							#region autosell 

							Gtk.MenuItem autosell = new MenuItem (Program.darkmode ? "<b>Prepare an automated <span fgcolor=\"#FFAABB\">sell</span> order at this price</b>" : "<b>Prepare an automated <span fgcolor=\"red\">sell</span> order at this price</b>");
							autosell.Show ();
							sellMenu.Add (autosell);
							menulabel = (Label)autosell.Child;
							menulabel.UseMarkup = true;

							autosell.ButtonPressEvent += (object obj, ButtonPressEventArgs buttonPressEventArgs) => {
#if DEBUG
								if (DebugIhildaWallet.OrderBookTableWidget) {
									Logging.WriteLog ("autosell selected");
								}
#endif

								TradeWindow tradeWin = ShowTradeWindow (rw);
								if (offerType == OrderEnum.BID) {
									AutomatedOrder automatedOrder = new AutomatedOrder (off) {
										taker_gets = off.taker_pays.DeepCopy (),
										taker_pays = off.taker_gets.DeepCopy ()
									};
									tradeWin.SetAutomatedSellOffer (automatedOrder);
								} else {
									tradeWin.SetAutomatedSellOffer (off);
								}

								//tradeWin.SetAutomatedOffer (off, offerType);

							};


							menu.Popup ();
#endregion

						};
					}
					eb.Add ((Label)orderbookLabel);

					table.Attach ((EventBox)eb, (uint)x, (uint)(x + 1), (uint)y, (uint)(y + 1), AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Shrink, 0, 0);



					orderbookLabel.UseMarkup = true;
					labels [x, y] = orderbookLabel;

					//if (Debug.OrderBookTableWidget) {
					//labels[x,y].Text = " -- unset -- ";
					//}




				}
			}
#if DEBUG
			if (DebugIhildaWallet.OrderBookTableWidget) {
				Logging.WriteLog("create table end");
			}
#endif
			this.scrolledwindow1.AddWithViewport (vbox);
			vbox.Show ();
		}


		private TradeWindow ShowTradeWindow (/*Offer off, */RippleWallet rw)
		{
			
			

			//TradePair tradepair = null;
			/*
			if (this.offerType.Equals (OrderEnum.ASK)) {
				tradepair = new TradePair (off.taker_gets, off.taker_pays);
			} else {
				tradepair = new TradePair (off.TakerPays, off.TakerGets);
			}
			*/

			TradeWindow tradeWin = new TradeWindow (rw, this._TradePair);

			//string address = rw?.GetStoredReceiveAddress ();


			tradeWin.Show ();

			return tradeWin;



		}

		public void TakeOffers (IEnumerable<Offer> offers)
		{

#if DEBUG
			string method_sig = clsstr + nameof (TakeOffers) + DebugRippleLibSharp.left_parentheses + nameof (offers) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.OrderBookTableWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			RippleWallet rippleWallet = _rippleWallet;

			if (rippleWallet == null) {
				return; // TODO
			}

			if (offers == null) {
				return;
				// TODO
			}

			int count = offers.Count ();



			Offer first = offers.FirstOrDefault ();

			// populate opposing orders

			AutomatedOrder [] opposing = new AutomatedOrder [count];

			/*
			for (int i = 0; i < count; i++) {
				opposing [i] = AutomatedOrder.getOpposingOrder (off.ElementAt(i));
			}
			*/

			/*IEnumerable<AutomatedOrder> oppose = from Offer offer in off select AutomatedOrder.getOpposingOrder(offer); */


			if (first == null) {
				// TODO no offers to accept redundant 
				return;
			}

			// Copy the first offer over 
			Offer totalOffer = first.Copy ();


			totalOffer.taker_gets.amount = 0;
			totalOffer.taker_pays.amount = 0;

			for (int i = 0; i < count; i++) {

				Offer offer = offers.ElementAt (i);
				totalOffer.taker_gets.amount += offer.taker_gets.amount;
				totalOffer.taker_pays.amount += offer.taker_pays.amount;

				opposing [i] = AutomatedOrder.GetOpposingOrder (offer);
				opposing [i].Account = rippleWallet.GetStoredReceiveAddress ();
			}

			StringBuilder stringBuilder = new StringBuilder ();
			StringBuilder offerTypeName = new StringBuilder ();



			bool plural = count > 1;

			offerTypeName.Append (offerType == OrderEnum.ASK ? "ask" : "bid");
			if (plural) {
				offerTypeName.Append ("'s");
			}

			stringBuilder.Append ("Would you like to proceed to " + OrderSubmitWindow.GUIName +  " and accept the ");

			if (plural) {
				stringBuilder.Append (count.ToString ());
				stringBuilder.Append (" ");
			}


			stringBuilder.Append (offerTypeName);



			stringBuilder.Append (" to purchase ");
			if (plural) {
				stringBuilder.Append ("a total of ");
			}
			stringBuilder.Append (totalOffer.taker_gets.ToString ());
			stringBuilder.Append (" for ");
			if (plural) {
				stringBuilder.Append ("a total of ");
			}

			stringBuilder.Append (totalOffer.taker_pays.ToString ());



			String title = "Accept " + offerTypeName + "?";



			Boolean accepted = AreYouSure.AskQuestion(title, stringBuilder.ToString());


			#if DEBUG
			if (DebugIhildaWallet.OrderBookTableWidget) {
				Logging.WriteLog (method_sig + nameof (accepted) + DebugRippleLibSharp.equals + accepted.ToString());
			}

			if (!accepted) {
				return;
			}
#endif
			LicenseType licenseT = Util.LicenseType.TRADING;
			if (LeIceSense.IsLicenseExempt(first.taker_gets) || LeIceSense.IsLicenseExempt (first.taker_pays)) {
				licenseT = LicenseType.NONE;
			}


			OrderSubmitWindow order_preview = new OrderSubmitWindow (_rippleWallet, licenseT);
			order_preview.SetOrders (opposing);
			order_preview.Show ();

		}

		public void ClearTable ()
		{
			ClearTable (0, MAX_ORDERS);
		}

		public void ClearTable (int start, int end)
		{
			for (int x = 0; x < 3; x++) {
				this.labels [x, 0].Hide ();
			}

			for (int y = start; y < end + 1; y++) {
				ClearTable (y);
			}


		}

		public void ClearTable (int index)
		{
			if (this.labels == null) {
				return;
			}

			Gtk.Application.Invoke (
				delegate {

					if (this.labels == null) {
						return;
					}

					for (int row = 0; row < 3; row++) {
						Label label = this.labels [row, index];
						if (label != null) {
							label.Text = "";
							label.Hide ();
						};

					}
				}
			);
		}

		public void SetAsk (AutomatedOrder [] asks)
		{
			int count = asks.Length;


#if DEBUG
			String method_sig = 
				clsstr 
				+ nameof (SetAsk) 
				+ DebugRippleLibSharp.left_parentheses 
				+ nameof (AutomatedOrder) 
				+ DebugRippleLibSharp.array_brackets  
				+ nameof(asks) 
				+ DebugRippleLibSharp.right_parentheses;



			if (DebugIhildaWallet.OrderBookTableWidget) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}

#endif

			lock (orderLock) {
				this.offerType = OrderEnum.ASK;
				offers = asks.ToArray ();

				if (asks == null) {
#if DEBUG
					if (DebugIhildaWallet.OrderBookTableWidget) {
						Logging.WriteLog (method_sig + "asks == null, returning");
					}
#endif
					return;
				}

				// set num to count or max_order, which ever is least
				int num = (count < MAX_ORDERS) ? count : MAX_ORDERS;

				decimal sum = 0; // cumalative sum of base currency for each order

				for (int y = 0; y < MAX_ORDERS; y++) {

					if (y >= num) {
						
						ClearTable (y);
						continue;

					}
#if DEBUG
					if (DebugIhildaWallet.OrderBookWidget) {
						Logging.WriteLog (method_sig + "y = " + y.ToString ());
					}
#endif

					decimal sumad = asks [y].taker_gets.amount;
#if DEBUG
					if (DebugIhildaWallet.OrderBookWidget) {
						Logging.WriteLog (method_sig + "sumad = " + sumad.ToString ());
					}
#endif
					if (asks [y].taker_gets.IsNative) {
						sumad = sumad / 1000000;
#if DEBUG
						if (DebugIhildaWallet.OrderBookTableWidget) {
							Logging.WriteLog (method_sig + "TakerGets is native, sumad = " + sumad.ToString ());
						}
#endif
					}
					sum += sumad;
#if DEBUG
					if (DebugIhildaWallet.OrderBookTableWidget) {
						Logging.WriteLog (method_sig + "sum = " + sum.ToString ());
					}
#endif
					decimal summ = sum;
					SetAsk (asks [y], y + 1, summ);
				}
			}

		}

		public void SetAsk (AutomatedOrder ask, int index, decimal sum)
		{
#if DEBUG
			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append (clsstr);
			stringBuilder.Append (nameof (SetAsk));
			stringBuilder.Append (DebugRippleLibSharp.left_parentheses);
			stringBuilder.Append (nameof (AutomatedOrder));
			stringBuilder.Append (DebugRippleLibSharp.space_char);
			stringBuilder.Append (nameof (ask));
			stringBuilder.Append (DebugRippleLibSharp.equals);
			stringBuilder.Append (DebugIhildaWallet.ToAssertString (ask));
			stringBuilder.Append (DebugRippleLibSharp.comma);
			stringBuilder.Append (nameof (Int32));
			stringBuilder.Append (DebugRippleLibSharp.space_char);
			stringBuilder.Append (nameof (index));
			stringBuilder.Append (DebugRippleLibSharp.equals);
			stringBuilder.Append (index.ToString());
			stringBuilder.Append (DebugRippleLibSharp.comma);
			stringBuilder.Append (nameof (Decimal));
			stringBuilder.Append (DebugRippleLibSharp.space_char);
			stringBuilder.Append (nameof (sum));
			stringBuilder.Append (DebugRippleLibSharp.equals);
			stringBuilder.Append (sum.ToString ());
			stringBuilder.Append (DebugRippleLibSharp.right_parentheses);
			String method_sig = stringBuilder.ToString();
#endif
			if (ask == null) {
#if DEBUG
				if (DebugIhildaWallet.OrderBookTableWidget) {
					Logging.WriteLog (method_sig + "ask == null, returning");
				}
#endif
				return;
			}

			if (index < 1) {
#if DEBUG
				if (DebugIhildaWallet.OrderBookTableWidget) {
					Logging.WriteLog (method_sig + "index < 1, returning");
				}
#endif
				return;
			}

			if (index > MAX_ORDERS) {
#if DEBUG
				if (DebugIhildaWallet.OrderBookTableWidget) {
					Logging.WriteLog (method_sig + "index > MAXORDERS, returning");
				}
#endif
				return;
			}



			//Decimal price = (decimal)ask.taker_pays.amount;
			Decimal price = ask.TakerGets.GetNativeAdjustedPriceAt (ask.TakerPays);

#if DEBUG
			if (DebugIhildaWallet.OrderBookTableWidget) {
				Logging.WriteLog(method_sig + "price = " + price.ToString());
			}
#endif



			Decimal amnt = ask.taker_gets.amount;

			if (ask.taker_gets.IsNative) {
				amnt = amnt / 1000000; // adjust the native amount to make the gui consistent 


#if DEBUG
				if (DebugIhildaWallet.OrderBookTableWidget) {
					Logging.WriteLog (method_sig + "ask.TakerGets.isNative : amnt = " + amnt.ToString());
				}
#endif
			}

#if DEBUG
			if (DebugIhildaWallet.OrderBookTableWidget) {
				Logging.WriteLog( method_sig + "amnt = " + amnt.ToString());
			}
#endif

			/*


			if (ask.taker_pays.isNative) { // WHY RL? WHY !!!!! 
				price = price / 1000000; // million drops chage to DROPS_PER_NATIVE
				//amnt *= 1000000;

				#if DEBUG
				if (Debug.OrderBookTableWidget) {
					Logging.writeLog (method_sig + "ask.TakerPays.isNative : amnt = " + price.ToString() );
				}
				#endif
			}
			*/


			//price = amnt / price; 
#if DEBUG
			if (DebugIhildaWallet.OrderBookTableWidget) {
				Logging.WriteLog (method_sig + "after");
			}

#endif

			RippleWallet rw = _rippleWallet;

			string address = rw.GetStoredReceiveAddress ();

			bool equal = false;

			if (address != null) {
				equal = address.Equals (ask?.Account);
			}

			string prce = Base58.TruncateTrailingZerosFromString (price.ToString ());
			string amt = Base58.TruncateTrailingZerosFromString (amnt.ToString ());
			string sm = Base58.TruncateTrailingZerosFromString (sum.ToString ());
			if (equal) {
				prce = "<markup><span background='orchid' foreground='black'>" + prce + "</span></markup>";
				amt = "<markup><span background='orchid' foreground='black'>" + amt + "</span></markup>";
				sm = "<markup><span background='orchid' foreground='black'>" + sm + "</span></markup>";

			}

			Gtk.Application.Invoke (delegate {
				//Logging.writeLog("price = " + price.ToString());
				//Logging.writeLog("amnt = " + amnt.ToString());



				labels [0, index].Markup = prce;
				labels [0, index].TooltipText = ask?.Account ?? "";
				labels [0, index].Show ();

				labels [1, index].Markup = amt;
				labels [1, index].TooltipText = ask?.Account ?? "";
				labels [1, index].Show ();

				labels [2, index].Markup = sm;
				labels [2, index].TooltipText = ask?.Account ?? "";
				labels [2, index].Show ();


			});

		}

		/* already implemented
		public void ClearIndex (int index)
		{

			Gtk.Application.Invoke (delegate {

				labels [0, index].Markup = "";
				labels [0, index].TooltipText = "";
				labels [0, index].Hide ();

				labels [1, index].Markup = "";
				labels [1, index].TooltipText = "";
				labels [1, index].Hide ();

				labels [2, index].Markup = "";
				labels [2, index].TooltipText = "";
				labels [2, index].Hide ();
			});
		}*/

		public void SetBid (AutomatedOrder bid, int index, decimal sum)
		{
#if DEBUG
			String method_sig = clsstr + "setBid ( bid, index" + index.ToString() + ") : ";

#endif

			if (bid == null) {
				return;
			}

			if (index < 1 || index > MAX_ORDERS) {
				return;
			}

#if DEBUG
			if (DebugIhildaWallet.OrderBookTableWidget) {
				Logging.WriteLog (method_sig + "before");
			}

#endif


			//Decimal price = bid.TakerGets.amount;	//
			//Decimal price = (decimal)bid.taker_pays.amount;
			Decimal price = bid.TakerPays.GetNativeAdjustedPriceAt (bid.TakerGets);

			//Decimal amnt = bid.TakerPays.amount;	//
			Decimal amnt = bid.taker_gets.amount;
			if (bid.taker_gets.IsNative) {
				amnt = amnt / 1000000; // // 
			}

			/*
			if (bid.taker_pays.isNative) { // WHY RL? WHY !!!!! 
				price = price / 1000000; // million drops chage to DROPS_PER_NATIVE
				//amnt *= 1000000;

			}
			*/


#if DEBUG
			if (DebugIhildaWallet.OrderBookTableWidget) {
				Logging.WriteLog (method_sig + "after");
			}
#endif


			RippleWallet rw = _rippleWallet;
			string address = rw.GetStoredReceiveAddress ();

			if (address != null) {

			}
			bool equal = false;

			if (address != null) {
				//bid.Account.Equals(address);
				equal = address.Equals (bid?.Account);


			}


			string prce = Base58.TruncateTrailingZerosFromString (price.ToString ());
			string amt = Base58.TruncateTrailingZerosFromString (amnt.ToString ());
			string sm = Base58.TruncateTrailingZerosFromString (sum.ToString ());

			if (equal) {
				prce = "<markup><span background='orchid' foreground='black'>" + prce + "</span></markup>";
				amt = "<markup><span background='orchid' foreground='black'>" + amt + "</span></markup>";
				sm = "<markup><span background='orchid' foreground='black'>" + sm + "</span></markup>";

			}

			Gtk.Application.Invoke (delegate {
#if DEBUG
				if (DebugIhildaWallet.OrderBookTableWidget) {
					Logging.WriteLog("price = " + price.ToString());
					Logging.WriteLog("amnt = " + amnt.ToString());
				}
#endif
				labels [0, index].Markup = sm;
				labels [0, index].TooltipText = bid?.Account ?? "";
				labels [0, index].Show ();

				labels [1, index].Markup = amt;
				labels [1, index].TooltipText = bid?.Account ?? "";
				labels [1, index].Show ();

				labels [2, index].Markup = prce;
				labels [2, index].TooltipText = bid.Account ?? "";
				labels [2, index].Show ();
			});


		}

		public void SetBids (AutomatedOrder [] bids)
		{
#if DEBUG
			string method_sig = nameof (SetBids) + DebugRippleLibSharp.left_parentheses + nameof(AutomatedOrder) + DebugRippleLibSharp.array_brackets + nameof(bids) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.OrderBookTableWidget) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}
#endif
			lock (orderLock) {
				this.offers = bids;
				this.offerType = OrderEnum.BID;
				if (bids == null) {
					return;
				}



				int num = (bids.Length < MAX_ORDERS) ? bids.Length : MAX_ORDERS;
				decimal sum = 0;
				for (int y = 0; y < MAX_ORDERS; y++) {

					if (y >= num) {
						ClearTable (y);
						continue;
					}

					decimal sumad = bids [y].taker_gets.amount;
					if (bids [y].taker_gets.IsNative) {
						sumad = sumad / 1000000;
					}
					sum += sumad;
					decimal summ = sum;
					SetBid (bids [y], y + 1, summ);
				}

			}
		}

		public void SetTitle (string [] tits)
		{
#if DEBUG
			string method_sig = clsstr + nameof (SetTitle) + DebugRippleLibSharp.left_parentheses + nameof (String) + DebugRippleLibSharp.array_brackets + nameof (tits) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.OrderBookTableWidget) {
				Logging.WriteLog ( method_sig + DebugRippleLibSharp.begin + " : tits = ", tits);
			}
#endif


			for (int x = 0; x < 3; x++) {
				int xx = x;
				Gtk.Application.Invoke (
					delegate {
#if DEBUG
						string event_sig = method_sig + DebugIhildaWallet.gtkInvoke;
						if (DebugIhildaWallet.OrderBookTableWidget) {
							Logging.WriteLog(event_sig + DebugRippleLibSharp.begin);
						}
#endif
						Label l = labels [xx, 0];
						l.Show ();
						l.Text = tits [xx];
						l.UseMarkup = true;
					}
				);
			}

			//labels[0,0].Text = "<b><u>Ask Price</u></b>";
			//labels[1,0].Text = "<b><u>Size</u></b>";
			//labels[2,0].Text = "<b><u>Sum</u></b>"



		}

		//public OrderBookLabel getFromPosition( double xx, double yy) {
		//table.
		//}

		public void SetRippleWallet (RippleWallet rippleWallet)
		{
			this._rippleWallet = rippleWallet;
		}

		public TradePair _TradePair {
			get;
			set;
		}

		private RippleWallet _rippleWallet = null;

		private OrderEnum offerType;

		public object orderLock = new object ();
		public Offer [] offers = null;


		private static readonly int MAX_ORDERS = 200; // the number of orders to show as top of orderbook

#if DEBUG
		private static readonly string clsstr = nameof (OrderBookTableWidget) + DebugRippleLibSharp.colon;
#endif

	}
}

