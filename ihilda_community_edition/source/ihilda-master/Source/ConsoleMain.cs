using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IhildaWallet.Networking;
using RippleLibSharp.Keys;
using RippleLibSharp.Network;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public class ConsoleMain
	{
		public ConsoleMain ()
		{
		}

		private static volatile bool keepRunning = true;

		public static CancellationTokenSource cancellationToken;

		public static CancellationTokenSource TokenSource = null;
		public static void DoConsoleMode (string [] args)
		{

#if DEBUG

			string method_sig = clsstr + nameof (DoConsoleMode) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.Program) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}

#endif

			System.Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e) {
				e.Cancel = true;
				keepRunning = false;
			};

			cancellationToken = new CancellationTokenSource ();
			CancellationToken token = cancellationToken.Token;

			Task t7 = NetworkController.AutoConnect ();

			WalletManager walletManager = new WalletManager ();

			var rippleWallet = walletManager.LookUp (ProgramVariables.botMode);

			if (rippleWallet == null) {
				Logging.WriteLog ("Could not find wallet : " + ProgramVariables.botMode);
				return;
			}

			Logging.WriteLog (rippleWallet.GetStoredReceiveAddress ());






			string account = rippleWallet.GetStoredReceiveAddress ();

			RuleManager RuleManagerObj = new RuleManager (account);
			RuleManagerObj.LoadRules ();


			Logging.WriteLog (RuleManagerObj.RulesList.Count.ToString ());

			OrderSubmitter orderSubmitter = new OrderSubmitter ();

			orderSubmitter.OnFeeSleep += (object sender, FeeSleepEventArgs e) => {
				if (e.State == FeeSleepState.Begin) {
					Logging.WriteLog ("Fee " + (string)(e?.FeeAndLastLedger?.Fee.ToString () ?? "null") + " is too high, waiting on lower fee");
					return;
				}

				if (e.State == FeeSleepState.PumpUI) {
					Logging.WriteLog (".");
					return;
				}

				if (e.State == FeeSleepState.Wake) {
					Logging.WriteLog ("\n");
					return;
				}



			};

			orderSubmitter.OnOrderSubmitted += (object sender, OrderSubmittedEventArgs e) => {
				StringBuilder stringBuilder = new StringBuilder ();


				if (e.Success) {
					stringBuilder.Append ("Submitted Order Successfully ");
					stringBuilder.Append ((string)(e?.RippleOfferTransaction?.hash ?? ""));




				} else {
					stringBuilder.Append ("Failed to submit order ");
					stringBuilder.Append ((string)(e?.RippleOfferTransaction?.hash ?? ""));

				}

				stringBuilder.AppendLine ();


				Logging.WriteLog (stringBuilder.ToString ());
			};

			orderSubmitter.OnVerifyingTxBegin += (object sender, VerifyEventArgs e) => {
				StringBuilder stringBuilder = new StringBuilder ();

				stringBuilder.Append ("Verifying transaction ");
				stringBuilder.Append ((string)(e.RippleOfferTransaction.hash ?? ""));
				stringBuilder.AppendLine ();

				Logging.WriteLog (stringBuilder.ToString ());

			};

			orderSubmitter.OnVerifyingTxReturn += (object sender, VerifyEventArgs e) => {
				StringBuilder stringBuilder = new StringBuilder ();
				string messg = null;
				if (e.Success) {
					stringBuilder.Append ("Transaction ");
					stringBuilder.Append ((string)(e?.RippleOfferTransaction?.hash ?? ""));
					stringBuilder.Append (" Verified");


					TextHighlighter.Highlightcolor = ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN;




				} else {
					stringBuilder.Append ("Failed to validate transaction ");
					stringBuilder.Append ((string)(e?.RippleOfferTransaction?.hash ?? ""));

					TextHighlighter.Highlightcolor = TextHighlighter.RED;

				}

				stringBuilder.AppendLine ();

				messg = TextHighlighter.Highlight (stringBuilder);

				Logging.WriteLog (messg);
			};


			Logging.WriteLog ("Running automation script for address " + account + "\n");
			Robotics robot = new Robotics (RuleManagerObj);

			robot.OnMessage += (object sender, MessageEventArgs e) => {
				Logging.WriteLog (e.Message);
			};



			t7.Wait (cancellationToken.Token);



			NetworkInterface ni = NetworkController.CurrentInterface;

			if (ni == null) {
				Logging.WriteLog ("Was unable to connect to network. Exiting");
				Environment.Exit (-1);
			}

			RippleIdentifier rippleSeedAddress = rippleWallet.GetDecryptedSeed ();

			while (rippleSeedAddress.GetHumanReadableIdentifier () == null) {

				Logging.WriteLog ("Unable to decrypt seed. Invalid password.\nWould you like to try again?");


				// TODO get user input

				rippleSeedAddress = rippleWallet.GetDecryptedSeed ();
			}


			if (rippleSeedAddress == null) {
				Environment.Exit (-1);
			}
			bool success = false;

			// Keeps track of where the next start ledger will be
			uint? projectedStart = null;

			if (!keepRunning) {
				string exitingMessage = "Quit request received\n";
				Logging.WriteLog (exitingMessage);

				// TODO environment var
				Environment.Exit (-1);
			}
			do {


				if (ProgramVariables.endledger != 0 && ProgramVariables.endledger != -1) {

					if (projectedStart != null && projectedStart > ProgramVariables.endledger) {

						string maxMessage = "Maximum ledger " + ProgramVariables.endledger.ToString () + " reached \n";
						Logging.WriteLog (maxMessage);

						// TODO 

						System.Environment.Exit (-1);
					}

				}


				Logging.WriteLog (
					"Polling data for "
					+ (string)(rippleWallet?.GetStoredReceiveAddress () ?? "null")
					+ "\n");


				WalletLedgerSave ledgerSave = WalletLedgerSave.LoadLedger (rippleWallet.BotLedgerPath);


				uint? last = ledgerSave?.Ledger;
				if (last == null || last == 0) {
					// TODO warning
				}


				if (ProgramVariables.ledger != 0) {
					Logging.WriteLog ("\nStarting from ledger " + (string)ProgramVariables.ledger.ToString () + "\n");
				} else {


					Logging.WriteLog ("\nUsing last known ledger " + last + "\n");
					if (last == 0) {

						Logging.WriteLog ("");
						Environment.Exit (-1);
					}
				}




				DoLogicResponse tuple = null;

				tuple = robot.DoLogic (rippleWallet, ni, last, ProgramVariables.endledger == 0 ? -1 : ProgramVariables.endledger, null, token);


				if (tuple == null) {
					return;
				}

				projectedStart = tuple.LastLedger + 1;



				IEnumerable<AutomatedOrder> orders = tuple.FilledOrders;
				if (orders == null || !orders.Any ()) {
					int seconds = 60;

					string infoMessage = "Sleeping for " + seconds + " seconds";

					try {
						Logging.WriteLog (infoMessage);
						for (int sec = 0; sec < seconds; sec++) {

							if (token.IsCancellationRequested || !keepRunning) {
								return;
							}

							if (sec % 5 == 0) {
								Logging.WriteLog (".");
							}
							token.WaitHandle.WaitOne (1000);



						}
					} catch (Exception e) when (e is TaskCanceledException || e is OperationCanceledException) {
#if DEBUG
						if (DebugIhildaWallet.Program) {
							Logging.ReportException (method_sig, e);
						}
#endif

						return;
					} finally {
						Logging.WriteLog ("\n");
					}
					continue;
				}

				int numb = orders.Count ();
				string submitMessage = "Submitting " + numb.ToString () + " orders\n";
				Logging.WriteLog (submitMessage);

				Tuple<bool, IEnumerable<OrderSubmittedEventArgs>> tupleResp = null;

				if (!ProgramVariables.parallelVerify) {
					orderSubmitter.SubmitOrders (orders, rippleWallet, rippleSeedAddress, ni, token);
				} else {
					orderSubmitter.SubmitOrdersParallel (orders, rippleWallet, rippleSeedAddress, ni, token);
				}

				if (tupleResp == null) {
					// TODO. probably unreachable code
				}

				success = tupleResp.Item1;
				if (!success) {
					string errMess = "Error submitting orders\n";
					Logging.WriteLog (errMess);
					//shouldContinue = false;
					break;
					//return;
				}

				string successMessage = "Orders submitted successfully\n";
				Logging.WriteLog (successMessage);

			} while (!token.IsCancellationRequested && keepRunning);
		}

		public static void DoExit ()
		{


			Task cancelTask = Task.Run ((System.Action)TokenSource.Cancel);

			if (Console.currentInstance != null) {
				Console.currentInstance.Hide ();
				Console.currentInstance.Destroy ();
				Console.currentInstance = null;
			}

			cancelTask.Wait ();
			TokenSource.Dispose ();


		}

		public static string GetPassword ()
		{
			string pass = "";
			do {
				ConsoleKeyInfo keyInfo = System.Console.ReadKey (true);
				if (keyInfo.Key != ConsoleKey.Backspace && keyInfo.Key != ConsoleKey.Enter) {
					pass += keyInfo.KeyChar;
					System.Console.Write ('*');

				} else {
					if (keyInfo.Key == ConsoleKey.Backspace && pass.Length > 0) {
						pass = pass.Substring (0, (pass.Length - 1));
						System.Console.Write ("\b \b");
					} else if (keyInfo.Key == ConsoleKey.Enter) {
						break;
					}
				}

			} while (true);

			return pass;
		}


#if DEBUG
		private const string clsstr = nameof (ConsoleMain) + DebugRippleLibSharp.colon;
#endif
	}
}
