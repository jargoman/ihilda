using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using IhildaWallet.Networking;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Commands.Subscriptions;
using RippleLibSharp.Keys;
using RippleLibSharp.Network;
using RippleLibSharp.Transactions;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public class UImain
	{
		public UImain ()
		{
		}

		public static SplashWindow splash = default (SplashWindow);
		//public static PaymentWindow win = default (PaymentWindow);
		public static WalletManagerWindow wmw = default (WalletManagerWindow);
		//public static TrustManagementWindow tmw = default (TrustManagementWindow);
		//public static TradeWindow trdw = default (TradeWindow);
		public static NotificationThread Notifications = default (NotificationThread);
		public static CancellationTokenSource TokenSource = default(CancellationTokenSource);

		private static Thread thr = new Thread (
			new ThreadStart (ThreadRoutine)
		);

		public static void DoUiRoutine (string [] args)
		{

#if DEBUG
			string method_sig = nameof (DoUiRoutine) + DebugRippleLibSharp.both_parentheses;
#endif
			//Gtk.Init.Check (ref args);
			//try {
			Application.Init (ProgramVariables.appname, ref args);
			//}

			//catch (Exception e) {
			//Exception inner = e.InnerException;
			//Logging.writeLog (inner.StackTrace);

			//}

#if DEBUG
			if (DebugIhildaWallet.Program) {
				Logging.WriteLog (method_sig + "Application.Init(" + ProgramVariables.appname + ")");
			}
#endif


#if DEBUG
			if (DebugIhildaWallet.Program) {
				Logging.WriteLog (method_sig + "Loading splash config");
			}
#endif
			SplashWindow.LoadSplash (); // loads splash config // must be first



			thr.Start ();
#if DEBUG
			if (DebugIhildaWallet.Program) {
				Logging.WriteLog (method_sig + "Application Run");
			}
#endif

			Application.Run ();

			Logging.WriteLog ("Application Run Exited");

		}

		static void ThreadRoutine ()
		{
			/* 
			 * This function is massively multithreaded. 
			 * It's the loadup process while the splash screen is displayed.
			 * Things are staggared the way they are to use multi-threading
			 * and to take advantage of more than one procesNotificationThreadsor. 
			 * the threads are staggared in placement
			 * 
			 * non-gtk-thread
			 * gtk-thread
			 * non-gtk
			 * gtk
			 * non
			 * gtk
			 * non
			 * ect...
			 */

			String method_sig = null;
#if DEBUG

			if (DebugIhildaWallet.Program) {
				method_sig = clsstr + "Main Thread Routine : ";
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}

#endif


#if DEBUG
			DebugIhildaWallet.InitExceptionCatching ();
#endif

			//Task<ConsoleWindow> consoleGuiTask = ConsoleWindow.InitGUI ();

			Task t7 = NetworkController.AutoConnect ();

			TokenSource = new CancellationTokenSource ();
			//CancellationToken token = TokenSource.Token;

			bool b = Compliance.DoUserAgreement ();
			if (!b) {
				TokenSource?.Cancel ();
				Application.Quit ();
				return;
			}

			try {

				Task t1 = InitSplash ();


				// wait until spash shows up before consuming resources
				t1.Wait (TokenSource.Token);


				//Task.WaitAll (new Task [] { t1});


			} catch (Exception e) {

#if DEBUG
				if (DebugIhildaWallet.Program) {
					Logging.ReportException (method_sig, e);
				}
#endif
				KillSplash ();
			}

			Task splashdelayTask = Task.Delay (3000, TokenSource.Token);


			//consoleGuiTask.Wait ();

			//Logging.writeBoth ("booting up....\n");

#if DEBUG

			//#endif
			if (DebugIhildaWallet.testVectors) {
				//Thread th = new Thread (
				//new ThreadStart (
				Task.Run (
						delegate {

							if (DebugIhildaWallet.Program) {
								Logging.WriteLog (method_sig + "Running test vectors");
							}

							RippleDeterministicKeyGenerator.TestVectors ();
						}
				, TokenSource.Token);
				//);
				//th.Start ();
			}
#endif


			/*
			Task t2 = Task.Run ( delegate {
				PluginController.initPlugins ();
				if (PluginController.currentInstance!=null) {
					PluginController.currentInstance.preStartUp();
				}
				#if DEBUG
				if (Debug.Program) {
					Logging.writeLog(method_sig + "t2 complete");
				}
				#endif
			});
			//t2.Start ();
			*/

			WalletManager wlm = null;
			Task t3 = Task.Run (
				delegate {

					wlm = new WalletManager ();

				}
			, TokenSource.Token);


			/*
			if (SplashWindow.delay == null) {
				if (Debug.Program) {
					Logging.writeLog(method_sig + "SplashWindow.delay == null, setting to default " + SplashWindow.default_delay.ToString());
				}
				SplashWindow.delay = SplashWindow.default_delay;
			}

			if (SplashWindow.delay!=null) {
				if (Debug.Program) {
					String d = SplashWindow.delay.ToString();
					Logging.writeLog(method_sig + "Splash delay is " + d + ", sleeping for " + d + "milliseconds");
				}
				Thread.Sleep( (Int32) SplashWindow.delay );
			}
			*/

			Task [] tsks = {/*t1, t2,*/ t3 };
			Task.WaitAll (tsks, TokenSource.Token);
#if DEBUG
			if (DebugIhildaWallet.Program) {
				Logging.WriteLog (method_sig + "stop waiting1");
			}
#endif
			//Task paymentUITaskT4 = PaymentWindow.InitGUI ();
			//t4.Start ();



			//Task trustUITaskT5 = TrustManagementWindow.InitGUI ();

			/*
			Task consoleHistoryT6 = Task.Run (delegate {
				Console.LoadHistory ();
#if DEBUG
				if (DebugIhildaWallet.Program) {
					Logging.WriteLog (method_sig + "t6 complete");
				}
#endif
			});

			*/



			/*
				Task.Run (delegate {
				
				




				#if DEBUG
				if (Debug.Program) {
					Logging.writeLog(method_sig + "t7 complete");
				}
				#endif
			});
			*/


			//	Task tradeUITaskT8 = TradeWindow.InitGUI ();



#if DEBUG


			//paymentUITaskT4.Wait ();

			//trustUITaskT5.Wait ();

			//consoleHistoryT6.Wait ();

			//tradeUITaskT8.Wait ();

#else
			//Task [] tska = new Task [] { paymentUITaskT4, trustUITaskT5, consoleHistoryT6, tradeUITaskT8 };
			//Task.WaitAll (tska);
#endif



			//foreach(Task ta in tska) {

			//}


#if DEBUG
			if (DebugIhildaWallet.Program) {
				Logging.WriteLog (method_sig + "stop waiting2");
			}
#endif
			/*while (win==null) {  
				Thread.Sleep(5);
			}*/

			/*
			Task t9 = Task.Run (
				delegate {
					
				}
			);
			*/

			//Task t9 = TradeWindow.initGUI ();
			Task<WalletManagerWindow> t9 = WalletManagerWindow.InitGUI ();

			/*
			Task t10 = Task.Run (delegate {
				try {

					BalanceTabOptionsWidget.loadBalanceConfig();
					// uses gtk invoke
					if (BalanceTab.currentInstance != null) {
						BalanceTab.currentInstance.set ();
					} else {
#if DEBUG
						if (Debug.Program) {
							Logging.writeLog (method_sig + "BalanceTab.currentInstance != null");
						}
#endif
					}

#if DEBUG
					if (Debug.Program) {
						Logging.writeLog(method_sig + "t10 complete");
					}
#endif
				}

				catch ( Exception e ) {
					Logging.reportException(method_sig, e);
				}
			});
			*/








			Task [] tasks = { t9 };
			Task.WaitAll (tasks);

			wmw = t9.Result;

#if DEBUG
			if (DebugIhildaWallet.Program) {
				Logging.WriteLog (method_sig + "stop waiting3");
			}
#endif


			Task balanceTask = Task.Run (delegate {
				t7.Wait ( TokenSource.Token);

				NetworkInterface ni = NetworkController.CurrentInterface;
				if (ni == null || !ni.IsConnected ()) {
					return;
				}
				var wallets = wlm.wallets;
				var v = from x in wallets select x.Value;
				v = v.Reverse ();
				//List<Task> minis = new List<Task> ();

				foreach (RippleWallet rippleWallet in v) {
					var t = Task.Run (delegate {

					RippleCurrency rippleCurrency =
					AccountInfo.GetNativeBalance (
						rippleWallet.GetStoredReceiveAddress (),
						ni,
						    TokenSource.Token
					    );

					//DateTime dateTime = DateTime.Now;


					if (rippleCurrency != null) {
						rippleWallet.LastKnownNativeBalance = rippleCurrency;
					} else {
						string mess = "<span fgcolor=\"salmon\">Could not update balance</span>";

						if (rippleWallet.Notification != mess) {
							rippleWallet.Notification = mess;

						}
					}
					});

					//minis.Add (t);
				}

				//Task.WaitAll (minis.ToArray (), 5000);
				//WalletManager.currentInstance?.UpdateUI ();
			});

			//Task.WaitAny (new Task [] { splashdelayTask, balanceTask });

			balanceTask.Wait (3000);
			//Task.WaitAll (new Task [] { delayTask });

			if (WalletManagerWidget.currentInstance != null) {
				WalletManagerWidget.currentInstance.SetWalletManager (wlm);

			} else {
				Logging.WriteLog ("WalletManagerWidget.currentInstance == null");
			}


			splashdelayTask.Wait (2000);

			Gtk.Application.Invoke (
				delegate {
					//win.Show();
					KillSplash ();

					if (wmw != null) {
						wmw.Show ();

					} else {
#if DEBUG
						Logging.WriteLog (method_sig + "window manager missing ? :/");
#endif
					}
				}
			);


			Logging.WriteBoth ("Welcome !!\n");




			//delayTask.Wait (1000);




			if (WalletManager.currentInstance.IsEmpty ()) {
				String isawesome = "You don't have any wallets. Would you like to create a new wallet?";
				Gtk.Application.Invoke (delegate {
					bool res = AreYouSure.AskQuestion ("New Wallet Wizard", isawesome);

					if (res) {
						//Program.KillSplash ();
						WalletManagerWidget.currentInstance.New_Wallet_Wizard ();
					}
				});
			}



			t7.Wait (15000, TokenSource.Token);






			Notifications = new NotificationThread (TokenSource.Token);
			Notifications.InitNotificationSystem ();

		}

		public static void KillSplash ()
		{
#if DEBUG
			String method_sig = clsstr + nameof (KillSplash) + DebugRippleLibSharp.both_parentheses;

			if (DebugIhildaWallet.Program) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			if (splash != null) {
#if DEBUG
				if (DebugIhildaWallet.Program) {
					Logging.WriteLog (method_sig + "killing splash");
				}
#endif
				splash.Hide ();
				splash.Destroy ();
				splash = null;
			}
#if DEBUG
			else if (DebugIhildaWallet.Program) {
				Logging.WriteLog (method_sig + "no splash screen to kill");
			}
#endif

		}

		static private Task InitSplash ()
		{
			return Task.Run (
				delegate {
#if DEBUG
					string method_sig = clsstr + nameof (InitSplash) + DebugRippleLibSharp.right_parentheses;
#endif



					TaskHelper.GuiInvokeSyncronous (delegate {


#if DEBUG
						if (DebugIhildaWallet.Program) {
							Logging.WriteLog (method_sig + DebugIhildaWallet.gtkInvoke + DebugRippleLibSharp.beginn);
						}
#endif

						if (SplashWindow.isSplash) {
#if DEBUG
							if (DebugIhildaWallet.Program) {
								Logging.WriteLog (method_sig + "Creating splash");
							}
#endif
							splash = new SplashWindow ();
						}
#if DEBUG
						if (DebugIhildaWallet.Program) {
							Logging.WriteLog (method_sig + "t1 complete");
						}
#endif
						//whandle.Set();	


					});



				});


		}

		public static void DoExit ()
		{
			Task cancelTask = Task.Run ((System.Action)TokenSource.Cancel);
			Task cancelTask2 = Task.Run ((System.Action)LedgerTracker.TokenSource.Cancel);
			if (splash != null) {
				splash.Hide ();
				splash.Destroy ();
				splash = null;
			}

			if (wmw != null) {
				wmw.Hide ();
				wmw.Destroy ();
				wmw = null;
			}

			



			if (Notifications?.StatusTrayIcon != null) {
				Notifications.StatusTrayIcon.Dispose ();
				Notifications.StatusTrayIcon = null;
			}
	    		
	    		/*
			if (WalletManagerWindow.currentInstance != null) {
				//WalletManagerWindow.currentInstance.Destroy ();
				WalletManagerWindow.currentInstance = null;
			}
			Notifications = null;
	    		*/
			//cancelTask.Wait (5000);
			//cancelTask2.Wait (5000);

			Task.WaitAll (new Task[] { cancelTask, cancelTask2 }, 5000);
			TokenSource.Dispose ();



		}

#if DEBUG
		private const string clsstr = nameof (UImain) + DebugRippleLibSharp.colon;
#endif
	}
}
