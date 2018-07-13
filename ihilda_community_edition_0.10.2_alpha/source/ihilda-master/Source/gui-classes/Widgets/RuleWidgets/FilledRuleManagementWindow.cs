﻿using System;
using System.Linq;

using System.Threading;
using System.Threading.Tasks;

using RippleLibSharp.Network;
using IhildaWallet.Networking;

using System.Collections.Generic;

using Gtk;

using RippleLibSharp.Keys;
using RippleLibSharp.Util;
using IhildaWallet.Util;
using RippleLibSharp.Transactions.TxTypes;
using System.Text;

namespace IhildaWallet
{
	public partial class FilledRuleManagementWindow : Window
	{
		public FilledRuleManagementWindow (RippleWallet rw) :
			base (Gtk.WindowType.Toplevel)
		{

			if (rw == null) {
				throw new NullReferenceException ();
			}

			this.Build ();

			if (walletswitchwidget2 == null) {
				walletswitchwidget2 = new WalletSwitchWidget ();
				walletswitchwidget2.Show ();

				hbox4.Add (walletswitchwidget2);
			}

			if (ledgerconstraintswidget1 == null) {

				ledgerconstraintswidget1 = new LedgerConstraintsWidget ();

				expander1.Add (ledgerconstraintswidget1);
				ledgerconstraintswidget1.Show ();
			}
			ledgerconstraintswidget1.HideForward ();

			//_rippleWallet = rw;



			walletswitchwidget2.WalletChangedEvent += (object source, WalletChangedEventArgs eventArgs) => {

				RippleWallet rippleWallet = eventArgs.GetRippleWallet ();
				this.SetRippleWallet (rippleWallet);
			};

			walletswitchwidget2.SetRippleWallet (rw);

			this.RuleListStore = new Gtk.ListStore (typeof (bool), typeof (string), typeof (string), typeof (string), typeof (string));

			//TextHighlighter.Highlightcolor = TextHighlighter.GREEN;
			//string coloredAdd = TextHighlighter.Highlight (rw.GetStoredReceiveAddress());
			//label1.Markup = "Rules for " + coloredAdd;


			CellRendererToggle toggleCell = new CellRendererToggle {
				Radio = false
			};


			toggleCell.Toggled += (object o, ToggledArgs args) => {


				if (RuleListStore == null) {

				}


				TreePath path = new TreePath (args.Path);


				if (!RuleListStore.GetIter (out TreeIter iter, path)) {
					return;
				}

				int index = path.Indices [0];

				OrderFilledRule ofr = this.RuleManagerObj.RulesList.ElementAt (index);
				if (ofr == null) {

					return;
				}


				ofr.IsActive = !ofr.IsActive;
				toggleCell.Active = ofr.IsActive;
				Task.Run ((System.Action)this.RuleManagerObj.SaveRules);
				SetRules (this.RuleManagerObj.RulesList);
			};

			this.treeview1.AppendColumn ("Active", toggleCell, "active", 0);
			this.treeview1.AppendColumn ("Bought", new CellRendererText (), "text", 1);
			this.treeview1.AppendColumn ("Sold", new CellRendererText (), "text", 2);
			this.treeview1.AppendColumn ("Pay Less", new CellRendererText (), "text", 3);
			this.treeview1.AppendColumn ("Get More", new CellRendererText (), "text", 4);


			this.addbutton.Clicked += (object sender, EventArgs e) => {
				OrderFilledRule rule = RuleCreateDialog.DoDialog ();

				if (rule == null) {
					return;
				}

				this.RuleManagerObj.AddRule (rule);
				this.RuleManagerObj.SaveRules ();
				SetRules (this.RuleManagerObj.RulesList);

			};

			this.removebutton.Clicked += (object sender, EventArgs e) => {
				var v = this.GetSelected ();

				this.RuleManagerObj.RemoveRule (v);
				this.RuleManagerObj.SaveRules ();
				SetRules (this.RuleManagerObj.RulesList);
			};

			this.dologicbutton.Clicked += (object sender, EventArgs e) => {


				Task.Run ((System.Action)DoLogicClicked);
			};

			this.automatebutton.Clicked += (object sender, EventArgs e) => {
				if (tokenSource != null) {
					MessageDialog.ShowMessage ("Automation thread is already running");
					return;
				}



				Task.Run ((System.Action)AutomateClicked);
			};

			button171.Clicked += (object sender, EventArgs e) => {
				if (tokenSource == null) {
					MessageDialog.ShowMessage ("Thread has already been stopped\n");
				}

				tokenSource.Cancel ();
				tokenSource = null;
			};

			//this.exitbutton.Clicked += (object sender, EventArgs e) => this.Destroy();





		}

