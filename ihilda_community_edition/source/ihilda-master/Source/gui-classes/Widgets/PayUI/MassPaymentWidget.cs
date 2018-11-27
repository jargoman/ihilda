using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using IhildaWallet.Networking;
using IhildaWallet.Util;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Keys;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;
using RippleLibSharp.Transactions.TxTypes;
using RippleLibSharp.Trust;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class MassPaymentWidget : Gtk.Bin
	{
		public MassPaymentWidget ()
		{
			this.Build ();

			bool sen = false;

			mincheckbox.Sensitive = sen;
			maxcheckbox.Sensitive = sen;
			mincomboboxentry.Sensitive = sen;
			maxcomboboxentry.Sensitive = sen;
			sharetickercomboboxentry.Sensitive = sen;
			shareIssuerComboboxentry.Sensitive = sen;
			label4.Sensitive = sen;
			label3.Sensitive = sen;

			requireTokenCheckbox.Clicked += (object sender, EventArgs e) => {

				bool sensitive = requireTokenCheckbox.Active;

				mincheckbox.Sensitive = sensitive;
				maxcheckbox.Sensitive = sensitive;


				sharetickercomboboxentry.Sensitive = sensitive;
				shareIssuerComboboxentry.Sensitive = sensitive;
				label4.Sensitive = sensitive;
				label3.Sensitive = sensitive;

				DoAndOrBoxGui ();
			};

			mincheckbox.Clicked += (object sender, EventArgs e) => { 
				bool sensitive = mincheckbox.Active;
				mincomboboxentry.Sensitive = sensitive;
			};

			maxcheckbox.Clicked += (object sender, EventArgs e) => {
				bool sensitive = maxcheckbox.Active;
				maxcomboboxentry.Sensitive = sensitive;
			};

			includecheckbox.Clicked += (object sender, EventArgs e) => {
				bool sensitive = includecheckbox.Active;
				filechooserbutton2.Sensitive = sensitive;

				DoAndOrBoxGui ();
			};

			excludecheckbox.Clicked += (object sender, EventArgs e) => {
				bool sensitive = excludecheckbox.Active;
				filechooserbutton3.Sensitive = sensitive;
			};

			button217.Clicked += Button217_Clicked;

			sharetickercomboboxentry.Changed += (object sender, EventArgs e) => {

				shareTickTokenSource?.Cancel ();
				shareTickTokenSource = new CancellationTokenSource ();
				CancellationToken token = shareTickTokenSource.Token;

				string str = sharetickercomboboxentry.ActiveText;
				if (RippleCurrency.NativeCurrency == str) {
					this.shareIssuerComboboxentry.Entry.Text = "";
					this.shareIssuerComboboxentry.Visible = false;
					this.label4.Visible = false;
					return;
				}

				this.shareIssuerComboboxentry.Visible = true;
				this.label4.Visible = true;

				Task.Run (delegate {


					NetworkInterface networkInterface = NetworkController.GetNetworkInterfaceNonGUIThread ();
					if (networkInterface == null) {
						return;
					}
					string account = _rippleWallet?.GetStoredReceiveAddress ();
					if (account == null) {
						return;
					}
					List<string> issuers = AccountLines.GetIssuersForCurrency (str, account, networkInterface, token);



					SetShareComboIssuers (issuers);


				}, token);
			};

			divCurcomboboxentry.Changed += (object sender, EventArgs e) => {

				dicCurTokenSource?.Cancel ();
				dicCurTokenSource = new CancellationTokenSource ();
				var token = dicCurTokenSource.Token;

				string str = divCurcomboboxentry.ActiveText;
				if (RippleCurrency.NativeCurrency == str) {
					this.divIssuercomboboxentry.Entry.Text = "";
					this.divIssuercomboboxentry.Visible = false;
					this.label8.Visible = false;
					return;
				}

				this.divIssuercomboboxentry.Visible = true;
				this.label8.Visible = true;

				Task.Run ( delegate {

					NetworkInterface networkInterface = NetworkController.GetNetworkInterfaceNonGUIThread ();
					if (networkInterface == null) {
						return;
					}
					string account = _rippleWallet?.GetStoredReceiveAddress ();
					if (account == null) {
						return;
					}
					List<string> issuers = AccountLines.GetIssuersForCurrency ( str, account, networkInterface, token );



					SetDivComboIssuers (issuers);


				}, token );

			};

			divIssuercomboboxentry.Changed += DivIssuercomboboxentry_Changed;

			shareIssuerComboboxentry.Changed += ShareIssuerComboboxentry_Changed;

		}

		CancellationTokenSource shareTickTokenSource = null;
		CancellationTokenSource dicCurTokenSource = null;
		CancellationTokenSource shareIssTokenSource = null;
		CancellationTokenSource divIssTokenSource = null;

		void ShareIssuerComboboxentry_Changed (object sender, EventArgs e)
		{

			shareIssTokenSource?.Cancel ();
			shareIssTokenSource = new CancellationTokenSource ();
			var token = shareIssTokenSource.Token;

			string str = shareIssuerComboboxentry.ActiveText;

			string account = null;

			try {
				account = new RippleAddress (str).ToString ();

			} catch (Exception ex) {
				// TODO

				return;
			}

			if (account == null) {
				return;
			}

			Task.Run (delegate {
				NetworkInterface networkInterface = NetworkController.GetNetworkInterfaceNonGUIThread ();
				if (networkInterface == null) {
					return;
				}

				Task<Response<AccountCurrenciesResult>> task = AccountCurrencies.GetResult (account, networkInterface, token);
				if (task == null) {
					return;
				}
				task.Wait (token);

				Response<AccountCurrenciesResult> response = task.Result;
				if (response == null) {
					// TODO
					return;
				}

				AccountCurrenciesResult accountCurrenciesResult = response.result;

				if (accountCurrenciesResult == null) {
					// TODO
					return;
				}

				string [] currencies = accountCurrenciesResult?.send_currencies;
				if (currencies == null) {
					return;
				}

				if (!currencies.Any ()) {
					return;
				}

				SetTickerCurrencies (currencies);

			}, token);
		}


		void DivIssuercomboboxentry_Changed (object sender, EventArgs e)
		{
			divIssTokenSource?.Cancel ();
			divIssTokenSource = new CancellationTokenSource ();
			CancellationToken token = divIssTokenSource.Token;

			string str = divIssuercomboboxentry.ActiveText;
			string account = null;

			try {
				account = new RippleAddress (str).ToString ();

			} catch (Exception ex) {
				// TODO

				return;
			}

			if (account == null) {
				return;
			}

			Task.Run (delegate {
				NetworkInterface networkInterface = NetworkController.GetNetworkInterfaceNonGUIThread ();
				if (networkInterface == null) {
					return;
				}

				Task <Response <AccountCurrenciesResult>> task = AccountCurrencies.GetResult (account, networkInterface, token);
				if (task == null) {
					return;
				}
				task.Wait (token);

				Response<AccountCurrenciesResult> response = task.Result;
				if (response == null) {
					// TODO
					return;
				}

				AccountCurrenciesResult accountCurrenciesResult = response.result;

				if (accountCurrenciesResult == null) {
					// TODO
					return;
				}

				string[] currencies = accountCurrenciesResult?.send_currencies;
				if (currencies == null) {
					return;
				}

				if (!currencies.Any()) {
					return;
				}

				SetDivCurrencies (currencies);

			});
		}

		public void SetDivCurrencies (IEnumerable <String> currencies)
		{
			if (currencies == null) {
				return;
			}

			if ( !currencies.Any()) {
				return;
			}

			Application.Invoke (delegate {
				ListStore store = new ListStore (typeof (string));

				foreach (String s in currencies) {
					store.AppendValues (s);
				}

				this.divCurcomboboxentry.Model = store;


			});
		}


		/*void ShareIssuercomboboxentry_Changed (object sender, EventArgs e)
		{
			string str = shareIssuerComboboxentry.ActiveText;
			string account = null;

			try {
				account = new RippleAddress (str).ToString ();

			} catch (Exception ex) {
				// TODO

				return;
			}

			if (account == null) {
				return;
			}

			Task.Run (delegate {
				NetworkInterface networkInterface = NetworkController.GetNetworkInterfaceNonGUIThread ();
				if (networkInterface == null) {
					return;
				}

				Task<Response<AccountCurrenciesResult>> task = AccountCurrencies.GetResult (account, networkInterface);
				if (task == null) {
					return;
				}
				task.Wait ();

				Response<AccountCurrenciesResult> response = task.Result;
				if (response == null) {
					// TODO
					return;
				}

				AccountCurrenciesResult accountCurrenciesResult = response.result;

				if (accountCurrenciesResult == null) {
					// TODO
					return;
				}

				string [] currencies = accountCurrenciesResult?.send_currencies;
				if (currencies == null) {
					return;
				}

				if (!currencies.Any ()) {
					return;
				}

				SetTickerCurrencies (currencies);
			});
		} */

		public void SetTickerCurrencies (IEnumerable<String> currencies)
		{
			if (currencies == null) {
				return;
			}

			if (!currencies.Any ()) {
				return;
			}

			Application.Invoke (delegate {
				ListStore store = new ListStore (typeof (string));

				foreach (String s in currencies) {
					store.AppendValues (s);
				}

				this.sharetickercomboboxentry.Model = store;


			});
		}


		public void SetShareComboIssuers (List<string> issuers)
		{

			if (issuers == null) {
				return;
			}

			if (issuers.Count < 1) {
				// TODO infobar
				return;
			}


			Application.Invoke (delegate {
				ListStore store = new ListStore (typeof (string));

				foreach (String s in issuers) {
					store.AppendValues (s);
				}

				this.shareIssuerComboboxentry.Model = store;


			});
		}



		public void SetDivComboIssuers (List<string> issuers)
		{

			if (issuers == null) {
				return;
			}

			if (issuers.Count < 1) {
				// TODO infobar
				return;
			}


			Application.Invoke (delegate {
				ListStore store = new ListStore (typeof (string));

				foreach (String s in issuers) {
					store.AppendValues (s);
				}

				this.divIssuercomboboxentry.Model = store;


			});
		}

		void Button217_Clicked (object sender, EventArgs e)
		{

			bool reqTokes = requireTokenCheckbox.Active;
			bool reqInc = includecheckbox.Active;
			bool reqEx = excludecheckbox.Active;
			string file1 = null;
			string file2 = null;

			string andOr = null;

			bool isMin = mincheckbox.Active;
			bool isMax = maxcheckbox.Active;

			Decimal? min = null;
			Decimal? max = null;

			TokenRequires tokenRequires = null;

			if (reqTokes) {
				tokenRequires = GetTokenRequires ();
				if (isMin) {
					string minstr = mincomboboxentry.ActiveText;
					min = RippleCurrency.ParseDecimal (minstr);
				}

				if (isMax) {
					string maxstr = maxcomboboxentry.ActiveText;
					max = RippleCurrency.ParseDecimal (maxstr);
				}
			}

			if (reqInc) {
				file1 = filechooserbutton2.Filename;
			}

			if (reqEx) {
				file2 = filechooserbutton3.Filename;
			}

			if (reqTokes && reqInc) {
				andOr = andOrBox.ActiveText;
			}


			string amountStr = divAmcomboboxentry.ActiveText;
			if (string.IsNullOrWhiteSpace (amountStr)) {
				// TODO
				return;
			}

			Decimal? amount = RippleCurrency.ParseDecimal (amountStr);
			if (amount == null) {
				return;
			}

			string divIssuer = divIssuercomboboxentry.ActiveText;
			if (string.IsNullOrWhiteSpace (divIssuer)) {
				// TODO 
				return;
			}

			RippleAddress issuerAddress = null;
			try {
				issuerAddress = new RippleAddress (divIssuer);
			} catch (Exception exception) {
				return;
			}

			if (issuerAddress == null) {
				// TODO. Will unrechable cose anyway
				return;
			}

			string divcur = divCurcomboboxentry.ActiveText;
			if (string.IsNullOrWhiteSpace (divcur)) {
				return;
			}



			RippleCurrency rippleCurrency = new RippleCurrency ((Decimal)amount, issuerAddress, divcur);

			Task.Run (
				delegate {
					IEnumerable<string> holders = null;
					IEnumerable<string> includes = null;
					IEnumerable<string> excludes = null;
					if (reqEx) {
						//excludes = File.ReadLines (file2);
						excludes = File.ReadAllLines (file2);
						StringBuilder stringBuild = new StringBuilder ();
						foreach (string s in excludes) {
							stringBuild.AppendLine (s);
						}

						MessageDialog.ShowMessage ("Excludes", stringBuild.ToString ());


					}

					if (reqTokes) {
						holders = GetTokenRequiresAccounts (tokenRequires, min, max);
						StringBuilder stringBuilder = new StringBuilder ();
						foreach (string s in holders) {
							

							stringBuilder.AppendLine (s);
						}

						MessageDialog.ShowMessage ("Addresses", stringBuilder.ToString ());

						if (andOr == null) {
							IEnumerable <string> accounts = DoSingleLogoc (holders, excludes);
							DoPayment (accounts, rippleCurrency);
							return;
						}
					}
					
					if (reqInc) {
						

						//includes = File.ReadLines (file1);
						includes = File.ReadAllLines (file1);
						StringBuilder stringB = new StringBuilder ();
						foreach (string s in includes) {
							stringB.AppendLine (s);
						}

						MessageDialog.ShowMessage ("Includes", stringB.ToString ());

						if (andOr == null) {
							IEnumerable <string> accounts = DoSingleLogoc (includes, excludes);
							DoPayment (accounts, rippleCurrency);
							return;
						}

					}



					if (andOr == "AND") {
						IEnumerable<string> accounts = DoAndLogic (holders,includes,excludes);
						DoPayment (accounts, rippleCurrency);
						return;
					}

					if (andOr == "OR") {
						IEnumerable<string> accounts = DoOrLogic (holders, includes, excludes);
						DoPayment (accounts, rippleCurrency);
						return;
					}

				}
			);
		}

		public IEnumerable<string> DoAndLogic (IEnumerable <string> tokenShare, IEnumerable<string> includes, IEnumerable<string> excludes)
		{
			IEnumerable<string> vs = 
				from string s in tokenShare 
					where includes.Contains (s) 
				                  && !(excludes?.Contains (s) ?? false) 
				                  select s;

			return vs;
		}

		public IEnumerable<string> DoOrLogic (IEnumerable <string> tokenShare, IEnumerable<string> includes, IEnumerable<string> excludes)
		{
			
			IEnumerable<string> vs = tokenShare.Concat (includes).Distinct ();

			vs = from string s in vs where !(excludes?.Contains (s) ?? false) select s;

			return vs;
		}

		public IEnumerable <string> DoSingleLogoc (IEnumerable <string> includes, IEnumerable <string> excludes )
		{
			IEnumerable<string> v = from string s in includes where !(excludes?.Contains (s) ?? false) select s;
			return v;
		}

		private void DoPayment (IEnumerable <string> addresses, RippleCurrency currencyAmount)
		{

			LicenseType licenseT = Util.LicenseType.MASSPAYMENT;
			if (LeIceSense.IsLicenseExempt (currencyAmount)) {
				licenseT = LicenseType.NONE;
			}

			var v = from string address in addresses select
				new RipplePaymentTransaction () {
					Account = _rippleWallet.GetStoredReceiveAddress (),
					Destination = address,
					Amount = currencyAmount
				};



			Application.Invoke ( delegate {
				PaymentSubmitWindow paymentSubmitWindow = new PaymentSubmitWindow (_rippleWallet, licenseT);
				paymentSubmitWindow.SetPayments (v);

			});
		}

		public IEnumerable <string> GetTokenRequiresAccounts ( TokenRequires tokenRequires, Decimal? min, Decimal? max )
		{
			
			Decimal minimum = min ?? 0;
			Decimal maximum = max ?? 0;
			NetworkInterface networkInterface = NetworkController.GetNetworkInterfaceNonGUIThread ();

			// TODO possibly use a real token
			IEnumerable <Response <AccountLinesResult>> results = AccountLines.GetResultFull (tokenRequires.ShareIssuer, networkInterface, new CancellationToken());

			List<string> assetHolders = new List<string> ();

			foreach ( Response <AccountLinesResult> response in results) {
				if (response == null) {
					throw new NotImplementedException ();
				}
				if (response.HasError ()) {
					// TODO
					throw new NotImplementedException ();
				}

				AccountLinesResult accountLinesResult = response.result;


				if (accountLinesResult == null) {
					return null;
				}

				TrustLine[] lines = accountLinesResult.lines;
				if (lines == null) {
					// TODO implement all these error cases.
					return null;
				}
				foreach ( TrustLine v in lines) {
					if (v == null) {
						throw new NotImplementedException ();
					}
					Decimal balance = v.GetBalanceAsDecimal ();
					if (balance < 0) {

						balance = Math.Abs (balance);

						if (balance < min && balance > max ) {
							string acc = v.account;
							if (acc == null) {

							}
							assetHolders.Add (acc);
						}
					}
				}
			}


			return assetHolders;

		}

		public TokenRequires GetTokenRequires ()
		{


			Decimal? min = null;
			Decimal? max = null;

			if (mincheckbox.Active) {
				String minStr = mincomboboxentry.ActiveText;
				min = RippleCurrency.ParseDecimal (minStr);
				if (min == null) {
					return null;

				}

			}

			if (maxcheckbox.Active) {
				String maxStr = maxcomboboxentry.ActiveText;
				max = RippleCurrency.ParseDecimal (maxStr);
				if (max == null) {
					return null;
				}
			}


			TokenRequires tokenRequires = new TokenRequires {
				ShareIssuer = shareIssuerComboboxentry.ActiveText,
				ShareTick = sharetickercomboboxentry.ActiveText,
				MinAmount = min,
				MaxAmount = max
			};




			return tokenRequires;
		}


		public void DoAndOrBoxGui ()
		{
			
			andOrBox.Visible = includecheckbox.Active && requireTokenCheckbox.Active;
			andOrBox.Sensitive = includecheckbox.Active && requireTokenCheckbox.Active;

		}

		public void SetRippleWallet ( RippleWallet rippleWallet)
		{
			_rippleWallet = rippleWallet;
		}

		private RippleWallet _rippleWallet = null;

	}

	public class TokenRequires
	{
		public string ShareIssuer {
			get;
			set;
		}

		public string ShareTick {
			get;
			set;
		}

		public Decimal? MinAmount {
			get;
			set;
		}

		public Decimal? MaxAmount {
			get;
			set;

		}
	}
}

