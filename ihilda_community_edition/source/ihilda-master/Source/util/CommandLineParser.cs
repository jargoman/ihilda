using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using RippleLibSharp.Util;
using System.Text;

namespace IhildaWallet
{
	public class CommandLineParser
	{

		// NOTE : this is the command line parser not the console tabs parser
		public CommandLineParser ()
		{
#if DEBUG
			String method_sig = clsstr + nameof (CommandLineParser) + DebugRippleLibSharp.both_parentheses;

			if (DebugIhildaWallet.CommandLineParser) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			#region confdir
			// command dir=path
			IEnumerable<String> conf_dir =
				new String [] { "dir", "conf_dir", "configuration_directory",
				"config_directory", "config_dir", "conf_directory", "confdir",
				"config_direct", "conf_direct", "directory", "config", "conf",
				"configuration", "conf_folder", "configuration_folder",
				"folder", "config " };


			StringBuilder usage = new StringBuilder ();
			usage.Append (Program.appname);
			usage.AppendLine (" conf={path_to_config_folder}");
			usage.Append ("Config folder defaults to ");
			usage.AppendLine (FileHelper.DEFAULT_CONFIG_FOLDER_PATH);
			Command con = new Command (conf_dir) {
				UsageText = usage.ToString ()
			};
			commands.Add (con);

			con.launch += delegate (String param) {
				//FileHelper.setFolderPath(param);
				if (param == null) {
					// todo debug

					return;
				}


				if (Directory.Exists (param)) {
					path = param;
				} else {
					Logging.WriteLog ("Path { " + param + " } doesn't exist");
					System.Environment.Exit (127);
				}
			};

			#endregion


			#region debugger

			// command debug=classname
			IEnumerable<String> debug = new string [] { "debug" };

			var attached = AttachSuffixes (debug, new String [] { "ging", "ger" });

			if (suffixes != null) {
				debug = debug.Concat (attached);  // debugging, debugger ect
			}

			Command debo = new Command (debug);
			commands.Add (debo);

#if DEBUG
			usage.Clear ();
			usage.Append (Program.appname);
			usage.AppendLine (" debug=Class1,Class2");
			usage.AppendLine ("List of c# classes to print debug information");
			usage.AppendLine ("For testing and development");

			debo.UsageText = usage.ToString ();

//#else
//			debo.UsageText = 
#endif


			debo.launch += delegate (String param) {
#if DEBUG

				Logging.WriteLog ("Setting debug to param = " + param ?? "null");
				DebugIhildaWallet.SetDebug (param, true);
#else
				Logging.WriteBoth ("Command line option debug is not implented in release mode");
#endif

			};


			#endregion


			/*
			// command plugin=true
			IEnumerable<String> plug = new string[] {"plugin", "plugins", "plug", "plugs"};

			Command plugin = new Command (plug);
			commands.Add(plugin);

			plugin.launch += delegate (String param) {
				PluginController.setAllow (param);
			};

			*/


			// command favorites=btc,usd,ice
			IEnumerable<String> fav = new String [] { "favorites", "favorite", "fave", "faves" };

			Command fab = new Command (fav);
			commands.Add (fab);

			usage.Clear ();
			usage.Append (Program.appname);
			usage.AppendLine (" favorites=XRP,BTC,ETH,BCH,ADA");
			usage.AppendLine ("Prefer to display information pertaining to these currencies");


			fab.UsageText = usage.ToString ();

			fab.launch += BalanceTabOptionsWidget.SetFavoriteParam;


			IEnumerable<String> tooltips = new String [] { "tooltip", "tooltips", "tips" };
			Command tooltip = new Command (tooltips);
			commands.Add (tooltip);

			usage.Clear ();
			usage.Append (Program.appname);
			usage.AppendLine (" tooltips={true,false}");
			usage.AppendLine ("Enable gui tooltips");

			tooltip.UsageText = usage.ToString ();

			tooltip.launch += (string param) => {
				if (param == null) {
					param = "true";
				}

				param = param.ToLower ();


				Program.showPopUps = !StringRepresentsFalse(param);
					


			};

			IEnumerable<String> networks = new String [] { "network", "networking", "net", "websocket", "web", "websockets", "socket", "sockets"};
			Command network = new Command (networks);
			commands.Add (network);

			usage.Clear ();
			usage.Append (Program.appname);
			usage.AppendLine (" networking={true,false}");
			usage.AppendLine ("Enable or disable networking");
			usage.AppendLine ("Defaults to true");

			network.UsageText = usage.ToString ();

			network.launch += (string param) => {
				if (param == null) {
					return;
				}

				param = param.ToLower ();


				if (StringRepresentsFalse (param)) {

					Program.network = false;
					System.Console.WriteLine ("Networking disabled");
					return;

				}

			};

			IEnumerable<String> helps = new String [] { "help", "info", "h", "example", "examples" };
			Command help = new Command (helps);
			commands.Add (help);

			usage.Clear ();
			usage.Append (Program.appname);
			usage.AppendLine (" help");
			usage.AppendLine ("Print this help text");

			help.UsageText = usage.ToString ();

			help.launch += (string param) => {

				StringBuilder stringBuilder = new StringBuilder ();
			
				System.Console.WriteLine (stringBuilder.ToString ());

				stringBuilder.AppendLine (Program.appname);
				stringBuilder.AppendLine ();
				stringBuilder.AppendLine ("Usage : ");
				stringBuilder.Append ("./");
				stringBuilder.Append (Program.appname);
				stringBuilder.AppendLine (" {option1}={value} {option2}={value} {option3}={value} ...");

				stringBuilder.AppendLine ("Options : \n");
				//stringBuilder.AppendLine ("help");

				foreach (Command capcom in commands) {

					if (capcom?.UsageText != null) {

						//stringBuilder.AppendLine ();
						stringBuilder.AppendLine (capcom.UsageText);
						//stringBuilder.AppendLine ();

					}

				}


				Logging.WriteLog (stringBuilder.ToString());

				System.Threading.Thread.Sleep (500);

				Environment.Exit (0);
			};

			IEnumerable<string> darks = new String [] { "dark", "darkmode"};
			Command dark = new Command (darks);
			commands.Add (dark);

			usage.Clear ();
			usage.Append (Program.appname);
			usage.AppendLine (" darkmode={true,false}");
			usage.AppendLine ("Enable or disable darkmode");
			usage.AppendLine ("Defaults to false"); 
			usage.AppendLine ("Note to achive a dark look you must edit your gtk2 and/or gtk3 theme");
			usage.AppendLine ("gtk themes are applied system wide");

			dark.UsageText = usage.ToString ();


			dark.launch += (string param) => {

				Program.darkmode |= StringRepresentsTrue (param);

			};

			IEnumerable<string> linqs = new String [] { "linq" };
			Command linq = new Command (linqs);
			commands.Add (linq);

			usage.Clear ();
			usage.Append (Program.appname);
			usage.AppendLine (" linq={true,false}");
			usage.AppendLine ("Enable or disable linq as the preferred algorithm.");
			usage.AppendLine ("Defaults to false");


			linq.UsageText = usage.ToString ();


			linq.launch += (string param) => {

				Program.preferLinq |= StringRepresentsTrue (param);
				RippleLibSharp.Configuration.Config.PreferLinq = Program.preferLinq;

			};

			string parallelStr = "parallelOrderSubmit";
			IEnumerable<string> parallels = new String [] { parallelStr };
			Command parallel = new Command (parallels);
			commands.Add (parallel);

			usage.Clear ();
			usage.Append (" ");
			usage.Append (parallelStr);
			usage.Append ("={true,false}");
			usage.Append ("Enable or disable parallel order verification");
			usage.Append ("Uses multithreading to speed up automation");
			usage.Append ("Defaults to false");

			parallel.UsageText = usage.ToString ();

			parallel.launch += (string param) => {
				Program.parallelVerify |= StringRepresentsTrue (param);
			};

			IEnumerable<string> bots = new String [] { "bot", "marketbot", "automate"};
			Command bot = new Command (bots);
			commands.Add (bot);

			usage.Clear ();
			usage.Append (Program.appname);
			usage.AppendLine (" bot={walletname}");
			usage.AppendLine ("Automate market bot in command line mode");
			usage.AppendLine ("Must specify a valid walletname");

			bot.UsageText = usage.ToString ();

			bot.launch += Bot_Launch;

			IEnumerable<string> ledgers = new String [] { "ledger", "startledger", "beginledger"};
			Command led = new Command (ledgers);
			commands.Add (led);


			usage.Clear ();
			usage.Append (Program.appname);
			usage.AppendLine (" startledger={int}");
			usage.AppendLine ("In botmode start from ledger");
			usage.AppendLine ("Must be a valid integer");
			bot.UsageText = usage.ToString ();

			led.launch += (string param) => {
				if (param != null) {
					bool b = int.TryParse (param, out int result);
					if (!b) {
						Logging.WriteLog ("Invalid start ledger, must be valid integer");

						// TODO error code number
						System.Environment.Exit (-1);
					}
					Program.ledger = result;
				}
			};

			IEnumerable<string> endledger = new String [] { "stopledger", "endledger", "lastledger" };
			Command endled = new Command (endledger);
			commands.Add (endled);

			usage.Clear ();
			usage.Append (Program.appname);
			usage.AppendLine (" stopledger={int}");
			usage.AppendLine ("In botmode stop when ledger has been reached");
			usage.AppendLine ("Must be a valid integer");

			endled.UsageText = usage.ToString ();

			endled.launch += (string param) => {
				if (param != null) {
					bool b = int.TryParse (param, out int result);
					if (!b) {
						Logging.WriteLog ("Invalid end ledger, must be valid integer");

						// TODO error code number
						System.Environment.Exit (-1);
					}
					Program.endledger = result;
				}
			};
		}


