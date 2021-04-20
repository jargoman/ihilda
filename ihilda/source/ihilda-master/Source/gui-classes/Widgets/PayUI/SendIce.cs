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
using static IhildaWallet.MemoCreateDialog;
using System.Collections.Generic;
using System.Linq;
using RippleLibSharp.Commands.Subscriptions;

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
					UpdateBalance ();
				}
			}
			);



			button114.Clicked += PercentageClicked;
			button115.Clicked += PercentageClicked;
			button111.Clicked += PercentageClicked;
			button112.Clicked += PercentageClicked;

			button123.Clicked += PercentageClicked;
			button113.Clicked += PercentageClicked;
			button119.Clicked += PercentageClicked;

			button116.Clicked += PercentageClicked;

			button120.Clicked += PercentageClicked;

			button117.Clicked += PercentageClicked;

			button118.Clicked += PercentageClicked;

			button121.Clicked += PercentageClicked;

			button122.Clicked += PercentageClicked;
			hscale2.ValueChanged += Hscale2_ValueChanged;
		}

		~SendIce ()
		{
			TokenSource.Cancel ();
			TokenSource.Dispose ();
		}

		void Hscale2_ValueChanged (object sender, EventArgs e)
		{
#if DEBUG
			string method_sig = clsstr + nameof (Hscale2_ValueChanged) + DebugRippleLibSharp.colon;
#endif
			double val = hscale2.Value;



			//String text2 = this.pricecomboboxentry.ActiveText;




			Task.Run (delegate {
				ScaleMethod (val);
			});

		}


		public void ScaleMethod (double value)
		{
#if DEBUG
			string method_sig = clsstr + nameof (ScaleMethod) + DebugRippleLibSharp.colon;

#endif

			string acc = _rippleWallet.Account;



			NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();

			CancellationTokenSource tokenSource = new CancellationTokenSource ();
			CancellationToken token = tokenSource.Token;


			var cur = AccountLines.GetBalanceForIssuer
				(
					LeIceSense.LICENSE_CURRENCY,
					LeIceSense.LICENSE_ISSUER,
					acc,
					ni,
					token
				);



			double bal = (double)cur.amount;

			double res = bal * value / 100;


			var ss = res.ToString ();
			Gtk.Application.Invoke (
			delegate {

				amountentry.Entry.Text = ss;
			});



		}


		void PercentageClicked (object sender, EventArgs e)
		{
			if (sender is Button b) {
				string s = b?.Label.TrimEnd ('%');
				double d = Convert.ToDouble (s);

				hscale2.Value = d;
			}
		}


		private CancellationTokenSource TokenSource = new CancellationTokenSource ();












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

			string amount = amountentry?.Entry.Text;
			string destination = destinationentry?.Text;

			if (amount == null || amount.Trim().Equals("")) {
				MessageDialog.ShowMessage ("You must specify an amount to send");
				return;
			}


			if (destination == null || destination.Trim().Equals("")) {
				MessageDialog.ShowMessage ("You must specify a destination address");
				return;
			}


			string destinationTag = destinationTagcomboboxentry?.ActiveText;

			uint? DestTag = null;
			UInt32 res = 0;

			if (!string.IsNullOrWhiteSpace (destinationTag)) {
				destinationTag = destinationTag.Trim ();

				bool hasDest = UInt32.TryParse (destinationTag, out res);
				if (!hasDest) {
					return;
				}

				if (res == 0) {
					string msg = "<span fgcolor>You've specified a destination tag of zero</span>. A destination tag is used by the recipiant to distinguisg payments from one another. Ask your recepient what destination tag if any to use\nWould you like to continue with a destination tag of zero?";
					bool b = AreYouSure.AskQuestion ("destination tag is zero", msg);

					if (!b) {
						return;
					}
				}
				DestTag = new uint? (res);

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



			if (string.IsNullOrWhiteSpace(destination)) {
				MessageDialog.ShowMessage ("You must specify a destination address : rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
				return;
			}

			destination = destination?.Trim ();

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
				amnt = new RippleCurrency((Decimal)dee, destination ,LeIceSense.LICENSE_CURRENCY);
				sendMax = new RippleCurrency ((Decimal)dee, LeIceSense.LICENSE_ISSUER, LeIceSense.LICENSE_CURRENCY);
			}

			#pragma warning disable 0168
			catch (Exception ex) {
				#pragma warning restore 0168

				#if DEBUG
				if (DebugIhildaWallet.SendIce) {
					Logging.ReportException(method_sig, ex);
				}
#endif

				throw ex;
			}



			RipplePaymentTransaction tx =
				new RipplePaymentTransaction (
					rw.GetStoredReceiveAddress (),
					payee,
					amnt,
					sendMax
				) {
					DestinationTag = DestTag
				};

			tx.Memos = memowidget1.HasSelectedMemos() ? memowidget1.GetSelectedMemos ().ToArray() : null;  //Memos?.Where ((SelectableMemoIndice arg) => arg.IsSelected).ToArray();

			RipplePaymentTransaction [] arr = { tx } ;



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

