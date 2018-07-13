/*
 *	License : Le Ice Sense 
 */

using Gtk;
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public static class Logging
	{
		

		public static bool WRITE_TO_STDOUT = true;

		public static void WriteLog (String message) 
		{

			if (message == null) {
				return;
			}

			if (WRITE_TO_STDOUT) {
				System.Console.WriteLine(message); // namespace is needed because I named a class Console like an idiot
			}

			// TODO write to log



		}

		public static void WriteConsole (String message)
		{
			if ( message == null || textview ==null ) {
				return;
			}


			Gtk.Application.Invoke ( delegate 
			{
				TextBuffer buff = textview.Buffer;
				buff.Text += message;
						
				TextIter it = buff.EndIter;
				textview.ScrollToIter(it,0,false,0,0);
				ClearAllBut(Console.max_screen_lines);
			}
			);

		}

		public static void WriteBoth (string message)
		{
			WriteConsole(message);
			WriteLog(message);

		}

		public static void ClearAllBut ( int lines )
		{
			int lin = lines;
			Gtk.Application.Invoke ( delegate {
			

				TextBuffer buff = textview.Buffer;

				if (buff.LineCount > lin) {
					var b = buff.GetIterAtLine(lin);

				var c = buff.StartIter;

					buff.Delete(ref b, ref c);

				}


					
				}


			);

		}
		

		public static void WriteLog (String message, IEnumerable<object> objects)
		{
			String[] ar = null;
			if (objects != null) {
				ar = new string[objects.Count()];
				int x = 0;
				foreach (object o in objects) {
					ar[x++] = o.ToString();
				}
			}

			WriteLog(message, ar);
		}

		public static void WriteLog (String message, IEnumerable<byte> bytes)
		{
			String[] ar = null;
			if (bytes != null) {
				ar = new string[bytes.Count()];
				int x = 0;
				foreach (byte b in bytes) {
					ar[x++] = b.ToString();
				}
			}

			WriteLog(message, ar);
		}

		public static void WriteLog (String message, IEnumerable<RippleWallet> wallets)
		{
			String[] ar = null;
			if (wallets != null) {
				ar = new string[wallets.Count()];
				int x = 0;
				foreach (RippleWallet o in wallets) {
					ar[x++] = o.ToString();
				}
			}

			WriteLog(message, ar);
		}
	
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

		public static void WriteLog <T>( 
		                                String message, 
		                                IEnumerable<T> numerable 
		                               ) {

			Logging.WriteLog ( message );

			if (numerable == null)
				return;
			

			foreach (T t in numerable) {
				Logging.WriteLog(t.ToString ());
			}
		}

		public static void ReportException (string method_sig, Exception e) {
			Exception ex = e;

			while (ex != null) {
				Logging.WriteLog( 
					method_sig + 
					#if DEBUG
				                 DebugRippleLibSharp.exceptionMessage + 
					#endif
					e.Message + "\n");
				Logging.WriteLog(e.StackTrace);

				ex = ex.InnerException;
			}
		}

		public static Gtk.TextView textview;
	}
}

