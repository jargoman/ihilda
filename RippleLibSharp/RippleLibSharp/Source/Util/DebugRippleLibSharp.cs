#if DEBUG

using System;

using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using RippleLibSharp.Keys;
using RippleLibSharp.Network;
using RippleLibSharp.Transactions;
using RippleLibSharp.Trust;

namespace RippleLibSharp.Util
{
	
	public static class DebugRippleLibSharp
	{

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public static bool AccountInfo = false;


		public static bool AccountLines = false;


		public static bool AccountOffers = false;

		public static bool AccountRCLSettingsWindow = false;

		public static bool AccountTx = false;

		public static bool allowInsecureDebugging = false; // VERY IMPORTANT. IF SET SEED / OR PASSWORDS WILL BE INCLUDED IN DEBUGGING INFORMATION

		public static bool Base58 = false;

		public static bool BinarySerializer = false;

		public static bool BinaryType = false;

		public static bool ConnectionSettings = false;

		public static bool DataAPI = false;

		public static bool debug = false;

		public static bool DynamicJson = false;

		public static bool NetworkInterface = false;

		public static bool NetworkRequestTask = false;





		//public static bool RandomSeedGenerator = false;

		public static bool RippleAddress = false;

		public static bool RippleBinaryObject = false;

		public static bool RippleCurrency = false;

		public static bool RippleDeterministicKeyGenerator = false;

		public static bool RippleIdentifier = false;

		public static bool RippleNode = false;

		public static bool RippleOrders = false;

		public static bool RippleSeedAddress = false;

		public static bool RippleTransaction = false;

		public static bool RippleTransactionType = false;

		public static bool RippleTrustSetTransaction = false;

		public static bool RippleTxStructure = false;





		public static bool RippleWallet = false;

		public static bool ServerInfo = false;

		public static bool testVectors = false;

		public static bool TradePair = false;

		public static bool TrustLine = false;

		//public static bool Wallet = false;

#pragma warning restore RECS0122 // Initializing field with default value is redundant



		public static void SetAll (bool boo)
		{


			String method_sig = clsstr + nameof (SetAll)  + left_parentheses + boo.ToString() + right_parentheses;

			if (debug) {
				Logging.WriteLog(method_sig + begin);
			}
			Type type = typeof (DebugRippleLibSharp);
			FieldInfo[] fields = type.GetFields();
			foreach (var field in fields) {
				if (field.FieldType == typeof(Boolean)) {
					if (DebugRippleLibSharp.debug) {
						Logging.WriteLog(method_sig + field.Name + " is a boolean");
					}

					field.SetValue(null,boo); 
				}

				else {
					if (debug) {
						Logging.WriteLog(field.Name + " is NOT a boolean");
					}
				}
			}

			// certain ones should be OFF no matter what for security reasons. Probably a good idea to never set it to true in the first place in the code above
			allowInsecureDebugging = false;
			testVectors = false;
		}

		public static bool SetDebug (String value)
		{
			String method_sig = 
                clsstr + 
                nameof (SetDebug) + 
                DebugRippleLibSharp.left_parentheses + 
                (value ?? DebugRippleLibSharp.null_str)  + 
                DebugRippleLibSharp.right_parentheses;

			if (DebugRippleLibSharp.debug) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}

			if (value == null) {
				if (DebugRippleLibSharp.debug) {
					Logging.WriteLog(method_sig + "value == null, returning false");
				}
				return false;
			}


			// possible options
			IEnumerable<String> options = new string[] { "all", "full", "total", "complete", "everything", "allclasses" };


			// attach prefixes
			IEnumerable<String> prefixes = new string[] { "debug", "allow", "enable", "set" };

			int length = options.Count () * (prefixes.Count () + 1);
			List<String> list = new List<string> (length);
			foreach (String opt in options) {
				foreach (String pre in prefixes) {
					list.Add (pre + opt);
				}
			}


			list.AddRange (options);
		




			foreach (String s in list) {
				if (DebugRippleLibSharp.debug) {
					Logging.WriteLog(method_sig + "option is " + s);
				}

				if ( value.Equals( s ) ) {
					if (DebugRippleLibSharp.debug) {
						Logging.WriteLog(method_sig + "option " + s + " is a match");
					}
					SetAll (true);
					return true;
				}
			}

			String[] values = value.Split(',');

