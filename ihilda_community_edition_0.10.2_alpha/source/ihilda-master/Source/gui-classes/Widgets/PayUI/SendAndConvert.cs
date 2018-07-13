/*
 *	License : Le Ice Sense 
 */

using System;
using System.Threading.Tasks;
using RippleLibSharp.Util;
using Gtk;
using System.Collections.Generic;
using Codeplex.Data;

using RippleLibSharp.Commands.Accounts;

using RippleLibSharp.Keys;
using RippleLibSharp.Transactions.TxTypes;
using RippleLibSharp.Transactions;
using RippleLibSharp.Network;
using IhildaWallet.Networking;

using RippleLibSharp.Result;
using System.Threading;
using System.Linq;
using IhildaWallet.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class SendAndConvert : Gtk.Bin
	{
		public SendAndConvert ()
		{
			this.Build ();
			//while(Gtk.Application.EventsPending())
			//	Gtk.Application.RunIteration();

#if DEBUG
			if (DebugIhildaWallet.SendAndConvert) {
				Logging.WriteLog (clsstr + "new");
			}
#endif

			//this.issuerentry.enActivated += new EventHandler (this.OnIssuerEntryActivated);

			this.comboboxentry.Changed += this.OnComboboxentryChanged;

			this.issuerentry.Entry.Activated += this.OnIssuerEntryActivated;
			this.issuerentry.SelectionReceived += this.OnIssuerSelection;

			this.issuerentry.Changed += this.OnComboboxentryChanged;
			this.destinationentry.Activated += this.OnDestinationEntryActivated;
			this.destinationentry.Changed += Destinationentry_Changed;
			this.sendmaxentry.Activated += this.OnSendMaxEntryActivated;
			this.receiveamountentry.Activated += this.OnReceiveAmountEntryActivated;

			this.sendbutton.Clicked += this.OnSendButtonClicked;

			//currentInstance = this;
		}

		public void SetCurrencies (String [] currencies)
		{

#if DEBUG
			String method_sig = clsstr + nameof (SetCurrencies) + "( String[] currencies ) ";
			if (DebugIhildaWallet.SendAndConvert) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);

			}
#endif

			if (currencies == null) {
#if DEBUG
				if (DebugIhildaWallet.SendAndConvert) {
					Logging.WriteLog (method_sig + "currencies == null, returning");
				}
#endif
				// todo set currencies to empty and continue??
				return;
			}


#if DEBUG
			if (DebugIhildaWallet.SendAndConvert) {
				Logging.WriteLog (method_sig, currencies);
			}
