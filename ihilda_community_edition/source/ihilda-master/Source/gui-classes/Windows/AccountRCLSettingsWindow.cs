using System;
using System.Text;
using Gtk;
using RippleLibSharp.Keys;
using RippleLibSharp.Transactions.TxTypes;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public partial class AccountRCLSettingsWindow : Gtk.Window
	{
		public AccountRCLSettingsWindow (RippleWallet rippleWallet) :
				base (Gtk.WindowType.Toplevel)
		{
			this.Build ();

			Gtk.TreeStore treeStore = new TreeStore ( typeof(string) );

			this.SetRippleWallet (rippleWallet);

			foreach (var wallet in WalletManager.currentInstance.wallets) {
				
				var actType = wallet.Value.AccountType;
				if (actType == RippleWalletTypeEnum.Regular) {
					string regAcct = wallet.Value?.Regular_Key_Account;
					string name = wallet.Key;
					string masAcct = wallet.Value.Account;
					if ( 
					    !string.IsNullOrWhiteSpace(name) && 
					    !string.IsNullOrWhiteSpace (regAcct)  && 
					    !string.IsNullOrWhiteSpace (masAcct )) 
					{

						RippleWallet selectedWallet = this.walletswitchwidget1.GetRippleWallet ();

						if (masAcct == selectedWallet?.Account) {



							treeStore.AppendValues (regAcct);
						}

					}

				}

			}

			comboboxentry2.Model = treeStore;

			button405.Clicked += (object sender, EventArgs exceptio) => {

				string event_sig = nameof (DebugRippleLibSharp.AccountRCLSettingsWindow);
				string str = comboboxentry2.ActiveText;

				if (string.IsNullOrEmpty(str)) {
					// TODO warn user
					return;
				}

				RippleAddress rippleAddress = null;

				try {
					rippleAddress = new RippleAddress (str);
				} catch ( Exception exception) {

#if DEBUG
					if (DebugRippleLibSharp.AccountRCLSettingsWindow) {
						Logging.ReportException (event_sig, exception);
					}
#endif
					return;

				}

				RippleWallet signingWallet = walletswitchwidget1.GetRippleWallet ();

				RippleSetRegularKey setRegKeyTx = new RippleSetRegularKey {
					Account = rippleWallet.Account,
					RegularKey = str
				};

				TransactionSubmitWindow transactionSubmitWindow = new TransactionSubmitWindow (signingWallet, Util.LicenseType.NONE);
				transactionSubmitWindow.SetTransactions (setRegKeyTx);

			};
		}


		public void SetRippleWallet ( RippleWallet rippleWallet )
		{
			this.walletswitchwidget1.SetRippleWallet (rippleWallet);
		}

#if DEBUG
		const string clsstr = nameof (SendRipple) + DebugRippleLibSharp.colon;
#endif

	}
}
