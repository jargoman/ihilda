using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Keys;
using RippleLibSharp.Network;

using IhildaWallet.Networking;

using RippleLibSharp.Result;
using RippleLibSharp.Commands.Accounts;

using RippleLibSharp.Transactions;
using RippleLibSharp.Util;
using System.Threading;
using System.Media;

namespace IhildaWallet
{
	public class Robotics
	{
		public Robotics (RuleManager rules)
		{

			this.RuleManagerObj = rules;

		}

		public DoLogicResponse DoLogic (
			RippleWallet wallet,
			NetworkInterface ni,
			Int64? ledgerstart,
			Int64? ledgerend,
			Int32? limit,
			CancellationToken cancelationToken
		)
		{


#if DEBUG
			string method_sig = clsstr + nameof (DoLogic) + DebugRippleLibSharp.both_parentheses;
#endif

			DoLogicResponse logicResponse = new DoLogicResponse ();

			OrderManagementBot omb = null;
			var ombTask = Task.Run (delegate {
				omb = new OrderManagementBot (wallet, ni, cancelationToken);

				omb.OnMessage += (object sender, MessageEventArgs e) => {
					OnMessage?.Invoke (this, new MessageEventArgs () { Message = e?.Message });
				};
			}, cancelationToken);

			SoundSettings settings = SoundSettings.LoadSoundSettings ();


			LedgerSave ledgerSave = LedgerSave.LoadLedger (wallet.BotLedgerPath);

			if (cancelationToken.IsCancellationRequested) {

				return null;
			}

			if (StopWhenConvenient) {
				return null;
			}

			string ledgerMax = ledgerend?.ToString () ?? (-1).ToString ();
			string ledgerMin = ledgerstart?.ToString ();

			if (ledgerMin == null) {

				if (ledgerSave.Ledger == null) {
					// TODO it might not be an error. What if they are running 
					//logicResponse.HasError = true;
					//logicResponse.ErrorMessage = "";
				}
				uint? lastRuleLedger = ledgerSave.Ledger;
				ledgerMin = lastRuleLedger?.ToString ();

			}

			int lim = limit ?? 0;

			int retry_count = 0;
		RETRY:
			if (retry_count++ > 3) {
				return null;
			}

			Task<FullTxResponse> task = null;

			try {

				task = limit == null
					? AccountTx.GetFullTxResult (
							wallet.GetStoredReceiveAddress (),
							ledgerMin,
							ledgerMax,

							/*false,*/
							ni,
						cancelationToken
					)
					: AccountTx.GetFullTxResult (
							wallet.GetStoredReceiveAddress (),
							ledgerMin,
							ledgerMax,
							lim,
							/*false,*/
							ni,
							   cancelationToken
							  );

				if (task == null) {
					//return null;
					throw new NullReferenceException ();
				}


				int minutes = 4;
				int maxSeconds = 60 * minutes; // 
				int seconds;
				for (seconds = 0; task != null && !task.IsCompleted && !task.IsFaulted && !task.IsCanceled && !cancelationToken.IsCancellationRequested && seconds < maxSeconds && !StopWhenConvenient;  ) {
					try {
						OnMessage?.Invoke (this, new MessageEventArgs { Message = "Waiting on network" });
						for (int i = 0; i < 10 && !task.IsCompleted && !cancelationToken.IsCancellationRequested; i++, seconds++) {  // seconds are incremented where a second actually occurs and not in it's own loop. It's going to be ok. 
							OnMessage?.Invoke (this, new MessageEventArgs () { Message = "." });
							task?.Wait (1000, cancelationToken);
						}
					} catch (Exception e) {

#if DEBUG
						if (DebugIhildaWallet.Robotics) {
							Logging.ReportException (method_sig, e);
						}
#endif
						throw e;


					} finally {
						OnMessage?.Invoke (this, new MessageEventArgs () { Message = "\n" });

					}

					if (seconds > 60 && !StopWhenConvenient) {
						if (Program.botMode == null) {
							OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Retrieving tx list is taking longer than usual. \"Stop when convient\" will exit loop gracefully\n" });
						} else {
							OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Retrieving tx list is taking longer than usual.\n" });
						}
					}

				}

				if (StopWhenConvenient) {
					return null;
				}

				if (task.IsCanceled || cancelationToken.IsCancellationRequested) {

					return null;
				}

