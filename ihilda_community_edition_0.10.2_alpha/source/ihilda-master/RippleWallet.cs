using System;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using RippleLibSharp.Keys;
using RippleLibSharp.Binary;
using Codeplex.Data;
using RippleLibSharp.Util;
using RippleLibSharp.Transactions;

namespace IhildaWallet
{
	public class RippleWallet
	{
		public RippleWallet (String secret)
		{
			this.Seed = new RippleSeedAddress (secret);
			this.Account = Seed.GetPublicRippleAddress ().ToString ();
			NextTransactionSequenceNumber = -1; // TODO what's a good "invalid" default number. 
		}

		public RippleWallet (RippleSeedAddress rseed)
		{
			this.Seed = rseed;
			this.Account = rseed.GetPublicRippleAddress ();
			NextTransactionSequenceNumber = -1;
		}

		public RippleWallet (String encrypted, String encryptionType)
		{
			if (encrypted == null) {
				// Todo debug
				return;
			}

			this.Encrypted_Wallet = encrypted;

			if (encrypted != null) {
				this.Encryption_Type = encryptionType;
			}


			NextTransactionSequenceNumber = -1;

		}

		public RippleWallet (String encrypted, String encryptionType, String receiveAddress) : this (encrypted, encryptionType)
		{
			this.Account = receiveAddress;  // can't verify
		}

		/*
		public void deleteWallet ()
		{
			if (WalletManager.currentInstance!=null) {
				AreYouSure ays = new AreYouSure("Warning you are about to delete wallet " + ((this.walletname!=null) ? this.walletname : "[ No Name ]") + "/n" + ((this.getStoredReceiveAddress())));
					
				ResponseType res = (ResponseType) ays.Run();

				if ( ResponseType.Ok == res ) {
					WalletManager.currentInstance.deleteWallet(this);
				}


			}
		}
		*/

		public string GetStoredEncryptionType ()
		{
			if (Encryption_Type != null) {
				return Encryption_Type;
			}

			if (Encrypted_Wallet == null && Seed != null) {
				return "plaintext";

			}

			return Rijndaelio.default_name;
		}
		public string GetStoredReceiveAddress ()
		{
			if (Seed != null) {
				return Seed.GetPublicRippleAddress ().ToString ();
			}

			if (Account != null) {
				return Account;
			}

			return "";
		}
		public string WalletName {
			get;
			set;
		}

		private RippleSeedAddress Seed {
			get;
			set;
		}

		public int NextTransactionSequenceNumber {
			get;
			set;
		}


		public String Encrypted_Wallet {
			get;
			set;
		}// encryptes bytes encoded as ripple Identifier

		public String Encryption_Type {
			get;
			set;
		}

		public String Account {
			get;
			set;
		}

		public String Salt {
			get;
			set;
		}

		/*
		public String saltTwo {
			get;
			set;
		}
		*/

		public uint? LastKnownLedger {
			get;
			set;
		}

		public string WalletPath {
			get;
			set;
		}

		public string Notification {
			get;
			set;
		}

		public RippleCurrency LastKnownNativeBalance {
			get;
			set;
		}

