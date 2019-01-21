
/*
 *	License : Le Ice Sense 
 */

using System;
using System.Threading.Tasks;
using System.Threading;

using IhildaWallet;
using Gtk;
using Codeplex.Data;
using System.Collections.Generic;

using RippleLibSharp.Transactions;
using System.Text;
using RippleLibSharp.Util;

namespace IhildaWallet
{

	public partial class PaymentWindow : Gtk.Window
	{
		public PaymentWindow () : base (Gtk.WindowType.Toplevel)
		{


#if DEBUG
			String method_sig = clsstr + nameof (PaymentWindow) + DebugRippleLibSharp.both_parentheses;

			if (DebugIhildaWallet.PaymentWindow) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif
			this.Hide ();
			//this.Visible = false;
			//this.NoShowAll = true;



			Build ();

			if (this.walletswitchwidget1 == null) {


				walletswitchwidget1 = new WalletSwitchWidget ();
				walletswitchwidget1.Show ();
				table15.Attach (walletswitchwidget1, 0, 1, 0, 1);

			}

			if (currencywidgetselector1 == null) {
				currencywidgetselector1 = new CurrencyWidgetSelector ();
				currencywidgetselector1.Show ();
				table15.Attach (currencywidgetselector1, 1, 2, 0, 1);
			}



			if (this.walletshowwidget1 == null) {
				walletshowwidget1 = new WalletShowWidget ();
				walletshowwidget1.Show ();

				if (label15 == null) {
					label15 = new Label ("<b>Wallet</b>") {
						UseMarkup = true
					};
				}

				notebook1.AppendPage (walletshowwidget1, label15);
			}

			if (this.balancetab1 == null) {
				this.balancetab1 = new BalanceTab ();
				balancetab1.Show ();

				if (label35 == null) {
					label35 = new Label ("<b>Balance</b>") {
						UseMarkup = true
					};
				}
				notebook1.AppendPage (walletshowwidget1, label35);
			}

			if (this.sendripple1 == null) {
				sendripple1 = new SendRipple ();
				sendripple1.Show ();
				if (label40 == null) {
					label40 = new Label ("<b>Send XRP</b>") {
						UseMarkup = true
					};
				}

				notebook1.AppendPage (sendripple1, label40);

			}

			if (this.sendiou1 == null) {
				sendiou1 = new SendIOU ();
				sendiou1.Show ();
				if (label48 == null) {
					label48 = new Label ("<b>Send IOU<b>") {
						UseMarkup = true
					};
				}
				notebook1.AppendPage (sendripple1, label48);
			}

			if (this.sendice1 == null) {
				sendice1 = new SendIce ();
				sendice1.Show ();
				if (label57 == null) {
					label57 = new Label ("<b>Send ICE</b>") {
						UseMarkup = true
					};
				}
				notebook1.AppendPage (sendice1, label57);
			}

			if (this.sendandconvert1 == null) {
				sendandconvert1 = new SendAndConvert ();
				sendandconvert1.Show ();
				if (label65 == null) {
					label65 = new Label ("<b>Send And Convert</b>") {
						UseMarkup = true
					};
				}
				notebook1.AppendPage (sendandconvert1, label65);
			}

			if (pathfindwidget1 == null) {
				pathfindwidget1 = new PathFindWidget ();
				pathfindwidget1.Show ();
				if (label37 == null) {
					label37 = new Label ("<b>Path Find</b>") {
						UseMarkup = true
					};
				}
				notebook1.AppendPage (pathfindwidget1, label37);
			}

			if (dividendwidget1 == null) {
				dividendwidget1 = new DividendWidget ();
				dividendwidget1.Show ();
				if (label46 == null) {
					label46 = new Label ("<b>Dividend</b>");
				}
				notebook1.AppendPage (dividendwidget1, label46);
			}

			if (masspaymentwidget1 == null) {
				masspaymentwidget1 = new MassPaymentWidget ();
				masspaymentwidget1.Show ();
				if (label55 == null) {
					label55 = new Label ("<b>Mass Payment</b>");
				}
				notebook1.AppendPage (masspaymentwidget1, label55);
			}




#if DEBUG
			if (DebugIhildaWallet.PaymentWindow) {
				Logging.WriteLog (method_sig + "build complete");
			}
#endif

			this.Visible = false;
			/*
			backbutton.Clicked += (object sender, EventArgs e) => {
#if DEBUG
				String event_sig = method_sig + "backbutton pressed : ";
				if (DebugIhildaWallet.PaymentWindow) {
					Logging.WriteLog (event_sig + DebugRippleLibSharp.begin);
				}
#endif

				if (WalletManagerWindow.currentInstance != null) {
#if DEBUG
					if (DebugIhildaWallet.PaymentWindow) {
						Logging.WriteLog (event_sig + "currentInstance!=null");
					}
#endif
					// todo debug
					this.Hide ();
					// TODO uncomment this??
					//setRippleWallet(null);
					WalletManagerWindow.currentInstance.Show ();
				} else {
					//todo Debug
#if DEBUG
					if (DebugIhildaWallet.PaymentWindow) {
						Logging.WriteLog (event_sig + "currentInstance==null");
					}
#endif
				}


			};
			*/

			//Gdk.Color col = new Gdk.Color(3, 3, 56);
			//this.eventbox3.ModifyBg(StateType.Normal, col);

			//this.loadPlugins();

			this.walletswitchwidget1.WalletChangedEvent += (object source, WalletChangedEventArgs eventArgs) => {
				RippleWallet newWallet = eventArgs.GetRippleWallet ();
				this.SetChildrensWallets (newWallet);
			};


#if DEBUG
			if (DebugIhildaWallet.PaymentWindow) {
				Logging.WriteLog (clsstr + "build complete\n");
			}
#endif







			// we'll put some global json parsing here but every plugin can subscripe to events

			//this.NoShowAll = false;
		}



