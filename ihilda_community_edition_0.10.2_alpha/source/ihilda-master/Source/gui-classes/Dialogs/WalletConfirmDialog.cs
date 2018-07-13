using System;
using System.Threading;
using Gtk;
namespace IhildaWallet
{
	public partial class WalletConfirmDialog : Gtk.Dialog
	{
		public WalletConfirmDialog (String message)
		{
			this.Build ();

			this.label3.Text = message;
		}

		public static ResponseType GetResponse (String title, String message)
		{

			ResponseType response = ResponseType.None;

			ManualResetEvent manualResetEvent = new ManualResetEvent (false);
			manualResetEvent.Reset ();

			Application.Invoke ((object sender, EventArgs e) => { 
				WalletConfirmDialog walletConfirmDialog = new WalletConfirmDialog (message) {
					Title = title
				};

				response = (ResponseType)walletConfirmDialog.Run ();
				manualResetEvent.Set ();
			
			});

			manualResetEvent.WaitOne ();

			return response;
		}
	}
}
