using System;
using System.Diagnostics;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public static class RippledController
	{
		/*
		public RippledController ()
		{

		}
		*/

		public static string RippledEval ( String arguments ) {
			#if DEBUG
			string method_sig = clsstr + nameof (RippledEval) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.RippledController) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
			#endif

			string retMe = null;
			try {


				Process p = new Process();



				p.StartInfo.FileName = "ping";
				p.StartInfo.CreateNoWindow = true;
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.RedirectStandardInput = true;
				p.StartInfo.RedirectStandardError = true;

				p.StartInfo.UseShellExecute = false;
				//p.StartInfo.

				p.EnableRaisingEvents = true;

				p.StartInfo.Arguments = arguments;

				p.OutputDataReceived += (object sender, DataReceivedEventArgs e) => {
					Logging.WriteLog(e.Data);
					retMe = e.Data;
				};



				p.Start();
				p.BeginOutputReadLine();

				p.WaitForExit();

			}

			#pragma warning disable 0168
			catch ( Exception ex ) {
			#pragma warning restore 0168

				#if DEBUG
				if (DebugIhildaWallet.RippledController) {
					Logging.ReportException (method_sig, ex);
				}

				#endif
			}

			return retMe;
		}

		#if DEBUG
		private static readonly string clsstr = nameof (RippledController) + DebugRippleLibSharp.colon;
		#endif
	}
}

