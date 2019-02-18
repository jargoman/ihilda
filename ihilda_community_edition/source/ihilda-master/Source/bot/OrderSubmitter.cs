using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IhildaWallet.Networking;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Commands.Server;
using RippleLibSharp.Commands.Subscriptions;
using RippleLibSharp.Commands.Tx;
using RippleLibSharp.Keys;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;
using RippleLibSharp.Transactions.TxTypes;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public class OrderSubmitter
	{
		
		//private IEnumerable<AutomatedOrder> orders = null;



		public MultipleOrdersSubmitResponse SubmitOrdersParallelImproved (
			IEnumerable<AutomatedOrder> orders,
			RippleWallet rw,
	    		RippleIdentifier rippleIdentifier,
		    	NetworkInterface networkInterface,
			CancellationToken token
		)
		{


			IEnumerable<IGrouping<string, AutomatedOrder>> books = 
				orders.OrderBy(ord => ord.TakerGets.GetNativeAdjustedPriceAt(ord.TakerPays)).GroupBy (
				(AutomatedOrder arg) => {

					var keys = new [] { arg.TakerGets.currency, arg.TakerPays.currency }.OrderBy ((String s) => s);
					return (string)(keys.ElementAt (0) + keys.ElementAt (1));
					},
				(AutomatedOrder arg) => arg
			);
			/*
			IEnumerable<IEnumerable<IGrouping<String, AutomatedOrder>>> markets = books.Select (
				(IGrouping<IOrderedEnumerable<string>, AutomatedOrder> arg) => 
					arg.GroupBy(
						(AutomatedOrder order) => order.TakerGets.currency,
						(AutomatedOrder order) => order)
						
			).ToArray (); */

			var markets = books
				.Select (
					(IGrouping<string, AutomatedOrder> arg) => 
						arg.GroupBy(
							(AutomatedOrder a) => a.taker_gets.currency) );


			foreach (var market in markets) {


				var side1 = market.ElementAt (0);
				var first = side1.FirstOrDefault ();

				var task = RippleLibSharp.Commands.Stipulate.BookOffers.GetResult (
					first.TakerPays,
		    			first.TakerGets,
					10,
					networkInterface,
		    			token
					);



				switch (market.Count () - 1) {
				case 0:
					break;
				case 1:
					var side2 = market.ElementAt (1);

					int x = 0, y = 0;

					foreach (var order in side1) {


						y = 0;
						foreach (var order2 in side2) {
							y++;

							if (y > x) {

								if (!order.taker_gets.currency.Equals (order2.TakerPays.currency)) {
									continue;
								}

								if (!order.TakerPays.currency.Equals (order2.TakerGets.currency)) {
									continue;
								}

								bool shouldCont;
								do {
									shouldCont = false;
									decimal price = order.TakerPays.GetNativeAdjustedPriceAt (order.TakerGets);
									//	decimal pricej = _offers [j].TakerPays.GetNativeAdjustedPriceAt (_offers [j].TakerGets);

									//	decimal cost = _offers [i].TakerPays.GetNativeAdjustedCostAt (_offers [i].TakerGets);

									decimal costj = order2.TakerPays.GetNativeAdjustedCostAt (order2.TakerGets);


									decimal spread = 1.009m;

									decimal resaleEstimate = price * spread;


									// 
									bool spreadTooSmall = resaleEstimate > costj;
									if (spreadTooSmall) {
										shouldCont = true;
										order.TakerGets /= 1.005m;
										order.TakerPays *= 1.005m;

										order2.TakerGets /= 1.005m;
										order2.TakerPays *= 1.005m;

									}

								} while (shouldCont);
							}
						}

						task.Wait ();

						var orderbook = task?.Result?.result?.offers?.Where((arg) => arg.Account == first.Account);

						if (orderbook != null && orderbook.Any ()) {
							foreach (Offer order2 in orderbook) {
								bool shouldCont;
								do {
									shouldCont = false;
									decimal price = order.TakerPays.GetNativeAdjustedPriceAt (order.TakerGets);
									//	decimal pricej = _offers [j].TakerPays.GetNativeAdjustedPriceAt (_offers [j].TakerGets);

									//	decimal cost = _offers [i].TakerPays.GetNativeAdjustedCostAt (_offers [i].TakerGets);

									decimal costj = order2.TakerPays.GetNativeAdjustedCostAt (order2.TakerGets);


									decimal spread = 1.009m;

									decimal resaleEstimate = price * spread;


									// 
									bool spreadTooSmall = resaleEstimate > costj;
									if (spreadTooSmall) {
										shouldCont = true;
										order.TakerGets /= 1.005m;
										order.TakerPays *= 1.005m;

										//order2.TakerGets /= 1.005m;
										//order2.TakerPays *= 1.005m;

									}

								} while (shouldCont);
							}

						}

						x++;
					}
					break;
				default:
					throw new OverflowException ();
				}
			}

			return _SubmitOrdersParallel (orders, rw, rippleIdentifier, networkInterface, token);
		}

		public MultipleOrdersSubmitResponse SubmitOrdersParallel (
			IEnumerable<AutomatedOrder> orders,
			RippleWallet rw,
	    		RippleIdentifier rippleIdentifier,
			NetworkInterface networkInterface,
			CancellationToken token
		)
		{

			ApplyRuleToNonProfitableOrders (orders);

			return _SubmitOrdersParallel (orders, rw, rippleIdentifier, networkInterface, token);

		}


		public MultipleOrdersSubmitResponse _SubmitOrdersParallel (
			IEnumerable<AutomatedOrder> orders, 
			RippleWallet rw, 
	    		RippleIdentifier rippleIdentifier, 
	    		NetworkInterface networkInterface, 
			CancellationToken token
		)
		{



#if DEBUG
			string method_sig = 
				nameof (SubmitOrdersParallel) + 
				DebugRippleLibSharp.left_parentheses + 
				nameof (orders) + 
				DebugRippleLibSharp.comma + 
				nameof (rw) + 
				DebugRippleLibSharp.comma + 
				nameof (networkInterface) + 
				DebugRippleLibSharp.right_parentheses;
#endif





			// TODO these are functions for splitting orders into smaller chhunks and spreading them out a bit. 
			//IEnumerable<AutomatedOrder> smallorders = ChopIntoSmaller (orders);
			//ApplyRuleToSamePrice (smallorders);
			//orders = smallorders;

			//this.orders = orders.ToArray ();

			SoundSettings settings = SoundSettings.LoadSoundSettings ();
			SoundPlayer txSubmitPlayer = null;
			SoundPlayer txFailPlayer = null;

			if (settings != null && settings.HasOnTxSubmit && settings.OnTxSubmit != null){
				txSubmitPlayer = new SoundPlayer (settings.OnTxSubmit);

				txSubmitPlayer.Load ();
			}

			if (settings != null && settings.HasOnTxFail && settings.OnTxFail != null) {


				txFailPlayer = new SoundPlayer (settings.OnTxFail);
				txFailPlayer.Load ();



			}


			List<OrderSubmittedEventArgs> events = new List<OrderSubmittedEventArgs> ();

			try {



				RippleIdentifier identifier = rippleIdentifier; //rw.GetDecryptedSeed ();


				IEnumerable<AutomatedOrder> ords = orders.ToList ();

				List<Task> taskList = new List<Task> ();
				do {

					taskList.Clear ();



					foreach (AutomatedOrder order in ords) {

						order.SubmittedEventArgs = _SubmitOrder (order, rw, networkInterface, token, identifier, true);
						if (order.SubmittedEventArgs == null) {
							MessageDialog.ShowMessage ("this should never return null !!!");


							return new MultipleOrdersSubmitResponse () {
								Message = "Order submit returned null\n",
								Succeeded = false,
								SubmitResponses = events
							};

							//return new Tuple<bool, IEnumerable<OrderSubmittedEventArgs>> (false, events);
						}

						OnOrderSubmitted?.Invoke (this, order.SubmittedEventArgs);

						if (order.SubmittedEventArgs.Unrecoverable) {
							// todo
							//OnOrderSubmitted?.Invoke (this, o
							if (settings != null && settings.HasOnTxFail && settings.OnTxFail != null) {

								Task.Run (delegate {


									txFailPlayer?.Play ();
								});

							}


							return new MultipleOrdersSubmitResponse () {
								Succeeded = false,
								SubmitResponses =events,
								Message = "Unrecoverable error",
								TroubleResponse = order?.SubmittedEventArgs
							};
						}

						

						Task task = Task.Run (
						delegate {
							AutomatedOrder automated = order;
							if (automated?.SubmittedEventArgs != null) {
								automated.SubmittedEventArgs.VerifyEvent = 
									VerifyTx (
										automated.SubmittedEventArgs.RippleOfferTransaction, 
										networkInterface, 
										token
								);
								if (automated.SubmittedEventArgs.VerifyEvent != null) {
									if (automated.SubmittedEventArgs.VerifyEvent.Success) {
										automated.IsValidated = true;

									}
									
								}
							}

						}, token);


						if (!order.SubmittedEventArgs.Success) {
							if (settings != null && settings.HasOnTxFail && settings.OnTxFail != null) {

								Task.Run (delegate {


									txFailPlayer?.Play ();
								});

							}

							task.Wait (6000);

							if (order.IsValidated) {
								continue;
							}
							break;
						} else {
							if (settings != null && settings.HasOnTxSubmit && settings.OnTxSubmit != null) {

								Task.Run (delegate {


									txSubmitPlayer?.Play ();
								});

							}

						}

						//events.Add (submitEvent);

						taskList.Add (task);


					}

					//token.WaitHandle.WaitOne (20);
					Task.WaitAll ( taskList.ToArray (), token ); // this is outside the for loop and waits on all

					ords = ords.Where ((AutomatedOrder arg) => !arg.IsValidated);

				} while (ords.Any());

				var acc = rw.GetStoredReceiveAddress ();

				Task.Run ( delegate {
					if (acc != null) {
						AccountSequenceCache accountSequenceCache = AccountSequenceCache.GetCacheForAccount (acc);
						accountSequenceCache?.Save ();
					}

				});


				return new MultipleOrdersSubmitResponse () {
					Message = "All orders succeeded\n",
					Succeeded = true,
					SubmitResponses = events
				};

				//return new Tuple<bool, IEnumerable<OrderSubmittedEventArgs>> (true, events);

			} catch (Exception e) {

#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.ReportException (method_sig, e);
				}
#endif

				StringBuilder strBuild = new StringBuilder ();
				strBuild.AppendLine ("Exception thrown : ");
				strBuild.AppendLine (e?.Message ?? "");
				strBuild.AppendLine (e?.StackTrace ?? "");

				return new MultipleOrdersSubmitResponse () {
					Message = strBuild.ToString (),
		    			
		    			Succeeded = false,
					SubmitResponses = events
				};


			}

		}



		public MultipleOrdersSubmitResponse SubmitOrders (
			IEnumerable<AutomatedOrder> orders, 
			RippleWallet rw, 
	    		RippleIdentifier rippleIdentifier, 
	    		NetworkInterface networkInterface, 
			CancellationToken token
		)
		{



#if DEBUG
			string method_sig = nameof (SubmitOrders) + DebugRippleLibSharp.left_parentheses + nameof (orders) + DebugRippleLibSharp.comma + nameof (rw) + DebugRippleLibSharp.comma + nameof (networkInterface) + DebugRippleLibSharp.right_parentheses;
#endif

			ApplyRuleToNonProfitableOrders (orders);


			// TODO these are functions for splitting orders into smaller chhunks and spreading them out a bit. 
			//IEnumerable<AutomatedOrder> smallorders = ChopIntoSmaller (orders);
			//ApplyRuleToSamePrice (smallorders);
			//orders = smallorders;

			//this.orders = orders.ToArray ();

			List<OrderSubmittedEventArgs> events = new List<OrderSubmittedEventArgs> ();

			try {



				RippleIdentifier identifier = rippleIdentifier; //rw.GetDecryptedSeed ();



				SoundSettings settings = SoundSettings.LoadSoundSettings ();
				SoundPlayer OnTxSubmitPlayer = null;
				SoundPlayer OnTxFailPlayer = null;

				if (settings != null && settings.HasOnTxSubmit && settings.OnTxSubmit != null) {

					Task.Run (delegate {

						OnTxSubmitPlayer = new SoundPlayer (settings.OnTxSubmit);
						OnTxSubmitPlayer.Load ();

					});

				}

				if (settings != null && settings.HasOnTxFail && settings.OnTxFail != null) {

					Task.Run (delegate {

						OnTxFailPlayer = new SoundPlayer (settings.OnTxFail);
						OnTxFailPlayer.Load ();

					});

				}

				foreach (AutomatedOrder order in orders) {

					OrderSubmittedEventArgs submitEvent = _SubmitOrder (order, rw, networkInterface, token, identifier, false);
					if (submitEvent == null) {
						MessageDialog.ShowMessage ("this should never return null !!!");

						return new MultipleOrdersSubmitResponse () {
							Succeeded = false,
			    				SubmitResponses = events,
							Message = "Order Submit returned null\n"
						};

						//return new Tuple<bool, IEnumerable<OrderSubmittedEventArgs>> (false, events);
					}
					events.Add (submitEvent);



					OnOrderSubmitted?.Invoke (this, submitEvent);


					if (!submitEvent.Success) {

						StringBuilder stringBuilder = new StringBuilder ();
						stringBuilder.AppendLine ("Order Submit not successful\n");
						//stringBuilder.AppendLine ();


						return new MultipleOrdersSubmitResponse () {
							Succeeded = false,
			    				SubmitResponses = events,
							Message = stringBuilder.ToString(),
			    				TroubleResponse = submitEvent

						};

						//return new Tuple<bool, IEnumerable<OrderSubmittedEventArgs>> (false, events);
					}
					
					if (settings != null && settings.HasOnTxSubmit && settings.OnTxSubmit != null) {

						Task.Run ((System.Action)OnTxSubmitPlayer.Play);

					}
				}
				var acc = rw.GetStoredReceiveAddress ();
				if (acc != null) {
					AccountSequenceCache accountSequenceCache = AccountSequenceCache.GetCacheForAccount (acc);
					accountSequenceCache?.Save ();
				}

				return new MultipleOrdersSubmitResponse () {
					Message = "All orders succeeded\n",
					Succeeded = true,
					SubmitResponses = events
				};

				//return new Tuple<bool, IEnumerable<OrderSubmittedEventArgs>> (true, events);

			} catch (Exception e) {

#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.ReportException (method_sig, e);
				}
#endif

				StringBuilder strBuild = new StringBuilder ();
				strBuild.AppendLine ("Exception thrown : ");
				strBuild.AppendLine (e.Message);
				strBuild.AppendLine (e.StackTrace);

				return new MultipleOrdersSubmitResponse () {
					Message = strBuild.ToString (),
					Succeeded = false,
					SubmitResponses = events
				};


				//return new Tuple<bool, IEnumerable<OrderSubmittedEventArgs>> (false, events);
			}

		}

		private OrderSubmittedEventArgs _SubmitOrder (
			AutomatedOrder order, 
			RippleWallet rw, 
	    		NetworkInterface networkInterface, 
	    		CancellationToken token, 
			RippleIdentifier rippleSeedAddress, 
			bool paralell
	    	)
		{

			// serial vs paralell. serial is one order at a time, verified before continuing to the next. Paralell is submitting them all then verifying the bunch
#if DEBUG
			string method_sig = clsstr + nameof (_SubmitOrder) + DebugRippleLibSharp.both_parentheses;
#endif

			if (order == null) {
				throw new ArgumentNullException ();
			}

			// object used to collect the information regarding an order submit attempt
	    		



			OrderSubmittedEventArgs orderSubmittedEventArgs = new OrderSubmittedEventArgs {
				Sequence =
				Convert.ToUInt32 (
					AccountInfo.GetSequence (
						rw.GetStoredReceiveAddress (),
						networkInterface,
						token
					)
				),

				submit_attempts = 0
			};


			UInt32? lastFee = null;


			orderSubmittedEventArgs.RippleOfferTransaction = new RippleOfferTransaction (order.Account, order);
		retry:

			if (orderSubmittedEventArgs.submit_attempts != 0) {
				MessageDialog.ShowMessage ("Retrying !!!");
			}

			if ( orderSubmittedEventArgs.submit_attempts >= MAX_SUBMIT_ATTEMPTS) {
				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = true;
				orderSubmittedEventArgs.Message = "Max submit retry attempts reached\n";
				return orderSubmittedEventArgs;
			}

	    		orderSubmittedEventArgs.signOptions = SignOptions.LoadSignOptions ();
			orderSubmittedEventArgs.feeSettings = FeeSettings.LoadSettings ();
			
			orderSubmittedEventArgs.feeSettings.OnFeeSleep += (object sender, FeeSleepEventArgs e) => {
				this.OnFeeSleep.Invoke (sender,e);
			};

			ParsedFeeAndLedgerResp ledFee = orderSubmittedEventArgs.feeSettings.GetFeeAndLastLedgerFromSettings (networkInterface, token, lastFee);

			string unable = "Unable to retrieve fee and last ledger\n";

			if (ledFee == null) {

				// todo
				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = false;
				orderSubmittedEventArgs.Message = unable + "paresed fee is null";
				return orderSubmittedEventArgs;
			}

			if (ledFee.HasError) {
				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = false;
				orderSubmittedEventArgs.Message = unable + (string)(ledFee?.ErrorMessage ?? "(null error message)");
				return orderSubmittedEventArgs;
			}

			UInt32 f = (UInt32)ledFee.Fee;
			orderSubmittedEventArgs.RippleOfferTransaction.fee = f.ToString ();

			orderSubmittedEventArgs.RippleOfferTransaction.Sequence = orderSubmittedEventArgs.Sequence; // sequence;

			uint lls = 0;
			if (orderSubmittedEventArgs.signOptions != null) {
				lls = orderSubmittedEventArgs.signOptions.LastLedgerOffset;
			}

			if (lls < 5) {
				lls = SignOptions.DEFAUL_LAST_LEDGER_SEQ;
			}

			orderSubmittedEventArgs.RippleOfferTransaction.LastLedgerSequence = (UInt32)ledFee.LastLedger + lls;

			if (orderSubmittedEventArgs.RippleOfferTransaction.fee.amount == 0) {
				// TODO robust error dealing
				orderSubmittedEventArgs.ApiRequestErrors++;
				goto retry;
			}

			if (orderSubmittedEventArgs.RippleOfferTransaction.Sequence == 0) {
				// TODO robust error dealing
				orderSubmittedEventArgs.ApiRequestErrors++;
				goto retry;
			}

			switch (orderSubmittedEventArgs.signOptions.SigningLibrary) {
			case "Rippled":
				string rpcmsg = "Signing using rpc";

#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {


					Logging.WriteLog (rpcmsg);
				}
#endif
				try {
					string signature = orderSubmittedEventArgs.RippleOfferTransaction.SignLocalRippled (rippleSeedAddress);
					if (signature == null) {
						orderSubmittedEventArgs.Unrecoverable = true;
						orderSubmittedEventArgs.Success = false;
						orderSubmittedEventArgs.Message = "Rippled returned a null signature\n";
						return orderSubmittedEventArgs;
					}
				} catch (Exception ex) {
#if DEBUG
					if (DebugIhildaWallet.OrderSubmitter) {
						Logging.ReportException (method_sig, ex);
					}


#endif

					StringBuilder stringBuilder = new StringBuilder ();

					string tites = "Error signing over rpc. Is rippled running";
					stringBuilder.AppendLine (tites);

#if DEBUG
					stringBuilder.Append (nameof (ex.Message));
					stringBuilder.AppendLine (DebugRippleLibSharp.colon);
					stringBuilder.AppendLine (ex.Message);

					stringBuilder.Append (nameof (ex.StackTrace));
					stringBuilder.AppendLine (DebugRippleLibSharp.colon);
					stringBuilder.AppendLine (ex.StackTrace);
#endif

					Logging.WriteLog (stringBuilder.ToString ());
					MessageDialog.ShowMessage (tites, stringBuilder.ToString ());

					orderSubmittedEventArgs.Success = false;
					orderSubmittedEventArgs.Unrecoverable = true;
					orderSubmittedEventArgs.Message = "Rippled threw an en error exception\n";
					orderSubmittedEventArgs.Message += ex?.Message ?? "";
					orderSubmittedEventArgs.Message += ex?.StackTrace ?? "";

					return orderSubmittedEventArgs;

				}

#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog ("Signed rpc");
				}
