using System;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using RippleLibSharp.Util;
using RippleLibSharp.Transactions;
using RippleLibSharp.Commands.Stipulate;
using RippleLibSharp.Result;
using RippleLibSharp.Network;
using IhildaWallet.Networking;

namespace IhildaWallet
{
	public partial class TradeWindow : Gtk.Window
	{
		public TradeWindow (RippleWallet rippleWallet, TradePair tradePair) : 
				base(Gtk.WindowType.Toplevel)
		{
			//string method_sig = clsstr + "new : ";
			this.Hide ();
			this.Visible = false;
			//this.NoShowAll = true;

			this.Build ();

			if (this.walletswitchwidget2 == null) {
				this.walletswitchwidget2 = new WalletSwitchWidget ();
				this.walletswitchwidget2.Show ();
				vbox2.Add (walletswitchwidget2);
			}

			if (this.currencywidget1 == null) {
				this.currencywidget1 = new CurrencyWidget ();
				this.currencywidget1.Show ();
				table8.Attach (currencywidget1, 0, 1, 0, 1);
			}

			if (this.currencywidget2 == null) {
				this.currencywidget2 = new CurrencyWidget ();
				this.currencywidget2.Show ();
				table8.Attach (currencywidget2, 0, 1, 1, 2);
			}

			if (this.spreadwidget1 == null) {
				this.spreadwidget1 = new SpreadWidget ();
				this.spreadwidget1.Show ();
				hbox3.Add (spreadwidget1);
			}

			if (this.buywidget1 == null) {
				buywidget1 = new BuyWidget ();
				buywidget1.Show ();

				if (label29 == null) {
					label29 = new Label ("<b>Buy</b>");
				}

				buynotebook.AppendPage (buywidget1, label29);
			}

			if (this.cascadedbuywidget1 == null) {
				cascadedbuywidget1 = new CascadedBuyWidget ();
				cascadedbuywidget1.Show ();

				if (label36 == null) {
					label36 = new Label ("<b>Cascaded Buy</b>");
				}

				buynotebook.AppendPage (cascadedbuywidget1, label36);
			}

			if (automatedbuywidget1 == null) {
				automatedbuywidget1 = new AutomatedBuyWidget ();
				automatedbuywidget1.Show ();

				if (label45 == null) {
					label45 = new Label ("<b>Automated Buy</b>");
				}

				buynotebook.AppendPage (automatedbuywidget1, label45);
			}

			if (sellwidget1 == null) {
				sellwidget1 = new SellWidget ();
				sellwidget1.Show ();

				if (label75 == null) {
					label75 = new Label ("<b>Sell</b>");
				}
				sellnotebook.AppendPage (sellwidget1, label75);
			}

			if (cascadedsellwidget1 == null) {
				cascadedsellwidget1 = new CascadedSellWidget ();
				cascadedsellwidget1.Show ();

				if (label75 == null) {
					label75 = new Label ("<b>Cascaded Sell</b>");
				}

				sellnotebook.AppendPage (cascadedbuywidget1, label75);
			}

			if (automatedsellwidget1 == null) {
				automatedsellwidget1 = new AutomatedSellWidget ();
				automatedsellwidget1.Show ();

				if (label99 == null) {
					label99 = new Label ("<b>Automated Sell</b>");
				}

				sellnotebook.AppendPage (automatedsellwidget1, label99);
			}

			this.SetTradePair (tradePair);

			walletswitchwidget2.WalletChangedEvent += (object source, WalletChangedEventArgs eventArgs) => {
				Task.Run (
					delegate {

						RippleWallet rw = eventArgs.GetRippleWallet ();
						this.SetAccountChildren (rw);


					}
				);

			};




			this.Visible = false;


			//this.addressLabel.Selectable = true;

			//while(Gtk.Application.EventsPending())
			//	Gtk.Application.RunIteration();


			this.masternotebook.CurrentPage = 0;
			this.sellnotebook.CurrentPage = 0;
			this.sellnotebook.CurrentPage = 0;

			this.WindowPosition = Gtk.WindowPosition.Center;
			//Gdk.Color col = new Gdk.Color(3, 3, 56);
			//this.eventbox4.ModifyBg(StateType.Normal, col);

			/*
			this.backtowalletbutton.Clicked += ( object sender, EventArgs e ) => {

				// comment old 
				if (WalletManagerWindow.currentInstance != null) {
					#if DEBUG
					if (!DebugIhildaWallet.NoHideWindows) {
					#endif
					this.Hide();
					#if DEBUG
					}
					#endif
					WalletManagerWindow.currentInstance.Show();
				}

				else {
					// todo debug
				}



			};
			*/

			//this.NoShowAll = false;

			walletswitchwidget2.SetRippleWallet (rippleWallet);


			if (ProgramVariables.darkmode) {
				label73.Markup = "<span fgcolor=\"chartreuse\"size=\"x-large\"><b>Buy</b></span>";
				label116.Markup = "<span fgcolor = \"red\" size = \"x-large\"><b>Sell</b></span>";
			}

	    		spreadwidget1.OnBid += Spreadwidget1_Clicked;

			spreadwidget1.OnAsk += Spreadwidget1_Clicked;

			spreadwidget1.OnSpread += Spreadwidget1_Clicked;

			currencywidget1.OnLabelClicked += (cur) => {
				// base
				sellwidget1.SetAmountMax ();
				automatedsellwidget1.SetAmountMax ();
				cascadedsellwidget1.SetAmountMax ();
			};

			currencywidget2.OnLabelClicked += (cur) => {
				// counter
				buywidget1.SetAmountMax ();
				automatedbuywidget1.SetAmountMax ();
				cascadedbuywidget1.SetAmountMax ();
			};

		}

