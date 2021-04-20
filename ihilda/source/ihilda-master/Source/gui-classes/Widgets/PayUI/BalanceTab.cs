using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using IhildaWallet.Networking;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Keys;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;
using RippleLibSharp.Transactions.TxTypes;
using RippleLibSharp.Trust;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class BalanceTab : Gtk.Bin
	{
		public BalanceTab ()
		{

#if DEBUG
			string method_sig = clsstr + nameof (BalanceTab) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.BalanceTab) {

				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			BalanceTab.CurrentInstance = this;
			this.Build ();


#if DEBUG
			if (DebugIhildaWallet.BalanceTab) {
				Logging.WriteLog (method_sig + DebugIhildaWallet.buildComp);
			}
#endif

			ListStoreObj = new ListStore (
				typeof (string),
				typeof (string),
				typeof (string)
			);

			Gtk.CellRendererText cell = new Gtk.CellRendererText {
				Editable = true

			};

			Gtk.CellRendererText nonEditable = new CellRendererText {
				Editable = false
			};



			//cell.Mode = CellRendererMode.

			this.treeview1.AppendColumn ("Currency", cell, "markup", 0);
			this.treeview1.AppendColumn ("Issuer", cell, "markup", 1);
			this.treeview1.AppendColumn ("Balance", nonEditable, "markup", 2);

			this.treeview1.HoverSelection = false;

			SetInteractivty ();



		}

		public void SetInteractivty ()
		{
			this.treeview1.ButtonReleaseEvent += Treeview1_ButtonReleaseEvent;
		}

		void Treeview1_ButtonReleaseEvent (object o, ButtonReleaseEventArgs args)
		{

#if DEBUG
			String method_sig = clsstr + nameof (Treeview1_ButtonReleaseEvent) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.BalanceTab) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif



			if (args.Event.Button == 1) {
				return;
			}

			TreeSelection ts = treeview1.Selection;

			if (ts == null) {
#if DEBUG
				if (DebugIhildaWallet.BalanceTab) {
					Logging.WriteLog (method_sig + "Selected item is null");

				}
#endif
				return;
			}



			bool success = ts.GetSelected (out TreeModel tm, out TreeIter ti);


			if (!success) {


#if DEBUG
				if (DebugIhildaWallet.BalanceTab) {
					Logging.WriteLog (method_sig + "failed to retreive string from UI, returning null");
				}
#endif





			}


#if DEBUG

			if (DebugIhildaWallet.BalanceTab) {
				Logging.WriteLog (method_sig + "retrieved value");
			}
#endif


			string cur = (string)tm.GetValue (ti, 0);

			string add = (string)tm.GetValue (ti, 1);

			string bal = (string)tm.GetValue (ti, 2);

			Menu menu = new Menu ();

			MenuItem copy = new MenuItem ("Copy " + add);
			Gtk.Label l = (Gtk.Label)copy.Child;
			l.UseMarkup = true;
			copy.Activated += (object sender, EventArgs e) => {

				var clipboard = this.GetClipboard (Gdk.Selection.Clipboard);
				clipboard.Clear ();

				clipboard.Text = add;
			};

			copy.Show ();


			menu.Add (copy);


			if (!bal.Contains (">0 limit<")) {
				MenuItem remove = new MenuItem ("Remove Trustline for " + add);
				l = (Gtk.Label)remove.Child;
				l.UseMarkup = true;


				remove.Activated += (object sender, EventArgs e) => {

					string account = this._rippleAddress;
					RippleCurrency currency = new RippleCurrency {
						issuer = add,
						amount = decimal.Zero,
						currency = cur
					};

					RippleWallet rippleWallet = WalletManager.GetRippleWallet ();
					if (rippleWallet == null) {
						MessageDialog.ShowMessage (
							"Error",
							    "Unable to open transaction manager. No wallet has been selected"
						);

						return;
					}

					if (account == null) {
						// TODO
						MessageDialog.ShowMessage (
							"Error",
							"Failed to retrieve wallet"
						);
						return;
					}

					if (account != rippleWallet.GetStoredReceiveAddress ()) {
						// TODO this might be a redundant check with regular key's implemented
						MessageDialog.ShowMessage (
							"Error",
							"Account and signing wallet do no match"
						);
						return;
					}

					RippleTrustSetTransaction rippleTrustSetTransaction = new RippleTrustSetTransaction (account, currency);

					TransactionSubmitWindow transactionSubmitWindow = new TransactionSubmitWindow (rippleWallet, Util.LicenseType.NONE);
					transactionSubmitWindow.SetTransactions (rippleTrustSetTransaction);
					transactionSubmitWindow.Show ();
				};

				remove.Show ();


				menu.Add (remove);
			}

			MenuItem edit = new MenuItem ("Edit Trustline for " + add);
			l = (Gtk.Label)edit.Child;
			l.UseMarkup = true;


			edit.Activated += (object sender, EventArgs e) => {

				bool worked = Decimal.TryParse (bal, out Decimal amnt);

				string account = this._rippleAddress;
				RippleCurrency currency = new RippleCurrency {
					issuer = add,
					amount = worked ? amnt : Decimal.Zero,
					currency = cur
				};

				RippleWallet rippleWallet = WalletManager.GetRippleWallet ();
				if (rippleWallet == null) {
					MessageDialog.ShowMessage (
						"Error",
						    "Unable to open transaction manager. No wallet has been selected"
					);

					return;
				}

				if (account == null) {
					// TODO might be redundant with regular key support
					MessageDialog.ShowMessage (
						"Error",
						"Failed to retrieve account from wallet"
					);
					return;
				}

				if (account != rippleWallet.GetStoredReceiveAddress ()) {
					// TODO
					MessageDialog.ShowMessage (
						"Error",
						"Account and signing wallet do no match"
					);
					return;
				}

				

				//RippleTrustSetTransaction rippleTrustSetTransaction = new RippleTrustSetTransaction (account, currency);

				//TransactionSubmitWindow transactionSubmitWindow = new TransactionSubmitWindow (rippleWallet, Util.LicenseType.NONE);
				//transactionSubmitWindow.SetTransactions (rippleTrustSetTransaction);
				//transactionSubmitWindow.Show ();

				TrustManagementWindow trustManagementWindow = new TrustManagementWindow (rippleWallet);
				TrustLine trustLine = new TrustLine (currency.issuer, currency.amount.ToString(), currency.currency, (0).ToString(), (0).ToString(), 0, 0);
				trustManagementWindow.EditTrustLine (trustLine);
				trustManagementWindow.Show ();



			};

			edit.Show ();


			menu.Add (edit);



			menu.Popup ();
		}




		private void Clear ()
		{

			Application.Invoke (
				delegate {

					ListStoreObj?.Clear ();
					treeview1.Model = ListStoreObj;

				}
			);

		}



		private string _rippleAddress = null;
		public void SetAddress (RippleAddress ra)
		{

#if DEBUG
			string method_sig = clsstr + nameof (SetAddress) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.BalanceTab) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			_rippleAddress = ra;

			if (ra == null) {

				Clear ();
				return;
			}


			if (!ra.Equals (_rippleAddress)) {
				Clear ();
			}




			this.UpdateBalance ();

		}


		private CancellationTokenSource balanceTokenSource = null;
		public void UpdateBalance ()
		{
			balanceTokenSource?.Cancel ();
			balanceTokenSource = new CancellationTokenSource ();

			Task.Run (
				delegate {
					this.Clear ();
					NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();
					if (ni == null) {
						bool test = NetworkController.DoNetworkingDialogNonGUIThread ();
						if (!test) {

							return;
						}

					}





					RippleAddress ra = _rippleAddress;

					if (ra == null) {
						return;
					}

					Task<Response<AccountLinesResult>> task = 
						AccountLines.GetResult (
							ra.ToString (), 
							ni, 
			    				balanceTokenSource.Token
			    			);



					task.Wait (balanceTokenSource.Token);

					Response<AccountLinesResult> response = task.Result;

					AccountLinesResult res = response?.result;

					if (res == null) {
						return;
					}

					TrustLine [] lines = res?.lines;
					if (lines == null) {
						return;
					}

					IEnumerable <RippleCurrency> cur = res?.GetBalancesAsRippleCurrencies ();

					if (cur == null) {
						return;
					}

					this.SetCurrencies (cur);
				}



			);
		}

		public void SetCurrencies (IEnumerable <RippleCurrency>  currencyArray)
		{


#if DEBUG
			String method_sig = clsstr + nameof (SetCurrencies) + DebugRippleLibSharp.left_parentheses + currencyArray.ToString () + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.BalanceTab) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}

