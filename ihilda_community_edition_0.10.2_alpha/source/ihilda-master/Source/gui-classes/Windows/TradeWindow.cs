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

				notebook.AppendPage (buywidget1, label29);
			}

			if (this.cascadedbuywidget1 == null) {
				cascadedbuywidget1 = new CascadedBuyWidget ();
				cascadedbuywidget1.Show ();

				if (label36 == null) {
					label36 = new Label ("<b>Cascaded Buy</b>");
				}

				notebook.AppendPage (cascadedbuywidget1, label36);
			}

			if (automatedbuywidget1 == null) {
				automatedbuywidget1 = new AutomatedBuyWidget ();
				automatedbuywidget1.Show ();

				if (label39 == null) {
					label39 = new Label ("<b>Automated Buy</b>");
				}

				notebook.AppendPage (automatedbuywidget1, label39);
			}

			if (sellwidget1 == null) {
				sellwidget1 = new SellWidget ();
				sellwidget1.Show ();

				if (selllabel == null) {
					selllabel = new Label ("<b>Sell</b>");
				}
				notebook.AppendPage (sellwidget1, selllabel);
			}

			if (cascadedsellwidget2 == null) {
				cascadedsellwidget2 = new CascadedSellWidget ();
				cascadedsellwidget2.Show ();

				if (label57 == null) {
					label57 = new Label ("<b>Cascaded Sell</b>");
				}

				notebook.AppendPage (cascadedbuywidget1, label57);
			}

			if (automatedsellwidget1 == null) {
				automatedsellwidget1 = new AutomatedSellWidget ();
				automatedsellwidget1.Show ();

				if (autoselltablabel == null) {
					autoselltablabel = new Label ("<b>Automated Sell</b>");
				}

				notebook.AppendPage (automatedsellwidget1, autoselltablabel);
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


			this.notebook.CurrentPage = 0;


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
			this.cascadedsellwidget2.TradePairInstance = tradepair;

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


				this.notebook.Page = 0;
				this.notebook.CurrentPage = 0;
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

			if (this.cascadedsellwidget2 != null) {
				this.cascadedsellwidget2.SetRippleWallet (rw);
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

		public void Update ()
		{
			NetworkInterface ni = NetworkController.CurrentInterface;
			if (ni == null) {
				return;
			}

			Task<Response<BookOfferResult>> task =
				BookOffers.GetResult (
					this._TradePairInstance.Currency_Base,
					this._TradePairInstance.Currency_Counter,
					1,
					ni
				);

			if (task == null) {
				return;
			}

			task.Wait ();

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
				SetAskOffer (off);
			} else {
				SetBidOffer (off);
			}
		}


		public void SetAutomatedOffer(Offer off, OrderEnum oe) {
			if (off == null) {
				return;
			}

			if (oe == OrderEnum.ASK) {
				SetAutomatedAskOffer (off);
			} else {
				SetAutomatedBidOffer (off);
			}
		}

		public void InitiateCascade (Offer off, OrderEnum offerType) {
			if (off == null) {
				return;
			}

			if (offerType == OrderEnum.ASK) {
				InitCascadedAskOffer (off);
			} else {
				InitCascadedBidOffer (off);
			}
		}

		public void InitCascadedAskOffer (Offer off) {
			Gtk.Application.Invoke ( delegate {
				this.notebook.Page = (int) NoteBookPages.cascadedSell;
				this.notebook.CurrentPage = (int) NoteBookPages.cascadedSell;
			});

			this.cascadedsellwidget2.SetOffer (off);
		}

		public void InitCascadedBidOffer (Offer off) {
			Gtk.Application.Invoke ( delegate {
				this.notebook.Page = (int) NoteBookPages.cascadedBuy;
				this.notebook.CurrentPage = (int) NoteBookPages.cascadedBuy;
			});
			this.cascadedbuywidget1.SetOffer (off);
			//this.CascadedbuyWidget1.SetOffer (off);
		}
		

		public void SetBidOffer (Offer off) {
			this.buywidget1.SetOffer(off);
			Gtk.Application.Invoke ( delegate {
				this.notebook.Page = (int) NoteBookPages.buy;
				this.notebook.CurrentPage = (int) NoteBookPages.buy;
			});


		}

		public void SetAskOffer (Offer off) {
			
			this.sellwidget1.SetOffer(off);

			Gtk.Application.Invoke ( delegate {
				this.notebook.Page = (int) NoteBookPages.sell;
				this.notebook.CurrentPage = (int) NoteBookPages.sell;
			});

		}

		public void SetAutomatedAskOffer ( Offer off ) {

			Gtk.Application.Invoke ( delegate {
				this.notebook.Page = (int) NoteBookPages.automatedSell;
				this.notebook.CurrentPage = (int) NoteBookPages.automatedSell;
			});

			this.automatedsellwidget1.SetOffer (off);
		}

		public void SetAutomatedBidOffer ( Offer off ) {
			Gtk.Application.Invoke ( delegate {
				this.notebook.Page = (int) NoteBookPages.automatedBuy;
				this.notebook.CurrentPage = (int) NoteBookPages.automatedBuy;
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
				EventWaitHandle wh = new ManualResetEvent (true);
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
						Logging.ReportException(method_sig, e);
						#endif
					}

					finally {
						wh.Set();
					}
				});
				wh.WaitOne ();
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
			sell = 3,
			cascadedSell = 4,
			automatedSell = 5

		}
	}



}
