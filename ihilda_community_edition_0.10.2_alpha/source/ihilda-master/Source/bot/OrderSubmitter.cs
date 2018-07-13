using System;
using System.Linq;
using System.Collections.Generic;
using RippleLibSharp.Keys;
using RippleLibSharp.Network;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Transactions;
using RippleLibSharp.Transactions.TxTypes;
using RippleLibSharp.Util;
using System.Threading.Tasks;
using RippleLibSharp.Result;
using IhildaWallet.Networking;
using System.Text;
using System.Threading;
using RippleLibSharp.Commands.Server;
using RippleLibSharp.Commands.Tx;

namespace IhildaWallet
{
	public class OrderSubmitter
	{


		public Tuple<bool, IEnumerable<OrderSubmittedEventArgs>> SubmitOrders (IEnumerable<AutomatedOrder> orders, RippleWallet rw, NetworkInterface networkInterface)
		{



#if DEBUG
			string method_sig = nameof (SubmitOrders) + DebugRippleLibSharp.left_parentheses + nameof (orders) + DebugRippleLibSharp.comma + nameof (rw) + DebugRippleLibSharp.comma + nameof (networkInterface) + DebugRippleLibSharp.right_parentheses;
#endif

			ApplyRuleToNonProfitableOrders (orders);


			// TODO these are functions for splitting orders into smaller chhunks and spreading them out a bit. 
			//IEnumerable<AutomatedOrder> smallorders = ChopIntoSmaller (orders);
			//ApplyRuleToSamePrice (smallorders);
			//orders = smallorders;


			List<OrderSubmittedEventArgs> events = new List<OrderSubmittedEventArgs> ();

			try {



				RippleSeedAddress rsa = rw.GetDecryptedSeed ();






				foreach (AutomatedOrder order in orders) {

					OrderSubmittedEventArgs submitEvent = _SubmitOrder (order, rw, networkInterface, rsa);

					events.Add (submitEvent);

					OrderSubmitted?.Invoke (this, submitEvent);


					if (!submitEvent.success) {
						return new Tuple<bool, IEnumerable<OrderSubmittedEventArgs>> (false, events);
					}
				}


				return new Tuple<bool, IEnumerable<OrderSubmittedEventArgs>> (true, events);

			} catch (Exception e) {

#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.ReportException (method_sig, e);
				}
#endif

				return new Tuple<bool, IEnumerable<OrderSubmittedEventArgs>> (false, events);
			}

		}

		private OrderSubmittedEventArgs _SubmitOrder (AutomatedOrder order, RippleWallet rw, NetworkInterface networkInterface, RippleSeedAddress rippleSeedAddress)
		{
#if DEBUG
			string method_sig = clsstr + nameof (_SubmitOrder) + DebugRippleLibSharp.both_parentheses;
#endif



			OrderSubmittedEventArgs orderSubmittedEventArgs = new OrderSubmittedEventArgs ();
			uint sequence = Convert.ToUInt32 (AccountInfo.GetSequence (rw.GetStoredReceiveAddress (), networkInterface));
			int submit_attempt = 0;
			UInt32? lastFee = null;
			SignOptions opts = null;

			orderSubmittedEventArgs.rippleOfferTransaction = new RippleOfferTransaction (order.Account, order);
		retry:

			if (submit_attempt != 0) {
				MessageDialog.ShowMessage ("Retrying !!!");
			}

			if (submit_attempt >= MAX_SUBMIT_ATTEMPTS) {
				orderSubmittedEventArgs.success = false;

				return orderSubmittedEventArgs;
			}


			opts = SignOptions.LoadSignOptions ();

			Tuple<UInt32, UInt32> tupe = FeeSettings.GetFeeAndLastLedgerFromSettings (networkInterface, lastFee);

			UInt32 f = tupe.Item1;
			orderSubmittedEventArgs.rippleOfferTransaction.fee = f.ToString ();

			orderSubmittedEventArgs.rippleOfferTransaction.Sequence = sequence;

			uint lls = 0;
			if (opts != null) {
				lls = opts.LastLedgerOffset;
			}

			if (lls < 5) {
				lls = SignOptions.DEFAUL_LAST_LEDGER_SEQ;
			}

			orderSubmittedEventArgs.rippleOfferTransaction.LastLedgerSequence = tupe.Item2 + lls;

			if (orderSubmittedEventArgs.rippleOfferTransaction.fee.amount == 0) {
				// TODO robust error dealing
			}

			if (orderSubmittedEventArgs.rippleOfferTransaction.Sequence == 0) {
				// TODO robust error dealing
			}

			if (opts.UseLocalRippledRPC) {

#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog ("Signing using rpc");
				}
#endif
				try {
					orderSubmittedEventArgs.rippleOfferTransaction.SignLocalRippled (rippleSeedAddress);
				} catch (Exception ex) {
#if DEBUG
					if (DebugIhildaWallet.OrderSubmitter) {
						Logging.ReportException (method_sig, ex);
					}
#endif
					string message = "Error signing over rpc. Is rippled running";
					Logging.WriteLog (message);
					MessageDialog.ShowMessage ("Error", message);

				}

#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog ("Signed rpc");
				}
#endif
			} else {

#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog ("Signing using RippleLibSharp");
				}

