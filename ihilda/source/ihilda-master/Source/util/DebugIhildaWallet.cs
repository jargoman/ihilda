#if DEBUG

using System;
using IhildaWallet;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

using RippleLibSharp.Keys;
using RippleLibSharp.Network;
using RippleLibSharp.Transactions;
using RippleLibSharp.Util;
namespace IhildaWallet
{
	
	public static class DebugIhildaWallet
	{




#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public static bool AccountLinesWidget = false;

		public static bool AccountSequenceCache = false;

		public static bool AccountOrders = false;

		//public static bool AccountTransactions = false;

		public static bool AddressBook = false; 

		//public static bool allowInsecureDebugging = false; // VERY IMPORTANT. IF SET SEED / OR PASSWORDS WILL BE INCLUDED IN DEBUGGING INFORMATION

		public static bool AutomatedBuyWidget = false;

		public static bool AutomatedSellWidget = false;



		public static bool BalanceTab = false;

		public static bool BalanceTabOptionsWidget = false;

		public static bool BalanceWidget = false;

		//public static bool Base58 = false;

		//public static bool BinarySerializer = false;

		//public static bool BinaryType = false;

		public static bool BuyOffer = false;



		public static bool BuyWidget = false;

		public static bool CascadedBuyWidget = false;

		public static bool CascadedSellWidget = false;

		public static bool CommandLineParser = false;

		public static bool ConnectedDisplayWidget = false;

		//public static bool ConnectionInfo = false;

		public static bool Console = false;

		public static bool ConsoleWindow = false;

		public static bool ConsoleInterpreter = false;

		public static bool CSharpInterpreter = false;

		public static bool CurrencyWidget = false;

		public static bool CurrencyWidgetSelector = false;

		public static bool debug = false;

		public static bool DepthChartWidget = false;

		public static bool DenominatedIssuedPopup = false;

		public static bool DividendWidget = false;

		public static bool DynamicJson = false;

		public static bool FileHelper = false;

		public static bool FilledRuleManagementWindow = false;

		public static bool FromScriptDialog = false;

		public static bool FromSecretDialog = false;

		public static bool LeIceSence = false;

		public static bool NameMaker = false;

		public static bool NetworkController = false;

		//public static bool NetworkInterface = false;

		public static bool NetworkSettings = false;

		public static bool NetworkSettingsDialog = false;

		public static bool NoHideWindows = false;

		public static bool NotificationThread = false;

		public static bool MassPaymentWidget = false;

		public static bool OpenOrdersTree = false;

		public static bool ObjectSync = false;

		public static bool OrderBookWidget = false;

		public static bool OrderBookTableWidget = false;

		public static bool OrderManagementBot = false;

		public static bool OrderPreviewSubmitWidget = false;

		public static bool OrdersTreeWidget = false;

		public static bool OrderSubmitter = false;

		public static bool OrdersWidget = false;

		public static bool OrderWidget = false;

		public static bool PageCache = false;

		public static bool PagerWidget = false;

		public static bool PairPopup = false;



		public static bool PasswordCreateDialog = false;
		public static bool PasswordDialog = false;

		public static bool PaymentPreviewSubmitWidget = false;

		public static bool PaymentTree = false;

		public static bool PaymentWindow = false;

		public static bool PluginController = false;

		public static bool PrismWidget = false;

		public static bool ProcessSplash = false;

		public static bool Program = false;

		public static bool RandomSeedGenerator = false;

		public static bool ReceiveWidget = false;

		//public static bool RippleAddress = false;

		//public static bool RippleBinaryObject = false;

		//public static bool RippleCurrency = false;

		public static bool RippledController = false;

		public static bool RippleDeterministicKeyGenerator = false;

		//public static bool RippleNode = false;

		public static bool RippleOrders = false;

		//public static bool RippleTransaction = false;

		//public static bool RippleTransactionType = false;

		//public static bool RippleTrustSetTransaction = false;

		//public static bool RippleIdentifier = false;

		//public static bool RippleSeedAddress = false;

		public static bool RippleWallet = false;

		public static bool Robotics = false;

		public static bool RuleManager = false;

		public static bool SeedFromHexDialog = false;

		public static bool SellOffer = false;

		public static bool SellWidget = false;

		public static bool SendAndConvert = false;

		public static bool SendIce = false; // this does NOT toggle if Ice can be sent from client. It toggles debbug mode of SendIce class


		public static bool SendIOU = false;

		public static bool SendRipple = false;

		public static bool ServerInfo = false;

