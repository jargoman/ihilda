using System;
using System.IO;
using Gtk;
using Codeplex.Data;
using System.Collections.Generic;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public class WalletManager
	{

		public WalletManager ()
		{
			currentInstance = this; // this MUST appear before loadWallets ();
#if DEBUG
			if (DebugIhildaWallet.WalletManager) {
				Logging.WriteLog(clsstr + "new");
			}
#endif

			InitWallets (); // initializes variables

			LoadWallets ();



		}

		public bool IsEmpty ()
		{

			//lock (walletLock) { 
			return this.wallets.Count == 0;
			//}
		}

#if DEBUG
		private const string clsstr = nameof (WalletManager) + DebugRippleLibSharp.colon;
#endif


#pragma warning disable RECS0122 // Initializing field with default value is redundant

		public static WalletManager currentInstance = null;

		public Dictionary<String, RippleWallet> wallets = null;

#pragma warning restore RECS0122 // Initializing field with default value is redundant

		//public static object walletLock = new object();


		//public static readonly bool prefer_filename = true;
		//public List<RippleWallet> wallets = null; 

		public void LoadWallets ()
		{



#if DEBUG
			String method_sig = clsstr + nameof (LoadWallets) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.WalletManager) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}
#endif



			String [] paths = FileHelper.GetDirFileNames (FileHelper.WALLET_FOLDER_PATH);

			foreach (String path in paths) {
				if (path == null) {
#if DEBUG
					Logging.WriteLog(method_sig + "null path?");
#endif
					return;
				}

				try {


					if (File.Exists (path)) {
#if DEBUG
						if (DebugIhildaWallet.WalletManager) {
							Logging.WriteLog(method_sig + "Path exists");
						}
#endif

						//if (path.EndsWith (".ice")) {
						if (path.EndsWith (".ice", StringComparison.CurrentCulture)) {
#if DEBUG
							if (DebugIhildaWallet.WalletManager) {
								Logging.WriteLog(method_sig + "is an ice file");
							}
#endif

							String wal = File.ReadAllText (path);
#if DEBUG
							if (DebugIhildaWallet.WalletManager) {
								Logging.WriteLog(method_sig + "Wallet contents = " + DebugIhildaWallet.AssertAllowInsecure(wal));
							}
#endif
							String name = Path.GetFileNameWithoutExtension (path);

#if DEBUG
							if (DebugIhildaWallet.WalletManager) {
								Logging.WriteLog(method_sig + "name = " + DebugIhildaWallet.ToAssertString(name));
							}
#endif


							RippleWallet rw = ProcessJsonWallet (wal, name, true);   //createWallet(wal, name);

							if (rw == null) {
#if DEBUG
								Logging.WriteLog(method_sig + "Could not process wallet " + name);
#endif

								continue;
							}

							if (rw.WalletName == null) {
#if DEBUG
								Logging.WriteLog(method_sig + "rw.walletname == null");
#endif

								// Todo
								continue;
							}

							//lock (walletLock) {
							wallets.Add (rw.WalletName, rw);
							//}
							rw.WalletPath = path;

						}

#if DEBUG
						else {

							Logging.WriteLog (method_sig + "Warning : Ignoring non ice file " + path);
						}
#endif

					}
					else {
						// Todo debug
						// file doesn't exist
					}
				}

				catch (Exception e) {
					Logging.WriteLog(e.Message);
					continue;
				}



			}
		}



		/*
		public static RippleWallet createWallet (String json, String name) {

			try {

				RippleWallet wall;
				dynamic dana = DynamicJson.Parse(json);

				if ((dana.IsDefined( "secret" ) && dana.secret is String)) {
					String secret = dana.secret;
					if (secret != null) {
						wall = new RippleWallet(secret);
						wall.walletname = name;
						return wall;
					}

					else {
						return null;
					}
				}

				else if (dana.IsDefined( "enrypted" ) && dana.encrypted is String) {
					String enc_type = null;
					String enc = dana.encrypted;

					if (enc==null) {
						// todo debug
						return null;
					}

					if (dana.IsDefined ("encrypted_type") && dana.encrypted_type is String) {
						enc_type = dana.encrypted_type;
					}
					else {
						enc_type = Rijndaelio.default_name;
					}

					wall = new RippleWallet(enc, enc_type);
					wall.walletname = name;
					return wall;
				}

				else {
					// todo invalid wallet  
					return null;
				}
			}

			catch (Exception e) {
				// todo
				return null;
			}

		}
		*/

		public RippleWallet LookUp (String s) {
			//lock (walletLock) {
			if (!wallets.TryGetValue (s, out RippleWallet rw)) {
				// todo debug
			}
			//}
			return rw;
		}

		public bool AddWallet (RippleWallet wallet)
		{
#if DEBUG
			String method_sig = clsstr + nameof(AddWallet) + DebugRippleLibSharp.left_parentheses + (wallet?.ToString () ?? "null") + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.WalletManager) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			if (wallet == null) {
				// todo debug
#if DEBUG
				if (DebugIhildaWallet.WalletManager) {
					Logging.WriteLog(method_sig + "wallet == null returning false");
				}
#endif
				return false;
			}




			if (wallet.WalletName == null) {
#if DEBUG
				if (DebugIhildaWallet.WalletManager) {
					Logging.WriteLog(method_sig + "wallet.walletname == null, returning false");
				}
#endif
				return false;
			}

			//String name = NameMaker.requestName( wallet.walletname, PluginType.WALLET );  // double check the name is available

			//if ( name.Equals ( wallet.walletname )) {  // are they equal?