#endif

				break;
			case "RippleLibSharp":

#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog ("Signing using RippleLibSharp");
				}

#endif

				try {
					string signature = orderSubmittedEventArgs.RippleOfferTransaction.Sign (rippleSeedAddress);
					if (signature == null) {
						orderSubmittedEventArgs.Unrecoverable = true;
						orderSubmittedEventArgs.Success = false;
						orderSubmittedEventArgs.Message = "RippleLibSharp returned null signature\n";
						return orderSubmittedEventArgs;
					}

				} catch (Exception e) {

#if DEBUG
					if (DebugIhildaWallet.OrderSubmitter) {
						Logging.ReportException (method_sig, e);
					}
#endif


					orderSubmittedEventArgs.Unrecoverable = true;
					orderSubmittedEventArgs.Success = false;
					orderSubmittedEventArgs.Message = "RippleLibSharp threw an error exception\n";
					orderSubmittedEventArgs.Message += e?.Message ?? "";
					orderSubmittedEventArgs.Message += e?.StackTrace ?? "";
					return orderSubmittedEventArgs;
				}
#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog ("Signed RippleLibSharp");
				}
#endif

				break;
			case "RippleDotNet":
#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog ("Signing using RippleDotNet");
				}

#endif

				try {
					string signature = orderSubmittedEventArgs.RippleOfferTransaction.SignRippleDotNet (rippleSeedAddress);
					if (signature == null) {
						orderSubmittedEventArgs.Unrecoverable = true;
						orderSubmittedEventArgs.Success = false;
						orderSubmittedEventArgs.Message = "Ripple-Dot-Net returned null signature\n";
						return orderSubmittedEventArgs;
					}
		    		

				} catch (Exception e) {

#if DEBUG
					if (DebugIhildaWallet.OrderSubmitter) {
						Logging.ReportException (method_sig, e);
					}
#endif


					orderSubmittedEventArgs.Unrecoverable = true;
					orderSubmittedEventArgs.Success = false;
					orderSubmittedEventArgs.Message = "Ripple-Dot-Net threw an error exception\n";
					orderSubmittedEventArgs.Message += e?.Message ?? "";
					orderSubmittedEventArgs.Message += e?.StackTrace ?? "";
					
					return orderSubmittedEventArgs;
				}
