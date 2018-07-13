using System;
using System.Text;
using Codeplex.Data;
using RippleLibSharp.Network;
//using Mono.CSharp;
using IhildaWallet.Networking;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public static class ConsoleInterpreter
	{

		public static string InterpretJSON (string json)
		{
			#if DEBUG
			String method_sig = clsstr + nameof (InterpretJSON) + DebugRippleLibSharp.left_parentheses + nameof (json) + DebugRippleLibSharp.equals + DebugIhildaWallet.ToAssertString(json) + " ) : ";
			if (DebugIhildaWallet.ConsoleInterpreter) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
			#endif

			NetworkInterface ni = NetworkController.GetNetworkInterfaceGuiThread();

			if ( ni == null ) {
				#if DEBUG
				if (DebugIhildaWallet.Console) {
					Logging.WriteLog ("Console : NetworkInterface.currentInstance == null");
				}
				#endif

				MessageDialog.ShowMessage ("You have to be connected to a server to send a json command.\nGo to the network settings tab");
				return null;
			}
			ni.SendToServer ( json );

			return json;
		}

		public static bool TestIsJSON (String json)
		{
			#if DEBUG
			String method_sig = clsstr + nameof (TestIsJSON)  + DebugRippleLibSharp.left_parentheses + DebugIhildaWallet.ToAssertString(json) + DebugRippleLibSharp.right_parentheses;
			#endif
			try {

			if (!IsBasicStatement (json)) {
				return false;
			}
			if (!(json.StartsWith ("{") && json.EndsWith ("}"))) {
				return false;
			}

			if (!(json.Contains ("\"") && json.Contains (":"))) {
				return false;
			}

			if ( ! DynamicJson.CanParse ( json ) ) {
				return false;
			}

				#pragma warning disable 0168
			} catch (Exception exce) {
				#pragma warning restore 0168

				#if DEBUG
				if (DebugIhildaWallet.ConsoleInterpreter) {
					//Logging.writeLog(method_sig);
					Logging.ReportException (method_sig, exce);
				}
				#endif
				return false;
			}

			return true; // It's most likely JSON ??
		}

		public static bool TestIsCSharp (String command)
		{
			// 

			if (!IsBasicStatement(command)) {
				return false;
			}

			command = command.Trim();

			//if (!command.EndsWith (";")) {
			if (!command.EndsWith(";", StringComparison.CurrentCulture)) {
				return false;
			}

			return true;
		}

		public static string InterpretRippled ( string message )
		{
			if ( !IsBasicStatement(message) ) {
				return null;
			}

			message = message.Trim();

			String[] tokens = message.Split(' ');
			string lower = tokens[0].ToLower();
			if (lower.Equals("rippled") || lower.Equals("./rippled")) {
				//StringBuilder sb = new StringBuilder((int)message.Length);

				string args = message.Remove(0, tokens[0].Length);
				return RippledController.RippledEval(args);

			}

			return null;
		}



		public static string InterpretCSharp ( string command ) {
			#if DEBUG
			string method_sig = clsstr + nameof (InterpretCSharp) + DebugRippleLibSharp.left_parentheses + nameof (String) + DebugRippleLibSharp.space_char + nameof(command) + DebugRippleLibSharp.equals + DebugRippleLibSharp.ToAssertString(command) + " ) : ";
			if (DebugIhildaWallet.ConsoleInterpreter) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
			#endif


			if (!IsBasicStatement(command)) {
				#if DEBUG
				if (DebugIhildaWallet.ConsoleInterpreter) {
					Logging.WriteLog(method_sig + "is not a basic c# statement, returning null");
				}
				#endif
				return null;
			}
			object o = CSharpInterpreter.InterpretCharp( command );
			return o?.ToString ();
			
		}

		public static Interpreters InterpretJargon ( string bonkers ) {


			if (!IsBasicStatement(bonkers)) {
				return Interpreters.NONE;
			}

			if (TestIsJSON(bonkers)) {
				InterpretJSON(bonkers);
				return Interpreters.JSON;
			}

			if (TestIsCSharp(bonkers)) {
				InterpretCSharp(bonkers);
				return Interpreters.CSHARP;
			}

			return IceJargonInterpreter.interpret (bonkers);
		}

		private static bool IsBasicStatement ( String statement )
		{
			if (statement == null) {
				return false;
			}

			statement = statement.Trim();

			if ( statement.Equals("") ) {
				return false;
			}

			return true;
		}

		public static string Interpret ( String message ) {
			switch (interpreter) {
			case Interpreters.JSON:
				return InterpretJSON(message);


			case Interpreters.RIPPLED:
				return InterpretRippled(message);

			case Interpreters.CSHARP:
				return InterpretCSharp(message);

			case Interpreters.ICEJARGON:
				return InterpretJargon(message).ToString();
			}

			return null;
		}

		public static Gtk.ListStore GetInterpretersListStore ()
		{
			#if DEBUG
			String method_sig = clsstr + nameof (GetInterpretersListStore) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.ConsoleInterpreter) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
			#endif
			String[] names = Enum.GetNames(typeof(Interpreters));
			Gtk.ListStore ls = new Gtk.ListStore(typeof (string));

			foreach (String name in names) {
				ls.AppendValues(name);
			}

			return ls;

			//return combo;
		}


		public static Interpreters interpreter = Interpreters.ICEJARGON;

		//public static string rippled

		#if DEBUG
		private static readonly string clsstr = nameof (ConsoleInterpreter) + DebugRippleLibSharp.colon;
		#endif
	}
}