		void Bot_Launch (string param)
		{

			Program.botMode = param;


		}


		private bool StringRepresentsTrue (string s)
		{
			s = s.ToLower ();
			switch (s) {
			case "true":
			case "yes":
			case "on":
			case "ensable":
			case "activate":
			case "begin":
			case "allow":
			case "start":
				return true;
			default:
				return false;
			}
		}

		private bool StringRepresentsFalse (string s)
		{
			s = s.ToLower ();
			switch (s) {
			case "false":
			case "no":
			case "off":
			case "disable":
			case "deactivate":
			case "remove":
			case "halt":
			case "stop":

				return true;

			default:
				return false;
			}
		}

		public static string [] prefixes = { "-", "--", "/", ":" };
		public static string [] suffixes = { "=", ":" };

		public static string path = null;

		private static List<Command> commands = new List<Command> ();

		public void ParseCommands (String [] args)
		{
#if DEBUG
			string method_sig = clsstr + nameof (ParseCommands) + " (String[] args)";
			if (DebugIhildaWallet.CommandLineParser) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif
			foreach (Command com in commands) {
				String param = ParseCommand (args, com);
				if (param != null) {
					com?.launch?.Invoke (param);
				}
			}
		}


		public static String ParseCommand (IEnumerable<String> args, Command command)
		{
#if DEBUG
			String method_sig = clsstr + nameof (ParseCommand) + DebugRippleLibSharp.both_parentheses;

			if (DebugIhildaWallet.CommandLineParser) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}

