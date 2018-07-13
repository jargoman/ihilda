using System;
using Gtk;

namespace IhildaWallet
{
	public partial class PasswordDialog : Gtk.Dialog
	{
		public PasswordDialog (String message)
		{
			this.Build ();

			#if DEBUG
			if (DebugIhildaWallet.PasswordDialog) {
				Logging.WriteLog("new PasswordDialog\n");
			}
			#endif

			this.textview1.Buffer.Text = message;
			//this.textview1.A

			this.entry1.GrabFocus ();
		}

		public string GetPassword () {
			#if DEBUG
			if (DebugIhildaWallet.PasswordDialog) {
				Logging.WriteLog ("PasswordDialog : method getPassword : begin\n");
			}
			#endif
			return this.entry1.Text;
		}

		protected void OnEntry1Activated (object sender, EventArgs e)
		{
			#if DEBUG
			if (DebugIhildaWallet.PasswordDialog) {
				Logging.WriteLog ("PasswordDialog : OnEntry1Activated : begin\n");
			}
			#endif
			this.buttonOk.Activate ();
		}



		public static string DoDialog () {

			PasswordDialog pd = new PasswordDialog ("Please enter your password");

			ResponseType rt = (ResponseType)pd.Run ();

			if (rt != ResponseType.Ok) {
				return null;
			}

			string pass = pd.GetPassword ();

			pd.Destroy ();

			return pass;
		}
	}
}