		public void EncryptWithSideEffects ()
		{

			IEncrypt ie = null;
			ResponseType resp = ResponseType.None;
			EncryptionType et = EncryptionType.Plaintext;
			ManualResetEventSlim mre = new ManualResetEventSlim ();
			mre.Reset ();
			Application.Invoke (delegate/*(object sender, EventArgs e )*/ {
				EncryptionTypeDialog etd = new EncryptionTypeDialog ();
				resp = (ResponseType)etd.Run ();
				et = etd.GetComboBoxChoice ();
				mre.Set ();
			});

			mre.Wait ();


			if (resp != ResponseType.Ok) {
				return;
			}



			switch (et) {
			case EncryptionType.Plaintext:
				// TODO alert user, nothing to be done.
				return;
			case EncryptionType.Rijndaelio:
				ie = new Rijndaelio ();



				break;

			case EncryptionType.TrippleEntente:
				ie = TrippleEntenteCreationDialog.DoDialog ();

				break;
			}


			Account = Seed.GetPublicRippleAddress ();

			//byte[] payload = seed.getBytes();
			ResponseType rt = ResponseType.None;
			RippleSeedAddress throwawayseed = null;

			mre.Reset ();
			Application.Invoke ( (object sender, EventArgs e) => {
				using (RandomSeedGenerator rsg = new RandomSeedGenerator ()) {
					rt = (ResponseType)rsg.Run ();
					throwawayseed = rsg.GetGeneratedSeed ();
					mre.Set ();
				}
			});
			mre.Wait ();


			if (rt != ResponseType.Ok || throwawayseed == null) {
				return;
			}

			byte [] salty = throwawayseed.GetBytes ();

			Salt = Base58.Encode (salty);

			byte [] enc = ie.Encrypt (Seed, salty);

			Encrypted_Wallet = Base58.Encode (enc);

			Encryption_Type = ie.Name;

			Seed = null;
		}

		public void DecryptWithSideEffects ()
		{


			RippleSeedAddress decr = DecryptNoSides ();

			if (decr == null) {
				return;
			}
			Seed = decr;
			Account = Seed.GetPublicRippleAddress ();
			Encryption_Type = "plaintext";
			Encrypted_Wallet = null;
			Salt = null;
		}

		private RippleSeedAddress DecryptNoSides ()
		{
#if DEBUG
			string method_sig = clsstr + nameof(DecryptNoSides) + DebugRippleLibSharp.both_parentheses;

#endif

			try {
				//String password, ,

				IEncrypt ie = null;
				bool worked = Enum.TryParse<EncryptionType> (this.Encryption_Type, out EncryptionType enc);
				if (!worked) {
					// TODO alert user of failure, 
				}

				switch (enc) {
				case EncryptionType.Rijndaelio:
					ie = new Rijndaelio ();
					break;

				case EncryptionType.TrippleEntente:

					ie = TrippleEntenteDialog.DoDialog ();
					break;

				}

				if (ie == null) {
					// TODO 
					return null;
				}

				byte [] salty = Base58.Decode (Salt);
				byte [] decoded = Base58.Decode (Encrypted_Wallet);


				byte [] decrypted = ie.Decrypt (decoded, salty, Account);

				RippleSeedAddress decryptedSeed = new RippleSeedAddress (decrypted);

				return decryptedSeed;
			}


#pragma warning disable 0168
			catch (Exception e) {
#pragma warning restore 0168

#if DEBUG
				if (DebugIhildaWallet.RippleWallet) {
					Logging.ReportException (method_sig, e);
				}
#endif
				return null;
			}
		}

		public RippleSeedAddress GetDecryptedSeed ()
		{

			if (Seed != null) {
				return Seed;
			}

			if (Encrypted_Wallet == null) {
				throw new NullReferenceException ();
			}

			if (Account == null) {
				throw new MissingFieldException ("account public key must be specified for decryption");
			}

			/*
			string pass = null;

			ManualResetEventSlim mre = new ManualResetEventSlim ();
			mre.Reset ();
			Application.Invoke ( delegate {
				
				pass = PasswordDialog.doDialog ();
				mre.Set();
			});
			mre.Wait ();
			*/

			//IEncrypt ie = new Rijndaelio ();

			//byte[] salty = Base58.decode (salt);
			RippleSeedAddress see = DecryptNoSides ();



			return see;


		}

		public void ClearSensitiveIfEncrypted ()
		{

		}