			if (!args.Any ()) {
				if (DebugIhildaWallet.CommandLineParser) {
					Logging.WriteLog (method_sig + "args.Count () < 1, returning");
				}
				return null;
			}
#endif

			bool nex = false;
			foreach (String s in args) {
				foreach (String com in command.modifiers) {
					IEnumerable<String> enumerab = new string [] { com };

					var pref = AttachPrefixes (enumerab, prefixes);

					IEnumerable<String> next = null;
					if (pref == null) {
						next = enumerab;
					} else {
						next = pref.Concat (enumerab);
					}



					var suf = AttachSuffixes (next, suffixes);

					IEnumerable<String> combo = suf;

					if (nex) {
#if DEBUG
						if (DebugIhildaWallet.CommandLineParser) {
							Logging.WriteLog (method_sig + "nex == true" + DebugRippleLibSharp.comma + DebugRippleLibSharp.returning + s ?? DebugRippleLibSharp.null_str);
						}
#endif

						return s;
					}


					if (TestNextCombo (s, next)) {
#if DEBUG
						if (DebugIhildaWallet.CommandLineParser) {
						}
#endif

						nex = true;
						continue;
					}

					String value = TestCombo (s, combo);

					if (value != null) {
#if DEBUG
						if (DebugIhildaWallet.CommandLineParser) {
							Logging.WriteLog (method_sig + "returning value == " + value);
						}
#endif

						return value;
					}


#if DEBUG
					if (DebugIhildaWallet.CommandLineParser) {
						Logging.WriteLog (method_sig + "value == null, continuing");
					}
#endif
					continue;


				}
			}


			return null;

		}


		public static bool TestNextCombo (String s, IEnumerable<String> comb)
		{
#if DEBUG
			String method_sig = clsstr + nameof (TestNextCombo) + DebugRippleLibSharp.left_parentheses + nameof (s) + DebugRippleLibSharp.equals + (s ?? DebugRippleLibSharp.null_str) + DebugRippleLibSharp.comma + nameof (comb) + DebugRippleLibSharp.equals + comb?.ToString () ?? DebugRippleLibSharp.null_str + DebugRippleLibSharp.right_parentheses;

			if (DebugIhildaWallet.CommandLineParser) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			if (s == null) {
#if DEBUG
				if (DebugIhildaWallet.CommandLineParser) {
					Logging.WriteLog (method_sig + "s ==" + DebugRippleLibSharp.null_str + DebugRippleLibSharp.comma + DebugRippleLibSharp.returning + DebugRippleLibSharp.null_str);
				}
#endif

				return false;
			}

			if (comb == null) {
#if DEBUG
				if (DebugIhildaWallet.CommandLineParser) {
					Logging.WriteLog (method_sig + "comb ==" + DebugRippleLibSharp.null_str + DebugRippleLibSharp.comma + DebugRippleLibSharp.returning + DebugRippleLibSharp.null_str);
				}
#endif

				return false;
			}

			foreach (String test in comb) {
				if (test.Equals (s)) {
					return true;
				}
			}

			return false;
		}

