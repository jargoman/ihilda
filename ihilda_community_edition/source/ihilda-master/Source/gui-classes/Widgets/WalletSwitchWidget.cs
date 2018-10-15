using System;
using System.Threading.Tasks;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class WalletSwitchWidget : Gtk.Bin
	{
		public WalletSwitchWidget ()
		{
			this.Build ();


			this.label1.UseMarkup = true;
			this.button91.Clicked += (object sender, EventArgs e) => {
				RippleWallet rippleWallet = WalletSelectDialog.DoDialog ();

				if (rippleWallet != null) {
					SetRippleWallet (rippleWallet);
				}


			};

			this.eventbox2.ButtonReleaseEvent += (object o, Gtk.ButtonReleaseEventArgs args) => { 
				RippleWallet rippleWallet = WalletSelectDialog.DoDialog ();

				if (rippleWallet != null) {
					SetRippleWallet (rippleWallet);
				}
			};
		}



		public void SetRippleWallet ( RippleWallet rippleWallet )
		{
			_rippleWallet = rippleWallet;

			Gtk.Application.Invoke ( delegate {
			
				this.label1.Markup = "<b><big><span fgcolor=\"green\">" + rippleWallet.Account + "</span></big></b>";

			});

			Task.Run (() => { 
				
				WalletChangedEvent?.Invoke (this, new WalletChangedEventArgs (rippleWallet));
			
			});


		}

		public RippleWallet GetRippleWallet ()
		{
			return _rippleWallet;
		}


		public event WalletChanged WalletChangedEvent;


		private RippleWallet _rippleWallet = null; 
	}

	public delegate void WalletChanged (object source, WalletChangedEventArgs eventArgs);
	public class WalletChangedEventArgs : EventArgs
	{
		private readonly RippleWallet _rippleWallet;
		public WalletChangedEventArgs (RippleWallet rippleWallet)
		{
			_rippleWallet = rippleWallet;
		}
		public RippleWallet GetRippleWallet ()
		{
			return _rippleWallet;
		}
	}

}
