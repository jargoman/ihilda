using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Codeplex.Data;

namespace IhildaWallet
{
	public class rsaEncryption
	{
		public rsaEncryption ()
		{
		}

		public byte[] EncryptBytes (string publicKey, byte[] data)
		{
			// Convert the text to an array of bytes   
			//UnicodeEncoding byteConverter = new UnicodeEncoding ();
			//byte [] dataToEncrypt = byteConverter.GetBytes (text);

			// Create a byte array to store the encrypted data in it   

			try {
				byte [] encryptedData;
				using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider ()) {
					// Set the rsa pulic key   
					rsa.FromXmlString (publicKey);

					// Encrypt the data and store it in the encyptedData Array   
					encryptedData = rsa.Encrypt (data, false);
				}
				// Save the encypted data array into a file   

				//Console.WriteLine ("Data has been encrypted");

				return encryptedData;
			} catch (Exception e) {
				return null;
			}

		}

		public byte[] DecryptData (string privateKey, byte[] data)
		{
			// read the encrypted bytes from the file   
			

			// Create an array to store the decrypted data in it   
			byte [] decryptedData;

			try {
				using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider ()) {
					// Set the private key of the algorithm   
					rsa.FromXmlString (privateKey);
					decryptedData = rsa.Decrypt (data, false);
				}

				return decryptedData;

			} catch (Exception e) {
				return null;
			}

			// Get the string value from the decryptedData byte array   
			

		}


		public static RSACryptoServiceProvider AssureRsaKeys ()
		{
			CspParameters cryptoParams = new CspParameters {
				KeyContainerName = "winter",
				
			};

			RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider (2048, cryptoParams);

			return cryptoServiceProvider;

		}

		public static void SaveAesKey (string user, string key)
		{
			string keypath = Path.Combine (FileHelper.WINTER_FOLDER_PATH, AESKEYPATH);

			UserKey o = new UserKey {
				user = user,
				key = key
			}; 


			var json = DynamicJson.Serialize (o);

			File.WriteAllText (keypath, json);


		}

		public static UserKey GetAesKey ()
		{
			string keypath = Path.Combine (FileHelper.WINTER_FOLDER_PATH, AESKEYPATH);

			if (!File.Exists(keypath)) {
				throw new FileNotFoundException ("Could not locate aes key", keypath);
			}

			string json = File.ReadAllText (keypath, Encoding.UTF8);
			UserKey userKey = DynamicJson.Parse (json);


			byte [] blob = Convert.FromBase64String (userKey.key);

			RSACryptoServiceProvider cryptoServiceProvider = AssureRsaKeys ();
			byte [] decrypted = cryptoServiceProvider.Decrypt (blob, false);

			string unencrypted = Convert.ToBase64String (decrypted);

			userKey.key = unencrypted;



			return userKey;

		}

		public static string AESKEYPATH = "aes.encrypted.key";
	}

	public class UserKey
	{
		public string key { get; set; }
		public string user { get; set; }
	}
}
