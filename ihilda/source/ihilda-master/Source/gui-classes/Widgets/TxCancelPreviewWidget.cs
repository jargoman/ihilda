using System;
using RippleLibSharp.Transactions;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class TxCancelPreviewWidget : Gtk.Bin
	{
		public TxCancelPreviewWidget ()
		{
			this.Build ();
		}

		public void setOffers (Offer[] offers) {
			setOffers (offers);
		}

		/*
		public void setOffers ( Offer[] offers, bool isSelectDefault ) {
			this.openorderstree1.setOffers ();
			//this.paymentstree1.setPayments (payments, isSelectDefault);
		}
		*/




		private Offer[] _default_offers;

		public void setDefaultPayments (Offer[] offers) {
			_default_offers = offers;
		}
	}
}

