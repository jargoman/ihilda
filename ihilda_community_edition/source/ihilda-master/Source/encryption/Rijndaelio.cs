/*
 *	License : Le Ice Sense 
 */

using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using Gtk;
using RippleLibSharp.Keys;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public class Rijndaelio : IEncrypt
	{
		/*
		public Rijndaelio ()
		{
			//this.Name = default_name;
		}
		*/




		public static readonly string default_name = nameof (Rijndaelio);

		public static bool IsDescribedByString (String s) {


			return (s.Equals(default_name) || s.Equals("default") || s.Equals("default_encryption"));
			
		}


		/*
		private static readonly byte [] SALT = new byte[] { 
			0x24, 0xa7, 0xfc, 0x12, 
			0x90, 0xb3, 0x5e, 0x6d, 
			0xe8, 0xc1, 0xa8, 0x3d, 
			0x03, 0x72, 0x99, 0xf4
		};  // Note : although this is a randomly derived array changing it's contents may break compatibility with decrypting existing wallets.

		*/
	 	public bool RememberPassword {
			get;
			set;
		}

		//public byte[] encrypt (byte[] message, String password) 
		public byte[] Encrypt ( RippleSeedAddress seed, byte[] salt )
		{

			// Note: we aren't using the seed to encrypt, we are using the password to encrypt the seed and the ripple address as salt
			/*
			if (ra == null) {
				Logging.writeLog ("Warning : Rijndaelio encrypting requires salt ");
				throw new NotSupportedException ();
			}
			*/

			string password = GetPasswordCreateInput ();

			// TODO should I OR the byte values? with an array of known hardcoded bytes?
			// should I OR it with a random salt and save that with the encrypted file?
			// scramble them?

			try {

				using (Rijndael myRijndael = Rijndael.Create())
				{
					

					using ( Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes (password, salt, 123456) ) 
					{
							myRijndael.Key = bytes.GetBytes(32);
							myRijndael.IV = bytes.GetBytes(16);

							using (MemoryStream memorystream = new MemoryStream ()) 
							{
						
								using (CryptoStream cryptostream = new CryptoStream (memorystream, myRijndael.CreateEncryptor(), CryptoStreamMode.Write))
								{

							

								//byte[] encode = Encoding.ASCII.GetBytes(seed.ToString());
								byte[] encode = seed.GetBytes();
									cryptostream.Write(encode,0,encode.Length);
									cryptostream.Close();
									memorystream.Flush ();
									return memorystream.ToArray();
								}
							}


					}
				}
				

			}


			catch (Exception e) 
			{
				// TODO print to screen debug ect this.mainWindow.
				string m = "Encryption Error : " 
					#if DEBUG
					+ DebugRippleLibSharp.exceptionMessage
					#endif
					;

			
					Logging.WriteLog (m + e.Message + "/n");

				return null;
			}


		}

		public byte[] Decrypt ( byte [] cipher, byte[] salt, RippleAddress ra) {



			string password = Password;


			if (password == null) {
				// TODO
			}

			try {



				using (Rijndael rijndael = Rijndael.Create())
				{
					using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes (password, salt, 123456))
					{
						rijndael.Key = bytes.GetBytes(32); // if the stupid editor won't let you type this without autocompleting into something else just write it as a comment then uncomment it
						rijndael.IV = bytes.GetBytes(16);

						using (MemoryStream memorystream = new MemoryStream()) 
						{
							using (CryptoStream cryptostream = new CryptoStream (memorystream,rijndael.CreateDecryptor(),CryptoStreamMode.Write))
							{
								cryptostream.Write(cipher, 0, cipher.Length);
								cryptostream.Close();

								byte[] array = memorystream.ToArray();
								//string value = ASCIIEncoding.ASCII.GetString(array);
								//return value;

								return array;
							}
						}

					}
				}
			}

			catch (Exception e) {
				// TODO print/debug
				string m = "Decryption Error : " 
					#if DEBUG
					+ DebugRippleLibSharp.exceptionMessage
					#endif
					;
				Logging.WriteLog (m + e.Message + "\n");
				return null;
			}
		}

		public string Name {
			get {
				return EncryptionType.Rijndaelio.ToString ("G");

			}
			/*
			set {

			}
			*/

		}

		public string Password { get; set; }

		public static Rijndaelio GetPasswordInput () {

			ManualResetEventSlim mre = new ManualResetEventSlim ();
			mre.Reset ();
			Rijndaelio rijndaelio = null;
			Application.Invoke ( (object sender, EventArgs e) => {

				rijndaelio = Rijndaelio.DoRijndaelioDialog ();
				mre.Set();
			});
			mre.Wait ();
			return rijndaelio;
		}

		public string GetPasswordCreateInput () {
			ManualResetEventSlim mre = new ManualResetEventSlim ();
			mre.Reset ();
			string pass = null;
			Application.Invoke ( (object sender, EventArgs e) => {
				pass = PasswordCreateDialog.DoDialog ("Enter a new password");
				mre.Set();
			});
			mre.Wait ();
			return pass;
		}

		public static Rijndaelio DoRijndaelioDialog ()
		{

			using (PasswordDialog pd = new PasswordDialog ("Please enter your password")) {

				ResponseType rt = (ResponseType)pd.Run ();
				pd.Hide ();

				if (rt != ResponseType.Ok) {
					return null;
				}


				string pass = pd.GetPassword ();

				Rijndaelio rijndaelio = new Rijndaelio {
					Password = pass,
					RememberPassword = pd.RememberPassword

				};

				pd.Destroy ();

				return rijndaelio;
			}
		}

	} // class
}  // namespace

