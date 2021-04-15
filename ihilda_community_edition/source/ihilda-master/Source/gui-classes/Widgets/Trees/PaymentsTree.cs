using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using IhildaWallet.Networking;
using RippleLibSharp.Binary;
using RippleLibSharp.Keys;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;
using RippleLibSharp.Transactions.TxTypes;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PaymentsTree : Gtk.Bin
	{
		public PaymentsTree ()
		{
			this.Build ();

			listStore = new ListStore ( 
				typeof(bool), 		// 0 checkbox 
				typeof(string), 	// 1 number
				typeof(string), 	// 2 Destination
				typeof(string), 	// 3 Amount
				typeof(string), 	// 4 Send Max
				typeof(string),		// 5 Memos
				typeof(string), 	// 6 Status
				typeof (string) );	// 7 Result

			Gtk.CellRendererToggle toggle = new CellRendererToggle {
				Activatable = true
			};
			toggle.Toggled += ItemToggled;


			CellRendererText txtr = new CellRendererText {
				Editable = false
			};

			treeview1.AppendColumn ("Select", toggle, "active", 0);

			//this.treeview1.AppendColumn ("<span fgcolor=\"green\">Buy</span>", txtr, "markup", 1);

			this.treeview1.AppendColumn ("#", txtr, "markup", 1);

			this.treeview1.AppendColumn ("Destination", txtr, "markup", 2);
			this.treeview1.AppendColumn ("Amount", txtr, "markup", 3);
			this.treeview1.AppendColumn ("Sendmax", txtr, "markup", 4);

			this.treeview1.AppendColumn ("Memos", txtr, "markup", 5);

			this.treeview1.AppendColumn ("Status", txtr, "markup", 6);
			this.treeview1.AppendColumn ("Result", txtr, "markup", 7);
		}


		public bool SubmitPaymentAtIndex ( int index , uint sequence, NetworkInterface ni, CancellationToken token, RippleIdentifier rsa) {

			#if DEBUG
			string method_sig = clsstr + nameof (SubmitPaymentAtIndex) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PaymentWindow) {
				Logging.WriteLog( method_sig + DebugRippleLibSharp.beginn );
			}
#endif


			TextHighlighter greenHighlighter = new TextHighlighter {
				Highlightcolor = ProgramVariables.darkmode ?
					TextHighlighter.CHARTREUSE :
					TextHighlighter.GREEN
			};

			TextHighlighter redHighlighter = new TextHighlighter {
				Highlightcolor = ProgramVariables.darkmode ?
					TextHighlighter.LIGHT_RED :
		    			TextHighlighter.RED
			};


			try {

				#region index_str
				StringBuilder stringBuilder = new StringBuilder ();

				stringBuilder.Append ("Tx ");
				stringBuilder.Append ((index + 1).ToString ());

				string bld = TextHighlighter.MakeBold (stringBuilder.ToString ());

				stringBuilder.Clear ();

				stringBuilder.Append (bld);
				stringBuilder.Append (" : ");

				string txAtIndexStr = stringBuilder.ToString ();
				#endregion

				stringBuilder.Clear ();
				stringBuilder.Append ("Submitting ");
				stringBuilder.Append (txAtIndexStr);

				this.SetInfoBox (greenHighlighter.Highlight( stringBuilder.ToString()));
				//
				this.SetStatus ( index.ToString(), "Queued...", TextHighlighter.GREEN );

				Tuple<RipplePaymentTransaction [], bool []> payTupe = _payments_tuple;




				retry:
				RipplePaymentTransaction tx = payTupe.Item1 [index];


				SignOptions opts = SignOptions.LoadSignOptions();


				string feeReq = "Requesting Fee...";

				stringBuilder.Clear ();
				stringBuilder.Append (txAtIndexStr);
				stringBuilder.Append (feeReq);

				SetInfoBox (greenHighlighter.Highlight (stringBuilder.ToString ()));

				this.SetStatus (
					index.ToString(), 
					feeReq, 
		    			TextHighlighter.GREEN);



				FeeSettings feeSettings = FeeSettings.LoadSettings ();
				if (feeSettings == null) {

					string missFee = "missing fee settings";

					stringBuilder.Clear ();
					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (missFee);

					this.SetInfoBox (redHighlighter.Highlight( stringBuilder.ToString ()));

					this.SetStatus (index.ToString(), missFee, TextHighlighter.RED);
					return false;
				}

				feeSettings.OnFeeSleep += (object sender, FeeSleepEventArgs e) => {
					// don't bother messing with the stringbuilder from a callback

					string mess =
						"Fee " +
						(e?.FeeAndLastLedger?.Fee.ToString () ?? "null") +
						" is too high, waiting on lower fee";

					string info = txAtIndexStr + mess;
					SetInfoBox (info);

					this.SetResult (
						index.ToString(), 
						mess,
						TextHighlighter.BLACK );
				};

				ParsedFeeAndLedgerResp feetupe = feeSettings.GetFeeAndLastLedgerFromSettings ( ni, token );


				if (token.IsCancellationRequested) {

					string m = "Aborted";
					stringBuilder.Clear ();
					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (m);

					this.SetInfoBox ( redHighlighter.Highlight (stringBuilder.ToString ()) );

					this.SetResult(index.ToString(), m, TextHighlighter.RED);
					
					return false;
				}

				if (feetupe == null) {
					string m = "Unable to retrieve fee and last ledger from settings\n";

					stringBuilder.Clear ();
					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (m);

					SetInfoBox ( redHighlighter.Highlight ( stringBuilder.ToString ()) );

					this.SetResult (index.ToString (), m, TextHighlighter.RED);

					return false;
				}

				if (feetupe.HasError) {

					string m = feetupe.ErrorMessage;

					stringBuilder.Clear ();
					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (m);

					SetInfoBox (redHighlighter.Highlight (stringBuilder.ToString ()));

					this.SetResult (index.ToString (), m, TextHighlighter.RED);
					return false;
				}

				/* always false
				if (tupe.Fee == null) {
					this.SetResult (index.ToString (), "null fee", TextHighlighter.RED);
					return false;
				}
				*/
				
				//UInt32 f = tupe.Item1; 
				UInt32 f = (UInt32)feetupe.Fee;
				tx.fee = f.ToString ();





				tx.Sequence = sequence; // note: don't update se++ with forloop, update it with each payment



				uint lls = 0;
				if (opts != null) {
					lls = opts.LastLedgerOffset;
				}

				if (lls < 5) {
					lls = SignOptions.DEFAUL_LAST_LEDGER_SEQ;
				}


				tx.LastLedgerSequence = (UInt32)feetupe.LastLedger + lls;

				if (tx.fee.amount == 0 ) {
					string m = "Invalid Fee zero";

					stringBuilder.Clear ();
					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (m);

					this.SetInfoBox (redHighlighter.Highlight (stringBuilder.ToString ()));

					this.SetResult(index.ToString(), m, TextHighlighter.RED);
					throw new Exception ();
				}


				if (tx.Sequence == 0) {
					string m = "Invalid Sequence";

					stringBuilder.Clear ();
					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (m);

					SetInfoBox (redHighlighter.Highlight (stringBuilder.ToString()));

					this.SetResult (index.ToString (), m, TextHighlighter.RED);
					throw new Exception ();
				}

				if (opts == null) {
					// TODO get user to choose and save choice
				}


				switch (opts.SigningLibrary) {

				case "Rippled":
				 
					{
						string m = "Signing using rpc";

						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);

						SetInfoBox (greenHighlighter.Highlight (stringBuilder.ToString ()));

						this.SetStatus (index.ToString (), m, TextHighlighter.GREEN);
					}

					try {
						tx.SignLocalRippled (rsa);
					} catch (Exception e) {

#if DEBUG
						if (DebugIhildaWallet.PaymentTree) {
							Logging.ReportException (method_sig, e);
						}
#endif

						{
							string m = "Error Signing using rpc";

							stringBuilder.Clear ();
							stringBuilder.Append (txAtIndexStr);
							stringBuilder.Append (m);

							SetInfoBox (redHighlighter.Highlight (stringBuilder.ToString ()));

							this.SetResult (index.ToString (), m, TextHighlighter.RED);
						}
						return false;
					} 

					{
						string m = "Signed rpc";

						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);

						SetInfoBox (greenHighlighter.Highlight (stringBuilder.ToString ()));

						this.SetStatus (index.ToString (), m, TextHighlighter.GREEN);
					}
					break;

				case "RippleLibSharp": 
				
					{
						string m = "Signing using RippleLibSharp";
						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);

						SetInfoBox (greenHighlighter.Highlight (stringBuilder.ToString ()));

						this.SetStatus (index.ToString (), m, TextHighlighter.GREEN);
					}
					try {
						tx.SignRippleLibSharp (rsa);
					} catch (Exception e) {

#if DEBUG
						if (DebugIhildaWallet.PaymentTree) {
							Logging.ReportException (method_sig, e);
						}
#endif
						{
							string m = "Signing using RippleLibSharp";
							stringBuilder.Clear ();
							stringBuilder.Append (txAtIndexStr);
							stringBuilder.Append (m);

							this.SetInfoBox (greenHighlighter.Highlight ( stringBuilder.ToString()));

							this.SetResult (index.ToString (), m, TextHighlighter.RED);
							return false;
						}
					} 

					{
						string m = "Signed RippleLibSharp";
						stringBuilder.Clear ();
						stringBuilder.Append(txAtIndexStr);
						stringBuilder.Append (m);
						this.SetInfoBox (greenHighlighter.Highlight (stringBuilder.ToString ()));


						this.SetStatus (index.ToString (), m, TextHighlighter.GREEN);
					}

					break;

				case "RippleDotNet": 
					{
						string m = "Signing using RippleDotNet";

						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);
						SetInfoBox (greenHighlighter.Highlight (stringBuilder.ToString ()));

						this.SetStatus (index.ToString (), m, TextHighlighter.GREEN);
					}
					try {
						tx.SignRippleDotNet (rsa);
					} catch (Exception e) {

#if DEBUG
						if (DebugIhildaWallet.PaymentTree) {
							Logging.ReportException (method_sig, e);
						}
#endif

						{
							string m = "Signing using RippleDotNet";
							stringBuilder.Clear ();
							stringBuilder.Append (txAtIndexStr);
							stringBuilder.Append (m);
							SetInfoBox (redHighlighter.Highlight (stringBuilder.ToString ()));

							this.SetResult (index.ToString (), m + "\n" + e.Message, TextHighlighter.RED);
						}
						return false;
					} 

					{
						string m = "Signed RippleDotNet";
						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);
						SetInfoBox (redHighlighter.Highlight (stringBuilder.ToString ()));

						this.SetStatus (index.ToString (), m, TextHighlighter.GREEN);
					}
					break;

				}

				if (tx.GetSignedTxBlob () == null) {

					{
						string m = "Error signing transaction";
						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);

						SetInfoBox (redHighlighter.Highlight (stringBuilder.ToString()));

						this.SetStatus (index.ToString (), m, TextHighlighter.RED);
					}
					return false;
				}


				if (token.IsCancellationRequested) {

					{
						string m = "Aborted";
						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);

						SetInfoBox (redHighlighter.Highlight (stringBuilder.ToString ()));

						this.SetResult (index.ToString (), m, TextHighlighter.RED);
					}
					//stop = false;
					return false;
				}



				Task< Response <RippleSubmitTxResult>> task = null;

				try {
					task = NetworkController.UiTxNetworkSubmit (tx, ni, token);

					{
						string m = "Submitted via websocket";
						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);

						SetInfoBox (greenHighlighter.Highlight (stringBuilder.ToString ()));

						this.SetStatus (index.ToString (), m, TextHighlighter.GREEN);
					}
					task.Wait (token);

					int MAXDOTS = 5;
					int x = 0;

					string waitMessg = "Waiting on network";

					while (
						task != null &&
						!task.IsCompleted &&
						!task.IsCanceled &&
						!task.IsFaulted &&
						!token.IsCancellationRequested

					) {
						task.Wait (1000, token);

						stringBuilder.Clear ();
						stringBuilder.Append (waitMessg);

						int dots = x++ % MAXDOTS;

						stringBuilder.Append (new string ('.', x));


						{
							string m = stringBuilder.ToString ();
							stringBuilder.Clear ();
							stringBuilder.Append (txAtIndexStr);
							stringBuilder.Append (m);

							this.SetInfoBox (greenHighlighter.Highlight (stringBuilder.ToString ()));

							this.SetResult (index.ToString (), stringBuilder.ToString (), TextHighlighter.GREEN);
						}


					}


				} catch (Exception e) when (e is TaskCanceledException || e is OperationCanceledException) {
#if DEBUG
					if (DebugIhildaWallet.PaymentTree) {
						Logging.ReportException (method_sig, e);
					}
#endif

					{
						string m = "Operation Cancelled";
						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);

						SetInfoBox (redHighlighter.Highlight (stringBuilder.ToString ()));

						this.SetStatus (index.ToString (), m, TextHighlighter.RED);
					}
				} catch (Exception e) {

					Logging.WriteLog (e.Message);

					{
						string m = "Network Error";

						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);

						SetInfoBox (redHighlighter.Highlight (stringBuilder.ToString ()));

						this.SetResult (index.ToString (), m, TextHighlighter.RED);
					}
					return false;
				}

				/*
				finally {
					Logging.writeLog (method_sig + "sleep");
					 may or may not keep a slight delay here for orders to process
				}
				*/

				Response<RippleSubmitTxResult> response = task.Result;

				string errorMessage = "Error submitting transaction";
				if (response == null ) {

					errorMessage += "(r == null)";

					{
						string m = errorMessage;
						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);

						SetInfoBox (redHighlighter.Highlight (stringBuilder.ToString ()));

						this.SetResult (index.ToString (), m, TextHighlighter.RED);
					}

					#if DEBUG
					if (DebugIhildaWallet.PaymentTree) {
						Logging.WriteLog(errorMessage);
						Logging.WriteLog (tx.ToJson());
					}
					#endif 


					return false;

				}


				if (response.status == null) {
					errorMessage += "(r.status == null)";

					{
						string m = errorMessage;
						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);

						SetInfoBox (redHighlighter.Highlight (stringBuilder.ToString ()));

						this.SetResult (index.ToString (), m, TextHighlighter.RED);
					}
