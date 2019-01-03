using System;

using Gtk;
using System.Collections.Generic;
using System.IO;

using RippleLibSharp.Binary;
using RippleLibSharp.Util;

using RippleLibSharp.Keys;
using RippleLibSharp.Result;
using RippleLibSharp.Commands.Accounts;
using System.Threading.Tasks;

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


		public void ClearUI ()
		{
			entry5.ModifyBase (StateType.Normal);
			secretentry.ModifyBase (StateType.Normal);
			combobox1.ModifyBase (StateType.Normal);
			combobox2.ModifyBase (StateType.Normal);
			comboboxentry2.ModifyBase (StateType.Normal);

			label6.Markup = "";
			label6.Hide ();
		}


		public void Initialize ()
		{



			SetComboBox ();


			fromhexbutton.Clicked += (object sender, EventArgs e) => {
				string rippleSigningKey = SeedFromHexDialog.DoDialog ();

				if (rippleSigningKey == null) {
					return;
				}

				secretentry.Text = rippleSigningKey;

			};


			entry5.Changed += (object sender, EventArgs e) => {
				entry5.ModifyBase (StateType.Normal);
			};



			secretentry.Changed += (object sender, EventArgs e) => {

#if DEBUG
				string event_sig = clsstr + "secretentry.changed : ";
				if (DebugIhildaWallet.FromSecretDialog) {
					Logging.WriteLog (event_sig + DebugRippleLibSharp.beginn);
				}
#endif

				secretentry.ModifyBase (StateType.Normal);

				string s = secretentry.Text;
				if (s == null || s.Equals ("")) {
					return;
				}


				if (!s.StartsWith ("s") && !s.StartsWith ("p")) {
					TextHighlighter.Highlightcolor = TextHighlighter.RED;
					this.label2.Markup = TextHighlighter.Highlight ("Secrets start with 's'");
					return;
				}

				if (!Base58.IsBase58 (s)) {
					TextHighlighter.Highlightcolor = TextHighlighter.RED;
					this.label2.Markup = TextHighlighter.Highlight ("invalid Base58 address");
					return;
				}

				if (s.StartsWith ("s")) {
					try {
						RippleWallet rw = new RippleWallet (s, RippleWalletTypeEnum.Master);
						string address = rw.GetStoredReceiveAddress ();
						TextHighlighter.Highlightcolor = Program.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN;
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
				} else {
					try {
						RipplePrivateKey privateKey = new RipplePrivateKey (s);
						string address = privateKey.GetPublicKey ().GetAddress ();
						TextHighlighter.Highlightcolor = Program.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN;
						this.label2.Markup = TextHighlighter.Highlight (address);

					} catch (Exception ex) {

#if DEBUG
						if (DebugIhildaWallet.FromSecretDialog) {
							Logging.ReportException (event_sig, ex);
						}
#endif

						TextHighlighter.Highlightcolor = TextHighlighter.RED;
						this.label2.Markup = TextHighlighter.Highlight ("invalid private key");
						return;

					}
				}

			};

			combobox2.Changed += (object sender, EventArgs e) => {
				combobox2.ModifyBase (StateType.Normal);
			};

			verifybutton.Clicked += Verifybutton_Clicked;


			combobox1.Changed += Entry_Changed;


			comboboxentry2.Changed += (object sender, EventArgs e) => {
				comboboxentry2.ModifyBase (StateType.Normal);
			};
			this.ClearUI ();



		}

		private void Entry_Changed (object sender, EventArgs e)
		{
#if DEBUG
			string method_sig = clsstr + nameof (Entry_Changed) + DebugRippleLibSharp.both_parentheses;

#endif
			string str = combobox1.ActiveText;
			combobox1.ModifyBase (StateType.Normal);

			RippleWalletTypeEnum walletTypeEnum = default (RippleWalletTypeEnum);

			try {
				walletTypeEnum = (RippleWalletTypeEnum)Enum.Parse (typeof (RippleWalletTypeEnum), str);


			} catch (Exception ex) {

#if DEBUG
				if (DebugIhildaWallet.FromSecretDialog) {
					Logging.ReportException (method_sig, ex);
				}
#endif

				combobox1.ModifyBase (StateType.Normal, new Gdk.Color (218, 112, 214));
				label6.Markup = "<span fgcolor=\"red\">Not a valid type</span>";
				label6.Show ();
				return;
			}

			switch (walletTypeEnum) {

			case RippleWalletTypeEnum.MasterPrivateKey:
				label5.Visible = false;
				comboboxentry2.Visible = false;
				return;

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
				RippleAddress add = seed.GetPublicRippleAddress ();
				address = add.ToString ();
			} catch (Exception exception) {

#if DEBUG
				if (DebugIhildaWallet.FromSecretDialog) {
					Logging.ReportException (method_sig, exception);
				}
#endif

				// It makes sense to print the libraries name to the user
				TextHighlighter.Highlightcolor = TextHighlighter.RED;
				label3.Markup = TextHighlighter.Highlight (nameof (RippleLibSharp) + " : Invalid seed");
				return;
			}



			try {
				Response<RpcWalletProposeResult> res = LocalRippledWalletPropose.GetResult (secret);
				address2 = res.result.account_id;
			} catch (Exception exception) {


#if DEBUG
				if (DebugIhildaWallet.FromSecretDialog) {
					Logging.ReportException (method_sig, exception);
				}
#endif

				TextHighlighter.Highlightcolor = TextHighlighter.RED;
				label3.Markup = TextHighlighter.Highlight ("No response from rippled");
				return;
			}



			if (!address.Equals (address2)) {
				TextHighlighter.Highlightcolor = TextHighlighter.RED;
				label3.Markup = TextHighlighter.Highlight ("Error. addresses do not match !!!");
				return;
			}

			TextHighlighter.Highlightcolor = Program.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN;
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


			string [] values = Enum.GetNames (typeof (EncryptionType));

			ListStore store = new ListStore (typeof (string));

			for (int i = 1; i < values.Length; i++) {

				store.AppendValues (values [i]);
			}
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
			typeStore.AppendValues (nameof (RippleWalletTypeEnum.MasterPrivateKey));
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

			RippleIdentifier seed = rippleWallet.GetDecryptedSeed ();
			while (seed.GetHumanReadableIdentifier () == null) {
				bool should = AreYouSure.AskQuestion (
				"Invalid password",
				"Unable to decrypt seed. Invalid password.\nWould you like to try again?"
				);

				if (!should) {
					return;
				}

				seed = rippleWallet.GetDecryptedSeed ();
			}



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


			Gdk.Color orchid = new Gdk.Color (218, 112, 214);

#if DEBUG

			String method_sig = clsstr + nameof (GetWallet) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.FromSecretDialog) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}