#endif

			// this sets the balance as well. 
			Gtk.Application.Invoke (delegate {
#if DEBUG
				if (DebugIhildaWallet.BalanceTab) {
					Logging.WriteLog (method_sig + DebugIhildaWallet.gtkInvoke);
				}
#endif

				TextHighlighter highlighter = new TextHighlighter ();

				ListStoreObj?.Clear ();

				List<Tuple<string, string, string>> values = new List<Tuple<string, string, string>> ();
				List<Tuple<string, string, string>> zerovalues = new List<Tuple<string, string, string>> ();

				foreach (RippleCurrency c in currencyArray) {

					
					if (c == null) {
						continue;
					}
					string cu = c.currency;
					string iss = c.IsNative ? "native currency" : c.issuer;
					string ba = c.amount.ToString ();
					string lim = c.SelfLimit;

					highlighter.Highlightcolor = "\"grey\"";
					lim = highlighter.Highlight (lim + " limit");

					if (c.amount == decimal.Zero) {
						highlighter.Highlightcolor = "\"grey\"";
						cu = highlighter.Highlight (cu);
						iss = highlighter.Highlight (iss);
						ba = highlighter.Highlight (ba);

						zerovalues.Add (new Tuple<string, string, string> (cu, iss, "<b>" + ba + "</b>" + "\n" + lim ));
					} else if (c.amount < decimal.Zero) {
						highlighter.Highlightcolor = "\"red\"";
						//cu = TextHighlighter.Highlight (cu);
						//iss = TextHighlighter.Highlight (iss);
						ba = highlighter.Highlight (ba);

						values.Add (new Tuple<string, string, string> (cu, iss, "<b>" + ba + "</b>" + "\n" + lim ));
					} else {

						highlighter.Highlightcolor = ProgramVariables.darkmode ? "\"chartreuse\"" : "\"green\"";
						//cu = TextHighlighter.Highlight (cu);
						//iss = TextHighlighter.Highlight (iss);
						ba = highlighter.Highlight (ba);

						values.Add (new Tuple<string, string, string> (cu, iss, "<b>" + ba + "</b>" + "\n" + lim ));
					}





				}

				values.AddRange (zerovalues);

				foreach (var v in values) {
					ListStoreObj.AppendValues (
						v.Item1,
						v.Item2,
		    				v.Item3
					);
				}



				treeview1.Model = ListStoreObj;



			}); // end delegate


		} // end public set() (str[] str) 


		private Gtk.ListStore ListStoreObj {
			get;
			set;
		}

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public static BalanceTab CurrentInstance = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant


#if DEBUG
		private static string clsstr = nameof (BalanceTab) + DebugRippleLibSharp.colon;
#endif

	}
}

