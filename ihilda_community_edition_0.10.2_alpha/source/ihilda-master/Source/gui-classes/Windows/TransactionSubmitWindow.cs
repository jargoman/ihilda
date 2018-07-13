using System;
using System.Collections.Generic;
using IhildaWallet.Util;
using RippleLibSharp.Transactions.TxTypes;

namespace IhildaWallet
{
	public partial class TransactionSubmitWindow : Gtk.Window
	{
		public TransactionSubmitWindow (RippleWallet rippleWallet, LicenseType licenseType) :
				base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			if (this.transactionsubmitwidget1 == null) {
				this.transactionsubmitwidget1 = new TransactionSubmitWidget ();
				this.transactionsubmitwidget1.Show ();
				this.vbox1.Add (this.transactionsubmitwidget1);
			}

			this.transactionsubmitwidget1.SetRippleWallet (rippleWallet);
			this.transactionsubmitwidget1.SetLicenseType (licenseType);
		}

		public void SetTransactions (RippleTransaction transaction)
		{
			this.SetTransactions (new RippleTransaction [] { transaction });
		}
		public void SetTransactions (IEnumerable<RippleTransaction> transactions)
		{
			SetTransactions (transactions, false);
		}
		public void SetTransactions (IEnumerable<RippleTransaction> transactions, bool isSelectDefault)
		{
			this.transactionsubmitwidget1.SetTransactions (transactions, isSelectDefault);
		}
	}
}
