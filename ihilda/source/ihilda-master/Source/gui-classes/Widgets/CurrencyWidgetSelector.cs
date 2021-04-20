using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using RippleLibSharp.Binary;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Result;
using IhildaWallet.Networking;
using RippleLibSharp.Network;
using RippleLibSharp.Transactions;
using RippleLibSharp.Keys;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CurrencyWidgetSelector : Gtk.Bin
	{
		public CurrencyWidgetSelector ()
		{
#if DEBUG
			string constructer_sig = clsstr + nameof (CurrencyWidgetSelector) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.CurrencyWidgetSelector) {
				Logging.WriteLog (constructer_sig + DebugRippleLibSharp.beginn);
			}

#endif

			this.Build ();

#if DEBUG
			if (DebugIhildaWallet.CurrencyWidgetSelector) {
				Logging.WriteLog (constructer_sig + DebugIhildaWallet.buildComp);
			}

#endif

			this.combobox.Changed += (sender, e) => Task.Run ((System.Action)Update);

			this.SetCurrencies (new string [] { RippleCurrency.NativeCurrency, Util.LeIceSense.LICENSE_CURRENCY });

			Task.Factory.StartNew (async () => {

				var token = tokenSource.Token;
				while (token.IsCancellationRequested) {
					await Task.Delay (30000, token);
					Update ();
				}
			}
			);

		}

		~CurrencyWidgetSelector () {
			tokenSource?.Cancel (); // = false;
			tokenSource?.Dispose ();
			tokenSource = null;
		}

		private CancellationTokenSource tokenSource = new CancellationTokenSource();

		public void SetCurrencies (IEnumerable<String> currencies)
		{

			ClearUI ();

			ListStore store = new ListStore (typeof (String));

			foreach (String s in currencies) {


				store.AppendValues (s);

			}

			this.combobox.Model = store;
			this.combobox.Active = 0;

			Update ();

		}

		public void Update ()
		{

			var token = tokenSource.Token;

			Task.Run (delegate {



				String cur = null;
				RippleAddress rippleAddress = _rippleAddress;
				if (rippleAddress == null) {
					ClearUI ();
					return;
				}

				NetworkInterface ni = NetworkController.CurrentInterface;
				if (ni == null) {
					return;
				}

				using (ManualResetEvent mre = new ManualResetEvent (false)) {
					Gtk.Application.Invoke (delegate {

						cur = this?.combobox?.ActiveText;

						// TODO uncomment
						// Decimal d = AccountLines.getCurrencyTotal(cur, rw.getStoredReceiveAddress());
						//String val = Base58.truncateTrailingZerosFromString(d.ToString());
						//this.label2.Text = val;

						mre.Set ();
					});

					mre.WaitOne ();

					WaitHandle.WaitAny (new [] { mre, token.WaitHandle });
				}

				if (token.IsCancellationRequested) {
					return;
				}
				
				if (cur == null) {
					//TODO Debud
					ClearUI ();
					return;
				}

				string message = null;

				if (RippleCurrency.NativeCurrency.Equals (cur.ToUpper ())) {
					RippleCurrency rc = null;
					rc = AccountInfo.GetNativeBalance (rippleAddress, ni, token);
					if (rc == null) {
						return;
					}

					message = rc.ToString ();

				} else {

					Decimal d = AccountLines.GetCurrencyAsSingleBalance (rippleAddress, cur, ni, token);
					message = d.ToString ();
				}



				Application.Invoke (delegate {

					this.label2.Text = message;
				});

			});
		}

		private void ClearUI ()
		{
			Application.Invoke (delegate {
				this.label2.Text = "";
			});
		}


		public void SetRippleAddress (RippleAddress rippleAddress)
		{

			if (rippleAddress == null || !rippleAddress.Equals (_rippleAddress)) {
				ClearUI ();
			}
			this._rippleAddress = rippleAddress;
			Update ();
		}

		// not used for UI. used to retrieve secre
		private RippleAddress _rippleAddress = null;

#if DEBUG
		private static readonly string clsstr = nameof (CurrencyWidgetSelector) + DebugRippleLibSharp.colon;
#endif
	}
}

