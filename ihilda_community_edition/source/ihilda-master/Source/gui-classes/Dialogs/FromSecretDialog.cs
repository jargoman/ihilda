using System;

using Gtk;
using System.Collections.Generic;
using System.IO;

using RippleLibSharp.Binary;
using RippleLibSharp.Util;

using RippleLibSharp.Keys;
using RippleLibSharp.Result;
using RippleLibSharp.Commands.Accounts;

namespace IhildaWallet
{

	public partial class FromSecretDialog : Gtk.Dialog
	{

		public FromSecretDialog ()
		{
#if DEBUG
			if (DebugIhildaWallet.FromSecretDialog) {
				Logging.WriteLog (clsstr + "new");
			}
#endif

			this.Build ();
			this.Modal = true;

			this.Initialize ();

		}



		public FromSecretDialog (String title)
		{
#if DEBUG
			if (DebugIhildaWallet.FromSecretDialog) {
				Logging.WriteLog (clsstr + "new ( title=" + DebugIhildaWallet.ToAssertString (title) + " )");
			}
#endif

			this.Build ();

			this.SetTitle (title);

			Initialize ();
		}


		public void Initialize ()
		{



			SetComboBox ();


			fromhexbutton.Clicked += (object sender, EventArgs e) => {
				RippleSeedAddress rippleSeedAddress = SeedFromHexDialog.DoDialog ();

				if (rippleSeedAddress == null) {
					return;
				}

				secretentry.Text = rippleSeedAddress.ToString ();
			
			};



			secretentry.Changed += (object sender, EventArgs e) => {

#if DEBUG
				string event_sig = clsstr + "secretentry.changed : ";
				if (DebugIhildaWallet.FromSecretDialog) {
					Logging.WriteLog (event_sig + DebugRippleLibSharp.beginn);
				}
#endif

				string s = secretentry.Text;
				if (s == null || s.Equals ("")) {
					return;
				}

				if (!s.StartsWith ("s")) {
					TextHighlighter.Highlightcolor = TextHighlighter.RED;
					this.label2.Markup = TextHighlighter.Highlight ("Secrets start with 's'");
					return;
				}

				if (!Base58.IsBase58 (s)) {
					TextHighlighter.Highlightcolor = TextHighlighter.RED;
					this.label2.Markup = TextHighlighter.Highlight ("invalid Base58 address");
					return;
				}


				try {
					RippleWallet rw = new RippleWallet (s, RippleWalletTypeEnum.Master);
					string address = rw.GetStoredReceiveAddress ();
					TextHighlighter.Highlightcolor = TextHighlighter.GREEN;
					this.label2.Markup = TextHighlighter.Highlight (address);

				}

#pragma warning disable 0168
				catch (Exception ex) {
#pragma warning restore 0168

#if DEBUG
					if (DebugIhildaWallet.FromSecretDialog) {
						Logging.ReportException (event_sig, ex);
					}
#endif

					TextHighlighter.Highlightcolor = TextHighlighter.RED;
					this.label2.Markup = TextHighlighter.Highlight ("invalid ripple address");
					return;
				}

			};

			verifybutton.Clicked += Verifybutton_Clicked;


			combobox1.Changed += Entry_Changed;


		}

		private void Entry_Changed (object sender, EventArgs e)
		{
#if DEBUG
			string method_sig = clsstr + nameof (Entry_Changed) + DebugRippleLibSharp.both_parentheses;
			string str = combobox1.ActiveText;
#endif
			RippleWalletTypeEnum walletTypeEnum = default (RippleWalletTypeEnum);

			try {
				walletTypeEnum = (RippleWalletTypeEnum)Enum.Parse (typeof (RippleWalletTypeEnum), str);


			} catch (Exception ex) {

#if DEBUG
				if (DebugIhildaWallet.FromSecretDialog) {
					Logging.ReportException (method_sig, ex);
				}
#endif
				return;
			}

			switch (walletTypeEnum) {
			/*
			case RippleWalletTypeEnum.HexPrivateKey:
				label5.Visible = false;
				comboboxentry2.Visible = false;
				return;
				*/
			case RippleWalletTypeEnum.Regular:
				label5.Visible = true;
				comboboxentry2.Visible = true;
				return;
			case RippleWalletTypeEnum.Master:
				label5.Visible = false;
				comboboxentry2.Visible = false;
				return;

			}
		}