		public static String TestCombo (String s, IEnumerable<String> comb)
		{
#if DEBUG
			String method_sig = clsstr + nameof (TestCombo) + DebugRippleLibSharp.left_parentheses + "s = " + s ?? DebugRippleLibSharp.null_str + ", comb = " + comb?.ToString () ?? DebugRippleLibSharp.null_str + DebugRippleLibSharp.right_parentheses;

			if (DebugIhildaWallet.CommandLineParser) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			if (s == null) {
#if DEBUG
				if (DebugIhildaWallet.CommandLineParser) {
					Logging.WriteLog (method_sig + "s == " + DebugRippleLibSharp.null_str + DebugRippleLibSharp.comma + DebugRippleLibSharp.returning + DebugRippleLibSharp.null_str);
				}
#endif

				return null;
			}

			if (comb == null) {
#if DEBUG
				if (DebugIhildaWallet.CommandLineParser) {
					Logging.WriteLog (method_sig + "comb == " + DebugRippleLibSharp.null_str + DebugRippleLibSharp.comma + DebugRippleLibSharp.returning + DebugRippleLibSharp.null_str);
				}
#endif

				return null;
			}

			foreach (String test in comb) {
				if (test == null || comb == null) {
					continue;
				}

				if (s.StartsWith (test)) {
#if DEBUG
					if (DebugIhildaWallet.CommandLineParser) {
						Logging.WriteLog (method_sig + s + " starts with " + test);
					}
#endif


					return s.Remove (0, test.Length);
				}
			}

			return null;
		}


		public static IEnumerable<String> AttachSuffixes (IEnumerable<String> strings, IEnumerable<string> suffixes)
		{

			if (suffixes == null) {
				return null;
			}

			if (strings == null) {
				return null;
			}

			String [] yo = new string [strings.Count () * suffixes.Count ()];



#if DEBUG
			String method_sig = clsstr + nameof (AttachSuffixes) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.CommandLineParser) {

				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);

				Logging.WriteLog (method_sig + nameof (strings) + DebugRippleLibSharp.equals, strings);

				//Logging.write(strings);

				Logging.WriteLog (method_sig + nameof (suffixes) + DebugRippleLibSharp.equals, suffixes);

				//Logging.write(suffixes);

			}
#endif

			int x = 0;
			foreach (String val in strings) {
				foreach (String suf in suffixes) {
					yo [x++] = val + suf;

				}
			}

#if DEBUG
			if (DebugIhildaWallet.CommandLineParser) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.returning, yo);
			}
#endif

			return yo;
		}

		public static IEnumerable<String> AttachPrefixes (IEnumerable<String> strings, IEnumerable<String> prefixes)
		{


#if DEBUG
			String method_sig = clsstr + nameof (AttachPrefixes) + DebugRippleLibSharp.both_parentheses;

			if (DebugIhildaWallet.CommandLineParser) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);

				Logging.WriteLog (method_sig + nameof (strings) + DebugRippleLibSharp.equals, strings);

				Logging.WriteLog (method_sig + nameof (prefixes) + DebugRippleLibSharp.equals, prefixes);
			}
#endif

			if (strings == null) {
				return null;
			}

			if (prefixes == null) {
				return null;
			}

			String [] returnMe = new string [prefixes.Count () * strings.Count ()];

			int x = 0;
			foreach (String s in strings) {
				foreach (String p in prefixes) {
					returnMe [x++] = p + s;
				}
			}

#if DEBUG
			if (DebugIhildaWallet.CommandLineParser) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.returning, returnMe);
			}
#endif

			return returnMe;
		}


		public class Command
		{
			public Command (IEnumerable<String> modifiers)
			{

				IEnumerable<String> pre = new String [] { "set", "enable", "allow", "use" };
				IEnumerable<String> suf = new String [] { "_", "-" };

				IEnumerable<String> en = AttachSuffixes (pre, suf);

				pre = pre.Concat (en); // pre contains all the prefixes // set,set-,allow_ ect


				this.modifiers = modifiers.Concat (AttachPrefixes (modifiers, pre));

#if DEBUG
				if (DebugIhildaWallet.CommandLineParser) {
					Logging.WriteLog (cls_str + "new Command" + DebugRippleLibSharp.colon, this.modifiers);
				}
#endif

			

			}

			public string UsageText = "";

#if DEBUG
			private const string cls_str = nameof (CommandLineParser) + "." + nameof (Command) +  DebugRippleLibSharp.both_parentheses;
#endif
			public IEnumerable<string> modifiers = null;

			public Del launch = null;
		}


		public delegate void Del (String param);

#if DEBUG
		private static string clsstr = nameof (CommandLineParser) + DebugRippleLibSharp.colon;
#endif
	}
}

