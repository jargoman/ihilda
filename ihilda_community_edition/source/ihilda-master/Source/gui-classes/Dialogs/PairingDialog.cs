using System;
using System.Security.Cryptography;
using System.Text;
using Codeplex.Data;
using Gtk;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public partial class PairingDialog : Gtk.Dialog
	{
		public PairingDialog ()
		{
			this.Build ();


		}


		public RsaKeyRequest GetRsaKeyRequest ()
		{

			RsaKeyRequest rsaKeyRequest = new RsaKeyRequest {
				userId = entry1.Text,
				initialToken = entry2.Text,
				onetimepassword = entry3.Text
			};

			return rsaKeyRequest;
		}

		public static RsaKeyRequest DoPairingDialog ()
		{
#if DEBUG
			string method_sig = clsstr + DebugRippleLibSharp.colon + nameof(DoPairing);
#endif
			int respType = (int)ResponseType.None;
			RsaKeyRequest keyRequest = null;




				try {
					using (PairingDialog dialog = new PairingDialog ()) {
						respType = dialog.Run ();

						if (respType == (int)ResponseType.Ok) {
							keyRequest = dialog.GetRsaKeyRequest ();
						}



						dialog.Destroy ();
					}
				} catch (Exception e) {

#if DEBUG
				Logging.ReportException (method_sig, e);
#endif
				} finally {
					
				}



			return keyRequest;
			//DoPairing (keyRequest);
		}

		public static ApiResult DoPairing (RsaKeyRequest keyRequest)
		{
			ApiResult returnMe = new ApiResult ();

			rsaEncryption encryption = new rsaEncryption ();

			RSACryptoServiceProvider rsa = rsaEncryption.AssureRsaKeys ();


			//string clientprivateKey = rsa.ToXmlString (true); // true to get the private key  

									  // Get the public keyy   
			string clientpublicKey = rsa.ToXmlString (false); // false to get the public key   


			string serverResp = GetServerPublicKey (keyRequest.userId, keyRequest.onetimepassword, clientpublicKey);

			ApiResult servRes = DynamicJson.Parse (serverResp);

			if (servRes.status == "error") {
				return servRes;
			}

			//Logging.WriteBoth (clientpublicKey);
			//Logging.WriteBoth (clientprivateKey);

			string URL = ProgramVariables.webUrl + "/api/pair";

			//keyRequest.clientRsaPublicKey = clientpublicKey;



			//var d = new { Command = "helloworld", Signature =  bytes};

			var keyreq = new {
				keyRequest.initialToken
			};
			
			string json = DynamicJson.Serialize (keyreq);

			byte [] byteArray = Encoding.UTF8.GetBytes (json);


			byte[] encrypted = encryption.EncryptBytes (servRes.result, byteArray);

			var payload = new {
				bytes = encrypted,
				userId = keyRequest.userId,
				clientRsaPublicKey = clientpublicKey
			};

			string blob = DynamicJson.Serialize (payload);

			// TODO no credentials to console
			//Logging.WriteBoth (json);

			string resp = Winter.DoPost (URL, blob);

			ApiResult apiResult = DynamicJson.Parse (resp);

			rsaEncryption.SaveAesKey (keyRequest.userId,apiResult.result);

			return apiResult;
		}

		private static string GetServerPublicKey (string id, string pass, string clientPublic)
		{
			string URL = ProgramVariables.webUrl + "/api/pair/" + id;
			var p = new { password = pass, clientPublic};

			string body = DynamicJson.Serialize (p);

			string res = Winter.DoPost (URL, body);

			Logging.WriteBoth ("res");

			return res;
		}

		public static readonly string clsstr = nameof(PairingDialog); 
	}
}