#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog ("Signed RippleDotNet");
				}
#endif

				break;
			default:
				throw new NotSupportedException ("Invalid sign option " + orderSubmittedEventArgs.signOptions.SigningLibrary);
			}

			Task<Response<RippleSubmitTxResult>> task = null; // order submit task
			Task<VerifyEventArgs> verifyTask = null; // verify task
			try {
				orderSubmittedEventArgs.submit_attempts++;
				task = NetworkController.UiTxNetworkSubmit (
					orderSubmittedEventArgs.RippleOfferTransaction,
					networkInterface,
					token
				);

#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog ("Submitted via websocket");
				}
#endif


				if (task == null) {
					MessageDialog.ShowMessage ("Error", "task == null");
					goto retry;
				}


				int maxseconds = 60 * 3; // 3 minutes max wait

				int faults = 0;
				for (int seconds = 0; task != null && !task.IsCanceled && !task.IsCompleted && !task.IsFaulted && !token.IsCancellationRequested && seconds < maxseconds; seconds++) {
					task.Wait (1000, token);

					if (seconds == 30) {  // after 30 seconds check to see what's going on by starting a verification task
						verifyTask = Task<VerifyEventArgs>.Run (delegate {

							return VerifyTx (orderSubmittedEventArgs.RippleOfferTransaction, networkInterface, token);
						});
					}

					if (verifyTask != null) {

						if (verifyTask.IsCanceled) {
							orderSubmittedEventArgs.Unrecoverable = true;
							orderSubmittedEventArgs.Success = false;
							orderSubmittedEventArgs.Message = "Order submit cancelled\n";
							return orderSubmittedEventArgs;
						}

						if (verifyTask.IsCompleted) {



							var v = verifyTask.Result;
							orderSubmittedEventArgs.Success = v.Success;

							orderSubmittedEventArgs.Message =
								v.Success ?
								"Transaction validated\n" :
				    				"Unable to validate transaction\n";

							return orderSubmittedEventArgs;
						}

						if (verifyTask.IsFaulted) {
							// if verification fails we will keep starting a new one

							if (faults < 2) {


								verifyTask = Task<VerifyEventArgs>.Run (delegate {

									// this returns the subthread
									return VerifyTx (orderSubmittedEventArgs.RippleOfferTransaction, networkInterface, token);
								});
							} else {
								// It failed three times ?? Something must be wrong. I.E no internet?
								// TODO

								break;
							}
						}

					}



				}

			} catch (NullReferenceException nullEx) {

#if DEBUG
				Logging.ReportException (method_sig, nullEx);
#endif
				orderSubmittedEventArgs.Unrecoverable = false;
				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Message = nameof (NullReferenceException);
				orderSubmittedEventArgs.Message += " : \n";
				orderSubmittedEventArgs.Message = nullEx?.Message ?? "";
				orderSubmittedEventArgs.Message = nullEx?.StackTrace ?? "";

				return orderSubmittedEventArgs;

			} catch (Exception e) {

#if DEBUG
				Logging.ReportException (method_sig, e);
				Logging.WriteLog (e.Message);

#endif

				StringBuilder stringBuilder = new StringBuilder ();

				stringBuilder.AppendLine ("Exception thrown : ");
				stringBuilder.AppendLine (e?.Message ?? "");
				stringBuilder.AppendLine (e?.StackTrace ?? "");


				//this.SetResult (index.ToString (), "Network Error", TextHighlighter.RED);
				if (!paralell) {
					// serial mode is real slow and steady
					VerifyEventArgs verifyResul = VerifyTx (orderSubmittedEventArgs.RippleOfferTransaction, networkInterface, token);
					if (verifyResul != null) {

						if (verifyResul.Success) {
							orderSubmittedEventArgs.Success = true;
							orderSubmittedEventArgs.Unrecoverable = true;
							orderSubmittedEventArgs.Message = stringBuilder.ToString ();
							return orderSubmittedEventArgs;
						}
					} else {
						// TODO
					}


					token.WaitHandle.WaitOne (FAILED_ATTEMPT_RETRY_DELAY);
					//Thread.Sleep (FAILED_ATTEMPT_RETRY_DELAY);
					goto retry;
				} else {

					// this is recoverable. Probably means no network. Maybe we can discover, throw, catch network errors ect
					orderSubmittedEventArgs.Success = false;
					orderSubmittedEventArgs.Unrecoverable = true;
					orderSubmittedEventArgs.Message = stringBuilder.ToString ();
					return orderSubmittedEventArgs;
				}

			}

			orderSubmittedEventArgs.response = task?.Result;

			if (orderSubmittedEventArgs.response == null) {

				string warningMessage = "response == null";



#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog ("Error submitting transaction : " + warningMessage);
					Logging.WriteLog (orderSubmittedEventArgs.RippleOfferTransaction.ToJson ());
				}
