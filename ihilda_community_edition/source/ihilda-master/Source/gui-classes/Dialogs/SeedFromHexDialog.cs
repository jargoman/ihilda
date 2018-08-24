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


		public RippleSeedAddress GetSeed ()
		{
			string hexstr = comboboxentry1.ActiveText;

			if (string.IsNullOrWhiteSpace (hexstr)) {
				return null;
			}


			byte[] bytes = Base58.StringToByteArray (hexstr);

			try {

				RippleSeedAddress rippleSeedAddress = new RippleSeedAddress (bytes);

				return rippleSeedAddress;
			} catch (Exception e) {
				return null;
			}

		}


		public static RippleSeedAddress DoDialog ()
		{
			SeedFromHexDialog seedFromHexDialog = new SeedFromHexDialog ();

			Gtk.ResponseType reponse = (ResponseType)seedFromHexDialog.Run ();
			if (reponse == ResponseType.Ok) {
				return seedFromHexDialog.GetSeed ();
			}

			return null;

		}
	}
}
