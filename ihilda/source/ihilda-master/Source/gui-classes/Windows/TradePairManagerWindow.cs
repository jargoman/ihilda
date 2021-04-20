using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using IhildaWallet.Splashes;
using IhildaWallet.Util;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Transactions;
using RippleLibSharp.Trust;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public partial class TradePairManagerWindow : Gtk.Window
	{
		public TradePairManagerWindow () :
			base (Gtk.WindowType.Toplevel)
		{
			CurrentInstance = this;

			this.Build ();

			if (this.tradepairtree1 == null) {
				this.tradepairtree1 = new TradePairTree ();
				this.tradepairtree1.Show ();
				vbox4.Add (this.tradepairtree1);

			}

			this.Destroyed += OnDestroy;

			this.newtpbutton.Clicked += NewTradePair;

			this.editbutton.Clicked += EditTradePair;

			this.vieworderbookbutton.Clicked += (sender, e) => Task.Run ((System.Action)ViewOrderBook);

			this.depthchartbutton.Clicked += delegate {
				Task.Run ((System.Action)ViewDepthChart);
			};

			this.removetpbutton.Clicked += Removetp;

			this.nsbutton.Clicked += Networksetting;

			this.tradeButton.Clicked += (sender, e) => {

#if DEBUG
				String event_sig = clsstr + "tradebutton Clicked : ";
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog (event_sig + DebugRippleLibSharp.beginn);
				}
#endif





				Task.Run ((System.Action)Trade);

			};

			trollbutton.Clicked += Trollbutton_Clicked;


			this.generatebutton.Clicked += (sender, e) => {

				Task.Run(delegate {
					RippleWallet rippleWallet = WalletManager.GetRippleWallet ();

					if (rippleWallet == null) {
						MessageDialog.ShowMessage ("Select a wallet", "You must select a wallet first before generating pairs");
						return;

					}


					var ni = Networking.NetworkController.GetNetworkInterfaceNonGUIThread ();

					var sure = AreYouSure.AskQuestionNonGuiThread (
						"Generate Pairs",
						"This wizard will create tradepairs based on the trustlines for account "
						+ rippleWallet.GetStoredReceiveAddress ());


					if (!sure) {
						return;
					}

					// todo integrate cancel token
					RippleLibSharp.Trust.TrustLine [] lines =
						AccountLines.GetTrustLines (
							rippleWallet.GetStoredReceiveAddress (),
							ni,
							    new CancellationTokenSource ().Token
			    		);


					if (lines == null) {

						// TODO alert user
						return;
					}


					if (!lines.Any()) {

						// TODO alert user
						return;
					}

		    			lines = lines.Where((arg) => int.Parse(arg.limit) > 0).Select((arg) =>  arg ).ToArray(); 
					List<TradePair> pears = new List<TradePair> ();


					for (int b = 0; b < lines.Length; b++) {
						for (int c = 0; c < lines.Length; c++) {
							if (b != c) {


								pears.Add (new TradePair () { 
									Currency_Base = new RippleCurrency(0, lines[b].account, lines[b].currency),
				    					Currency_Counter = new RippleCurrency(0, lines[c].account, lines[c].currency)
								});
							}
						}

						pears.Add (new TradePair () {
							Currency_Base = new RippleCurrency (0), // xrp
							Currency_Counter = new RippleCurrency (0, lines [b].account, lines [b].currency)

						});

						pears.Add (new TradePair () {
							Currency_Base = new RippleCurrency (0, lines [b].account, lines [b].currency),
							Currency_Counter = new RippleCurrency (0) // XRP
						});
					}




					foreach(TradePair obj in pears) {
						tpm.AddTradePair (obj);
					};



					tpm.SaveTradePairs ();

					Application.Invoke ((s, ev) => {
						UpdateUI ();
					});

				});



				
			};

			tpm = new TradePairManager ();

			UpdateUI ();



		}

		void Trollbutton_Clicked (object sender, EventArgs e)
		{
			RippleWallet rippleWallet = WalletManager.GetRippleWallet ();

			TradePair tp = TradePairManager.SelectedTradePair;

			if (tp == null) {
				// TODO update user ui
				return;
			}

			TrollBoxWindow trollBoxWindow = new TrollBoxWindow (tp.Currency_Base.currency, tp.Currency_Counter.currency);
			trollBoxWindow.Show ();
		}


		public void ViewDepthChart ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (ViewDepthChart) + DebugRippleLibSharp.both_parentheses;
