using System;
using System.Text;
using Gtk;
using RippleLibSharp.Keys;
using RippleLibSharp.Transactions.TxTypes;
using RippleLibSharp.Util;
using RippleLibSharp.Binary;

namespace IhildaWallet
{
	public partial class AccountRCLSettingsWindow : Gtk.Window
	{
		public AccountRCLSettingsWindow (RippleWallet rippleWallet) :
				base (Gtk.WindowType.Toplevel)
		{
			this.Build ();

			Gtk.TreeStore treeStore = new TreeStore (typeof (string));

			this.SetRippleWallet (rippleWallet);

			foreach (var wallet in WalletManager.currentInstance.wallets) {

				var actType = wallet.Value.AccountType;
				if (actType == RippleWalletTypeEnum.Regular) {
					string regAcct = wallet.Value?.Regular_Key_Account;
					string name = wallet.Key;
					string masAcct = wallet.Value.Account;
					if (
						!string.IsNullOrWhiteSpace (name) &&
						!string.IsNullOrWhiteSpace (regAcct) &&
						!string.IsNullOrWhiteSpace (masAcct)) {

						RippleWallet selectedWallet = this.walletswitchwidget1.GetRippleWallet ();

						if (masAcct == selectedWallet?.Account) {



							treeStore.AppendValues (regAcct);
						}

					}

				}

			}

			comboboxentry2.Model = treeStore;

			setregularkeybutton.Clicked += (object sender, EventArgs exceptio) => {


#if DEBUG
				string event_sig = nameof (DebugRippleLibSharp.AccountRCLSettingsWindow);

#endif

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

			accountsetbutton.Clicked += (object sender, EventArgs e) => {

				RippleWallet signingWallet = walletswitchwidget1.GetRippleWallet ();

				RippleAccountSetTransaction accountSetTransaction = new RippleAccountSetTransaction ();

				accountSetTransaction.Account = signingWallet.GetStoredReceiveAddress ();

				if (setcheckbutton.Active) {
					bool b  = uint.TryParse (setflagentry.Text, out uint set);
					if (b) {
						accountSetTransaction.SetFlag = set;
					} else {
						return;
					}

		    			
				}

				if (clearflagcheckbox.Active) {
					bool b = uint.TryParse (clearflagentry.Text, out uint clear);
					if (b) {
						accountSetTransaction.ClearFlag = clear;
					} else {
						return;
					}
				}

				if (domaincheckbutton.Active) {
					accountSetTransaction.Domain = domainentry.Text;
				}


				if (transferratecheckbutton.Active) {
					bool b = uint.TryParse (clearflagentry.Text, out uint rate);
					if (b) {
						accountSetTransaction.TransferRate = rate;
					} else {
						return;
					}
					
				}

				if (emailhashcheckbutton.Active) {
					accountSetTransaction.EmailHash = emailhashentry.Text;
				}

				if (messagekeycheckbutton.Active) {
					// TODO possibly give user option to convert number to hex
					//var message = Base58.StringToHex (messagekeyentry.Text);
					var message = messagekeyentry.Text;

					accountSetTransaction.MessageKey = message;
				}
				if (ticksizecheckbutton.Active) {
					bool b = byte.TryParse (ticksizeentry.Text, out byte tick);
					if (b) {
						accountSetTransaction.TickSize = tick;
					} else {
						return;
					}

				}

				TransactionSubmitWindow transactionSubmitWindow = new TransactionSubmitWindow (signingWallet, Util.LicenseType.NONE);

				transactionSubmitWindow.SetTransactions (accountSetTransaction);
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