#endif


			Gtk.Application.Invoke (delegate {
#if DEBUG
				if (DebugIhildaWallet.SendAndConvert) {
					Logging.WriteLog (method_sig + "gtk invoke");
				}
#endif

				ListStore store = new ListStore (typeof (string));

				//foreach (String s in currencies) {
				//	store.AppendValues(s);
				//}

				store.AppendValues (RippleCurrency.NativeCurrency);
				if (currencies != null) {
					foreach (String s in currencies) {
						store.AppendValues (s);
					}
				}

				comboboxentry.Model = store;


			});
		}

		//#pragma warning disable RECS0122 // Initializing field with default value is redundant
		//		public static SendAndConvert currentInstance = null;
		//#pragma warning restore RECS0122 // Initializing field with default value is redundant

		// TODO uncomment
		//
		/*
		public void updateBalance () {
			#if DEBUG
			String method_sig = clsstr + "updateBalance () : ";

			if (Debug.SendAndConvert) {
				Logging.writeLog(method_sig + Debug.begin);
			}
			#endif

			RippleAddress ra = WalletManager.selectedWallet.getStoredReceiveAddress();


			Dictionary<String, Decimal> cash = AccountLines.getCurrencyCache (ra.ToString());

			if (cash == null) {
				#if DEBUG
				if (Debug.SendAndConvert) {
					Logging.writeLog(method_sig + "cash == null, returning");
				}
				#endif

				MessageDialog.showMessage ("what to say :P"); // need to sync // TODO
				return;
			}

			Gtk.Application.Invoke ( delegate {
				#if DEBUG
				if (Debug.SendAndConvert) {
					Logging.writeLog(method_sig + "gtk invoke");
				}
				#endif
				

				if (this.comboboxentry==null) {
					// TODO bug
					#if DEBUG
					if (Debug.SendAndConvert) {
						Logging.writeLog(method_sig + "comboboxentry==null, returning");
					}
					#endif
					return;
				}

				String cur = this.comboboxentry.ActiveText;

				if (cur == null) {
					#if DEBUG
					if (Debug.SendAndConvert) {
						Logging.writeLog(method_sig + "cur == null, returning");
					}
					#endif
				}

				#if DEBUG
				if (Debug.SendAndConvert) {
					Logging.writeLog(method_sig + "cur = " + cur);
				}
				#endif

				cur = cur.Trim();

				if (cash.ContainsKey (cur)) {
					decimal dud;

					if (cash.TryGetValue (cur, out dud)) {

						this.balancelabel.Text = dud.ToString();

					} else {
						// TODO debug
					}


				} // end if cash contains combobox text 

				else {
					// TODO debug/alert user
				}

			} // end delegate
			);  // end invoke


		} // end public void updateBalance
		*/

		protected void OnComboboxentryChanged (object sender, EventArgs e)
		{
			// TODO uncomment
			//this.updateBalance ();
			this.UpdateCurrencyIssuers ();
		}

		protected void OnIssuerEntryActivated (object sender, EventArgs e)
		{
			// issuer entry
			if (this.destinationentry == null) {
				// TODO debug
				return;
			}

			this.destinationentry.GrabFocus ();
		}

		protected void OnIssuerSelection (object o, SelectionReceivedArgs args)
		{
			if (this.destinationentry == null) {
				// TODO debug
				return;
			}

			this.destinationentry.GrabFocus ();
		}
		protected void OnDestinationEntryActivated (object sender, EventArgs e)
		{

			// Destination
			if (sendmaxentry == null) {
				//TODO debug

				return;
			}

			this.sendmaxentry.GrabFocus ();
		}

		void Destinationentry_Changed (object sender, EventArgs e)
		{

			string account = destinationentry?.Text;
			Task.Run (
				delegate {

					if (account == null) {
						// TODO
						return;
					}

					if (account == "") {
						return;
					}

					NetworkInterface networkInterface = NetworkController.CurrentInterface;
					if (networkInterface == null) {
						// TODO
						return;
					}

					Task<Response<AccountCurrenciesResult>> task = AccountCurrencies.GetResult (account, networkInterface);
					if (task == null) {
						return;
					}
					task.Wait ();
					Response<AccountCurrenciesResult> response = task.Result;


					if (response == null) {
						return;
					}

					AccountCurrenciesResult result = response.result;
					if (result == null) {
						return;
					}

					string [] currencies = result.receive_currencies;
					if (currencies == null) {
						return;
					}

					if (!currencies.Any ()) {
						return;
					}


					Application.Invoke ((object senderObj, EventArgs evnt) => {
						ListStore store = new ListStore (typeof (string));

						foreach (String s in currencies) {
							store.AppendValues (s);
						}


						this.comboboxentry2.Model = store;

					});

				}
			);
		}


		protected void OnSendMaxEntryActivated (object sender, EventArgs e)
		{
			if (this.receiveamountentry == null) {
				// TODO degub
				return;
			}

			this.receiveamountentry.GrabFocus ();
		}

		protected void OnReceiveAmountEntryActivated (object sender, EventArgs e)
		{
			//throw new NotImplementedException ();
			if (this.sendbutton == null) {
				// TODO debug
				return;
			}

			this.sendbutton.GrabFocus ();
		}

		protected void OnSendButtonClicked (object sender, EventArgs e)
		{
			Send ();
		}

		private void Send ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (Send) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.SendAndConvert) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			//RippleWallet rw = MainWindow.currentInstance.getRippleWallet();


			String issuer = this.issuerentry.Entry.Text;


			String destination = this.destinationentry.Text;

			String amount = this.receiveamountentry.Text;


			String currency = this.comboboxentry2.ActiveText;
			String sendmax = this.sendmaxentry.Text;

			String receiveCurrency = this.comboboxentry.ActiveText;


			/* // Maybe I should allow self payments?? // yes allow self payments for self converting payments/. 
			if (account.Equals(destination)) {
				MessageDialog.showMessage ("You're trying to send to yourself?");
				return;
			}
			*/