#endif
				orderSubmittedEventArgs.rippleOfferTransaction.Sign (rippleSeedAddress);

#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog ("Signed RippleLibSharp");
				}
#endif

			}

			Task<Response<RippleSubmitTxResult>> task = null;

			try {
				submit_attempt++;
				task = NetworkController.UiTxNetworkSubmit (orderSubmittedEventArgs.rippleOfferTransaction, networkInterface);

#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog ("Submitted via websocket");
				}
#endif


				if (task == null) {
					MessageDialog.ShowMessage ("Error", "task == null");
					goto retry;
				}

				task.Wait ();


			} catch (Exception e) {

#if DEBUG
				Logging.ReportException (method_sig, e);
				Logging.WriteLog (e.Message);

#endif

				//this.SetResult (index.ToString (), "Network Error", TextHighlighter.RED);
				if (VerifyTx (orderSubmittedEventArgs.rippleOfferTransaction, networkInterface)) {
					orderSubmittedEventArgs.success = true;
					return orderSubmittedEventArgs;
				}
				Thread.Sleep (FAILED_ATTEMPT_RETRY_DELAY);
				goto retry;
			}

			orderSubmittedEventArgs.response = task?.Result;

			if (orderSubmittedEventArgs.response == null) {

				string warningMessage = "response == null";



#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog ("Error submitting transaction : " + warningMessage);
					Logging.WriteLog (orderSubmittedEventArgs.rippleOfferTransaction.ToJson ());
				}
#endif
				if (VerifyTx (orderSubmittedEventArgs.rippleOfferTransaction, networkInterface)) {
					orderSubmittedEventArgs.success = true;
					return orderSubmittedEventArgs;

				}

				Thread.Sleep (FAILED_ATTEMPT_RETRY_DELAY);
				goto retry;
			}

			if (orderSubmittedEventArgs.response.HasError ()) {

				StringBuilder sb = new StringBuilder ();
				sb.Append ("Error response : ");
				sb.Append (orderSubmittedEventArgs.response?.error_message ?? "");

#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog ("Error submitting transaction ");
					Logging.WriteLog (orderSubmittedEventArgs.rippleOfferTransaction.ToJson ());
					Logging.WriteLog (sb.ToString ());
				}
#endif
				if (VerifyTx (orderSubmittedEventArgs.rippleOfferTransaction, networkInterface)) {
					orderSubmittedEventArgs.success = true;
					return orderSubmittedEventArgs;
				}
				Thread.Sleep (FAILED_ATTEMPT_RETRY_DELAY);
				goto retry;

			}


			RippleSubmitTxResult res = orderSubmittedEventArgs.response?.result;

			if (res == null) {

#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog ("res == null, Bug?");
				}
#endif


				if (VerifyTx (orderSubmittedEventArgs.rippleOfferTransaction, networkInterface)) {
					orderSubmittedEventArgs.success = true;
					return orderSubmittedEventArgs;
				}

				Thread.Sleep (FAILED_ATTEMPT_RETRY_DELAY);
				goto retry;
			}

			string een = res?.engine_result;

			if (een == null) {

#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog ("engine_result null");
				}
#endif

				if (VerifyTx (orderSubmittedEventArgs.rippleOfferTransaction, networkInterface)) {
					orderSubmittedEventArgs.success = true;
					return orderSubmittedEventArgs;
				}
				Thread.Sleep (FAILED_ATTEMPT_RETRY_DELAY);
				goto retry;
			}

			Ter ter;

			try {
				ter = (Ter)Enum.Parse (typeof (Ter), een, true);
				//ter = (Ter)Ter.Parse (typeof(Ter), een, true);

			} catch (ArgumentNullException exc) {
#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.ReportException (method_sig, exc);
					Logging.WriteLog ("null exception");
				}
#endif
				if (VerifyTx (orderSubmittedEventArgs.rippleOfferTransaction, networkInterface)) {
					orderSubmittedEventArgs.success = true;
					return orderSubmittedEventArgs;
				}
				Thread.Sleep (FAILED_ATTEMPT_RETRY_DELAY);
				goto retry;


			} catch (OverflowException overFlowException) {
#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.ReportException (method_sig, overFlowException);
				}
