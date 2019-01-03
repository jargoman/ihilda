using System;
using RippleLibSharp.Transactions;
using RippleLibSharp.Keys;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class TradePairEntryWidget : Gtk.Bin
	{
		public TradePairEntryWidget ()
		{
			this.Build ();




			this.basecurrencycombobox.Changed += (object sender, EventArgs e) => {
				basecurrencycombobox.ModifyBase (Gtk.StateType.Normal);

				string cur = basecurrencycombobox.ActiveText;
				if (RippleCurrency.NativeCurrency == cur) {
					baseissuercombobox.Visible = false;

				} else {
					baseissuercombobox.Visible = true;
				}

				OnWidgetChanged (e);


			};

			this.countercurrencycombobox.Changed += (object sender, EventArgs e) => {
				countercurrencycombobox.ModifyBase (Gtk.StateType.Normal);
				string cur = countercurrencycombobox.ActiveText;
				if (RippleCurrency.NativeCurrency == cur) {
					countercurrencycombobox1.Visible = false;

				} else {
					countercurrencycombobox1.Visible = true;
				}

				OnWidgetChanged (e);

			};

			this.baseissuercombobox.Changed += (object sender, EventArgs e) => {
				countercurrencycombobox.ModifyBase (Gtk.StateType.Normal);

				OnWidgetChanged (e);
			};

			this.countercurrencycombobox1.Changed += (object sender, EventArgs e) => {
				countercurrencycombobox1.ModifyBase (Gtk.StateType.Normal);

				OnWidgetChanged (e);
			};




		}


		public event EventHandler WidgetChanged;

		public virtual void OnWidgetChanged (EventArgs eventArgs)
		{
			WidgetChanged?.Invoke (this, eventArgs);
		}



		public TradePair GetTradePair (bool alertUser)
		{
#if DEBUG
			String method_sig = clsstr + nameof(GetTradePair) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.TradePairWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif
			TradePair tp = new TradePair {
				Currency_Base = GetRippleCurrencyBase (alertUser),
				Currency_Counter = GetRippleCurrencyCounter (alertUser)
			};

			return tp;
		}

		public void SetTradePair (TradePair tp)
		{

			if (tp == null) {
				basecurrencycombobox.Entry.Text = "";
				baseissuercombobox.Entry.Text = "";

				countercurrencycombobox.Entry.Text = "";
				countercurrencycombobox1.Entry.Text = "";
			} else {
				basecurrencycombobox.Entry.Text = tp.Currency_Base.currency;
				baseissuercombobox.Entry.Text = tp.Currency_Base.issuer;

				countercurrencycombobox.Entry.Text = tp.Currency_Counter.currency;
				countercurrencycombobox1.Entry.Text = tp.Currency_Counter.issuer;
			}


		}

		public OrderFilledRule GetFillRule ()
		{
			string baseCurrency = this.basecurrencycombobox?.Entry?.Text;
			string baseIssuer = this.baseissuercombobox?.Entry?.Text;

			string counterCurrency = this.countercurrencycombobox?.Entry?.Text;
			string counterIssuer = this.countercurrencycombobox1?.Entry?.Text;



			if (baseCurrency == null) {
				// TODO highlight comboxox entry for base and counter
				MessageDialog.ShowMessage ("Base Currency required");
				return null;
			}

			if (counterCurrency == null) {
				MessageDialog.ShowMessage ("Counter Currency required");
				return null;
			}

			OrderFilledRule orderFilledRule = new OrderFilledRule ();

			if (baseCurrency.Equals (RippleCurrency.NativeCurrency)) {
				if (counterCurrency.Equals (RippleCurrency.NativeCurrency)) {
					MessageDialog.ShowMessage (RippleCurrency.NativeCurrency + " can not be used to buy itself");
					return null;
				}


				orderFilledRule.BoughtCurrency = new RippleCurrency (0);
			} else {
				/*
				if ( baseIssuer == null || baseIssuer.Trim().Equals("") ) {

					

				}
				*/




				orderFilledRule.BoughtCurrency = new RippleCurrency (0, baseIssuer, baseCurrency);

			}


			if (counterCurrency.Equals (RippleCurrency.NativeCurrency)) {
				orderFilledRule.SoldCurrency = new RippleCurrency (0);
			} else {
				orderFilledRule.SoldCurrency = new RippleCurrency (0, counterIssuer, counterCurrency);
			}






			return orderFilledRule;

		}


		public string GetBaseCurrency ()
		{
			return this.basecurrencycombobox.ActiveText;
		}

		public RippleCurrency GetRippleCurrencyBase (bool alertUser)
		{

#if DEBUG
			String method_sig = clsstr + nameof (GetRippleCurrencyBase) + " () : ";

#endif

			//ManualResetEvent ev = new ManualResetEvent(false);
			String currency = GetBaseCurrency();
			String issuer = null;

			//Application.Invoke (
			//	delegate {

			issuer = this.baseissuercombobox.ActiveText;

			//	ev.Set();
			//	}

			//);
			//ev.WaitOne();

			// todo verify issuer ect

			currency = currency.Trim ();
			issuer = issuer.Trim ();

			if (currency == null || currency.Equals ("")) {
				// todo currency required

				if (alertUser) {
					this.basecurrencycombobox.ModifyBase (Gtk.StateType.Normal, new Gdk.Color (255, 55, 55));
					MessageDialog.ShowMessage ("You must specify a base currency");
				}
				return null;
			}

			if (currency.Equals (RippleCurrency.NativeCurrency)) {

				if (issuer != null && !issuer.Equals ("")) {
					// alert user about specifying a native issuer
					if (alertUser) {
						this.baseissuercombobox.ModifyBase (Gtk.StateType.Normal, new Gdk.Color (255, 55, 55));
						MessageDialog.ShowMessage ("You cannot specify an issuer for native base currency " + RippleCurrency.NativeCurrency);
					}
					return null;
				}

				RippleCurrency bob = new RippleCurrency (0.0m);
				return bob;
			}
			if (issuer == null || issuer.Equals ("")) {

				if (alertUser) {
					this.baseissuercombobox.ModifyBase (Gtk.StateType.Normal, new Gdk.Color (255, 55, 55));
					MessageDialog.ShowMessage ("You must specify an issuer for non native base currency ");
				}
				return null;
			}


			RippleAddress raish = null;
			try {
				raish = new RippleAddress (issuer);
			} catch (Exception e) {
				#if DEBUG
				Logging.WriteLog (method_sig +
						  DebugRippleLibSharp.exceptionMessage +
				e.Message);

#endif

				if (alertUser) {
					this.baseissuercombobox.ModifyBase (Gtk.StateType.Normal, new Gdk.Color (255, 55, 55));
					MessageDialog.ShowMessage ("Invalid base issuer address. \n");
				}
				return null;
			}

			if (raish == null) {
				return null;
			}


			try {
				

				RippleCurrency bas = new RippleCurrency (0.0m, raish, currency);
				return bas;
			} catch (Exception e) {
				// todo debug // look into class RippleAddress to see what exeption to catch

#if DEBUG
				Logging.WriteLog (method_sig +
				                  DebugRippleLibSharp.exceptionMessage +
				e.Message);

#endif
				if (alertUser) {
					this.basecurrencycombobox.ModifyBase (Gtk.StateType.Normal, new Gdk.Color (255, 55, 55));
					this.baseissuercombobox.ModifyBase (Gtk.StateType.Normal, new Gdk.Color (255, 55, 55));
					MessageDialog.ShowMessage ("Exception creating base currency address \n");
				}
				return null;
			}

		}

		public string GetCounterCurrency ()
		{
			return this.countercurrencycombobox.ActiveText;
		}

		public RippleCurrency GetRippleCurrencyCounter (bool alertUser)
		{

#if DEBUG

			String method_sig = clsstr + nameof (GetRippleCurrencyCounter) + " () : ";

#endif

			//ManualResetEvent ev = new ManualResetEvent(false);
			String currency = null;
			String issuer = null;

			//Application.Invoke (
			//	delegate {
			currency = GetCounterCurrency ();
			issuer = this.countercurrencycombobox1.ActiveText;
			if (currency == null || currency.Equals ("")) {
				// todo currency required
				if (alertUser) {
					this.countercurrencycombobox.ModifyBase (Gtk.StateType.Normal, new Gdk.Color (255, 55, 55));
					MessageDialog.ShowMessage ("You must specify a counter currency");
				}
				return null;
			}

			//	ev.Set();
			//	}

			//);
			//ev.WaitOne();

			// 

			if (currency.Equals (RippleCurrency.NativeCurrency)) {

				if (issuer != null && !issuer.Equals ("")) {
					// alert user about specifying a native issuer

					if (alertUser) {
						this.countercurrencycombobox1.ModifyBase (Gtk.StateType.Normal, new Gdk.Color (255, 55, 55));
						MessageDialog.ShowMessage ("You cannot specify an issuer for native counter currency " + RippleCurrency.NativeCurrency);
					}
					return null;
				}

				RippleCurrency bob = new RippleCurrency (0.0m);
				return bob;
			}
			if (issuer == null || issuer.Equals ("")) {

				if (alertUser) {
					this.countercurrencycombobox1.ModifyBase (Gtk.StateType.Normal, new Gdk.Color (255, 55, 55));
					MessageDialog.ShowMessage ("You must specify an issuer for non native counter currency ");
				}
				return null;
			}

			RippleAddress raish = null;
			try {
				raish = new RippleAddress (issuer);

			} catch (Exception e) {

				if (alertUser) {
#if DEBUG
					// todo debug // look into class RippleAddress to see what exeption to catch

					this.countercurrencycombobox.ModifyBase (Gtk.StateType.Normal, new Gdk.Color (255, 55, 55));
					Logging.WriteLog (method_sig + "Exception thrown : " + e.Message);

#endif

					MessageDialog.ShowMessage ("Invalid counter issuer address. \n");
				}
				return null;
			}

			if (raish == null) {
				return null;
			}

			try {
				

				RippleCurrency cou = new RippleCurrency (0.0m, raish, currency);
				return cou;
			} catch (Exception e) {


#if DEBUG
				// todo debug // look into class RippleAddress to see what exeption to catch
				Logging.WriteLog (method_sig + "Exception thrown : " + e.Message);

#endif
				if (alertUser) {
					this.countercurrencycombobox.ModifyBase (Gtk.StateType.Normal, new Gdk.Color (255, 55, 55));
					this.countercurrencycombobox1.ModifyBase (Gtk.StateType.Normal, new Gdk.Color (255, 55, 55));
					MessageDialog.ShowMessage ("Exception creating counter currency address \n");
				}
				return null;
			}

		}


#if DEBUG
		private const string clsstr = nameof (TradePairEntryWidget) + DebugRippleLibSharp.colon;
#endif
	}
}