			Type type = typeof(DebugRippleLibSharp);
			//FieldInfo[] fields = type.GetFields ();
			foreach (var s in values) {
				try {
					FieldInfo fi = type.GetField(s);
					if (fi!=null) {
						//fi.SetValue(
						fi.SetValue(null, true); // mark the field that corresponds to a class in the debugger
					}

					else {
						Logging.WriteLog ("Value " + DebugRippleLibSharp.ToAssertString(s) + " in not a valid debug symbol" );
					}

				}
				catch (Exception e) {
					Logging.WriteLog("Exception in debugger " + e.Message);
					// Todo should return on debuuger input error? I say no unless theres a security risk
					//return false;
				}
			}

			return false;
		}

		public static string ToAssertString (object o) {
			/*
			 * Returns objects as strings while avoiding null reference exceptions. 
			 */
			string method_sig = clsstr + nameof (ToAssertString) + DebugRippleLibSharp.left_parentheses + nameof (Object) + DebugRippleLibSharp.space_char + nameof (o) + DebugRippleLibSharp.right_parentheses;


			if (o == null) {
				if (DebugRippleLibSharp.debug) {
					Logging.WriteLog (method_sig + nameof (o) + DebugRippleLibSharp.equals + DebugRippleLibSharp.null_str + DebugRippleLibSharp.comma + DebugRippleLibSharp.returning + DebugRippleLibSharp.null_str );
				}
				return DebugRippleLibSharp.null_str;
			}

			if (o is Codeplex.Data.DynamicJson) {
				return o.ToString ();
			}





			if (o is string) {
				return o as string;
			}

			if (o is RippleSeedAddress) {
				return AssertAllowInsecure (o);
			}

			if (o is int) {
				return o.ToString ();
			}

			if (o is uint?) {
				return (o as uint?).ToString();
			}

			if (o is Org.BouncyCastle.Math.BigInteger) {
				return (o as Org.BouncyCastle.Math.BigInteger).ToString();
			}

			/*
			if (o is RippleWallet) {
				return (o as RippleWallet).getStoredReceiveAddress ();
			}*/

			if (o is ConnectionSettings) {
				ConnectionSettings ci = o as ConnectionSettings;
				if (ci == null) {
					return DebugRippleLibSharp.null_str;
				}
				return ci.ToString() ?? DebugRippleLibSharp.null_str;
			}

			if (o is Offer) {
				if (!(o is Offer off)) {
					return DebugRippleLibSharp.null_str;
				}
				string offstr = off.ToString ();
				return offstr ?? DebugRippleLibSharp.null_str;
			}

			if (o is RippleCurrency) {
				return (o as RippleCurrency).ToString ();
			}
			/*
			if (o is TradePair) {
				return (o as TradePair).toHumanString();
			}

			
			*/

			if (o is EventArgs) {
				EventArgs eva = o as EventArgs;
				return eva.ToString ();
			}

			if (o is System.String[]) {
				System.Text.StringBuilder sb = new System.Text.StringBuilder ();
				String [] ar = o as String [];
				foreach (String s in ar) {
					sb.Append (s);
				}
				return sb.ToString ();
			}

			return (o?.ToString () ?? DebugRippleLibSharp.null_str);
		}

		public static string AssertAllowInsecure ( object o ) {
			// add new types to toAssertString (object o);
			if (o == null) {
				return "null";
			}

			if (o is RippleSeedAddress) {
				RippleSeedAddress rs = o as RippleSeedAddress;
				return allowInsecureDebugging ? rs.ToString () : rs.ToHiddenString ();

			}



			if (!allowInsecureDebugging) {
				return " { withheld for security reasons  } ";
			}


			return ToAssertString (o);
		}

		public const string begin = nameof (begin); // lol
		public const string beginn = begin + "\n";
		public const string colon = " : ";
		public const string comma = ", ";
		public const string left_parentheses = " ( ";
		public const string right_parentheses = " )" + colon;
		public const string both_parentheses = left_parentheses + right_parentheses;
		public const string array_brackets = "[]";
		public const string equals = " = ";
		public const string space_char = " ";
		public const string exceptionMessage = "Exception thrown : \n\n";
		public const string null_str = "null";
		public const string returning = nameof (returning) + space_char; 

		private const string clsstr = nameof (DebugRippleLibSharp) + colon;
	}

}

#endif