		void Verifybutton_Clicked (object sender, EventArgs e)
		{

#if DEBUG
			string method_sig = clsstr + nameof (Verifybutton_Clicked) + DebugRippleLibSharp.left_parentheses + nameof (sender) + DebugRippleLibSharp.comma + nameof (e) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.FromSecretDialog) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif
			string secret = secretentry.Text;
			string address = null;
			string address2 = null;

			try {
				RippleSeedAddress seed = new RippleSeedAddress (secret);
				address = seed.GetPublicRippleAddress ().ToString ();
			} catch (Exception exception) {

				// It makes sense to print the libraries name to the user
				TextHighlighter.Highlightcolor = TextHighlighter.RED;
				label3.Markup = TextHighlighter.Highlight (nameof (RippleLibSharp) + " : Invalid seed");
				return;
			}



			try {
				Response<RpcWalletProposeResult> res = LocalRippledWalletPropose.GetResult (secret);
				address2 = res.result.account_id;
			} catch (Exception exception) {

				TextHighlighter.Highlightcolor = TextHighlighter.RED;
				label3.Markup = TextHighlighter.Highlight ("No response from rippled");
				return;
			}



			if (!address.Equals (address2)) {
				TextHighlighter.Highlightcolor = TextHighlighter.RED;
				label3.Markup = TextHighlighter.Highlight ("Error. addresses do not match !!!");
				return;
			}

			TextHighlighter.Highlightcolor = TextHighlighter.GREEN;
			label3.Markup = TextHighlighter.Highlight (address2);
		}


		public FromSecretDialog (string title, RippleWallet rw)
		{
#if DEBUG
			if (DebugIhildaWallet.FromSecretDialog) {
				Logging.WriteLog (clsstr + "new ( title=" + DebugIhildaWallet.ToAssertString (title) + ", rw =" + DebugIhildaWallet.ToAssertString (rw) + " )");
			}
#endif
			this.Build ();

			this.SetTitle (title);
			this.Initialize ();
			this.SetWallet (rw);


		}


		public static char [] invalidChars = System.IO.Path.GetInvalidFileNameChars ();

		private void SetComboBox ()
		{
#if DEBUG
			String method_sig = clsstr + nameof (SetComboBox) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.FromSecretDialog) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif


			ListStore store = new ListStore (typeof (string));

			store.AppendValues ("plaintext");
			store.AppendValues ("Rijndaelio");

			/*
			//if (PluginController.encryptors != null && PluginController.encryptors.Keys != null) {


				/*
				foreach (String s in PluginController.encryptors.Keys) {
					if (s == null) {
						// todo debug
						continue;
					}
					if (Debug.FromSecretDialog) {
						Logging.write (method_sig + "AppendingValue " + s + " to combobox");
					}

					store.AppendValues (s); // know there is an appendvalues function. Oh well. I want to make sure I'm setting it to string. 


				}
				*/

			//	store.AppendValues(PluginController.encryptors.Keys);

			//} else {
			// todo. figure out what the minimum is the plugin system should handle when not used. 
			// i.e allowplugins = false;
			// It may be desirable to have built in plugins working. 
			//	return;
			//}


			this.combobox2.Model = store;