#pragma warning disable 0168
			try {

				decimal amountd = Convert.ToDecimal (amount);

				if (amountd < 0) {
					MessageDialog.ShowMessage ("Sending negative amounts is not supported. Please enter a valid amount");
					return;
				}

				decimal max = 0;

				if (!("".Equals (sendmax.Trim ()))) { // if sendmax is not blank

					try {
						max = Convert.ToDecimal (sendmax); //Convert.ToDouble (sendmax);  // and is a valid number


					} catch (FormatException ex) {
#if DEBUG
						if (DebugIhildaWallet.SendAndConvert) {
							Logging.WriteLog (method_sig + "FormatException\n");
							Logging.WriteLog (ex.Message);
						}
#endif
						MessageDialog.ShowMessage ("SendMax is fomated incorrectly for sending an IOU. It must be a valid decimal number or left blank");
						return;

					} catch (OverflowException ex) {
						MessageDialog.ShowMessage ("SendMax is greater than a double? No one's got that much money");
						return;
					} catch (Exception ex) {
						MessageDialog.ShowMessage ("Amount is fomated incorrectly for sending an IOU. It must be a valid decimal number or left blank");
						return;
					}


				} else {
					max = amountd;
				}




				this.SendConvertPayment (destination, amountd, currency, issuer, max, receiveCurrency);

			} catch (FormatException ex) {

				MessageDialog.ShowMessage ("Amount is fomated incorrectly for sending an IOU.\n It must be a valid decimal number\n");
				return;

			} catch (OverflowException ex) {
				MessageDialog.ShowMessage ("Send amount is greater than a double? No one's got that much money\n");
				return;
			} catch (Exception ex) {
				MessageDialog.ShowMessage ("Amount is fomated incorrectly for sending an IOU.\n It must be a valid decimal number\n");
				return;
			}