		private void SetIsRunningUI (bool isRunning)
		{
			string message = "Automation Status : " + (String)(isRunning ? "<span fgcolor=\"green\">Running</span>" : "<span fgcolor=\"red\">Stopped</span>");
			Application.Invoke (delegate {
				walletswitchwidget2.Sensitive = !isRunning;
				label4.Markup = message;
			});

		}


		private void SetRippleWallet (RippleWallet rw)
		{
			Task.Run (delegate {



				RuleManagerObj = new RuleManager (rw.GetStoredReceiveAddress ());
				RuleManagerObj.LoadRules ();

				this.ledgerconstraintswidget1.SetLastKnownLedger (this.RuleManagerObj.LastKnownLedger.ToString ());

				SetRules (this.RuleManagerObj.RulesList);

			});

		}

		public void WriteToInfoBox (string message)
		{

			Application.Invoke (delegate {
				textview1.Buffer.Text += message;
			});
		}

		public void SetRules (IEnumerable<OrderFilledRule> rules)
		{

			Gtk.Application.Invoke (
				(object sender, EventArgs e) => {
					RuleListStore.Clear ();



					foreach (OrderFilledRule rle in rules) {

						bool b = rle.IsActive;

						string boughtc = rle?.BoughtCurrency?.ToIssuerString () ?? "";

						string soldc = rle?.SoldCurrency?.ToIssuerString () ?? "";

						string payless = rle?.RefillMod?.Pay_Less.ToString () ?? "";

						string getmore = rle?.RefillMod?.Get_More.ToString () ?? "";


						RuleListStore.AppendValues (b, boughtc, soldc, payless, getmore);

					}

					this.treeview1.Model = RuleListStore;


				}
			);

		}


		public OrderFilledRule GetSelected ()
		{





			TreeSelection ts = this.treeview1.Selection;

			if (ts == null) {

				return null;
			}




			if (ts.GetSelected (out TreeModel tm, out TreeIter ti)) {

				// zero is the bool
				object b = tm.GetValue (ti, 1);
				object s = tm.GetValue (ti, 2);
				object pl = tm.GetValue (ti, 3);
				object gm = tm.GetValue (ti, 4);

				return RuleManagerObj.RetreiveFromValues ((string)b, (string)s, (string)pl, (string)gm);
			}



			return null;

		}



