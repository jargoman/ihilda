using System;
using System.Diagnostics.Contracts;
using System.Threading;
using Gtk;

namespace IhildaWallet
{
	public partial class TrippleEntenteDialog : Gtk.Dialog
	{



		public TrippleEntenteDialog ()
		{
			this.Build ();

			if (label3 == null) {
				label3 = new Label ("2Factor Auth");
			}

			if (pincodewidget1 == null) {
				pincodewidget1 = new PinCodeWidget ();
				vbox3.Add ( pincodewidget1 );
			}

			if (label7 == null) {
				label7 = new Label ("Prism");
			}

			if (prismwidget1 == null) {
				prismwidget1 = new PrismWidget ();
				vbox7.Add (prismwidget1);
			}

		}




		public TrippleEntente GetEntente () {

			TrippleEntente te = new TrippleEntente ();
			
			string pincode = this.pincodewidget1.GetEntryString ();
			var v = this.prismwidget1.CollectPrisms ();
			if (v == null) {
				
				label6.Markup = "<span fgcolor=\"red\">Invalid Prism Values</span>";
				this.label6.Show ();

				return null;
			}

			te.Pincode = pincode;
			te.ColorCrypt = v.Item1;
			te.Animal = v.Item2;
			te.Element = v.Item3;
			te.Planet = v.Item4;
			te.Card = v.Item5;
			te.Suit = v.Item6;

			te.Password = passentry.Text;
			te.RememberPassword = checkbutton1.Active;


			return te;

		}

		public void HideInfoBarLabels ()
		{
			this.label6.Markup = "";
			this.label6.Hide ();
			passentry.ModifyBase (StateType.Normal);

			pincodewidget1.HideInfoBarLabels ();
			prismwidget1.HideInfoBarLabels ();



		}

		/*
		private static ColorCrypts GetColorConsole ()
		{
		
			do {
				string pass = Program.GetPassword ();

				bool success = Enum.TryParse (pass, out ColorCrypts color);

				if (success) {
					return color;
				}

			} while (true);
			

		}
		*/


		public static TrippleEntente DoDialogConsole ()
		{

			Logging.WriteLog ("Please enter your password : ");
			string password = Program.GetPassword ();

			Logging.WriteLog ("Please enter your pin code");
			string pin = Program.GetPassword ();

			TrippleEntente entente = new TrippleEntente {
				ColorCrypt = GetEnumConsole<ColorCrypts> (),
				Animal = GetEnumConsole<Animals> (),
				Element = GetEnumConsole<Elements> (),
				Planet = GetEnumConsole<Planet> (),
				Card = GetEnumConsole<Cards> (),
				Suit = GetEnumConsole<Suits> (),
				Pincode = pin,
				Password = password
			};

			return entente;
		}

		public static TEnum GetEnumConsole<TEnum> () where TEnum : struct
		{
			//Contract.Ensures (Contract.Result<Enum> () != null);

			do {
				TEnum ret = default(TEnum);
				Logging.WriteLog ("Enter your " + ret.GetType().ToString() + " : ");

				string pass = Program.GetPassword ();

				bool success = Enum.TryParse (pass, out ret);

				if (success) {
					return ret;
				}

				Logging.WriteLog (pass + "is not of type " + ret.GetType ().ToString ());

			} while (true);

		}

		public static TrippleEntente DoDialog ()
		{

			if ( Program.botMode != null ) {
				return DoDialogConsole ();
			}

			TrippleEntente tripple = null;
			using (ManualResetEventSlim mre = new ManualResetEventSlim ()) {
				mre.Reset ();
				Application.Invoke ((object sender, EventArgs e) => {
					using (TrippleEntenteDialog dialog = new TrippleEntenteDialog ()) {
						dialog.HideInfoBarLabels ();
						while (tripple == null) {

							//dialog.HideInfoBarLabels ();
							ResponseType rt = (ResponseType)dialog.Run ();
							dialog.HideInfoBarLabels ();
							if (rt != ResponseType.Ok) {
								//dialog.Destroy ();
								break;
							}

							tripple = dialog.GetEntente ();

							/*
							if (tripple != null) {
							    break;
							}
							*/

						}

						dialog.Destroy ();


					}
					mre.Set ();

				});
				mre.Wait ();
			}

			return tripple;


		}




	}
}

