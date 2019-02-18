using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using Codeplex.Data;
using IhildaWallet.Networking;
using RippleLibSharp.Network;
using RippleLibSharp.Transactions.TxTypes;
using RippleLibSharp.Keys;
using RippleLibSharp.Transactions;
using RippleLibSharp.Binary;
using RippleLibSharp.Result;
using IhildaWallet.Util;
using RippleLibSharp.Util;
using RippleLibSharp.Commands.Accounts;
using System.Collections.Generic;
using static IhildaWallet.MemoCreateDialog;
using RippleLibSharp.Commands.Subscriptions;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class SendRipple : Gtk.Bin
	{
		public SendRipple ()
		{
			this.Build ();

			this.balanceLabel.Text = AddressDisplayWidget.UNSYNCED; // unsynced;

			this.unitsSelectBox.Changed += OnUnitsSelectBoxChanged;

			this.sendNativeButton.Clicked += OnSendNativeButtonClicked;

			this.addmemobutton.Clicked += (object sender, EventArgs e) => {

				SelectableMemoIndice createdMemo = null;
				using (MemoCreateDialog memoCreateDialog = new MemoCreateDialog ()) {
					try {
						ResponseType resp = (ResponseType)memoCreateDialog.Run ();


						if (resp != ResponseType.Ok) {

							return;
						}
						createdMemo = memoCreateDialog.GetMemoIndice ();
						this.AddMemo (createdMemo);
					} catch (Exception ee) {
						throw ee;
					} finally {
						memoCreateDialog?.Destroy ();
					}
				}

				
				
				
			};


			clearmemobutton.Clicked += (object sender, EventArgs e) => {
				ListStore.Clear ();

				Memos = null;

			};

			Task.Factory.StartNew ( () => {
				var token = TokenSource.Token;
				while (!token.IsCancellationRequested) {
					if (TokenSource == null) {
						return;
					}

					for (int i = 0; i < 5; i++) {
						WaitHandle.WaitAny (
							new WaitHandle [] { 
								LedgerTracker.LedgerResetEvent, 
								token.WaitHandle 
							}, 
							6000
			    			);
					}

					//await Task.Delay (30000, token);
					Sync ();
				}
			}
			);

			CellRendererToggle rendererToggle = new CellRendererToggle () { 
				Activatable = true
			};

			CellRendererText cellRendererText = new CellRendererText ();

			treeview1.AppendColumn ("Enabled", rendererToggle, "active", 0);
			treeview1.AppendColumn ("MemoType", cellRendererText, "text", 1);
			treeview1.AppendColumn ("MemoFormat", cellRendererText, "text", 2);
			treeview1.AppendColumn ("MemoData", cellRendererText, "text", 3);

			ListStore = new ListStore (
					typeof (bool),
					typeof (string),
		    			typeof (string),
					typeof (string)
				);


			var memo = Program.GetClientMemo ();
			this.AddMemo (memo);
		}

		Gtk.ListStore ListStore {
			get;
			set;
		}

		~SendRipple ()
		{
			TokenSource?.Cancel ();
			TokenSource = null;
		}

		private CancellationTokenSource TokenSource = new CancellationTokenSource();

		//String unsynced = "   --   unsynced   --   ";

		public void SendNativePayment (String destination, UInt32? DestTag, Decimal nativeamount)
		{

			nativeamount = nativeamount * 1000000m; // convert to drops
			ulong lamount;
			try {

				lamount = (ulong)nativeamount;

			}

#pragma warning disable 0168
			catch (OverflowException ex) {
#pragma warning restore 0168

#if DEBUG
				if (DebugIhildaWallet.SendRipple) {
					string message = "OverflowException : can't convert " + RippleCurrency.NativeCurrency + " to " + RippleCurrency.NativePip + " because value can't fit inside unsigned long";
					Logging.ReportException (message, ex);
				}
#endif

				MessageDialog.ShowMessage ("OverflowException : can't convert " + RippleCurrency.NativeCurrency + " to " + RippleCurrency.NativePip + " because value can't fit inside unsigned long");

				return;
			}

#pragma warning disable 0168
			catch (Exception ex) {
#pragma warning restore 0168

#if DEBUG
				if (DebugIhildaWallet.SendRipple) {
					Logging.WriteLog ("Exception thrown while converting to drops : " + ex.Message);
				}
#endif

				MessageDialog.ShowMessage ("Exception thrown while converting to drops : " + ex.Message);
				return;
			}


			SendPipsPayment (destination, DestTag, (Decimal)lamount);



		}


		private static void SendPipsUsingPaymentManager (RippleWallet rw, string destination, UInt32? DestTag, Decimal dropsamount, IEnumerable<MemoIndice> memoIndices)
		{

			RippleAddress payee = new RippleAddress (destination);
			RippleCurrency amnt = new RippleCurrency (dropsamount);


			RipplePaymentTransaction tx =
				new RipplePaymentTransaction (
					rw.GetStoredReceiveAddress (),
					payee,
					amnt,
					null
				) {
					DestinationTag = DestTag,
		    			Memos = memoIndices?.ToArray()
				};

			RipplePaymentTransaction [] arr = { tx };


			Application.Invoke (
				delegate {
					PaymentSubmitWindow paymentWindow = new PaymentSubmitWindow (rw, LicenseType.NONE);
					paymentWindow.SetPayments (arr);
					paymentWindow.Show ();
				}
			);
		}

		public void SendPipsPayment (String destination, UInt32? DestTag, Decimal dropsamount)
		{
#if DEBUG
			if (DebugIhildaWallet.SendRipple) {
				Logging.WriteLog ("send drops payment of " + dropsamount.ToString () + " drops");
			}
#endif


			RippleWallet rw = _rippleWallet;
			if (rw == null) {
				return;
			}


	    		IEnumerable <MemoIndice> memoIndices = Memos?.Where((SelectableMemoIndice arg) => arg.IsSelected);
			SendPipsUsingPaymentManager (rw, destination, DestTag, dropsamount, memoIndices);

			/*
			if (rw.seed == null) {
				// TODO 
			}
			*/

			/*
			bool cont = LeIceSense.doPurchaceDialog (
				rw,
				LicenseType.PAYMENT
			);

			if (!cont) return;
			*/


			/*
			NetworkInterface ni = Neworking.NetworkController.getNetworkInterfaceGuiThread ();
			if (ni == null) {
				NetworkController.doNetworkingDialogNonGUIThread ();
				return;
			}
			*/





			/*
			SignOptions opts = SignOptions.loadSignOptions();
			uint lls = 0;
			if (opts != null) {
				lls = opts.lastLedgerOffset;
			}

			if (lls < 5) {
				lls = SignOptions.DEFAUL_LAST_LEDGER_SEQ;
			}



			tx.autoRequestSequence ( rw.getStoredReceiveAddress(), ni );

			Tuple<UInt32,UInt32> f = FeeSettings.getFeeAndLastLedgerFromSettings ( ni );
			if (f == null) {
				return;
			}
			tx.fee = f.Item1.ToString ();
			tx.LastLedgerSequence = f.Item2 + lls;

			RippleSeedAddress rsa = rw.getDecryptedSeed ();

			if (opts.useLocalRippledRPC) {
				Logging.writeLog("Signing using rpc");
				tx.signLocalRippled (rsa);
				Logging.writeLog ("Signed rpc");
			}

			else {
				Logging.writeLog("Signing using RippleLibSharp");
				tx.sign(rsa);
				Logging.writeLog("Signed RippleLibSharp");

			}

			Task< Response <RippleSubmitTxResult>> task = NetworkController.uiTxNetworkSubmit ( tx, ni );
			task.Wait ();
			*/

		}



		protected void OnSendNativeButtonClicked (object sender, EventArgs eventArgs)
		{

#if DEBUG
			string method_sig = nameof (OnSendNativeButtonClicked) + DebugRippleLibSharp.left_parentheses + nameof (sender) + DebugRippleLibSharp.comma + nameof (eventArgs) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.SendRipple) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}

