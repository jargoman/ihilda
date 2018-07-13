using System;
using RippleLibSharp.Keys;

namespace IhildaWallet
{
	public partial class TxWindow : Gtk.Window
	{
		public TxWindow (RippleAddress ra) :
			base(Gtk.WindowType.Toplevel) 
		{
			this.Build ();


			if (txviewwidget1 == null) {
				txviewwidget1 = new TxViewWidget ();
				txviewwidget1.Show ();

				if (this.txtablabel == null) {
					this.txtablabel = new Gtk.Label ("<b>Transactions</b>") {
						UseMarkup = true
					};
				}
				notebook1.AppendPage (txviewwidget1, this.txtablabel);
			}

			if (orderswidget1 == null) {
				orderswidget1 = new OrdersWidget ();
				orderswidget1.Show ();
				if (this.label39 == null) {
					this.label39 = new Gtk.Label ("<b>Orders Pager</b>") {
						UseMarkup = true
					};
				}
				notebook1.AppendPage ( orderswidget1, this.label39);
			}

			if (orderstreewidget1 == null) {
				orderstreewidget1 = new OrdersTreeWidget ();
				orderstreewidget1.Show ();
				if (label40 == null) {
					label40 = new Gtk.Label ("<b>Orders Tree</b>") {
						UseMarkup = true
					};
				}
				notebook1.AppendPage (orderstreewidget1, this.label40 );
			}

			if (canseltxwidget1 == null) {
				canseltxwidget1 = new CanselTxWidget ();
				canseltxwidget1.Show ();
				if (label43 == null) {
					label43 = new Gtk.Label ("<b>Cancel</b>");
				}
				notebook1.AppendPage (canseltxwidget1, this.label43);
			}


			this.SetRippleAddress (ra);	
			this.txviewwidget1.HideUnusedWidgets ();
		}


		public TxWindow () :
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			this.SetRippleAddress (null);	
		}


		public void SetRippleAddress( RippleAddress ra ) {
			if ( this.txviewwidget1 != null ) this.txviewwidget1.SetRippleAddress (ra);
			if (this.orderswidget1 != null) this.orderswidget1.SetRippleAddress (ra);
			if (this.orderstreewidget1 != null) this.orderstreewidget1.SetRippleAddress (ra);
		}
	}
}