				if (task.IsFaulted) {
					OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Network task faulted Retrying\n" });
					if (settings.HasOnNetWorkFail && settings.OnNetWorkFail != null) {

						Task.Run (delegate {

							SoundPlayer player =
							new SoundPlayer (settings.OnNetWorkFail);
							player.Load ();
							player.Play ();
						});

					}

		    			
					goto RETRY;
				}

				if (!(seconds < maxSeconds)) {
					OnMessage?.Invoke (this, new MessageEventArgs () { Message = "No response after " + minutes + " minutes elapsed. Retrying \n" });

					if (settings.HasOnNetWorkFail && settings.OnNetWorkFail != null) {

						Task.Run (delegate {

							SoundPlayer player =
							new SoundPlayer (settings.OnNetWorkFail);
							player.Load ();
							player.Play ();
						});

					}



					goto RETRY;
				}


			} catch (Exception ex) when (ex is OperationCanceledException || ex is TaskCanceledException) {
#if DEBUG
				if (DebugIhildaWallet.Robotics) {
					Logging.ReportException (method_sig, ex);
				}
#endif
				throw ex;
			}

			catch (Exception e) {

				if (settings.HasOnNetWorkFail && settings.OnNetWorkFail != null) {

					Task.Run (delegate {

						SoundPlayer player =
						new SoundPlayer (settings.OnNetWorkFail);
						player.Load ();
						player.Play ();
					});

				}




				StringBuilder errorMessage = new StringBuilder ();

				errorMessage.AppendLine ("Network exception : ");
				if (e != null) {
					errorMessage.AppendLine (e.Message);
					errorMessage.AppendLine (e.StackTrace);
					while (e.InnerException != null) {
						errorMessage.AppendLine ("Inner Exception");
						errorMessage.AppendLine (e.InnerException.Message);
						errorMessage.AppendLine (e.InnerException.StackTrace);

						e = e.InnerException;
					}
				}

				OnMessage?.Invoke (this, new MessageEventArgs () { Message = errorMessage.ToString () + "\n" });
				Logging.WriteBoth (errorMessage.ToString());
				MessageDialog.ShowMessage (errorMessage.ToString());

				return null;
			}

			if (cancelationToken.IsCancellationRequested) {
				return null;
			}

			if (StopWhenConvenient) {
				return null;
			}

			FullTxResponse fullTx = task?.Result;

			if (fullTx == null) {
				return null;
			}

			if (fullTx.HasError) {
				return null;
			}

			if (fullTx.Responses == null) {
				return null;
			}


			IEnumerable<Response<AccountTxResult>> responses = fullTx.Responses;  



			if (responses == null) {
				OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Null response\n" });
				if (settings.HasOnNetWorkFail && settings.OnNetWorkFail != null) {

					Task.Run (delegate {

						SoundPlayer player =
						new SoundPlayer (settings.OnNetWorkFail);
						player.Load ();
						player.Play ();
					});

				}

				goto RETRY;
				//return null;
			}

	//		OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Received response from network\n" });



			OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Received response from server\n" });


			IEnumerable<RippleTxStructure> structures = null;

			uint lastledger = 0;
			if (!Program.preferLinq) {

				List<RippleTxStructure> txStructures = new List<RippleTxStructure> ();



				foreach ( Response<AccountTxResult> res in responses ) {

					AccountTxResult accTxResult = res?.result;

					if (accTxResult == null) {
						return null;
					}


#if DEBUG

					string debug =
						"ledgermax" +
						accTxResult.ledger_index_max.ToString () +

						"ledgermin" +
						accTxResult.ledger_index_min.ToString ();

					Logging.WriteLog (debug);

#endif

					RippleTxStructure [] txs = accTxResult.transactions;

					txStructures.AddRange (txs);

					lastledger = (uint)Math.Max (accTxResult.ledger_index_max, lastledger);

					structures = txStructures;
				}


			} else {

				IEnumerable<AccountTxResult> results = from x in responses where x?.result != null select x.result;

				lastledger = (uint)results.Max (arg => arg.ledger_index_max);

				var txStructuresLinq = results
				.Where (result => result.transactions != null)
			    	.SelectMany (result =>
				     result.transactions
			    	);

				structures = txStructuresLinq;
			}

