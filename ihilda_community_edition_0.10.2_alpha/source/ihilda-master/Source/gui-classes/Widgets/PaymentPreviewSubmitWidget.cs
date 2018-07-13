using System;
using System.Threading.Tasks;
using RippleLibSharp.Transactions.TxTypes;
using RippleLibSharp.Network;
using RippleLibSharp.Keys;
using IhildaWallet.Networking;
using RippleLibSharp.Util;
using System.Collections.Generic;
using IhildaWallet.Util;
using System.Linq;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PaymentPreviewSubmitWidget : Gtk.Bin
	{
		public PaymentPreviewSubmitWidget ()
		{
			this.Build ();

			if (paymentstree1 == null) {
				paymentstree1 = new PaymentsTree ();
				paymentstree1.Show ();

				vbox2.Add (paymentstree1);
			}

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

				if (_default_payments != null) {
					foreach (RipplePaymentTransaction payment in _default_payments) {
						payment.Account = acc;
					}
				}

				if (paymentstree1._payments_tuple != null) {
					foreach (RipplePaymentTransaction payment in paymentstree1._payments_tuple.Item1) {
						payment.Account = acc;
					}
				}
			};

			this.submitButton.Clicked += delegate {
				Task.Run ((Action)SubmitAll);
			};

			this.selectButton.Clicked += paymentstree1.Selectbutton_Clicked;

			this.removeButton.Clicked += paymentstree1.Removebutton_Clicked;

			this.resetButton.Clicked += ResetButton_Clicked;

			this.stopButton.Clicked += paymentstree1.Stopbutton_Clicked;
		}

		void ResetButton_Clicked (object sender, EventArgs e)
		{
			
			this.paymentstree1.SetPayments (_default_payments);
		}





		internal void SetLicenseType (LicenseType licenseType)
		{
			this._licenseType = licenseType;
		}

		public void SubmitAll ()
		{
			
#if DEBUG
			string method_sig = clsstr + nameof (SubmitAll) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PaymentPreviewSubmitWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			RippleWallet rw = walletswitchwidget1.GetRippleWallet();
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

			uint se = Convert.ToUInt32 (RippleLibSharp.Commands.Accounts.AccountInfo.GetSequence ( rw.GetStoredReceiveAddress (), ni) );


			RippleSeedAddress rsa = rw.GetDecryptedSeed ();
			for (int index = 0; index < this.paymentstree1._payments_tuple.Item1.Length; index++) {


				if (this.paymentstree1.stop) {
					this.paymentstree1.stop = false;
					return;
				}


				bool b = this.paymentstree1._payments_tuple.Item2 [index];
				if (!b) {
					continue;
				}


				bool suceeded = this.paymentstree1.SubmitOrderAtIndex (index, se++, ni, rsa);
				if (!suceeded) {
					return;
				}
			}





		}




		public void SetPayments (IEnumerable <RipplePaymentTransaction> payments)
		{
			SetPayments (payments, false);
		}
		public void SetPayments (IEnumerable <RipplePaymentTransaction> payments, bool isSelectDefault)
		{
			this.paymentstree1.SetPayments (payments, isSelectDefault);
			this.SetDefaultPayments (payments.ToArray());
		}

		private RipplePaymentTransaction [] _default_payments = null;

		public void SetDefaultPayments (RipplePaymentTransaction [] payments)
		{
			_default_payments = payments;
		}

		public void SetRippleWallet (RippleWallet rippleWallet)
		{
			
			this.walletswitchwidget1.SetRippleWallet (rippleWallet);
		}

		//private RippleWallet _rippleWallet = null;

		private LicenseType _licenseType {
			get;
			set;
		}

#if DEBUG
		public string clsstr = nameof (PaymentPreviewSubmitWidget) + DebugRippleLibSharp.colon;
#endif
	}
}

