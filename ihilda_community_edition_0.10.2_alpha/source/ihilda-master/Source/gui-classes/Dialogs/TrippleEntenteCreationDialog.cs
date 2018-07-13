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
		}


		public TrippleEntente GetEntente () {

			TrippleEntente te = new TrippleEntente ();

			string pincode = this.pincodewidget1.GetEntryString ();
			var v = this.prismwidget2.collectPrisms ();

			te.Pincode = pincode;
			te.ColorCrypt = v.Item1;
			te.Animal = v.Item2;
			te.Element = v.Item3;
			te.Planet = v.Item4;
			te.Card = v.Item5;
			te.Suit = v.Item6;


			string str = this.passentry.Text;
			string confstr = this.confentry.Text;

			if (string.IsNullOrEmpty (str)) {
				// TODO alert user
				return null;
			}

			if (!str.Equals(confstr)) {
				return null;
			}

			te.Password = str;

			return te;

		}

		public static TrippleEntente DoDialog () {
			TrippleEntente tripple = null;

			ManualResetEventSlim mre = new ManualResetEventSlim ();
			mre.Reset ();
			Application.Invoke( (object sender, EventArgs e) => {
				using (TrippleEntenteCreationDialog dialog = new TrippleEntenteCreationDialog ()) {
				while (true) {


					ResponseType rt = (ResponseType)dialog.Run ();

					if (rt != ResponseType.Ok) {
						
						break;
					}

					tripple = dialog.GetEntente();

					if (tripple != null) {
						break;
					}
				}
				}
				mre.Set();

			});
			mre.Wait ();

			return tripple;





		}
	}
}

