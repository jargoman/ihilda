using System;
using System.Threading;
using Gtk;

namespace IhildaWallet
{
	public partial class AggreementDialog : Gtk.Dialog
	{
		public AggreementDialog ()
		{
			this.Build ();


			checkbutton3.Toggled += (object sender, EventArgs e) => {
				buttonOk.Sensitive = checkbutton3.Active;
			};
		}


		public static bool DoDialog ()
		{

			ResponseType resp = ResponseType.None;
			using (ManualResetEvent manualResetEvent = new ManualResetEvent (false)) {
				manualResetEvent.Reset ();

				Gtk.Application.Invoke (delegate {
					using (
					    AggreementDialog aggreementDialog = new AggreementDialog ()
					) {
						resp = (ResponseType)aggreementDialog.Run ();
						aggreementDialog.Destroy ();
						manualResetEvent.Set ();
					}
				});

				manualResetEvent.WaitOne ();
			}

			return (resp == ResponseType.Ok) ? true : false;
		}

	}
}
