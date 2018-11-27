/*
 *	License : Le Ice Sense 
 */


using Gtk;
using System;
using System.Threading;
using System.Threading.Tasks;
using IhildaWallet;
using IhildaWallet.Util;
using RippleLibSharp.Keys;
using RippleLibSharp.Result;

using RippleLibSharp.Commands.Accounts;

using RippleLibSharp.Network;
using IhildaWallet.Networking;

using RippleLibSharp.Transactions;
using RippleLibSharp.Transactions.TxTypes;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SendIce : Gtk.Bin
	{
		public SendIce ()
		{

			this.Build ();

			this.issuerLabel.Text = LeIceSense.LICENSE_ISSUER;
			this.label1.Text = "<u>" + LeIceSense.LICENSE_CURRENCY + "</u>";

			this.label1.UseMarkup = true;

			this.sendICEButton.Clicked += OnSendICButtonClicked;


		}

		protected void OnSendICButtonClicked (object sender, EventArgs e)
		{
			#if DEBUG
			string method_sig = clsstr + "OnSendICButtonClicked : ";
			if (DebugIhildaWallet.SendIce) {

			}
			#endif

			RippleWallet rw = _rippleWallet;
			if (rw == null) {
				// TODO
				return;
			}

			string sendingAccount = rw.GetStoredReceiveAddress ();

			string amount = amountentry?.Text;
			string destination = destinationentry?.Text;

			if (amount == null || amount.Trim().Equals("")) {
				MessageDialog.ShowMessage ("You must specify an amount to send");
				return;
			}


			if (destination == null || destination.Trim().Equals("")) {
				MessageDialog.ShowMessage ("You must specify a destination address");
				return;
			}

			Decimal? dee = RippleCurrency.ParseDecimal (amount);  





			if (dee == null) {
				MessageDialog.ShowMessage ( "Send Amount is not a valid Decimal" );
				return;
			} 


			if (dee < 0) {
				MessageDialog.ShowMessage("Sending negative amounts is not supported. Please enter a valid amount");
				return;
			}



			if (destination == null || destination.Trim().Equals("")) {
				MessageDialog.ShowMessage ("You must specify a destination address : rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
				return;
			}

			destination = destination.Trim ();

			try {
				#pragma warning disable 0219
				RippleAddress destcheck = new RippleAddress( destination );
				#pragma warning restore 0219

			}

#pragma warning disable 0168
			catch (Exception exce) {
#pragma warning restore 0168
#if DEBUG
				if (DebugIhildaWallet.SendIce) {
					Logging.ReportException(method_sig, exce);
				}
#endif

				// it'll never be null but an exception was thrown with destination in the try block
				MessageDialog.ShowMessage ("Invalid destination address : " + destination ?? "null" ); 
					 
				return;
			}

			RippleAddress payee = new RippleAddress(destination);
			RippleCurrency sendMax = null;

			RippleCurrency amnt = null;
			try {
				amnt = new RippleCurrency((Decimal)dee, destination, LeIceSense.LICENSE_CURRENCY);
				sendMax = new RippleCurrency ((Decimal)dee, sendingAccount, LeIceSense.LICENSE_CURRENCY);
			}

			#pragma warning disable 0168
			catch (Exception ex) {
				#pragma warning restore 0168

				#if DEBUG
				if (DebugIhildaWallet.SendIce) {
					Logging.ReportException(method_sig, ex);
				}
				#endif
			}



			RipplePaymentTransaction tx = 
				new RipplePaymentTransaction(
					rw.GetStoredReceiveAddress(), 
					payee,
					amnt,
					sendMax
				); 

			RipplePaymentTransaction[] arr = { tx } ;



			PaymentSubmitWindow paymentWindow = new PaymentSubmitWindow (rw, LicenseType.NONE);
			paymentWindow.SetPayments( arr);
			paymentWindow.Show ();



		}


		public void UpdateBalance () {

			Task.Run (() => {

				string ra = _rippleWallet?.GetStoredReceiveAddress ();
				if (ra == null) {
					return;
				}

				NetworkInterface ni = NetworkController.CurrentInterface;
				if (ni == null) {
					return;
				}

			Task<Response<AccountLinesResult>> task = AccountLines.GetResult (
				ra,
				ni,
				new CancellationToken ()
				);

				if (task == null) {
					return;
				}

				task.Wait ();

				Response<AccountLinesResult> resp = task?.Result;

				AccountLinesResult result = resp?.result;

				if (result == null) {
					return;
				}

				RippleCurrency rc = result.GetBalanceAsCurrency (
					LeIceSense.LICENSE_CURRENCY,
					LeIceSense.LICENSE_ISSUER
				);

				if (rc == null) {
					return;
				}
				Application.Invoke (
					delegate {
						this.balancelabel.Text = rc.amount.ToString ();
						//this.issuerLabel.Text = LeIceSense.LICENSE_ISSUER;
					}

				);


			});




		}

		public void SetRippleWallet (RippleWallet rippleWallet)
		{
			this._rippleWallet = rippleWallet;
			UpdateBalance ();
		}

		private RippleWallet _rippleWallet {
			get;
			set;
		}


		#if DEBUG
		private const string clsstr = nameof (SendIce) + DebugRippleLibSharp.colon;
		#endif
	}
}

