using System;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using IhildaWallet.Networking;
using IhildaWallet.Util;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AutomatedSellWidget : Gtk.Bin
	{
		public AutomatedSellWidget ()
		{
			this.Build ();

			this.amountcomboboxentry.Changed += (object sender, EventArgs e) => {
				CalculateMax ();
			};

			this.pricecomboboxentry.Changed += (object sender, EventArgs e) => {
				CalculateMax ();
			};

			this.sellbutton.Clicked += Sellbutton_Clicked;

			Label l = (Label)this.sellbutton.Child;
			l.UseMarkup = true;

			this.label19.Text = "";
			this.label20.Text = "";

			button114.Clicked += PercentageClicked;
			button115.Clicked += PercentageClicked;
			button111.Clicked += PercentageClicked;
			button112.Clicked += PercentageClicked;

			button123.Clicked += PercentageClicked;
			button113.Clicked += PercentageClicked;
			button119.Clicked += PercentageClicked;

			button116.Clicked += PercentageClicked;

			button120.Clicked += PercentageClicked;

			button117.Clicked += PercentageClicked;

			button118.Clicked += PercentageClicked;

			button121.Clicked += PercentageClicked;

			button122.Clicked += PercentageClicked;

			hscale2.ValueChanged += Hscale2_ValueChanged;

		}

		void PercentageClicked (object sender, EventArgs e)
		{
			if (sender is Button b) {
				string s = b?.Label.TrimEnd ('%');
				double d = Convert.ToDouble (s);

				hscale2.Value = d;
			}
		}


		void Hscale2_ValueChanged (object sender, EventArgs e)
		{
			string acc = _rippleWallet.Account;

			NetworkInterface ni = NetworkController.GetNetworkInterfaceGuiThread ();

			CancellationTokenSource tokenSource = new CancellationTokenSource ();
			CancellationToken token = tokenSource.Token;
			double val = hscale2.Value;

			if (!_TradePair.Currency_Base.IsNative) {
				var cur = AccountLines.GetBalanceForIssuer
				(
					_TradePair.Currency_Base.currency,
					_TradePair.Currency_Base.issuer,
					acc,
					ni,
					token
				);

				double bal = (double)cur.amount;

				double res = bal * val / 100;

				amountcomboboxentry.Entry.Text = res.ToString ();
			} else {

				Task<Response<AccountInfoResult>> task =
					AccountInfo.GetResult (acc, ni, token);

				task.Wait (token);

				Response<AccountInfoResult> resp = task.Result;
				AccountInfoResult res = resp.result;

				var reserve = res.GetReserveRequirements (ni, token);

				RippleCurrency rippleCurrency = new RippleCurrency (res.account_data.Balance);

				double bal = (double)(rippleCurrency.amount - reserve.amount) / 1000000 * val / 100;

				amountcomboboxentry.Entry.Text = bal.ToString ();

			}
		}



		public void SetAmount (Decimal amount)
		{
			amountcomboboxentry.Entry.Text = amount.ToString ();
		}


		public void SetPrice (Decimal price)
		{
			pricecomboboxentry.Entry.Text = price.ToString ();
		}

		public void SetAmountMax ()
		{
			this.hscale2.Value = 100.0;
		}


		public void SetOffer (Offer off)
		{
#if DEBUG
			string method_sig = clsstr + nameof(SetOffer) + DebugRippleLibSharp.left_parentheses + nameof (Offer) + DebugRippleLibSharp.space_char + nameof (off) + DebugRippleLibSharp.equals + DebugIhildaWallet.ToAssertString (off) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.AutomatedSellWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			AutomationWarning = true;

			if (off == null) {
#if DEBUG
				if (DebugIhildaWallet.AutomatedSellWidget) {
					Logging.WriteLog (method_sig + "off == null\n");
				}
#endif

				Application.Invoke (
					delegate {
						this.amountcomboboxentry.Entry.Text = "";
						this.pricecomboboxentry.Entry.Text = "";
						this.maxcomboboxentry.Entry.Text = "";
					}
				);

				return;
			}

			if (off.taker_gets != null) {
#if DEBUG
				if (DebugIhildaWallet.AutomatedSellWidget) {
					Logging.WriteLog (method_sig + "off.TakerGets != null\n");
				}
#endif

				Decimal amount = off.taker_gets.amount;
				if (off.taker_gets.IsNative) {
					amount /= 1000000;
				}

				string tex = amount.ToString ();

				Application.Invoke (
					delegate {
						if (amountcomboboxentry?.Entry != null) {

						}
						this.amountcomboboxentry.Entry.Text = tex;
					}
				);

			}

			if (off.taker_pays != null) {
#if DEBUG
				if (DebugIhildaWallet.AutomatedSellWidget) {

				}
#endif

				string tex = off.taker_gets.GetNativeAdjustedPriceAt (off.TakerPays).ToString ();

				Application.Invoke (
					delegate {
						this.pricecomboboxentry.Entry.Text = tex;
					}
				);

			}
		}

		void Sellbutton_Clicked (object sender, EventArgs e)
		{

			TextHighlighter highlighter = new TextHighlighter ();

			Decimal? amoun = RippleCurrency.ParseDecimal (amountcomboboxentry?.ActiveText);
			if (amoun == null) {
				var message = "Invalid buy amount\n";
				highlighter.Highlightcolor = TextHighlighter.RED;
				infobar.Markup = highlighter.Highlight (message);
				return;
			}
			Decimal? maxPrice = RippleCurrency.ParseDecimal (pricecomboboxentry?.ActiveText);
			if (maxPrice == null) {
				var message = "Invalid Price Price\n";
				highlighter.Highlightcolor = TextHighlighter.RED;
				infobar.Markup = highlighter.Highlight (message);
				return;
			}

			bool sane = _parent.SafetyCheck ((Decimal)maxPrice, "sell");
			if (!sane) {
				return;
			}

			Decimal? maxValue = RippleCurrency.ParseDecimal (maxcomboboxentry?.ActiveText);
			if (maxValue == null) {
				var message = "Invalid max value\n";
				highlighter.Highlightcolor = TextHighlighter.RED;
				infobar.Markup = highlighter.Highlight (message);
				return;
			}
			Int32? minTx = RippleCurrency.ParseInt32 (comboboxentry3?.ActiveText);
			if (minTx == null) {
				var message = "Invalid minTx\n";
				highlighter.Highlightcolor = TextHighlighter.RED;
				infobar.Markup = highlighter.Highlight (message);
				return;

			}
			Int32? maxTx = RippleCurrency.ParseInt32 (comboboxentry4?.ActiveText);
			if (maxTx == null) {
				var message = "Invalid maxTx";
				highlighter.Highlightcolor = TextHighlighter.RED;
				infobar.Markup = highlighter.Highlight (message);
				return;

			}

			//RippleOfferTransaction tx = new RippleOfferTransaction(rw.getStoredReceiveAddress(), off);

			AutomatedOrder automatedOrder = CreateOffer ();


			Task.Run (
				() => { 
					LicenseType licenseT = Util.LicenseType.SEMIAUTOMATED;
					if (LeIceSense.IsLicenseExempt (automatedOrder.taker_gets) || LeIceSense.IsLicenseExempt (automatedOrder.taker_pays)) {
						licenseT = LicenseType.NONE;
					}

					bool shouldCont = LeIceSense.LastDitchAttempt (_rippleWallet, licenseT);

					if (!shouldCont) {
						return;
					}

					Application.Invoke (
						delegate {

							if (AutomationWarning) {

								bool ignored = AreYouSure.AutomatedTradingWarning ();
								if (!ignored) return;
							}

							AutomatedPurchaseWindow automatedPurchaseWindow = new AutomatedPurchaseWindow (_rippleWallet, automatedOrder, (Int32)minTx, (Int32)maxTx);

							automatedPurchaseWindow.Show ();

						}
					);

				}
			);



		}


		public AutomatedOrder CreateOffer ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (CreateOffer) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.AutomatedSellWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif


			TextHighlighter highlighter = new TextHighlighter ();

			if (
				this.amountcomboboxentry == null
				|| this.pricecomboboxentry == null
				|| this.maxcomboboxentry == null
			) {
				// todo gui in state of disarray, debug
#if DEBUG
				if (DebugIhildaWallet.AutomatedSellWidget) {
					Logging.WriteLog (method_sig + "this.amountcomboboxentry == null || this.pricecomboboxentry == null || this.maxcomboboxentry == null\n");
				}
#endif
				return null;
			}

			if (this._TradePair == null) {
				// todo debug. warn user?
#if DEBUG
				if (DebugIhildaWallet.AutomatedSellWidget) {
					Logging.WriteLog (method_sig + "this._tradePair == null\n");
				}
#endif
				return null;
			}

			//RippleWallet rw = MainWindow.currentInstance.getRippleWallet();

			/*
			RippleWallet rw = WalletManager.selectedWallet;
			if (rw == null) {
				#if DEBUG
				if (Debug.SellWidget) {
					Logging.writeLog (method_sig + "w == null, returning\n");
				}
				#endif
			}
			RippleSeedAddress seed = rw.getDecryptedSeed ();

			#if DEBUG
			if (Debug.SellWidget) {
				Logging.writeLog (method_sig + "rw = ", Debug.toAssertString(rw));
				Logging.writeLog (method_sig + "seed = ", Debug.toAssertString(seed));
			}
			#endif
			*/

			TradePair tp = this._TradePair.DeepCopy ();
			if (tp == null) {
#if DEBUG
				if (DebugIhildaWallet.AutomatedSellWidget) {
					Logging.WriteLog (method_sig + "tp == null, returning\n");
				}
#endif
				return null;
			}

#if DEBUG
			if (DebugIhildaWallet.AutomatedSellWidget) {
				Logging.WriteLog (method_sig + "tp.currency_base =" + tp.Currency_Base.ToString ());
				Logging.WriteLog (method_sig + "tp.currency_counter =" + tp.Currency_Counter.ToString ());
			}
#endif





			SellOffer off = new SellOffer ();

			//off.taker_gets = tp.currency_base;
			//off.taker_pays = tp.currency_counter;

			off.SetFromTradePair (tp);

			Decimal? getamount = RippleCurrency.ParseDecimal (amountcomboboxentry.ActiveText);
			if (getamount == null) {
				var message = off.taker_gets.currency + " getsamount is formatted incorrectly\n";

				highlighter.Highlightcolor = TextHighlighter.RED;
				infobar.Markup = highlighter.Highlight (message);

#if DEBUG
				if (DebugIhildaWallet.AutomatedSellWidget) {
					Logging.WriteLog (method_sig + "getsamount == null\n");
				}
#endif
				return null;
			}

			Decimal? payamount = RippleCurrency.ParseDecimal (maxcomboboxentry.ActiveText);
			if (payamount == null) {

				var message = off.taker_pays.currency + " payamount is formatted incorrectly\n";

				highlighter.Highlightcolor = TextHighlighter.RED;
				infobar.Markup = highlighter.Highlight (message);

#if DEBUG
				if (DebugIhildaWallet.AutomatedSellWidget) {
					Logging.WriteLog (method_sig + "payamount == null\n");
				}
#endif
				return null;
			}

			Decimal? price = RippleCurrency.ParseDecimal (this.pricecomboboxentry.ActiveText);
			if (price == null) {
				string message = "price is formatted incorrectly\n";
				highlighter.Highlightcolor = TextHighlighter.RED;
				infobar.Markup = highlighter.Highlight (message);
				return null;
			}

			

			off.taker_pays.amount = off.taker_pays.IsNative ? (Decimal)payamount * 1000000 : (Decimal)payamount;
			off.taker_gets.amount = off.taker_gets.IsNative ? (Decimal)getamount * 1000000 : (Decimal)getamount;


			off.Account = _rippleWallet.GetStoredReceiveAddress();


			return off;


		}

		private void CalculateMax ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (CalculateMax) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.AutomatedSellWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif


			TextHighlighter highlighter = new TextHighlighter ();

			String text = this.amountcomboboxentry.ActiveText;
