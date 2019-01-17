/*
 *	License : Le Ice Sense 
 */


using System;
using System.Reflection;
//using Mono;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

using Gtk;
//using Gdk;

using IhildaWallet.Networking;

using RippleLibSharp.Transactions.TxTypes;
using RippleLibSharp.Keys;
using RippleLibSharp.Network;

using System.Media;
using RippleLibSharp.Util;
using RippleLibSharp.Transactions;
using RippleLibSharp.Binary;
using System.Collections.Generic;
using System.Linq;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Commands.Subscriptions;

namespace IhildaWallet
{
	class Program
	{
		//[System.Runtime.InteropServices.DllImport ("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
		//[return: System.Runtime.InteropServices.MarshalAs (System.Runtime.InteropServices.UnmanagedType.Bool)]
		//static extern bool SetDllDirectory (string lpPathName);

		public static readonly string appname = "ihilda";
		public static readonly string version = "1.0.1";
		public static readonly string verboseName = appname + "_community_edition_" + version;

		public static bool showPopUps = true;
		public static bool network = true;
		public static bool darkmode = false;
		public static bool preferLinq = false;
		public static bool parallelVerify = false;
		public static bool usePager = false;
		public static string botMode = null;
		internal static int ledger;
		internal static int endledger;

		//public static Task walletTask = null;

		public static MemoIndice GetClientMemo ()
		{
			MemoIndice indice = new MemoIndice () {
				Memo = new RippleMemo () {
					MemoType = Base58.StringToHex ("client"),
					MemoFormat = Base58.StringToHex (Program.appname),
					MemoData = Base58.StringToHex (Program.verboseName)
				}
			};

			return indice;


		}

		private static Thread thr = new Thread (
			new ThreadStart (ThreadRoutine)
		);



		//static ThreadNotify notify;
		/*
		public static SplashWindow splash = null;
		public static PaymentWindow win = null;
		public static WalletManagerWindow wmw = null;
		public static TrustManagementWindow tmw = null;
		public static TradeWindow trdw = null;
		*/

#pragma warning disable RECS0122 // Initializing field with default value is redundant

		public static SplashWindow splash = default (SplashWindow);
		//public static PaymentWindow win = default (PaymentWindow);
		public static WalletManagerWindow wmw = default (WalletManagerWindow);
		//public static TrustManagementWindow tmw = default (TrustManagementWindow);
		//public static TradeWindow trdw = default (TradeWindow);
		public static NotificationThread Notifications = default (NotificationThread);
		public static CancellationTokenSource TokenSource = null;

#pragma warning restore RECS0122 // Initializing field with default value is redundant


		//public static Gtk.Menu menu = null;




		public static void Main (string [] args)
		{



#if DEBUG
			string method_sig = null;




			if (DebugIhildaWallet.Program) { // one day we should remove the debug clutter. At the moment it's proving usefull for debugging a multithreaded app. 
				method_sig = clsstr + nameof (Main) + DebugRippleLibSharp.both_parentheses;


				AssemblyDebug ad = new AssemblyDebug ();
				ad.DebugAssembly ();

				if (args.Length < 1) {
					Logging.WriteLog (method_sig + "No command line arguments");
				} else {
					Logging.WriteLog (method_sig + "Command line args = ", args);
				}

			}
#endif



			//r = new Mono.CSharp.CommandLineParser.ParseResult();
			//Application.Init ();

#if DEBUG
			if (DebugIhildaWallet.Program) {
				Logging.WriteLog (method_sig + "Parsing command line arguments");
			}
#endif

			// TODO command line parser was abandoned 
			CommandLineParser parse = new CommandLineParser ();
			//parse.parseCommands(new string [] {"-dir=/home/karim/confdirtest"});
			parse.ParseCommands (args);
			//Debug.setAll(true);  // VVEEEERRRRYYY SSSLLLOOOOWWWWWWW !!!!

			FileHelper.SetFolderPath (CommandLineParser.path);

			if (Program.botMode != null) {
				DoConsoleMode (args);
			} else {
				DoUiRoutine (args);
			}

		}

