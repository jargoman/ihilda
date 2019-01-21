using System;

using RippleLibSharp.Transactions;
using RippleLibSharp.Transactions.TxTypes;

using RippleLibSharp.Network;
using IhildaWallet.Networking;

using RippleLibSharp.Keys;
using System.Threading;
using System.Threading.Tasks;
using RippleLibSharp.Result;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CancelTxWidget : Gtk.Bin
	{
		public CancelTxWidget ()
		{
			this.Build ();

			this.button55.Clicked += (object sender, EventArgs e) => {


				//tokenSource?.Cancel ();
				tokenSource = new CancellationTokenSource ();
				CancellationToken token = tokenSource.Token;

				label3.Text = "";

				// TODO lookup tx, verify matches signing account, are you sure?? ect



				string tx_hash = this.comboboxentry1?.Entry?.Text;
				if (string.IsNullOrWhiteSpace (tx_hash)) {
					WriteToInfoBox ("Please specify a transaction hash\n");
					return;
				}

				
				RippleCancelTransaction tx = new RippleCancelTransaction ();

				RippleWallet rw = _rippleWallet;
				if (rw == null) {


					WriteToInfoBox ("Please specify a wallet using the wallet manager\n");

					return;
				}


				string acc = rw.GetStoredReceiveAddress ();

				NetworkInterface ni = NetworkController.GetNetworkInterfaceGuiThread ();
				if (ni == null) {
					string messg = "Unable to connect to network";
					MessageDialog.ShowMessage ("Network Error", messg);
					WriteToInfoBox (messg + "\n");
					return;
				}


				bool isSeq = UInt32.TryParse (tx_hash, out uint offerseq);
				if (!isSeq) {

					//var tx_task = RippleLibSharp.Commands.Tx.tx.GetRequestDataApi (tx_hash, token);
					var tx_task = RippleLibSharp.Commands.Tx.tx.GetRequest (tx_hash, ni, token);
					if (tx_task == null) {
						return;
					}

					tx_task.Wait ();



					if (tx_task == null) {
						return;
					}

					var response = tx_task.Result;

					if (response == null) {
						return;
					}

					if (response.HasError()) {
						return;
					}

					var res = response.result;

					if (res == null) {
						return;
					}

					offerseq = res.Sequence;

				}


				Task.Run (delegate {

					//WriteToInfoBox ("");

					uint se = Convert.ToUInt32 (RippleLibSharp.Commands.Accounts.AccountInfo.GetSequence (acc, ni, token));
					if (se == 0) {
						WriteToInfoBox ("Unable to determine sequence number for account " + acc + "\n");
						return;
					}

					FeeSettings feeSettings = FeeSettings.LoadSettings ();
					if (feeSettings == null) {
						// TODO
						WriteToInfoBox ("Failed to load fee settings\n");
						return;
					}

					ParsedFeeAndLedgerResp tupe = feeSettings.GetFeeAndLastLedgerFromSettings (ni, token);
					if (tupe == null) {
						WriteToInfoBox ("Failed to retrieve fee and last ledger");
						return;
					}

					if (tupe.HasError) {
						WriteToInfoBox (tupe.ErrorMessage);
						return;
					}


					SignOptions opts = SignOptions.LoadSignOptions ();

					tx.fee = ((UInt32)tupe.Fee * 2).ToString ();

					tx.Sequence = se; // 
					tx.LastLedgerSequence = (UInt32)tupe.LastLedger + (opts?.LastLedgerOffset ?? SignOptions.DEFAUL_LAST_LEDGER_SEQ);

					tx.Account = rw.GetStoredReceiveAddress ();



					tx.OfferSequence = offerseq;



					if (opts == null) {
						WriteToInfoBox ("Failed to load sign options\n");
						return;
					}

					RippleIdentifier seed = rw.GetDecryptedSeed ();



					while (seed?.GetHumanReadableIdentifier () == null) {
						bool should = AreYouSure.AskQuestion (
						"Invalid password",
						"Unable to decrypt seed. Invalid password.\nWould you like to try again?"
						);

						if (!should) {
							return;
						}

						seed = rw.GetDecryptedSeed ();
					}

					switch (opts.SigningLibrary) {
					case "RippleDotNet":
						WriteToInfoBox ("RippleDotNet\n");

						var signature = tx.SignRippleDotNet (seed);
						if (signature == null) {
							WriteToInfoBox ("Error signing with RippleDotNet.");
						}

						break;
					case "Rippled":
						WriteToInfoBox ("Signing with rippled\n");

						var signature2 = tx.SignLocalRippled (seed);
						if (signature2 == null) {
							WriteToInfoBox ("Error signing with rippled. Is rippled running?");
							return;
						}

						break;
					case "RippleLibSharp":
						WriteToInfoBox ("Signing with ripple-lib-sharp\n");

						var signature3 = tx.Sign (seed);
						if (signature3 == null) {
							WriteToInfoBox ("Error signing with ripple-lib-sharp.");
						}

						break;
					default:
						throw new NotSupportedException ("Invalid sign option " + opts.SigningLibrary);
					}

					Task<Response<RippleSubmitTxResult>> task =
						NetworkController.UiTxNetworkSubmit (tx, ni, token);

					if (task == null) {
						WriteToInfoBox ("task == null\n");
						return;
					}

					task.Wait (1000 * 60 * 2);

					Response<RippleSubmitTxResult> res = task.Result;

					if (res == null) {
						WriteToInfoBox ("Invalid response\n");
						return;
					}

					if (res.HasError ()) {
						WriteToInfoBox ("Error : " + res?.error_message ?? "{Error message null}");
						return;
					};

					WriteToInfoBox ((res.status ?? "Unknown status") + "\n");

					WriteToInfoBox ((res.result.engine_result ?? "") + "\n");
					WriteToInfoBox ((res.result.engine_result_message ?? "") + "\n");
				});




			};
			
		}

		public void WriteToInfoBox ( string message )
		{
			Gtk.Application.Invoke ( delegate {
				label3.Text += message;

			} );

		}

		private CancellationTokenSource tokenSource = null;

		public void SetRippleWallet (RippleWallet rippleWallet)
		{
			this._rippleWallet = rippleWallet;
		}

		private RippleWallet _rippleWallet {
			get;
			set;
		}

	}
}