		void Spreadwidget1_Clicked (decimal? price)
		{
			if (price == null) {
				return;
			}
			Decimal p = (Decimal)price;
			this.buywidget1.SetPrice (p);
			sellwidget1.SetPrice (p);

			cascadedbuywidget1.SetPrice (p);
			cascadedsellwidget1.SetPrice (p);

			automatedbuywidget1.SetPrice (p);
			automatedsellwidget1.SetPrice (p);

		}


		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			//if (this.networksettings1!=null) {
			//	this.networksettings1.saveSettings ();
			//}




			//MainClass.quitRequest(sender, a);

			 GoBack();   //(sender, a);


		}

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private TradePair _TradePairInstance = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		private void SetTradePair (TradePair tradepair) {
			
			_TradePairInstance = tradepair;
			if (tradepair == null) {
				// TODO setting to null is a l
				return;
			}

			#if DEBUG
			string method_sig = clsstr + nameof (SetTradePair) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.TradeWindow) {
				
				Logging.WriteLog(method_sig + "tp != null");
			}
			#endif

			if (tradepair?.Currency_Base?.currency != null) {
				#if DEBUG
				if (DebugIhildaWallet.TradeWindow) {
					Logging.WriteLog(method_sig  + "base currency not null");
				}
				#endif
				this.currencywidget1.SetCurrency (tradepair.Currency_Base.currency);
			}

			if (tradepair?.Currency_Counter?.currency != null) {
				#if DEBUG
				if (DebugIhildaWallet.TradeWindow) {
					Logging.WriteLog(method_sig + "counter currency not null");
				}
				#endif
				this.currencywidget2.SetCurrency (tradepair.Currency_Counter.currency);
			}

			this.buywidget1.TradePairInstance = tradepair; // auto sets UI
			this.sellwidget1.TradePairInstance = tradepair;
			this.cascadedbuywidget1.TradePairInstance = tradepair;
			this.cascadedsellwidget1.TradePairInstance = tradepair;

			this.automatedbuywidget1.TradePairInstance = tradepair;
			this.automatedsellwidget1.TradePairInstance = tradepair;