			if (!ombTask.IsCompleted && !ombTask.IsCanceled && !ombTask.IsFaulted && !cancelationToken.IsCancellationRequested) {
				try {

					OnMessage?.Invoke (
						this, 
						new MessageEventArgs () { 
							Message = "Waiting on order management bot to load" });


					cancelationToken.WaitHandle.WaitOne (1000);
					while (!ombTask.IsCompleted && !ombTask.IsCanceled && !ombTask.IsFaulted && !cancelationToken.IsCancellationRequested) {
						OnMessage?.Invoke (this, new MessageEventArgs () { Message = "." });
						cancelationToken.WaitHandle.WaitOne (1000);
					}
				} catch (Exception e) {
#if DEBUG
					if (DebugIhildaWallet.Robotics) {
						Logging.ReportException (method_sig, e);
					}
#endif
				} finally {
					OnMessage?.Invoke (this, new MessageEventArgs () { Message = "\n" });
				}
			}

			if (omb == null) {
				OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Order management bot failed to load\n" });
				return null;
			}

			IEnumerable<AutomatedOrder> total = null;

			OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Processing transactions\n" });
			try {

				total = omb.UpdateTx (structures);
			}

#pragma warning disable 0168
			catch (Exception e) {
#pragma warning restore 0168

#if DEBUG
				if (DebugIhildaWallet.Robotics) {
					Logging.ReportException (method_sig, e);
				}
#endif
				//MessageDialog.showMessage ("Exception processing tx's");
				MessageDialog.ShowMessage ("Exception processing tx's\n" + e.ToString () + e.StackTrace);
				return null;
			}


			if (total == null) {
				MessageDialog.ShowMessage ("Error processing tx's\n" + " total == null\n");
				return null;
			}

			//updateFilledOrders (total);

			IEnumerable<AutomatedOrder> orders = null;



			if (total.Any()) {
				OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Generating buy back orders" });
				try {

					var genTask = Task.Run (delegate {
						orders = omb.GetBuyBackOrders (total);

					});

					while ( genTask != null && !genTask.IsCanceled && !genTask.IsCompleted && !genTask.IsFaulted) {
						genTask.Wait (500);
						OnMessage?.Invoke (this, new MessageEventArgs () { Message = "." });
					}


#pragma warning disable 0168
				} catch (Exception e) {
#pragma warning restore 0168

#if DEBUG
					if (DebugIhildaWallet.Robotics) {
						Logging.ReportException (method_sig, e);
					}
#endif

					StringBuilder message = new StringBuilder ();
					message.Append ("\nException calculating buyorders...\nMessage : \n");
					message.Append (e.Message);
					message.Append ("\n");
					message.Append ("Stakctrace : \n");
					message.Append (e.StackTrace);
					MessageDialog.ShowMessage (message.ToString ());
					OnMessage?.Invoke (this, new MessageEventArgs () { Message = message.ToString () });
					return null;
				} finally {

					OnMessage?.Invoke (this, new MessageEventArgs () { Message = "\n"});
				}

				OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Generated " + orders.Count() + " Orders\n" });

			}


			OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Saving lastledger to rule list\n" });
			//RuleManagerObj.LastKnownLedger = lastledger;
			//RuleManagerObj.SaveRules ();
			wallet.SaveBotLedger (lastledger, wallet.BotLedgerPath);

			logicResponse.LastLedger = (uint?)lastledger;
			logicResponse.FilledOrders = orders;

			return logicResponse;


		}

		public event EventHandler<MessageEventArgs> OnMessage;




		/*
		public void LaunchPreviewWidget (IEnumerable<AutomatedOrder> orders)
		{
			Gtk.Application.Invoke ( (object sender, EventArgs e) => {




			});
		}
		*/

		public RuleManager RuleManagerObj {
			get;
			set;
		}

		public bool StopWhenConvenient {
			get;
			set;
		}


#if DEBUG
		private const string clsstr = nameof (Robotics) + DebugRippleLibSharp.colon;
#endif




	}

	public class MessageEventArgs : EventArgs
	{

		public string Message {
			get;
			set;
		}
	}

	public class DoLogicResponse
	{
		public IEnumerable<AutomatedOrder> FilledOrders {
			get;
			set;
		}

		public uint? LastLedger {
			get;
			set;
		}

		public string ErrorMessage {
			get;
			set;
		}

		private string _err_mess = null;

		public int ErrorCode {
			get;
			set;
		}

		public bool HasError {
			get;
			set;
		}

		private bool hasErr = false;
	}

}

