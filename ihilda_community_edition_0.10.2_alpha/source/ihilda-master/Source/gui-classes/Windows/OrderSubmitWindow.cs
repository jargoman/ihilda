using System;
using System.Collections.Generic;

using System.Threading;
using IhildaWallet.Util;
using Gtk;

namespace IhildaWallet
{
	public partial class OrderSubmitWindow : Gtk.Window
	{
		public OrderSubmitWindow (RippleWallet rippleWallet, LicenseType licenseType) :
			base (WindowType.Toplevel)
		{
			this.Build ();

			if (orderpreviewsubmitwidget1 == null) {
				orderpreviewsubmitwidget1 = new OrderPreviewSubmitWidget ();
				orderpreviewsubmitwidget1.Show ();
				Add (orderpreviewsubmitwidget1);
			}

			orderpreviewsubmitwidget1.SetRippleWallet (rippleWallet);
			orderpreviewsubmitwidget1.SetLicenseType (licenseType);
			//_rippleWallet = rippleWallet;
		}

		public void SetOrders ( IEnumerable <AutomatedOrder> offers ) {

			if (orderpreviewsubmitwidget1 == null) {
				OrderPreviewSubmitWidget opsw = new OrderPreviewSubmitWidget ();
				//this.Child = (Gtk.Widget)opsw;

				Add (opsw);

			}

			this.orderpreviewsubmitwidget1.SetDefaultOrders (offers);
			this.orderpreviewsubmitwidget1.SetOffers (offers);

		}

		public static bool ShortHandSubmit ( RippleWallet rippleWallet, IEnumerable<AutomatedOrder> offers, LicenseType licenseType)
		{

			bool ret = false;
			ManualResetEvent manualReset = new ManualResetEvent (false);
			manualReset.Reset ();

			Application.Invoke (
				delegate {
					OrderSubmitWindow orderSubmitWindow = new OrderSubmitWindow (rippleWallet, licenseType);

					orderSubmitWindow.SetOrders (offers);

					orderSubmitWindow.DeleteEvent += (object o, DeleteEventArgs args) => {
						ret = orderSubmitWindow.GetReturnValue ();
						manualReset.Set ();
					};

				}
			);

			manualReset.WaitOne ();

			manualReset.Dispose ();

			return ret;

		}

		private bool GetReturnValue ()
		{
			return this.orderpreviewsubmitwidget1.AllSubmitted;
		}

		/*
		public void SetRippleWallet (RippleWallet rippleWallet)
		{
			this._rippleWallet = rippleWallet;
		}


		private RippleWallet _rippleWallet {
			get;
			set;
		}
		*/

		public const string GUIName = "Order Manager";
	}
}

