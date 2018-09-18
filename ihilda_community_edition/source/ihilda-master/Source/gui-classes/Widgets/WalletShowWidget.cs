using System;

using RippleLibSharp.Keys;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WalletShowWidget : Gtk.Bin
	{
		public WalletShowWidget ()
		{
			#if DEBUG
			string method_sig = clsstr + nameof (WalletShowWidget) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.WalletShowWidget) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}
			#endif
			this.Build ();



			//while(Gtk.Application.EventsPending())
			//	Gtk.Application.RunIteration();
			#if DEBUG
			if (DebugIhildaWallet.WalletShowWidget) {
				Logging.WriteLog(method_sig + DebugIhildaWallet.buildComp);
			}
			#endif
			this.accountbutton.Clicked += (sender, e) => clipboard.Text = accountlabel.Text;

			this.namebutton.Clicked += (sender, e) => clipboard.Text = namelabel.Text;

			this.secretbutton.Clicked += delegate {
				// TODO warn user about security implications of copy and pasting to clipboard
				//Gtk.Clipboard clipboard = Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));  // 
				clipboard.Text = secretlabel.Text;

			};


			// I'm not quite sure why Activate only fires once
			/*
			this.checkbutton.Activated += delegate {
				
				toggleSecret();
			};
			*/

			this.checkbutton.Clicked += (sender, e) => ToggleSecret ();
		}


		public void SetRippleWallet (RippleWallet rippleWallet)
		{
			if (rippleWallet == null) {
				throw new ArgumentNullException (nameof (rippleWallet));
			}

			this._rippleWallet = rippleWallet;
			//this._walletswitchwidget = rippleWallet;

			Gtk.Application.Invoke( delegate {

			
				this.namelabel.Text = AddressDisplayWidget.UNSYNCED;
				this.accountlabel.Text = AddressDisplayWidget.UNSYNCED;
				this.secretlabel.Text = AddressDisplayWidget.UNSYNCED;

				if (_rippleWallet == null) {
					return;
				}

				if (_rippleWallet.WalletName!=null) {
					this.namelabel.Text = _rippleWallet.WalletName;
				}

				this.accountlabel.Text = _rippleWallet.GetStoredReceiveAddress();
				this.secretlabel.Text = "Concealed";
				//ToggleSecret();
				
			});







		}

		/*
		public RippleWallet GetWallet () {
			return this.wallet;
		}
		*/

		private void ToggleSecret () {
			
			RippleWallet rw = _rippleWallet;
			if (checkbutton.Active) {
				bool sure = AreYouSure.AskQuestion ("Security", "Are you sure you want to display the secret for this account?");

				if (sure) {
					RippleIdentifier rsa = rw.GetDecryptedSeed ();
					if (rsa != null) {
						this.secretlabel.Text = rsa.ToString ();
					}
				}

			}

			else {
				
				//if (rw.seed!=null) {
				//	this.secretlabel.Text = rw.seed.ToHiddenString();
				//}

				this.secretlabel.Text = "Concealed";
			}

		}

		private RippleWallet _rippleWallet = null;
		Gtk.Clipboard clipboard = Gtk.Clipboard.Get( Gdk.Atom.Intern( "CLIPBOARD" , false) );

		#if DEBUG
		private static string clsstr = nameof (WalletShowWidget) + DebugRippleLibSharp.colon;
		#endif

	}

}