#endif
				Logging.WriteLog ("Overflow Exception");
				orderSubmittedEventArgs.success = false;
				return orderSubmittedEventArgs;
			} catch (ArgumentException argumentException) {
#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog ("Argument Exception");
					Logging.ReportException (method_sig, argumentException);
				}
#endif

				orderSubmittedEventArgs.success = false;
				return orderSubmittedEventArgs;
			} catch (Exception e) {
#if DEBUG
				if (DebugIhildaWallet.OrderSubmitter) {
					Logging.WriteLog ("Unknown Exception");
					Logging.ReportException (method_sig, e);
				}
#endif
				orderSubmittedEventArgs.success = false;
				return orderSubmittedEventArgs;
			}


			switch (ter) {

			case Ter.tefALREADY:

				LogResult (res.engine_result, res.engine_result_message);

				// TODO verify ?
				Thread.Sleep (FAILED_ATTEMPT_RETRY_DELAY);
				// not actually a failure so we are continuing 
				orderSubmittedEventArgs.success = true;
				return orderSubmittedEventArgs;

			case Ter.terQUEUED:

				Thread.Sleep (1000);

				LogResult (res.engine_result, res.engine_result_message);



				if (VerifyTx (orderSubmittedEventArgs.rippleOfferTransaction, networkInterface)) {
					orderSubmittedEventArgs.success = true;
					return orderSubmittedEventArgs;
				}

				Thread.Sleep (FAILED_ATTEMPT_RETRY_DELAY);
				goto retry;


			case Ter.tesSUCCESS:

				LogResult (res.engine_result, res.engine_result_message);

				if (VerifyTx (orderSubmittedEventArgs.rippleOfferTransaction, networkInterface)) {
					orderSubmittedEventArgs.success = true;
					return orderSubmittedEventArgs;
				}

				Thread.Sleep (FAILED_ATTEMPT_RETRY_DELAY);
				goto retry;

			case Ter.terPRE_SEQ:
			case Ter.tefPAST_SEQ:
			case Ter.tefMAX_LEDGER:

				LogResult (res.engine_result, res.engine_result_message);
				Thread.Sleep (FAILED_ATTEMPT_RETRY_DELAY);
				if (VerifyTx (orderSubmittedEventArgs.rippleOfferTransaction, networkInterface)) {
					orderSubmittedEventArgs.success = true;
					return orderSubmittedEventArgs;
				}
				goto retry;

			case Ter.terRETRY:
			case Ter.telCAN_NOT_QUEUE:

			case Ter.telCAN_NOT_QUEUE_FULL:
				LogResult (res.engine_result, res.engine_result_message);
				Thread.Sleep (FAILED_ATTEMPT_RETRY_DELAY);
				if (VerifyTx (orderSubmittedEventArgs.rippleOfferTransaction, networkInterface)) {
					orderSubmittedEventArgs.success = true;
					return orderSubmittedEventArgs;
				}
				goto retry;

			case Ter.telCAN_NOT_QUEUE_BALANCE:
			case Ter.telCAN_NOT_QUEUE_FEE:
			case Ter.telINSUF_FEE_P:
			case Ter.terFUNDS_SPENT:

				if (lastFee != null) {
					orderSubmittedEventArgs.success = false;
					return orderSubmittedEventArgs;

				}

				lastFee = (UInt32)orderSubmittedEventArgs.rippleOfferTransaction.fee.amount;
				Thread.Sleep (FAILED_ATTEMPT_RETRY_DELAY);

				LogResult (res.engine_result + " retrying", res.engine_result_message);

				if (VerifyTx (orderSubmittedEventArgs.rippleOfferTransaction, networkInterface)) {
					orderSubmittedEventArgs.success = true;
					return orderSubmittedEventArgs;
				}
				goto retry;


			case Ter.temBAD_AMOUNT:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.success = false;
				return orderSubmittedEventArgs;

			case Ter.tecNO_ISSUER:
			case Ter.temBAD_ISSUER:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.success = false;
				return orderSubmittedEventArgs;

			case Ter.tecUNFUNDED_OFFER:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.success = false;
				return orderSubmittedEventArgs;

			case Ter.tecUNFUNDED:
			case Ter.tecINSUF_RESERVE_OFFER:
			case Ter.tecINSUF_RESERVE_LINE:
			case Ter.tecINSUFFICIENT_RESERVE:
			case Ter.tecNO_LINE_INSUF_RESERVE:


				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.success = false;
				return orderSubmittedEventArgs;

			case Ter.temBAD_AUTH_MASTER:
			case Ter.tefBAD_AUTH_MASTER:
			case Ter.tefMASTER_DISABLED:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.success = false;
				return orderSubmittedEventArgs;


			case Ter.terNO_ACCOUNT:

				LogResult (res.engine_result, res.engine_result_message);

				orderSubmittedEventArgs.success = false;
				return orderSubmittedEventArgs;

			case Ter.tecNO_AUTH: // Not authorized to hold IOUs.
			case Ter.tecNO_LINE:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.success = false;
				return orderSubmittedEventArgs;

			case Ter.tecFROZEN:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.success = false;
				return orderSubmittedEventArgs;

			case Ter.tefFAILURE:
				// TODO what to do?

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.success = false;
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

				orderSubmittedEventArgs.success = false;
				return orderSubmittedEventArgs;

			case Ter.tecPATH_DRY:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.success = false;
				return orderSubmittedEventArgs;

			case Ter.tecPATH_PARTIAL:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.success = false;
				return orderSubmittedEventArgs;

			case Ter.tecOVERSIZE:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.success = false;
				return orderSubmittedEventArgs;

			case Ter.tefINTERNAL:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.success = false;
				return orderSubmittedEventArgs;

			case Ter.tefEXCEPTION:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.success = false;
				return orderSubmittedEventArgs;

			case Ter.tefBAD_LEDGER:
				// report bug to ripple labs

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.success = false;
				return orderSubmittedEventArgs;

			case Ter.tecDIR_FULL:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.success = false;
				return orderSubmittedEventArgs;

			case Ter.tecCLAIM:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.success = false;
				return orderSubmittedEventArgs;

			case Ter.tecEXPIRED:

				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.success = false;
				return orderSubmittedEventArgs;

			default:
				Logging.WriteLog (res.engine_result + " : response not implemented");
				LogResult (res.engine_result, res.engine_result_message);
				orderSubmittedEventArgs.success = false;
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
						decimal pricej = _offers [j].TakerPays.GetNativeAdjustedPriceAt (_offers [j].TakerGets);

						decimal cost = _offers [i].TakerPays.GetNativeAdjustedCostAt (_offers [i].TakerGets);
						decimal costj = _offers [j].TakerPays.GetNativeAdjustedCostAt (_offers [j].TakerGets);


						decimal spread = 1.006m;

						decimal resaleEstimate = price * spread;
						//diff = Math.Abs (dif);

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

		private bool VerifyTx (RippleOfferTransaction offerTransaction, NetworkInterface networkInterface)
		{



			Thread.Sleep (1000);
			Logging.WriteLog ("validating tx\n");
			Thread.Sleep (2000);
			for (int i = 0; i < 100; i++) {
				Thread.Sleep (3000);
				Tuple<string, uint> tuple = ServerInfo.GetFeeAndLedgerSequence (networkInterface);

				Task<Response<RippleTransaction>> task = tx.GetRequest (offerTransaction.hash, networkInterface);
				if (task == null) {
					// TODO Debug
					Logging.WriteLog ("Error : task == null");
					MessageDialog.ShowMessage ("Error : task == null");
					//Thread.Sleep (3000);
					continue;
				}


				task.Wait ();

				Response<RippleTransaction> response = task.Result;
				if (response == null) {
					Logging.WriteLog ("Error : response == null");
					continue;
				}
				RippleTransaction transaction = response.result;

				if (transaction == null) {
					Logging.WriteLog ("Error : transaction == null");
					continue;
				}

				if (transaction.validated != null && (bool)transaction.validated) {

					Logging.WriteLog ("validated");
					return true;
				}

				if (tuple.Item2 > offerTransaction.LastLedgerSequence) {

					Logging.WriteLog ("failed to validate before LastLedgerSequence exceeded");

					return false;
				}



				Logging.WriteLog ("Not validated yet ");

			}


			Logging.WriteLog ("Max validation attempts exceeded");
			return false;

		}


		private void LogResult (string result, string message)
		{
			Logging.WriteLog ("Result = " + result);
			Logging.WriteLog ("Message = " + message);
		}

		public static int MAX_SUBMIT_ATTEMPTS = 3;
		public static int FAILED_ATTEMPT_RETRY_DELAY = 6000;

		public event EventHandler<OrderSubmittedEventArgs> OrderSubmitted;

		//public delegate void ThresholdReached (object sender, OrderSubmittedEventArgs orderSubmittedEventArgs);
#if DEBUG
		const string clsstr = nameof (OrderSubmittedEventArgs) + DebugRippleLibSharp.colon;
#endif

	}

	public class OrderSubmittedEventArgs : EventArgs
	{

		public bool success {
			get;
			set;
		}
		public RippleOfferTransaction rippleOfferTransaction {
			get;
			set;
		}

		public AutomatedOrder automatedOrder {
			get;
			set;
		}

		public Response<RippleSubmitTxResult> response {
			get;
			set;
		}


	}
}
