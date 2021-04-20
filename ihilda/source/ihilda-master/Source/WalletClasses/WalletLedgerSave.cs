using System;
using System.IO;
using System.Text;
using Codeplex.Data;
using RippleLibSharp.Util;

namespace IhildaWallet
{


	/* 
	 * The wallets last ledger is saved to its own file to avoid file corruption affecting other data
    	*/ 
	public class WalletLedgerSave
	{
		public UInt32? Ledger {
			get;
			set;
		}


		public static WalletLedgerSave LoadLedger (string path)
		{
#if DEBUG
			string method_sig = nameof (WalletLedgerSave) + DebugRippleLibSharp.colon + nameof (LoadLedger);

			if (DebugIhildaWallet.WalletManager) {
				Logging.WriteLog (method_sig + path + DebugRippleLibSharp.beginn);
			}

#endif

			try {

				Logging.WriteLog ("Looking for wallet at " + path + "\n");

				if (File.Exists (path)) { // 
					Logging.WriteLog ("Wallet " + path + " Exists!\n");

					string wah = File.ReadAllText (path, Encoding.UTF8);

					WalletLedgerSave ledge = DynamicJson.Parse (wah);
					return ledge;
				}


			} catch (Exception e) {
				Logging.WriteLog (e.Message);
			}

			return null;


		}
	}
}
