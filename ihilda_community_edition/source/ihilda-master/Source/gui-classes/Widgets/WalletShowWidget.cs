using System;
using System.Threading;
using System.Threading.Tasks;
using RippleLibSharp.Binary;
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

				if (_rippleWallet.AccountType == RippleWalletTypeEnum.Regular) {

				}

				switch (_rippleWallet.AccountType) {
					case RippleWalletTypeEnum.Master:
					typelabel.Text = "Master key";
					break;

					case RippleWalletTypeEnum.MasterPrivateKey:
					typelabel.Text = "Private Key";
					break;

					case RippleWalletTypeEnum.Regular:
					typelabel.Text = "Regular key";
					break;
				}
				
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

						Task.Run (delegate {



							PasswordAttempt passwordAttempt = new PasswordAttempt ();

							passwordAttempt.InvalidPassEvent += (object sender, EventArgs e) =>
							{
								bool shou = AreYouSure.AskQuestionNonGuiThread (
								"Invalid password",
								"Unable to decrypt seed. Invalid password.\nWould you like to try again?"
								);
							};

							passwordAttempt.MaxPassEvent += (object sender, EventArgs e) =>
							{
								string mess = "Max password attempts";

								MessageDialog.ShowMessage (mess);
								//WriteToOurputScreen ("\n" + mess + "\n");
							};


							DecryptResponse response = passwordAttempt.DoRequest (rw, new CancellationTokenSource().Token);




							RippleIdentifier rsa = response.Seed;
							if (rsa?.GetHumanReadableIdentifier () != null) {

								if (rsa is RippleSeedAddress seedAddress) {
									Gtk.Application.Invoke ( delegate {

										string sec = seedAddress.AsHex ();
										var privKey = seedAddress.GetPrivateKey (0);

										var pubKey = privKey.GetPublicKey ();

										this.secretHex.Markup = sec;
										this.privateLabel.Markup = privKey.GetHumanReadableIdentifier();
										this.privateHex.Markup = privKey.AsHex();
										this.publicLabel.Markup = pubKey.GetHumanReadableIdentifier();
										this.publicHex.Markup = pubKey.AsHex();
									});
								}

								if (rsa is RipplePrivateKey privateKey) {
									Gtk.Application.Invoke ( delegate {

										var pub = privateKey.GetPublicKey ();

										this.secretHex.Markup = "N/A";
										

										this.privateLabel.Markup = privateKey.GetHumanReadableIdentifier ();
										this.privateHex.Markup = privateKey.AsHex ();
										this.publicLabel.Markup = pub.GetHumanReadableIdentifier ();
										this.publicHex.Markup = pub.AsHex ();
									});
								}

								

								Gtk.Application.Invoke ( delegate {

									this.secretHex.SetAlignment (0, 0.5f);

									this.privateLabel.SetAlignment (0, 0.5f);
									this.privateHex.SetAlignment (0, 0.5f);
									this.publicLabel.SetAlignment (0, 0.5f);
									this.publicHex.SetAlignment (0, 0.5f);

									this.secretlabel.Text = rsa.ToString ();

								});

							}

						});
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