#if DEBUG
			if (DebugIhildaWallet.AutomatedSellWidget) {
				Logging.WriteLog (method_sig + "this.amountcomboboxentry.ActiveText = " + DebugIhildaWallet.ToAssertString (text) + "\n");
			}
#endif

			Decimal? am = RippleCurrency.ParseDecimal (text);
			if (am == null) {

				var message = "Amount is formatted incorrectly";

				highlighter.Highlightcolor = TextHighlighter.RED;
				infobar.Markup = highlighter.Highlight (message);

				// TODO color amount entry red


#if DEBUG
				if (DebugIhildaWallet.AutomatedSellWidget) {
					Logging.WriteLog (method_sig + "am==null, returning\n");
				}
#endif
				return;
			}

			String text2 = this.pricecomboboxentry.ActiveText;
#if DEBUG
			if (DebugIhildaWallet.AutomatedSellWidget) {
				Logging.WriteLog (method_sig + "this.pricecomboboxentry.ActiveText = " + DebugIhildaWallet.ToAssertString (text2) + "\n");
			}
#endif
			Decimal? pr = RippleCurrency.ParseDecimal (text2);

			if (pr == null) {

				//MessageDialog.showMessage ("Price is formatted incorrectly\n");
#if DEBUG
				if (DebugIhildaWallet.AutomatedSellWidget) {
					Logging.WriteLog (method_sig + "pr==null, returning\n");
				}
#endif
				return;
			}

			try {
				Decimal d = (Decimal)(am * pr);
				maxcomboboxentry.Entry.Text = d.ToString ();
			}