		public void DoLogicClicked ()
		{



			NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();
			if (ni == null) {

				MessageDialog.ShowMessage ("Network Warning", "Could not connect to network");
				return;
			}
			RippleWallet rw = walletswitchwidget2.GetRippleWallet ();
			if (rw == null) {

				MessageDialog.ShowMessage ("No wallet Selected");
				return;
			}

			bool ShouldContinue = LeIceSense.LastDitchAttempt (rw, LicenseType.AUTOMATIC);
			if (!ShouldContinue) {
				return;
			}

			Robotics robot = new Robotics (this.RuleManagerObj);

			Int32? strt = this.ledgerconstraintswidget1.GetStartFromLedger ();
			Int32? endStr = this.ledgerconstraintswidget1.GetEndLedger ();

			int? lim = this.ledgerconstraintswidget1.GetLimit ();

			Tuple<Int32?, IEnumerable<AutomatedOrder>> tuple = robot.DoLogic (rw, ni, strt, endStr, lim);


			if (tuple?.Item1 == null) {
				MessageDialog.ShowMessage ("No filled", "Filled orders error");
				return;
			}

			if (tuple.Item2 == null) {
				MessageDialog.ShowMessage ("No filled", "Filled orders array is null");
				return;
			}

			if (!tuple.Item2.Any ()) {
				MessageDialog.ShowMessage ("No filled", "There are no new filled orders\n");
				return;
			}

			Application.Invoke (delegate {
				this.ledgerconstraintswidget1.SetLastKnownLedger (tuple.Item1.ToString ()); //this.label9.Text = 



				LicenseType licenseT = Util.LicenseType.MARKETBOT;
				if (LeIceSense.IsLicenseExempt (tuple.Item2.ElementAt (0).taker_gets) || LeIceSense.IsLicenseExempt (tuple.Item2.ElementAt (0).taker_pays)) {
					licenseT = LicenseType.NONE;
				}

				OrderSubmitWindow win = new OrderSubmitWindow (rw, licenseT);
				win.SetOrders (tuple.Item2);



			});

		}

