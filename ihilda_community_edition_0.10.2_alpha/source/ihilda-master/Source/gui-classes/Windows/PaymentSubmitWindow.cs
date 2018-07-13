using System;
using System.Collections.Generic;
using IhildaWallet.Util;
using RippleLibSharp.Transactions.TxTypes;

namespace IhildaWallet
{
	public partial class PaymentSubmitWindow : Gtk.Window
	{
		public PaymentSubmitWindow (RippleWallet rippleWallet, LicenseType licenseType) :
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			if (this.paymentpreviewsubmitwidget1 == null) {
				this.paymentpreviewsubmitwidget1 = new PaymentPreviewSubmitWidget ();
				this.paymentpreviewsubmitwidget1.Show ();
				this.Add (paymentpreviewsubmitwidget1);
			}



			this.paymentpreviewsubmitwidget1.SetRippleWallet(rippleWallet);
			this.paymentpreviewsubmitwidget1.SetLicenseType (licenseType);
		}



		public void SetPayments ( RipplePaymentTransaction payment)
		{
			this.SetPayments ( new RipplePaymentTransaction [] { payment });
		}
		public void SetPayments (IEnumerable <RipplePaymentTransaction> payments) {
			SetPayments (payments,false);
		}
		public void SetPayments ( IEnumerable <RipplePaymentTransaction> payments, bool isSelectDefault ) {
			this.paymentpreviewsubmitwidget1.SetPayments (payments, isSelectDefault);
		}
	}
}