#pragma warning disable 0168
			catch (Exception ee) {
#pragma warning restore 0168

				// TODO color
#if DEBUG
				if (DebugIhildaWallet.AutomatedSellWidget) {
					Logging.WriteLog (method_sig + "Exception thrown\n");
					Logging.WriteLog (ee.Message);
				}
#endif
				return;
			}




		}


		public TradePair TradePairInstance {

			get {
				return _TradePair;
			}

			set {
				this._TradePair = value;

				bool requirements = (this._TradePair != null) && this._TradePair.HasRequirements ();

				String b = requirements ? _TradePair.Currency_Base.currency : "";
				String c = requirements ? _TradePair.Currency_Counter.currency : "";

				Gtk.Application.Invoke (
				delegate {

					label10.Markup = "<span foreground=\"red\"><b><u>Sell " + b + "</u></b></span>";
					label12.Text = b;
					label15.Text = c;
					label16.Text = c;
				}
				);
			}
		}

		#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private TradePair _TradePair = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant


		public bool AutomationWarning { get; set; }

		internal void SetParent (TradeWindow parent)
		{
			this._parent = parent;
		}

		private TradeWindow _parent = null;

		internal void SetRippleWallet (RippleWallet rw)
		{
			_rippleWallet = rw;
		}

		private RippleWallet _rippleWallet = null;

		#if DEBUG
		private static readonly string clsstr = nameof (AutomatedSellWidget) + DebugRippleLibSharp.colon;
#endif

	}
}

