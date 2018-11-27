using System;
using System.Text;
using Org.BouncyCastle.Math;
using Gtk;
using RippleLibSharp.Binary;
using RippleLibSharp.Transactions;
using RippleLibSharp.Keys;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public partial class FromScriptDialog : Gtk.Dialog
	{
		public FromScriptDialog ()
		{
			this.Build ();
			this.Modal = true;

			this.label4.Text = Base58.ALPHABET;

			this.NamePattern = new WalletNamePattern ();

			SetToolTips ();
		}

		public void SetToolTips ()
		{
			if (!Program.showPopUps) {
				return;
			}

			var alphab = "These are the available characters used in the rcl base58 alphabet";
			label4.TooltipMarkup = alphab;
			label5.TooltipMarkup = alphab;

			var pattern = "Creat a vanity address that contains this string of characters\v";
			label2.TooltipMarkup = pattern;
			patternentry.TooltipMarkup = pattern;

			var thr = "Number of threads to use at once\nRecommended : Number one thread per core or less\n";
			threadnumentry.TooltipMarkup = thr;
			label3.TooltipMarkup = thr;

			this.checkbutton1.TooltipMarkup = "Search for an address that begins with pattern";
			this.checkbutton2.TooltipMarkup = "Search for an address that ends with pattern";

			this.checkbutton3.TooltipMarkup = "Search for an address that contains the pattern";
			this.checkbutton4.TooltipMarkup = "Do not distignish between uppercase and lowercase letters";

			buttonOk.TooltipMarkup = "Begin thread(s) execution";
			buttonCancel.TooltipMarkup = "Cancel and close the window";
		}

		public WalletNamePattern NamePattern {
			get;
			set;
		}




		public int? GetThreads ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (GetThreads) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.FromScriptDialog) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			string numstr = threadnumentry.ActiveText;
			int? num = RippleCurrency.ParseInt32 (numstr);
			if (num == null) {
				MessageDialog.ShowMessage ("Number of threads is formatter incorrectly \n");
			}
#if DEBUG
			if (DebugIhildaWallet.FromScriptDialog) {
				Logging.WriteLog (method_sig + "returning num = " + DebugIhildaWallet.ToAssertString (num));
			}
#endif
			return num;
		}

		public String GetStringPattern ()
		{
#if DEBUG
			String method_sig = clsstr + nameof (GetStringPattern) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.FromScriptDialog) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			NamePattern.BeginsWith = checkbutton1.Active;
			NamePattern.EndsWith = checkbutton2.Active;
			NamePattern.Contains = checkbutton3.Active;
			NamePattern.Ignorecase = checkbutton4.Active;

#if DEBUG
			if (DebugIhildaWallet.FromScriptDialog) {
				Logging.WriteLog (method_sig + nameof(NamePattern.BeginsWith) + DebugRippleLibSharp.equals + NamePattern.BeginsWith.ToString () + nameof (NamePattern.EndsWith) + DebugRippleLibSharp.equals + NamePattern.EndsWith.ToString () + DebugRippleLibSharp.comma + nameof (NamePattern.Contains) + DebugRippleLibSharp.equals + NamePattern.Contains.ToString () + DebugRippleLibSharp.comma + nameof (NamePattern.Ignorecase) + DebugRippleLibSharp.equals + NamePattern.Ignorecase.ToString ());
				Logging.WriteLog (method_sig + DebugRippleLibSharp.returning + patternentry.ActiveText);
			}
#endif

			return this.patternentry.ActiveText;
		}


		public static void DoDialog ()
		{
#if DEBUG
			String method_sig = clsstr + nameof (DoDialog) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.FromScriptDialog) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			//RippleSeedAddress res = null;

			RandomSeedGenerator rsg = new RandomSeedGenerator ();

#if DEBUG
			if (DebugIhildaWallet.FromScriptDialog) {
				Logging.WriteLog (method_sig + "starting random generator");
			}
#endif
			ResponseType resp = (ResponseType)rsg.Run ();
			rsg.Hide ();

			if (resp != ResponseType.Ok) {
#if DEBUG
				if (DebugIhildaWallet.FromScriptDialog) {
					Logging.WriteLog (method_sig + "user did not click OK, returning");
				}
#endif
				return;
			}

#if DEBUG
			if (DebugIhildaWallet.FromScriptDialog) {
				Logging.WriteLog (method_sig + "user clicked ok");
			}