		/*
		public void setICE ()
		{
			throw new NotImplementedException ();
		}
		*/
		//private UInt32 highestLedger = 0;

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		//		public static PaymentWindow currentInstance = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
#if DEBUG

		
			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append (clsstr);
			stringBuilder.Append (nameof (OnDeleteEvent));
			stringBuilder.Append (DebugRippleLibSharp.left_parentheses);
			stringBuilder.Append ("object sender.getType() = ");
			stringBuilder.Append (sender?.GetType ()?.ToString () ?? DebugRippleLibSharp.null_str);
			stringBuilder.Append (", DeleteEventArgs a = ");
			stringBuilder.Append (a?.ToString () ?? DebugRippleLibSharp.null_str);
			stringBuilder.Append (DebugRippleLibSharp.right_parentheses);

			String method_sig = stringBuilder.ToString ();
			if (DebugIhildaWallet.PaymentWindow) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			//if (this.networksettings1!=null) {
			//	this.networksettings1.saveSettings ();
			//}
			if (IhildaWallet.Console.currentInstance != null) {
#if DEBUG
				if (DebugIhildaWallet.PaymentWindow) {
					Logging.WriteLog (method_sig + "Console!=null, saving history");
				}
#endif

				IhildaWallet.Console.currentInstance.SaveHistory ();
			}


			PaymentWindow window = sender as PaymentWindow;
			a.RetVal = false;

			this.Hide ();
			//Program.QuitRequest (sender, a);



		}


