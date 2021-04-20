using System;
using RippleLibSharp.Keys;

namespace IhildaWallet
{
	public partial class TxWindow : Gtk.Window
	{
		public TxWindow (RippleWallet rippleWallet) :
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
			if (ProgramVariables.usePager) {
				if (orderswidget1 == null) {
					orderswidget1 = new OrdersWidget ();
					orderswidget1.Show ();
					if (this.label50 == null) {
						this.label50 = new Gtk.Label ("<b>Orders Pager</b>") {
							UseMarkup = true
						};
					}
					notebook1.AppendPage (orderswidget1, this.label50);
				}

				orderstreewidget1.Hide ();
				label52.Hide ();
			} else {

				if (orderstreewidget1 == null) {
					orderstreewidget1 = new OrdersTreeWidget ();
					orderstreewidget1.Show ();
					if (label52 == null) {
						label52 = new Gtk.Label ("<b>Orders Tree</b>") {
							UseMarkup = true
						};
					}
					notebook1.AppendPage (orderstreewidget1, this.label52);

				}

				orderswidget1.Hide ();
				label50.Hide ();
			}

			if (canceltxwidget1 == null) {
				canceltxwidget1 = new CancelTxWidget ();
				canceltxwidget1.Show ();
				if (label56 == null) {
					label56 = new Gtk.Label ("<b>Cancel</b>");
				}
				notebook1.AppendPage (canceltxwidget1, this.label56);
			}


			this.SetRippleWallet (rippleWallet);	
			this.txviewwidget1.HideUnusedWidgets ();
		}


		public TxWindow () :
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			this.SetRippleWallet (null);	
		}


		public void SetRippleWallet( RippleWallet rippleWallet ) {
			if ( this.txviewwidget1 != null ) this.txviewwidget1.SetRippleWallet (rippleWallet);
			if (this.orderswidget1 != null) this.orderswidget1.SetRippleWallet (rippleWallet);
			if (this.orderstreewidget1 != null) this.orderstreewidget1.SetRippleWallet(rippleWallet);
			if (this.canceltxwidget1 != null) this.canceltxwidget1.SetRippleWallet (rippleWallet); 
			//this.orderstreewidget1.SetRipples ();
		}
	}
}

