using System;
using Gtk;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public partial class WalletViewWindow : Gtk.Window
	{
		public WalletViewWindow (RippleWallet rippleWallet) :
				base (Gtk.WindowType.Toplevel)
		{
			this.Build ();


			if (this.walletshowwidget1 == null) {
				walletshowwidget1 = new WalletShowWidget ();
				walletshowwidget1.Show ();

				if (label4 == null) {
					label4 = new Label ("<b>Wallet</b>") {
						UseMarkup = true
					};
				}

				notebook1.AppendPage (walletshowwidget1, label4);
			}

			if (this.balancetab1 == null) {
				this.balancetab1 = new BalanceTab ();
				balancetab1.Show ();

				if (label5 == null) {
					label5 = new Label ("<b>Balance</b>") {
						UseMarkup = true
					};
				}
				notebook1.AppendPage (walletshowwidget1, label5);
			}


			SetRippleWallet (rippleWallet);



		}


		public void SetRippleWallet (RippleWallet rippleWallet)
		{
#if DEBUG


			string method_sig = clsstr + nameof (SetRippleWallet) + DebugRippleLibSharp.colon;
#endif


			if (this.walletshowwidget1 != null) {
#if DEBUG
				if (DebugIhildaWallet.WalletShowWidget) {
					Logging.WriteLog (method_sig + "wallet1 != null");
				}
#endif
				this.walletshowwidget1.SetRippleWallet (rippleWallet);
			}



			if (this.balancetab1 != null) {
#if DEBUG
				if (DebugIhildaWallet.WalletShowWidget) {
					Logging.WriteLog (method_sig + "balancetab1 != null");
				}
#endif

				this.balancetab1.SetAddress (rippleWallet.GetStoredReceiveAddress ());
			}
		}


#if DEBUG
		private const string clsstr = nameof (WalletViewWindow) + DebugRippleLibSharp.colon;
#endif


	}
}
