using System;
using System.Threading;
using System.Threading.Tasks;
using Gtk;

using RippleLibSharp.Util;

namespace IhildaWallet
{
	public partial class TrustManagementWindow : Gtk.Window
	{
		public TrustManagementWindow (RippleWallet rippleWallet) :
				base (Gtk.WindowType.Toplevel)
		{

#if DEBUG
			string method_sig = clsstr + nameof (TrustManagementWindow) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.TrustManagementWindow) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			this.Hide ();
			this.Visible = false;
			//this.NoShowAll = true;

			this.Build ();

			if (walletswitchwidget3 == null) {
				walletswitchwidget3 = new WalletSwitchWidget ();
				walletswitchwidget3.Show ();
				hbox4.Add (walletswitchwidget3);
			}

			if (trustsetter2 == null) {
				trustsetter2 = new TrustSetter ();
				trustsetter2.Show ();
				notebook1.PrependPage (trustsetter2, new Label ("<b>Trust Set</b>") { UseMarkup = true });
			}

			if (accountlineswidget1 == null) {
				accountlineswidget1 = new AccountLinesWidget ();
				accountlineswidget1.Show ();
				notebook1.PrependPage (accountlineswidget1, new Label ("<b>Account Lines</b>") { UseMarkup = true });
			}
			//this.Visible = false;



#if DEBUG
			if (DebugIhildaWallet.TrustManagementWindow) {
				Logging.WriteLog ("build complete");
			}
#endif

			this.walletswitchwidget3.WalletChangedEvent += (object source, WalletChangedEventArgs eventArgs) => {
				this.SetChildrensWallets (eventArgs.GetRippleWallet ());
			};

			if (this.trustsetter2 != null) {
				this.trustsetter2.HideRipplingWidgets ();
			}

			this.DeleteEvent += OnDeleteEvent;

			//this.backbutton.Clicked += (sender, e) => Goback ();

			//Gdk.Color col = new Gdk.Color(3, 3, 56);
			//eventbox1.ModifyBg(StateType.Normal, col);


			this.SetRippleWallet (rippleWallet);
		}


		private void SetRippleWallet (RippleWallet rw)
		{
#if DEBUG
			String method_sig = clsstr +  nameof (SetRippleWallet) + DebugRippleLibSharp.left_parentheses + nameof (RippleWallet) + DebugRippleLibSharp.space_char + nameof(rw) + DebugRippleLibSharp.equals + DebugIhildaWallet.ToAssertString(rw) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.TrustManagementWindow) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin );
			}
#endif



			if (this.walletswitchwidget3 != null) {
#if DEBUG
				if (DebugIhildaWallet.TrustManagementWindow) {
					
					Logging.WriteLog (method_sig + "walletswitchwidget3 != null");
				}
#endif
				this.walletswitchwidget3.SetRippleWallet (rw);
			}


		}


		private void SetChildrensWallets (RippleWallet rippleWallet)
		{

#if DEBUG
			string method_sig = nameof (SetChildrensWallets) + DebugRippleLibSharp.both_parentheses;
#endif


			if (this.accountlineswidget1 != null) {

#if DEBUG
				if (DebugIhildaWallet.TrustManagementWindow) {
					Logging.WriteLog (method_sig + "this.accountlines1 != null");
				}
#endif

				this.accountlineswidget1.SetViewAccount ((rippleWallet?.GetStoredReceiveAddress ()));
				this.accountlineswidget1.SetRippleWallet (rippleWallet);
			}

			if (this.trustsetter2 != null) {
				this.trustsetter2.SetRippleWallet (rippleWallet);
			}

		}


		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Goback();
		}

		private void Goback () {

			this.Hide ();

			if (WalletManagerWindow.currentInstance != null) {
				
				WalletManagerWindow.currentInstance.Show();
			}

			else {
				// todo debug
			}
		}

		/* = new Task (delegate {
			
		
			string method_sig = clsstr + "initGUI () : ";
			if (TrustManagementWindow.currentInstance == null) {
				if (Debug.TrustManagementWindow) {
					Logging.writeLog (method_sig + "new TrustManagenentWindow");
				}
				EventWaitHandle tradeWaitHandle = new ManualResetEvent (true);
				tradeWaitHandle.Reset ();
				Gtk.Application.Invoke (delegate {
					new TrustManagementWindow ();
					tradeWaitHandle.Set ();
				});

				//Gtk.Event
				tradeWaitHandle.WaitOne ();
			}
		});*/

		public static Task<TrustManagementWindow> InitGUI (RippleWallet rippleWallet) {
			return Task.Run ( delegate {
#if DEBUG
				string method_sig = clsstr + DebugRippleLibSharp.both_parentheses;
#endif
				TrustManagementWindow tmw = null;
				EventWaitHandle wh = new ManualResetEvent (true);
				wh.Reset ();
				Gtk.Application.Invoke (
					delegate {
#if DEBUG
						if (DebugIhildaWallet.TrustManagementWindow) {
							Logging.WriteLog (
								method_sig
								+ "Invoking TrustManagerWindow creation thread : "
							);
						}
#endif
						tmw = new TrustManagementWindow (rippleWallet);
						tmw.Hide ();
						//tmw.HideAll ();
						tmw.Visible = false;

#if DEBUG
						if (DebugIhildaWallet.Program) {
							Logging.WriteLog (method_sig + "finished creating trust management window \n");
						}



						if (DebugIhildaWallet.Program) {
							Logging.WriteLog (method_sig + "t5 complete");
						}
#endif
						wh.Set ();
					}
				);
				wh.WaitOne ();
				return tmw;
			

			});

		}

			/*
		public static delegate initGUI () {


		}*/

		//public static TrustManagementWindow currentInstance = null;


#if DEBUG
		private const string clsstr = nameof (TrustManagementWindow) + DebugRippleLibSharp.colon;
#endif

	}
}

