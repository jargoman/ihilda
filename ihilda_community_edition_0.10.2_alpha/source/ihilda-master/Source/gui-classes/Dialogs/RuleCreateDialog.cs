using System;

using Gtk;

namespace IhildaWallet
{
	public partial class RuleCreateDialog : Gtk.Dialog
	{
		public RuleCreateDialog ()
		{
			this.Build ();


		}

		public OrderFilledRule GetRule () {

			OrderFilledRule rule = this.tradepairentrywidget1.GetFillRule ();



			string payless = this.comboboxentry1.Entry.Text;
			string getmore = this.comboboxentry2.Entry.Text;


			bool b1 = Decimal.TryParse (payless, out decimal p);
			bool b2 = Decimal.TryParse (getmore, out decimal g); 

			Decimal min_profit = 1.005m;
			if (b1 == false || b2 == false || p < min_profit || g < min_profit) {
				string message = "Both Pay Less and Get More must be Decimal values greater than " + min_profit.ToString ();

				MessageDialog.ShowMessage (message);
				return null;
			}
			rule.RefillMod = new ProfitStrategy (p, g);

			return rule;

		}


		public static OrderFilledRule DoDialog () {

			RuleCreateDialog rcd = new RuleCreateDialog ();
			OrderFilledRule ret = null;

			do {
				ResponseType res = (ResponseType)rcd.Run ();

				/* 
				 * One should prefer ResponseType != ok rather than ResponseType == cancel
				 * !ok would also include the window being closed prematurely 
				 */
				if (res != ResponseType.Ok) {
					//return null;
					ret = null;
					break;
				}

				ret = rcd.GetRule();


			} while (ret == null);

			rcd.Destroy ();

			return ret;
		}
	}
}