		public static bool ServerInfoWidget = false;

		public static bool SplashOptionsWidget = false;

		public static bool SplashWindow = false;

		public static bool SpreadWidget = false;

		public static bool testVectors = false;



		public static bool TradePair = false;

		public static bool TradePairManager = false;

		public static bool TradePairManagerWindow = false;

		public static bool TradePairTree = false;

		public static bool TradePairWidget = false;

		public static bool TradeWindow = false;

		public static bool TransactionSubmitWidget = false;

		public static bool TrustManagementWindow = false;

		public static bool TrustLine = false;

		public static bool TrustSetter = false;
		public static bool TxViewWidget = false;
		public static bool TxWidget = false;

		public static bool Wallet = false;
		public static bool WalletManager = false;
		public static bool WalletManagerWidget = false;
		public static bool WalletManagerWindow = false;
		public static bool WalletShowWidget = false;
		public static bool WalletTree = false;

		public static bool WalletViewPort = false;

		public static bool WalletUtil = false;


		public static void SetAll (bool boo)
		{
			String method_sig = clsstr + nameof(SetAll) + DebugRippleLibSharp.left_parentheses + boo.ToString() + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.debug) {
				Logging.WriteLog( method_sig + DebugRippleLibSharp.beginn );
			}

			Type type = typeof (DebugIhildaWallet);

			FieldInfo[] fields = type.GetFields();

			foreach (var field in fields) {
				
				if (field.FieldType == typeof( Boolean )) {
					
					if (DebugIhildaWallet.debug) {
						
						Logging.WriteLog(method_sig + field.Name + " is a boolean");

					}

					field.SetValue(null,boo); // I beleieve the first paramerer is null because there is no object for a static variable
				}

				else {
					if ( DebugIhildaWallet.debug ) {
						
						Logging.WriteLog(field.Name + " is NOT a boolean");

					}
				}
			}

			// certain ones should be OFF no matter what for security reasons. Probably a good idea to never set it to true in the first place in the code above
			//DebugIhildaWallet.allowInsecureDebugging = false;
			DebugIhildaWallet.testVectors = false;
		}

		public static bool SetDebug (String name, bool value)
		{
			String method_sig = clsstr + nameof (SetDebug) + DebugRippleLibSharp.left_parentheses + (name ?? "null") + DebugRippleLibSharp.right_parentheses;

			if (DebugIhildaWallet.debug) {
				Logging.WriteLog(method_sig + RippleLibSharp.Util.DebugRippleLibSharp.begin);
			}

			if (name == null) {
				if (DebugIhildaWallet.debug) {
					Logging.WriteLog(method_sig + "value == null, returning false");
				}
				return false;
			}


			// possible options
			IEnumerable<String> options = new string[] { "all", "full", "total", "complete", "everything", "allclasses" };


			// attach prefixes
			IEnumerable<String> prefixes = new string[] { "debug", "allow", "enable", "set" };

			IEnumerable<String> temp = IhildaWallet.CommandLineParser.AttachPrefixes ( options, prefixes );

			options = temp.Concat(options);


			//attch some more  // no wait that's a bug
			/*
			prefixes = IhildaWallet.CommandLineParser.prefixes;

			temp = IhildaWallet.CommandLineParser.attachPrefixes ( options, prefixes );

			options = temp.Concat(options);
			*/




			foreach (String s in options) {
				if (DebugIhildaWallet.debug) {
					Logging.WriteLog(method_sig + "option is " + s);
				}

				if ( name.Equals( s ) ) {
					if (DebugIhildaWallet.debug) {
						Logging.WriteLog(method_sig + "option " + s + " is a match");
					}
					SetAll (value);
					return true;
				}
			}

			String[] values = name.Split(',');

			Type type = typeof( DebugIhildaWallet );
			//FieldInfo[] fields = type.GetFields ();
			foreach (var s in values) {
				try {
					FieldInfo fi = type.GetField(s);
					if (fi==null) {
						Logging.WriteLog ("Value " + DebugIhildaWallet.ToAssertString(s) + " in not a valid debug symbol" );
						continue;
					}

					//fi.SetValue(
					fi.SetValue(null, value); // mark the field that corresponds to a class in the debugger

				

				}
				catch (Exception e) {
					Logging.WriteLog("Exception in debugger " + e.Message);
					// Todo should return on debuuger input error? I say no unless theres a security risk
					//return false;
				}
			}

			return false;
		}

