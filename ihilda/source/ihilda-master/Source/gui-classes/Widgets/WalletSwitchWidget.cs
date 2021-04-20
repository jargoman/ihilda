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
				ChooseWallet ();

			};

			this.viewbutton.Clicked += (sender, e) => {
				ViewWallet ();
			};

			this.eventbox2.ButtonReleaseEvent += (object o, Gtk.ButtonReleaseEventArgs args) => {
				ChooseWallet ();
			};
		}

		private void ViewWallet () {
			if (_rippleWallet == null) {

				ChooseWallet ();
			}

			if (_rippleWallet == null) {
				return;
			}

			WalletViewWindow walletViewWindow = new WalletViewWindow (_rippleWallet);
			walletViewWindow.Show ();

	    		
		}

		private void ChooseWallet () {
			RippleWallet rippleWallet = WalletSelectDialog.DoDialog ();

			if (rippleWallet != null) {
				SetRippleWallet (rippleWallet);
			}

		}



		public void SetRippleWallet ( RippleWallet rippleWallet )
		{
			_rippleWallet = rippleWallet;

			Gtk.Application.Invoke ( delegate {
				if (ProgramVariables.darkmode) {
					this.namelabel.Markup = "<b><span size=\"large\" fgcolor=\"chartreuse\">" + rippleWallet.WalletName + "</span></b>";
					this.label1.Markup = "<b><span size=\"x-large\" fgcolor=\"chartreuse\">" + rippleWallet.Account + "</span></b>";
				} else {
					this.namelabel.Markup = "<b><span size=\"large\" fgcolor=\"green\">" + rippleWallet.WalletName + "</span></b>";
					this.label1.Markup = "<b><span size=\"x-large\" fgcolor=\"green\">" + rippleWallet.Account + "</span></b>";
				}

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
