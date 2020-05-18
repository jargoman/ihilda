using System;
using System.Net;
using System.IO;
using Codeplex.Data;
using RippleLibSharp.Result;
using RippleLibSharp.Util;

//using System.Security.Cryptography.X509Certificates;
//using System.Net.Security;
using System.Threading;

namespace RippleLibSharp.Network
{
	public static class DataApi
	{
		
	

		public static T GetResponseObject <T> ( string request, CancellationToken token) {
			#if DEBUG
			string typeName = typeof (T)?.ToString () ?? DebugRippleLibSharp.null_str;
			string method_sig = nameof(GetResponseObject) + " <" + typeName + ">" + DebugRippleLibSharp.left_parentheses + (request ?? DebugRippleLibSharp.null_str) + DebugRippleLibSharp.right_parentheses;
			if (DebugRippleLibSharp.DataAPI) {
				Logging.WriteLog(typeName);
			}
			#endif


			string jsonResponse = Get (request, token);

			T d = default(T);
			try {
				d = DynamicJson.Parse( jsonResponse );

				if (!typeof(T).IsValueType ||
#pragma warning disable RECS0017 // Possible compare of value type with 'null'
					d == null) {
#pragma warning restore RECS0017 // Possible compare of value type with 'null'

				}

			}

			catch (Exception e) {
				#if DEBUG
				if (DebugRippleLibSharp.DataAPI) {
					Logging.ReportException(method_sig, e);
				}
				#endif
			}

			return d;

		}


		/*
		static public bool MyRemoteCertificateValidationCallback(System.Object sender,
			X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			bool isOk = true;
			// If there are errors in the certificate chain,
			// look at each error to determine the cause.
			if (sslPolicyErrors != SslPolicyErrors.None) {
				for (int i=0; i<chain.ChainStatus.Length; i++) {
					if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown) {
						continue;
					}
					chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
					chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
					chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan (0, 1, 0);
					chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
					bool chainIsValid = chain.Build ((X509Certificate2)certificate);
					if (!chainIsValid) {
						isOk = false;
						break;
					}
				}
			}
			return isOk;
		}
		*/

		public static string Get(string uri, CancellationToken token)
		{
			int maxattempts = 3;
			int attempt = 0;
			while (attempt++ != maxattempts) {
				DoThrottlingWait (token);

				//System.Security.Cryptography.AesCryptoServiceProvider b = new System.Security.Cryptography.AesCryptoServiceProvider(); 
				try {
					HttpWebRequest request = (HttpWebRequest)WebRequest.Create (uri);

					//request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
					//ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
					//ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
					//ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
					//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;//Tls12;

					using (HttpWebResponse response = (HttpWebResponse)request.GetResponse ())
					using (Stream stream = response.GetResponseStream ())
					using (StreamReader reader = new StreamReader (stream)) {
						return reader.ReadToEnd ();
					}
		    		// TODO better thread api support
				} catch (System.Net.WebException we) {
					// TODO certmanager
					Thread.Sleep (1000 * 60);
					//throw we;

				} catch (Exception e) {
					Thread.Sleep (1000 * 60);
					//throw e;
				}
			}
			throw new WebException("Could not retrieve data api info");
		}

		private static DateTime last_call_time = default (DateTime);
		private static void DoThrottlingWait (CancellationToken token)
		{
			if (last_call_time == default (DateTime)) {
				last_call_time = DateTime.Now;
			} else {

				while (((DateTime.Now - last_call_time).TotalMilliseconds) < 2500 && !token.IsCancellationRequested) {
					//Thread.Sleep (125);
					token.WaitHandle.WaitOne (125);
				}
			}



			last_call_time = DateTime.Now;

		}


		/*
		public static bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}
		*/

	}
}

