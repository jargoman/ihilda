
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using IhildaWallet.Networking;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Keys;
using System.Linq;

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
				string baseCur = tradepairentrywidget1.GetBaseCurrency ();
				string countCur = tradepairentrywidget1.GetCounterCurrency ();

				if (string.IsNullOrWhiteSpace (baseCur)) {
					label16.Text = "";
					label16.Visible = false;
					label18.Text = "";
					label18.Visible = false;
				} else {
					label16.Text = baseCur;
					label16.Visible = true;
					label18.Text = baseCur;
					label18.Visible = true;
				}

				if (string.IsNullOrWhiteSpace (countCur)) {
					label17.Text = "";
					label17.Visible = false;
					label19.Text = "";
					label19.Visible = false;
				} else {
					label17.Text = countCur;
					label17.Visible = true;
					label19.Text = countCur;
					label19.Visible = true;

				}
			};

			onebutton.Clicked += (sender, e) => {
				SetPercentage (1.002);
			};

			onep5button.Clicked += (sender, e) => {
				SetPercentage (1.007);
			};

			twobutton.Clicked += (sender, e) => {
				SetPercentage (1.012);
			};

			twop5button.Clicked += (sender, e) => {
				SetPercentage (1.017);
			};

			Modal = true;



		}

		public RuleCreateDialog (RippleAddress address) : this()
		{
			if (address != null) {
				tradepairentrywidget1.SetAddress (address);
			}
		}


		public void SetPercentage (double speculate) {
			comboboxentry1.Entry.Text = 1.005.ToString();
			comboboxentry2.Entry.Text = 1.005.ToString ();
			comboboxentry3.Entry.Text = speculate.ToString ();
		}



		public void SetToolTips ()
		{
			if (!ProgramVariables.showPopUps) {

				return;
			}

			label14.TooltipMarkup = "Pattern filled order must match to trigger rule";
			label15.TooltipMarkup = "Action to apply to newly created order once rule has been triggered";

			//StringBuilder help = new StringBuilder ();
			String help = "Mark your newly created order with this value\n" +
				"* will mark it the same as the mark(string or integer) which triggered this rule\n" +
				"++ adds one to the mark<span fgcolor=\"grey\">(integer)</span> triggering the order";

			label11.TooltipMarkup = help;
			comboboxentry5.TooltipMarkup = help;

			help = "Pay x amount less when rebuying the same amount\n" +
				"Example 1.01 = rebuy sold amount at 1% profit";

			label3.TooltipMarkup = help;
			comboboxentry1.TooltipMarkup = help;

			help = "Get x amount more than was sold\n" +
				"Example 1.01 = rebuy 1% more 1% cheaper";

			label4.TooltipMarkup = help;
			comboboxentry2.TooltipMarkup = help;

			help = "Same as payless but to the exponent mark\n" +
				"Mark must be an integer greater than zero\n" +
				"Example 1.01 = rebuy same amount @ 1.01^(mark) cheaper";

			label13.TooltipMarkup = help;
			comboboxentry6.TooltipMarkup = help;

			help = "Same as getmore except to the exponet mark\n" +
				"Mark must be an integer greater than zero\n" +
				"Example 1.01 = rebuy 1.01^(mark) cheaper and get 1.01^(mark) more";

			label12.TooltipMarkup = help;
			comboboxentry7.TooltipMarkup = help;

			help = "Trigger this rule when this currency is bought\n" +
				"Issuer may be left empty to match any issuer";

			label5.TooltipMarkup = help;


			help = "Trigger this rule when this currency was sold\n" +
				"Issuer may be left empty to match any issuer";

			label6.TooltipMarkup = help;

			help = "Trigger this rule when filled order is marked with this value\n" +
				"Use [1-100] to match a range of numbers\n" +
				"Use [blue,green,yellow] to match more than one mark\n" +
				"Use * to match any mark that isn't blank\n" +
				"Leave blank to match orders created with other trading clients";

			label10.TooltipMarkup = help;
			comboboxentry4.TooltipMarkup = help;
		}

		public OrderFilledRule GetRule () {

			OrderFilledRule rule = this.tradepairentrywidget1?.GetFillRule ();

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

			return _DoDialog (rcd, filledRule);

		}

		public static OrderFilledRule DoDialog (RippleWallet wallet, OrderFilledRule filledRule = null)
		{
			RuleCreateDialog rcd = new RuleCreateDialog (wallet?.GetStoredReceiveAddress());
			return _DoDialog (rcd, filledRule);
		}

		private static OrderFilledRule _DoDialog (RuleCreateDialog rcd, OrderFilledRule filledRule = null)
		{


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

				ret = rcd.GetRule ();


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

