using System;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using RippleLibSharp.Keys;
using RippleLibSharp.Transactions;
using RippleLibSharp.Util;
using RippleLibSharp.Transactions.TxTypes;
using RippleLibSharp.Result;
using RippleLibSharp.Network;
using IhildaWallet.Networking;
using Gtk;
using System.Collections.Generic;
using RippleLibSharp.Binary;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PaymentsTree : Gtk.Bin
	{
		public PaymentsTree ()
		{
			this.Build ();

			listStore = new ListStore ( typeof(bool), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof (string) );

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

			this.treeview1.AppendColumn ("Status", txtr, "markup", 5);
			this.treeview1.AppendColumn ("Result", txtr, "markup", 6);
		}


		public bool SubmitOrderAtIndex ( int index , uint sequence, NetworkInterface ni, CancellationToken token, RippleIdentifier rsa) {

			#if DEBUG
			string method_sig = clsstr + nameof (SubmitOrderAtIndex) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PaymentWindow) {
				Logging.WriteLog( method_sig + DebugRippleLibSharp.beginn );
			}
			#endif

			try {


				//
				this.SetStatus ( index.ToString(), "Queued", TextHighlighter.GREEN );

				Tuple<RipplePaymentTransaction [], bool []> payTupe = _payments_tuple;

				retry:
				RipplePaymentTransaction tx = payTupe.Item1 [index];


				SignOptions opts = SignOptions.LoadSignOptions();

				this.SetStatus (index.ToString(), "Requesting Fee", TextHighlighter.GREEN);

				FeeSettings feeSettings = FeeSettings.LoadSettings ();
				if (feeSettings == null) {
					return false;
				}

				feeSettings.OnFeeSleep += (object sender, FeeSleepEventArgs e) => {
					this.SetResult (index.ToString(), (string)("Fee " + e?.FeeAndLastLedger?.Fee.ToString() ?? "null") + " is too high, waiting on lower fee", TextHighlighter.BLACK);
				};

				ParsedFeeAndLedgerResp tupe = feeSettings.GetFeeAndLastLedgerFromSettings ( ni, token );


				if (token.IsCancellationRequested) {

					this.SetResult(index.ToString(), "Aborted", TextHighlighter.RED);
					
					return false;
				}

				if (tupe == null) {
					this.SetResult (index.ToString (), "Unable to retrieve fee and last ledger from settings\n", TextHighlighter.RED);
					return false;
				}

				if (tupe.HasError) {
					this.SetResult (index.ToString (), tupe.ErrorMessage, TextHighlighter.RED);
					return false;
				}

				
				//UInt32 f = tupe.Item1; 
				UInt32 f = (UInt32)tupe.Fee;
				tx.fee = f.ToString ();

				tx.Sequence = sequence; // note: don't update se++ with forloop, update it with each payment



				uint lls = 0;
				if (opts != null) {
					lls = opts.LastLedgerOffset;
				}

				if (lls < 5) {
					lls = SignOptions.DEFAUL_LAST_LEDGER_SEQ;
				}


				tx.LastLedgerSequence = (UInt32)tupe.LastLedger + lls;

				if (tx.fee.amount == 0 || tx.Sequence == 0 ) {
					this.SetResult(index.ToString(), "Invalid Fee or Sequence", TextHighlighter.RED);
					throw new Exception ();
				}

				if (opts == null) {
					// TODO get user to choose and save choice
				}


				if (opts.SigningLibrary == "Rippled") {

					this.SetStatus (index.ToString (), "Signing using rpc", TextHighlighter.GREEN);
					try {
						tx.SignLocalRippled (rsa);
					} catch (Exception e) {

#if DEBUG
						if (DebugIhildaWallet.PaymentTree) {
							Logging.ReportException (method_sig, e);
						}
#endif

						this.SetResult (index.ToString (), "Error Signing using rpc", TextHighlighter.RED);
						return false;
					}

					this.SetStatus (index.ToString (), "Signed rpc", TextHighlighter.GREEN);
				} else if (opts.SigningLibrary == "RippleLibSharp") {

					this.SetStatus (index.ToString (), "Signing using RippleLibSharp", TextHighlighter.GREEN);
					try {
						tx.Sign (rsa);
					} catch (Exception e) {

#if DEBUG
						if (DebugIhildaWallet.PaymentTree) {
							Logging.ReportException (method_sig, e);
						}
#endif

						this.SetResult (index.ToString (), "Signing using RippleLibSharp", TextHighlighter.RED);
						return false;
					}
					this.SetStatus (index.ToString (), "Signed RippleLibSharp", TextHighlighter.GREEN);

				} else if (opts.SigningLibrary == "RippleDotNet") {
					this.SetStatus (index.ToString (), "Signing using RippleDotNet", TextHighlighter.GREEN);
					try {
						tx.SignRippleDotNet (rsa);
					} catch (Exception e) {

#if DEBUG
						if (DebugIhildaWallet.PaymentTree) {
							Logging.ReportException (method_sig, e);
						}
#endif

						this.SetResult (index.ToString (), "Signing using RippleDotNet", TextHighlighter.RED);
						return false;
					}
					this.SetStatus (index.ToString (), "Signed RippleDotNet", TextHighlighter.GREEN);

				}



				if (token.IsCancellationRequested) {
					this.SetResult(index.ToString(), "Aborted", TextHighlighter.RED);
					//stop = false;
					return false;
				}



				Task< Response <RippleSubmitTxResult>> task = null;

				try {
					task = NetworkController.UiTxNetworkSubmit (tx, ni, token);
					this.SetStatus(index.ToString(), "Submitted via websocket", TextHighlighter.GREEN);
					task.Wait (token);


				}
				catch ( Exception e ) {

					Logging.WriteLog ( e.Message );
					this.SetResult ( index.ToString(), "Network Error", TextHighlighter.RED );
					return false;
				}

				/*
				finally {
					Logging.writeLog (method_sig + "sleep");
					 may or may not keep a slight delay here for orders to process
				}
				*/

				var r = task.Result;

				string errorMessage = "Error submitting transaction";
				if (r == null ) {

					errorMessage += "(r == null)";
					this.SetResult( index.ToString(), errorMessage,  TextHighlighter.RED);
					#if DEBUG
					if (DebugIhildaWallet.PaymentTree) {
						Logging.WriteLog(errorMessage);
						Logging.WriteLog (tx.ToJson());
					}
					#endif 


					return false;

				}


				if (r.status == null) {
					errorMessage += "(r.status == null)";
					this.SetResult (index.ToString (), errorMessage, TextHighlighter.RED);
#if DEBUG
					if (DebugIhildaWallet.PaymentTree) {
						Logging.WriteLog (errorMessage);
						Logging.WriteLog (tx.ToJson ());
					}
#endif


					return false;

				}

				if (!r.status.Equals ("success")) {
					errorMessage += "!r.status.Equals (\"success\")";
					this.SetResult (index.ToString (), r.error_message, TextHighlighter.RED);
#if DEBUG
					if (DebugIhildaWallet.PaymentTree) {
						Logging.WriteLog (errorMessage);
						Logging.WriteLog (tx.ToJson ());
					}
#endif


					return false;

				}


				RippleSubmitTxResult res = r.result;

				if (res == null) {
					errorMessage += "res == null";
						this.SetResult( index.ToString(), errorMessage, TextHighlighter.RED );
					return false;
				}


				//tefPAST_SEQ


				switch ( res.engine_result ) {

				case null:
					errorMessage += "res.engine_result = null";
					this.SetResult ( index.ToString(), "null", TextHighlighter.RED);
					return false;

				case "terQUEUED":
					//Thread.Sleep(1000);
					token.WaitHandle.WaitOne (1000);
					this.SetResult (index.ToString (), res.engine_result, TextHighlighter.GREEN);
					return true;

				case "tesSUCCESS":
					this.SetResult (index.ToString (), res.engine_result, TextHighlighter.GREEN);
					return true;

				case  "terPRE_SEQ":
				case "tefPAST_SEQ":
				case "tefMAX_LEDGER":
				case "tecNO_DST_INSUF_XRP":
				case  "noNetwork":
					this.SetResult (index.ToString (), res.engine_result, TextHighlighter.RED);
					return false;

				case "telCAN_NOT_QUEUE":
					this.SetResult (index.ToString (), res.engine_result + " retrying", TextHighlighter.RED);
					goto retry;

				case "telINSUF_FEE_P":

					this.SetResult( index.ToString (), res.engine_result, TextHighlighter.RED );
					return false;


				case "tecNO_ISSUER":
					this.SetResult (index.ToString (), res.engine_result, TextHighlighter.RED);
					return false;

					/*
				case "tecUNFUNDED_OFFER":
					this.setFailed (index.ToString(), res.engine_result);
					return false;
					*/

				default:
					this.SetResult (index.ToString (), "Response not imlemented : " + res.engine_result, TextHighlighter.RED);
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

				this.SetResult (index.ToString (), "EXception Thrown in code", TextHighlighter.RED);
				return false;
				//return false;
			}

		}


		public void SetStatus(string path, string message, string colorName) {
			if (message == null)
				message = "";

			TextHighlighter.Highlightcolor = colorName;
			string s = TextHighlighter.Highlight (/*"Success : " + */message);

			Gtk.Application.Invoke ( (object sender, EventArgs e) => {
				if (listStore.GetIterFromString (out TreeIter iter, path)) {
					listStore.SetValue (iter, 5, s);



				}
			});

		}

		public void SetResult (string path, string message, string colorName) {
			
			TextHighlighter.Highlightcolor = colorName;
			string s = TextHighlighter.Highlight (message ?? "");


			Gtk.Application.Invoke ( (object sender, EventArgs e) => {
				
				if (listStore.GetIterFromString (out TreeIter iter, path)) {
					listStore.SetValue (iter, 6, s);



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

				listStore.AppendValues (payTupe.Item2[i], Destination ?? "", Amount?.ToString () ?? "", Sendmax?.ToString () ?? "");

				//o.selected = true;
			}

		}


		public void SetPayments ( IEnumerable <RipplePaymentTransaction> payments, bool isSelectDefault ) {
			
			int count = payments.Count ();
			RipplePaymentTransaction[] _payments = new RipplePaymentTransaction[ count ];
			bool[] Selections = new bool[ count ];



			listStore.Clear ();

			for (int i = 0; i < count; i++) {
				RippleBinaryObject obj = payments?.ElementAt (i)?.GetBinaryObject ();
				if (obj == null) {
					//TODO
				}
				RipplePaymentTransaction o = 
					new RipplePaymentTransaction( 
					                             
						obj
					);

				string Destination = o?.Destination;
				RippleCurrency Amount = o?.Amount;
				RippleCurrency Sendmax = o?.SendMax;

				listStore.AppendValues (isSelectDefault, i + 1, Destination ?? "", Amount?.ToString() ?? "", Sendmax?.ToString() ?? "");

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

