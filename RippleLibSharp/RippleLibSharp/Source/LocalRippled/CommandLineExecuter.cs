
using System;

namespace RippleLibSharp.LocalRippled
{
	public class CommandLineExecuter
	{
		/*
		public CommandLineExecuter ()
		{
		}
		*/

		public string GetResult ( string command ) {



			// commant line access for rippled


			System.Diagnostics.Process proc = new System.Diagnostics.Process(); 

			proc.StartInfo.FileName = rcommand;
			proc.StartInfo.Arguments = command; 
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.RedirectStandardOutput = true; 
			proc.Start();

			proc.WaitForExit ();



			string output = proc.StandardOutput.ReadToEnd();


			return output;

		}

		/*
		public string submitJSON (string ) {

		}
		*/

		public string SignJSON (string txjson, string seed) {



			string command = "sign " + seed + " " + txjson + " offline";
			//string command = "--help";
			return GetResult (command);
		}

		/*
		public string SignJSONPrivateKey (string txjson, string seed)
		{



			string command = "sign " + seed + " " + txjson + " offline";
			//string command = "--help";
			return GetResult (command);
		}
*/


		public string WalletPropose (string seed) {
			string command = "wallet_propose " + seed;
			return GetResult (command);
		}

		string rcommand = "/opt/ripple/bin/rippled";
	}
}

