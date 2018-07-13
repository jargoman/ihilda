using System;
using System.Threading;
using Gtk;

namespace IhildaWallet
{
	public partial class TrippleEntenteDialog : Gtk.Dialog
	{
		public TrippleEntenteDialog ()
		{
			this.Build ();

		}




		public TrippleEntente GetEntente () {

			TrippleEntente te = new TrippleEntente ();
			
			string pincode = this.pincodewidget2.GetEntryString ();
			var v = this.prismwidget1.collectPrisms ();

			te.Pincode = pincode;
			te.ColorCrypt = v.Item1;
			te.Animal = v.Item2;
			te.Element = v.Item3;
			te.Planet = v.Item4;
			te.Card = v.Item5;
			te.Suit = v.Item6;

			te.Password = passentry.Text;

			return te;

		}


		public static TrippleEntente DoDialog () {


			TrippleEntente tripple = null;

			ManualResetEventSlim mre = new ManualResetEventSlim ();
			mre.Reset ();
			Application.Invoke( (object sender, EventArgs e) => {
				using (TrippleEntenteDialog dialog = new TrippleEntenteDialog ()) {
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

					dialog.Destroy();

				
				}
				mre.Set();

			});
			mre.Wait ();

			return tripple;


		}




	}
}

