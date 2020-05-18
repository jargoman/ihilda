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
using static IhildaWallet.MemoCreateDialog;
using RippleLibSharp.Commands.Subscriptions;

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

			this.sendcurrencyentry.Changed += this.OnComboboxentryChanged;

			this.issuerentry.Entry.Activated += this.OnIssuerEntryActivated;
			this.issuerentry.SelectionReceived += this.OnIssuerSelection;

			this.issuerentry.Changed += this.OnComboboxentryChanged;
			this.destinationentry.Activated += this.OnDestinationEntryActivated;
			this.destinationentry.Changed += Destinationentry_Changed;
			this.sendmaxentry.Entry.Activated += this.OnSendMaxEntryActivated;
			this.receiveamountentry.Activated += this.OnReceiveAmountEntryActivated;

			this.sendbutton.Clicked += this.OnSendButtonClicked;
			this.addmemobutton.Clicked += (object sender, EventArgs e) => {

				SelectableMemoIndice createdMemo = null;
				using (MemoCreateDialog memoCreateDialog = new MemoCreateDialog ()) {
					try {
						ResponseType resp = (ResponseType)memoCreateDialog.Run ();


						if (resp != ResponseType.Ok) {

							return;
						}
						createdMemo = memoCreateDialog.GetMemoIndice ();
						this.AddMemo (createdMemo);
					} catch (Exception ee) {
						throw ee;
					} finally {
						memoCreateDialog?.Destroy ();
					}
				}


				Task.Factory.StartNew (() => {
					var token = TokenSource.Token;
					while (!token.IsCancellationRequested) {
						if (TokenSource == null) {
							return;
						}

						for (int i = 0; i < 5; i++) {
							WaitHandle.WaitAny (
								new WaitHandle [] {
								LedgerTracker.LedgerResetEvent,
								token.WaitHandle
								},
								6000
							    );
						}
						UpdateCurrencies ();
						//await Task.Delay (30000, token);
						
					}
				}
			);

			};

			clearmemobutton.Clicked += (object sender, EventArgs e) => {
				ListStore.Clear ();

				Memos = null;

			};

			CellRendererToggle rendererToggle = new CellRendererToggle () {
				Activatable = true
			};

			CellRendererText cellRendererText = new CellRendererText ();

			treeview1.AppendColumn ("Enabled", rendererToggle, "active", 0);
			treeview1.AppendColumn ("MemoType", cellRendererText, "text", 1);
			treeview1.AppendColumn ("MemoFormat", cellRendererText, "text", 2);
			treeview1.AppendColumn ("MemoData", cellRendererText, "text", 3);

			ListStore = new ListStore (
					typeof (bool),
					typeof (string),
		    			typeof (string),
					typeof (string)
				);


			var memo = Program.GetClientMemo ();
			this.AddMemo (memo);
			//currentInstance = this;

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


		void Hscale2_ValueChanged (object sender, EventArgs e)
		{
#if DEBUG
			string method_sig = clsstr + nameof (Hscale2_ValueChanged) + DebugRippleLibSharp.colon;
#endif
			double val = hscale2.Value;

			string cur = sendcurrencyentry?.Entry?.Text;

			if (cur == null) {

				string message = "Must set sending currency to use percentage shortcuts";
				if (ProgramVariables.darkmode) {
					sendcurrencyentry.ModifyBase (StateType.Normal, new Gdk.Color (0xff, 0xAA, 0xBB));

					infobar.Text = "<span fgcolor=\"#FFAABB\">" + message + "</span>";
					
				} else {
					sendcurrencyentry.ModifyBase (StateType.Normal, new Gdk.Color (0xFF, 0x00, 0x00));
					infobar.Text = "<span fgcolor=\"#Red\">" + message + "</span>";
				}
				return;
			}

			

			string issuer = issuerentry?.Entry?.Text;

			if (!RippleCurrency.NativeCurrency.Equals (cur) && issuer == null) {

				string message = "Must set issuer to use percentage shortcuts for non xrp currencies";
				if (ProgramVariables.darkmode) {
					issuerentry.ModifyBase (StateType.Normal, new Gdk.Color (0xff, 0xAA, 0xBB));

					infobar.Text = "<span fgcolor=\"#FFAABB\">" + message + "</span>";

				} else {
					issuerentry.ModifyBase (StateType.Normal, new Gdk.Color (0xFF, 0x00, 0x00));
					infobar.Text = "<span fgcolor=\"#Red\">" + message + "</span>";
				}

				return;
			}


			Task.Run (delegate {
				ScaleMethod (val, cur, issuer);
			});

		}

		public void ScaleMethod (double value, string currency, string issuer)
		{
#if DEBUG
			string method_sig = clsstr + nameof (ScaleMethod) + DebugRippleLibSharp.colon;

#endif

			string acc = _rippleWallet.Account;



			NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();

			CancellationTokenSource tokenSource = new CancellationTokenSource ();
			CancellationToken token = tokenSource.Token;


			if (!RippleCurrency.NativeCurrency.Equals (currency)) {
				var cur = AccountLines.GetBalanceForIssuer
				(
					currency,
					issuer,
					acc,
					ni,
					token
				);

				double bal = (double)cur.amount;

				double res = bal * value / 100;




				var ss = res.ToString ();
				Gtk.Application.Invoke (
				delegate {

					sendmaxentry.Entry.Text = ss;
				});

			} else {

				Task<Response<AccountInfoResult>> task =
					AccountInfo.GetResult (acc, ni, token);

				task.Wait (token);

				Response<AccountInfoResult> resp = task.Result;
				AccountInfoResult res = resp.result;

				RippleCurrency reserve = res.GetReserveRequirements (ni, token);

				RippleCurrency rippleCurrency = new RippleCurrency (res.account_data.Balance);

				double bal = (double)(rippleCurrency.amount - reserve.amount) / 1000000 * value / 100;

				

				string ss = bal.ToString ();
				Gtk.Application.Invoke (delegate {
					sendmaxentry.Entry.Text = ss;
				});
			}

		}

		Gtk.ListStore ListStore {
			get;
			set;
		}

		~SendAndConvert ()
		{
			TokenSource?.Cancel ();
			TokenSource = null;
		}

		void PercentageClicked (object sender, EventArgs e)
		{
			if (sender is Button b) {
				string s = b?.Label.TrimEnd ('%');
				double d = Convert.ToDouble (s);

				hscale2.Value = d;
			}
		}


		private CancellationTokenSource TokenSource = new CancellationTokenSource ();

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

				sendcurrencyentry.Model = store;


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

		private IEnumerable<SelectableMemoIndice> Memos {
			get;
			set;
		}


		public void AddMemo (SelectableMemoIndice indice)
		{
			List<SelectableMemoIndice> memoIndices = Memos?.ToList () ?? new List<SelectableMemoIndice> ();
			indice.IsSelected = true;
			memoIndices.Add (indice);

			SetMemos (memoIndices);

		}



		public void SetMemos (IEnumerable<SelectableMemoIndice> Memos)
		{
			Gtk.Application.Invoke (
				delegate {
					ListStore.Clear ();

					foreach (SelectableMemoIndice memoIndice in Memos) {
						ListStore.AppendValues (
							memoIndice.IsSelected,
							memoIndice?.GetMemoTypeAscii (),
							memoIndice?.GetMemoFormatAscii (),
							memoIndice?.GetMemoDataAscii ()
						);
					}

					this.Memos = Memos;
					this.treeview1.Model = ListStore;

				}
			);

		}


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


		private CancellationTokenSource destcancelSource = null;
		void Destinationentry_Changed (object sender, EventArgs e)
		{
			destcancelSource?.Cancel ();
			destcancelSource = new CancellationTokenSource ();
			CancellationToken token = destcancelSource.Token;

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

					Task<Response<AccountCurrenciesResult>> task = AccountCurrencies.GetResult (account, networkInterface, token);
					if (task == null) {
						return;
					}
					task.Wait (token);
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


						this.destCurrencyEntry.Model = store;

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
			String destinationTag = destinationTagcomboboxentry?.ActiveText;


			String receiveamount = this.receiveamountentry.Text;


			String sendcurrency = this.sendcurrencyentry.ActiveText;
			String sendmax = this.sendmaxentry.Entry.Text;

			String receiveCurrency = this.destinationentry.Text;


			/* // Maybe I should allow self payments?? // yes allow self payments for self converting payments/. 
			if (account.Equals(destination)) {
				MessageDialog.showMessage ("You're trying to send to yourself?");
				return;
			}
			*/


#pragma warning disable 0168

			decimal max = 0;
			decimal receiveamountd = 0;
			try {

				receiveamountd = Convert.ToDecimal (receiveamount);

				if (receiveamountd < 0) {
					MessageDialog.ShowMessage ("Sending negative amounts is not supported. Please enter a valid amount");
					return;
				}

				

				




				

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


	    		if (!string.IsNullOrWhiteSpace(sendmax)) {
				sendmax = sendmax.Trim ();
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
					MessageDialog.ShowMessage ("SendMax is fomated incorrectly for sending an IOU. It must be a valid decimal number or left blank");
					return;
				}


			}

			uint? DestTag = null;
			UInt32 res = 0;

			if (!string.IsNullOrWhiteSpace (destinationTag)) {
				destinationTag = destinationTag.Trim ();
				bool hasDest = UInt32.TryParse (destinationTag, out res);
				if (!hasDest) {
					return;
				}

				if (res == 0) {
					string msg = "<span fgcolor>You've specified a destination tag of zero</span>. A destination tag is used by the recipiant to distinguisg payments from one another. Ask your recepient what destination tag if any to use\nWould you like to continue with a destination tag of zero?";
					bool b = AreYouSure.AskQuestion ("destination tag is zero", msg);

					if (!b) {
						return;
					}
				}
				DestTag = new uint? (res);

			}



			this.SendConvertPayment (destination, DestTag, receiveamountd, sendcurrency, issuer, max, receiveCurrency);

#pragma warning restore 0168

		}

		protected void SendConvertPayment (String destination, uint? DestTag, decimal receiveamount, String sendcurrency, String sendissuer, decimal sendmax, String destcurrency)
		{
#if DEBUG
			string method_sig = clsstr + nameof (SendConvertPayment) + DebugRippleLibSharp.both_parentheses;
			string method_sig_long = method_sig +

				", destination=" + DebugIhildaWallet.ToAssertString (destination) +
				", receiveamount=" + DebugIhildaWallet.ToAssertString (receiveamount) +

				", sendcurrency=" + DebugIhildaWallet.ToAssertString (sendcurrency) +
				", sendissuer=," + DebugIhildaWallet.ToAssertString (sendissuer) +
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
			if (RippleCurrency.NativeCurrency.Equals (sendcurrency)) {


				// if xrp to xrp transaction
				if (RippleCurrency.NativeCurrency.Equals (destcurrency)) {
					MessageDialog.ShowMessage ("Request denied. To send " + RippleCurrency.NativeCurrency + " to " + RippleCurrency.NativeCurrency + " just use the send ripples tab.");
					return;
				}

				decimal amnt = sendmax * 1000000m;

				decimal floored = Math.Floor (amnt);

				if (!(amnt.Equals (floored))) {
					// User entered too many decimals. 
					MessageDialog.ShowMessage ("Warning, you entered too many decimals. Amount " + amnt.ToString () + " " + RippleCurrency.NativeCurrency + " will truncated to " + floored.ToString ());
					amnt = floored;
				}


				SendMax = new RippleCurrency (amnt);

			} else {

				SendMax = new RippleCurrency (sendmax, new RippleAddress (sendissuer), sendcurrency);
			}


			if (RippleCurrency.NativeCurrency == destcurrency) {
				decimal snd = receiveamount * 1000000m;
				decimal floor = Math.Floor (snd);

				if (!(snd.Equals (floor))) {
					MessageDialog.ShowMessage ("Warning, you entered too many decimals. SendMax " + snd.ToString () + " " + RippleCurrency.NativeCurrency + " will truncated to " + floor.ToString ());
					snd = floor;
				}


				Amount = new RippleCurrency (snd);

			} else {


				Amount = new RippleCurrency (receiveamount, rw.GetStoredReceiveAddress (), destcurrency);
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
			) {
				DestinationTag = DestTag,
				Memos = Memos?.Where ((SelectableMemoIndice arg1) => arg1.IsSelected).ToArray ()
			};


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


		private CancellationTokenSource IssuerTokenSource = null;
		private void UpdateCurrencyIssuers ()
		{
#if DEBUG
			String method_sig = clsstr + nameof (UpdateCurrencyIssuers) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.SendAndConvert) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			IssuerTokenSource?.Cancel ();
			IssuerTokenSource = new CancellationTokenSource ();
			CancellationToken token = IssuerTokenSource.Token;

			Task.Run (
				delegate {



					RippleWallet rw = _rippleWallet;
					string address = rw.GetStoredReceiveAddress ();
					String cur = null;
					using (ManualResetEvent manualResetEvent = new ManualResetEvent (false)) {
						manualResetEvent.Reset ();
						Gtk.Application.Invoke (delegate {
#if DEBUG
							if (DebugIhildaWallet.SendAndConvert) {
								Logging.WriteLog (method_sig + "gtk invoke");
							}
#endif

							cur = this.sendcurrencyentry.ActiveText;


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

						//manualResetEvent.WaitOne (token);
						WaitHandle.WaitAny (new [] { manualResetEvent, token.WaitHandle });


						NetworkInterface ni = NetworkController.CurrentInterface;

						if (ni == null) {
							return;
						}



						IEnumerable<String> lis = AccountLines.GetIssuersForCurrency (cur, address, ni, token);

						manualResetEvent.Reset ();
						Application.Invoke ((object sender, EventArgs e) => {
							ListStore store = new ListStore (typeof (string));

							foreach (String s in lis) {
								store.AppendValues (s);
							}


							this.issuerentry.Model = store;
							manualResetEvent.Set ();
						});
						//manualResetEvent.WaitOne ();

						WaitHandle.WaitAny (new [] { manualResetEvent, token.WaitHandle });
					}

					UpdateBalanceIOU (_rippleWallet?.GetStoredReceiveAddress ());

				}
			);
		}



		private CancellationTokenSource updateCurrCancelSource = null;
		private void UpdateCurrencies ()
		{

			updateCurrCancelSource?.Cancel ();
			updateCurrCancelSource = new CancellationTokenSource ();
			CancellationToken token = updateCurrCancelSource.Token;

			string account = _rippleWallet?.GetStoredReceiveAddress ();
			if (account == null) {
				return;
			}

			NetworkInterface ni = NetworkController.CurrentInterface;
			if (ni == null) {
				return;
			}

			Task<Response<AccountCurrenciesResult>> task = AccountCurrencies.GetResult (account, ni, token);
			if (task == null) {
				return;
			}

			task.Wait (token);
			Response<AccountCurrenciesResult> response = task.Result;


			AccountCurrenciesResult accountCurrenciesResult = response.result;

			string [] sendCurrencies = accountCurrenciesResult.send_currencies;

			SetCurrencies (sendCurrencies);
		}

		private CancellationTokenSource xrpTokenSource = null;
		public void SyncXRPBalance ()
		{
#if DEBUG
			string method_sig = nameof (SyncXRPBalance) + DebugRippleLibSharp.both_parentheses;

#endif


			xrpTokenSource?.Cancel ();
			xrpTokenSource = new CancellationTokenSource ();
			CancellationToken token = xrpTokenSource.Token;

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

				Task<Response<AccountInfoResult>> task = AccountInfo.GetResult (account, networkInterface, token);

				if (task == null) {

					return;
				}

				task.Wait (token);



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


		private CancellationTokenSource iouCancel = null;

		public void UpdateBalanceIOU (string address)
		{

			iouCancel?.Cancel ();
			iouCancel = new CancellationTokenSource ();
			CancellationToken token = iouCancel.Token;

			if (address == null) {
				return;
			}

			string cur = null;
			RippleAddress issuer = null;
			using (ManualResetEvent mre = new ManualResetEvent (false)) {
				mre.Reset ();

				Gtk.Application.Invoke (

				    delegate {

					    if (this.sendcurrencyentry == null) {
				    // TODO bug

				    return;
					    }
					    try {
						    cur = this.sendcurrencyentry.ActiveText;

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

				//mre.WaitOne (token);
				WaitHandle.WaitAny (new [] { mre, token.WaitHandle });
			}

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
				Decimal d = AccountLines.GetCurrencyAsSingleBalance (address, cur, ni, token);
				s = d.ToString ();

			} else {

				result = AccountLines.GetBalanceForIssuer (cur, issuer, address, ni, token);
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

