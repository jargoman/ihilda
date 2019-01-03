using System;
using System.Threading.Tasks;
using Gtk;
using IhildaWallet.Util;
using RippleLibSharp.Keys;
using RippleLibSharp.Transactions;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AutomatedBuyWidget : Gtk.Bin
	{
		public AutomatedBuyWidget ()
		{
			this.Build ();

			this.amountcomboboxentry.Changed += (object sender, EventArgs e) => {
				CalculateMax ();
			};

			this.pricecomboboxentry.Changed += (object sender, EventArgs e) => {
				CalculateMax ();
			};




			buybutton.Clicked += Buybutton_Clicked;

			Label l = (Label)this.buybutton.Child;
			l.UseMarkup = true;

			this.label9.Text = "";
			this.label10.Text = "";
		}

		private void CalculateMax ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (CalculateMax) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.AutomatedBuyWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif



			String text = this.amountcomboboxentry.ActiveText;
#if DEBUG
			if (DebugIhildaWallet.AutomatedBuyWidget) {
				Logging.WriteLog (method_sig + "this.amountcomboboxentry.ActiveText = " + DebugIhildaWallet.ToAssertString (text) + "\n");
			}
#endif

			Decimal? am = RippleCurrency.ParseDecimal (text);
			if (am == null) {
				MessageDialog.ShowMessage ("Amount is formatted incorrectly \n");

#if DEBUG
				if (DebugIhildaWallet.AutomatedBuyWidget) {
					Logging.WriteLog (method_sig + "am==null, returning\n");
				}
#endif
				return;
			}

			String text2 = this.pricecomboboxentry.ActiveText;
#if DEBUG
			if (DebugIhildaWallet.AutomatedBuyWidget) {
				Logging.WriteLog (method_sig + "this.pricecomboboxentry.ActiveText = " + DebugIhildaWallet.ToAssertString (text2) + "\n");
			}
#endif
			Decimal? pr = RippleCurrency.ParseDecimal (text2);

			if (pr == null) {

				//MessageDialog.showMessage ("Price is formatted incorrectly");

#if DEBUG
				if (DebugIhildaWallet.AutomatedBuyWidget) {
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

				// todo color
#if DEBUG
				if (DebugIhildaWallet.AutomatedBuyWidget) {
					Logging.WriteLog (method_sig + DebugRippleLibSharp.exceptionMessage);
					Logging.WriteLog (ee.Message);
				}
#endif
				return;
			}

		}

		public void SetOffer (Offer off)
		{
#if DEBUG
			string method_sig = clsstr + nameof (SetOffer) + DebugRippleLibSharp.left_parentheses + nameof (Offer) + DebugRippleLibSharp.space_char + nameof (off) + DebugRippleLibSharp.equals + DebugIhildaWallet.ToAssertString (off) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.BuyWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			if (off == null) {
#if DEBUG
				if (DebugIhildaWallet.BuyWidget) {
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

			if (off.taker_pays != null) {
#if DEBUG
				if (DebugIhildaWallet.BuyWidget) {
					Logging.WriteLog (method_sig + "off.TakerGets != null\n");
				}
#endif

				Decimal amount = off.taker_pays.amount;
				if (off.taker_pays.IsNative) {
					amount /= 1000000;
				}

				string tex = amount.ToString ();

				Application.Invoke (
					delegate {
						Entry entry = this.amountcomboboxentry?.Entry;
						if (entry != null) {
							entry.Text = tex;
						}

					}
				);

			}

			if (off.taker_gets != null) {
#if DEBUG
				if (DebugIhildaWallet.BuyWidget) {
					Logging.WriteLog (method_sig + "off.TakerPays != null\n");
				}
#endif

				string tex = off.taker_pays.GetNativeAdjustedPriceAt (off.taker_gets).ToString ();

				Application.Invoke (delegate {

					this.pricecomboboxentry.Entry.Text = tex;
				});

			}


		}

		internal void SetRippleWallet (RippleWallet rw)
		{
			_rippleWallet = rw;
		}

		void Buybutton_Clicked (object sender, EventArgs e)
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
			Int32? minTx = RippleCurrency.ParseInt32 (comboboxentry1?.ActiveText);
			if (minTx == null) {
				MessageDialog.ShowMessage ("Invalid minTx\n");
				return;

			}
			Int32? maxTx = RippleCurrency.ParseInt32 (comboboxentry2?.ActiveText);
			if (maxTx == null) {
				MessageDialog.ShowMessage ("Invalid maxTx");
				return;

			}

			AutomatedOrder automatedOrder = CreateOffer ();

			//RippleOfferTransaction tx = new RippleOfferTransaction(rw.getStoredReceiveAddress(), off);

			Task.Run (
				() => { 
					LicenseType licenseT = Util.LicenseType.SEMIAUTOMATED;
					if (LeIceSense.IsLicenseExempt (automatedOrder.taker_gets) || LeIceSense.IsLicenseExempt (automatedOrder.taker_pays)) {
						licenseT = LicenseType.NONE;
					}

					bool shouldContinue = LeIceSense.LastDitchAttempt (_rippleWallet, licenseT);
					if (!shouldContinue) {
						return;
					}

					Application.Invoke (
						delegate {
						
							AutomatedPurchaseWindow automatedPurchaseWindow = new AutomatedPurchaseWindow (_rippleWallet, automatedOrder, (Int32)minTx, (Int32)maxTx);

							automatedPurchaseWindow.Show ();

						}
					);

				}
			);

		}


		public TradePair TradePairInstance {

			get {
#if DEBUG
				string method_sig = clsstr + nameof (TradePair) + " get" + DebugRippleLibSharp.colon;
				if (DebugIhildaWallet.AutomatedBuyWidget) {
					Logging.WriteLog (method_sig + DebugRippleLibSharp.returning + DebugIhildaWallet.ToAssertString (_tradePair));
				}
#endif
				return _tradePair;
			}

			set {
#if DEBUG
				string method_sig = clsstr + nameof (TradePairInstance) + DebugRippleLibSharp.space_char + "set" + DebugRippleLibSharp.colon;
				if (DebugIhildaWallet.AutomatedBuyWidget) {
					Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
				}
#endif
				_tradePair = value;

#if DEBUG
				if (DebugIhildaWallet.AutomatedBuyWidget) {
					Logging.WriteLog (method_sig + "value set to " + DebugIhildaWallet.ToAssertString (_tradePair));
				}
#endif


				bool requirements = _tradePair != null && this._tradePair.HasRequirements ();


				String a = requirements ? _tradePair.Currency_Base.currency : "";
				String b = requirements ? _tradePair.Currency_Counter.currency : "";

				Application.Invoke (delegate {
#if DEBUG
					string event_sig = method_sig + DebugIhildaWallet.gtkInvoke;
					if (DebugIhildaWallet.AutomatedBuyWidget) {
						Logging.WriteLog (event_sig + nameof (a) + DebugRippleLibSharp.equals + DebugIhildaWallet.ToAssertString (a) + DebugRippleLibSharp.comma + nameof (b) + DebugRippleLibSharp.equals + DebugIhildaWallet.ToAssertString (b));
					}
#endif
					label10.Markup = "<b><u>Buy " + a + "</u></b>";
					label12.Text = a;
					label15.Text = b;
					label16.Text = b;
				}
				);
			}
		}


		public BuyOffer CreateOffer ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (CreateOffer) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.AutomatedBuyWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			if (amountcomboboxentry == null || pricecomboboxentry == null || maxcomboboxentry == null) {
				// todo gui in state of disarray, debug
				return null;
			}

			if (_tradePair == null) {
				// todo debug. warn user?
#if DEBUG
				if (DebugIhildaWallet.BuyWidget) {
					Logging.WriteLog (method_sig + nameof (_tradePair) + " == null\n");
				}
#endif
				return null;
			}

			//RippleWallet rw = MainWindow.currentInstance.getRippleWallet();
			RippleWallet rw = _rippleWallet;
			if (rw == null) {
#if DEBUG
				if (DebugIhildaWallet.AutomatedBuyWidget) {
					Logging.WriteLog (method_sig + "w == null, returning\n");
				}
#endif

				// TODO warn user // shouldn't happen actually, make sure this is set by the time it's used
				return null;
			}

			RippleIdentifier seed = rw.GetDecryptedSeed ();

			while (seed.GetHumanReadableIdentifier() == null) {
				bool should = AreYouSure.AskQuestion (
				"Invalid password", 
				"Unable to decrypt seed. Invalid password.\nWould you like to try again?"
				);

				if (!should) {
					return null;
				}

				seed = rw.GetDecryptedSeed ();
			}

#if DEBUG
			if (DebugIhildaWallet.AutomatedBuyWidget) {
				Logging.WriteLog (method_sig + nameof (rw) + DebugRippleLibSharp.equals + DebugIhildaWallet.ToAssertString (rw));

				// The log might be aware not to print the secret
				Logging.WriteLog (method_sig + nameof (seed) + DebugRippleLibSharp.equals + DebugIhildaWallet.ToAssertString (seed));

			}
#endif


			TradePair tp = this._tradePair.DeepCopy ();
			if (tp == null) {
#if DEBUG
				if (DebugIhildaWallet.AutomatedBuyWidget) {
					Logging.WriteLog ("tp == null, returning\n");
				}
#endif
				return null;
			}

#if DEBUG
			if (DebugIhildaWallet.AutomatedBuyWidget) {
				Logging.WriteLog ("tp.currency_base =" + tp.Currency_Base.ToString ());
				Logging.WriteLog ("tp.currency_counter =" + tp.Currency_Counter.ToString ());
			}
#endif



			BuyOffer off = new BuyOffer {
				Account = rw.Account
			};

			//off.taker_gets = tp.currency_counter;
			//off.taker_pays = tp.currency_base;

			off.SetFromTradePair (tp);

			Decimal? payamount = RippleCurrency.ParseDecimal (amountcomboboxentry.ActiveText);
			if (payamount == null) {
				MessageDialog.ShowMessage (off.taker_pays.currency + " payamount is formatted incorrectly \n");
				return null;
			}

			Decimal? getamount = RippleCurrency.ParseDecimal (maxcomboboxentry.ActiveText);
			if (getamount == null) {
				MessageDialog.ShowMessage (off.taker_gets.currency + " getamount is formatted incorrectly \n");
				return null;
			}


			off.taker_pays.amount = off.taker_pays.IsNative ? (Decimal)payamount * 1000000 : (Decimal)payamount;
			off.taker_gets.amount = off.taker_gets.IsNative ? (Decimal)getamount * 1000000 : (Decimal)getamount;


			off.Account = _rippleWallet.GetStoredReceiveAddress ();

			return off;


		}

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private TradePair _tradePair = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant
		private RippleWallet _rippleWallet = null;


#if DEBUG
		private const string clsstr = nameof (AutomatedBuyWidget) + DebugRippleLibSharp.colon;


#endif
	}
}

