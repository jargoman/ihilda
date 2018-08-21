using System;

namespace IhildaWallet
{
	public partial class MessageDialog : Gtk.Dialog
	{
		public MessageDialog (String message)
		{
			this.Build ();
			this.Title = "Notice";
			this.textview1.Buffer.Text = message;


		}

		public MessageDialog (String title, String message)
		{
			this.Build ();

			this.textview1.Buffer.Text = message;

			this.Title = title;

			this.label11.Markup = "<big><b><u> " + title + " </u></b></big>";

		}

		public static void ShowMessage (String message) {


			Gtk.Application.Invoke( delegate {
				MessageDialog mg = new MessageDialog (message) {
					Modal = true
				};

				mg.Run ();

				mg.Destroy ();

			}
			);


		}

		public static void ShowMessage (string title, string message) => Gtk.Application.Invoke (delegate {
			MessageDialog mg = new MessageDialog (title, message) {
				Modal = true
			};

			mg.Run ();

			mg.Destroy ();

		}
			);
	}
}

