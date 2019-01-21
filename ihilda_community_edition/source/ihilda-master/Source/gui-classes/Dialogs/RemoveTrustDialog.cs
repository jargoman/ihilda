using System;
using System.Threading;
using Gtk;

using System.Threading.Tasks;

using RippleLibSharp.Result;

using RippleLibSharp.Transactions;
using RippleLibSharp.Transactions.TxTypes;

using RippleLibSharp.Network;
using IhildaWallet.Networking;

using RippleLibSharp.Keys;
using System.Text;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public class RemoveTrustDialog : AreYouSure
	{
		public RemoveTrustDialog (string issuer, string cur)
			: base (TITLE, message1 + cur + "." + issuer)
		{

		}


		public static void DoDialog (string issuer, string cur)
		{


			using (RemoveTrustDialog rtd = new RemoveTrustDialog (issuer, cur)) {
				int rt = rtd.Run ();

				rtd.Destroy ();

				if (rt != (int)ResponseType.Ok)
					return;



			}
		}


		public static void RemoveTrust (RippleWallet rippleWallet, string issuer, string cur, CancellationToken token)
		{

#if DEBUG
			StringBuilder stringbuilder = new StringBuilder ();
			stringbuilder.Append (clsstr);
			stringbuilder.Append (nameof (RemoveTrust));
			stringbuilder.Append (DebugRippleLibSharp.left_parentheses);
			stringbuilder.Append (cur.GetType ().ToString ());
			stringbuilder.Append (DebugRippleLibSharp.space_char);
			stringbuilder.Append (nameof (issuer));
			stringbuilder.Append (DebugRippleLibSharp.equals);
			stringbuilder.Append ((issuer ?? "null"));
			stringbuilder.Append (DebugRippleLibSharp.comma);
			stringbuilder.Append (cur.GetType ().ToString ());
			stringbuilder.Append (DebugRippleLibSharp.space_char);
			stringbuilder.Append (nameof (cur));
			stringbuilder.Append (DebugRippleLibSharp.equals);
			stringbuilder.Append ((cur ?? "null"));
			stringbuilder.Append (DebugRippleLibSharp.right_parentheses);
			string method_sig =  stringbuilder.ToString();
#endif

			RippleWallet rw = rippleWallet;
			if (rw == null) {
#if DEBUG
				if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
					Logging.WriteLog (method_sig + "w == null, returning\n");
				}
#endif
			}

			RippleIdentifier rsa = rw.GetDecryptedSeed ();
			while (rsa.GetHumanReadableIdentifier () == null) {
				bool should = AreYouSure.AskQuestion (
				"Invalid password",
				"Unable to decrypt seed. Invalid password.\nWould you like to try again?"
				);

				if (!should) {
					return;
				}

				rsa = rw.GetDecryptedSeed ();
			}


			NetworkInterface ni = NetworkController.CurrentInterface;

			if (ni == null) {
				// TODO alert user
			}

			RippleCurrency limitAmount = new RippleLibSharp.Transactions.RippleCurrency (
											 0m,
											 issuer,
											 cur);

			RippleTrustSetTransaction rts = new RippleTrustSetTransaction (
				issuer,
				limitAmount,
				0,
				0);

			SignOptions opts = SignOptions.LoadSignOptions ();
			FeeSettings feeSettings = FeeSettings.LoadSettings ();

			ParsedFeeAndLedgerResp tupe = feeSettings.GetFeeAndLastLedgerFromSettings (ni, token);
			if (tupe == null) {
				//TODO
				return;
			}

			if (tupe.HasError) {
				// TODO
				return;
			}
			uint se = Convert.ToUInt32 (RippleLibSharp.Commands.Accounts.AccountInfo.GetSequence (rw.GetStoredReceiveAddress (), ni, token));


			UInt32 f = (UInt32)tupe.Fee;
			rts.fee = f.ToString ();

			rts.Sequence = se;

			uint lls = 0;
			if (opts != null) {
				lls = opts.LastLedgerOffset;
			}

			if (lls < 5) {
				lls = SignOptions.DEFAUL_LAST_LEDGER_SEQ;
			}


			rts.LastLedgerSequence = (UInt32)tupe.LastLedger + lls;

			if (rts.fee.amount == 0 || rts.Sequence == 0) {
				//
				throw new Exception ();
			}


			if (opts == null) {
				// TODO get user to choose and save choice
			}

			switch (opts.SigningLibrary) {
			case "Rippled":
				rts.SignLocalRippled (rsa);
				break;
			case "RippleLibSharp":
				rts.Sign (rsa);
				break;
			case "RippleDotNet":
				rts.SignRippleDotNet (rsa);
				break;
			default:
				throw new NotSupportedException ("Invalid sign option " + opts.SigningLibrary);
			}

			Task<Response<RippleSubmitTxResult>> task = null;

			try {
				task = NetworkController.UiTxNetworkSubmit (rts, ni, token);
				Logging.WriteLog ("Submitted via websocket");
				task.Wait (token);


			} catch (Exception e) {

				Logging.WriteLog (e.Message);
				Logging.WriteLog ("Network Error");
				return;
			} finally {

#if DEBUG
				Logging.WriteLog (method_sig + "sleep");
#endif
				//may or may not keep a slight delay here for orders to process
				Thread.Sleep (10);
			}

			if (token.IsCancellationRequested) {
				return;
			}

			var r = task.Result;

			if (r == null || r.status == null || !r.status.Equals ("success")) {

#if DEBUG
				if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
					Logging.WriteLog ("Error submitting remove trust transaction ");

				}
#endif


				return;

			}

			RippleSubmitTxResult res = r.result;

			if (res == null) {


				return;
			}

#if DEBUG
			Logging.WriteLog (method_sig + "engine_result = " + (res.engine_result ?? null));
#endif


			//tefPAST_SEQ

			/*

			switch ( res.engine_result ) {

			case null:
				this.setIsSubmitted ( index.ToString(), "null");
				return false;

			case "terQUEUED":
				Thread.Sleep(1000);
				this.setIsSubmitted (index.ToString (), res.engine_result);
				return true;

			case "tesSUCCESS":
				this.setIsSubmitted (index.ToString (), res.engine_result);
				return true;

			case  "terPRE_SEQ":
			case "tefPAST_SEQ":
			case "tefMAX_LEDGER":
				this.setFailed (index.ToString (), res.engine_result);
				return false;

			case "telCAN_NOT_QUEUE":
				this.setFailed (index.ToString (), res.engine_result + " retrying");
				goto retry;

			case "telINSUF_FEE_P":
				this.setFailed ( index.ToString (), res.engine_result );
				return false;

			case "tecNO_ISSUER":
				this.setFailed (index.ToString (), res.engine_result);
				return false;

			case "tecUNFUNDED_OFFER":
				this.setFailed (index.ToString(), res.engine_result);
				return false;

			default:
				this.setIsSubmitted (index.ToString (), "Response not imlemented");
				return false;

			}

			*/


		}



		private const string message1 = "Are you sure you want to remove trust for ";
		//private const string message2 = "";
		private const string TITLE = "Remove Trust";

#if DEBUG
		private const string clsstr = nameof (RemoveTrustDialog) + DebugRippleLibSharp.colon;
#endif
	}
	
}