#endif
			try {

				String name = this.entry5.Text;

				if (!string.IsNullOrWhiteSpace (name)) {

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

					this.entry5.ModifyBase (StateType.Normal, orchid);

					this.label6.Markup = "<span fgcolor=\"red\">Wallet needs a name</span>";

					this.label6.Show ();

					return null;
				}

				if (WalletManager.currentInstance == null) {
#if DEBUG
					if (DebugIhildaWallet.FromSecretDialog) {
						Logging.WriteLog (method_sig + "WallerManager.currentInstance == null, returning");
					}
#endif

					this.label6.Markup = "<span fgcolor=\"red\">Integral bug. No wallet manager found in application. Definitely a bug</span>";
					this.label6.Show ();
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


					this.label6.Markup = "<span fgcolor=\"red\">invalid name</span>";
					this.label6.Show ();
					this.entry5.ModifyBase (StateType.Normal, orchid);
					return null;
				}

				if (!s.Equals (name)) {
#if DEBUG
					if (DebugIhildaWallet.FromSecretDialog) {
						Logging.WriteLog (method_sig + "s != name, returning");
					}

					this.label6.Markup = "<span fgcolor=\"red\">invalid name</span>";
					this.label6.Show ();
					this.entry5.ModifyBase (StateType.Normal, orchid);
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

					label6.Markup = "<span fgcolor=\"red\">name contains invalid characters</span>";
					label6.Show ();
					entry5.ModifyBase (StateType.Normal, orchid);
					return null;
				}

				String secret = secretentry.Text;


				if (string.IsNullOrWhiteSpace (secret)) {
					this.label6.Markup = "<span fgcolor=\"red\">You need to specify a secret</span>";
					secretentry.ModifyBase (StateType.Normal, orchid);
					this.label6.Show ();
					return null;
				}

				secret = secret.Trim ();

				if (!Base58.IsBase58 (secret)) {
					// todo show invalid secret message
					this.label6.Markup = "<span fgcolor=\"red\">Invalid Secret</span>";
					secretentry.ModifyBase (StateType.Normal, orchid);
					this.label6.Show ();
					return null;
				}

				String selection = this.combobox2.ActiveText;

				string walletType = this.combobox1.ActiveText;
				RippleWalletTypeEnum walletTypeEnum = default (RippleWalletTypeEnum);

				try {
					walletTypeEnum = (IhildaWallet.RippleWalletTypeEnum)Enum.Parse (typeof (RippleWalletTypeEnum), walletType);
				} catch (Exception e) {


#if DEBUG
					if (DebugIhildaWallet.FromScriptDialog) {
						Logging.WriteLog (e.Message);
						Logging.ReportException (method_sig, e);

					}

#endif

					this.label6.Markup = "<span fgcolor=\"red\">Invalid Wallet type " + (string)(walletType ?? null) + "</span>";
					combobox1.ModifyBase (StateType.Normal, orchid);
					this.label6.Show ();
					return null;
				}


				RippleWallet rw = null;

				if (walletTypeEnum == RippleWalletTypeEnum.Master || walletTypeEnum == RippleWalletTypeEnum.Regular) {
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

						this.label6.Markup = "<span fgcolor=\"red\">Failed to create the wallet. Valid secret??</span>";
						secretentry.ModifyBase (StateType.Normal, orchid);
						this.label6.Show ();
						MessageDialog.ShowMessage ("Invalid seed");
						return null;
					}
				} else if (walletTypeEnum == RippleWalletTypeEnum.MasterPrivateKey) {
					try {
						rw = new RippleWallet (secret, RippleWalletTypeEnum.MasterPrivateKey);

					} catch (Exception e) {

#if DEBUG
						if (DebugIhildaWallet.FromSecretDialog) {
							Logging.WriteLog (e.Message);
							Logging.ReportException (method_sig, e);

						}
#endif

						this.label6.Markup = "<span fgcolor=\"red\">Failed to create the wallet. Valid private key??</span>";
						secretentry.ModifyBase (StateType.Normal, orchid);
						this.label6.Show ();
						MessageDialog.ShowMessage ("Invalid private key");
						return null;
					}


				}


				if (walletTypeEnum == RippleWalletTypeEnum.Regular) {
					string mast = comboboxentry2.ActiveText;

					if (string.IsNullOrWhiteSpace (mast)) {
						// TODO are ya sure. limited account capabilities 
						// I haven't tested regular accounts yet

						this.label6.Markup = "<span fgcolor=\"red\">To create a regular key wallet; you must specify the master account it signs on behalf of</span>";
						comboboxentry2.ModifyBase (StateType.Normal, orchid);
						this.label6.Show ();
						return null;
					}

					try {
						RippleAddress rippleAddress = new RippleAddress (mast);
						rw.Account = rippleAddress;

					} catch (Exception e) {

#if DEBUG
						if (DebugIhildaWallet.FromSecretDialog) {
							Logging.ReportException (method_sig, e);
						}
#endif
						this.label6.Markup = "<span fgcolor=\"red\">Invalid Master account</span>";
						comboboxentry2.ModifyBase (StateType.Normal, orchid);
						this.label6.Show ();
						return null;
					}



				}


				rw.WalletName = name;



				if (string.IsNullOrWhiteSpace (selection)) {
					combobox2.ModifyBase (StateType.Normal, orchid);
					label6.Markup = "<span>Encryption type can not be blank</span>";
					label6.Show ();
					return null;
				}

				string [] values = Enum.GetNames (typeof (EncryptionType));

				bool worked = Enum.TryParse (selection, out EncryptionType encryptionType);


				if (!worked) {

					combobox2.ModifyBase (StateType.Normal, orchid);
					label6.Markup = "<span>Could not determine encryption type</span>";
					label6.Show ();
					return null;
				}


				//rw.Encryption_Type = selection;

				if (encryptionType == EncryptionType.None) {
						combobox2.ModifyBase (StateType.Normal, orchid);
					label6.Markup = "<span>Encryption type can not be null</span>";
					label6.Show ();
					return null;
				}

				if (encryptionType == EncryptionType.Plaintext) {
					return rw;
				}




				//byte[] payload = seed.getBytes();

				RippleSeedAddress throwawayseed = null;



				using (RandomSeedGenerator rsg = new RandomSeedGenerator ()) {
					ResponseType rt = ResponseType.None;
					rt = (ResponseType)rsg.Run ();
					throwawayseed = rsg.GetGeneratedSeed ();
					rsg.Destroy ();

					if (rt != ResponseType.Ok) {

						this.label6.Markup = "Wallet creation canceled";
						this.label6.Show ();
						return null;
					}

					if (throwawayseed == null) {
						// TODO debug
						return null;
					}


				}






				byte [] salty = throwawayseed.GetBytes ();


				IEncrypt ie = null;

				switch (encryptionType) {
				case EncryptionType.Plaintext:
					// TODO alert user, nothing to be done.
					return rw;
				case EncryptionType.Rijndaelio:
					ie = new Rijndaelio ();



					break;

				case EncryptionType.TrippleEntente:
					ie = TrippleEntenteCreationDialog.DoDialogGuiThread ();

					break;
				}

				/*
			if (rw.Encryption_Type == selection) {
				bool b = AreYouSure.AskQuestion ("change password", "You haven't changed the encryption type. Therefore do not need to change your password. Do you want to change your password anyway?");

				if (!b) {
					return rw;

				}

			}

			bool succeed = rw.DecryptWithSideEffects ();
				if (!succeed) {
				return null;
			}
			*/
				rw._encyptWithSideEffects (ie, salty);



				return rw;




				// todo add format and crypto expection catchers

#pragma warning disable 0168
			} catch (Exception e) {
#pragma warning disable 0168

#if DEBUG
				Logging.WriteLog (method_sig + DebugRippleLibSharp.exceptionMessage + e.Message);
#endif
				return null;
			}


			//return null;
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