#endif


				if (!paralell) {
					VerifyEventArgs verifyR = VerifyTx (orderSubmittedEventArgs.RippleOfferTransaction, networkInterface, token);
					if (verifyR.Success) {
						orderSubmittedEventArgs.Success = true;
						return orderSubmittedEventArgs;

					}

					token.WaitHandle.WaitOne (FAILED_ATTEMPT_RETRY_DELAY);
					//Thread.Sleep ();
					goto retry;
				} else {


					orderSubmittedEventArgs.Success = false;
					orderSubmittedEventArgs.Unrecoverable = false;
					return orderSubmittedEventArgs;
				}
			}

			if (orderSubmittedEventArgs.response.HasError ()) {  // this is why we use return an events arg, it contains all the relevent info including errors

				StringBuilder sb = new StringBuilder ();
				sb.Append ("Error response : ");
				sb.Append (orderSubmittedEventArgs.response?.error_message ?? "");

#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog ("Error submitting transaction ");
					Logging.WriteLog (orderSubmittedEventArgs.RippleOfferTransaction.ToJson ());
					Logging.WriteLog (sb.ToString ());
				}
#endif
				if (!paralell) {
					VerifyEventArgs verifyR = VerifyTx (orderSubmittedEventArgs.RippleOfferTransaction, networkInterface, token);
					if (verifyR.Success) {
						orderSubmittedEventArgs.Success = true;
						return orderSubmittedEventArgs;
					}

					token.WaitHandle.WaitOne (FAILED_ATTEMPT_RETRY_DELAY);
					//Thread.Sleep ();
					goto retry;
				} else {
					orderSubmittedEventArgs.Success = false;
					return orderSubmittedEventArgs;
				}

			}


			RippleSubmitTxResult res = orderSubmittedEventArgs.response?.result;

			if (res == null) {

#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog ("res == null, Bug?\n");
				}