		private CancellationTokenSource tokenSource = null;
		//private CancellationToken token = this.tokenSource.Token;
		public void AutomateClicked ()
		{
			//if (token != null) {
			// TODO
			//}

			this.tokenSource = new CancellationTokenSource ();

			CancellationToken token = tokenSource.Token;

			try {

				this.SetIsRunningUI (true);

				RippleWallet rw = walletswitchwidget2.GetRippleWallet ();
				if (rw == null) {
					MessageDialog.ShowMessage ("No wallet Selected");
					//shouldContinue = false;

					return;
				}

				NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();
				if (ni == null) {

					MessageDialog.ShowMessage ("Network Warning", "Could not connect to network");
					//shouldContinue = false;
					return;
				}

				LicenseType licenseT = Util.LicenseType.AUTOMATIC;

				bool should = LeIceSense.LastDitchAttempt (rw, licenseT);
				if (!should) {
					//shouldContinue = false;
					return;
				}

				bool cont = false;
				ManualResetEvent manualResetEvent = new ManualResetEvent (false);
				manualResetEvent.Reset ();

				Gtk.Application.Invoke (
					delegate {
						// TODO more explicit warning

						progressbar1.Pulse ();
						cont = AreYouSure.AskQuestion ("Warning !!!", "<markup><span foreground=\"red\"><big><b>WARNING!</b></big></span> : This <b>TRADING BOT</b> will execute orders automatically for account <b>" + rw.GetStoredReceiveAddress () + "</b></markup>");
						progressbar1.Pulse ();
						manualResetEvent.Set ();


					}
				);

				manualResetEvent.WaitOne ();
				if (!cont) {
					//shouldContinue = false;
					return;
				}

				OrderSubmitter orderSubmitter = new OrderSubmitter ();
				orderSubmitter.OrderSubmitted += (object sender, OrderSubmittedEventArgs e) => {
					StringBuilder stringBuilder = new StringBuilder ();

					if (e.success) {
						stringBuilder.Append ("Submitted Order Successfully ");
						stringBuilder.Append ((string)(e?.rippleOfferTransaction?.hash ?? ""));




					} else {
						stringBuilder.Append ("Failed to submit order ");
						stringBuilder.Append ((string)(e?.rippleOfferTransaction?.hash ?? ""));

					}

					stringBuilder.AppendLine ();

					Application.Invoke (delegate {

						progressbar1.Pulse ();
					});

					this.WriteToInfoBox (stringBuilder.ToString ());
				};




				Application.Invoke (delegate {

					progressbar1.Pulse ();
				});
				this.WriteToInfoBox ("Running automation script for address " + rw.GetStoredReceiveAddress () + "\n");
				Robotics robot = new Robotics (this.RuleManagerObj);

				Int32? strt = this.ledgerconstraintswidget1.GetStartFromLedger ();
				Int32? endStr = this.ledgerconstraintswidget1.GetEndLedger ();

				int? lim = this.ledgerconstraintswidget1.GetLimit ();

				bool success = false;
				do {

					Application.Invoke (delegate {
						progressbar1.Pulse ();

					});

					this.WriteToInfoBox ("Polling data for " + (string)(rw?.GetStoredReceiveAddress () ?? "null") + "\n");
					this.WriteToInfoBox ("Starting from ledger " + (string)(strt?.ToString () ?? "null") + "\n");
					Tuple<Int32?, IEnumerable<AutomatedOrder>> tuple = robot.DoLogic (rw, ni, strt, endStr, lim);
					if (tuple == null) {
						MessageDialog.ShowMessage ("Automate : DoLogic tuple == null");
						break;
					}

					Application.Invoke (delegate {
						this.ledgerconstraintswidget1.SetLastKnownLedger (tuple.Item1.ToString ());
						progressbar1.Pulse ();
					});

					strt = tuple.Item1 + 1;
					if (endStr != null) {
						if (strt > endStr) {
							// TODO

							string maxMessage = "Maximum ledger " + endStr + " reached \n";
							this.WriteToInfoBox (maxMessage);
							break;
						}
					}

					IEnumerable<AutomatedOrder> orders = tuple.Item2;
					if (orders == null || !orders.Any ()) {

						int seconds = 60;

						string infoMessage = "Sleeping for " + seconds + " seconds \n";

#if DEBUG
						if (DebugIhildaWallet.FilledRuleManagementWindow) {
							Logging.WriteLog (infoMessage);
						}
#endif

						this.WriteToInfoBox (infoMessage);

						for (int sec = 0; sec < seconds; sec++) {
							//for (int i = 0; i < 4; i++) {
								if (token.IsCancellationRequested) {
									return;
								}
								//Thread.Sleep (250);

								token.WaitHandle.WaitOne (1000);
								//Task.Delay (250).Wait ();
								Application.Invoke (delegate {
									progressbar1.Pulse ();

								});
							//}


						}
						//success = true;
						continue;
					}

					int numb = orders.Count ();
					string submitMessage = "Submitting " + numb.ToString () + " orders\n";
					this.WriteToInfoBox (submitMessage);
					Application.Invoke (delegate {

						progressbar1.Pulse ();
					});
					Tuple<bool, IEnumerable<OrderSubmittedEventArgs>> tupleResp = orderSubmitter.SubmitOrders (orders, rw, ni);
					Application.Invoke (delegate {

						progressbar1.Pulse ();
					});

					if (tupleResp == null) {
						// TODO. probably unreachable code
					}
					success = tupleResp.Item1;
					if (!success) {
						string errMess = "Error submitting orders";
						this.WriteToInfoBox (errMess);
						//shouldContinue = false;
						//break;
						return;
					}

					string successMessage = "Orders submitted successfully\n";
					this.WriteToInfoBox (successMessage);
					Application.Invoke (delegate {

						progressbar1.Pulse ();
					});

				} while (!token.IsCancellationRequested);

			} catch (Exception e) {
				this.WriteToInfoBox (e.Message);
			} finally {
				//tokenSource.Dispose ();
				this.tokenSource = null;
				this.SetIsRunningUI (false);

				Application.Invoke (delegate {
					progressbar1.Fraction = 0;

				});
			}

			string message = "Automation thread has stopped\n";
			MessageDialog.ShowMessage ("Automation stopped", message);
		}






		/*
		private RippleWallet _rippleWallet {
			get;
			set;
		}
		*/

		private Gtk.ListStore RuleListStore {
			get;
			set;
		}

		RuleManager RuleManagerObj {
			get;
			set;
		}


	}
}
