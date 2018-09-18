using System;
using RippleLibSharp.Keys;
using RippleLibSharp.Binary;
using Gtk;

namespace IhildaWallet
{
	public partial class SeedFromHexDialog : Gtk.Dialog
	{
		public SeedFromHexDialog ()
		{
			this.Build ();


		}


		public string GetSeed ()
		{
			string hexstr = comboboxentry1.ActiveText;

			if (string.IsNullOrWhiteSpace (hexstr)) {
				return null;
			}


			byte[] bytes = Base58.StringToByteArray (hexstr);

			string base58encoded = "test"; //Base58.Encode (bytes);

			try {
				RipplePrivateKey ripplePrivateKey = new RipplePrivateKey (bytes);
				base58encoded = ripplePrivateKey.ToString ();
			} catch (Exception e) {
				return "error";
			}



			return base58encoded;

		}


		public static string DoDialog ()
		{
			using (SeedFromHexDialog seedFromHexDialog = new SeedFromHexDialog ()) {
				
				Gtk.ResponseType reponse = (ResponseType)seedFromHexDialog.Run ();
				string seedAddress = seedFromHexDialog.GetSeed ();
				seedFromHexDialog.Destroy ();

				if (reponse == ResponseType.Ok) {
					return seedAddress;
				}

				return null;
			}
		

		}
	}
}