			ListStore typeStore = new ListStore (typeof (string));
			typeStore.AppendValues (nameof (RippleWalletTypeEnum.Master));
			typeStore.AppendValues (nameof (RippleWalletTypeEnum.Regular));
			this.combobox1.Model = typeStore;
		}


		public void SetWallet (RippleWallet rippleWallet)
		{
#if DEBUG
			String method_sig = clsstr + nameof (SetWallet) + DebugRippleLibSharp.left_parentheses + nameof (rippleWallet) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.FromSecretDialog) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			if (rippleWallet == null) {
				// todo debug
#if DEBUG
				if (DebugIhildaWallet.FromSecretDialog) {
					Logging.WriteLog (method_sig + "rw == null, returning");
				}
#endif
				return;
			}

			if (rippleWallet.WalletName != null) {
#if DEBUG
				if (DebugIhildaWallet.FromSecretDialog) {
					Logging.WriteLog (method_sig + "rw.walletname = " + rippleWallet.WalletName);
				}
#endif
				this.entry5.Text = rippleWallet.WalletName;
			}


			/*
			if (rw.getStoredEncryptionType () == null) {
				#if DEBUG
				if (Debug.FromSecretDialog) {
					//	Logging.writeLog(method_sig + "rw.seed.ToString() == " + (String)(Debug.allowInsecureDebugging ? rw.seed.ToString() : rw.seed.ToHiddenString()));
				}
				#endif
				RippleSeedAddress seed = rw.getDecryptedSeed ();
				this.entry6.Text = seed.ToString ();
			} else {

				this.entry6.Text = rw.getDecryptedSeed ();

			}
			*/

			RippleSeedAddress seed = rippleWallet.GetDecryptedSeed ();
			this.secretentry.Text = seed.ToString ();

			//if (PluginController.currentInstance!=null) {
			//	PluginController.currentInstance.lookupEncryption(s);
			//}

			String enc = rippleWallet.GetStoredEncryptionType ();

			//
			if (combobox2.Model.GetIterFirst (out TreeIter iter)) {
				do {

					GLib.Value thisRow = new GLib.Value ();
					combobox2.Model.GetValue (iter, 0, ref thisRow);
					if (thisRow.Val is String s && s.Equals (enc)) {
						combobox2.SetActiveIter (iter);
						break;
					}

				} while (combobox2.Model.IterNext (ref iter));
			} else {
				//todo debug
			}


			
				if (combobox1.Model.GetIterFirst (out TreeIter iter2)) {
				do {

					GLib.Value thisRow = new GLib.Value ();
					combobox1.Model.GetValue (iter2, 0, ref thisRow);
						if (thisRow.Val is String s && s.Equals (rippleWallet.AccountType.ToString ())) {
						combobox1.SetActiveIter (iter2);
						break;
					}

				} while (combobox1.Model.IterNext (ref iter2));
			} else {
				//todo debug
			}

				if (rippleWallet.AccountType == RippleWalletTypeEnum.Regular) {
				comboboxentry2.Entry.Text = rippleWallet?.Account?.ToString () ?? "";
			}


			//combobox. = rippleWallet.AccountType.ToString ();


		}

		public RippleWallet GetWallet ()
		{
#if DEBUG

			String method_sig = clsstr + nameof (GetWallet) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.FromSecretDialog) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			try {

				String name = this.entry5.Text;
				if (name != null) {
#if DEBUG
					if (DebugIhildaWallet.FromSecretDialog) {
						Logging.WriteLog (method_sig + nameof (name) + DebugRippleLibSharp.equals + name);
					}
#endif

					name = name.Trim ();
				} else {
#if DEBUG
					// Todo needs a name
					if (DebugIhildaWallet.FromSecretDialog) {
						Logging.WriteLog (method_sig + nameof (name) + DebugRippleLibSharp.equals + DebugRippleLibSharp.null_str + DebugRippleLibSharp.comma + DebugRippleLibSharp.returning);
					}
#endif


					return null;
				}

				if (WalletManager.currentInstance == null) {
#if DEBUG
					if (DebugIhildaWallet.FromSecretDialog) {
						Logging.WriteLog (method_sig + "WallerManager.currentInstance == null, returning");
					}
#endif
					return null;
				}

				String s = NameMaker.RequestName (name, PluginType.WALLET);  // must be a valid name ie path must be available and
				if (s == null) {
					// todo debug // el robusto
#if DEBUG
					if (DebugIhildaWallet.FromSecretDialog) {
						Logging.WriteLog (method_sig + "requestedName string s == null, returning");
					}
#endif

					return null;
				}

				if (!s.Equals (name)) {
#if DEBUG
					if (DebugIhildaWallet.FromSecretDialog) {
						Logging.WriteLog (method_sig + "s != name, returning");
					}
#endif
					this.entry5.Text = s;
					return null;
				}

				int invalid = 0;
				foreach (Char c in invalidChars) {
					if (name.Contains (c.ToString ())) {
						// todo show invalid char message
#if DEBUG
						if (DebugIhildaWallet.FromSecretDialog) {
							Logging.WriteLog (method_sig + "name contains invalid char ");
						}
#endif
						invalid++;
					}
				}
				if (invalid > 0) {
#if DEBUG
					if (DebugIhildaWallet.FromSecretDialog) {
						Logging.WriteLog (method_sig + "name contains " + ((invalid > 1) ? "invalid characters" : "an invalid character") + ", returning");
					}
#endif
					return null;
				}

				String secret = secretentry.Text;
				if (secret != null) {
					secret = secret.Trim ();
				}

				if (!Base58.IsBase58 (secret)) {
					// todo show invalid secret message
					return null;
				}

				String selection = this.combobox2.ActiveText;

				string walletType = this.combobox1.ActiveText;
					RippleWalletTypeEnum walletTypeEnum = default (RippleWalletTypeEnum);

				try {
					walletTypeEnum = (IhildaWallet.RippleWalletTypeEnum)Enum.Parse (typeof (RippleWalletTypeEnum), walletType);
				} catch (Exception e) {
					if (DebugIhildaWallet.FromScriptDialog) {
						Logging.WriteLog (e.Message);
						Logging.ReportException (method_sig, e);

					}

						return null;
				}
				RippleWallet rw = null;
				try {
					rw = new RippleWallet (secret, walletTypeEnum);
				}

#pragma warning disable 0168
				catch (Exception eg) {
#pragma warning restore 0168


#if DEBUG
					if (DebugIhildaWallet.FromSecretDialog) {
						Logging.WriteLog (eg.Message);
						Logging.ReportException (method_sig, eg);

					}
#endif



					MessageDialog.ShowMessage ("Invalid seed");
					return null;
				}


				if (walletTypeEnum == RippleWalletTypeEnum.Regular) {
					string mast = comboboxentry2.ActiveText;

					if (string.IsNullOrWhiteSpace (mast)) {
						// TODO are ya sure. limited account capabilities 
						// I haven't tested regular accounts yet
					}

					try {
						RippleAddress rippleAddress = new RippleAddress (mast);
							rw.Account = rippleAddress;
						
						} 


						catch (Exception e) {
							
#if DEBUG
						if (DebugIhildaWallet.FromSecretDialog) {
								Logging.ReportException (method_sig, e);
							}
#endif
						return null;
						}

					

				}


				rw.WalletName = name;



				if (string.IsNullOrWhiteSpace (selection)
					|| selection == "plaintext"
					|| selection == "none") {

					return rw;
				}


				if (selection == "Rijndaelio") {

					// TODO more encryption was added and the program became more modular. 
					//string pass = PasswordCreateDialog.doDialog( "Enter your desired new password" );

					//IEncrypt ie = new Rijndaelio( );
					rw.EncryptWithSideEffects ();
					return rw;
				}

				/*
				else {
					if (PluginController.currentInstance != null) {
						IEncrypt ie = PluginController.currentInstance.lookupEncryption (selection);
						if (ie != null) {
							//todo
							// get password from password dialog
							rw.encrypt ("password", ie);
							return rw;
						}

					} else {
						// todo debug
						return null;
					}
				}
				*/


				// todo add format and crypto expection catchers

#pragma warning disable 0168
			} catch (Exception e) {
#pragma warning disable 0168

#if DEBUG
				Logging.WriteLog (method_sig + DebugRippleLibSharp.exceptionMessage + e.Message);
#endif
				return null;
			}


			return null;
		}


		private void SetTitle (String s)
		{
			if (s == null) {
#if DEBUG
				if (DebugIhildaWallet.FromSecretDialog) {
					Logging.WriteLog ("FromSecretDialog : setTitle : Attempted to set title to null");
				}
#endif
				return;
			}

			if (label8 == null) {
				// this is crazy but I had a null pointer exception. 

				Logging.WriteLog ("label8 is null");
				return;
			}

			//if (label8.Text == null) {
			//	Logging.write ("text");
			//	return;
			//}


			this.label8.Text = "<big><b><u>" + s + "</u></b></big>";
			this.label8.UseMarkup = true;
		}


#if DEBUG
		private static readonly String clsstr = nameof (FromSecretDialog) + DebugRippleLibSharp.colon;
#endif



	}
}