#if DEBUG
					if (DebugIhildaWallet.PaymentTree) {
						Logging.WriteLog (errorMessage);
						Logging.WriteLog (tx.ToJson ());
					}
#endif


					return false;

				}

				if (!response.status.Equals ("success")) {
					errorMessage += "!r.status.Equals (\"success\")";

					{
						string m = response.error_message;
						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);

						this.SetResult (index.ToString (), m, TextHighlighter.RED);
					}

#if DEBUG
					if (DebugIhildaWallet.PaymentTree) {
						Logging.WriteLog (errorMessage);
						Logging.WriteLog (tx.ToJson ());
					}
#endif


					return false;

				}


				RippleSubmitTxResult res = response.result;

				if (res == null) {
					errorMessage += "res == null";

					{
						string m = errorMessage;
						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);

						SetInfoBox (redHighlighter.Highlight (stringBuilder.ToString ()));

						this.SetResult (index.ToString (), m, TextHighlighter.RED);
					}
					return false;
				}


				//tefPAST_SEQ


				switch ( res.engine_result ) {

				case null:

					errorMessage += "res.engine_result = null"; 

					{
						string m = errorMessage;
						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);

						SetInfoBox (redHighlighter.Highlight (stringBuilder.ToString ()));

						this.SetResult (index.ToString (), m, TextHighlighter.RED);
					}

					return false;

				case "terQUEUED":
					//Thread.Sleep(1000);
					token.WaitHandle.WaitOne (1000); 
					{
						string m = res.engine_result;
						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);

						SetInfoBox (greenHighlighter.Highlight (stringBuilder.ToString ()));

						this.SetResult (index.ToString (), m, TextHighlighter.GREEN);
					}
					return true;

				case "tesSUCCESS": 
					{
						string m = res.engine_result;
						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);

						SetInfoBox (greenHighlighter.Highlight (stringBuilder.ToString ()));


						this.SetResult (index.ToString (), m, TextHighlighter.GREEN);
					}
					return true;

				case  "terPRE_SEQ":
				case "tefPAST_SEQ":
				case "tefMAX_LEDGER":
				case "tecNO_DST_INSUF_XRP":
				case  "noNetwork": 
					{
						string m = res.engine_result;
						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);

						SetInfoBox (redHighlighter.Highlight (stringBuilder.ToString ()));

						this.SetResult (index.ToString (), m, TextHighlighter.RED);
					}
					return false;

				case "telCAN_NOT_QUEUE": 
				
					{
						string m = res.engine_result + " retrying";
						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);

						SetInfoBox (redHighlighter.Highlight (stringBuilder.ToString ()));

						this.SetResult (index.ToString (), m, TextHighlighter.RED);
					}
					goto retry;

				case "telINSUF_FEE_P": 
				
					{
						string m = res.engine_result;
						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);

						SetInfoBox (redHighlighter.Highlight (stringBuilder.ToString()));

						this.SetResult (index.ToString (), m, TextHighlighter.RED);
					}
					return false;


				case "tecNO_ISSUER": 
				
					{
						string m = res.engine_result;
						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);

						SetInfoBox (redHighlighter.Highlight (stringBuilder.ToString ()));

						this.SetResult (index.ToString (), m, TextHighlighter.RED);
					}

					return false;


				case "tefMASTER_DISABLED": 
				
					{
						string m = res.engine_result;
						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);

						SetInfoBox (redHighlighter.Highlight (stringBuilder.ToString ()));

						this.SetResult (index.ToString (), m, TextHighlighter.RED);
					}
					return false;


					/*
				case "tecUNFUNDED_OFFER":
					this.setFailed (index.ToString(), res.engine_result);
					return false;
					*/

				default: 
				
					{

						string m = res.engine_result;
						stringBuilder.Clear ();
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (m);

						SetInfoBox (redHighlighter.Highlight (stringBuilder.ToString ()));

						this.SetResult (index.ToString (), "Response not imlemented : " + m, TextHighlighter.RED);
					}
					return false;

				}


				/*
				 * 
				if ( res.tx_json == null || res.tx_json.meta == null || res.tx_json.meta.AffectedNodes == null) {
					return false;
				}

				IEnumerable<RippleNode> nds = from RippleNodeGroup rng in res.tx_json.meta.AffectedNodes 
					where rng.CreatedNode != null

				//&& rng.CreatedNode.FinalFields != null
				&& rng.CreatedNode.NewFields.Account != null
				&& rng.CreatedNode.NewFields.Account == rw.getStoredReceiveAddress()
				&& rng.CreatedNode.LedgerEntryType != null
				&& rng.CreatedNode.LedgerEntryType.Equals( "OfferCreate" )

				select rng.getnode();




				// A newly created order wouldn't cause any new orders except it's own, so there is one or none
				foreach ( RippleNode rn in nds) {

					//OrderChange oc = rn.getOfferChange (rw.getStoredReceiveAddress());
					AutomatedOrder filled = null;
					filled = AutomatedOrder.reconstructFromNode (rn);
					filledorders.AddLast (filled);
					continue; 
				}

				*/

			}

			#pragma warning disable 0168
			catch ( Exception e ) {
				#pragma warning restore 0168


				#if DEBUG
				if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
					Logging.ReportException (method_sig, e);
				}
				#endif


				
				this.SetResult (
					index.ToString (), 
					"Exception Thrown in code\n" + (string)(e?.Message ?? "{null message}"), 
		    			TextHighlighter.RED);

				return false;

			}

		}

		public void SetParent (PaymentPreviewSubmitWidget parent)
		{
			this._parent = parent;
		}

		private PaymentPreviewSubmitWidget _parent;

		private void SetInfoBox (string v)
		{
			_parent.SetInfoBox (v);
		}

		public void SetStatus(string path, string message, string colorName) {
			if (message == null)
				message = "";

			TextHighlighter highlighter = new TextHighlighter {
				Highlightcolor = colorName
			};

			string s = highlighter.Highlight (/*"Success : " + */message);

			Gtk.Application.Invoke ( (object sender, EventArgs e) => {
				if (listStore.GetIterFromString (out TreeIter iter, path)) {
					listStore.SetValue (iter, 6, s);



				}
			});

		}

		public void SetResult (string path, string message, string colorName) {
			TextHighlighter highlighter = new TextHighlighter {
				Highlightcolor = colorName
			};

			string s = highlighter.Highlight (message ?? "");


			Gtk.Application.Invoke ( (object sender, EventArgs e) => {

				if (listStore.GetIterFromString (out TreeIter iter, path)) {
					listStore.SetValue (iter, 7, s);



				}
			});




		}




		public void SetPayments (IEnumerable <RipplePaymentTransaction> payments) {
			SetPayments (payments,false);
		}

		public void RefreshPaymentGUI ()
		{

			Tuple<RipplePaymentTransaction [], bool []> payTupe = _payments_tuple;
			listStore.Clear ();
			if (payTupe == null) {
				// TODO
				return;
			}

			

			for (int i = 0; i < payTupe.Item1.Length; i++) {
				
				string Destination = payTupe.Item1[i]?.Destination;
				RippleCurrency Amount = payTupe.Item1[i]?.Amount;
				RippleCurrency Sendmax = payTupe.Item1[i]?.SendMax;
				MemoIndice [] memos = payTupe.Item1 [i]?.Memos;



				listStore.AppendValues (
					payTupe.Item2[i], 
					Destination ?? "", 
		    			Amount?.ToString () ?? "", 
		    			Sendmax?.ToString () ?? "");
					GetMemosString (memos);
				//o.selected = true;
			}

		}

		private string GetMemosString (MemoIndice[] memos)
		{
			StringBuilder memoStringBuilder = new StringBuilder ();


			if (memos != null) {

				bool first = true;
				foreach (MemoIndice memo in memos) {
					if (first) {
						first = false;
					} else {
						memoStringBuilder.AppendLine ();
					}

					memoStringBuilder.AppendLine ("Type:");
					memoStringBuilder.AppendLine (memo.GetMemoTypeAscii ());
					memoStringBuilder.AppendLine ();

					memoStringBuilder.AppendLine ("Format:");
					memoStringBuilder.AppendLine (memo.GetMemoFormatAscii ());
					memoStringBuilder.AppendLine ();

					memoStringBuilder.AppendLine ("Data:");
					memoStringBuilder.AppendLine (memo.GetMemoDataAscii ());
					memoStringBuilder.AppendLine ();


				}
			}

			return memoStringBuilder.ToString();
		}


		public void SetPayments ( IEnumerable <RipplePaymentTransaction> payments, bool isSelectDefault ) {
			
			int count = payments.Count ();
			RipplePaymentTransaction[] _payments = new RipplePaymentTransaction[ count ];
			bool[] Selections = new bool[ count ];



			listStore.Clear ();

			for (int i = 0; i < count; i++) {

				RipplePaymentTransaction ripplePaymentTransaction = payments?.ElementAt (i);

				RippleBinaryObject obj = ripplePaymentTransaction?.GetBinaryObject ();


				if (obj == null) {
					//TODO
				}

				RipplePaymentTransaction o = 
					new RipplePaymentTransaction( 
					                             
						obj
					);

				string Destination = o?.Destination ?? "(null destination)";



				if (o?.DestinationTag != null) {
					Destination = Destination + "\nDestination Tag : " + o?.DestinationTag.ToString ();
				}


				RippleCurrency Amount = o?.Amount;

				RippleCurrency Sendmax = o?.SendMax;
				MemoIndice [] memos = ripplePaymentTransaction.Memos; //o?.Memos;

				listStore.AppendValues (
					isSelectDefault, 
					(i + 1).ToString (), 
		    			Destination, 
		    			Amount?.ToString() ?? "", 
					Sendmax?.ToString() ?? "",
		    			GetMemosString(memos)
		    
				);

				//o.selected = true;

				_payments [i] = o;
				Selections [i] = isSelectDefault;

				Tuple <RipplePaymentTransaction [], bool []> tuple 
				= new Tuple<RipplePaymentTransaction [], bool []> (_payments,Selections);

				_payments_tuple = tuple;

			}


			treeview1.Model = listStore;


		}

		public void Removebutton_Clicked (object sender, EventArgs e)
		{

			Tuple<RipplePaymentTransaction [], bool []> oldTuple = _payments_tuple;
			if (oldTuple == null) {
				// TODO
				return;
			}

			if (oldTuple.Item1 == null) {
				// TODO
				// bug report. Should be unreachable
				return;
			}

			if (oldTuple.Item2 == null) {
				return;
			}

			List<RipplePaymentTransaction> newTransactions = new List<RipplePaymentTransaction> ();

			for (int i = 0; i < oldTuple.Item2.Length; i++) {
				if (!oldTuple.Item2[i]) {
					newTransactions.Add (oldTuple.Item1[i]);

				}

			}

			bool [] newSelections = new bool [ newTransactions.Count() ];


			RipplePaymentTransaction[] paymentTransactions = newTransactions.ToArray ();
			bool [] selections = newSelections;

			Tuple<RipplePaymentTransaction [], bool []> newTuple = 
				new Tuple<RipplePaymentTransaction [], bool []> (paymentTransactions,selections);

			this._payments_tuple = newTuple;

			this.RefreshPaymentGUI ();

		}

		public void Selectbutton_Clicked (object sender, EventArgs e)
		{
			bool allselected = true;

			Tuple<RipplePaymentTransaction [], bool []> tuple = _payments_tuple;
			foreach (bool b in tuple.Item2 ) {
				if (!b) {
					allselected = false;
					break;

				}
			}
			         
			for (int i = 0; i < tuple.Item2.Length; i++) {
			
				tuple.Item2[i] = !allselected;
			}


		       

			this.RefreshPaymentGUI ();

		}

		private void ItemToggled (object sender, ToggledArgs args)
		{

			//string s = args.Path;
			int index = Convert.ToInt32 (args.Path);

			if (listStore.GetIterFromString (out TreeIter iter, args.Path)) {
				bool val = (bool)listStore.GetValue (iter, 0);
				listStore.SetValue (iter, 0, !val);


				this._payments_tuple.Item2 [index] = !val;
			}
		}

		public void ClearPayments () {
			this._payments_tuple = null;

			listStore.Clear ();
		}

		public void Stopbutton_Clicked (object sender, EventArgs e)
		{
			//this.stop = true;
			tokenSource?.Cancel ();
			tokenSource.Dispose ();
			tokenSource = null;
		}

		public Tuple <RipplePaymentTransaction[], bool []> _payments_tuple {
			get;
			set;
		}


		ListStore listStore;

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public CancellationTokenSource tokenSource = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

#if DEBUG
		private const string clsstr = nameof (PaymentsTree) + DebugRippleLibSharp.colon;
		#endif
	}
}