		public bool ForgetDialog ()
		{
#if DEBUG

			if (DebugIhildaWallet.RippleWallet) {
				Logging.WriteLog ("RippleWallet : method forget : begin\n");
			}
#endif

			if (File.Exists (WalletPath)) {
#if DEBUG
				if (DebugIhildaWallet.RippleWallet) {
					Logging.WriteLog ("RippleWallet : method forget : Wallet Exists\n");
				}
#endif





				AreYouSure ayu = new AreYouSure ("You are about to delete wallet "
								 + (this.WalletName ?? " ")
								 + " from the harddrive."

								 + " Make sure you have a copy of your address and secret otherwise your funds at address "
								 + this.GetStoredReceiveAddress ()
								 + " would be lost\n\n"
								 + "Click \"Ok\" to delete file "
								 + WalletPath) {
					Modal = true
				};
				ResponseType ret = (ResponseType)ayu.Run ();
				ayu.Destroy ();

				if (ret == ResponseType.Ok) {
#if DEBUG
					if (DebugIhildaWallet.RippleWallet) {
						Logging.WriteLog ("Wallet : method forget : User clicked ok, deleting file" + WalletPath + "\n");
					}
#endif


					if (WalletManager.currentInstance != null && WalletName != null) {
						//WalletManager.currentInstance.deleteWallet(walletname);

						WalletManager.currentInstance.DeleteWallet (this);
					}



					return true;
				}




				return false;



			}
#if DEBUG
			if (DebugIhildaWallet.RippleWallet) {
				Logging.WriteLog ("Wallet : method forget : Wallet DOESN'T Exists\n");
			}
#endif
			MessageDialog.ShowMessage ("There is no wallet to delete");
			return false;
		}


		public void Forget ()
		{

#if DEBUG
			String method_sig = clsstr + nameof(Forget) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.RippleWallet) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			ThreadStart ts = new ThreadStart (
				delegate {

#if DEBUG
					if (DebugIhildaWallet.RippleWallet) {
						Logging.WriteLog (method_sig + "delegate begin");

					}
#endif
					try {
						if (WalletPath == null) {
#if DEBUG
							if (DebugIhildaWallet.RippleWallet) {
								Logging.WriteLog (method_sig + "walletpath == null, returning");
							}
#endif
							return;
						}

						if (!File.Exists (WalletPath)) {
#if DEBUG
							if (DebugIhildaWallet.RippleWallet) {
								Logging.WriteLog (method_sig + "the file doesn't exist, returning");
							}
#endif
							return;
						}

						File.Delete (WalletPath);
#if DEBUG
						if (DebugIhildaWallet.RippleWallet) {
							Logging.WriteLog (method_sig + "wallet successfully removed from hard drive");
						}
#endif
						return;
					}

#pragma warning disable 0168
			catch (Exception e) {
#pragma warning restore 0168

#if DEBUG
						if (DebugIhildaWallet.RippleWallet) {
							Logging.ReportException (method_sig, e);
						}
#endif
						// todo debug
						return;
					}
				});

#if DEBUG
			if (DebugIhildaWallet.RippleWallet) {
				Logging.WriteLog (method_sig + "starting thread");
			}
#endif
			Thread th = new Thread (ts);
			th.Start ();

			//Task.Run (ts);

		}

		public String ToJsonString ()
		{
#if DEBUG
			string method_sig = clsstr + " toJsonString () : ";
#endif

			try {  // just in case :) I don't trust dyamic variables, especially in c#


				//dynamic d = new DynamicJson ();
				JsonWallet jw = new JsonWallet ();
				/*
				if (seed != null) {
					string s = seed.ToString ();
					jw.secret = s;
					RippleAddress r = seed.getPublicRippleAddress ();
					if (r != null) {
						jw.Account = r.ToString ();
					} else {
						// todo debug
					}
				}

				if (Account != null) {
					jw.Account = this.Account;
				}

				if (encrypted_wallet != null) {
					jw.encrypted_wallet = encrypted_wallet;

					if (encryption_type!=null) {
						jw.encryption_type = encryption_type;
					}

				} else {
					if (seed!=null && seed.ToString()!=null) {
						jw.secret = seed.ToString();
					}
				}

				if (walletname!=null) {
					jw.name = walletname;
				}
				*/

				if (Seed != null) {

					jw.Secret = Seed.ToString ();
				}

				jw.Account = this.Account;
				//jw.network = this.
				jw.Encrypted_Wallet = this.Encrypted_Wallet;

				jw.Encryption_Type = this.Encryption_Type;
				jw.Name = this.WalletName;
				jw.Salt = this.Salt;

				jw.LastKnownLedger = LastKnownLedger;

				string st = DynamicJson.Serialize (jw);
				return st;

			}

#pragma warning disable 0168
			catch (Exception e) {
#pragma warning restore 0168

#if DEBUG
				if (DebugIhildaWallet.RippleWallet) {
					Logging.ReportException (method_sig, e);
				}
#endif
				return null;
			}
		}

