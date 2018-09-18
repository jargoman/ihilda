using System;
using System.Threading;
using Gtk;

namespace IhildaWallet
{
	public partial class TrippleEntenteCreationDialog : Gtk.Dialog
	{
		public TrippleEntenteCreationDialog ()
		{
			this.Build ();

			passentry.Changed += (object sender, EventArgs e) => {

				passentry.ModifyBase (StateType.Normal);

			};

			confentry.Changed += (object sender, EventArgs e) => {
				confentry.ModifyBase (StateType.Normal);
			};


		}


		public TrippleEntente GetEntente () {

			TrippleEntente te = new TrippleEntente ();

			Gdk.Color orchid = new Gdk.Color (218, 112, 214);

			string str = this.passentry.Text;
			string confstr = this.confentry.Text;

			if (string.IsNullOrEmpty (str)) {
				// TODO alert user
				label6.Markup = "<span fgcolor=\"red\">Password can not be blank</span>";
				label6.Show ();
				passentry.ModifyBase (StateType.Normal, orchid);
				return null;
			}

			if (string.IsNullOrEmpty (confstr)) {
				label6.Markup = "<span fgcolor=\"red\">Confirm your password</span>";
				label6.Show ();
				this.confentry.ModifyBase (StateType.Normal, orchid);
				return null;
			}

			if (!str.Equals(confstr)) {
				label6.Markup = "<span fgcolor=\"red\">Passwords do not match</span>";
				passentry.ModifyBase (StateType.Normal, orchid);
				confentry.ModifyBase (StateType.Normal, orchid);
				return null;
			}

			string pincode = this.pincodewidget1.GetEntryString ();
			var v = this.prismwidget2.CollectPrisms ();
			if (v == null) {

				label6.Markup = "<span fgcolor=\"red\">Invalid Prism Values</span>";
				label6.Show ();
				return null;
			}

			te.Pincode = pincode;
			te.ColorCrypt = v.Item1;
			te.Animal = v.Item2;
			te.Element = v.Item3;
			te.Planet = v.Item4;
			te.Card = v.Item5;
			te.Suit = v.Item6;


			te.Password = str;

			return te;

		}

		public void HideInfoBarLabels ()
		{
			this.label6.Markup = "";
			this.label6.Hide ();
			passentry.ModifyBase (StateType.Normal);
			confentry.ModifyBase (StateType.Normal);

			this.prismwidget2.HideInfoBarLabels ();
			this.pincodewidget1.HideInfoBarLabels ();
		}


		public static TrippleEntente DoDialogGuiThread ()
		{
			TrippleEntente tripple = null;
			using (TrippleEntenteCreationDialog dialog = new TrippleEntenteCreationDialog ()) {
				dialog.HideInfoBarLabels ();
				while (true) {


					ResponseType rt = (ResponseType)dialog.Run ();
					dialog.HideInfoBarLabels ();
					if (rt != ResponseType.Ok) {
						dialog.Destroy ();
						break;
					}

					tripple = dialog.GetEntente ();

					if (tripple != null) {
						dialog.Destroy ();
						break;
					}
				}
			}

			return DoDialog ();
		}
		public static TrippleEntente DoDialog () {
			TrippleEntente tripple = null;

			ManualResetEventSlim mre = new ManualResetEventSlim ();

			mre.Reset ();

			Application.Invoke( (object sender, EventArgs e) => {
				tripple = DoDialogGuiThread ();
				mre.Set();

			});
			mre.Wait ();

			return tripple;





		}
	}
}

