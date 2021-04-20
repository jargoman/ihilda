using System;

using RippleLibSharp.Transactions;
using RippleLibSharp.Transactions.TxTypes;

using RippleLibSharp.Network;
using IhildaWallet.Networking;

using RippleLibSharp.Keys;
using System.Threading;
using System.Threading.Tasks;
using RippleLibSharp.Result;
using RippleLibSharp.Util;
using System.Text;
using RippleLibSharp.Commands.Tx;
using RippleLibSharp.Commands.Subscriptions;
using RippleLibSharp.Commands.Server;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CancelTxWidget : Gtk.Bin
	{
		public CancelTxWidget ()
		{
			this.Build ();

			this.AbortCancellationButton.Clicked += (object sender, EventArgs e) => {
				tokenSource?.Cancel ();
			};

			this.cancelOrderButton.Clicked += (object sender, EventArgs e) => {

#if DEBUG
				string method_sig = clsstr + (nameof(cancelOrderButton)) + DebugRippleLibSharp.colon;
#endif
				//tokenSource?.Cancel ();


				label3.Text = "";

				// TODO lookup tx, verify matches signing account, are you sure?? ect



				string tx_hash = this.comboboxentry1?.Entry?.Text;

				Task.Run (delegate {

					try {
						tokenSource = new CancellationTokenSource ();
						CancellationToken token = tokenSource.Token;
						//WriteToInfoBox ("");

						if (string.IsNullOrWhiteSpace (tx_hash)) {
							WriteToInfoBox ("Please specify a transaction hash\n");
							return;
						}



						tx_hash = tx_hash.Trim ();

						WriteToInfoBox ("Cancelling order " + tx_hash + "\n");


						RippleCancelTransaction tx = new RippleCancelTransaction ();

						RippleWallet rw = _rippleWallet;
						if (rw == null) {


							WriteToInfoBox ("Please specify a wallet using the wallet manager\n");

							return;
						}


						string acc = rw.GetStoredReceiveAddress ();

						NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();
						if (ni == null) {
							string messg = "Unable to connect to network";
							MessageDialog.ShowMessage ("Network Error", messg);
							WriteToInfoBox (messg + "\n");
							return;
						}



						PasswordAttempt passwordAttempt = new PasswordAttempt ();

						passwordAttempt.InvalidPassEvent += (object s, EventArgs ev) =>
						{
							bool shou = AreYouSure.AskQuestionNonGuiThread (
							"Invalid password",
							"Unable to decrypt seed. Invalid password.\nWould you like to try again?"
							);
						};

						passwordAttempt.MaxPassEvent += (object s, EventArgs ev) =>
						{
							string mess = "Max password attempts";

							MessageDialog.ShowMessage (mess);
							//WriteToOurputScreen ("\n" + mess + "\n");
						};


						DecryptResponse decryptResponse = passwordAttempt.DoRequest (rw, token);




						RippleIdentifier seed = decryptResponse.Seed;



						if (seed?.GetHumanReadableIdentifier () == null) {
							return;
						}


						bool isSeq = UInt32.TryParse (tx_hash, out uint offerseq);
						if (!isSeq) {

							WriteToInfoBox ("Looking up sequence using hash..");

							//var tx_task = RippleLibSharp.Commands.Tx.tx.GetRequestDataApi (tx_hash, token);
							var tx_task = RippleLibSharp.Commands.Tx.tx.GetRequest (tx_hash, ni, token);
							if (tx_task == null) {

								WriteToInfoBox ("Unrecoverable error accessing network/n");
								return;
							}

							int count2 = 0;
							while (!tx_task.IsCompleted && !tx_task.IsFaulted && !tx_task.IsCanceled) {
								WriteToInfoBox (".");
								tx_task.Wait (1000);
								if (count2 % 10 == 9) {
									WriteToInfoBox ("/nWaiting on network..");
								}
								count2++;
							}

							WriteToInfoBox ("\n");

							if (tx_task == null) {
								// will never happen
								WriteToInfoBox ("Unrecoverable error accessing network\n");
								return;
							}

							var response = tx_task.Result;

							if (response == null) {
								WriteToInfoBox ("Invlid respose from network\n");
								return;
							}

							if (response.HasError ()) {
								WriteToInfoBox ("<span foreground=\"red\">Error : </span>\n");
								WriteToInfoBox (response.error_message + "\n");
								return;
							}

							var seqres = response.result;

							if (seqres == null) {
								// shouldn't happen
								WriteToInfoBox ("Invalid response \n");
								return;
							}

							offerseq = seqres.Sequence;

							WriteToInfoBox ("Found order with sequence " + (offerseq.ToString ()) + "\n");
						}


						WriteToInfoBox ("Canceling order with sequence " + offerseq.ToString () + "\n");

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

						SignOptions opts = SignOptions.LoadSignOptions ();

						if (opts == null) {
							// unreachable code
							WriteToInfoBox ("Unrecoverble error retrieving sign options\n");
							return;
						}

						if (opts.HasError) {
							WriteToInfoBox ("Failed to load sign options\n");
							WriteToInfoBox (opts.ErrorMessage + "\n");
							return;
						}

						WriteToInfoBox ("Retrieving fee and current ledger from network\n");
						ParsedFeeAndLedgerResp tupe = feeSettings.GetFeeAndLastLedgerFromSettings (ni, token);
						if (tupe == null) {
							WriteToInfoBox ("Failed to retrieve fee and last ledger\n");
							return;
						}

						if (tupe.HasError) {
							WriteToInfoBox (tupe.ErrorMessage);
							return;
						}




						tx.fee = ((UInt32)tupe.Fee).ToString ();

						WriteToInfoBox ("Using fee " + (tupe.Fee).ToString () + " in drops\n");

						tx.Sequence = se; // 




						tx.LastLedgerSequence = (UInt32)tupe.LastLedger + (opts?.LastLedgerOffset ?? SignOptions.DEFAUL_LAST_LEDGER_SEQ);

						tx.Account = rw.GetStoredReceiveAddress ();



						tx.OfferSequence = offerseq;





						switch (opts.SigningLibrary) {
						case "RippleDotNet":
							WriteToInfoBox ("Signing with RippleDotNet\n");

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

							var signature3 = tx.SignRippleLibSharp (seed);
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

						string subMessage = "Submitting cancel request..";

						int count = 0;
						do {
							WriteToInfoBox (subMessage);
							while ((!task.IsCompleted && !task.IsFaulted && !task.IsCanceled) && (count % 10 != 9)) {

								WriteToInfoBox (".");
								task.Wait (1000);
								count++;
							}
							WriteToInfoBox ("\n");

							if (count > 120) {
								WriteToInfoBox ("Network request took too long. Aborting\n");
								return;
							}

						} while (!task.IsCompleted && !task.IsFaulted && !task.IsCanceled);

						if (task.IsCanceled) {
							WriteToInfoBox ("Request canceled\n");
							return;
						}

						if (task.IsFaulted) {
							WriteToInfoBox ("Submit task faulted\n");
							return;
						}




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

						Ter ter;

						try {
							ter = (Ter)Enum.Parse (typeof (Ter), res.result.engine_result, true);
							//ter = (Ter)Ter.Parse (typeof(Ter), een, true);

						} catch (ArgumentNullException exc) {

#if DEBUG

#endif


							return;
						} catch (OverflowException overFlowException) {


							return;

						} catch (ArgumentException argumentException) {



							WriteToInfoBox ("Engine result not implemented\n");

							return;
						} catch (Exception ex) {


							WriteToInfoBox (ex.Message);
							return;
						}


						switch (ter) {
						case Ter.tesSUCCESS:
						case Ter.terQUEUED:

							// TODO verify tx
							ValidateTx (tx.hash, tx.LastLedgerSequence, ni, token);

							break;

						default:
							return;
						}



					} catch (Exception ex) {
						WriteToInfoBox ("Exception event cancelling order\n");
						WriteToInfoBox (ex.Message);
#if DEBUG
						Logging.ReportException (method_sig, ex);
#endif
					} finally {
						tokenSource.Cancel ();
					}
				});




			};
			
		}


		public void ValidateTx (string hash, uint lastLedger, NetworkInterface ni, CancellationToken token)
		{


#if DEBUG
			string method_sig = clsstr + (nameof (cancelOrderButton)) + DebugRippleLibSharp.colon;
#endif


			try {

				// seconds to wait before validating
				int verifyWaitTime = 6;

				string valStr = "Validating Tx";
				StringBuilder valStrBuild = new StringBuilder ();

				for (int i = verifyWaitTime; i > 0; i--) {

					valStrBuild.Clear ();
					valStrBuild.Append (valStr);
					valStrBuild.Append (" in ");
					valStrBuild.AppendLine (i.ToString ());

					this.WriteToInfoBox (valStrBuild.ToString ());

					token.WaitHandle.WaitOne (1000);
				}



				for (int i = 0; i < 100; i++) {


					//FeeAndLastLedgerResponse feeResp = ServerInfo.GetFeeAndLedgerSequence (ni, token);
					Task<Response<RippleTransaction>> task = tx.GetRequest (hash, ni, token);



					Task<uint?> ledgerTask = Task.Run (
						delegate {

							for (int attempt = 0; attempt < 5; attempt++) {
								uint? ledgerOrNull = LedgerTracker.GetRecentLedgerOrNull ();

								if (ledgerOrNull == null) {

									FeeAndLastLedgerResponse feeResp = ServerInfo.GetFeeAndLedgerSequence (ni, token);
									ledgerOrNull = feeResp?.LastLedger;


								}
								if (ledgerOrNull != null) {
									return ledgerOrNull;
								}
							}

							return null;
						}
					);


					if (task == null) {
						// TODO Debug
						this.WriteToInfoBox ("Error : task == null\n");
						return;
					}
					task.Wait (token);

					Response<RippleTransaction> response = task.Result;
					if (response == null) {
						this.WriteToInfoBox ("Error : response == null\n");
						return;
					}
					RippleTransaction transaction = response.result;

					if (transaction == null) {
						this.WriteToInfoBox ("Error : transaction == null\n");
					}

					if (transaction.validated != null && (bool)transaction.validated) {

						this.WriteToInfoBox ("Validated\n");


						// TODO




						return;
					}

					if (ledgerTask != null) {
						ledgerTask.Wait (1000 * 60 * 2, token);
					}


					//
					uint? ledger = ledgerTask?.Result;

					if (ledger != null && ledger > lastLedger) {
						this.WriteToInfoBox ("failed to validate before LastLedgerSequence exceeded");
					}

					string str = "Not validated ";

					try {
						valStrBuild.Clear ();
						valStrBuild.Append (str);
						valStrBuild.Append (i.ToString ());

						this.WriteToInfoBox (valStrBuild.ToString ());


						for (int ind = 0; ind < 2; ind++) {

							token.WaitHandle.WaitOne (1000);

							this.WriteToInfoBox (".");

						}


					} finally {

						this.WriteToInfoBox ("\n");
					}




				}

			} catch (Exception e) {

				this.WriteToInfoBox (e.Message);
			}
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

#if DEBUG
		private const string clsstr = nameof (CancelTxWidget) + DebugRippleLibSharp.colon;
#endif

	}
}