#endif

				string msg = "Order submit returned null\n";

				if (!paralell) {
					VerifyEventArgs verifyR = VerifyTx (orderSubmittedEventArgs.RippleOfferTransaction, networkInterface, token);
					if (verifyR.Success) {
						orderSubmittedEventArgs.Success = true;
						orderSubmittedEventArgs.Message = msg;
						return orderSubmittedEventArgs;
					}

					token.WaitHandle.WaitOne (FAILED_ATTEMPT_RETRY_DELAY);
					//Thread.Sleep ();
					goto retry;
				}

				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Message = msg;
				return orderSubmittedEventArgs;
			}

			string een = res?.engine_result;

			if (een == null) {
				string msg = "engine_result null\n";


#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog (msg);
				}
#endif

				if (!paralell) {
					VerifyEventArgs verifyR = VerifyTx (orderSubmittedEventArgs.RippleOfferTransaction, networkInterface, token);
					if (verifyR.Success) {
						
						orderSubmittedEventArgs.Success = true;
						orderSubmittedEventArgs.Message = msg;
						return orderSubmittedEventArgs;
					}

					token.WaitHandle.WaitOne (FAILED_ATTEMPT_RETRY_DELAY);
					//Thread.Sleep ();
					goto retry; 
				}

				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Message = msg;

				return orderSubmittedEventArgs;
			}

			Ter ter;

			try {
				ter = (Ter)Enum.Parse (typeof (Ter), een, true);
				//ter = (Ter)Ter.Parse (typeof(Ter), een, true);

			} catch (ArgumentNullException exc) {
				StringBuilder stringBuilder = new StringBuilder ();

				stringBuilder.Append (nameof (ArgumentNullException));
				stringBuilder.AppendLine (" : ");
				stringBuilder.Append (exc?.Message ?? "");
				stringBuilder.Append (exc?.StackTrace ?? "");
#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.ReportException (method_sig, exc);
					Logging.WriteLog (stringBuilder.ToString ());
				}
#endif

				if (!paralell) {
					var verRes = VerifyTx (orderSubmittedEventArgs.RippleOfferTransaction, networkInterface, token);
					if (verRes.Success) {
						orderSubmittedEventArgs.Success = true;
						orderSubmittedEventArgs.Message = stringBuilder.ToString ();
						return orderSubmittedEventArgs;
					}

					token.WaitHandle.WaitOne (FAILED_ATTEMPT_RETRY_DELAY);
					//Thread.Sleep ();
					goto retry;
				}

				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = false;
				orderSubmittedEventArgs.Message = stringBuilder.ToString ();
				return orderSubmittedEventArgs;

			} catch (OverflowException overFlowException) {
#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.ReportException (method_sig, overFlowException);
				}

#endif

				StringBuilder stringBuilder = new StringBuilder ();
				stringBuilder.Append ("Overflow Exception\n");
				Logging.WriteLog (stringBuilder.ToString());

				stringBuilder.AppendLine (overFlowException?.Message ?? "");
				stringBuilder.AppendLine (overFlowException?.StackTrace ?? "");

				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = true;
				orderSubmittedEventArgs.Message = stringBuilder.ToString ();
				return orderSubmittedEventArgs;
			} catch (ArgumentException argumentException) {

				StringBuilder stringBuilder = new StringBuilder ();
				stringBuilder.Append ("Argument Exception");

#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog (stringBuilder.ToString ());
					Logging.ReportException (method_sig, argumentException);
				}
