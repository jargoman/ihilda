using System;

using Gtk;

namespace IhildaWallet
{
	public partial class RuleCreateDialog : Gtk.Dialog
	{
		public RuleCreateDialog ()
		{
			this.Build ();

			this.label16.Text = "";
			this.label17.Text = "";
			this.label18.Text = "";
			this.label19.Text = "";
			this.label20.Text = "";

			if (tradepairentrywidget1 == null) {
				tradepairentrywidget1 = new TradePairEntryWidget ();

				table1.Attach (tradepairentrywidget1, 1, 2, 0, 3);
			}

			tradepairentrywidget1.WidgetChanged += (object sender, EventArgs e) => {
				TradePair tradePair = tradepairentrywidget1.GetTradePair ();

				if (tradePair == null) {
					return;
				}

				if (string.IsNullOrWhiteSpace (tradePair.Currency_Base?.currency)) {
					label16.Text = "";
					label16.Visible = false;
					label18.Text = "";
					label18.Visible = false;
				} else {
					label16.Text = tradePair.Currency_Base.currency;
					label16.Visible = true;
					label18.Text = tradePair.Currency_Base.currency;
					label18.Visible = true;
				}

				if (string.IsNullOrWhiteSpace (tradePair.Currency_Counter?.currency)) {
					label17.Text = "";
					label17.Visible = false;
					label19.Text = "";
					label19.Visible = false;
				} else {
					label17.Text = tradePair.Currency_Counter.currency;
					label17.Visible = true;
					label19.Text = tradePair.Currency_Counter.currency;
					label19.Visible = true;

				}
			};

			Modal = true;


		}

		public OrderFilledRule GetRule () {

			OrderFilledRule rule = this.tradepairentrywidget1.GetFillRule ();

			string marking = this.comboboxentry4?.Entry?.Text?.Trim ();
			string markas = this.comboboxentry5?.Entry?.Text?.Trim ();

			string payless = this.comboboxentry1?.Entry?.Text?.Trim();
			string getmore = this.comboboxentry2?.Entry?.Text?.Trim ();
			string speculate = this.comboboxentry3?.Entry?.Text?.Trim ();

			string exppayless = this.comboboxentry6?.Entry?.Text?.Trim ();
			string expgetmore = this.comboboxentry7?.Entry?.Text?.Trim ();

			bool b1 = Decimal.TryParse (payless, out decimal pay_less_dec);
			bool b2 = Decimal.TryParse (getmore, out decimal get_more_dec);


			Decimal min_profit = 1.002m;
			if (b1 == false || b2 == false || pay_less_dec < min_profit || get_more_dec < min_profit) {
				string message = "Both Pay Less and Get More must be Decimal values greater than " + min_profit.ToString ();

				MessageDialog.ShowMessage (message);
				return null;
			}

			decimal spec = Decimal.Zero;
			if (!string.IsNullOrWhiteSpace(speculate)) {
				bool b3 = Decimal.TryParse (speculate, out spec);
				if (b3 == false) {
					string message = "Speculate must be a decimal value or ommited for default value zero";
					MessageDialog.ShowMessage (message);
					return null;
				}
			}

			decimal exp_pay_less_dec = Decimal.Zero;
			decimal exp_get_more_dec = Decimal.Zero;

			if (!string.IsNullOrWhiteSpace (exppayless)) {
				bool b4 = Decimal.TryParse (exppayless, out exp_pay_less_dec);
				if (!b4) {
					string message = "Exponential pay less must be a valid decimal";
					MessageDialog.ShowMessage (message);
					return null;
				}
			}

			if (!string.IsNullOrWhiteSpace (expgetmore)) {
				bool b5 = Decimal.TryParse (expgetmore, out exp_get_more_dec);
				if (!b5) {
					string message = "Exponential get more must be a valid decimal";
					MessageDialog.ShowMessage (message);
					return null;
				}
			}

			rule.RefillMod = new ProfitStrategy (pay_less_dec, get_more_dec, spec) {
				Exp_Pay_Less = exp_pay_less_dec,
				Exp_Get_More = exp_get_more_dec
			};

			rule.Mark = marking;
			rule.MarkAs = markas;

			return rule;

		}


		public static OrderFilledRule DoDialog (OrderFilledRule filledRule = null) {

			RuleCreateDialog rcd = new RuleCreateDialog ();
			rcd.SetRule (filledRule);
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

		private void SetRule (OrderFilledRule filledRule)
		{
			if (filledRule == null) {
				return;
			}


			TradePair tradePair = new TradePair () {
				Currency_Base = filledRule?.BoughtCurrency,
				Currency_Counter = filledRule?.SoldCurrency
			};


			tradepairentrywidget1.SetTradePair (tradePair);

			comboboxentry4.Entry.Text = filledRule.Mark;
			comboboxentry5.Entry.Text = filledRule.MarkAs;

			comboboxentry1.Entry.Text = filledRule.RefillMod?.Pay_Less.ToString();
			comboboxentry2.Entry.Text = filledRule.RefillMod?.Get_More.ToString();
			comboboxentry3.Entry.Text = filledRule.RefillMod?.Speculate.ToString();

			comboboxentry6.Entry.Text = filledRule.RefillMod?.Exp_Pay_Less.ToString ();
			comboboxentry7.Entry.Text = filledRule.RefillMod?.Exp_Get_More.ToString ();

		}
	}
}

