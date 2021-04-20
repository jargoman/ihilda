using System;
using Gtk;

namespace IhildaWallet
{
	public partial class PasswordCreateDialog : Gtk.Dialog
	{
		public PasswordCreateDialog (String message)
		{
			this.Build ();
			#if DEBUG
			if (DebugIhildaWallet.PasswordCreateDialog) {
				Logging.WriteLog ("new PasswordCreateDialog\n");
			}
			#endif
			this.textview1.Buffer.Text = message;

			this.secretentry.GrabFocus ();
		}


		public int VerifyPasswords () {
			#if DEBUG
			if (DebugIhildaWallet.PasswordCreateDialog) {
				Logging.WriteLog ("PasswordCreateDialog : method verifyPasswords : begin\n");
			}
			#endif
			String passone = this.secretentry.Text;
			String passtwo = this.secretentry1.Text;
			if (!passone.Equals(passtwo)) {
				#if DEBUG
				if (DebugIhildaWallet.PasswordCreateDialog) {
					Logging.WriteLog ("PasswordCreateDialog : method verifyPasswords : Passwords do not match\n");
				}
				#endif
				return PASSNOTMATCH;
			}

			#if DEBUG
			if (DebugIhildaWallet.PasswordCreateDialog) {
				Logging.WriteLog ("PasswordCreateDialog : method verifyPasswords : Passwords match\n");
			}
			#endif
			return PASSISVALID;
		}

		public String GetPassword () {
			#if DEBUG
			if (DebugIhildaWallet.PasswordCreateDialog) {
				Logging.WriteLog ("PasswordCreateDialog : method getPassword : begin\n");
			}
			#endif
			return this.secretentry.Text;
		}


		public static int PASSNOTMATCH = 1;

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public static int PASSISVALID = 0;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		protected void OnSecretentryActivated (object sender, EventArgs e)
		{
			#if DEBUG
			if (DebugIhildaWallet.PasswordCreateDialog) {
				Logging.WriteLog ("PasswordCreateDialog : method OnSecretentryActivated : begin\n");
			}
			#endif


			this.secretentry1.GrabFocus ();
		}

		protected void OnSecretentry1Activated (object sender, EventArgs e)
		{
			#if DEBUG
			if (DebugIhildaWallet.PasswordCreateDialog) {
				Logging.WriteLog ("PasswordCreateDialog : method OnSecretentry1Activated : begin\n");
			}
			#endif
			this.buttonOk.GrabFocus ();
		}


		public static string DoDialog ( string message ) {

			using (PasswordCreateDialog pcd = new PasswordCreateDialog (message)) {

				ResponseType rt = (ResponseType)pcd.Run ();
				pcd.Hide ();

				if (rt != ResponseType.Ok) {

					return null;
				}

				string p = pcd.GetPassword ();

				return p;
			}
		}
	}
}