#endif

				stringBuilder.Append (argumentException?.Message ?? "");

				stringBuilder.Append (argumentException?.StackTrace ?? "");

				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = false;
				orderSubmittedEventArgs.Message = stringBuilder.ToString ();
				return orderSubmittedEventArgs;
			} catch (Exception e) {

				StringBuilder stringBuilder = new StringBuilder ();
				stringBuilder.Append ("Unknown Exception");
#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog (stringBuilder.ToString ());
					Logging.ReportException (method_sig, e);
				}
#endif

				stringBuilder.AppendLine (e?.Message ?? "");
				stringBuilder.AppendLine (e?.StackTrace ?? "");
				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = true;
				orderSubmittedEventArgs.Message = stringBuilder.ToString ();
				return orderSubmittedEventArgs;
			}

			StringBuilder messageBuilder = new StringBuilder ();
			messageBuilder.AppendLine (res.engine_result);
			messageBuilder.AppendLine (res.engine_result_message);

			orderSubmittedEventArgs.Message = messageBuilder.ToString ();

			VerifyEventArgs verifyResult = null;
			switch (ter) {

			case Ter.tefALREADY:

				LogResult (res.engine_result, res.engine_result_message);

				// TODO verify ?

				//token.WaitHandle.WaitOne (FAILED_ATTEMPT_RETRY_DELAY);
				//Thread.Sleep (FAILED_ATTEMPT_RETRY_DELAY);

				// not actually a failure so we are continuing 
				orderSubmittedEventArgs.Success = true;
				orderSubmittedEventArgs.Unrecoverable = false;

				return orderSubmittedEventArgs;

			case Ter.terQUEUED:

				token.WaitHandle.WaitOne (1000);
				//Thread.Sleep (1000);

				LogResult (res.engine_result, res.engine_result_message);

				if (!paralell) {

					verifyResult = VerifyTx (orderSubmittedEventArgs.RippleOfferTransaction, networkInterface, token);
					if (verifyResult.Success) {
						orderSubmittedEventArgs.Success = true;
						orderSubmittedEventArgs.Unrecoverable = false;
						return orderSubmittedEventArgs;
					}

					token.WaitHandle.WaitOne (FAILED_ATTEMPT_RETRY_DELAY);
					//Thread.Sleep (FAILED_ATTEMPT_RETRY_DELAY);
					goto retry;
				}

				orderSubmittedEventArgs.Success = true;
				orderSubmittedEventArgs.Unrecoverable = false;
				return orderSubmittedEventArgs;


			case Ter.tesSUCCESS:
				if (!paralell) {
					LogResult (res.engine_result, res.engine_result_message);

					verifyResult = VerifyTx (orderSubmittedEventArgs.RippleOfferTransaction, networkInterface, token);
					if (verifyResult.Success) {
						orderSubmittedEventArgs.Success = true;
						orderSubmittedEventArgs.Unrecoverable = false;
						return orderSubmittedEventArgs;
					}

					token.WaitHandle.WaitOne (FAILED_ATTEMPT_RETRY_DELAY);
					//Thread.Sleep (FAILED_ATTEMPT_RETRY_DELAY);
					goto retry;
				}
				orderSubmittedEventArgs.Success = true;
				orderSubmittedEventArgs.Unrecoverable = false;
				return orderSubmittedEventArgs;

			case Ter.terPRE_SEQ:
			case Ter.tefPAST_SEQ:
			case Ter.tefMAX_LEDGER:
				if (!paralell) {
					LogResult (res.engine_result, res.engine_result_message);
					Thread.Sleep (FAILED_ATTEMPT_RETRY_DELAY);

					verifyResult = VerifyTx (orderSubmittedEventArgs.RippleOfferTransaction, networkInterface, token);
					if (verifyResult.Success) {
						orderSubmittedEventArgs.Success = true;
						orderSubmittedEventArgs.Unrecoverable = false;
						return orderSubmittedEventArgs;
					}
					goto retry;
				}

				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = false;
				return orderSubmittedEventArgs;

			case Ter.terRETRY:
			case Ter.telCAN_NOT_QUEUE:

			case Ter.telCAN_NOT_QUEUE_FULL:

				if (!paralell) {
					LogResult (res.engine_result, res.engine_result_message);
					Thread.Sleep (FAILED_ATTEMPT_RETRY_DELAY);
					verifyResult = VerifyTx (orderSubmittedEventArgs.RippleOfferTransaction, networkInterface, token);
					if (verifyResult.Success) {
						orderSubmittedEventArgs.Success = true;
						orderSubmittedEventArgs.Unrecoverable = false;
						return orderSubmittedEventArgs;
					}
					goto retry;
				}
				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = false;
				return orderSubmittedEventArgs;

			case Ter.telCAN_NOT_QUEUE_BALANCE:
			case Ter.telCAN_NOT_QUEUE_FEE:
			case Ter.telINSUF_FEE_P:
			case Ter.terFUNDS_SPENT:

				if (lastFee != null) {
					orderSubmittedEventArgs.Success = false;
					return orderSubmittedEventArgs;

				}

				lastFee = (UInt32)orderSubmittedEventArgs.RippleOfferTransaction.fee.amount;


				if (!paralell) {

					token.WaitHandle.WaitOne (FAILED_ATTEMPT_RETRY_DELAY);
					//Thread.Sleep (FAILED_ATTEMPT_RETRY_DELAY);

					LogResult (res.engine_result + " retrying", res.engine_result_message);

					verifyResult = VerifyTx (orderSubmittedEventArgs.RippleOfferTransaction, networkInterface, token);
					if (verifyResult.Success) {
						orderSubmittedEventArgs.Success = true;
						return orderSubmittedEventArgs;
					}
					goto retry;
				}

				


				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = false;
				return orderSubmittedEventArgs;

			case Ter.temBAD_AMOUNT:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.Unrecoverable = true;
				orderSubmittedEventArgs.Success = false;
				return orderSubmittedEventArgs;

			case Ter.tecNO_ISSUER:
			case Ter.temBAD_ISSUER:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.Unrecoverable = true;
				orderSubmittedEventArgs.Success = false;
				return orderSubmittedEventArgs;

			case Ter.tecUNFUNDED_OFFER:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = true;
				return orderSubmittedEventArgs;

			case Ter.tecUNFUNDED:
			case Ter.tecINSUF_RESERVE_OFFER:
			case Ter.tecINSUF_RESERVE_LINE:
			case Ter.tecINSUFFICIENT_RESERVE:
			case Ter.tecNO_LINE_INSUF_RESERVE:


				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = true;
				return orderSubmittedEventArgs;

			case Ter.temBAD_AUTH_MASTER:
			case Ter.tefBAD_AUTH_MASTER:
			case Ter.tefMASTER_DISABLED:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = true;
				return orderSubmittedEventArgs;


			case Ter.terNO_ACCOUNT:

				LogResult (res.engine_result, res.engine_result_message);

				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = true;
				return orderSubmittedEventArgs;

			case Ter.tecNO_AUTH: // Not authorized to hold IOUs.
			case Ter.tecNO_LINE:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = true;
				return orderSubmittedEventArgs;

			case Ter.tecFROZEN:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = true;
				return orderSubmittedEventArgs;

			case Ter.tefFAILURE:
				// TODO what to do?

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = true;
				return orderSubmittedEventArgs;

			case Ter.temBAD_FEE:
			case Ter.temMALFORMED:
			case Ter.temINVALID:
			case Ter.temBAD_SIGNATURE:
			case Ter.temBAD_PATH:
			case Ter.temBAD_PATH_LOOP:
			case Ter.temBAD_SEQUENCE:
			case Ter.temBAD_SRC_ACCOUNT:
			case Ter.temDST_IS_SRC:
			case Ter.temDST_NEEDED:
			case Ter.temREDUNDANT:
			case Ter.temRIPPLE_EMPTY:
			case Ter.temDISABLED:
			case Ter.tecOWNERS:
			case Ter.tecINVARIANT_FAILED:

				LogResult (res.engine_result, res.engine_result_message);

				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = true;
				return orderSubmittedEventArgs;

			case Ter.tecPATH_DRY:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = true;
				return orderSubmittedEventArgs;

			case Ter.tecPATH_PARTIAL:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = true;
				return orderSubmittedEventArgs;

			case Ter.tecOVERSIZE:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = true;
				return orderSubmittedEventArgs;

			case Ter.tefINTERNAL:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = true;
				return orderSubmittedEventArgs;

			case Ter.tefEXCEPTION:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = true;
				return orderSubmittedEventArgs;

			case Ter.tefBAD_LEDGER:
				// report bug to ripple labs

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = true;
				return orderSubmittedEventArgs;

			case Ter.tecDIR_FULL:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = true;
				return orderSubmittedEventArgs;

			case Ter.tecCLAIM:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = true;
				return orderSubmittedEventArgs;

			case Ter.tecEXPIRED:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = true;
				return orderSubmittedEventArgs;

			default:
				Logging.WriteLog (res.engine_result + " : response not implemented");
				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.Success = false;
				orderSubmittedEventArgs.Unrecoverable = true;
				return orderSubmittedEventArgs;

			}




		}

		private IEnumerable<AutomatedOrder> ChopIntoSmaller (IEnumerable<AutomatedOrder> orders)
		{
			List<AutomatedOrder> newOrders = new List<AutomatedOrder> ();

			foreach (AutomatedOrder automatedOrder in orders) {

				if ("BTC".Equals (automatedOrder?.TakerGets?.currency?.ToUpper ())) {
					if (automatedOrder.TakerGets.amount > 0.02m) {
						AutomatedOrder [] two = automatedOrder.Split (2);

						newOrders.AddRange (two);
						continue;
					}


				}

				if ("BTC".Equals (automatedOrder?.TakerPays?.currency?.ToUpper ())) {
					if (automatedOrder.TakerPays.amount > 0.02m) {
						AutomatedOrder [] two = automatedOrder.Split (2);
						newOrders.AddRange (two);
						continue;

					}




				}

				newOrders.Add (automatedOrder);

			}
			return newOrders;
		}

		private void ApplyRuleToSamePrice (IEnumerable<AutomatedOrder> orders)
		{
			AutomatedOrder [] _offers = orders.ToArray ();
			for (int i = 0; i < _offers.Count (); i++) {

				for (int j = i + 1; j < _offers.Count (); j++) {


					//bool loop = true;
					//do {
					decimal price = _offers [i].TakerPays.GetNativeAdjustedPriceAt (_offers [i].TakerGets);
					decimal pricej = _offers [j].TakerPays.GetNativeAdjustedPriceAt (_offers [j].TakerGets);

					decimal margin = price * 0.0001m;

					if ((Math.Abs (price - pricej)) < margin) {

						_offers [i].TakerGets.amount /= 1.00034567m;
						_offers [i].TakerPays.amount *= 1.00034567m;

					}




					//} while (loop);

				}

			}
		}

		private void ApplyRuleToNonProfitableOrders (IEnumerable<AutomatedOrder> orders)
		{



			AutomatedOrder [] _offers = orders.ToArray ();


			for (int i = 0; i < _offers.Length; i++) {

				for (int j = i + 1; j < _offers.Length; j++) {

					if (!_offers [i].taker_gets.currency.Equals (_offers [j].TakerPays.currency)) {
						continue;
					}

					if (!_offers [i].TakerPays.currency.Equals (_offers [j].TakerGets.currency)) {
						continue;
					}

					bool shouldCont;
					do {
						shouldCont = false;
						decimal price = _offers [i].TakerPays.GetNativeAdjustedPriceAt (_offers [i].TakerGets);
					//	decimal pricej = _offers [j].TakerPays.GetNativeAdjustedPriceAt (_offers [j].TakerGets);

					//	decimal cost = _offers [i].TakerPays.GetNativeAdjustedCostAt (_offers [i].TakerGets);

						decimal costj = _offers [j].TakerPays.GetNativeAdjustedCostAt (_offers [j].TakerGets);


						decimal spread = 1.006m;

						decimal resaleEstimate = price * spread;


						// 
						bool spreadTooSmall = resaleEstimate > costj;
						if (spreadTooSmall) {
							shouldCont = true;
							_offers [i].TakerGets /= 1.005m;
							_offers [i].TakerPays *= 1.005m;

							_offers [j].TakerGets /= 1.005m;
							_offers [j].TakerPays *= 1.005m;

						}

					} while (shouldCont);
				}

			}


		}

		private VerifyEventArgs VerifyTx ( RippleOfferTransaction offerTransaction, NetworkInterface networkInterface, CancellationToken token )
		{

			if (offerTransaction == null) {
				throw new ArgumentNullException (nameof (offerTransaction));
			}
			VerifyEventArgs verifyEventArgs = _VerifyTx (offerTransaction, networkInterface, token);

			OnVerifyingTxReturn?.Invoke (this, verifyEventArgs);

			AutomatedOrder ao = AutomatedOrder.ReconsctructFromTransaction (offerTransaction);
			AccountSequenceCache sequenceCache = AccountSequenceCache.GetCacheForAccount (offerTransaction.Account);
			sequenceCache.UpdateOrdersCache (ao);
			//sequenceCache.UpdateAndSave (ao);
			return verifyEventArgs;
		}

		private VerifyEventArgs _VerifyTx (RippleOfferTransaction offerTransaction, NetworkInterface networkInterface, CancellationToken token)
		{


			VerifyEventArgs verifyEventArgs = new VerifyEventArgs {
				Success = false,
				RippleOfferTransaction = offerTransaction
			};

			if (offerTransaction == null) {
				verifyEventArgs.Message = "offerTransaction == null\n";
				return verifyEventArgs;
			}

			if (offerTransaction.hash == null) {

				throw new ArgumentNullException (nameof(offerTransaction), "Hash MUST be set to sign transactions!!");

				//return verifyEventArgs;
			}

			token.WaitHandle.WaitOne (1000);
			//Thread.Sleep (1000);

			Logging.WriteLog ("Validating Tx\n");
			//Thread.Sleep (2000);
			token.WaitHandle.WaitOne (1000);

			OnVerifyingTxBegin?.Invoke (this, verifyEventArgs);

			for (int i = 0; i < 100; i++) {

				//token.WaitHandle.WaitOne (3000);

				WaitHandle.WaitAny ( new WaitHandle[] { token.WaitHandle, LedgerTracker.LedgerResetEvent }, 4000 );
				//Thread.Sleep (3000);



				Task<Response<RippleTransaction>> task = tx.GetRequest (
					offerTransaction.hash, 
					networkInterface, 
		    			token
		    		);


				Task<uint?> ledgerTask = Task.Run (
					delegate {

						for (int attempt = 0; attempt < 5; attempt++) {
							uint? led = LedgerTracker.GetRecentLedgerOrNull ();

							if (led == null) {

								FeeAndLastLedgerResponse feeResp = ServerInfo.GetFeeAndLedgerSequence (networkInterface, token);
								led = feeResp?.LastLedger;


							}
							if (led != null) {
								return led;
							}
						}

						return null;
					}
				);


				if (task == null) {
					// TODO Debug
					string msg = "Error : task == null\n";
					Logging.WriteLog (msg);
					MessageDialog.ShowMessage (msg);
					verifyEventArgs.Message = msg;

					OnVerifyTxMessage?.Invoke (this, verifyEventArgs);
					//Thread.Sleep (3000);
					goto End;
				}

				task.Wait (1000 * 60 * 2, token);

				//if () {

				//}

				Response<RippleTransaction> response = task.Result;
				if (response == null) {

					verifyEventArgs.Message = "Error : response == null\n";
					Logging.WriteLog (verifyEventArgs.Message);
					OnVerifyTxMessage?.Invoke (this, verifyEventArgs);
					goto End;
				}
				RippleTransaction transaction = response.result;

				if (transaction == null) {
					verifyEventArgs.Message = "Error : transaction == null\n";
					Logging.WriteLog (verifyEventArgs.Message);
					OnVerifyTxMessage?.Invoke (this, verifyEventArgs);
					goto End;
				}

				if (transaction.validated != null && (bool)transaction.validated) {


					verifyEventArgs.Message = "Validated\n";
					Logging.WriteLog (verifyEventArgs.Message);
					verifyEventArgs.Success = true;
					//OnVerifyTxMessage?.Invoke (this, verifyEventArgs);
					return verifyEventArgs;

				}

			End:

				if (ledgerTask != null) {
					ledgerTask.Wait (1000 * 60 * 2, token);
				}


				//
				uint? ledger = ledgerTask?.Result;
				
				if ( ledger != null && ledger > offerTransaction.LastLedgerSequence) {

					verifyEventArgs.Message = "failed to validate before LastLedgerSequence exceeded\n";
					Logging.WriteLog (verifyEventArgs.Message);
					OnVerifyTxMessage?.Invoke (this, verifyEventArgs);
					return verifyEventArgs;
				}


				verifyEventArgs.Message = "Tx " + (string)(offerTransaction?.hash ?? "(null hash)") + " not validated yet\n";
				Logging.WriteLog (verifyEventArgs?.Message);

			}


			Logging.WriteLog ("Tx " + (string)(offerTransaction?.hash ?? "(null hash)") + " Max validation attempts exceeded\n");
			return verifyEventArgs;

		}


		private void LogResult (string result, string message)
		{
			Logging.WriteLog ("Result = " + result);
			Logging.WriteLog ("Message = " + message);
		}

		public static int MAX_SUBMIT_ATTEMPTS = 3;
		public static int FAILED_ATTEMPT_RETRY_DELAY = 6000;

		public event EventHandler<FeeSleepEventArgs> OnFeeSleep;

		public event EventHandler<OrderSubmittedEventArgs> OnOrderSubmitted;

		public event EventHandler<VerifyEventArgs> OnVerifyingTxBegin;

		public event EventHandler<VerifyEventArgs> OnVerifyTxMessage;

		public event EventHandler<VerifyEventArgs> OnVerifyingTxReturn;

		//public delegate void ThresholdReached (object sender, OrderSubmittedEventArgs orderSubmittedEventArgs);