		public static string ToAssertString (object obj) {
			/*
			 * Returns objects as strings while avoiding null reference exceptions. 
			 */

			if (obj == null) {
				return "null";
			}

			if (obj is Codeplex.Data.DynamicJson) {
				return obj.ToString ();
			}





			if (obj is string) {
				//return o as string;
				return (string)obj;
			}

			if (obj is RippleSeedAddress) {
				return AssertAllowInsecure (obj);
			}

			if (obj is int) {
				return obj.ToString ();
			}

			if (obj is uint?) {
				return (obj as uint?).ToString();
			}

			if (obj is Org.BouncyCastle.Math.BigInteger) {
				return (obj as Org.BouncyCastle.Math.BigInteger).ToString();
			}

			if (obj is RippleWallet) {
				return (obj as RippleWallet).GetStoredReceiveAddress ();
			}

			if (obj is ConnectionSettings) {
				ConnectionSettings ci = obj as ConnectionSettings;
				if (ci == null) {
					return "null";
				}
				return ci.ToString() ?? "null";
			}

			if (obj is Offer) {
#pragma warning disable IDE0020 // Use pattern matching
				Offer off = (Offer)obj;
#pragma warning restore IDE0020 // Use pattern matching
				if (off == null) {
					return "null";
				}
				string offstr = off.ToString ();
				return offstr ?? "null";
			}

			if (obj is RippleCurrency) {
				return (obj as RippleCurrency).ToString ();
			}

			if (obj is TradePair) {
				return (obj as TradePair).ToHumanString();
			}

			if (obj is Gtk.Window) {
				return (obj as Gtk.Window).ToString ();
			}

			if (obj is EventArgs) {
				EventArgs eva = obj as EventArgs;
				return eva.ToString ();
			}

			if (obj is Gtk.DeleteEventArgs) {
				Gtk.DeleteEventArgs dea = obj as Gtk.DeleteEventArgs;
				return dea.ToString ();
			}

			if (obj is System.String[]) {
				System.Text.StringBuilder sb = new System.Text.StringBuilder ();
				String [] ar = obj as String [];
				foreach (String s in ar) {
					sb.Append (s);
				}
				return sb.ToString ();
			}

			return obj?.ToString () ?? "null";
		}

		public static string AssertAllowInsecure ( object o ) {
			// add new types to toAssertString (object o);
			if (o == null) {
				return "null";
			}

			if (o is RippleSeedAddress) {
				RippleSeedAddress rs = o as RippleSeedAddress;
				return DebugRippleLibSharp.allowInsecureDebugging ? rs.ToString () : rs.ToHiddenString ();

			}



			if (!DebugRippleLibSharp.allowInsecureDebugging) {
				return " { withheld for security reasons  } ";
			}


			return ToAssertString (o);
		}


		public static void InitExceptionCatching () {
			Gtk.Application.Invoke ( delegate {
				GLib.ExceptionManager.UnhandledException += delegate( GLib.UnhandledExceptionArgs ar ) {
					if (ar == null) {
						// todo debug
					}




					if (ar.ExceptionObject is Exception e) {
						Logging.WriteBoth ("Uhandled exception : ");
						Logging.WriteBoth (e.Message + "\n");
						Logging.WriteBoth (e.Data.ToString () + "\n");
						Logging.WriteBoth (e.StackTrace + "\n");

						if (e.InnerException != null) {
							Logging.WriteBoth ("Inner exception");
							Logging.WriteBoth (e.InnerException.Message);
							Logging.WriteBoth (e.InnerException.Data.ToString ());
							Logging.WriteBoth (e.InnerException.StackTrace);
						}
						// todo, optional bug report? alert user through gui. 

						// ... Application should continue? Money is on the line 
					}

				}; 
			});

		}

		//public const string begin = "begin";
		//public const string beginn = DebugIhildaWallet.begin + "\n";
		//
		//public const string colon = " : ";
		//public const string comma = ", ";
		//public const string left_parentheses = " ( ";
		//public const string right_parentheses = " ) : ";
		//public const string both_parentheses = left_parentheses + right_parentheses;
		//public const string array_brackets = "[]";
		//public const string equals = " = ";
		//public const string space_char = " ";


		public const string gtkInvoke = "Gtk Invoke : ";
		public const string buildComp = "Build Complete";
		public const string returning = nameof (returning) + DebugRippleLibSharp.space_char; 
		private const string clsstr = nameof(DebugIhildaWallet) + DebugRippleLibSharp.colon;

	}

}

#pragma warning restore RECS0122 // Initializing field with default value is redundant
#endif