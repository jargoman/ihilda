using System;
using System.Linq;
using System.Collections.Generic;
using Gtk;
namespace IhildaWallet
{
	public partial class WalletSelectDialog : Gtk.Dialog
	{
		public WalletSelectDialog (IEnumerable<RippleWallet> wallets)
		{
			this.Build ();

			this.wallets = wallets;

			ListStore listStore = new ListStore (typeof (string), typeof (string), typeof (string));

			foreach (RippleWallet rippleWallet in wallets) {

				listStore.AppendValues (rippleWallet.WalletName, rippleWallet.Account, rippleWallet.Encryption_Type);

			}

			this.combobox2.Model = listStore;
			this.combobox2.Active = 0;
		}

		public RippleWallet GetRippleWallet ()
		{
			int index = combobox2.Active;

			return wallets.ElementAt (index);

		}

		public static RippleWallet DoDialog ()
		{
			RippleWallet rippleWallet = null;
			Dictionary<String, RippleWallet> keyValuePairs = WalletManager.currentInstance.wallets;
			List<RippleWallet> list = new List<RippleWallet> ();
			foreach (var v in keyValuePairs) {
				list.Add(v.Value);
			}

			using (WalletSelectDialog walletSelectDialog = new WalletSelectDialog (list)) {

				ResponseType response = (ResponseType)walletSelectDialog.Run ();
				rippleWallet = walletSelectDialog.GetRippleWallet ();

				walletSelectDialog.Destroy ();
				if (response != ResponseType.Ok) {
					return null;
				}
			}

			return rippleWallet;

		}

		private IEnumerable<RippleWallet> wallets = null;
	}
}