			this.spreadwidget1.Set (_TradePairInstance);
		}


		private void SetAccountChildren (RippleWallet rw) {


			#if DEBUG
			string method_sig = clsstr + nameof (SetAccountChildren) + DebugRippleLibSharp.left_parentheses + nameof (RippleWallet) + DebugRippleLibSharp.space_char + nameof (rw) + DebugRippleLibSharp.equals + DebugIhildaWallet.ToAssertString(rw) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.TradeWindow) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
			#endif


			#region gtk
			Gtk.Application.Invoke (delegate {
				#if DEBUG
				string event_sig = method_sig + DebugIhildaWallet.gtkInvoke;
				if (DebugIhildaWallet.TradeWindow) {
					Logging.WriteLog( event_sig + DebugRippleLibSharp.begin);
				}
#endif


				this.masternotebook.Page = 0;
				this.masternotebook.CurrentPage = 0;
			});
			#endregion


			if (this.buywidget1 != null) {
				this.buywidget1.SetRippleWallet (rw);
			}

			if (this.cascadedbuywidget1 != null) {
				this.cascadedbuywidget1.SetRippleWallet (rw);
			}

			if (this.automatedbuywidget1 != null) {
				this.automatedbuywidget1.SetRippleWallet (rw);
			}

			if (this.sellwidget1 != null) {
				this.sellwidget1.SetRippleWallet (rw);
			}

			if (this.cascadedsellwidget1 != null) {
				this.cascadedsellwidget1.SetRippleWallet (rw);
			}

			if (this.automatedsellwidget1 != null) {
				this.automatedsellwidget1.SetRippleWallet (rw);
			}

			if (this.currencywidget1 != null) {
				this.currencywidget1.SetRippleWallet (rw.GetStoredReceiveAddress ());
			}

			if (this.currencywidget2 != null) {
				this.currencywidget2.SetRippleWallet (rw.GetStoredReceiveAddress ());
			}

		}

		public void Update (CancellationToken token)
		{

			// TODO I think this does nothing and called by nothing
			NetworkInterface ni = NetworkController.CurrentInterface;
			if (ni == null) {
				return;
			}

			Task<Response<BookOfferResult>> task =
				BookOffers.GetResult (
					this._TradePairInstance.Currency_Base,
					this._TradePairInstance.Currency_Counter,
					1,
					ni,
					token
				);

			if (task == null) {
				return;
			}

			task.Wait (token);

			Response<BookOfferResult> resp = task.Result;
			if (resp == null) {
				return;
			}
			if (resp.HasError ()) {
				return;
			}

			BookOfferResult bor = resp.result;
			if (bor == null) {
				return;
			}


		}


		public void SetOffer(Offer off, OrderEnum oe) {
			if (off == null) {
				return;
			}

			if (oe == OrderEnum.ASK) {
				SetSellOffer (off);
			} else {
				SetBuyOffer (off);
			}
		}


		public void SetAutomatedOffer(Offer off, OrderEnum oe) {
			if (off == null) {
				return;
			}

			if (oe == OrderEnum.ASK) {
				SetAutomatedSellOffer (off);
			} else {
				SetAutomatedBuyOffer (off);
			}
		}

		public void InitiateCascade (Offer off, OrderEnum offerType) {
			if (off == null) {
				return;
			}

			if (offerType == OrderEnum.ASK) {
				InitCascadedSellOffer (off);
			} else {
				InitCascadedBuyOffer (off);
			}
		}


		public void InitCascadedSellOffer (Offer off) {
			Gtk.Application.Invoke ( delegate {
				this.masternotebook.Page = 1;
				this.masternotebook.CurrentPage = 1;

				this.sellnotebook.Page = (int) NoteBookPages.cascadedSell;
				this.sellnotebook.CurrentPage = (int) NoteBookPages.cascadedSell;
			});

			this.cascadedsellwidget1.SetOffer (off);
		}

		public void InitCascadedBuyOffer (Offer off) {
			Gtk.Application.Invoke ( delegate {
				this.masternotebook.Page = 0;
				this.masternotebook.CurrentPage = 0;

				this.buynotebook.Page = (int) NoteBookPages.cascadedBuy;
				this.buynotebook.CurrentPage = (int) NoteBookPages.cascadedBuy;
			});
			this.cascadedbuywidget1.SetOffer (off);
			//this.CascadedbuyWidget1.SetOffer (off);
		}
		

		public void SetBuyOffer (Offer off) {
			this.buywidget1.SetOffer(off);
			Gtk.Application.Invoke ( delegate {
				this.masternotebook.Page = 0;
				this.masternotebook.CurrentPage = 0;

				this.buynotebook.Page = (int) NoteBookPages.buy;
				this.buynotebook.CurrentPage = (int) NoteBookPages.buy;
			});


		}

		public void SetSellOffer (Offer off) {
			
			this.sellwidget1.SetOffer(off);

			Gtk.Application.Invoke ( delegate {
				this.masternotebook.Page = 1;
				this.masternotebook.CurrentPage = 1;

				this.sellnotebook.Page = (int) NoteBookPages.sell;
				this.sellnotebook.CurrentPage = (int) NoteBookPages.sell;
			});

		}

		public void SetAutomatedSellOffer ( Offer off ) {

			Gtk.Application.Invoke ( delegate {
				this.masternotebook.Page = 1;
				this.masternotebook.CurrentPage = 1;


				this.sellnotebook.Page = (int) NoteBookPages.automatedSell;
				this.sellnotebook.CurrentPage = (int) NoteBookPages.automatedSell;
			});

			this.automatedsellwidget1.SetOffer (off);
		}

		public void SetAutomatedBuyOffer ( Offer off ) {
			Gtk.Application.Invoke ( delegate {
				this.masternotebook.Page = 0;
				this.masternotebook.CurrentPage = 0;


				this.buynotebook.Page = (int) NoteBookPages.automatedBuy;
				this.buynotebook.CurrentPage = (int) NoteBookPages.automatedBuy;
			});

			this.automatedbuywidget1.SetOffer (off);
		}



		private void GoBack ( ) // ( object sender, DeleteEventArgs a ) // 
		{

			if (IhildaWallet.Console.currentInstance!=null) {
				IhildaWallet.Console.currentInstance.SaveHistory();
			}

			this.Hide ();

			if (WalletManagerWindow.currentInstance != null) {





				WalletManagerWindow.currentInstance.Show();
			}

			else {
				// todo debug
			}
		}


		public void Reshowall ()
		{

			/*
			Widget[] children = this.Children;

			foreach (Widget child in children) {
				Logging.writeLog ( child.GetType().ToString() );
				child.Show();
				child.ShowAll();
			}
			*/


			Show ();
			//this.ShowAll();
			ShowNow ();


		}

		public static Task<TradeWindow> InitGUI (RippleWallet rw, TradePair tradePair) {
			return Task.Run (delegate {
				#if DEBUG
				string method_sig = clsstr + nameof (InitGUI) + DebugRippleLibSharp.both_parentheses;
#endif
				TradeWindow trdw = null;
				using (EventWaitHandle wh = new ManualResetEvent (true)) {
					wh.Reset ();
					Gtk.Application.Invoke (delegate {
						try {
#if DEBUG
							if (DebugIhildaWallet.Program) {
								Logging.WriteLog (
								    method_sig
								    + "Invoking " + nameof (TradeWindow) + " creation thread : "

								);
							}
#endif
							trdw = new TradeWindow (rw, tradePair);
							trdw.Hide ();
							//trdw.HideAll ();
							trdw.Visible = false;
#if DEBUG
							if (DebugIhildaWallet.Program) {
								Logging.WriteLog (method_sig + "creation complete");
							}
#endif
							wh.Set ();
						}

#pragma warning disable 0168
			catch (Exception e) {
#pragma warning restore 0168
#if DEBUG
							Logging.ReportException (method_sig, e);
#endif
						} finally {
							wh.Set ();
						}
					});
					wh.WaitOne ();
				}

				return trdw;
			});


		}

		//public static TradeWindow currentInstance;

		#if DEBUG
		private const string clsstr = nameof (TradeWindow) + DebugRippleLibSharp.colon;
		#endif

		//public const int buy
		private enum NoteBookPages {
			buy = 0,
			cascadedBuy = 1,
			automatedBuy = 2,
			sell = 0,
			cascadedSell = 1,
			automatedSell = 2

		}
	}



}