#endif


			//RippleWallet rw = MainWindow.currentInstance.getRippleWallet();


			Thread th = new Thread (new ParameterizedThreadStart (SendThread));

			String amount = this.amountcomboboxentry.ActiveText;        //this.amountEntry.Text; // used to be a text entry
			String destination = this.destinationcomboboxentry.ActiveText;      //this.destinationentry.Text;


			if (destination == null) {

				return;
			}

			string destinationTag = destinationTagcomboboxentry?.ActiveText;

			//uint? res = 0;
			uint? notDestTag = null;


			if (!string.IsNullOrWhiteSpace (destinationTag)) {
				destinationTag = destinationTag.Trim ();
				//bool hasDest = uint.TryParse (destinationTag, out res);

				notDestTag = uint.TryParse (destinationTag, out uint resul) ? new uint? (resul) : null;

				if (notDestTag == null) {
					MessageDialog.ShowMessage ("Invalid DestTag", "Could not parse destination tag. Must be an integer");
					return;
				}

				if (notDestTag == 0) {
					string msg = "<span fgcolor>You've specified a destination tag of zero</span>. A destination tag is used by the recipiant to distinguisg payments from one another. Ask your recepient what destination tag if any to use\nWould you like to continue with a destination tag of zero?";
					bool b = AreYouSure.AskQuestion ("destination tag is zero", msg);

					if (!b) {
						return;
					}
				}

				//notDestTag = new uint? (res); // NECESSARY !!! wouldn't implicitly cast

			}