		/*
	public void loadPlugins ()
	{
		String method_sig = clsstr + "loadPlugins () : ";

			#if DEBUG
		if (Debug.PaymentWindow) {
			Logging.writeLog (method_sig + Debug.begin);
		}
			#endif
		

		if (!PluginController.allow_plugins) {
				#if DEBUG
			if (Debug.PaymentWindow) {
				Logging.writeLog(method_sig + "plugins have been disabled, returning");
			}
				#endif
			return;
		}

		if (PluginController.currentInstance == null) {
				#if DEBUG
			if (Debug.PaymentWindow) {
				Logging.writeLog(method_sig + "PluginController.currentInstance == null, returning");
			}
				#endif
			return;
		}


		if (PluginController.pluginList== null) {
			// todo debug
				#if DEBUG
			if (Debug.PaymentWindow) {
				Logging.writeLog(method_sig + "PluginController.pluginList== null, returning");
			}
				#endif
			return;
		}

		var vals = PluginController.pluginList.Values;
		if (vals == null) {
				#if DEBUG
			if (Debug.PaymentWindow) {
				Logging.writeLog(method_sig + "vals == null, returning");
			}
				#endif
			return;
		}

			foreach (Plugin p in vals) {
				if (p == null) {
					#if DEBUG
					if (Debug.PaymentWindow) {
						Logging.writeLog ("null plugin, continuing");
					}
					#endif
					continue;
				}

				#if DEBUG
				if (Debug.PaymentWindow) {
					Logging.writeLog (method_sig + "checking pluging " + p.name);
				}
				#endif
		
				Gtk.Widget mainTab = p.getMainTab () as Gtk.Widget;
				if (mainTab == null) {
					#if DEBUG
					if (Debug.PaymentWindow) {
						Logging.writeLog (method_sig + "mainTab == null, returning");
					}
					#endif
					return;
				}
					
				if (this.notebook1 != null) {
					//if (Debug.MainWindow) {
					Logging.writeLog ("Appending page");
					//}
					this.notebook1.AppendPage (mainTab, new Label (p.tab_label));

					this.notebook1.ShowAll ();
					this.ShowAll ();
				}



		



			}


	}
	*/

		public void UpdateUIBalance ()
		{
#if DEBUG
			String method_sig = clsstr + "updateUIBalance () : ";
			if (DebugIhildaWallet.PaymentWindow) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			//this.currencywidget10.set(RippleCurrency.Native);
			// todo update currencywidgetselector



			//this.NoShowAll = true;

			//this.NoShowAll = false;

		}

		public void SetRippleWallet (RippleWallet rippleWallet)
		{

#if DEBUG
			string method_sig = nameof (SetRippleWallet) + DebugRippleLibSharp.left_parentheses + nameof (rippleWallet) + DebugRippleLibSharp.right_parentheses;
#endif

			if (this.walletswitchwidget1 != null) {
#if DEBUG
				if (DebugIhildaWallet.PaymentWindow) {
					Logging.WriteLog (method_sig + "receivewidget2 != null");
				}
#endif
				this.walletswitchwidget1.SetRippleWallet (rippleWallet);
				//this.currencywidgetselector1
			}
		}
		private void SetChildrensWallets (RippleWallet rw)
		{
#if DEBUG
			String method_sig = clsstr + nameof(SetChildrensWallets) + " (RippleWallet rw = " + DebugIhildaWallet.ToAssertString (rw) + " ) : ";
			if (DebugIhildaWallet.PaymentWindow) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}

#endif




			if (this.currencywidgetselector1 != null) {

#if DEBUG
				if (DebugIhildaWallet.PaymentWindow) {
					Logging.WriteLog (method_sig + "currencywidgetselector1 != null");
				}
#endif
				this.currencywidgetselector1.SetRippleAddress (rw.GetStoredReceiveAddress());
			}


			if (this.walletshowwidget1 != null) {
#if DEBUG
				if (DebugIhildaWallet.PaymentWindow) {
					Logging.WriteLog (method_sig + "wallet1 != null");
				}
#endif
				this.walletshowwidget1.SetRippleWallet (rw);
			}



			if (this.balancetab1 != null) {
#if DEBUG
				if (DebugIhildaWallet.PaymentWindow) {
					Logging.WriteLog (method_sig + "balancetab1 != null");
				}
#endif

				this.balancetab1.SetAddress (rw.GetStoredReceiveAddress ());
			}

			if (this.sendripple1 != null) {
				this.sendripple1.SetRippleWallet (rw);
				this.sendripple1.Sync ();
			}


			if (this.sendice1 != null) {
#if DEBUG

				this.sendice1.SetRippleWallet (rw);

#endif
			}


			if (this.sendiou1 != null) {
				this.sendiou1.SetRippleWallet (rw);
			}

			if (this.sendandconvert1 != null) {
				this.sendandconvert1.SetRippleWallet (rw);
			}

			if (this.pathfindwidget1 != null) {
				pathfindwidget1.SetRippleWallet (rw);
			}

			if (dividendwidget1 != null) {
				dividendwidget1.SetRippleWallet (rw);

			}