#endif

			RippleWallet rippleWallet = WalletManager.GetRippleWallet ();

			// make trading free

			TradePair tp = TradePairManager.SelectedTradePair;

			if (tp == null) {
				// TODO update user ui
				return;
			}


	    		/*
			bool hasPro = LeIceSense.DoTrialDialog (rippleWallet, LicenseType.TRADING);
	    		*/


			DepthChartWidget dcw = null;
			DepthChartWindow dcwin = null;
			using (ManualResetEvent manualResetEvent = new ManualResetEvent (false)) {
				manualResetEvent.Reset ();
				Application.Invoke (
				    delegate {
					    dcwin = new DepthChartWindow (rippleWallet, tp);
					    dcwin.Show ();

					    dcw = dcwin.GetWidget ();

					    manualResetEvent.Set ();



				    }
				);
				manualResetEvent.WaitOne ();
			}

			if (dcw == null) {
				return;
			}

			//dcw.UpdateBooksOnce ();

			dcw.InitBooksUpdate ();

			Gtk.Application.Invoke ( delegate {
				dcwin.Show ();
				dcw.Show ();

			});

		}

		public void ViewOrderBook ()
		{

			RippleWallet rippleWallet = WalletManager.GetRippleWallet ();
			if (rippleWallet == null) {
				return;
			}


	    		/*
			bool shouldContinue = LeIceSense.DoTrialDialog (rippleWallet, LicenseType.TRADING);
			if (!shouldContinue) {
				return;
			}
			*/    

			TradePair tp = TradePairManager.SelectedTradePair;
			if (tp == null) {
				return;
			}

			Application.Invoke (
				delegate {

					OrderBookWindow obw = new OrderBookWindow (rippleWallet);
					Task.Run ( delegate {

						obw.SetTradePair (tp);

					});


				}
			);

		}




		public void NewTradePair (object sender, EventArgs args)
		{

			TradePair tp = TradePairCreateDialog.DoDialog ();

			if (tp == null) {
				return;
			}

			tpm.AddTradePair (tp);
			tpm.SaveTradePairs ();
			UpdateUI ();

		}


		public void EditTradePair (object sender, EventArgs args)
		{

#if DEBUG
			string method_sig = clsstr + nameof (EditTradePair) + DebugRippleLibSharp.both_parentheses;
#endif

			Gtk.Application.Invoke (
				delegate {

					TradePair oldtp = TradePairManager.SelectedTradePair;

					if (oldtp == null) {
						MessageDialog.ShowMessage ("You must first select a tradepair to edit.");
						return;
					}
				
					RippleWallet rippleWallet = WalletManager.GetRippleWallet ();

					TradePair newtp = TradePairCreateDialog.DoDialog (oldtp, rippleWallet);

					if (newtp == null) {
						//MessageDialog.ShowMessage ("");
						return;
					}

					if (newtp.Currency_Base == null) {
						return;
					}

					if (newtp.Currency_Base == null) {
						return;
					}

					tpm.RemoveTradePair (oldtp);
					tpm.AddTradePair (newtp);
					tpm.SaveTradePairs ();



					UpdateUI ();
				}
			);


		}

		public void Removetp ( object sender, EventArgs args ) {
			TradePair tp = TradePairManager.SelectedTradePair;

			if (tp == null) return;

			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.AppendLine ("Are you sure you would like to remove this tradepair?");


			if (tp.Currency_Base != null) {
				stringBuilder.Append ("\nBase : ");
				stringBuilder.Append (tp.Currency_Base.currency);
				if (!tp.Currency_Base.IsNative) {
					stringBuilder.Append (":");
					stringBuilder.Append (tp.Currency_Base.issuer);
				}

			}

			if (tp.Currency_Counter != null) {
				stringBuilder.Append ("\nCounter : ");
				stringBuilder.Append (tp.Currency_Counter.currency);
				if (tp.Currency_Counter.IsNative) {
					stringBuilder.Append (":");
					stringBuilder.Append (tp.Currency_Counter.issuer);
				}

			}
			bool sure = AreYouSure.AskQuestion ("Remove TradePair", stringBuilder.ToString());
			if (!sure) { // lol
				return;
			}

			tpm.RemoveTradePair (tp);
			tpm.SaveTradePairs ();

			Application.Invoke ( delegate {

				UpdateUI ();
			});

		}

		public void Networksetting ( object sender, EventArgs args ) {

			NetworkSettingsDialog.ShowDialog ();
		}


		public void UpdateUI () {



			if (tradepairtree1 == null) {
				// TODO DEBUG
				return;
			}


			if (tpm == null || tpm.pairs == null || tpm.pairs.Count == 0) {
				tradepairtree1.ClearValues ();
				return; // with error ect
			}

			tradepairtree1.ClearValues ();

			if (tpm.pairs.Count > 0) {
				tradepairtree1.SetValues (tpm.pairs.Values);
			} else {
				tradepairtree1.ClearValues ();
			}
		}


		public void Trade ()
		{
#if DEBUG
			String method_sig = clsstr + nameof (Trade) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.WalletManagerWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif
			LoadingWindow loadingwin = null;

			RippleWallet rw = WalletManager.GetRippleWallet();
			if (rw ==null) {
#if DEBUG
				if (DebugIhildaWallet.TradePairManagerWindow) {
					Logging.WriteLog (method_sig + "rw == null, returning");
				}
#endif
				return;
			}



#if DEBUG
			if (DebugIhildaWallet.TradePairManagerWindow) {
				Logging.WriteLog ( method_sig + DebugIhildaWallet.ToAssertString(rw) );
			}
#endif


	    		/*
			bool shouldContinue = LeIceSense.DoTrialDialog (rw, LicenseType.TRADING);
			if (!shouldContinue) {
				return;
			}
			*/    

			TradePair tp = TradePairManager.SelectedTradePair;
			if (tp == null) {
				// TODO
				return;
			}


			Gtk.Application.Invoke (delegate {
#if DEBUG
				string event_sig = method_sig + DebugIhildaWallet.gtkInvoke;
				if (DebugIhildaWallet.TradePairManagerWindow) {
					Logging.WriteLog( event_sig + DebugRippleLibSharp.begin );
				}
#endif
				loadingwin = new LoadingWindow();
				loadingwin.Show();
			});




			////// everything above is null checks and setting loading splash
			/// TradeWindow.currentInstance.SetTradePair (tp);
			Task<TradeWindow> t1 = TradeWindow.InitGUI(rw, tp);

			/*new Task (
				delegate {
					// if a trade window isn't already created then create one ASYNC
					if (TradeWindow.currentInstance == null) {
						EventWaitHandle tradeWaitHandle = new ManualResetEvent( true );
						tradeWaitHandle.Reset ();

						Gtk.Application.Invoke (delegate {
							new TradeWindow ();
							tradeWaitHandle.Set();

						}
						);
						//tradeWaitHandle.WaitOne (20000);
						tradeWaitHandle.WaitOne ();
					}
				}
			);*/



			Task[] tsks = { 
				t1
			};



			Task.WaitAll (tsks);



			TradeWindow tradeWindow = t1.Result;

			/*
			if (WalletManagerWindow.currentInstance != null) {
				if (Debug.WalletManagerWidget) {
					Logging.writeLog (method_sig + "hiding wallet manager");
				}
				Application.Invoke (delegate {
					WalletManagerWindow.currentInstance.Hide ();
				});

			}
			*/


			if (tradeWindow != null) {
				//TradeWindow.currentInstance.Show ();  // todo, show vs showall?
				//TradeWindow.currentInstance.ShowAll();




				//tradeWindow.SetAccount (rw);


				Application.Invoke ((sender, e) => tradeWindow.Reshowall ());
			}

			Gtk.Application.Invoke (delegate {
				loadingwin.Hide();
				loadingwin.Destroy();
				loadingwin = null;

			});


			/*
			if (PaymentWindow.currentInstance != null  ) {
#if DEBUG
				if (DebugIhildaWallet.NoHideWindows) {
#endif
					Application.Invoke ((sender, e) => PaymentWindow.currentInstance.Hide ());

#if DEBUG
				}
#endif
			}
			*/
		}


		public static TradePairManagerWindow CurrentInstance {
			get;
			set;
		}


		protected void OnDestroy (object sender, EventArgs a)
		{
			//if (this.networksettings1!=null) {
			//	this.networksettings1.saveSettings ();
			//}


			TradePairManagerWindow.CurrentInstance = null;

			//MainClass.quitRequest(sender, a);

			   //(sender, a);


		}

		public TradePairManager tpm = null;

#if DEBUG
		private string clsstr = nameof (TradePairManagerWindow) + DebugRippleLibSharp.colon;
#endif
	}
}

