/*
 *	License : Le Ice Sense 
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
//using Mono;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
//using Gdk;

using IhildaWallet.Networking;
using RippleLibSharp.Binary;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Keys;
using RippleLibSharp.Network;
using RippleLibSharp.Transactions;
using RippleLibSharp.Transactions.TxTypes;
using RippleLibSharp.Util;
//using static IhildaWallet.MemoCreateDialog;

namespace IhildaWallet
{
	public static class Program
	{
		//[System.Runtime.InteropServices.DllImport ("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
		//[return: System.Runtime.InteropServices.MarshalAs (System.Runtime.InteropServices.UnmanagedType.Bool)]
		//static extern bool SetDllDirectory (string lpPathName);

		

		//public static Task walletTask = null;

		public static SelectableMemoIndice GetClientMemo ()
		{
			SelectableMemoIndice indice = new SelectableMemoIndice () {
				Memo = new RippleMemo () {
					MemoType = Base58.StringToHex ("client"),
					MemoFormat = Base58.StringToHex (ProgramVariables.appname),
					MemoData = Base58.StringToHex (ProgramVariables.verboseName)
				}
			};

			return indice;


		}





		//static ThreadNotify notify;
		/*
		public static SplashWindow splash = null;
		public static PaymentWindow win = null;
		public static WalletManagerWindow wmw = null;
		public static TrustManagementWindow tmw = null;
		public static TradeWindow trdw = null;
		*/

#pragma warning disable RECS0122 // Initializing field with default value is redundant

		

#pragma warning restore RECS0122 // Initializing field with default value is redundant


		//public static Gtk.Menu menu = null;




		public static void Main (string [] args)
		{
			/*
			var connectSettings = new ConnectionSettings ();

			connectSettings.ServerUrls = new [] { "s1.ripple.com"};
			

			NetworkInterface networkInterface = new NetworkInterface (connectSettings);

			var connectTask = networkInterface.ConnectTask ();
			connectTask.Wait ();


			string acc = "rBuDDpdVBt57JbyfXbs8gjWvp4ScKssHzx";

			var task2 = AccountCurrencies.GetResult (acc, networkInterface, new CancellationToken(), null);

			task2.Wait ();


			RippleOfferTransaction offer = new RippleOfferTransaction ();


			RippleCurrency takerGets = new RippleCurrency (1000000);

			RippleCurrency takerPays = new RippleCurrency (0.0001m, "rE9S_ripple_issuer_account", "BTC");



			offer.TakerGets = takerGets;
			offer.TakerPays = takerPays;

			offer.Sequence = 1;
			offer.LastLedgerSequence = 1234568890;

			offer.fee = new RippleCurrency (1000);

			string secret = "sABDS_secret_ripple_key";

			RippleSeedAddress rippleSeed = new RippleSeedAddress (secret);

			offer.Sign (rippleSeed);

			var response = offer.Submit (networkInterface, new CancellationToken());

			System.Console.WriteLine(response.result.engine_result_message);
	    		*/
			//return;
			//var v = new signalR ();
			//return;
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

			if (ProgramVariables.botMode != null) {
				ConsoleMain.DoConsoleMode (args);
			} else {
				UImain.DoUiRoutine (args);
			}

		}


		//public static CancellationTokenSource cancellationToken;





		


		






		



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
			stringBuilder.Append (ProgramVariables.appname ?? "the application?");

			AreYouSure aus = new AreYouSure (stringBuilder.ToString ());
			DeleteEventArgs de = a as DeleteEventArgs;

			Gtk.ResponseType responseType = (ResponseType)aus.Run ();
			aus.Destroy ();

			if (responseType == ResponseType.Ok) {
				
				if (de != null) de.RetVal = true; // slick!

				
				Application.Quit ();
				DoExit ();
				System.Environment.Exit (0);

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


			if (ProgramVariables.botMode != null) {
				ConsoleMain.DoExit ();
			} else {
				UImain.DoExit ();
			}

			
			//Thread.Sleep (1000);


		}

		#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public static StatusIcon statusIcon = null;

#pragma warning restore RECS0122 // Initializing field with default value is redundant

#if DEBUG
		private const string clsstr = nameof (Program) + DebugRippleLibSharp.colon;
#endif

	}  // end class


} // end namespace
