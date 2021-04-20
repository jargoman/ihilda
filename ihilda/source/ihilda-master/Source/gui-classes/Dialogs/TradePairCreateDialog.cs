using Gtk;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public partial class TradePairCreateDialog : Gtk.Dialog
	{
		public TradePairCreateDialog ()
		{
			this.Build ();
		}

		public TradePairCreateDialog (TradePair preset)
		{
			this.Build ();

			if (tradepairentrywidget1 == null) {
				tradepairentrywidget1 = new TradePairEntryWidget ();
				tradepairentrywidget1.Show ();

				vbox2.Add (tradepairentrywidget1);
			}


			this.tradepairentrywidget1.SetTradePair (preset);
		}

		public TradePair GetTradePair ()
		{

			return this.tradepairentrywidget1.GetTradePair (true);
		}

		public void SetAddress (RippleWallet rippleWallet) {
			this.tradepairentrywidget1.SetAddress (rippleWallet.Account);
		}

		public static TradePair DoDialog (RippleWallet rippleWallet = null)
		{
			return DoDialog (null, rippleWallet);
		}

		public static TradePair DoDialog (TradePair preset, RippleWallet rippleWallet = null)
		{


			using (TradePairCreateDialog tpcd = new TradePairCreateDialog (preset)) {
				string address = rippleWallet?.Account;
				if (address != null) {
					tpcd.SetAddress (rippleWallet);
				}
				while (true) {

					TradePair tp = null;

					tpcd.Show ();
					ResponseType res = (ResponseType)tpcd.Run ();
					tpcd.Hide ();


					if (res != ResponseType.Ok) {
						tpcd.Destroy ();
						break;
					}

					tp = tpcd.GetTradePair ();

					if (tp != null) {
						tpcd.Destroy ();
						return tp;
					}





				}
			}

			return null;

		}

#if DEBUG
		private const string clsstr = nameof (TradePairCreateDialog) + DebugRippleLibSharp.colon;
#endif
	}
}