#if DEBUG
		const string clsstr = nameof (OrderSubmittedEventArgs) + DebugRippleLibSharp.colon;
#endif

	}

	public class MultipleOrdersSubmitResponse
	{
		public string Message {
			get;
			set;
		}

		public bool Succeeded {
			get;
			set;
		}

		public IEnumerable<OrderSubmittedEventArgs> SubmitResponses {
			get;
			set;
		}

		public OrderSubmittedEventArgs TroubleResponse {
			get;
			set;
		}


	}

	public class VerifyEventArgs : EventArgs
	{

		public bool Success {
			get;
			set;
		}

		public RippleOfferTransaction RippleOfferTransaction {
			get;
			set;
		}

		public string Message {
			get;
			set;
		}
	}


	public class OrderSubmittedEventArgs : EventArgs
	{


		public string Message {
			get;
			set;
		}
		public bool Success {
			get;
			set;
		}

		public uint Sequence {
			get;
			set;
		}

		public int submit_attempts {
			get;
			set;
		}

		public int ApiRequestErrors {
			get;
			set;
		}

		public bool Unrecoverable {
			get;
			set;
		}

		public VerifyEventArgs VerifyEvent {
			get;
			set;
		}

		public RippleOfferTransaction RippleOfferTransaction {
			get;
			set;
		}

		public AutomatedOrder AutomatedOrderObj {
			get;
			set;
		}


		public SignOptions signOptions {
			get;
			set;
		}

		public FeeSettings feeSettings {
			get;
			set;
		}

		public Response<RippleSubmitTxResult> response {
			get;
			set;
		}


	}

	/*
	public class TxSubmittedEventArgs : EventArgs
	{

	}
	*/
}
