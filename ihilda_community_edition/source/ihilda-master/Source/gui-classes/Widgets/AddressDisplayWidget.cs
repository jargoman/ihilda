using System;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AddressDisplayWidget : Gtk.Bin
	{
		public AddressDisplayWidget ()
		{
			this.Build ();
			//while(Gtk.Application.EventsPending())
			//	Gtk.Application.RunIteration();

			this.receiveLabel.Selectable = true;
		}


		public void SetAddress ( RippleWallet rw) {
			String address = rw.GetStoredReceiveAddress() ?? UNSYNCED;
			#if DEBUG
			String method_sig = clsstr + nameof(SetAddress) + DebugRippleLibSharp.left_parentheses + DebugIhildaWallet.ToAssertString(address) + DebugRippleLibSharp.right_parentheses;
			#endif

			Gtk.Application.Invoke (delegate {
				#if DEBUG
					if (DebugIhildaWallet.ReceiveWidget) {
					Logging.WriteLog(method_sig + DebugIhildaWallet.gtkInvoke + "setting receiveLabel text");
					}
				#endif
					
				this.receiveLabel.Text = address;
			}
			);
		}

		protected void OnReceiveButtonClicked (object sender, EventArgs e)
		{
			//this.receiveLabel

			if (this.receiveLabel == null) {
				Logging.WriteLog ("Error in class ReceiveWidget. receiveLabel is null\n");
				return;
			}


			int len = receiveLabel.Text.Length;
			if (len > 0) {
				receiveLabel.SelectRegion (0, receiveLabel.Text.Length);
				return;
			}


			ReceiveWidget.Warn ();
			return;


		}

		public static String UNSYNCED = " -- unsynced -- ";

		#if DEBUG
		private static readonly string clsstr = nameof (AddressDisplayWidget) + DebugRippleLibSharp.colon;
		#endif
	}
}

