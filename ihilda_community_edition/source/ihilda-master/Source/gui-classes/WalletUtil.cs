/*
 *	License : Le Ice Sense 
 */

using System;
using System.IO;
using System.Text;
using Gtk;
using IhildaWallet;
using Codeplex.Data;
//using Microsoft.CSharp;
using System.Security.Cryptography;

using RippleLibSharp.Keys;
using RippleLibSharp.Util;

namespace IhildaWallet
{



	[System.ComponentModel.ToolboxItem (true)]
	public partial class WalletUtil : Gtk.Bin
	{
		public WalletUtil ()
		{
#if DEBUG
			if (DebugIhildaWallet.Wallet) {
				Logging.WriteLog ("new Wallet (Gtk widget)\n");
			}
#endif

			this.Build ();

#if DEBUG
			if (DebugIhildaWallet.Wallet) {
				Logging.WriteLog ("new Wallet (Gtk widget) build complete\n");
			}
#endif







			showsecretcheckbox.Active = false;

			this.secretentry.Visibility = false;



		}

		protected void OnShowsecretcheckboxToggled (object sender, EventArgs e)
		{
#if DEBUG
			if (DebugIhildaWallet.Wallet) {
				Logging.WriteLog ("Wallet : method OnShowsecretcheckboxToggled : begin : ");
			}
#endif

			this.secretentry.Visibility = showsecretcheckbox.Active; // K.I.S.S :D

		}


		protected void OnReceiveAddressActivated (object sender, EventArgs e)
		{
#if DEBUG
			if (DebugIhildaWallet.Wallet) {
				Logging.WriteLog ("Wallet : method OnReceiveAddressActivated : begin\n");
			}
#endif
			if (secretentry == null) {
				// TODO debug

				Logging.WriteLog ("Wallet : method OnReceiveAddressActivated : Critical bug : secretentry = null\n");

				return;
			}

			secretentry.GrabFocus ();
		}


		protected void OnSecretentryActivated (object sender, EventArgs e)
		{
#if DEBUG
			if (DebugIhildaWallet.Wallet) {
				Logging.WriteLog("Wallet : method OnSecretentryActivated() : begin\n");
			}
#endif



		}



		protected void GetReceiveFromSecret (object sender, EventArgs e)
		{
			String Secret = this.secretentry.Text;

			if (Secret == null || Secret.Trim ().Equals ("")) {
				MessageDialog.ShowMessage ("Please enter or generate a secret ripple seed address");
				return;
			}

			RippleSeedAddress rsa = new RippleSeedAddress (Secret);

			RippleAddress ra = rsa.GetPublicRippleAddress ();

			this.receiveAddress.Text = ra.ToString ();

		}




		protected void OnVerifyClicked (object sender, EventArgs eventArgs)
		{
#if DEBUG
			string method_sig = clsstr + nameof (OnVerifyClicked) + DebugRippleLibSharp.left_parentheses + nameof (sender) + DebugRippleLibSharp.comma + nameof (eventArgs);
#endif

			String secret = this.secretentry.Text;
			String address = this.receiveAddress.Text;

			RippleSeedAddress seed = null;
			RippleAddress addr = null;

			bool shouldreturn = false; // might as well verify both befor returning

			if (secret == null || secret.Trim ().Equals ("")) {
				MessageDialog.ShowMessage ("Secret entry is empty");
				shouldreturn = true;
				secret = null;
			} else {
				secret = secret.Trim ();

				try {
					seed = new RippleSeedAddress (secret);
#pragma warning disable 0168
				} catch (FormatException ex) {
#pragma warning restore 0168


#if DEBUG
					if (DebugIhildaWallet.WalletUtil) {
						Logging.ReportException (method_sig, ex);
					}
#endif

					MessageDialog.ShowMessage ("The secret (seed address) entered is invalid");
					shouldreturn = true;
				} catch (CryptographicException ex) {
					MessageDialog.ShowMessage (ex.Message);
				}

			}

			if (address == null || address.Trim ().Equals ("")) {
				MessageDialog.ShowMessage ("ReceiveAddress is blank");
				shouldreturn = true;
				address = null;
			} else {
				address = address.Trim ();

				try {
					addr = new RippleAddress (address);

#pragma warning disable 0168
				} catch (FormatException ex) {

#if DEBUG
					if (DebugIhildaWallet.WalletUtil) {
						Logging.ReportException(method_sig, ex);
					}
#endif


					MessageDialog.ShowMessage ("The address entered is invalid");

					shouldreturn = true;

				} catch (CryptographicException ex) {

#if DEBUG
					if (DebugIhildaWallet.WalletUtil) {
						Logging.ReportException(method_sig, ex);
					}
#endif

					MessageDialog.ShowMessage (ex.Message);



				} catch (Exception exception) {
#pragma warning restore 0168


#if DEBUG
					if (DebugIhildaWallet.WalletUtil) {
						Logging.ReportException(method_sig, exception);
					}
#endif
				}
			}

			if (shouldreturn) {  // it's intentional that both secret and receiveaddress are fully verified before returning
				return;
			}


			if (seed == null || addr == null) {
				return;
			}

			if (addr.Equals (seed.GetPublicRippleAddress ())) {
				MessageDialog.ShowMessage ("Secret seed address and public ripple address are a matching keypair");
			} else {
				MessageDialog.ShowMessage ("Secret seed address and public ripple address do not match");
			}


		}


#if DEBUG
		public static string clsstr = nameof (WalletUtil) + DebugRippleLibSharp.colon;
#endif



	}
}

