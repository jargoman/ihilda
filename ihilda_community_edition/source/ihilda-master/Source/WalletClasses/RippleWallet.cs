using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading;
using Codeplex.Data;
using Gtk;
using QRCoder;
using RippleLibSharp.Binary;
using RippleLibSharp.Keys;
using RippleLibSharp.Transactions;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public class RippleWallet
	{
		public RippleWallet (String secret, RippleWalletTypeEnum wallettype)
		{


			switch (wallettype) {
			case RippleWalletTypeEnum.Master:
				this.Seed = new RippleSeedAddress (secret);
				this.Account = Seed.GetPublicRippleAddress ()?.ToString ();
				break;
			case RippleWalletTypeEnum.Regular:
				this.Regular_Seed = new RippleSeedAddress (secret);
				this.Regular_Key_Account = Regular_Seed.GetPublicRippleAddress ().ToString ();
				break;
			case RippleWalletTypeEnum.MasterPrivateKey:
				this.PrivateKey = new RipplePrivateKey (secret);
				this.Account = PrivateKey.GetPublicKey ()?.GetAddress ();
				break;
			}


			this.AccountType = wallettype;
			this.Encryption_Type = nameof(EncryptionType.Plaintext);
			NextTransactionSequenceNumber = -1; // TODO what's a good "invalid" default number. 
		}

		public RippleWallet (RippleSeedAddress rseed, RippleWalletTypeEnum wallettype)
		{
			switch (wallettype) {
			case RippleWalletTypeEnum.Master:
				this.Seed = rseed;
				this.Account = rseed.GetPublicRippleAddress ();
				break;
			case RippleWalletTypeEnum.Regular:
				this.Regular_Seed = rseed;
				this.Regular_Key_Account = rseed.GetPublicRippleAddress ().ToString ();
				break;
			case RippleWalletTypeEnum.MasterPrivateKey:
				throw new ArgumentException ("Wallet type master private can not be instantiated wuth a seed\n", nameof (wallettype));
				//break;
			}

			this.AccountType = wallettype;
			NextTransactionSequenceNumber = -1;
		}

		public RippleWallet (String encrypted, String encryptionType, RippleWalletTypeEnum walletType)
		{
			if (encrypted == null) {
				// Todo debug
				return;
			}

			switch (walletType) {
			case RippleWalletTypeEnum.Master:
				this.Encrypted_Wallet = encrypted;
				this.Encryption_Type = encryptionType;

				break;

			case RippleWalletTypeEnum.Regular:
				this.Encrypted_Regular_Wallet = encrypted;
				//this.Encryption_Type = encryptionType;
				this.Regular_Key_Encryption_Type = encryptionType;
				break;

			case RippleWalletTypeEnum.MasterPrivateKey:
				this.Encrypted_Wallet = encrypted;
				this.Encryption_Type = encryptionType;
				break;
			}



			this.AccountType = walletType;

			NextTransactionSequenceNumber = -1;

		}

		public RippleWallet (String encrypted, String encryptionType, String receiveAddress, RippleWalletTypeEnum walletType) : this (encrypted, encryptionType, walletType)
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

		public bool IsEncrypted ()
		{
			if (Encryption_Type == EncryptionType.Plaintext.ToString()) {
				return false;
			}

			if (Encryption_Type == EncryptionType.None.ToString ()) {
				return false;
			}

			if (Regular_Key_Encryption_Type == EncryptionType.Plaintext.ToString()) {
				return false;
			}

			if (Regular_Key_Encryption_Type == EncryptionType.None.ToString ()) {
				return false;
			}

			return true;

		}

		public string GetStoredEncryptionType ()
		{
			if (Encryption_Type != null) {
				return Encryption_Type;
			}


			if (Regular_Key_Encryption_Type != null) {
				return Regular_Key_Encryption_Type;
			}


	                // these functions should only be called After determining both encryption types has failed. 
			if (Encrypted_Wallet == null && Seed != null) {
				return nameof(EncryptionType.Plaintext);

			}

			if (Encrypted_Wallet == null && PrivateKey != null) {
				return nameof (EncryptionType.Plaintext);
			}

			

			if (Encrypted_Regular_Wallet == null && Regular_Seed != null) {
				return nameof (EncryptionType.Plaintext);
			}


			return nameof(EncryptionType.None);
		}
		public string GetStoredReceiveAddress ()
		{
			if (this.AccountType == RippleWalletTypeEnum.MasterPrivateKey) {
				if (this.PrivateKey != null) {

					RipplePublicKey publicKey = this.PrivateKey.GetPublicKey ();
					if (publicKey == null) {
						return publicKey.GetAddress ();
					}
				}

				if (Account != null) {
					return Account;
				}
			}

			if (this.AccountType == RippleWalletTypeEnum.Master) {
				if (Seed != null) {
					return Seed.GetPublicRippleAddress ().ToString ();
				}

				if (Account != null) {
					return Account;
				}
			}

			if (this.AccountType == RippleWalletTypeEnum.Regular) {
				if (Account != null) {
					return Account;
				}

				if (Regular_Seed != null) {
					return Regular_Seed.GetPublicRippleAddress ().ToString ();
				}

				if (Regular_Key_Account != null) {
					return Regular_Key_Account;
				}
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

		private RipplePrivateKey PrivateKey {
			get;
			set;
		}

		public String Encrypted_PrivateKey {
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

		public RippleWalletTypeEnum AccountType {
			get;
			set;
		}


		public String Salt {
			get;
			set;
		}



		public uint? NotificationLedger {
			get;
			set;
		}

		public string WalletPath {
			get;
			set;
		}


		public string BotLedgerPath {
			get {
				return Path.Combine (FileHelper.WALLET_TRACK_PATH, WalletName + ".bot");
			}
		}

		public string NotificationLadgerPath {
			get {
				return Path.Combine (FileHelper.WALLET_TRACK_PATH, WalletName + ".led");
			}
			
		}

		public string Notification {
			get;
			set;
		}

		public RippleCurrency LastKnownNativeBalance {
			get;
			set;
		}

		public string BalanceNote {
			get;
			set;
		}

		public byte CouldNotUpdateBalanceCount {
			get;
			set;
		}


#region regularkey

		public RippleSeedAddress Regular_Seed {
			get;
			set;
		}

		public string Regular_Key_Account {
			get;
			set;
		}

		public string Encrypted_Regular_Wallet {
			get;
			set;
		}

		public string Regular_Key_Encryption_Type {
			get;
			set;
		}

#endregion



		public void EncryptWithSideEffects ()
		{


			ResponseType resp = ResponseType.None;
			EncryptionType et = EncryptionType.None;






			//byte[] payload = seed.getBytes();
			ResponseType rt;
			RippleSeedAddress throwawayseed;
			using (ManualResetEventSlim mre = new ManualResetEventSlim ()) {
				mre.Reset ();
				Application.Invoke (delegate /*(object sender, EventArgs e )*/
				 {

					 EncryptionTypeDialog etd = new EncryptionTypeDialog ();
					 etd.ClearInfoBar ();
					 while (et == EncryptionType.None) {

						 resp = (ResponseType)etd.Run ();

						 if (resp != ResponseType.Ok) {
							 break;
						 }

						 etd?.ClearInfoBar ();
						 et = etd.GetComboBoxChoice ();

					 }
					 etd.Destroy ();
					 mre.Set ();
				 });

				mre.Wait ();


				if (resp != ResponseType.Ok) {
					return;
				}
				rt = ResponseType.None;
				throwawayseed = null;
				mre.Reset ();
				Application.Invoke ((object sender, EventArgs e) => {
					using (RandomSeedGenerator rsg = new RandomSeedGenerator ()) {
						rt = (ResponseType)rsg.Run ();
						throwawayseed = rsg.GetGeneratedSeed ();

						rsg.Destroy ();
						mre.Set ();
					}
				});
				mre.Wait ();
			}

			if (rt != ResponseType.Ok) {
				return;
			}

			if (throwawayseed == null) {
				// TODO debug
				return;
			}

			byte [] salty = throwawayseed.GetBytes ();

			IEncrypt ie = null;

			switch (et) {
			case EncryptionType.Plaintext:
				// TODO alert user, nothing to be done.
				return;
			case EncryptionType.Rijndaelio:
				ie = new Rijndaelio {
					Password = Rijndaelio.GetPasswordCreateInput ()
				};

				break;

			case EncryptionType.TrippleEntente:
				ie = TrippleEntenteCreationDialog.DoDialog ();

				break;
			}


			_encyptWithSideEffects (ie, salty);



		}

		public void _encyptWithSideEffects (IEncrypt ie, byte[] salty)
		{




			if (Account == null) {
				if (this.AccountType == RippleWalletTypeEnum.Master) {
					Account = Seed?.GetPublicRippleAddress ();
				}

				else if (this.AccountType == RippleWalletTypeEnum.MasterPrivateKey) {
					Account = PrivateKey?.GetPublicKey ()?.GetAddress ();
				}
			}

			if (Regular_Key_Account == null && this.AccountType == RippleWalletTypeEnum.Regular) {
				Regular_Key_Account = Regular_Seed?.GetPublicRippleAddress ();
			}




			Salt = Base58.Encode (salty);


			byte [] enc = null;  

			switch (AccountType) {
			case RippleWalletTypeEnum.Master:

				enc = ie.Encrypt (Seed, salty);
				Encrypted_Wallet = Base58.Encode (enc);

				Encryption_Type = ie.Name;
				Regular_Key_Encryption_Type = null;
				Seed = null;
				Regular_Seed = null;
				break;
			case RippleWalletTypeEnum.Regular:

				enc = ie.Encrypt (Regular_Seed, salty);

				Encrypted_Regular_Wallet = Base58.Encode (enc);

				
				Regular_Key_Encryption_Type = ie.Name;
				Encryption_Type = null;
				Seed = null;
				Regular_Seed = null;
				break;
			case RippleWalletTypeEnum.MasterPrivateKey:
				enc = ie.Encrypt (PrivateKey, salty);
				Encrypted_PrivateKey = Base58.Encode (enc);
				Encryption_Type = ie.Name;

				Seed = null;
				Regular_Seed = null;
				Regular_Key_Encryption_Type = null;

				break;

			}


				

		}

		public void DecryptWithSideEffects ()
		{
			

			RippleIdentifier decr = DecryptNoSides ();

			if (decr == null) {
				return;
			}

			switch (AccountType) {
			case RippleWalletTypeEnum.Master:
				Seed = (RippleSeedAddress)decr;
				if (Account == null) {
					Account = Seed.GetPublicRippleAddress ();
				}
				Encryption_Type = nameof (EncryptionType.Plaintext);
				Encrypted_Wallet = null;
				break;
			case RippleWalletTypeEnum.Regular:
				Regular_Seed = (RippleSeedAddress)decr;
				if (Account == null) {
					Account = Regular_Seed.GetPublicRippleAddress ();
				}
				Regular_Key_Encryption_Type = nameof (EncryptionType.Plaintext);
				Encrypted_Regular_Wallet = null;
				break;

			case RippleWalletTypeEnum.MasterPrivateKey:
				PrivateKey = (RipplePrivateKey)decr;
				if (Account == null) {
					Account = PrivateKey.GetPublicKey ().GetAddress ();
				}
				Encryption_Type = nameof (EncryptionType.Plaintext);
				Encrypted_Wallet = null;
				break;
			}



			Salt = null;
		}

		private RippleIdentifier DecryptNoSides ()
		{
#if DEBUG
			string method_sig = clsstr + nameof(DecryptNoSides) + DebugRippleLibSharp.both_parentheses;

#endif
			string enc_typ_str = null;
			string enc_wal_str = null;

			switch (AccountType) {
			case RippleWalletTypeEnum.Master:
				enc_typ_str = this.Encryption_Type;
				enc_wal_str = this.Encrypted_Wallet;

				break;
			case RippleWalletTypeEnum.Regular:
				enc_typ_str = this.Regular_Key_Encryption_Type;
				enc_wal_str = this.Encrypted_Regular_Wallet;
				break;
			case RippleWalletTypeEnum.MasterPrivateKey:
				enc_typ_str = this.Encryption_Type;
				enc_wal_str = this.Encrypted_PrivateKey;
				break;
			}



			try {
				//String password, ,

				IEncrypt ie = null;
				bool worked = Enum.TryParse<EncryptionType> (enc_typ_str, out EncryptionType enc);
				if (!worked) {
					// TODO alert user of failure, 
					MessageDialog.ShowMessage ("Wallet does not support encryption type" + enc_typ_str );
					return null;
				}

				switch (enc) {
				case EncryptionType.Rijndaelio:

					Rijndaelio rijndaelio = RememberRijndaelio ?? Rijndaelio.GetPasswordInput();
					if ( rijndaelio != null && rijndaelio.RememberPassword) {
						RememberRijndaelio = rijndaelio;
					}
					ie = rijndaelio;
					break;

				case EncryptionType.TrippleEntente:

					TrippleEntente tripple = RememberedEntente ?? TrippleEntenteDialog.DoDialog ();
					if (tripple != null && tripple.RememberPassword) {
						RememberedEntente = tripple;
					}

					ie = tripple;

					break;

				}

				if (ie == null) {
					// TODO 
					return null;
				}

				byte [] salty = Base58.Decode (Salt);
				byte [] decoded = Base58.Decode (enc_wal_str);


				byte [] decrypted = ie.Decrypt (
					decoded, 
					salty, 
					Account
				);


				


				RippleSeedAddress decryptedSeed = null;
				RipplePrivateKey decryptedPrivateKey = null;

				if (AccountType == RippleWalletTypeEnum.Master || AccountType == RippleWalletTypeEnum.Regular) {
					try {
						decryptedSeed = new RippleSeedAddress (decrypted);
						if (decryptedSeed != null && AccountType == RippleWalletTypeEnum.Regular && Regular_Key_Account == null) {
							Regular_Key_Account = decryptedSeed?.GetPublicRippleAddress ();
						}

					} catch (Exception e) {
						RememberRijndaelio = null;
						RememberedEntente = null;
						MessageDialog.ShowMessage ("Invalid password. Unable to decrypt seed.");
#if DEBUG
						if (DebugIhildaWallet.RippleWallet) {
							Logging.ReportException (method_sig, e);
						}
#endif
						return null;

					}

					return decryptedSeed;
				} else {
					try {

						decryptedPrivateKey = new RipplePrivateKey (decrypted);

					} catch (Exception e) {
						RememberRijndaelio = null;
						RememberedEntente = null;

						MessageDialog.ShowMessage ("Invalid password. Unable to decrypt private key.");

						#if DEBUG
						if (DebugIhildaWallet.RippleWallet) {
							Logging.ReportException (method_sig, e);
						}
#endif
						return null;

					}

					return decryptedPrivateKey;
				}




			}


#pragma warning disable 0168
			catch (Exception e) {
#pragma warning restore 0168

				RememberRijndaelio = null;
				RememberedEntente = null;

#if DEBUG
				if (DebugIhildaWallet.RippleWallet) {
					Logging.ReportException (method_sig, e);
				}
#endif
				throw e;
			}
		}


		public void ForgetPasswords ()
		{
			RememberedEntente = null;
			RememberRijndaelio = null;
		}
		private Rijndaelio RememberRijndaelio = null;
		private TrippleEntente RememberedEntente = null; 

		public RippleIdentifier GetDecryptedSeed ()
		{


			switch (AccountType) {
			case RippleWalletTypeEnum.Master:
				if (Seed != null) {
					return Seed;
				}

				if (Encrypted_Wallet == null) {
					throw new NullReferenceException (nameof (Encrypted_Wallet) + " is null");
				}
				break;
			case RippleWalletTypeEnum.Regular:
				if (Regular_Seed != null) {
					return Regular_Seed;
				}

				if (Encrypted_Regular_Wallet == null) {
					throw new NullReferenceException (nameof (Encrypted_Wallet) + " is null");
				}
				break;

			case RippleWalletTypeEnum.MasterPrivateKey:
				if (PrivateKey != null) {
					return PrivateKey;
				}

				if (Encrypted_PrivateKey == null) {
					throw new NullReferenceException (nameof (Encrypted_Wallet) + " is null");
				}
				break;
			}



			if (Account == null) {
				throw new MissingFieldException ("Account public key must be specified for decryption");
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
			RippleIdentifier see = DecryptNoSides ();



			return see;


		}

		public void ClearSensitiveIfEncrypted ()
		{
			// TODO
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
				JsonWallet jw = new JsonWallet {
					Secret = Seed?.ToString (),


					Account = this.Account,



					//jw.network = this.
					Encrypted_Wallet = this.Encrypted_Wallet,

					Encryption_Type = this.Encryption_Type,
					Name = this.WalletName,
					Salt = this.Salt,

					//LastKnownLedger = LastKnownLedger,

					AccountType = this.AccountType.ToString (),

					Regular_Key_Secret = this.Regular_Seed?.ToString (),

					Regular_Key_Account = this.Regular_Key_Account,




					Encrypted_Regular_Wallet = this.Encrypted_Regular_Wallet,
					Regular_Key_Encryption_Type = this.Regular_Key_Encryption_Type,

					Private_Key_Master_Secret = this.PrivateKey?.ToString ()
				};

				if (jw.Regular_Key_Account == null && this.Regular_Seed != null) {
					jw.Regular_Key_Account = Regular_Seed?.GetPublicRippleAddress ();
				}

				if (jw.Account == null && Seed != null) {
					jw.Account = Seed?.GetPublicRippleAddress ();
				}


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


		public static object fileLock = new object ();
		public static object ledgerFileLock = new object ();

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



			String json = ToJsonString();

			if (string.IsNullOrWhiteSpace(json)) {
				MessageDialog.ShowMessage ("hey, the developer wants to see this error ( String json = ToJsonString(); // json == null)  in RippleWallet save");

				return false;
			}



			try {

				lock (fileLock) {

					if (!File.Exists (path)) {
						File.WriteAllText (path, json, Encoding.UTF8);
						
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


		public bool SaveBotLedger (uint? ledger, string path)
		{

			//string path = BotLedgerPath;

			StringBuilder sb = new StringBuilder ();
#if DEBUG

			if (DebugIhildaWallet.RippleWallet) {

				sb.Append ("RippleWallet : saveBotLedger ( ");
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


			LedgerSave ledgerSave = new LedgerSave {
				Ledger = ledger
			};
			String json = DynamicJson.Serialize (ledgerSave);

			if (string.IsNullOrWhiteSpace (json)) {
				MessageDialog.ShowMessage ("hey, the developer wants to see this error ( String json = ToJsonString(); // json == null)  in RippleWallet save");

				return false;
			}



			try {

				lock (ledgerFileLock) {

					if (!File.Exists (path)) {
						File.WriteAllText (path, json, Encoding.UTF8);

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

		private Gdk.Pixbuf _Pixbuf = null;
		public Gdk.Pixbuf GetQrCode ()
		{

			if (_Pixbuf == null) {
				QRCodeGenerator qRCodeGenerator = QRCodeGenerator;


				QRCodeData addressqrCodeData = qRCodeGenerator.CreateQrCode (this.GetStoredReceiveAddress (), QRCodeGenerator.ECCLevel.Q);
				QRCode qrCodeAdd = new QRCode (addressqrCodeData);

				Gdk.Pixbuf puf = global::Gdk.Pixbuf.LoadFromResource ("IhildaWallet.Images.xrp-symbol-black-25x25.png");


				TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));

       
				Bitmap xmap = (Bitmap)tc.ConvertFrom(puf.SaveToBuffer("png")); 

				Bitmap bitmap = qrCodeAdd.GetGraphic (6, System.Drawing.Color.DarkBlue, System.Drawing.Color.LavenderBlush, xmap, 16, 4, false);
				Bitmap qrCodeImageAdd = bitmap; //qrCodeAdd.GetGraphic (5, System.Drawing.Color.Black, System.Drawing.Color.White, true);


				MemoryStream ms = new MemoryStream ();
				qrCodeImageAdd.Save (ms, ImageFormat.Png);
				ms.Position = 0;
				_Pixbuf = new Gdk.Pixbuf (ms);
			}
			return _Pixbuf;

		}

		private static QRCodeGenerator QRCodeGenerator {
			get {
				if (_QRCodeGenerator == null) {
					_QRCodeGenerator = new QRCodeGenerator ();
				}
				return _QRCodeGenerator;
			}
		}

		private static QRCodeGenerator _QRCodeGenerator = null;


#if DEBUG
		private const string clsstr = nameof (RippleWallet) + DebugRippleLibSharp.colon;
#endif
	}



	public class LedgerSave {
		public UInt32? Ledger {
			get;
			set;
		}


		public static LedgerSave LoadLedger (String path)
		{
#if DEBUG
			string method_sig = nameof (LedgerSave) + DebugRippleLibSharp.colon + nameof (LoadLedger);

			if (DebugIhildaWallet.WalletManager) {
				Logging.WriteLog (method_sig + path + DebugRippleLibSharp.beginn);
			}

#endif

			try {

				Logging.WriteLog ("Looking for wallet at " + path + "\n");

				if (File.Exists (path)) { // 
					Logging.WriteLog ("Wallet " + path + " Exists!\n");

					String wah = File.ReadAllText (path, Encoding.UTF8);

					LedgerSave ledge = DynamicJson.Parse (wah);
					return ledge;
				}


			} catch (Exception e) {
				Logging.WriteLog (e.Message);
			}

			return null;


		}
	}
}

