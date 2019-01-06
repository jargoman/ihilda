using System;
using System.Threading;
using System.Threading.Tasks;
using Gtk;

using IhildaWallet.Splashes;

using IhildaWallet.Util;
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


			tpm = new TradePairManager ();

			UpdateUI ();



		}


		public void ViewDepthChart ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (ViewDepthChart) + DebugRippleLibSharp.both_parentheses;
#endif

			RippleWallet rippleWallet = WalletManager.GetRippleWallet ();
			bool shouldContinue = LeIceSense.DoTrialDialog (rippleWallet, LicenseType.TRADING);
			if (!shouldContinue) {
				return;
			}

			TradePair tp = TradePairManager.SelectedTradePair;

			if (tp == null) {
				return;
			}

			DepthChartWidget dcw = null;
			DepthChartWindow dcwin = null;

			ManualResetEvent manualResetEvent = new ManualResetEvent (false);
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

			if (dcw == null) {
				return;
			}
			dcw.UpdateBooks (new CancellationToken ());

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

			bool shouldContinue = LeIceSense.DoTrialDialog (rippleWallet, LicenseType.TRADING);
			if (!shouldContinue) {
				return;
			}

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


			TradePair oldtp = TradePairManager.SelectedTradePair;

			if (oldtp == null) {
				MessageDialog.ShowMessage ("You must first select a tradepair to edit.");
				return;
			}

			TradePair newtp = TradePairCreateDialog.DoDialog (oldtp);

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

		public void Removetp ( object sender, EventArgs args ) {
			TradePair tp = TradePairManager.SelectedTradePair;

			if (tp == null) {
				return;
			}
				
			bool sure = AreYouSure.AskQuestion ("Remove TradePair", "Are you sure you would like to remove this tradepair?");
			if (!sure) { // lol
				return;
			}

			tpm.RemoveTradePair (tp);
			tpm.SaveTradePairs ();

			UpdateUI ();
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


			bool shouldContinue = LeIceSense.DoTrialDialog (rw, LicenseType.TRADING);
			if (!shouldContinue) {
				return;
			}

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

