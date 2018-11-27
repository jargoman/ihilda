using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using RippleLibSharp.Network;
using IhildaWallet.Networking;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;
using RippleLibSharp.Keys;
using RippleLibSharp.Util;
using System.Threading;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CurrencyWidget : Gtk.Bin
	{
		public CurrencyWidget ()
		{

			TokenSource = new CancellationTokenSource ();
			var token = TokenSource.Token;

#if DEBUG
			string method_sig = clsstr + nameof (CurrencyWidget) + DebugRippleLibSharp.both_parentheses;
#endif

			this.Build ();

			this.SetAsUnset ();


			Task.Factory.StartNew (async () => {



				while (!token.IsCancellationRequested) {

					try {
						await Task.Delay (30000, token);
						UpdateNetwork (TokenSource.Token);
					} catch (Exception e) {
#if DEBUG
						if (DebugIhildaWallet.CurrencyWidget) {
							Logging.ReportException ( clsstr, e );
						}
#endif
					}
				}
			}
			);
			
		}

		~CurrencyWidget ()
		{
			TokenSource?.Cancel ();
			TokenSource?.Dispose ();
			TokenSource = null;
		}

		private CancellationTokenSource TokenSource = null;

		/* note : there is subtle differences between the the 
		 * various set functions
		 * 
		 * 
		 */


		private void SetRippleCurrency (RippleCurrency dic)
		{
			if (dic == null) {
				return;
			}

			Decimal amount = dic.amount;

			if (dic.IsNative) {
				amount /= 1000000;
			}
			//_currency = dic.currency;
			this.SetUI(dic.currency, amount.ToString());
		}


		private void SetUI (String currency, String amount) {



			Gtk.Application.Invoke (
				delegate {

				this.label3.Text = currency ?? "null";
				this.label4.Text = amount ?? "null";

			});



		}



		public void SetCurrencies ( IEnumerable<RippleCurrency> currencies ) {
			if (currencies == null) {
				return;
			}
			decimal totalamount = 0;
			string cur = null;
			foreach (RippleCurrency rc in currencies) {
				totalamount += rc.amount;
				cur = rc.currency;
				//_currency = rc.currency;
			}

			SetUI (cur, totalamount.ToString());

		}


		public void SetCurrency ( String currency ) {
			#if DEBUG
			string method_sig = clsstr + nameof (SetUI) + DebugRippleLibSharp.left_parentheses + nameof (String) + DebugRippleLibSharp.space_char + nameof (currency) + DebugRippleLibSharp.right_parentheses + DebugIhildaWallet.ToAssertString(currency) + ") : ";
			if (DebugIhildaWallet.CurrencyWidget) {
				Logging.WriteLog (method_sig);
			}
			#endif

			_currency = currency;

			if (currency == null) {
				return;
			}







		}

		public void UpdateNetwork (CancellationToken token)
		{

#if DEBUG
			string method_sig = clsstr + nameof (UpdateNetwork) + DebugRippleLibSharp.both_parentheses;

#endif

			NetworkInterface ni = NetworkController.CurrentInterface;
			if (ni == null) {
				return;
			}


			if (_rippleAddress == null) {
#if DEBUG
				if (DebugIhildaWallet.CurrencyWidget) {
					Logging.WriteLog (method_sig + "rw == null, returning\n");
				}
#endif
				return;
			}

			if (_currency == null) {
				return;
			}

			#if DEBUG
			if (DebugIhildaWallet.CurrencyWidget) {
				Logging.WriteLog (method_sig + "rw == null, returning\n");
			}
			#endif

			if ( RippleCurrency.NativeCurrency ==  _currency ) {
				UpdateNative ( _rippleAddress, ni, token );
			} else {
				UpdateIOU (_rippleAddress, _currency, ni, token);
			}

		}

		private void UpdateIOU ( RippleAddress rippleAddress, String currency, NetworkInterface ni, CancellationToken token) {
			Task<Response<AccountLinesResult>> task = AccountLines.GetResult(
				rippleAddress,
				ni,
				token
			);

			if (task == null) {
				// TODO debug
				this.SetAsUnSynced ();
				return;
			}

			task.Wait (token);

			Response<AccountLinesResult> resp = task.Result;

			if (resp == null) {
				// TODO debug
				this.SetAsUnSynced ();
				return;
			}

			if (resp.HasError()) {
				this.SetAsError (resp.error_message);
				return;
			}


			AccountLinesResult alr = resp.result;

			List<RippleCurrency> rc = alr.GetBalanceAsCurrency (currency);
			if (rc == null) {
				this.SetAsUnSynced ();
				return;
			}
			this.SetCurrencies(rc);

		}

		private void UpdateNative ( RippleAddress rippleAddress, NetworkInterface ni, CancellationToken token ) {

			if (rippleAddress == null) {
				return;
			}


			Task <Response < AccountInfoResult >> task = AccountInfo.GetResult (
				rippleAddress.ToString(),
				ni,
				token
			);

			if (task == null) {
				this.SetAsUnSynced ();
				return;
			}

			task.Wait (token);

			Response<AccountInfoResult> resp = task.Result;

			if (resp == null) {
				this.SetAsUnSynced ();
				return;
			}

			if (resp.HasError ()) {
				this.SetAsError (resp.error_message);
				return;
			}

			AccountInfoResult res = resp.result;
			if (res == null) {
				this.SetAsUnSynced ();
				return;
			}

			RippleCurrency rc = res.GetNativeBalance ();
			if (rc == null) {
				this.SetAsUnSynced();
				return;
			}

			this.SetRippleCurrency (rc);
		}

		/*
		public void UpdateBalance ()
		{
			if (this._currency == null) {
				SetCurrency (_currency);
			}

		}
		*/

		private string _currency = null;


		public void SetAsUnset () {
			this.SetUI("UnSet", "  --  Unset-- ");
		}

		public void SetAsUnSynced ()
		{
			this.SetUI (_currency ?? "null", "  --  Unsynced-- ");
		}

		public void SetAsError (String errorMessage)
		{
			this.SetUI (_currency ?? "null", errorMessage);
		}

		public void SetRippleWallet (RippleAddress rippleWallet)
		{

			this._rippleAddress = rippleWallet;
			this.SetAsUnSynced ();

			UpdateNetwork (TokenSource.Token);
		}

		private RippleAddress _rippleAddress = null;

		#if DEBUG
		private static readonly string clsstr = nameof (CurrencyWidget) + DebugRippleLibSharp.colon;
		#endif
	}
}

