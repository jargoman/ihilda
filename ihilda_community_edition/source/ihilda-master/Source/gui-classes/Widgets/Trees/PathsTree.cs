using System;
using Gtk;
using RippleLibSharp.Result;
using RippleLibSharp.Commands.Stipulate;
using RippleLibSharp.Paths;
using RippleLibSharp.Transactions;
using System.Linq;
using RippleLibSharp.Transactions.TxTypes;
using IhildaWallet.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PathsTree : Gtk.Bin
	{
		public PathsTree ()
		{
			this.Build ();


			liststore = new ListStore ( typeof (string), typeof (string), typeof(string) );

			this.treeview2.AppendColumn("Amount", new CellRendererText(), "text", 0 );
			this.treeview2.AppendColumn("Currency", new CellRendererText(), "text", 1 );
			this.treeview2.AppendColumn("Issuer", new CellRendererText(), "text", 2 );

			this.treeview2.Model = liststore;

			//this.treeview2.ButtonPressEvent += Treeview2_ButtonPressEvent;
			this.treeview2.ButtonReleaseEvent += Treeview2_ButtonReleaseEvent;

		}

		void Treeview2_ButtonReleaseEvent (object o, ButtonReleaseEventArgs args)
		{

			PathFindResult pathFindResult = _pathFindResult;
			if (pathFindResult == null) {
				// TODO debug


				return;
			}
			Alternative [] alternatives = pathFindResult.alternatives;

			if (alternatives == null) {
				// TODO debug
				return;
			}

			if ( !alternatives.Any ()) {
				// TODO
				return;
			}

			int x = Convert.ToInt32 (args.Event.X);
			int y = Convert.ToInt32 (args.Event.Y);
			if (!treeview2.GetPathAtPos (x, y, out TreePath path)) {
				return;
			}

			int index = path.Indices [0];

			//int index = Convert.ToInt32 (args);

			Alternative alt = alternatives [index];

			if (alt == null) {
				// TODO
				return;
			}

			RipplePaymentTransaction ripplePaymentTransaction = new RipplePaymentTransaction {
				Destination = pathFindResult.destination_account,
				Account = pathFindResult.source_account,
				Amount = pathFindResult.destination_amount,
				Paths = alt.paths_computed,
				SendMax = alt.source_amount
			};

			LicenseType licenseT = Util.LicenseType.PAYMENT;
			if (LeIceSense.IsLicenseExempt (ripplePaymentTransaction.Amount) || LeIceSense.IsLicenseExempt (ripplePaymentTransaction.SendMax)) {
				licenseT = LicenseType.NONE;
			}

			PaymentSubmitWindow paymentSubmitWindow = new PaymentSubmitWindow (_rippleWallet, licenseT);

			paymentSubmitWindow.SetPayments ( ripplePaymentTransaction );
			paymentSubmitWindow.Show ();

		}


		public void SetPathFindResult (PathFindResult result) {
			if (result == null) {
				return;
			}

			// TODO if 
			Alternative [] alternatives = result.alternatives;


			Application.Invoke (delegate {
				liststore.Clear ();

				if ( alternatives == null || !result.alternatives.Any () ) {
					MessageDialog.ShowMessage (
						"No paths found. Try placing an order on the orderbook to buy " 
						+ result.destination_amount.ToString() 
						+ " using the trading client"
					);
				}


				foreach (Alternative a in result.alternatives) {
					RippleCurrency cur = a.source_amount;

					if (RippleCurrency.NativeCurrency.Equals (cur.currency)) {
						liststore.AppendValues (cur.amount.ToString (), cur.currency, "");
					} else {
						liststore.AppendValues (cur.amount.ToString (), cur.currency, cur.issuer);
					}
				}
				this._pathFindResult = result;
			});


		}

		public void SetRippleWallet (RippleWallet rippleWallet)
		{
			this._rippleWallet = rippleWallet;
		}


		private RippleWallet _rippleWallet = null;
		private PathFindResult _pathFindResult = null;

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		ListStore liststore = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

	}
}

