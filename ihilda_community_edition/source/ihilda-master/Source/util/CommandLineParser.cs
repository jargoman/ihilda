using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using RippleLibSharp.Util;

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
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
			#endif

			#region confdir
			// command dir=path
			IEnumerable<String> conf_dir = 
				new String[] { "dir", "conf_dir", "configuration_directory", 
				"config_directory", "config_dir", "conf_directory", "confdir", 
				"config_direct", "conf_direct", "directory", "config", "conf", 
				"configuration", "conf_folder", "configuration_folder", 
				"folder", "config " };

			Command con = new Command (conf_dir);
			commands.Add(con);

			con.launch += delegate (String param) {
				//FileHelper.setFolderPath(param);
				if (param == null) {
					// todo debug

					return;
				}


				if (Directory.Exists(param)) {
					path = param;
				}

				else {
					Logging.WriteLog("Path { " + param + " } doesnn't exist");
					System.Environment.Exit(127);
				}
			};
			#endregion


			#region debugger

			// command debug=classname
			IEnumerable<String> debug = new string[] { "debug" };

			var attached = AttachSuffixes (debug, new String [] { "ging", "ger" });

			if (suffixes != null) {
				debug = debug.Concat (attached);  // debugging, debugger ect
			}

			Command debo = new Command (debug);
			commands.Add(debo);


			debo.launch += delegate (String param) {
				#if DEBUG

				Logging.WriteLog ("Setting debug to param = " + param ?? "null");
					DebugIhildaWallet.SetDebug(param, true);
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
			IEnumerable<String> fav = new String[] {"favorites", "favorite", "fave", "faves"};

			Command fab = new Command(fav);
			commands.Add(fab);

			fab.launch += BalanceTabOptionsWidget.SetFavoriteParam;


			IEnumerable<String> tooltips = new String [] { "tooltip", "tooltips", "tips"};
			Command tooltip = new Command (tooltips);
			commands.Add (tooltip);

			tooltip.launch += (string param) => {
				if (param == null) {
					param = "true";
				}

				param = param.ToLower ();

				switch (param) {
				case "false":
				case "no":
				case "off":
				case "disable":
				case "deactivate":
				case "remove":
				case "halt":
				case "stop":
					Program.showPopUps = false;
					return;

				default:
					return;
				}
			};


			IEnumerable<String> helps = new String [] { "help", "info", "h"};
		}

		public static string[] prefixes = {"-", "--", "/", ":"};
		public static string[] suffixes = {"=", ":"};

		public static string path = null;

		private static List<Command> commands = new List<Command>();

		public void ParseCommands (String[] args)
		{
			#if DEBUG
			string method_sig = clsstr + nameof (ParseCommands) +  " (String[] args)";
			if (DebugIhildaWallet.CommandLineParser) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}
			#endif
			foreach (Command com in commands) {
				String param = ParseCommand (args, com);
				if (param!=null) {
					com.launch.Invoke(param);
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
					Logging.WriteLog(method_sig + "args.Count () < 1, returning");
				}
				return null;
			}
			#endif

			bool nex = false;
			foreach (String s in args) {
				foreach (String com in command.modifiers) {
					IEnumerable<String> enumerab = new string[] {com};

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
							Logging.WriteLog(method_sig + "nex == true" + DebugRippleLibSharp.comma + DebugRippleLibSharp.returning + s ?? DebugRippleLibSharp.null_str);
						}
						#endif
					
						return s;
					}
				
				
					if (TestNextCombo(s, next)) {
						#if DEBUG
						if (DebugIhildaWallet.CommandLineParser) {
						}
						#endif

						nex = true;
						continue;
					}

					String value = TestCombo(s, combo);

					if (value != null) {
						#if DEBUG
						if (DebugIhildaWallet.CommandLineParser) {
							Logging.WriteLog(method_sig + "returning value == " + value);
						}
						#endif

						return value;
					}


					#if DEBUG
					if (DebugIhildaWallet.CommandLineParser) {
						Logging.WriteLog(method_sig + "value == null, continuing");
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
			String method_sig = clsstr + nameof (TestNextCombo) + DebugRippleLibSharp.left_parentheses + nameof (s) + DebugRippleLibSharp.equals + (s ?? DebugRippleLibSharp.null_str) + DebugRippleLibSharp.comma + nameof(comb) + DebugRippleLibSharp.equals + comb?.ToString () ?? DebugRippleLibSharp.null_str + DebugRippleLibSharp.right_parentheses;

			if (DebugIhildaWallet.CommandLineParser) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
			#endif

			if (s == null) {
				#if DEBUG
				if (DebugIhildaWallet.CommandLineParser) {
					Logging.WriteLog(method_sig + "s ==" + DebugRippleLibSharp.null_str + DebugRippleLibSharp.comma + DebugRippleLibSharp.returning + DebugRippleLibSharp.null_str);
				}
				#endif

				return false;
			}

			if (comb == null) {
				#if DEBUG
				if (DebugIhildaWallet.CommandLineParser) {
					Logging.WriteLog(method_sig + "comb ==" + DebugRippleLibSharp.null_str + DebugRippleLibSharp.comma + DebugRippleLibSharp.returning + DebugRippleLibSharp.null_str);
				}
				#endif

				return false;
			}

			foreach (String test in comb) {
						if (test.Equals(s)) {
							return true;
						}
			}

			return false;
		}

		public static String TestCombo (String s, IEnumerable<String> comb)
		{
			#if DEBUG
			String method_sig = clsstr + nameof (TestCombo) + DebugRippleLibSharp.left_parentheses + "s = " + s ?? DebugRippleLibSharp.null_str + ", comb = " + comb?.ToString() ?? DebugRippleLibSharp.null_str + DebugRippleLibSharp.right_parentheses;

			if (DebugIhildaWallet.CommandLineParser) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
			#endif

			if (s == null) {
				#if DEBUG
				if (DebugIhildaWallet.CommandLineParser) {
					Logging.WriteLog(method_sig + "s == " + DebugRippleLibSharp.null_str + DebugRippleLibSharp.comma + DebugRippleLibSharp.returning + DebugRippleLibSharp.null_str);
				}
				#endif

				return null;
			}

			if (comb == null) {
				#if DEBUG
				if (DebugIhildaWallet.CommandLineParser) {
					Logging.WriteLog(method_sig + "comb == " + DebugRippleLibSharp.null_str + DebugRippleLibSharp.comma + DebugRippleLibSharp.returning + DebugRippleLibSharp.null_str);
				}
				#endif

				return null;
			}

			foreach (String test in comb) {
				if (test == null || comb == null) {
					continue;
				}

				if (s.StartsWith(test)) {
					#if DEBUG
					if (DebugIhildaWallet.CommandLineParser) {
						Logging.WriteLog(method_sig + s + " starts with " + test);
					}
					#endif


					return s.Remove(0,test.Length);
				}
			}

			return null;
		}


		public static IEnumerable<String> AttachSuffixes (IEnumerable<String> strings, IEnumerable<string> suffixes) {
			
			if (suffixes == null) {
				return null;
			}

			if (strings == null) {
				return null;
			}

			String[] yo = new string[strings.Count() * suffixes.Count()];



			#if DEBUG
			String method_sig = clsstr + nameof (AttachSuffixes) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.CommandLineParser) {

				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);

				Logging.WriteLog(method_sig + nameof (strings) + DebugRippleLibSharp.equals, strings);

				//Logging.write(strings);

				Logging.WriteLog(method_sig + nameof (suffixes) + DebugRippleLibSharp.equals, suffixes);

				//Logging.write(suffixes);

			}
			#endif

			int x = 0;
			foreach (String val in strings) {
				foreach (String suf in suffixes) {
					yo[x++] = val + suf;

				}
			}

			#if DEBUG
			if (DebugIhildaWallet.CommandLineParser) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.returning, yo);
			}
			#endif

			return yo;
		}

		public static IEnumerable<String> AttachPrefixes (IEnumerable<String> strings, IEnumerable<String> prefixes) {


			#if DEBUG
			String method_sig = clsstr + nameof (AttachPrefixes) + DebugRippleLibSharp.both_parentheses;

			if (DebugIhildaWallet.CommandLineParser) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);

				Logging.WriteLog(method_sig + nameof (strings) + DebugRippleLibSharp.equals, strings);

				Logging.WriteLog(method_sig + nameof(prefixes) + DebugRippleLibSharp.equals, prefixes);
			}
