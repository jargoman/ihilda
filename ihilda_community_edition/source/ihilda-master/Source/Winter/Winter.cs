using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Security;
using Codeplex.Data;
using RippleLibSharp.Network;
using System.Security.Cryptography;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public class Winter
	{
		public Winter ()
		{
		}


		public bool AuthenticateUser ()
		{

#if DEBUG
			string method_sig = clsstr + DebugRippleLibSharp.colon + nameof (AuthenticateUser);
#endif

			UserKey userKey = null;

			try {

				userKey = rsaEncryption.GetAesKey ();

			} catch (FileNotFoundException e) {

#if DEBUG
				Logging.ReportException (method_sig, e);
#endif

				return false;
			} catch (Exception e) {

#if DEBUG
				Logging.ReportException (method_sig, e);
#endif
				return false;
			}

			if (userKey == null) {
				return false;
			}



			try {
				HttpWebRequest request = 
				WebRequest.Create (ProgramVariables.webUrl + "/Account/LoginRemote") as HttpWebRequest;

				request.Method = "POST";
				//request.ContentType = "application/x-www-form-urlencoded";
				request.ContentType = "application/json; charset=UTF-8";

				request.CookieContainer = new CookieContainer ();

				//var authCredentials = "UserName=" + userKey.user + "&Password=" + userKey.key;

				var auth = new UserKey { user = userKey.user, key = userKey.key };
				var authCredentials = DynamicJson.Serialize (auth);

				byte [] bytes = System.Text.Encoding.UTF8.GetBytes (authCredentials);
				request.ContentLength = bytes.Length;
				using (var requestStream = request.GetRequestStream ()) {
					requestStream.Write (bytes, 0, bytes.Length);
				}
				var resp = request.GetResponse ();

				if (resp == null) {
					return false;
				}
				using (var response = resp as HttpWebResponse) {

					var cookies = response.Cookies;
					string cookieName = ".AspNet.ApplicationCookie";
					authCookie = response.Cookies [cookieName];
				}

				
			} catch (Exception e) {
#if DEBUG
				Logging.ReportException (method_sig, e);
#endif
			}

			if (authCookie != null) {
				return true;
			} else {
				return false;
			}

		}

		public Cookie authCookie { get; set; }

		public string Error { get; set; }


		public static void GetRequest (CancellationToken token)
		{
			string URL = "http://192.168.1.195:45455/Account/LoginAPI";

			string str = DataApi.Get (URL, token);

			// TODO make sure you stop printing access tokens to the console. Debug only
			Logging.WriteBoth (str);


		}


		public static string DoPost (string url, string body)
		{


			WebRequest webRequest = WebRequest.Create (url);
			webRequest.Method = "POST";

			webRequest.ContentType = "application/json; charset=UTF-8";
			

			byte [] byteArray = Encoding.UTF8.GetBytes (body);

			webRequest.ContentLength = byteArray.Length;

			Stream dataStream = webRequest.GetRequestStream ();

			dataStream.Write (byteArray, 0, byteArray.Length);

			dataStream.Close ();

			WebResponse response = webRequest.GetResponse ();
			HttpWebResponse httpWeb = (HttpWebResponse)response;

			Logging.WriteBoth (httpWeb.StatusDescription);

			string responseFromServer;
			using (dataStream = response.GetResponseStream()) {
				// Open the stream using a StreamReader for easy access.  
				StreamReader reader = new StreamReader (dataStream);
				// Read the content.  
				responseFromServer = reader.ReadToEnd ();
				// Display the content.  
				Logging.WriteBoth (responseFromServer);
			}

			return responseFromServer;
		}


#if DEBUG
		private const string clsstr = nameof (Winter) + DebugRippleLibSharp.colon;
#endif

	}
}