			if (masspaymentwidget1 != null) {
				masspaymentwidget1.SetRippleWallet (rw);

			}
		}



		private void UnSetAll ()
		{
			// this is important !! clean out old values before using aother wallet
			// todo figure out what should be deleted,unset ect
			this.nativeBalance = 0;
			this.dropBalance = 0;

			// TODO uncomment
			//sequence = 0;


		}

		private DateTime anfdt = DateTime.MinValue;

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private bool showinganf = false;
#pragma warning restore RECS0122 // Initializing field with default value is redundant


		public void AccountNotFound (String account)
		{
			// only show the message every so often
#if DEBUG
			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append (clsstr);
			stringBuilder.Append ("accountNotFound ( account = ");
			stringBuilder.Append (account ?? "null");
			stringBuilder.Append (" ) : ");
			String method_sig = stringBuilder.ToString ();

			if (DebugIhildaWallet.PaymentWindow) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			double delay = 100.0;

			DateTime now = DateTime.Now;
			TimeSpan ts = now - anfdt;
#if DEBUG
			if (DebugIhildaWallet.PaymentWindow) {
				Logging.WriteLog (method_sig + "ts = " + ts.TotalSeconds.ToString ());
				Logging.WriteLog (method_sig + "showinganf = " + showinganf.ToString ());
			}
#endif


			if (ts.TotalSeconds > delay && !showinganf) {

				Gtk.Application.Invoke (delegate {
#if DEBUG
					if (DebugIhildaWallet.PaymentWindow) {
						Logging.WriteLog (method_sig + "gtk invoke");
					}
#endif

					String message = "Account Not Found : To use account " + account + " it must be funded with enough reserve " + RippleCurrency.NativeCurrency + " and also " + Util.LeIceSense.LICENSE_CURRENCY;

#if DEBUG
					if (DebugIhildaWallet.PaymentWindow) {
						Logging.WriteLog (method_sig + "showing user message dialog : " + message);
					}
#endif
					showinganf = true;
					IhildaWallet.MessageDialog.ShowMessage (message);
					showinganf = false;
				});


			}





		}

		public static Task<PaymentWindow> InitGUI ()
		{

			return Task.Run (
				delegate {

#if DEBUG
					string method_sig = clsstr + nameof (InitGUI) + DebugRippleLibSharp.both_parentheses;
#endif
					PaymentWindow win = null;
					using (EventWaitHandle wh = new ManualResetEvent (true)) {
						wh.Reset ();
						Gtk.Application.Invoke (
						    delegate {

							    try {

#if DEBUG
					    if (DebugIhildaWallet.PaymentWindow) {
									    Logging.WriteLog (method_sig + "Invoking MainWindow creation thread : Thread priority = " + Thread.CurrentThread.Priority);
								    }
#endif
					    win = new PaymentWindow ();  // not visible yet
					    win.Hide ();
					    //win.HideAll ();
					    //win.Visible = false;

#if DEBUG
					    if (DebugIhildaWallet.PaymentWindow) {
									    Logging.WriteLog (method_sig + "finished creating window \n");
								    }
#endif

#if DEBUG
					    if (DebugIhildaWallet.PaymentWindow) {
									    Logging.WriteLog (method_sig + "t4 complete");
								    }
#endif
					    wh.Set ();
							    } catch (Exception e) {

#if DEBUG
					    Logging.ReportException (method_sig, e);
#endif

					    wh.Set ();

							    } finally {
								    wh.Set ();
							    }

						    }
						    );
						wh.WaitOne ();
					}

					return win;
				}

			);
		}



		private static void Testing (String testMe)
		{
#if DEBUG
			if (DebugIhildaWallet.PaymentWindow) {
				Logging.WriteLog ("Testing if " + DebugIhildaWallet.ToAssertString (testMe) + " is defined");
			}
#endif
		}

		private static void Isdefi (String testMe)
		{
#if DEBUG
			if (DebugIhildaWallet.PaymentWindow) {
				Logging.WriteLog (DebugIhildaWallet.ToAssertString (testMe) + " is defined");
			}
#endif
		}
#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public decimal nativeBalance = 0m;

		public decimal dropBalance = 0m;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		//public UInt32 sequence = 0;

#if DEBUG
		private const string clsstr = nameof (PaymentWindow) + DebugRippleLibSharp.colon;
#endif

	}

}