#endif

			if (strings == null) {
				return null;
			}

			if (prefixes == null) {
				return null;
			}

			String[] returnMe = new string[prefixes.Count() * strings.Count()]; 

			int x = 0;
			foreach (String s in strings) {
				foreach (String p in prefixes) {
					returnMe[x++] = p + s;
				}
			}

			#if DEBUG
			if (DebugIhildaWallet.CommandLineParser) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.returning, returnMe);
			}
			#endif

			return returnMe;
		}


		public class Command {
			public Command (IEnumerable<String> modifiers) {

				IEnumerable<String> pre = new String[] {"set", "enable", "allow", "use"};
				IEnumerable<String> suf = new String[] {"_", "-"};

				IEnumerable<String> en = AttachSuffixes (pre, suf);

				pre = pre.Concat(en); // pre contains all the prefixes // set,set-,allow_ ect


				this.modifiers = modifiers.Concat( AttachPrefixes( modifiers, pre) );

				#if DEBUG
				if (DebugIhildaWallet.CommandLineParser) {
					Logging.WriteLog(cls_str + "new Command : ", this.modifiers);
				}
				#endif
			}

			private const string cls_str = "CommandLineParser.Command : ";

			public IEnumerable<string> modifiers = null;

			public Del launch = null;
		}


		public delegate void Del (String param);

#if DEBUG
		private static string clsstr = nameof (CommandLineParser) + DebugRippleLibSharp.colon;
		#endif
	}
}

