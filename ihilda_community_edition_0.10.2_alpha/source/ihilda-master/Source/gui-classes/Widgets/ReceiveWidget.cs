/*
 *	License : Le Ice Sense 
 */

using System;
using RippleLibSharp.Util;
using System.Collections.Generic;
using Codeplex.Data;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ReceiveWidget : Gtk.Bin
	{
		public ReceiveWidget ()
		{

			this.Build ();

			if (addressdisplaywidget1 == null) {
				addressdisplaywidget1 = new AddressDisplayWidget ();
				addressdisplaywidget1.Show ();
				hbox6.Add (addressdisplaywidget1);
			}
			//while(Gtk.Application.EventsPending())
			//	Gtk.Application.RunIteration();

#if DEBUG
			if (DebugIhildaWallet.ReceiveWidget) {
				//Logging.write ("new ReceiveWidget\n");
				Logging.WriteLog (clsstr + "new");
			}
#endif



			this.syncbutton.Clicked += this.OnSyncClicked;

			/*
			NetworkInterface.onOpen += delegate (object sender, EventArgs e) {
				if (Debug.ReceiveWidget) {
					Logging.writeLog ("ReceiveWidget : NetworkInterface.onOpen : delegate begin\n");
				}

				if (this.isSet) {
					this.requestInfo(this.getReceiveAddress());
				}


			};
			*/
		}




		String address = AddressDisplayWidget.UNSYNCED;


		/* no receive address on start */
#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private bool isSet = false;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		/* user clicked the synced button */
		protected void OnSyncClicked (object sender, EventArgs e)
		{
			if (!isSet) {
				Warn ();
				return;
			}

			//String address = this.getReceiveAddress();
			// TODO uncomment or rewrite below
			//requestInfo (address);

		}



		public static void Warn ()
		{
			MessageDialog.ShowMessage ("You need a public and private key. Go to wallet tab and enter your wallet key pair\n");
		}

		public void SetRippleWallet (RippleWallet rw)
		{


#if DEBUG
			//String ad = (string)((rw == null) ? UNSYNCED : rw.getStoredReceiveAddress ());
			String method_sig = clsstr + nameof(SetRippleWallet) + DebugRippleLibSharp.left_parentheses + nameof(rw) + DebugRippleLibSharp.equals + ( rw?.ToString () ?? "null") + DebugRippleLibSharp.right_parentheses;

			if (DebugIhildaWallet.ReceiveWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			//AccountLines.cash = new Dictionary<string, Decimal> ();

			addressdisplaywidget1.SetAddress (rw);

			if (rw == null) {
#if DEBUG
				if (DebugIhildaWallet.ReceiveWidget) {
					Logging.WriteLog (method_sig + "Wallet is null");
				}
#endif
				this.address = null;
				this.isSet = false;
			} else {
#if DEBUG
				if (DebugIhildaWallet.ReceiveWidget) {
					Logging.WriteLog (method_sig + "Wallet is NOT null");
				}
#endif
				this.address = rw.GetStoredReceiveAddress ();
				this.isSet = true;



			}



		}



		public String GetReceiveAddress ()
		{

			if (!isSet) {

				Warn ();
				return null;
			}

			return this.address;
		}

#if DEBUG
		private const string clsstr = nameof (ReceiveWidget) + DebugRippleLibSharp.colon;
#endif

	}
}

