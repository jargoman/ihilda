using System;
using System.Threading.Tasks;
using System.ComponentModel;
using RippleLibSharp.Commands.Stipulate;
using RippleLibSharp.Result;
using RippleLibSharp.Util;
using RippleLibSharp.Network;
using IhildaWallet.Networking;
using RippleLibSharp.Transactions;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using RippleLibSharp.Transactions.TxTypes;
using System.Threading;
using System.Text;
using RippleLibSharp.Keys;
using IhildaWallet.Util;

namespace IhildaWallet
{
	public partial class AutomatedPurchaseWindow : Gtk.Window
	{
		public AutomatedPurchaseWindow (RippleWallet rw, AutomatedOrder automatedOrder, int minTransactions, int maxTransactions) :
				base (Gtk.WindowType.Toplevel)
		{

			this.Build ();

			backgroundWorker = new BackgroundWorker ();
			manualReset = new ManualResetEvent (false);

			backgroundWorker.DoWork += (object sender, DoWorkEventArgs e) => {

				CancellationToken token = this.tokenSource.Token;
				string accout = automatedOrder.Account;
				if (accout == null) {
					//TODO
					WriteToInfoBox ("Account == null");
					return;
				}

				NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();
				if (ni == null) {
					// TODO

					//WriteToInfoBox ("NetworkInterface is null");
					e.Result = null;
					return;
				}

				RippleIdentifier rippleSeedAddress = rw.GetDecryptedSeed ();
				while (rippleSeedAddress.GetHumanReadableIdentifier () == null) {
					bool should = AreYouSure.AskQuestion (
					"Invalid password",
					"Unable to decrypt seed. Invalid password.\nWould you like to try again?"
					);

					if (!should) {
						return;
					}

					rippleSeedAddress = rw.GetDecryptedSeed ();
				}



				Decimal targetPrice = automatedOrder.TakerPays.GetNativeAdjustedCostAt (automatedOrder.TakerGets);
				//Decimal targetPrice = automatedOrder.TakerGets.GetNativeAdjustedPriceAt (automatedOrder.TakerPays);
				string targetMessage = "Target price is " + targetPrice.ToString () + "\n";
				WriteToInfoBox (targetMessage);

				OrderSubmitter orderSubmitter = new OrderSubmitter ();

				orderSubmitter.OnFeeSleep += (object send, FeeSleepEventArgs ee) => {

					switch (ee.State) {
					case FeeSleepState.Begin:
						this.WriteToInfoBox ("Fee " + ee.FeeAndLastLedger.Item1.ToString () + " is too high, waiting on lower fee");
						break;
					case FeeSleepState.PumpUI:
						this.WriteToInfoBox (".");
						break;
					case FeeSleepState.Wake:
						this.WriteToInfoBox ("\n");
						break;
					}

					Gtk.Application.Invoke (
						delegate {
							progressbar1?.Pulse ();

						}
					);
				};

				orderSubmitter.OnOrderSubmitted += (object senderObj, OrderSubmittedEventArgs eventObj) => {
					string submitMessage = "Successfully Submitted order : " + eventObj.RippleOfferTransaction.hash + "\n";
					WriteToInfoBox (submitMessage);
				};

				while (!backgroundWorker.CancellationPending) {



					backgroundWorker.ReportProgress (0);

					if (automatedOrder == null) {
						// TODO

						e.Result = null;
						return;
					}

					if (automatedOrder.TakerGets == null) {
						// TODO
						e.Result = null;
						return;
					}

					if (automatedOrder.TakerPays == null) {
						// TODO
						e.Result = null;
						return;
					}

					Decimal minPaysAmount = automatedOrder.TakerPays.amount / maxTransactions;
					Decimal maxPaysAmount = automatedOrder.TakerPays.amount / minTransactions;

					Decimal minGetsAmount = automatedOrder.TakerGets.amount / maxTransactions;
					Decimal maxGetsAmount = automatedOrder.TakerGets.amount / minTransactions;


					string minPays = "minPaysAmount = " + minPaysAmount.ToString ();
					string maxPays = "maxPaysAmount = " + maxPaysAmount.ToString ();

					string minGets = "minGetsAmount = " + minGetsAmount.ToString ();
					string maxGets = "maxGetsAmount = " + maxGetsAmount.ToString ();

					StringBuilder minMaxMesage = new StringBuilder ();
					minMaxMesage.AppendLine (minPays);
					minMaxMesage.AppendLine (maxPays);
					minMaxMesage.AppendLine (minGets);
					minMaxMesage.AppendLine (maxGets);

					WriteToInfoBox (minMaxMesage.ToString ());

					backgroundWorker.ReportProgress (0);

					Task<Response<BookOfferResult>> task =
						BookOffers.GetResult (
							automatedOrder.TakerPays,
							automatedOrder.TakerGets,
							ni,
							token
						);

					/*
					Task<Response <BookOfferResult>> task = 
						BookOffers.GetResult (
							automatedOrder.TakerGets, 
							automatedOrder.TakerPays, 
							ni
						);
					*/
					if (task == null) {
						// TODO error handling

					}






					while (!task.IsCompleted && !token.IsCancellationRequested && !backgroundWorker.CancellationPending) {

						try {

							backgroundWorker.ReportProgress (0);

							WriteToInfoBox ("Waiting on network");
							task.Wait (1000, token);

							for (int i = 0; i < 20 && !task.IsCompleted && !token.IsCancellationRequested && !backgroundWorker.CancellationPending; i++) {

								backgroundWorker.ReportProgress (0);

								WriteToInfoBox (".");
								task.Wait (1000, token);
							}


						} catch (Exception canEx) when (canEx is TaskCanceledException || canEx is OperationCanceledException) {


							return;
						} catch (Exception /*ex*/) {

							//	Logging.ReportException (method_sig, ex);

							return;

						} finally {
							WriteToInfoBox ("\n");
						}

					}


					Response<BookOfferResult> response = task.Result;
					if (response == null) {
						//

						WriteToInfoBox ("response == null");
						return;
					}

					BookOfferResult bookOfferResult = response.result;


					if (bookOfferResult == null) {
						//

						WriteToInfoBox ("bookOfferResult == null");
						return;
					}

					Offer [] offers = bookOfferResult.offers;
					if (offers == null) {
						WriteToInfoBox ("offers == null");
						return;
					}

					if (offers.Length < 1) {
						// TODO

						WriteToInfoBox ("Orderbook does not have any orders\n");
						return;
					}

					Decimal getsSum = Decimal.Zero;
					Decimal paysSum = Decimal.Zero;
					Decimal bestPrice = Decimal.Zero;


					for (int i = 0; i < offers.Length; i++) {
						Offer offer = offers [i];

						if (offer == null) continue;
						if (offer.taker_gets == null) {

							// TODO
							break;
						}
						if (offer.taker_pays == null) {
							// TODO
							break;
						}

						backgroundWorker.ReportProgress (0);
						getsSum += offer.taker_gets.amount;
						paysSum += offer.taker_pays.amount;

						//bestPrice = offer.taker_gets.GetNativeAdjustedPriceAt ( offer.taker_pays );
						bestPrice = offer.taker_pays.GetNativeAdjustedPriceAt (offer.TakerGets);
						StringBuilder priceMessage = new StringBuilder ();
						priceMessage.Append ("Price at index ");
						priceMessage.AppendLine (i.ToString ());
						priceMessage.AppendLine (" is ");
						priceMessage.Append (bestPrice.ToString ());

						WriteToInfoBox (priceMessage.ToString ());

						if (bestPrice < targetPrice) {
							StringBuilder expenseMessage = new StringBuilder ();
							expenseMessage.Append ("Price at index ");
							expenseMessage.AppendLine (i.ToString ());
							expenseMessage.Append (bestPrice.ToString ());
							expenseMessage.AppendLine (" is too cheap to trade for ");
							WriteToInfoBox (expenseMessage.ToString ());

							break;
						}

						if (paysSum < minGetsAmount) {
							StringBuilder tooSmallMessage = new StringBuilder ();
							tooSmallMessage.Append ("pays sum ");
							tooSmallMessage.Append (paysSum.ToString ());
							tooSmallMessage.Append (" is smaller than minimum ordersize ");
							tooSmallMessage.AppendLine (minGetsAmount.ToString ());
							tooSmallMessage.AppendLine ("checking next order");
							WriteToInfoBox (tooSmallMessage.ToString ());
							continue;
						}

						AutomatedOrder ao = new AutomatedOrder {
							TakerPays = automatedOrder.TakerPays.DeepCopy (),
							TakerGets = automatedOrder.TakerGets.DeepCopy ()
						};

						if (paysSum > maxGetsAmount) {
							ao.TakerGets.amount = maxGetsAmount;
							ao.TakerPays.amount = maxPaysAmount;
						} else {
							//ao.TakerGets.amount = getsSum;
							//ao.TakerPays.amount = paysSum;

							ao.TakerGets.amount = paysSum;
							ao.TakerPays.amount = getsSum;


						}


						StringBuilder stringBuilder = new StringBuilder ();
						stringBuilder.Append ("Submitting offer to trade ");
						stringBuilder.Append (ao.TakerGets.ToString ());
						stringBuilder.Append (" for ");
						stringBuilder.Append (ao.TakerPays.ToString ());
						stringBuilder.AppendLine ();

						WriteToInfoBox (stringBuilder.ToString ());


						AutomatedOrder [] automatedOrders = { new AutomatedOrder (ao) };

						/*Gtk.Application.Invoke ( delegate {
							OrderSubmitWindow orderSubmitWindow = new OrderSubmitWindow ( rw, LicenseType.NONE );
							orderSubmitWindow.Show ();
							orderSubmitWindow.SetOrders (automatedOrders);

						});*/



						Tuple<bool, IEnumerable<OrderSubmittedEventArgs>> tuple = 
							orderSubmitter.SubmitOrders (
								automatedOrders, 
								rw, 
								rippleSeedAddress, 
								ni, 
								token
							);


						if (tuple == null) {
							// TODO likely unreachable code

							return;
						}

						if (!tuple.Item1) {
							// TODO 
							WriteToInfoBox ("Submitting orders failed \n");
							return;
						}




						automatedOrder.taker_gets.amount -= ao.taker_gets.amount;
						automatedOrder.taker_pays.amount -= ao.taker_pays.amount;

						minTransactions--;
						maxTransactions--;
						if (minTransactions < 1) {
							minTransactions = 1;
						}
						if (maxTransactions < 1) {
							maxTransactions = 1;
						}
						break;
					}

					if (automatedOrder.taker_gets.amount <= Decimal.Zero) {
						break;
					}

					if (automatedOrder.taker_pays.amount <= Decimal.Zero) {
						break;
					}


					try {
						WriteToInfoBox ("Sleeping for one minute. \n");

						for (int i = 0; i < 60; i++) {
							if ( backgroundWorker.CancellationPending ) { break; };
							WaitHandle.WaitAny (new [] { manualReset, token.WaitHandle }, 1000);
							backgroundWorker.ReportProgress (0);

						}
					} catch (Exception canex) when (canex is OperationCanceledException || canex is TaskCanceledException) {

					} finally {
						WriteToInfoBox ("\n");
					}


				}

				e.Cancel = backgroundWorker.CancellationPending;

				return;
			};

			backgroundWorker.ProgressChanged += (object sender, ProgressChangedEventArgs e) => {
#if DEBUG
				String method_sig = clsstr + nameof (backgroundWorker.ProgressChanged) + DebugRippleLibSharp.both_parentheses;
				if (DebugIhildaWallet.ProcessSplash) {
					Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
				}


#endif

				if (!(sender is BackgroundWorker backwrk)) {

#if DEBUG
					if (DebugIhildaWallet.FromScriptDialog) {
						Logging.WriteLog (method_sig + "not a BackgroundWorker, returning");
					}
#endif
					return;
				}


				if (!backwrk.IsBusy) {
#if DEBUG
					if (DebugIhildaWallet.ProcessSplash) {
						Logging.WriteLog (method_sig + "is not busy");
					}
#endif
					return;
				}

				if (backwrk.CancellationPending) {
#if DEBUG
					if (DebugIhildaWallet.ProcessSplash) {
						Logging.WriteLog (method_sig + "backwrk.CancellationPending");
					}
#endif
					return;
				}


				Gtk.Application.Invoke (delegate {
#if DEBUG
					if (DebugIhildaWallet.ProcessSplash) {
						Logging.WriteLog (method_sig + "gtk invoke progressbar pulse begin");
					}
#endif

					if (this.progressbar1 == null) {
						return;
					}

					if (!this.progressbar1.IsRealized) {
#if DEBUG
						if (DebugIhildaWallet.ProcessSplash) {
							Logging.WriteLog (method_sig + "progressbar is not realized");
						}
#endif
						return;
					}

					if (!this.progressbar1.IsDrawable) {
#if DEBUG
						if (DebugIhildaWallet.ProcessSplash) {
							Logging.WriteLog (method_sig + "progressbar is not ");
						}
#endif
						return;
					}


#if DEBUG
					if (DebugIhildaWallet.ProcessSplash) {
						Logging.WriteLog (method_sig + "progressbar is realised, pulsing");
					}
#endif


					this.progressbar1.Pulse ();  // wahooo!!
					return;
				});



			};

			backgroundWorker.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs e) => {
				if (e.Cancelled) {
					// TODO
					WriteToInfoBox ("Task has been canceled.\n");
					return;
				}

				// TODO

				WriteToInfoBox ("Automated order has completed\n");
			};

