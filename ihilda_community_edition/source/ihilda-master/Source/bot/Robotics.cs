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

namespace IhildaWallet
{
	public class Robotics
	{
		public Robotics (RuleManager rules)
		{

			this.RuleManagerObj = rules;

		}

		public Tuple<Int32?, IEnumerable<AutomatedOrder>> DoLogic (
			RippleWallet wallet,
			NetworkInterface ni,
			Int32? ledgerstart,
			Int32? ledgerend,
			Int32? limit,
			CancellationToken cancelationToken
		)
		{

#if DEBUG
			string method_sig = clsstr + nameof (DoLogic) + DebugRippleLibSharp.both_parentheses;
#endif
			OrderManagementBot omb = null;

			var ombTask = Task.Run (delegate {
				 omb = new OrderManagementBot (wallet, ni, cancelationToken);

				omb.OnMessage += (object sender, MessageEventArgs e) => {
					OnMessage?.Invoke (this, new MessageEventArgs () { Message = e.Message });
				};
			}, cancelationToken);

			if (cancelationToken.IsCancellationRequested) {
				return null;
			}

			if (StopWhenConvenient) {
				return null;
			}

			string ledgerMax = ledgerend?.ToString() ?? (-1).ToString ();
			string ledgerMin = ledgerstart?.ToString();

			if (ledgerMin == null) {
				
				if (RuleManagerObj?.LastKnownLedger == null) {
					// TODO
				}
				int lastRuleLedger = ((int)(RuleManagerObj?.LastKnownLedger));
				ledgerMin = lastRuleLedger.ToString();

			}

			int lim = limit ?? 0;

			RETRY:

			Task<IEnumerable <Response<AccountTxResult>>> task = null;

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
				for (seconds = 0;  !task.IsCompleted && !task.IsFaulted && !task.IsCanceled && !cancelationToken.IsCancellationRequested && seconds < maxSeconds && !StopWhenConvenient;  ) {
					try {
						OnMessage?.Invoke (this, new MessageEventArgs { Message = "Waiting on network" });
						for (int i = 0; i < 10 && !task.IsCompleted && !cancelationToken.IsCancellationRequested; i++, seconds++) {  // seconds are incremented where a second actually occurs and not in it's own loop. It's going to be ok. 
							OnMessage?.Invoke (this, new MessageEventArgs () { Message = "." });
							task.Wait (1000, cancelationToken);
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
					goto RETRY;
				}

				if (!(seconds < maxSeconds)) {
					OnMessage?.Invoke (this, new MessageEventArgs () { Message = "No response after " + minutes + " minutes elapsed. Retrying \n" });
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

			IEnumerable<Response<AccountTxResult>> responses = task?.Result;  



			if (responses == null) {
				OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Null response\n" });
				return null;
			}

			OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Received response from network\n" });



			OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Received response from server\n" });


			IEnumerable<RippleTxStructure> structures = null;

			int lastledger = 0;
			if (!Program.preferLinq) {

				List<RippleTxStructure> txStructures = new List<RippleTxStructure> ();



				foreach (Response<AccountTxResult> res in responses) {

					AccountTxResult accTxResult = res.result;

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

					lastledger = Math.Max (accTxResult.ledger_index_max, lastledger);

					structures = txStructures;
				}


			} else {

				var results = from x in responses where x?.result != null select x.result;

				lastledger = results.Max (arg => arg.ledger_index_max);

				var txStructuresLinq = results
				.Where (result => result.transactions != null)
			    	.SelectMany (result =>
				     result.transactions
			    	);

				structures = txStructuresLinq;
			}

			if (!ombTask.IsCompleted && !ombTask.IsCanceled && !ombTask.IsFaulted) {
				try {

					OnMessage?.Invoke (
						this, 
						new MessageEventArgs () { 
							Message = "Waiting on order management bot to load" });


					cancelationToken.WaitHandle.WaitOne (1000);
					while (!ombTask.IsCompleted && !ombTask.IsCanceled && !ombTask.IsFaulted) {
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
			RuleManagerObj.LastKnownLedger = lastledger;
			RuleManagerObj.SaveRules ();

			Tuple<Int32?, IEnumerable <AutomatedOrder>> tuple = new Tuple<int?, IEnumerable <AutomatedOrder>> (lastledger, orders);

			return tuple;


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

}