		public void ExportWallet ()
		{
#if DEBUG
			if (DebugIhildaWallet.RippleWallet) {
				Logging.WriteLog ("RippleWallet : method ExportWallet : begin\n");
			}
#endif

			FileChooserDialog fcd = new FileChooserDialog ("Export Wallet",
									   /*PaymentWindow.currentInstance*/null,
									   FileChooserAction.Save,
									   "Cancel", ResponseType.Cancel,
									   "Save", ResponseType.Accept);

			if (fcd.Run () == (int)ResponseType.Accept) {
#if DEBUG
				if (DebugIhildaWallet.RippleWallet) {
					Logging.WriteLog ("Wallet : method ExportWallet : user chose to export to file " + fcd.Filename + "\n");
				}
#endif

				Save (fcd.Filename);
			}

			fcd.Destroy ();
		}

		public bool Save ()
		{
			if (WalletPath == null) {
				// todo debug
				return false;
			}

			return this.Save (WalletPath);
		}


		private object fileLock = new object ();

		public bool Save (String path)
		{

			StringBuilder sb = new StringBuilder ();
#if DEBUG

			if (DebugIhildaWallet.RippleWallet) {

				sb.Append ("RippleWallet : save ( ");
				sb.Append (path ?? "null");
				sb.Append (" ) : ");
				String method_sig = sb.ToString ();
				sb.Append (DebugRippleLibSharp.beginn);
				Logging.WriteLog (sb.ToString ());
				sb.Clear ();
			}
#endif

			if (path == null) {
				// todo debug
				return false;
			}

			sb.AppendLine (ToJsonString ().Trim ());

			String json = sb.ToString ();
			/*
			if (s == null) {
				MessageDialog.showMessage ("hey, the developer wants to see this error (s == null) in RippleWallet save");

				return false;
			}
			*/

			if (sb.ToString ().Equals ("")) {
				MessageDialog.ShowMessage ("hey, the developer wants to see this error (s == \"\")  in RippleWallet save");

				return false;
			}

			try {

				lock (fileLock) {

					if (!File.Exists (path)) {
						File.WriteAllText (path, sb.ToString (), Encoding.UTF8);
						return true;
					}

					sb.Clear ();

					sb.Append (path);
					sb.Append (FileHelper.TEMP_EXTENTION);
					string tempPath = sb.ToString ();


					sb.Clear ();
					sb.Append (path);
					sb.Append (FileHelper.BACKUP_EXT);
					string backup = sb.ToString ();



					byte [] data = Encoding.UTF8.GetBytes (json);


					if (File.Exists (backup)) {
						File.Delete (backup);
					}

					using (var tempFile = File.Create (tempPath, 1024, FileOptions.WriteThrough)) {
						tempFile.Write (data, 0, data.Length);
					}

					File.Replace (tempPath, path, backup);



				}
				return true;
			} catch (Exception e) {
				//Logging.writeLog(method_sig + "Exception thrown, " + e.Message);
#if Debug
				Logging.reportException(method_sig, e);
					
#endif

				sb.Clear ();
				sb.Append ("hey, the developer wants to see this error\n\n");
				sb.Append (e.Message);
				sb.Append ("\n\n");
				sb.Append (e.StackTrace);

				MessageDialog.ShowMessage (sb.ToString ());
				return false;
			}

		}


#if DEBUG
		private const string clsstr = nameof (RippleWallet) + DebugRippleLibSharp.colon;
#endif
	}
}
