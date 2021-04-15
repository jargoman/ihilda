using System;
using RippleLibSharp.Keys;
using RippleLibSharp.Binary;
using Gtk;
using RippleLibSharp.Util;

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

#if DEBUG
			string method_sig = clsstr + nameof (GetSeed) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.SeedFromHexDialog) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif
			string hexstr = comboboxentry1.ActiveText;

			if (string.IsNullOrWhiteSpace (hexstr)) {
				return null;
			}


			byte[] bytes = Base58.HexStringToByteArray (hexstr);

			//string base58encoded = "test";
			string base58encoded = Base58.Encode (bytes);
			try {
				RipplePrivateKey ripplePrivateKey = new RipplePrivateKey (bytes);
				base58encoded = ripplePrivateKey.ToString ();
			} catch (Exception e) {

#if DEBUG
				if (DebugIhildaWallet.SeedFromHexDialog) {
					Logging.ReportException (method_sig, e);
				}
#endif
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


#if DEBUG
		private const string clsstr = nameof (SeedFromHexDialog) + DebugRippleLibSharp.colon;
#endif

	}
}