		public static string GetPassword ()
		{
			string pass = "";
			do {
				ConsoleKeyInfo keyInfo = System.Console.ReadKey(true);
				if (keyInfo.Key != ConsoleKey.Backspace && keyInfo.Key != ConsoleKey.Enter) {
					pass += keyInfo.KeyChar;
					System.Console.Write ('*');

				} else {
					if (keyInfo.Key == ConsoleKey.Backspace && pass.Length > 0) {
						pass = pass.Substring (0, (pass.Length - 1));
						System.Console.Write ("\b \b");
					} else if (keyInfo.Key == ConsoleKey.Enter) {
						break;
					}
				}

			} while (true);

			return pass;
		}

		public static CancellationTokenSource cancellationToken;

		private static volatile bool keepRunning = true; 
		public static void DoConsoleMode (string [] args)
		{

#if DEBUG

			string method_sig = clsstr + nameof (DoConsoleMode) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.Program) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}

#endif

			System.Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e) {
				e.Cancel = true;
				keepRunning = false;
			};

			cancellationToken = new CancellationTokenSource ();
			CancellationToken token = cancellationToken.Token;
			
			Task t7 = NetworkController.AutoConnect ();

			WalletManager walletManager = new WalletManager ();

			var rippleWallet = walletManager.LookUp (Program.botMode);

			if (rippleWallet == null) {
				Logging.WriteLog ("Could not find wallet : " + Program.botMode );
				return;
			}

			Logging.WriteLog (rippleWallet.GetStoredReceiveAddress ());






			string account = rippleWallet.GetStoredReceiveAddress ();

			RuleManager RuleManagerObj = new RuleManager (account);
			RuleManagerObj.LoadRules ();


			Logging.WriteLog ( RuleManagerObj.RulesList.Count.ToString() );

			OrderSubmitter orderSubmitter = new OrderSubmitter ();

			orderSubmitter.OnFeeSleep += (object sender, FeeSleepEventArgs e) => {
				if (e.State == FeeSleepState.Begin) {
					Logging.WriteLog ("Fee " + e.FeeAndLastLedger.Item1.ToString () + " is too high, waiting on lower fee");
					return;
				}

				if (e.State == FeeSleepState.PumpUI) {
					Logging.WriteLog (".");
					return;
				}

				if (e.State == FeeSleepState.Wake) {
					Logging.WriteLog ("\n");
					return;
				}



			};

			orderSubmitter.OnOrderSubmitted += (object sender, OrderSubmittedEventArgs e) => {
				StringBuilder stringBuilder = new StringBuilder ();


				if (e.Success) {
					stringBuilder.Append ("Submitted Order Successfully ");
					stringBuilder.Append ((string)(e?.RippleOfferTransaction?.hash ?? ""));




				} else {
					stringBuilder.Append ("Failed to submit order ");
					stringBuilder.Append ((string)(e?.RippleOfferTransaction?.hash ?? ""));

				}

				stringBuilder.AppendLine ();


				Logging.WriteLog (stringBuilder.ToString ());
			};

			orderSubmitter.OnVerifyingTxBegin += (object sender, VerifyEventArgs e) => {
				StringBuilder stringBuilder = new StringBuilder ();

				stringBuilder.Append ("Verifying transaction ");
				stringBuilder.Append ((string)(e.RippleOfferTransaction.hash ?? ""));
				stringBuilder.AppendLine ();

				Logging.WriteLog (stringBuilder.ToString ());

			};

			orderSubmitter.OnVerifyingTxReturn += (object sender, VerifyEventArgs e) => {
				StringBuilder stringBuilder = new StringBuilder ();
				string messg = null;
				if (e.Success) {
					stringBuilder.Append ("Transaction ");
					stringBuilder.Append ((string)(e?.RippleOfferTransaction?.hash ?? ""));
					stringBuilder.Append (" Verified");


					TextHighlighter.Highlightcolor = Program.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN;




				} else {
					stringBuilder.Append ("Failed to validate transaction ");
					stringBuilder.Append ((string)(e?.RippleOfferTransaction?.hash ?? ""));

					TextHighlighter.Highlightcolor = TextHighlighter.RED;

				}

				stringBuilder.AppendLine ();

				messg = TextHighlighter.Highlight (stringBuilder);

				Logging.WriteLog (messg);
			};


			Logging.WriteLog ("Running automation script for address " + account + "\n");
			Robotics robot = new Robotics ( RuleManagerObj );

			robot.OnMessage += (object sender, MessageEventArgs e) => {
				Logging.WriteLog (e.Message);
			};



			t7.Wait (cancellationToken.Token);



			NetworkInterface ni = NetworkController.CurrentInterface;

			if (ni == null) {
				Logging.WriteLog ("Was unable to connect to network. Exiting");
				Environment.Exit (-1);
			}

			RippleIdentifier rippleSeedAddress = rippleWallet.GetDecryptedSeed ();

			while (rippleSeedAddress.GetHumanReadableIdentifier () == null) {

				Logging.WriteLog ("Unable to decrypt seed. Invalid password.\nWould you like to try again?");
				

				// TODO get user input

				rippleSeedAddress = rippleWallet.GetDecryptedSeed ();
			}


			if (rippleSeedAddress == null) {
				Environment.Exit (-1);
			}
			bool success = false;

			// Keeps track of where the next start ledger will be
			int? projectedStart = null;

			if (!keepRunning) {
				string exitingMessage = "Quit request received\n";
				Logging.WriteLog (exitingMessage);

				// TODO environment var
				Environment.Exit (-1);
			}
			do {


				if (Program.endledger != 0 && Program.endledger != -1) {

					if (projectedStart != null && projectedStart > Program.endledger) {

						string maxMessage = "Maximum ledger " + endledger.ToString () + " reached \n";
						Logging.WriteLog (maxMessage);

						// TODO 

						System.Environment.Exit (-1);
					}

				}


				Logging.WriteLog (
					"Polling data for "
					+ (string)(rippleWallet?.GetStoredReceiveAddress () ?? "null")
					+ "\n");

				int last = RuleManagerObj.LastKnownLedger;

				if (Program.ledger != 0) {
					Logging.WriteLog ("\nStarting from ledger " + (string)Program.ledger.ToString () + "\n");
				} else {


					Logging.WriteLog ("\nUsing last known ledger " + last + "\n");
					if (last == 0) {

						Logging.WriteLog ("");
						Environment.Exit (-1);
					}
				}




				Tuple<Int32?, IEnumerable<AutomatedOrder>> tuple = null;

				tuple = robot.DoLogic (rippleWallet, ni, last, Program.endledger == 0 ? -1 : Program.endledger, null, token);


				if (tuple == null) {
					return;
				}

				projectedStart = tuple.Item1 + 1;



				IEnumerable<AutomatedOrder> orders = tuple.Item2;
				if (orders == null || !orders.Any ()) {
					int seconds = 60;

					string infoMessage = "Sleeping for " + seconds + " seconds";

					try {
						Logging.WriteLog (infoMessage);
						for (int sec = 0; sec < seconds; sec++) {

							if (token.IsCancellationRequested || !keepRunning) {
								return;
							}

							if (sec % 5 == 0) {
								Logging.WriteLog (".");
							}
							token.WaitHandle.WaitOne (1000);



						}
					} catch (Exception e) when (e is TaskCanceledException || e is OperationCanceledException) {
#if DEBUG
						if (DebugIhildaWallet.Program) {
							Logging.ReportException (method_sig, e);
						}
#endif

						return;
					} finally {
						Logging.WriteLog ( "\n" );
					}
					continue;
				}

				int numb = orders.Count ();
				string submitMessage = "Submitting " + numb.ToString () + " orders\n";
				Logging.WriteLog (submitMessage);

				Tuple<bool, IEnumerable<OrderSubmittedEventArgs>> tupleResp = null;

				if (!Program.parallelVerify) {
					orderSubmitter.SubmitOrders (orders, rippleWallet, rippleSeedAddress, ni, token);
				} else {
					orderSubmitter.SubmitOrdersParallel (orders, rippleWallet, rippleSeedAddress, ni, token);
				}

				if (tupleResp == null) {
					// TODO. probably unreachable code
				}

				success = tupleResp.Item1;
				if (!success) {
					string errMess = "Error submitting orders\n";
					Logging.WriteLog (errMess);
					//shouldContinue = false;
					break;
					//return;
				}

				string successMessage = "Orders submitted successfully\n";
				Logging.WriteLog (successMessage);

			} while (!token.IsCancellationRequested && keepRunning);
		}


		public static void DoUiRoutine (string[] args)
		{

#if DEBUG
			string method_sig = nameof (DoUiRoutine) + DebugRippleLibSharp.both_parentheses;
#endif
			//Gtk.Init.Check (ref args);
			//try {
			Application.Init (appname, ref args);
			//}

			//catch (Exception e) {
			//Exception inner = e.InnerException;
			//Logging.writeLog (inner.StackTrace);

			//}

#if DEBUG
			if (DebugIhildaWallet.Program) {
				Logging.WriteLog (method_sig + "Application.Init(" + appname + ")");
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

			Logging.WriteLog ("Application Run");

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
			CancellationToken token = TokenSource.Token;

			bool b = Compliance.DoUserAgreement ();
			if (!b) {
				TokenSource?.Cancel ();
				Application.Quit ();
				return;
			}

			try {

				Task t1 = InitSplash ();

				t1.Wait (token);
				

				//Task.WaitAll (new Task [] { t1});
				

			} catch (Exception e) {

#if DEBUG
				if (DebugIhildaWallet.Program) {
					Logging.ReportException (method_sig, e);
				}
#endif
				KillSplash ();
			}

			Task delayTask = Task.Delay (1000, token);


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
				, token);
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


					/*
					EventWaitHandle whand = new ManualResetEvent (true);
					whand.Reset ();
					Application.Invoke (delegate {

						try {

							wlm = new WalletManager ();

#if DEBUG
							if (DebugIhildaWallet.Program) {
								Logging.WriteLog (method_sig + nameof (t3) + " complete");
							}
#endif

							whand.Set ();





						} catch (Exception e) {

#if DEBUG
							Logging.ReportException (method_sig, e);
#endif

						} finally {
							whand?.Set ();

						}
					});
					whand?.WaitOne ();
					*/	    
				} 
			);


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
			Task.WaitAll (tsks, token);
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
				t7.Wait (3000, token);

				NetworkInterface ni = NetworkController.CurrentInterface;
				if (ni == null || !ni.IsConnected ()) {
					return;
				}

				var v = from x in wlm.wallets select x.Value;

				//List<Task> minis = new List<Task> ();

				foreach (RippleWallet rippleWallet in v) {
					//var t = Task.Run (delegate {

					RippleCurrency rippleCurrency =
					AccountInfo.GetNativeBalance (
						rippleWallet.GetStoredReceiveAddress (),
						ni,
						    token
					    );

					DateTime dateTime = DateTime.Now;


					if (rippleCurrency != null) {
						rippleWallet.LastKnownNativeBalance = rippleCurrency;
					} else {
						string mess = "<span fgcolor=\"salmon\">Could not update balance</span>";

						if (rippleWallet.Notification != mess) {
							rippleWallet.Notification = mess;

						}
					}
					//});

					//minis.Add (t);
				}

				//Task.WaitAll (minis.ToArray (), 5000);
				//WalletManager.currentInstance?.UpdateUI ();
			});

			Task.WaitAll (new Task [] { delayTask, balanceTask });

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
			if (WalletManager.currentInstance != null) {

				if (WalletManagerWidget.currentInstance != null) {
					WalletManagerWidget.currentInstance.SetWalletManager (wlm);

				} else {
					Logging.WriteLog ("WalletManagerWidget.currentInstance == null");
				}

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
			}

			
			t7.Wait (15000);






			Notifications = new NotificationThread (token);
			Notifications.InitNotificationSystem ();

		}






		public static void KillSplash ()
		{
#if DEBUG
			String method_sig = clsstr + nameof(KillSplash) + DebugRippleLibSharp.both_parentheses;

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



		public static void QuitRequest (object sender, EventArgs a)
		{

			StringBuilder stringBuilder = new StringBuilder ();
#if DEBUG
			String method_sig = null;

			if (DebugIhildaWallet.Program) {
				stringBuilder.Append (clsstr);
				stringBuilder.Append ( nameof(QuitRequest));
				stringBuilder.Append (DebugRippleLibSharp.both_parentheses);
				method_sig = stringBuilder.ToString ();

				stringBuilder.Append (DebugRippleLibSharp.begin);
				Logging.WriteLog (stringBuilder.ToString ());
				stringBuilder.Clear ();

				stringBuilder.Append (method_sig);
				stringBuilder.Append ("sender is of type : ");
				stringBuilder.Append (sender?.GetType ()?.ToString () ?? "null");
				Logging.WriteLog (stringBuilder.ToString ());
				stringBuilder.Clear ();
			}
#endif

			stringBuilder.Append ("Are you sure you want to close ");
			stringBuilder.Append (appname ?? "the application?");

			AreYouSure aus = new AreYouSure (stringBuilder.ToString ());
			DeleteEventArgs de = a as DeleteEventArgs;

			Gtk.ResponseType responseType = (ResponseType)aus.Run ();
			aus.Destroy ();

			if (responseType == ResponseType.Ok) {
				
				if (de != null) de.RetVal = true; // slick!


				Application.Quit ();

				DoExit ();
				return;


			}


			//if (responseType == ResponseType.Cancel) {
				

				if (de != null) de.RetVal = false; // 
				Window window = sender as Window; // there is a global window 
				window = sender as Window;

				if (window != null) {

					//DoExit ();
#if DEBUG
					if (DebugIhildaWallet.Program) {
						Logging.WriteLog (method_sig + "reshowing window");
					}
#endif

					Widget [] children = window.Children;

					foreach (Widget child in children) {
						Logging.WriteLog (child.GetType ().ToString ());
						child.Show ();
						child.ShowAll ();
					}
					//DoExit ();

					window.Show ();
					//win.ShowAll();
					window.ShowNow ();
				}

				Application.Quit ();
				Application.Run ();

			//}



		}

		private static void DoExit ()
		{
			Task cancelTask = Task.Run ((System.Action)TokenSource.Cancel);
			 //();

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

			if (Console.currentInstance != null) {
				Console.currentInstance.Hide ();
				Console.currentInstance.Destroy ();
				Console.currentInstance = null;
			}

			/*
			if (win != null) {
				win.Destroy ();
				win = null;
			}
			*/



			if (Notifications?.StatusTrayIcon != null) {
				Notifications.StatusTrayIcon.Dispose ();
				Notifications.StatusTrayIcon = null;
			}

			if (WalletManagerWindow.currentInstance != null) {
				//WalletManagerWindow.currentInstance.Destroy ();
				WalletManagerWindow.currentInstance = null;
			}
			Notifications = null;

			cancelTask.Wait ();
			TokenSource.Dispose();

			//Thread.Sleep (1000);


		}
		static private Task InitSplash ()
		{
			return Task.Run (
				delegate {
#if DEBUG
					string method_sig = clsstr + nameof (InitSplash) + DebugRippleLibSharp.right_parentheses;
#endif

				EventWaitHandle whandle = new ManualResetEvent (true);
				whandle.Reset ();

				Gtk.Application.Invoke (delegate {
				try {

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
				} catch (Exception e) {
#if DEBUG
							Logging.ReportException (method_sig, e);
#endif

						} finally {
							whandle.Set ();
						}

					});

					whandle.WaitOne ();
				});





		}
		#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public static StatusIcon statusIcon = null;

#pragma warning restore RECS0122 // Initializing field with default value is redundant

#if DEBUG
		private const string clsstr = nameof (Program) + DebugRippleLibSharp.colon;
#endif

	}  // end class


} // end namespace
