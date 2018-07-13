using System;

using RippleLibSharp.Transactions;
using RippleLibSharp.Transactions.TxTypes;

using RippleLibSharp.Network;
using IhildaWallet.Networking;

using RippleLibSharp.Keys;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CanselTxWidget : Gtk.Bin
	{
		public CanselTxWidget ()
		{
			this.Build ();

			this.button55.Clicked += (object sender, EventArgs e) => {

				// TODO lookup tx, verify matches signing account, are you sure?? ect

				string sq = this.comboboxentry1.Entry.Text;

				RippleCancelTransaction tx = new RippleCancelTransaction();

				RippleWallet rw = _rippleWallet;
				if (rw == null) {
					/*
					#if DEBUG
					if (Debug.BuyWidget) {
						Logging.writeLog (method_sig + "w == null, returning\n");
					}
					#endif
					*/

					return;
				}




				NetworkInterface ni = NetworkController.GetNetworkInterfaceGuiThread();
				if (ni == null) {
					MessageDialog.ShowMessage ("Network Error", "Unable to connect to network");
					return;
				}
				uint se = Convert.ToUInt32(RippleLibSharp.Commands.Accounts.AccountInfo.GetSequence (rw.GetStoredReceiveAddress(), ni));
				if (se == 0) {
					return;
				}
				Tuple< UInt32,UInt32 > tupe = FeeSettings.GetFeeAndLastLedgerFromSettings ( ni );
				if (tupe == null) {
					return;
				}

				tx.fee = (tupe.Item1 * 2).ToString();

				tx.Sequence = se; // 
				tx.LastLedgerSequence = tupe.Item2 + 6;

				tx.Account = rw.GetStoredReceiveAddress();

				tx.OfferSequence = UInt32.Parse(sq);

				SignOptions opts = SignOptions.LoadSignOptions();

				RippleSeedAddress seed = rw.GetDecryptedSeed();
				if (opts == null || opts.UseLocalRippledRPC) {

					tx.SignLocalRippled (seed);
				}

				else {

					tx.Sign(seed);

				}


				//Task< Response <RippleSubmitTxResult>> task = null;
				/*task =*/ NetworkController.UiTxNetworkSubmit (tx, ni);
				/*task.Wait ();*/


			};

		}

		public void SetRippleWallet (RippleWallet rippleWallet)
		{
			this._rippleWallet = rippleWallet;
		}

		private RippleWallet _rippleWallet {
			get;
			set;
		}

	}
}

