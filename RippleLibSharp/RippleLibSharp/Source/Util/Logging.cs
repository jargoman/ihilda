/*
 *	License : Le Ice Sense 
 */


using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using RippleLibSharp.Network;

namespace RippleLibSharp.Util
{
	public static class Logging
	{
		
		/*
		public Logging ()
		{
		}
		*/

		public static bool WRITE_TO_STDOUT = true;

		public static void WriteLog (String message) 
		{

			if (message == null) {
				return;
			}

			if (WRITE_TO_STDOUT) {
				try {
					System.Console.WriteLine (message); // namespace is needed because I named a class Console like an idiot
				} catch (Exception e) {

				}
			}

			// TODO write to log



		}

		public static void ReportException (string method_sig, Exception e) {
			Exception ex = e;

			while (ex != null) {
				Logging.WriteLog( 
					method_sig + 
					"/n" +
					e.Message + "\n");
				Logging.WriteLog(e.StackTrace);

				ex = ex.InnerException;
			}
		}


	



		public static void WriteLog (String message, IEnumerable<object> objects)
		{
			Logging.WriteLog (message);

			if (objects == null) {
				
				Logging.WriteLog ("null");
				return;
			}

			foreach (object obj in objects) {
				WriteLog( obj.ToString () );
			}


		}

		public static void WriteLog (String message, IEnumerable<byte> bytes)
		{

			WriteLog (message);

			if (bytes != null) {
				
				foreach (byte b in bytes) {
					WriteLog ( b.ToString() );
				}
			}


		}



		/*
		public static void writeLog (String message, IEnumerable<RippleWallet> wallets)
		{
			String[] ar = null;
			if (wallets != null) {
				ar = new string[wallets.Count()];
				int x = 0;
				foreach (RippleWallet o in wallets) {
					ar[x++] = o.ToString();
				}
			}

			writeLog(message, ar);
		}
		*/

		/*
		public static void writeLog (String message, IEnumerable<ConnectionInfo>  conny ) {
			
			String[] ar = new string[] {  
				"ServerUrl = " + Debug.toAssertString(conny.ServerUrls) +
				"\nLocalUrl = " + Debug.toAssertString(conny.LocalUrl) +
				"\nUserAgent = " + Debug.toAssertString(conny.UserAgent)
			};

			writeLog (message, ar);
		}*/

	
		public static void WriteLog (String message, IEnumerable<String> strings)
		{


			if (strings == null && message == null) {
				return;
			}

			if (message == null) {
				message = "";
			}


			StringBuilder sb = new StringBuilder(message);

			if (strings != null) {

				sb.Append("{ ");
				int c = strings.Count();

				if (c > 0) {

				
					int x = 1; 
					foreach (object s in strings) {

						sb.Append( (string) (s ?? "null") );

						if (x++ != c) {
							sb.Append(", ");
						}

						else {
							sb.Append(" ");
						}
					}

				}

				else {
					sb.Append("EMPTY");
				}

				sb.Append("}");
			}

			Logging.WriteLog( sb.ToString() );
			sb.Clear();
		}

		public static void WriteLog (IEnumerable<object> strings)
		{
			WriteLog(null, strings);
		}

		public static void WriteLog <T>(String message, IEnumerable<T> numerable ) {
			
			if (numerable == null)
				return;

			WriteLog (message);

			foreach ( T t in numerable ) {
				WriteLog( t.ToString () );
			}
		}


	}
}