#if DEBUG
				if (DebugIhildaWallet.WalletManager) {
					Logging.WriteLog(method_sig + "wallet name is available");
				}
#endif



				//String path = FileHelper.getWalletPath(name);
			String path = FileHelper.GetWalletPath(wallet.WalletName);
				//if ( File.Exists(path) ) {
					// TODO OMFG so many debug, this won't happen
				//	if (Debug.WalletManager) {
				//		Logging.write(methog_sig + "");
				//	}

					// what to do if path of plugin already exists ]
						// should never happen
				//}

			wallet.WalletPath = path;


			//lock (walletLock) {
				try {
					this.wallets.Add (wallet.WalletName, wallet);
				}

				catch (Exception exc) {
					Logging.WriteLog(exc.Message);
					return false;

				}

				if (wallet.Save(path)) // boom shakalaka 
				{
					return true;
				}

				try {
					this.wallets.Remove(wallet.WalletName); // might as well remove it so the name can be reused. 
				}

				catch (Exception exb) {
					Logging.WriteLog (exb.Message);

					//Application.Quit(); // or recommend shuttown. 
				}
			//}
			//}

			//else {
				// yikes this is mess, the wallet name already exists
			//	if (Debug.WalletManager) {
			//		Logging.write(methog_sig + "walletname already exists");
			//	}


				// todo recover and return true

				return false;
			//}



			//return false;

		}

		//public void deleteWallet (RippleWallet rw)
		//{
		//	this.wallets.r
		//}

		public bool DeleteWallet (String name)
		{
			

#if DEBUG
			String method_sig = null;
			if (DebugIhildaWallet.WalletManager) {
				method_sig = clsstr + nameof (DeleteWallet) + DebugRippleLibSharp.left_parentheses + DebugIhildaWallet.ToAssertString(name ) + ") : ";
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
#endif

			try {

				if (wallets==null) {
#if DEBUG
					if (DebugIhildaWallet.WalletManager) {
						//Logging.write(method_sig + "wallets == null, returning false");
					}
#endif

					return false;
				}

				// Todo delete wallet
				RippleWallet wall = this.LookUp (name);
				return DeleteWallet(wall);
#pragma warning disable 0168
			} catch (Exception e) {
#pragma warning restore 0168


#if DEBUG
				if (DebugIhildaWallet.WalletManager) {
					Logging.ReportException(method_sig, e);
				}
#endif
			}

			return false;

		}

		public bool DeleteWallet (RippleWallet rw) {
#if DEBUG
			String method_sig = clsstr + "deleteWallet (rw = " + (rw?.GetStoredReceiveAddress () ?? "null") + " ) ";

#endif

			if (rw == null) {
				// todo debug
#if DEBUG
					if (DebugIhildaWallet.WalletManager) {
						Logging.WriteLog(method_sig + "wall == null, returning false");
					}
#endif
				return false;
			}

			//lock (walletLock) {
			rw.Forget();
			if ( wallets.Remove( rw.WalletName)) {
#if DEBUG
				if (DebugIhildaWallet.WalletManager) {
					Logging.WriteLog(method_sig + "wallet successfully removed from dictionary");
				}
#endif

						
				return true; // successfully removed


					//if (Debug.WalletManager) {
					//	Logging.write(method_sig + "failed to remove wallet from Dictionary. Attempting to readd to dictionary");
					//}

					//if (!addWallet(wall)) { // if it didn't succeed RE-ADD THE WALLET
						// todo debug. 
						// wallet only partially removed

					//}
			}
			//}

			return false;
		}

		public void InitWallets ()
		{
			//lock (walletLock) {
				if (wallets == null) {
					wallets = new Dictionary<string, RippleWallet>();//new List<RippleWallet> ();
				} else {
					wallets.Clear();
				}
			//}
		}


		public static void ImportWallet () {
#if DEBUG
			string method_sig = clsstr + nameof (ImportWallet) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.WalletManager) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			if (currentInstance==null) {
#if DEBUG
				if (DebugIhildaWallet.WalletManager) {
					Logging.WriteLog (method_sig + nameof (currentInstance) + "==" + DebugRippleLibSharp.null_str);
				}
#endif

				return;
			}

			FileChooserDialog fcd = new FileChooserDialog ("Import Wallet", 
			                                               /*PaymentWindow.currentInstance*/ null, 
			                                               FileChooserAction.Open,
			                                               "Cancel", ResponseType.Cancel,
			                                               "Open", ResponseType.Accept);


			if (fcd.Run () == (int)ResponseType.Accept) {
#if DEBUG
				if (DebugIhildaWallet.WalletManager) {
					Logging.WriteLog (method_sig + "user chose file: " + DebugIhildaWallet.ToAssertString(fcd.Filename) + "\n");
				}
#endif

				if (fcd.Filename!=null) {
					string nme = Path.GetFileName (fcd.Filename);

					string destFile = System.IO.Path.Combine(FileHelper.WALLET_FOLDER_PATH, nme);

#if DEBUG
					if (DebugIhildaWallet.WalletManager) {
						Logging.WriteLog (
							method_sig + "nme=" + DebugIhildaWallet.ToAssertString (nme) + "\n" +
							method_sig + "destFile=" + DebugIhildaWallet.ToAssertString (destFile) + "\n");			
					}
#endif

					try {
						System.IO.File.Copy (fcd.Filename, destFile);
					}

#pragma warning disable 0168
					catch (FileNotFoundException fnf) {
#pragma warning restore 0168

#if DEBUG
						Logging.WriteBoth (DebugIhildaWallet.ToAssertString(nme) + "File not found");
						if (DebugIhildaWallet.WalletManager) {
							Logging.WriteLog (method_sig + fnf.Message);
						}
#endif
					}

					//catch () {
					String json = LoadWallet (destFile);



					RippleWallet rw = ProcessJsonWallet( json, nme, false);

					if (rw != null) {
						WalletManager.currentInstance.AddWallet(rw);
					}

					else {
						// todo you guessed it. debug 
					}
				}

			}

			fcd.Destroy ();

		}



		public static String LoadWallet ( String path ) {
#if DEBUG
			if (DebugIhildaWallet.WalletManager) {
				Logging.WriteLog ("WalletManager : method loadWallet ( " + path + " ) : begin\n");
			}
#endif

			try {

				Logging.WriteLog ("Looking for wallet at " + path + "\n");

				if (File.Exists(path)) { // a wallet needs to be loaded
					Logging.WriteLog ("Wallet " + path + " Exists!\n");

					// old
					//wallet_bytes = File.ReadAllBytes(path);
					//Wallet.path = path;
					//return true;

					// new
					String wah = File.ReadAllText(path, System.Text.Encoding.UTF8);
					return wah;
				}


			}

			catch (Exception e) {
				Logging.WriteLog(e.Message);
			}

			return null;
			//return this.loadWallet (walletpath);

		}




		public static RippleWallet ProcessJsonWallet ( String plain, String name, bool prefer_filename) {

#if DEBUG
			String method_sig = clsstr + nameof (ProcessJsonWallet ) + DebugRippleLibSharp.left_parentheses + DebugIhildaWallet.AssertAllowInsecure(plain)  +  DebugRippleLibSharp.comma + DebugIhildaWallet.ToAssertString(name) + prefer_filename.ToString() + DebugRippleLibSharp.both_parentheses;  // wow! ?

			if (DebugIhildaWallet.WalletManager) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			// WARNING variable plain contains secret and should never be printed even in debug mode

			if (plain == null) {
#if DEBUG
				if (DebugIhildaWallet.WalletManager) {
					Logging.WriteLog(method_sig + "plain == " + null);
				}
#endif
				return null;
			}

			plain = plain.Trim ();

			String sec = null;
			String acc = null;
			String enc = null;
			String typ = null;
			String nama = null;

			try {

				if (!(plain.Contains ("{") && plain.Contains ("}"))) { // big bad validity test   
					//MessageDialog.showMessage ("Invalid wallet. It must be a valid json object\n");
#if DEBUG
					Logging.WriteLog(method_sig + "Invalid wallet. It must be a valid json object");
#endif
					return null;
				}

				// it's most likely a json object
#if DEBUG
				if (DebugIhildaWallet.WalletManager) {
					Logging.WriteLog (method_sig + "plaintext passed basic validity test\n");
				}
#endif

				JsonWallet jwallet; 

#if DEBUG
				String warn = method_sig + "Unable to parse wallet, invalid json = " + DebugIhildaWallet.AssertAllowInsecure( plain );
#endif
				try {
					

					jwallet = DynamicJson.Parse(plain);


					if (jwallet==null) {
#if DEBUG

						Logging.WriteLog( warn );
#endif
							return null;
					}
				}

#pragma warning disable 0168
				catch (Exception clap) {
#pragma warning restore 0168

#if DEBUG
					Logging.WriteLog(warn + DebugRippleLibSharp.exceptionMessage + clap.Message + clap.StackTrace);
#endif
					return null;
					//Todo invalid wallet
				}


				RippleWalletTypeEnum walletTypeEnum = default(RippleWalletTypeEnum);

				try {
					walletTypeEnum = (IhildaWallet.RippleWalletTypeEnum)Enum.Parse (typeof (RippleWalletTypeEnum), jwallet.AccountType);
				} catch (Exception e) {
					Logging.WriteLog (warn + DebugRippleLibSharp.exceptionMessage + e.Message + e.StackTrace);
					return null;
				}



				/*** *** begin secret *** ***/

				if (walletTypeEnum == RippleWalletTypeEnum.Master) {
					if (jwallet.Secret != null) {
#if DEBUG
						if (DebugIhildaWallet.WalletManager) {
							Logging.WriteLog (method_sig + "secret is defined\n");
						}
#endif

						sec = jwallet.Secret;



					} else {
						//MessageDialog.showMessage ("Secret is not defined\n");
#if DEBUG

						Logging.WriteLog (method_sig + "Secret is not defined");  // 
#endif
						//return false;  // do not return. secret may be encrypted

					}
				} else if (walletTypeEnum == RippleWalletTypeEnum.Regular) {
					if (jwallet.Regular_Key_Secret != null) {
#if DEBUG
						if (DebugIhildaWallet.WalletManager) {
							Logging.WriteLog (method_sig + "secret is defined\n");
						}
#endif

						sec = jwallet.Regular_Key_Secret;



					} else {
						//MessageDialog.showMessage ("Secret is not defined\n");
#if DEBUG

						Logging.WriteLog (method_sig + "Secret is not defined");  // 
#endif
						//return false;  // do not return. secret may be encrypted

					}
				}
				/*** *** end secret *** ***/

				/*** *** begin account *** ***/

				if (walletTypeEnum == RippleWalletTypeEnum.Master) {
					if (jwallet.Account != null) {
#if DEBUG

						if (DebugIhildaWallet.WalletManager) {
							Logging.WriteLog (method_sig + "Account is defined\n");
						}
#endif


						acc = jwallet.Account;

					} else {
#if DEBUG

						//MessageDialog.showMessage ("Account is not Defined\n");
						if (DebugIhildaWallet.WalletManager) {
							Logging.WriteLog (method_sig + "Account is NOT defined");
						}
#endif
						//return false; // don't return. account does not need to be defined
					}
				}

			 else if (walletTypeEnum == RippleWalletTypeEnum.Regular) {
					if (jwallet.Regular_Key_Account != null) {
#if DEBUG

						if (DebugIhildaWallet.WalletManager) {
							Logging.WriteLog (method_sig + "Account is defined\n");
						}
#endif


						acc = jwallet.Regular_Key_Account;

					} else {
#if DEBUG

						//MessageDialog.showMessage ("Account is not Defined\n");
						if (DebugIhildaWallet.WalletManager) {
							Logging.WriteLog (method_sig + "Account is NOT defined");
						}
#endif
						//return false; // don't return. account does not need to be defined
					}

			}
				/*** *** end account *** ***/

				/*** *** begin encrypted_wallet *** ***/

				if (walletTypeEnum == RippleWalletTypeEnum.Master) {
					if (jwallet.Encrypted_Wallet != null) {
#if DEBUG

						if (DebugIhildaWallet.WalletManager) {
							Logging.WriteLog (method_sig + "encrypted_wallet is defined");
						}
#endif


						enc = jwallet.Encrypted_Wallet;

					} else {
#if DEBUG

						if (DebugIhildaWallet.WalletManager) {
							Logging.WriteLog (method_sig + "encrypted_wallet is NOT defined");
						}
#endif
					}
				} else if (walletTypeEnum == RippleWalletTypeEnum.Regular) {
					if (jwallet.Encrypted_Regular_Wallet != null) {
#if DEBUG

						if (DebugIhildaWallet.WalletManager) {
							Logging.WriteLog (method_sig + "encrypted_wallet is defined");
						}
#endif


						enc = jwallet.Encrypted_Regular_Wallet;

					} else {
#if DEBUG

						if (DebugIhildaWallet.WalletManager) {
							Logging.WriteLog (method_sig + "encrypted_wallet is NOT defined");
						}
#endif
					}
				}
				/*** *** end encrypted_wallet *** ***/


				/*** *** begin encryption_type *** ***/

				if (walletTypeEnum == RippleWalletTypeEnum.Master) {

					if (jwallet.Encryption_Type != null) {
#if DEBUG

						if (DebugIhildaWallet.WalletManager) {
							Logging.WriteLog (method_sig + "encryption_type is defined");
						}
#endif


						typ = jwallet.Encryption_Type;
					} else {
#if DEBUG

						if (DebugIhildaWallet.WalletManager) {
							Logging.WriteLog (method_sig + "encryption_type is NOT defined");
						}
#endif
					}

				}

			else if (walletTypeEnum == RippleWalletTypeEnum.Regular) {
					if (jwallet.Regular_Key_Encryption_Type != null) {
#if DEBUG

						if (DebugIhildaWallet.WalletManager) {
							Logging.WriteLog (method_sig + "encryption_type is defined");
						}
#endif


						typ = jwallet.Regular_Key_Encryption_Type;
					} else {
#if DEBUG

						if (DebugIhildaWallet.WalletManager) {
							Logging.WriteLog (method_sig + "encryption_type is NOT defined");
						}
#endif
					}

			}
					/*** *** end encryption_type *** ***/



					/*** *** begin walletname *** ***/
					if (jwallet.Name != null) {
					
#if DEBUG
						
						if (DebugIhildaWallet.WalletManager) {
							Logging.WriteLog(method_sig + "name is defined");
						}
#endif

					nama = jwallet.Name;
						
					}

					else {
#if DEBUG

						if (DebugIhildaWallet.WalletManager) {
							Logging.WriteLog(method_sig + "name is NOT defined");
						}
#endif
					}
					/*** *** end walletname *** ***/


					/*** *** BEGIN WALLET CREATION *** ***/

					RippleWallet rw = null;
					if (sec!=null) {
#if DEBUG

						if (DebugIhildaWallet.WalletManager) {
							Logging.WriteLog(method_sig + "sec != null");
						}
#endif

					rw = new RippleWallet(sec, walletTypeEnum);


						if (acc!=null) {
							if (!acc.Equals(rw.GetStoredReceiveAddress())) {
								// todo warn account does not match secret
							}
						}

					}

					else if (enc!=null) { // I know, I know, if else madness!!!
					rw = new RippleWallet(enc,typ,acc, walletTypeEnum);

					}


					else  {
						// todo, insufficient information to reconstruct wallet
						return null;
					}

					if (rw == null) { // impossible ?

					}

					if (currentInstance!=null) {
						rw.WalletName = currentInstance.DecideName(nama,name,prefer_filename);
					}

					else {

					}
#if DEBUG

					if (DebugIhildaWallet.WalletManager) {
						Logging.WriteLog(method_sig + "returning a valid wallet" );
					}
#endif


				if (walletTypeEnum == RippleWalletTypeEnum.Regular) {
					rw.Account = jwallet.Account;
				}

				rw.Salt = jwallet.Salt;
				rw.LastKnownLedger = jwallet.LastKnownLedger;

				//rw.Regular_Key_Account = jwallet.Regular_Key_Account;
				//rw.Regular_Seed = jwallet.Regular_Key_Secret;
				//rw.Encrypted_Regular_Wallet = jwallet.Encrypted_Regular_Wallet;
				//rw.Regular_Key_Encryption_Type = jwallet.Regular_Key_Encryption_Type;

				rw.AccountType = walletTypeEnum;
					return rw;

				

#pragma warning disable 0168
			} catch (Exception e) {
#pragma warning restore 0168

#if DEBUG

				Logging.WriteLog (method_sig + "uncaught exception : " + e.Message + "\n");
#endif
				return null;
			}
		}


		public String DecideName (String fileName, String givenName, bool prefer_filename) {

#if DEBUG
			String method_sig = clsstr + nameof (DecideName) + DebugRippleLibSharp.left_parentheses + nameof (fileName) + DebugRippleLibSharp.equals + DebugIhildaWallet.ToAssertString(fileName) + DebugRippleLibSharp.comma + nameof (givenName) + DebugRippleLibSharp.equals + DebugIhildaWallet.ToAssertString(givenName) + DebugRippleLibSharp.comma + nameof (prefer_filename) + DebugRippleLibSharp.equals + (prefer_filename.ToString()) + DebugRippleLibSharp.right_parentheses;
#endif

			String returnMe = null;

#if DEBUG
			if (DebugIhildaWallet.WalletManager) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
#endif

			if ( fileName == null && givenName != null ) {
				returnMe = NameMaker.RequestName(givenName, PluginType.WALLET);
			}

			else if (givenName == null && fileName != null) {
				returnMe = NameMaker.RequestName(fileName, PluginType.WALLET);
			}

			else if (fileName != null && givenName != null) {
				if (fileName.Equals(givenName)) {
					returnMe = NameMaker.RequestName(fileName, PluginType.WALLET);
				}

				else {
					if ("".Equals(fileName)) {
						returnMe = NameMaker.RequestName(givenName, PluginType.WALLET);
					}

					if ("".Equals(givenName)) {
						returnMe = NameMaker.RequestName(fileName, PluginType.WALLET);
					}

					String ret_fileName = NameMaker.RequestName(fileName, PluginType.WALLET);
					String ret_givenName = NameMaker.RequestName(givenName, PluginType.WALLET);

					if (ret_fileName.Equals(fileName) && !ret_givenName.Equals(givenName)) {
						returnMe = ret_fileName;
					}

					if (!ret_fileName.Equals(fileName) && ret_givenName.Equals(givenName)) {
						returnMe = ret_givenName;
					}



					returnMe = prefer_filename ? NameMaker.RequestName( fileName, PluginType.WALLET) : NameMaker.RequestName( givenName, PluginType.WALLET);
				}
			}

#if DEBUG
			if (DebugIhildaWallet.WalletManager) {
				Logging.WriteLog(method_sig + "returning " + DebugIhildaWallet.ToAssertString(returnMe));
			}
#endif

			return returnMe; // will never happen but the compiler doesn't know that

		}


		public bool Replace (String old_wallet, String new_wallet)
		{
#if DEBUG
			String method_sig = clsstr + "replace (old_wallet=" + (old_wallet ?? "null") + ", new_wallet=" + (new_wallet ?? "null");
#endif

			RippleWallet ol = LookUp(old_wallet);
			RippleWallet ne = LookUp(new_wallet);

			if ( ol == null) { //
				// todo alert user
#if DEBUG
				if (DebugIhildaWallet.WalletManager) {
					Logging.WriteLog(method_sig + "ol == null");
				}
#endif

				return false;
			}

			if (ne == null) {
				// todo alert user
#if DEBUG
				if (DebugIhildaWallet.WalletManager) {
					Logging.WriteLog(method_sig + "ne == null");
				}
#endif
				return false;
			}

			return Replace ( ol, ne );
		}

		public bool Replace (RippleWallet old_wallet, RippleWallet new_wallet)
		{
#if DEBUG
			String method_sig = clsstr + nameof (Replace) + DebugRippleLibSharp.left_parentheses + nameof (old_wallet) + DebugRippleLibSharp.equals + DebugIhildaWallet.ToAssertString(old_wallet) + DebugRippleLibSharp.comma + nameof (new_wallet) + DebugRippleLibSharp.equals + DebugIhildaWallet.ToAssertString(new_wallet) + " ) : ";
#endif

			try {
#if DEBUG
				String isnull = "== null, " + DebugRippleLibSharp.returning;
				if (DebugIhildaWallet.WalletManager) {
					Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
				}
#endif



				if (wallets == null) {
					// todo degug
#if DEBUG
					if (DebugIhildaWallet.WalletManager) {
						Logging.WriteLog (method_sig + nameof (wallets) + DebugRippleLibSharp.space_char + isnull);
					}
#endif
					return false;
				}

				if (old_wallet == null) {
#if DEBUG
					if (DebugIhildaWallet.WalletManager) {
						Logging.WriteLog (method_sig + nameof(old_wallet) + isnull);
					}
#endif
					return false;
				}


				if (new_wallet == null) {
					// todo debug
#if DEBUG
					if (DebugIhildaWallet.WalletManager) {
						Logging.WriteLog (method_sig + "new_wallet " + isnull);
					}
#endif

					return false;
				}

				if (old_wallet.WalletName == null) {
#if DEBUG
					if (DebugIhildaWallet.WalletManager) {
						Logging.WriteLog (method_sig + "old_wallet.walletname " + isnull);
					}
#endif
				}
	
				if (new_wallet.WalletName == null) {
					// todo debug
#if DEBUG
					if (DebugIhildaWallet.WalletManager) {
						Logging.WriteLog (method_sig + "new_wallet.walletname " + isnull);
					}
#endif
					return false;
				}

				/*
			if (old_wallet.walletpath == null) {
				String p = FileHelper.getWalletPath();
				if(File.Exists(p)) {
					try {
						String a = File.ReadAllText(p);

						if (a != null && a.Contains("{") && a.Contains("}") && a.Contains(old_wallet.walletname)) {
							old_wallet.walletpath = p;
							goto OLD_SET;
						}

						else {
							return false;
						}
					}

					catch (Exception e) {
						return false;
					}
				}

				else {

				}
			}

			OLD_SET:
			*/


				if (new_wallet.WalletPath == null) {
					// todo debug
#if DEBUG
					if (DebugIhildaWallet.WalletManager) {
						Logging.WriteLog (method_sig + "new_wallet.walletpath == null");
					}
#endif

					new_wallet.WalletPath = FileHelper.GetWalletPath (new_wallet.WalletName);
					if (new_wallet.WalletPath == null) { // never happen
#if DEBUG
						if (DebugIhildaWallet.WalletManager) {
							Logging.WriteLog (method_sig + "new_wallet.walletpath is still null, returning");
						}
#endif
						UpdateUI();
						return false;
					}
				}



				/*
				if ( File.Exists(new_wallet.walletpath) ) {
					// todo. wallet already has that path
					return false;
				}
				*/

				if (!DeleteWallet (old_wallet.WalletName)) {
#if DEBUG
					if (DebugIhildaWallet.WalletManager) {
						Logging.WriteLog (method_sig + "failed to delete wallet");
					}
#endif
					UpdateUI();
					return false;
				}


				if (AddWallet (new_wallet)) {
#if DEBUG
					if (DebugIhildaWallet.WalletManager) {
						Logging.WriteLog (method_sig + "new_wallet added");
					}
#endif
					if (new_wallet.Save ()) {
#if DEBUG
						if (DebugIhildaWallet.WalletManager) {
							Logging.WriteLog (method_sig + "new_wallet saved");
						}
#endif

						UpdateUI();
						return true;
					} 
#if DEBUG
					if (DebugIhildaWallet.WalletManager) {
						Logging.WriteLog (method_sig + "failed to save wallet");
					}
#endif

				} else {
#if DEBUG
					if (DebugIhildaWallet.WalletManager) {
						Logging.WriteLog (method_sig + "failed to add new_wallet");
					}
#endif
				}


				if (AddWallet (old_wallet)) { // if function fails then readd the old wallet
					// todo debug. Could not readd the old wallet
#if DEBUG
					if (DebugIhildaWallet.WalletManager) {
						Logging.WriteLog (method_sig + "failed to READD the old wallet. yikes!!");
					}
#endif
				}

				UpdateUI();
				return false;

#pragma warning disable 0168
			} catch (Exception e) {
#pragma warning restore 0168

#if DEBUG
				Logging.WriteLog(method_sig + e.Message);
#endif
				UpdateUI();
				return false;

			} 
		}


		// TODO remove ui logic from class ??
		public void UpdateUI () {
#if DEBUG
			if (DebugIhildaWallet.WalletManager) {
				Logging.WriteLog(clsstr + "updateUI");
			}
#endif
			if ( WalletManagerWidget.currentInstance!=null) {
				WalletManagerWidget.currentInstance.UpdateUI();
			}
		}


		private static RippleWallet SelectedWallet {
			get;
			set;
		}

		public static RippleWallet GetRippleWallet ()
		{
			return SelectedWallet;
		}

		public static void SetRippleWallet ( RippleWallet rippleWallet)
		{
			SelectedWallet = rippleWallet;
		}


	}
}

