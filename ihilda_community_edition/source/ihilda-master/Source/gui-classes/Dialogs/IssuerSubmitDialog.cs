using System;
using Gtk;
using RippleLibSharp.Keys;

namespace IhildaWallet
{
	public partial class IssuerSubmitDialog : Gtk.Dialog
	{
		public IssuerSubmitDialog ()
		{
			this.Build ();
		}

		public RippleAddress GetAddress () {

			return this.addressentry1.GetAddress();
		}

		public static RippleAddress DoDialog (string message) {


			using (IssuerSubmitDialog dialog = new IssuerSubmitDialog ()) {
				dialog.textview1.Buffer.Text = message;

				while (true) {
					ResponseType rt = (ResponseType) dialog.Run ();
					RippleAddress ra = dialog.GetAddress ();
					switch ( rt ) {
					case ResponseType.Ok:
						if (ra != null) {
							return ra;
						}
						MessageDialog.ShowMessage ("Invalid ripple address ");
						continue;

					case ResponseType.Cancel:
						break;
					//case ResponseType.Apply:

					default:
						continue;
						
					}
				}
			}


		}
	}
}

