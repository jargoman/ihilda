using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using IhildaWallet.Networking;
using IhildaWallet.Util;
using RippleLibSharp.Keys;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;
using RippleLibSharp.Transactions.TxTypes;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class TransactionSubmitWidget : Gtk.Bin
	{
		public TransactionSubmitWidget ()
		{
			this.Build ();

			if (walletswitchwidget1 == null) {
				walletswitchwidget1 = new WalletSwitchWidget ();
				walletswitchwidget1.Show ();

				hbox1.Add (walletswitchwidget1);
			}

			walletswitchwidget1.WalletChangedEvent += (source, eventArgs) => {

				RippleWallet rippleWallet = eventArgs.GetRippleWallet ();
				string acc = rippleWallet?.GetStoredReceiveAddress ();
				if (acc == null) {
					return;
				}

				if (_default_transactions != null) {
					foreach (RippleTransaction transaction in _default_transactions) {
						transaction.Account = acc;
					}
				}

				if (_tx_tuple != null) {
					foreach (RippleTransaction transaction in _tx_tuple.Item1) {
						transaction.Account = acc;
					}
				}
			};

			listStore = new ListStore (typeof (bool), typeof (string), typeof (string), typeof (string));


			Gtk.CellRendererToggle toggle = new CellRendererToggle {
				Activatable = true
			};
			toggle.Toggled += ItemToggled;


			CellRendererText txtr = new CellRendererText {
				Editable = false
			};

			treeview1.AppendColumn ("Select", toggle, "active", 0);
			this.treeview1.AppendColumn ("Description", txtr, "markup", 1);
			this.treeview1.AppendColumn ("Status", txtr, "markup", 2);
			this.treeview1.AppendColumn ("Result", txtr, "markup", 3);

			this.submitButton.Clicked += delegate {
				Task.Run ((System.Action)SubmitAll);
			};

			this.resetButton.Clicked += ResetButton_Clicked;
			this.stopButton.Clicked += Stopbutton_Clicked;
			this.removeButton.Clicked += Removebutton_Clicked;

			this.selectButton.Clicked += SelectButton_Clicked;;
		}

		internal void SetLicenseType (LicenseType licenseType)
		{
			this._licenseType = licenseType;
		}

		void SelectButton_Clicked (object sender, EventArgs e)
		{
			bool allselected = true;

			Tuple<RippleTransaction [], bool []> tuple = _tx_tuple;
			foreach (bool b in tuple.Item2) {
				if (!b) {
					allselected = false;
					break;

				}
			}

			for (int i = 0; i < tuple.Item2.Length; i++) {

				tuple.Item2 [i] = !allselected;
			}




			this.RefreshGUI ();
		}


		private void SubmitAll ()
		{

			tokenSource?.Cancel ();
			tokenSource = new CancellationTokenSource ();
			CancellationToken token = tokenSource.Token;

#if DEBUG
			string method_sig = clsstr + nameof (SubmitAll) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PaymentPreviewSubmitWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			RippleWallet rw = walletswitchwidget1.GetRippleWallet ();
			if (rw == null) {
#if DEBUG
				if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
					Logging.WriteLog (method_sig + "w == null, returning\n");
				}
#endif

				// 
				return;
			}

			NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();
			if (ni == null) {
				// TODO network interface
				return;
			}

			bool ShouldContinue = LeIceSense.LastDitchAttempt (rw, _licenseType);
			if (!ShouldContinue) {
				return;
			}



			RippleIdentifier rsa = rw.GetDecryptedSeed ();

			while (rsa.GetHumanReadableIdentifier () == null) {
				bool should = AreYouSure.AskQuestion (
				"Invalid password",
				"Unable to decrypt seed. Invalid password.\nWould you like to try again?"
				);

				if (!should) {
					return;
				}

				rsa = rw.GetDecryptedSeed ();
			}


			for (int index = 0; index < this._tx_tuple.Item1.Length; index++) {


				if (token.IsCancellationRequested) {
					return;
				}


				bool b = this._tx_tuple.Item2 [index];
				if (!b) {
					continue;
				}


				bool suceeded = this.SubmitOrderAtIndex (index, ni, token, rsa);
				if (!suceeded) {
					return;
				}
			}



		}

		private void ItemToggled (object o, ToggledArgs args)
		{
			string path = args.Path;
			int index = Convert.ToInt32 (args.Path);

			if (listStore.GetIterFromString (out TreeIter iter, args.Path)) {
				bool val = (bool)listStore.GetValue (iter, 0);
				listStore.SetValue (iter, 0, !val);


				this._tx_tuple.Item2 [index] = !val;
			}

		}

		public bool SubmitOrderAtIndex (int index, NetworkInterface ni, CancellationToken token, RippleIdentifier rsa)
		{

#if DEBUG
			string method_sig = clsstr + nameof (SubmitOrderAtIndex) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.TransactionSubmitWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			try {



				//
				this.SetStatus (index.ToString (), "Queued", Program.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);

				Tuple<RippleTransaction [], bool []> payTupe = _tx_tuple;

			retry:
				RippleTransaction tx = _tx_tuple.Item1 [index];

				if (tx == null) {
					// TODO
					return false;
				}

				uint sequence = Convert.ToUInt32 (RippleLibSharp.Commands.Accounts.AccountInfo.GetSequence (tx.Account, ni, token));


				SignOptions opts = SignOptions.LoadSignOptions ();
				FeeSettings feeSettings = FeeSettings.LoadSettings ();

				this.SetStatus (index.ToString (), "Requesting Fee", Program.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);

				Tuple<UInt32, UInt32> tupe = feeSettings.GetFeeAndLastLedgerFromSettings (ni, token);


				if (token.IsCancellationRequested) {

					this.SetResult (index.ToString (), "Aborted", TextHighlighter.RED);

					return false;
				}
				//UInt32 f = tupe.Item1; 
				UInt32 f = tupe.Item1;
				tx.fee = f.ToString ();

				tx.Sequence = sequence; // note: don't update se++ with forloop, update it with each payment



				uint lls = 0;
				if (opts != null) {
					lls = opts.LastLedgerOffset;
				}

				if (lls < 5) {
					lls = SignOptions.DEFAUL_LAST_LEDGER_SEQ;
				}


				tx.LastLedgerSequence = tupe.Item2 + lls;

				if (tx.fee.amount == 0 || tx.Sequence == 0) {
					this.SetResult (index.ToString (), "Invalid Fee or Sequence", TextHighlighter.RED);
					throw new Exception ();
				}

				if (opts == null) {
					// TODO get user to choose and save choice
				}


				if (opts.UseLocalRippledRPC) {

					this.SetStatus (index.ToString (), "Signing using rpc", Program.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);
					try {
						tx.SignLocalRippled ( rsa );
					} catch (Exception e) {

#if DEBUG
						if (DebugIhildaWallet.TransactionSubmitWidget) {
							Logging.ReportException (method_sig, e);
						}
#endif

						this.SetResult (index.ToString (), "Error Signing using rpc", TextHighlighter.RED);
						return false;
					}

					this.SetStatus (index.ToString (), "Signed rpc", Program.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);
				} else {

					this.SetStatus (index.ToString (), "Signing using RippleLibSharp", Program.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);
					try {
						if (rsa is RippleSeedAddress) {
							tx.Sign ((RippleSeedAddress)rsa);
						}

						if (rsa is RipplePrivateKey) {
							tx.Sign ((RipplePrivateKey)rsa);
						}

					} catch (Exception e) {

#if DEBUG
						if (DebugIhildaWallet.TransactionSubmitWidget) {
							Logging.ReportException (method_sig, e);
						}
#endif


						this.SetResult ( index.ToString (), "Signing using RippleLibSharp", TextHighlighter.RED );
						return false;
					}
					this.SetStatus (index.ToString (), "Signed RippleLibSharp", Program.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);

				}



				if (token.IsCancellationRequested) {

					this.SetResult (index.ToString (), "Aborted", TextHighlighter.RED);
					
					return false;
				}



				Task<Response<RippleSubmitTxResult>> task = null;

				try {
					task = NetworkController.UiTxNetworkSubmit (tx, ni, token);
					this.SetStatus (index.ToString (), "Submitted via websocket", Program.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);
					task.Wait (token);


				} catch (Exception e) {

					Logging.WriteLog (e.Message);
					this.SetResult (index.ToString (), "Network Error", TextHighlighter.RED);
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
				if (r == null) {

					errorMessage += "(r == null)";
					this.SetResult (index.ToString (), errorMessage, TextHighlighter.RED);
#if DEBUG
					if (DebugIhildaWallet.TransactionSubmitWidget) {
						Logging.WriteLog (errorMessage);
						Logging.WriteLog (tx.ToJson ());
					}
#endif


					return false;

				}


				if (r.status == null) {
					errorMessage += "(r.status == null)";
					this.SetResult (index.ToString (), errorMessage, TextHighlighter.RED);
#if DEBUG
					if (DebugIhildaWallet.TransactionSubmitWidget) {
						Logging.WriteLog (errorMessage);
						Logging.WriteLog (tx.ToJson ());
					}
#endif


					return false;

				}

				if (!r.status.Equals ("success")) {
					errorMessage += "!r.status.Equals (\"success\")";
					this.SetResult (index.ToString (), errorMessage, TextHighlighter.RED);
#if DEBUG
					if (DebugIhildaWallet.TransactionSubmitWidget) {
						Logging.WriteLog (errorMessage);
						Logging.WriteLog (tx.ToJson ());
					}
#endif


					return false;

				}


				RippleSubmitTxResult res = r.result;

				if (res == null) {
					errorMessage += "res == null";
					this.SetResult (index.ToString (), errorMessage, TextHighlighter.RED);
					return false;
				}


				//tefPAST_SEQ


				switch (res.engine_result) {

				case null:
					errorMessage += "res.engine_result = null";
					this.SetResult (index.ToString (), "null", TextHighlighter.RED);
					return false;

				case "terQUEUED":
					Thread.Sleep (1000);
					this.SetResult (index.ToString (), res.engine_result, Program.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);
					return true;

				case "tesSUCCESS":
					this.SetResult (index.ToString (), res.engine_result, Program.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);
					return true;

				case "terPRE_SEQ":
				case "tefPAST_SEQ":
				case "tefMAX_LEDGER":
				case "tecNO_DST_INSUF_XRP":
				case "noNetwork":
					this.SetResult (index.ToString (), res.engine_result, TextHighlighter.RED);
					return false;

				case "telCAN_NOT_QUEUE":
					this.SetResult (index.ToString (), res.engine_result + " retrying", TextHighlighter.RED);
					goto retry;

				case "telINSUF_FEE_P":

					this.SetResult (index.ToString (), res.engine_result, TextHighlighter.RED);
					return false;


				case "tecNO_ISSUER":
					this.SetResult (index.ToString (), res.engine_result, TextHighlighter.RED);
					return false;

				
				case "tecUNFUNDED_OFFER":
					this.SetResult (index.ToString(), res.engine_result, TextHighlighter.RED);
				return false;
				

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
			catch (Exception e) {
#pragma warning restore 0168


#if DEBUG
				if (DebugIhildaWallet.TransactionSubmitWidget) {
					Logging.ReportException (method_sig, e);
				}
#endif

				this.SetResult (index.ToString (), "EXception Thrown in code", TextHighlighter.RED);
				return false;
				//return false;
			}

		}


		public void SetTransactions (IEnumerable<RippleTransaction> transactions)
		{
			SetTransactions (transactions, false);
		}

		public void SetTransactions (IEnumerable<RippleTransaction> transactions, bool isSelectDefault)
		{

			int count = transactions.Count ();
			RippleTransaction [] _transactions = new RippleTransaction [count];
			bool [] Selections = new bool [count];



			listStore.Clear ();

			for (int i = 0; i < count; i++) {
				// TODO deep copy?
				RippleTransaction transaction = transactions?.ElementAt (i);
				if (transaction == null) {
					//TODO
				}

				listStore.AppendValues (isSelectDefault, transaction.ToString ());

				//o.selected = true;

				_transactions [i] = transaction;
				Selections [i] = isSelectDefault;

				Tuple<RippleTransaction [], bool []> tuple
				= new Tuple<RippleTransaction [], bool []> (_transactions, Selections);

				_tx_tuple = tuple;

			}


			treeview1.Model = listStore;

			this.SetDefaultPayments (transactions.ToArray ());
		}

		public void SetDefaultPayments (RippleTransaction [] transactions)
		{
			
			_default_transactions = transactions;
		}
		public void SetIsSubmitted (string _path, string message)
		{
			if (message == null)
				message = "";

			TextHighlighter.Highlightcolor = Program.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN;
			string s = TextHighlighter.Highlight (/*"Success : " + */message);

			Gtk.Application.Invoke ((object sender, EventArgs e) => {
				if (listStore.GetIterFromString (out TreeIter iterater, _path)) {
					listStore.SetValue (iterater, 2, s);



				}
			});

		}

		public void Stopbutton_Clicked (object sender, EventArgs e)
		{
			//this.stop = true;
			tokenSource?.Cancel ();
		}

		#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public CancellationTokenSource tokenSource = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant


		public void Removebutton_Clicked (object sender, EventArgs e)
		{

			Tuple<RippleTransaction [], bool []> oldTuple = _tx_tuple;
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

			List<RippleTransaction> newTransactions = new List<RippleTransaction> ();

			for (int i = 0; i < oldTuple.Item2.Length; i++) {
				if (!oldTuple.Item2 [i]) {
					newTransactions.Add (oldTuple.Item1 [i]);

				}

			}

			bool [] newSelections = new bool [newTransactions.Count ()];


			RippleTransaction [] transactions = newTransactions.ToArray ();
			bool [] selections = newSelections;

			Tuple<RippleTransaction [], bool []> newTuple =
				new Tuple<RippleTransaction [], bool []> (transactions, selections);

			this._tx_tuple = newTuple;

			this.RefreshGUI ();

		}

		public void RefreshGUI ()
		{

			Tuple<RippleTransaction [], bool []> txTupe = _tx_tuple;
			listStore.Clear ();
			if (txTupe == null) {
				// TODO
				return;
			}
			for (int i = 0; i < txTupe.Item1.Length; i++) {
				

				RippleCurrency message = txTupe.Item1 [i].ToString ();


				listStore.AppendValues (txTupe.Item2 [i], message ?? "");

				//o.selected = true;
			}

		}



		void ResetButton_Clicked (object sender, EventArgs e)
		{

			this.SetTransactions (_default_transactions);
		}





		public void SetRippleWallet (RippleWallet rippleWallet)
		{
			//this._rippleWallet = rippleWallet;
			this.walletswitchwidget1.SetRippleWallet (rippleWallet);
		}

		private RippleTransaction [] _default_transactions = null;

		public Tuple<RippleTransaction [], bool []> _tx_tuple {
			get;
			set;
		}

		private LicenseType _licenseType {
			get;
			set;
		}

		public void SetStatus (string path, string message, string colorName)
		{
			if (message == null)
				message = "";

			TextHighlighter.Highlightcolor = colorName;
			string s = TextHighlighter.Highlight (/*"Success : " + */message);

			Gtk.Application.Invoke ((object sender, EventArgs e) => {
				if (listStore.GetIterFromString (out TreeIter iter, path)) {
					listStore.SetValue (iter, 2, s);



				}
			});

		}

		public void SetResult (string path, string message, string colorName)
		{

			TextHighlighter.Highlightcolor = colorName;
			string s = TextHighlighter.Highlight (message ?? "");


			Gtk.Application.Invoke ((object sender, EventArgs e) => {

				if (listStore.GetIterFromString (out TreeIter iter, path)) {
					listStore.SetValue (iter, 3, s);



				}
			});




		}



		ListStore listStore;

		#if DEBUG
		public string clsstr = nameof (TransactionSubmitWidget) + DebugRippleLibSharp.colon;
#endif
	}
}
