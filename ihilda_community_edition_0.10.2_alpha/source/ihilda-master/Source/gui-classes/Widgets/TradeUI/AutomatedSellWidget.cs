using System;
using Gtk;
using IhildaWallet.Util;
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

			this.label13.Text = "";
			this.label14.Text = "";

		}


		public void SetOffer (Offer off)
		{
#if DEBUG
			string method_sig = clsstr + nameof(SetOffer) + DebugRippleLibSharp.left_parentheses + nameof (Offer) + DebugRippleLibSharp.space_char + nameof (off) + DebugRippleLibSharp.equals + DebugIhildaWallet.ToAssertString (off) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.AutomatedSellWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

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
			Decimal? amoun = RippleCurrency.ParseDecimal (amountcomboboxentry?.ActiveText);
			if (amoun == null) {
				MessageDialog.ShowMessage ("Invalid buy amount\n");
				return;
			}
			Decimal? maxPrice = RippleCurrency.ParseDecimal (amountcomboboxentry?.ActiveText);
			if (maxPrice == null) {
				MessageDialog.ShowMessage ("Invalid Price Amount\n");
				return;
			}

			Decimal? maxValue = RippleCurrency.ParseDecimal (maxcomboboxentry?.ActiveText);
			if (maxValue == null) {
				MessageDialog.ShowMessage ("Invalid max value\n");
				return;
			}
			Int32? minTx = RippleCurrency.ParseInt32 (comboboxentry3?.ActiveText);
			if (minTx == null) {
				MessageDialog.ShowMessage ("Invalid minTx\n");
				return;

			}
			Int32? maxTx = RippleCurrency.ParseInt32 (comboboxentry4?.ActiveText);
			if (maxTx == null) {
				MessageDialog.ShowMessage ("Invalid maxTx");
				return;

			}

			//RippleOfferTransaction tx = new RippleOfferTransaction(rw.getStoredReceiveAddress(), off);
			LicenseType licenseT = Util.LicenseType.SEMIAUTOMATED;
			//if (LeIceSense.IsLicenseExempt (off.taker_gets) || LeIceSense.IsLicenseExempt (off.taker_pays)) {
			//		licenseT = LicenseType.NONE;
			//	}

			AutomatedOrder automatedOrder = CreateOffer ();

			AutomatedPurchaseWindow automatedPurchaseWindow = new AutomatedPurchaseWindow (_rippleWallet, automatedOrder, (Int32)minTx, (Int32)maxTx);

			automatedPurchaseWindow.Show ();
		}


		public AutomatedOrder CreateOffer ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (CreateOffer) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.AutomatedSellWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif
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
				MessageDialog.ShowMessage (off.taker_gets.currency + " getsamount is formatted incorrectly\n");
#if DEBUG
				if (DebugIhildaWallet.AutomatedSellWidget) {
					Logging.WriteLog (method_sig + "getsamount == null\n");
				}
#endif
				return null;
			}

			Decimal? payamount = RippleCurrency.ParseDecimal (maxcomboboxentry.ActiveText);
			if (payamount == null) {

				MessageDialog.ShowMessage (off.taker_pays.currency + " payamount is formatted incorrectly\n");
#if DEBUG
				if (DebugIhildaWallet.AutomatedSellWidget) {
					Logging.WriteLog (method_sig + "payamount == null\n");
				}
#endif
				return null;
			}

			off.taker_pays.amount = off.taker_pays.IsNative ? (Decimal)payamount * 1000000 : (Decimal)payamount;
			off.taker_gets.amount = off.taker_gets.IsNative ? (Decimal)getamount * 1000000 : (Decimal)getamount;

			LicenseType licenseT = Util.LicenseType.TRADING;
			if (LeIceSense.IsLicenseExempt (off.taker_gets) || LeIceSense.IsLicenseExempt (off.taker_pays)) {
				licenseT = LicenseType.NONE;
			}

		


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



			String text = this.amountcomboboxentry.ActiveText;
#if DEBUG
			if (DebugIhildaWallet.AutomatedSellWidget) {
				Logging.WriteLog (method_sig + "this.amountcomboboxentry.ActiveText = " + DebugIhildaWallet.ToAssertString (text) + "\n");
			}
#endif

			Decimal? am = RippleCurrency.ParseDecimal (text);
			if (am == null) {

				MessageDialog.ShowMessage ("Amount is formatted incorrectly");
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

					label10.Markup = "<b><u>Sell " + b + "</u></b>";
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

