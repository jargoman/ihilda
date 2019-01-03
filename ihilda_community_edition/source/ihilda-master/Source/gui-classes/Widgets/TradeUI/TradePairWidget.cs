using System;
using System.Collections.Generic;
using System.Threading;
using Gtk;

using RippleLibSharp.Transactions;
using RippleLibSharp.Keys;

using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Network;
using IhildaWallet.Networking;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class TradePairWidget : Gtk.Bin
	{
		public TradePairWidget ()
		{
#if DEBUG
			string method_sig = clsstr + nameof(TradePairWidget) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.TradePairWidget) {
				Logging.WriteLog (method_sig + "new begin");
			}
#endif
			this.Build ();
			//while(Gtk.Application.EventsPending())
			//	Gtk.Application.RunIteration();

			this._SyncButton = this.syncButtongui;

			this.flipbutton.Clicked += (object sender, EventArgs e) => {
#if DEBUG
				if (DebugIhildaWallet.TradePairWidget) {
					Logging.WriteLog (method_sig + "flipbutton clicked");
				}
#endif

				String ba = basecurrencycombobox.ActiveText;
				String cow = countercurrencycombobox.ActiveText;

				countercurrencycombobox.Entry.Text = ba;
				basecurrencycombobox.Entry.Text = cow;

				ba = baseissuercombobox.ActiveText;
				cow = countercurrencycombobox1.ActiveText;

				baseissuercombobox.Entry.Text = cow;
				countercurrencycombobox1.Entry.Text = ba;

			};

			this.pairsSelectButton.Clicked += (object sender, EventArgs e) => {
				TradePair tp = PairPopup.DoPopup ();

				if (tp == null) {
					return;
				}

				//if (tp.currency_base ) {

				//}

				basecurrencycombobox.Entry.Text = tp.Currency_Base.currency;
				countercurrencycombobox.Entry.Text = tp.Currency_Counter.currency;

				this.UpdateCurrencyIssuers ();
			};


			this.basebutton.Clicked += (object sender, EventArgs e) => {
				RippleCurrency dic = DenominatedIssuedPopup.DoPopup ("Base Issuer", "Enter the issuer code in the form XXX/rXXXXXXXXXXXXXXXX");

				SetIssuedCurrency (this.basecurrencycombobox, this.baseissuercombobox, dic);
			};

			this.counterbutton.Clicked += (object sender, EventArgs e) => {
				RippleCurrency dic = DenominatedIssuedPopup.DoPopup ("Counter Issuer", "Enter the issuer code in the form XXX/rXXXXXXXXXXXXXXXX");

				SetIssuedCurrency (this.countercurrencycombobox, this.countercurrencycombobox1, dic);
			};
		}


		public override string ToString ()
		{
			return string.Format ("[TradePairWidget]");
		}


		public TradePair GetTradePair ()
		{
#if DEBUG
			String method_sig = clsstr + nameof (GetTradePair) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.TradePairWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			TradePair tp = new TradePair {
				Currency_Base = GetBaseCurrency (),
				Currency_Counter = GetCounterCurrency ()
			};

			return tp;
		}

		public void SetIssuedCurrency (ComboBoxEntry currencyBox, ComboBoxEntry issuerBox, RippleCurrency dic)
		{
			if (currencyBox == null || issuerBox == null || dic == null) {
				return;
			}


			if (!string.IsNullOrEmpty (dic.currency)) {
				currencyBox.Entry.Text = dic.currency;
			}


			if (dic.issuer != null) {
				issuerBox.Entry.Text = dic.issuer;
			}

		}

		public RippleCurrency GetBaseCurrency ()
		{

#if DEBUG
			String method_sig = clsstr + nameof (GetBaseCurrency) + DebugRippleLibSharp.both_parentheses;
#endif
			//ManualResetEvent ev = new ManualResetEvent(false);
			String currency = null;
			String issuer = null;

			//Application.Invoke (
			//	delegate {
			currency = this.basecurrencycombobox.ActiveText;
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
				MessageDialog.ShowMessage ("You must specify a base currency");
				return null;
			}

			if (currency.Equals (RippleCurrency.NativeCurrency)) {

				if (issuer != null && !issuer.Equals ("")) {
					// alert user about specifying a native issuer
					MessageDialog.ShowMessage ("You cannot specify an issuer for native currency " + RippleCurrency.NativeCurrency);
					return null;
				}

				RippleCurrency bob = new RippleCurrency (0.0m);
				return bob;
			}
			if (issuer == null || issuer.Equals ("")) {
				MessageDialog.ShowMessage ("You must specify an issuer for non native base currency ");
				return null;
			}

			try {
				RippleAddress raish = new RippleAddress (issuer);

				RippleCurrency bas = new RippleCurrency (0.0m, raish, currency);
				return bas;
			} catch (Exception e) {

#if DEBUG
				// todo debug // look into class RippleAddress to see what exeption to catch
				Logging.WriteLog (method_sig + "exception thrown : " + e.Message);

#endif
				return null;
			}

		}

		public RippleCurrency GetCounterCurrency ()
		{
#if DEBUG
			String method_sig = clsstr + "getCounterCurrency () : ";
#endif
			//ManualResetEvent ev = new ManualResetEvent(false);
			String currency = null;
			String issuer = null;

			//Application.Invoke (
			//	delegate {
			currency = this.countercurrencycombobox.ActiveText;
			issuer = this.countercurrencycombobox1.ActiveText;
			if (currency == null || currency.Equals ("")) {
				// todo currency required
				MessageDialog.ShowMessage ("You must specify a counter currency");
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
				}

				RippleCurrency bob = new RippleCurrency (0.0m);
				return bob;
			}
			if (issuer == null || issuer.Equals ("")) {
				MessageDialog.ShowMessage ("You must specify an issuer for non native counter currency ");
				return null;
			}

			try {
				RippleAddress raish = new RippleAddress (issuer);

				RippleCurrency bas = new RippleCurrency (0.0m, raish, currency);
				return bas;
			} catch (Exception e) {
				// todo debug // look into class RippleAddress to see what exeption to catch

#if DEBUG
				Logging.WriteLog (method_sig + "Exception thrown : " + e.Message);
#endif
				return null;
			}

		}


		private Button _SyncButton {
			get;
			set;
		}


		private void UpdateCurrencyIssuers ()
		{
#if DEBUG
			String method_sig = clsstr + nameof (UpdateCurrencyIssuers) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.TradePairWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			Gtk.Application.Invoke (delegate {
#if DEBUG
				if (DebugIhildaWallet.TradePairWidget) {
					Logging.WriteLog (method_sig + DebugIhildaWallet.gtkInvoke);
				}
#endif




				UpdateCurrencyHelper (this.countercurrencycombobox, this.countercurrencycombobox1);
				UpdateCurrencyHelper (this.basecurrencycombobox, this.baseissuercombobox);
			});

		}

		private static void UpdateCurrencyHelper (ComboBoxEntry currencycombobox, ComboBoxEntry issuercombobox)
		{


#if DEBUG
			String method_sig = clsstr +
				nameof (UpdateCurrencyHelper)	+
			" ( currencycombobox = " +
			currencycombobox?.ActiveText ?? "null" +
			", issuercombobox = " +  // why do I do this to myself lol, I might have to debug the debug code
			issuercombobox?.ActiveText ?? "null" +
			" ) : ";

			if (DebugIhildaWallet.TradePairWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			String cur = currencycombobox.ActiveText;

			// TODO switch to relevent address. User may have selected a new wallet. This gets confusing either way. Multi wallet apps are scary
			RippleWallet rw = WalletManager.GetRippleWallet();
			string address = rw.GetStoredReceiveAddress ();

			NetworkInterface ni = NetworkController.CurrentInterface;

			IEnumerable<String> lis = AccountLines.GetIssuersForCurrency (cur, address, ni, new CancellationToken());


			if (lis == null) {
#if DEBUG
				if (DebugIhildaWallet.TradePairWidget) {
					Logging.WriteLog (method_sig + "lis == null, returning\n");
				}
#endif
				return;
			}

#if DEBUG
			if (DebugIhildaWallet.TradePairWidget) {
				Logging.WriteLog (method_sig + "number of issuers = ", lis);
			}
#endif

			ListStore store = new ListStore (typeof (string));

			foreach (String s in lis) {
#if DEBUG
				int debug_cnt = 0;
				if (DebugIhildaWallet.TradePairWidget) {
					Logging.WriteLog (method_sig + "count " + (debug_cnt++).ToString () + " of String s = " + DebugIhildaWallet.ToAssertString (s));
				}
#endif
				store.AppendValues (s);
			}

			issuercombobox.Model = store; //issuerentry.Model = store;

		}



#if DEBUG
		private const string clsstr = nameof (TradePairWidget) + DebugRippleLibSharp.colon;

		public Button SyncButton { get => _SyncButton; set => _SyncButton = value; }
#endif
	}
}