			backgroundWorker.WorkerReportsProgress = true;
			backgroundWorker.WorkerSupportsCancellation = true;

			startbutton.Clicked += Startbutton_Clicked;

			cancelButton.Clicked += (object sender, EventArgs e) => {
				Task.Run (
					(System.Action)backgroundWorker.CancelAsync

				);
				manualReset.Set ();
				tokenSource?.Cancel ();
				//tokenSource.Dispose ();



			};
		}

		private CancellationTokenSource tokenSource = null;

		void Startbutton_Clicked (object sender, EventArgs e)
		{

			if (this.tokenSource != null) {
				MessageDialog.ShowMessage ("Script is already running");
				return;
			}

			Task.Factory.StartNew (backgroundWorker.RunWorkerAsync);
			tokenSource = new CancellationTokenSource ();
			manualReset.Reset ();

			this.cancelButton.Sensitive = true;
			this.startbutton.Sensitive = false;

		}


		public void WriteToInfoBox (string message)
		{

			Gtk.Application.Invoke (delegate {

				this.textview1.Buffer.Text += message;

			});

		}

		private ManualResetEvent manualReset;


#if DEBUG
		public static string clsstr = nameof (ProcessSplash) + DebugRippleLibSharp.colon;
#endif

		BackgroundWorker backgroundWorker;
	}
}