#endif

			FromScriptDialog fsd = new FromScriptDialog ();
			int? ts = null;  //
			String pattern = null;
			String [] patterns = null;

			int cores = GetNumCores ();

			while (ts == null) { //
#if DEBUG
				if (DebugIhildaWallet.FromScriptDialog) {
					Logging.WriteLog (method_sig + "running from secret dialog");
				}
#endif
				resp = (ResponseType)fsd.Run ();

				if (resp != ResponseType.Ok) {
					fsd.Destroy ();
					return;
				}

				fsd.Hide ();
#if DEBUG
				if (DebugIhildaWallet.FromScriptDialog) {
					Logging.WriteLog (method_sig + "user clicked ok");
				}
#endif

				pattern = fsd.GetStringPattern ();
				if (pattern == null) {
#if DEBUG
					if (DebugIhildaWallet.FromScriptDialog) {
						Logging.WriteLog (method_sig + nameof (pattern) + " == null, continuing");
					}
#endif

					fsd.textview.Buffer.Text = "You must specify the text you want to match in your ripple address";
					continue;
				}


				int? threads = fsd.GetThreads ();
				if (threads == null) {
					threads = 1;
				}
				if (threads < 1) {
					fsd.textview.Buffer.Text = "The number of threads must be greater than zero";
					continue;
				}
				if (threads > cores) {
					fsd.textview.Buffer.Text = "The number of threads must not be greater than the number or processors";
					continue;
				}


				patterns = pattern.Split (' ');

#if DEBUG
				if (DebugIhildaWallet.FromScriptDialog) {
					Logging.WriteLog (method_sig + nameof (pattern) + " split into...");
					int x = 0;
					foreach (String s in patterns) {
						Logging.WriteLog (method_sig + "patterns[" + x.ToString () + "] = " + DebugIhildaWallet.ToAssertString (s));
					}
				}
#endif

				foreach (String st in patterns) {
					if (!Base58.IsBase58 (st)) {


						string warn = "Pattern is not a valid BASE58 string\n";
#if DEBUG
						if (DebugIhildaWallet.FromScriptDialog) {
							Logging.WriteLog (method_sig + warn);
						}
#endif
						fsd.textview.Buffer.Text = warn;
						continue;

					}
				}

				ts = fsd.GetThreads ();

			}



			BigInteger bigInt = rsg.GetBigInt ();
#if DEBUG
			if (DebugIhildaWallet.FromScriptDialog) {
				Logging.WriteLog (method_sig + nameof (bigInt) + DebugRippleLibSharp.equals + DebugIhildaWallet.AssertAllowInsecure (bigInt));
			}
#endif

			ProcessSplash splsh = new ProcessSplash (bigInt, ts.GetValueOrDefault ());
			splsh.Show ();




#if DEBUG
			if (DebugIhildaWallet.FromScriptDialog) {
				Logging.WriteLog (method_sig + nameof (pattern) + DebugRippleLibSharp.equals + DebugIhildaWallet.ToAssertString (pattern));
			}
#endif



			splsh.SetParams (patterns, fsd.NamePattern.Ignorecase, fsd.NamePattern.BeginsWith, fsd.NamePattern.EndsWith, fsd.NamePattern.Contains, 0);
#if DEBUG
			if (DebugIhildaWallet.FromScriptDialog) {
				Logging.WriteLog (method_sig + "running process splash");
			}
#endif


			splsh.RunScript ();  // returns almost immediately as it eventually calls async method
#if DEBUG
			if (DebugIhildaWallet.FromScriptDialog) {
				Logging.WriteLog (method_sig + "process		 splash, runScript returned");
			}
#endif



			return;

		}


		private static bool VerifyPattern (string [] patterns, bool beginsWith, bool endsWith, bool contains, bool ignorecase)
		{


#if DEBUG
			string method_sig = clsstr + nameof (VerifyPattern) + " ( string[] patterns, bool beginsWith=" + beginsWith.ToString () + ", bool endsWith=" + endsWith.ToString () + ", bool contains" + contains.ToString () + ", bool ignorecase" + ignorecase.ToString () + ") : ";

			if (DebugIhildaWallet.FromScriptDialog) {
				Logging.WriteLog (method_sig + "begin, patterns=", patterns);
			}
#endif


			if (beginsWith) {
				if (endsWith || contains) {
					bool atLeastOnce = false;
					foreach (string s in patterns) {
						//if (s == null) {
						//	continue;
						//}

						if (Base58.IsBase58 (s)) {

						}

						//if (s.StartsWith ("r")) {
						atLeastOnce |= s.StartsWith ("r", StringComparison.CurrentCulture);
					}

					if (!atLeastOnce) {
						MessageDialog.ShowMessage ("None of the string patterns you entered begins with r. When specifying the beginswith flag along with other flags, at least one of the patterns should begin with r");
						//return false;
					}

					return atLeastOnce;
				}
				foreach (string s in patterns) {
					//if (s.StartsWith ("r")) {
					if (s.StartsWith ("r", StringComparison.CurrentCulture)) {
						MessageDialog.ShowMessage ("Since all ripple addresses begin with \"r\" When beginswith is the only flag set all string patterns should begin with r");
						return false;
					}
				}
			}

			return false;
		}



		private static int GetNumCores () {
			return Environment.ProcessorCount;
		}

		#if DEBUG
		private static readonly string clsstr = nameof (FromScriptDialog) + DebugRippleLibSharp.colon;
		#endif



	}

	public class WalletNamePattern
	{
		//private String pattern = null;

		public bool BeginsWith {
			get;
			set;
		}

		public bool EndsWith {
			get;
			set;
		}

		public bool Contains {
			get;
			set;
		}

		public bool Ignorecase {
			get;
			set;
		}


	}


}