#if DEBUG
			if (DebugIhildaWallet.SendRipple) {
				Logging.WriteLog ("SendRipple.OnSendNativeButtonClicked : destination = " + destination);
			}
#endif




			String units = this.unitsSelectBox.ActiveText;

			ThreadParam tp = new ThreadParam (amount, destination, notDestTag, units);

			th.Start (tp);

		}

		private class ThreadParam
		{
			public ThreadParam (String amount, String destination, uint? destinationTag, String units)
			{
				this.amount = amount;
				this.destination = destination;


				this.units = units;

				this.DestTag = destinationTag;
			}

			public String amount;
			public String destination;


			public String units;

			public uint? DestTag;
		}

		public void SetMemos (IEnumerable<SelectableMemoIndice> Memos)
		{
			Gtk.Application.Invoke (
				delegate {
					ListStore.Clear ();

					foreach (SelectableMemoIndice memoIndice in Memos) {
						ListStore.AppendValues (
							memoIndice.IsSelected,
							memoIndice?.GetMemoTypeAscii(),
							memoIndice?.GetMemoFormatAscii(),
							memoIndice?.GetMemoDataAscii()
						);
					}

					this.Memos = Memos;
					this.treeview1.Model = ListStore;

				}
			);

		}

		private IEnumerable<SelectableMemoIndice> Memos {
			get;
			set;
		}

		public void AddMemo (SelectableMemoIndice indice)
		{
			List<SelectableMemoIndice> memoIndices = Memos?.ToList() ?? new List<SelectableMemoIndice>();
			indice.IsSelected = true;
			memoIndices.Add (indice);

			SetMemos (memoIndices);

		}

		private void SendThread (object param)
		{



#if DEBUG
			string method_sig = clsstr + nameof (SendThread) + DebugRippleLibSharp.left_parentheses + nameof (param) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.SendRipple) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			if (!(param is ThreadParam tp)) {
				throw new InvalidCastException ("Unable to cast object to type " + nameof (ThreadParam));
			}


#if DEBUG
			if (DebugIhildaWallet.SendRipple) {

				Logging.WriteLog("Units" + DebugRippleLibSharp.equals + (tp.units ?? "null"));

				Logging.WriteLog(method_sig + "requesting Server Info\n");
			}
#endif

			//ServerInfoWidget.refresh_blocking();

#if DEBUG
			if (DebugIhildaWallet.SendRipple) {
				// TODO uncomment
				//Logging.WriteLog(method_sig + "refresh_blocking returned, ServerInfo.transaction_fee = " + ServerInfo.serverInfo.transaction_fee.ToString());
			}
