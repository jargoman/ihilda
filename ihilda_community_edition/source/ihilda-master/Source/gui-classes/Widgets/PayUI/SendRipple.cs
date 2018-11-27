using System;
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

			Task.Factory.StartNew (async () => {

				while (TokenSource?.IsCancellationRequested != true) {
					await Task.Delay (30000, TokenSource.Token);
					Sync ();
				}
			}
			);

		}

		~SendRipple ()
		{
			TokenSource.Cancel ();
		}

		private CancellationTokenSource TokenSource = null;

		//String unsynced = "   --   unsynced   --   ";

		public void SendNativePayment (String destination, Decimal nativeamount)
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
					Logging.ReportException(message, ex);
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
					Logging.WriteLog("Exception thrown while converting to drops : " + ex.Message);
				}
#endif

				MessageDialog.ShowMessage ("Exception thrown while converting to drops : " + ex.Message);
				return;
			}


			SendPipsPayment (destination, (Decimal)lamount);



		}


		private static void SendPipsUsingPaymentManager (RippleWallet rw, string destination, Decimal dropsamount)
		{

			RippleAddress payee = new RippleAddress (destination);
			RippleCurrency amnt = new RippleCurrency (dropsamount);


			RipplePaymentTransaction tx =
				new RipplePaymentTransaction (
					rw.GetStoredReceiveAddress (),
					payee,
					amnt,
					null
				);

			RipplePaymentTransaction [] arr = { tx };


			Application.Invoke (
				delegate {
					PaymentSubmitWindow paymentWindow = new PaymentSubmitWindow (rw, LicenseType.NONE);
					paymentWindow.SetPayments (arr);
					paymentWindow.Show ();
				}
			);
		}

		public void SendPipsPayment (String destination, Decimal dropsamount)
		{
#if DEBUG
			if (DebugIhildaWallet.SendRipple) {
				Logging.WriteLog("send drops payment of " + dropsamount.ToString() + " drops");
			}
#endif


			RippleWallet rw = _rippleWallet;
			if (rw == null) {
				return;
			}

			SendPipsUsingPaymentManager (rw, destination, dropsamount);

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


#if DEBUG
			if (DebugIhildaWallet.SendRipple) {
				Logging.WriteLog("SendRipple.OnSendNativeButtonClicked : destination = " + destination);
			}
#endif




			String units = this.unitsSelectBox.ActiveText;

			ThreadParam tp = new ThreadParam (amount, destination, units);

			th.Start (tp);

		}

		private class ThreadParam
		{
			public ThreadParam (String amount, String destination, String units)
			{
				this.amount = amount;
				this.destination = destination;


				this.units = units;
			}

			public String amount;
			public String destination;


			public String units;
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



					SendPipsPayment (tp.destination, (decimal)amountl);
				} else {
					MessageDialog.ShowMessage ("Amount of " + RippleCurrency.NativePip + " is formatted incorrectly \n");
				}
				return;


			}

			if (RippleCurrency.NativeCurrency.Equals (tp.units)) {
				Logging.WriteLog (RippleCurrency.NativeCurrency);

				Decimal? amountd = RippleCurrency.ParseDecimal (tp.amount);
				if (amountd != null) {

					SendNativePayment (tp.destination, (Decimal)amountd);
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