#pragma warning restore 0168

		}

		protected void SendConvertPayment (String destination, decimal amount, String currency, String issuer, decimal sendmax, String destcurrency)
		{
#if DEBUG
			string method_sig = clsstr + nameof (SendConvertPayment) + DebugRippleLibSharp.both_parentheses;
			string method_sig_long = method_sig +

				", destination=" + DebugIhildaWallet.ToAssertString (destination) +
				", amount=" + DebugIhildaWallet.ToAssertString (amount) +

				", currency=" + DebugIhildaWallet.ToAssertString (currency) +
				", issuer=," + DebugIhildaWallet.ToAssertString (issuer) +
				" sendmax=," + DebugIhildaWallet.ToAssertString (sendmax) +
				" destcurrency=" + DebugIhildaWallet.ToAssertString (destcurrency) +
				" ) : ";
			if (DebugIhildaWallet.SendAndConvert) {
				Logging.WriteLog (method_sig_long + DebugRippleLibSharp.beginn);
			}
#endif


			RippleWallet rw = _rippleWallet;
			if (rw == null) {
#if DEBUG
				if (DebugIhildaWallet.SendAndConvert) {
					Logging.WriteLog (method_sig + "rw == null, returning");
				}
#endif
				return;
			}


			RippleCurrency Amount = null;
			RippleCurrency SendMax = null;

			// if the sending curreny is XRP
			if (RippleCurrency.NativeCurrency.Equals (currency)) {


				// if xrp to xrp transaction
				if (RippleCurrency.NativeCurrency.Equals (destcurrency)) {
					MessageDialog.ShowMessage ("Request denied. To send " + RippleCurrency.NativeCurrency + " to " + RippleCurrency.NativeCurrency + " just use the send ripples tab.");
					return;
				}

				decimal amnt = amount * 1000000m;

				decimal floored = Math.Floor (amnt);

				if (!(amnt.Equals (floored))) {
					// User entered too many decimals. 
					MessageDialog.ShowMessage ("Warning, you entered too many decimals. Amount " + amnt.ToString () + " " + RippleCurrency.NativeCurrency + " will truncated to " + floored.ToString ());
					amnt = floored;
				}


				Amount = new RippleCurrency (amnt);

			} else {

				Amount = new RippleCurrency (amount, new RippleAddress (issuer), currency);
			}


			if (RippleCurrency.NativeCurrency == destcurrency) {
				decimal snd = sendmax * 1000000m;
				decimal floor = Math.Floor (snd);

				if (!(snd.Equals (floor))) {
					MessageDialog.ShowMessage ("Warning, you entered too many decimals. SendMax " + snd.ToString () + " " + RippleCurrency.NativeCurrency + " will truncated to " + floor.ToString ());
					snd = floor;
				}


				SendMax = new RippleCurrency (snd);

			} else {


				SendMax = new RippleCurrency (sendmax, rw.GetStoredReceiveAddress (), destcurrency);
			}


			if (Amount == null || SendMax == null) {
#if DEBUG
				if (DebugIhildaWallet.SendAndConvert) {
					Logging.WriteLog ("SendAndConvert : Error : Either Amount or SendMax is null\n");
				}
#endif
				return;
			}


			RipplePaymentTransaction tx = new RipplePaymentTransaction (
				new RippleAddress (rw.GetStoredReceiveAddress ()),
				new RippleAddress (destination),
				Amount,
				SendMax
			);





			NetworkInterface ni = NetworkController.GetNetworkInterfaceGuiThread ();
			if (ni == null) {
				NetworkController.DoNetworkingDialogNonGUIThread ();
				return;
			}

			LicenseType licenseT = Util.LicenseType.TRADING;
			if (LeIceSense.IsLicenseExempt (Amount)) {
				licenseT = LicenseType.NONE;
			}

			Application.Invoke (
				delegate {


					PaymentSubmitWindow paymentWindow = new PaymentSubmitWindow (rw, licenseT);
					paymentWindow.SetPayments (tx);
					paymentWindow.Show ();

				}
			);

			//tx.autoRequestSequence (rw.getStoredReceiveAddress(), ni);

			//Tuple<UInt32,UInt32> fl = FeeSettings.getFeeAndLastLedgerFromSettings (ni);
			//if (fl == null) {
			//	return;
			//}

			//tx.fee = fl.Item1.ToString ();
			//tx.LastLedgerSequence = fl.Item2 + 5;

			//RippleSeedAddress rsa = rw.getDecryptedSeed ();
			//tx.sign(rsa);


			//Task< Response <RippleSubmitTxResult>> task = NetworkController.uiTxNetworkSubmit (tx, ni);
			//task.Wait ();
			//tx.submit(ni);



		} // end sendConvertPayment

		private void UpdateCurrencyIssuers ()
		{
#if DEBUG
			String method_sig = clsstr + nameof (UpdateCurrencyIssuers) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.SendAndConvert) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			Task.Run (
				delegate {



					RippleWallet rw = _rippleWallet;
					string address = rw.GetStoredReceiveAddress ();
					String cur = null;
					ManualResetEvent manualResetEvent = new ManualResetEvent (false);
					manualResetEvent.Reset ();
					Gtk.Application.Invoke (delegate {
#if DEBUG
						if (DebugIhildaWallet.SendAndConvert) {
							Logging.WriteLog (method_sig + "gtk invoke");
						}
#endif

						cur = this.comboboxentry.ActiveText;


						if (RippleCurrency.NativeCurrency == cur) {

							issuerentry.Entry.Text = "";
							issuerentry.Sensitive = false;
							issuerentry.Visible = false;

							label17.Visible = false;

							Task.Run ((System.Action)SyncXRPBalance);

							return;
						}

						issuerentry.Sensitive = true;
						issuerentry.Visible = true;

						label17.Visible = true;

						manualResetEvent.Set ();

					});

					manualResetEvent.WaitOne ();


					NetworkInterface ni = NetworkController.CurrentInterface;

					if (ni == null) {
						return;
					}



					List<String> lis = AccountLines.GetIssuersForCurrency (cur, address, ni);

					manualResetEvent.Reset ();
					Application.Invoke ((object sender, EventArgs e) => {
						ListStore store = new ListStore (typeof (string));

						foreach (String s in lis) {
							store.AppendValues (s);
						}


						this.issuerentry.Model = store;
						manualResetEvent.Set ();
					});
					manualResetEvent.WaitOne ();

					UpdateBalanceIOU (_rippleWallet?.GetStoredReceiveAddress ());

				}
			);
		}


		private void UpdateCurrencies ()
		{
			string account = _rippleWallet?.GetStoredReceiveAddress ();
			if (account == null) {
				return;
			}

			NetworkInterface ni = NetworkController.CurrentInterface;
			if (ni == null) {
				return;
			}

			Task<Response<AccountCurrenciesResult>> task = AccountCurrencies.GetResult (account, ni);
			if (task == null) {
				return;
			}

			task.Wait ();
			Response<AccountCurrenciesResult> response = task.Result;


			AccountCurrenciesResult accountCurrenciesResult = response.result;

			string [] sendCurrencies = accountCurrenciesResult.send_currencies;

			SetCurrencies (sendCurrencies);
		}


		public void SyncXRPBalance ()
		{
#if DEBUG
			string method_sig = nameof (SyncXRPBalance) + DebugRippleLibSharp.both_parentheses;

#endif




			string labelText = "";
			try {

				string account = _rippleWallet?.Account;
				if (account == null) {

					return;
				}

				NetworkInterface networkInterface = NetworkController.CurrentInterface;
				if (networkInterface == null) {

					return;
				}

				Task<Response<AccountInfoResult>> task = AccountInfo.GetResult (account, networkInterface);

				if (task == null) {

					return;
				}

				task.Wait ();



				Response<AccountInfoResult> response = task.Result;
				if (response == null) {

					return;
				}

				if (response.HasError ()) {
					return;
				}

				AccountInfoResult accountTxResult = response.result;
				if (accountTxResult == null) {
					return;
				}



				RippleCurrency rippleCurrency = accountTxResult.GetNativeBalance ();

				labelText = (rippleCurrency.amount / 1000000m).ToString ();


			} catch (Exception e) {

				labelText = "";

#if DEBUG
				Logging.ReportException (method_sig, e);
#endif
			} finally {

				Application.Invoke (delegate {

					balancelabel.Text = labelText ?? "";
				});
			}

		}

		public void UpdateBalanceIOU (string address)
		{
			if (address == null) {
				return;
			}

			string cur = null;
			RippleAddress issuer = null;

			ManualResetEvent mre = new ManualResetEvent (false);
			mre.Reset ();

			Gtk.Application.Invoke (

				delegate {

					if (this.comboboxentry == null) {
						// TODO bug

						return;
					}
					try {
						cur = this.comboboxentry.ActiveText;

					}

#pragma warning disable 0168
					catch (Exception e) {
#pragma warning restore 0168
						cur = null;
						mre.Set ();
						return;
					}

					try {
						issuer = this.issuerentry.ActiveText;
					}

#pragma warning disable 0168
					catch (Exception e) {
#pragma warning restore 0168
						issuer = null;
					}
					mre.Set ();
				} // end delegate

			);  // end invoke

			mre.WaitOne ();

			if (cur == null) {
				return;
			}


			NetworkInterface ni = NetworkController.CurrentInterface;

			if (ni == null) {
				return;
			}

			RippleCurrency result = null;

			string s = null;
			if (issuer == null) {
				Decimal d = AccountLines.GetCurrencyAsSingleBalance (address, cur, ni);
				s = d.ToString ();

			} else {

				result = AccountLines.GetBalanceForIssuer (cur, issuer, address, ni);
				s = result?.amount.ToString ();

			}

			Gtk.Application.Invoke ((object sender, EventArgs e) => {

				this.balancelabel.Text = s ?? "";

			});





		} // end public void updateBalanceIOU

		public void SetRippleWallet (RippleWallet rippleWallet)
		{
			this._rippleWallet = rippleWallet;
			UpdateCurrencies ();
		}

		private RippleWallet _rippleWallet {
			get;
			set;
		}

#if DEBUG
		public const string clsstr = nameof (SendAndConvert) + DebugRippleLibSharp.colon;
#endif

	} // end class
} // end namespace