#endif




			if (RippleCurrency.NativePip.Equals (tp.units)) {
				Logging.WriteLog (RippleCurrency.NativePip);


				ulong? amountl = RippleCurrency.ParseUInt64 (tp.amount);
				if (amountl != null) {



					SendPipsPayment (tp.destination, tp.DestTag,(decimal)amountl);
				} else {
					MessageDialog.ShowMessage ("Amount of " + RippleCurrency.NativePip + " is formatted incorrectly \n");
				}
				return;


			}

			if (RippleCurrency.NativeCurrency.Equals (tp.units)) {
				Logging.WriteLog (RippleCurrency.NativeCurrency);

				Decimal? amountd = RippleCurrency.ParseDecimal (tp.amount);
				if (amountd != null) {

					SendNativePayment (tp.destination, tp.DestTag, (Decimal)amountd);
				} else {
					MessageDialog.ShowMessage ("Amount Entered is formatted incorrectly\n");
				}
				return;

			}

			Logging.WriteConsole ("Please specify a currency unit, eg. \"drops\"");
			return;




		}


		public void SetPipBalance (decimal balance)
		{

			Application.Invoke (delegate {

				if (this.unitsSelectBox?.ActiveText == null) {
					// Todo debuging

					return;
				}

				if ("".Equals (this.unitsSelectBox?.ActiveText?.Trim ())) {

					// todo debug
					return;
				}

				if (RippleCurrency.NativePip.Equals (unitsSelectBox.ActiveText)) {
					this.balanceLabel.Text = Base58.TruncateTrailingZerosFromString (balance.ToString ());
					return;
				}

				if (RippleCurrency.NativeCurrency.Equals (unitsSelectBox.ActiveText)) {
					this.balanceLabel.Text = Base58.TruncateTrailingZerosFromString ((balance / 1000000.0m).ToString ());
					return;
				}


				// todo debugging
				return;

			}
				);

		}


		protected void OnUnitsSelectBoxChanged (object sender, EventArgs e)
		{

			// TODO set balance
			if (AddressDisplayWidget.UNSYNCED.Equals (this.balanceLabel.Text)) {
				// TODO ??



				return;
			}

			Sync ();

			/*
			if (PaymentWindow.currentInstance != null) {

				this.SetPipBalance ( PaymentWindow.currentInstance.dropBalance );
				return;
			}
			*/
			//
		}

		protected void OnAmountEntryActivated (object sender, EventArgs e)
		{

			if (this.destinationcomboboxentry == null) {
				return;
			}

			this.destinationcomboboxentry.GrabFocus ();

		}

		protected void OnDestinationentryActivated (object sender, EventArgs e)
		{
			if (this.sendNativeButton == null) {
				// bad news this bug would be :/
				return;
			}

			this.sendNativeButton.GrabFocus ();
		}

		public void ClearUI ()
		{

			Application.Invoke (delegate {

				balanceLabel.Text = "";

			});


		}

		public void Sync ()
		{


#if DEBUG
			string method_sig = nameof (Sync) + DebugRippleLibSharp.both_parentheses;
#endif

			string account = _rippleWallet?.Account;
			if (account == null) {
				ClearUI ();
				return;
			}

			NetworkInterface networkInterface = NetworkController.CurrentInterface;
			if (networkInterface == null) {
				//ClearUI ();
				return;
			}

			try {

				Task<Response<AccountInfoResult>> task = AccountInfo.GetResult (account, networkInterface, TokenSource.Token);

				if (task == null) {
					return;
				}

				task.Wait (TokenSource.Token);



				Response<AccountInfoResult> response = task.Result;
				if (response == null) {
					return;
				}

				if (response.HasError ()) {
					return;
				}

				AccountInfoResult accountTxResult = response.result;
				if (accountTxResult == null) {
					return;
				}



				RippleCurrency rippleCurrency = accountTxResult.GetNativeBalance ();

				this.SetPipBalance (rippleCurrency.amount);

			} catch (Exception e) {
				ClearUI ();

#if DEBUG
				Logging.ReportException (method_sig, e);
#endif

			}

		}

		public void SetRippleWallet (RippleWallet rippleWallet)
		{
			if (rippleWallet == null || !rippleWallet.Equals(_rippleWallet)) {
				ClearUI ();
			}
			this._rippleWallet = rippleWallet;
		}

		private RippleWallet _rippleWallet {
			get;
			set;
		}



#if DEBUG
		const string clsstr = nameof (SendRipple) + DebugRippleLibSharp.colon;
#endif

	}




}

