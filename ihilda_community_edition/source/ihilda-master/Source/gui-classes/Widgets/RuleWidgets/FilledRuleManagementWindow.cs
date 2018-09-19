using System;
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

			this.label6.UseMarkup = true;

			walletswitchwidget2.WalletChangedEvent += (object source, WalletChangedEventArgs eventArgs) => {

				RippleWallet rippleWallet = eventArgs.GetRippleWallet ();
				this.SetRippleWallet (rippleWallet);
			};

			walletswitchwidget2.SetRippleWallet (rw);

			this.RuleListStore = 
				new Gtk.ListStore ( 
				                   typeof (bool), // active
				                   typeof (string), // bought
				                   typeof (string), // sold
				                   typeof (string), // mark
				                   typeof (string), // mark as
				                   typeof (string), // pay less
				                   typeof (string), // get more
				                   typeof (string), // exponential pay less 
				                   typeof (string), // exponential get more
				                   typeof (string)); // speculate
			
			this.SentimentStore = new Gtk.ListStore (typeof (string), typeof (string));

			//TextHighlighter.Highlightcolor = TextHighlighter.GREEN;
			//string coloredAdd = TextHighlighter.Highlight (rw.GetStoredReceiveAddress());
			//label1.Markup = "Rules for " + coloredAdd;


			CellRendererToggle toggleCell = new CellRendererToggle {
				Radio = false
			};


			toggleCell.Toggled += (object o, ToggledArgs args) => {


				if (RuleListStore == null) {
					return;
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

			CellRendererText cellRendererText = new CellRendererText ();

			CellRendererCombo cellRendererCombo = new CellRendererCombo ();

			ListStore comboModel = new ListStore (typeof (string));

			SentimentRatingEnum [] sentimentEnums = (SentimentRatingEnum [])Enum.GetValues (typeof (SentimentRatingEnum));

			foreach (SentimentRatingEnum sent in sentimentEnums) {
				comboModel.AppendValues (sent.ToString ());
			}

			cellRendererCombo.Editable = true;
			cellRendererCombo.Edited += (object o, EditedArgs args) => {
				//args.NewText;

				Sentiment sentiment = GetSelectedSentiment ();


				sentiment.Rating = ((SentimentRatingEnum)Enum.Parse (typeof (SentimentRatingEnum), args.NewText)).ToString();
				//SentimentManagerObject.RemoveSentiment(sentiment)



				this.SentimentManagerObject.SaveSentiments ();
				SetSentiments (this.SentimentManagerObject.SentimentList);

			};
			cellRendererCombo.Model = comboModel;
			cellRendererCombo.TextColumn = 0;
			//cellRendererCombo.
			cellRendererCombo.HasEntry = false;
			//cellRendererCombo.Mode = CellRendererMode.Editable;
			//cellRendererCombo.Sensitive = true;

			this.treeview1.AppendColumn ("Active", toggleCell, "active", 0);
			this.treeview1.AppendColumn ("Bought", cellRendererText, "text", 1);
			this.treeview1.AppendColumn ("Sold", cellRendererText, "text", 2);
			this.treeview1.AppendColumn ("Marking", cellRendererText, "markup", 3);
			this.treeview1.AppendColumn ("Mark As", cellRendererText, "markup", 4);
			this.treeview1.AppendColumn ("Pay Less", cellRendererText, "text", 5);
			this.treeview1.AppendColumn ("Get More", cellRendererText, "text", 6);
			this.treeview1.AppendColumn ("Exp Pay Less", cellRendererText, "text", 7);
			this.treeview1.AppendColumn ("Exp Get More", cellRendererText, "text", 8);
			this.treeview1.AppendColumn ("Speculate", cellRendererText, "text", 9);

			this.sentimenttreeview.AppendColumn ("Asset", cellRendererText, "text", 0);
			this.sentimenttreeview.AppendColumn ("Sentiment", cellRendererCombo, "text", 1);
			//this.sentimenttreeview.


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
				
				var v = this.GetSelectedRule ();
				this.RuleManagerObj.RemoveRule (v);
				this.RuleManagerObj.SaveRules ();
				SetRules (this.RuleManagerObj.RulesList);
			};

			this.editRulebutton.Clicked += (object sender, EventArgs e) => {
				OrderFilledRule v = this.GetSelectedRule ();

				OrderFilledRule rule = RuleCreateDialog.DoDialog (v);

				if (rule == null) {
					return;
				}

				this.RuleManagerObj.RemoveRule (v);
				this.RuleManagerObj.AddRule (rule);
				this.RuleManagerObj.SaveRules ();
				SetRules (this.RuleManagerObj.RulesList);
			};


			this.removesentimentbutton.Clicked += (object sender, EventArgs e) => {

			};

			this.addsentimentbutton.Clicked += (object sender, EventArgs e) => {
				Sentiment sentiment = SentementCreateDialog.DoDialog ();

				if (sentiment == null) {
					return;
				}

				this.SentimentManagerObject.AddSentiment (sentiment);
				this.SentimentManagerObject.SaveSentiments ();
				SetSentiments (this.SentimentManagerObject.SentimentList);
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
					return;
				}

				tokenSource.Cancel ();
				tokenSource = null;
			};

			//this.exitbutton.Clicked += (object sender, EventArgs e) => this.Destroy();





		}

		private void SetIsRunningUI (bool isRunning)
		{
			string message = "<b>Automation Status </b>: " + (String)(isRunning ? "<span fgcolor=\"green\">Running</span>" : "<span fgcolor=\"red\">Stopped</span>");
			Application.Invoke (delegate {
				walletswitchwidget2.Sensitive = !isRunning;
				label4.Markup = message;
			});

		}


		private void SetRippleWallet (RippleWallet rw)
		{
			Task.Run (delegate {


				string account = rw.GetStoredReceiveAddress ();
				RuleManagerObj = new RuleManager (account);
				RuleManagerObj.LoadRules ();

				SentimentManagerObject = new SentimentManager (account);
				SentimentManagerObject.LoadSentiments ();

				this.ledgerconstraintswidget1.SetLastKnownLedger (this.RuleManagerObj.LastKnownLedger.ToString ());

				SetRules (this.RuleManagerObj.RulesList);
				SetSentiments (this.SentimentManagerObject.SentimentList);

			});

		}

		public void WriteToInfoBox (string message)
		{

			Application.Invoke (delegate {
				label6.Markup = label6.Text + message;
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

						string mark = rle?.Mark ?? "";

						string markas = rle?.MarkAs ?? "";

						string payless = rle?.RefillMod?.Pay_Less.ToString () ?? "";

						string getmore = rle?.RefillMod?.Get_More.ToString () ?? "";

						string exppayless = rle?.RefillMod?.Exp_Pay_Less.ToString () ?? "";

						string expgetmore = rle?.RefillMod?.Exp_Pay_Less.ToString () ?? "";

						string spec = rle?.RefillMod?.Speculate.ToString () ?? "";

						RuleListStore.AppendValues (b, boughtc, soldc, mark, markas, payless, getmore, exppayless, expgetmore, spec);

					}

					this.treeview1.Model = RuleListStore;


				}
			);

		}

		public void SetSentiments (IEnumerable<Sentiment> sentiments)
		{
			Gtk.Application.Invoke (
				(object sender, EventArgs e) => {
					SentimentStore.Clear ();


					/*
					ListStore comboModel = new ListStore (typeof (string));

					SentimentRating [] sentimentEnums = (SentimentRating [])Enum.GetValues (typeof (SentimentRating));

					foreach (SentimentRating sent in sentimentEnums) {
						comboModel.AppendValues (sent.ToString ());
					}
					*/




					foreach (Sentiment sent in sentiments) {






						string m = sent?.Match?.ToString () ?? "";
						string s = sent?.Rating.ToString () ?? "";

						SentimentStore.AppendValues (m, s);

					}

					this.sentimenttreeview.Model = SentimentStore;


				}
			);
		}

		public Sentiment GetSelectedSentiment ()
		{
			TreeSelection treeSelection = sentimenttreeview.Selection;

			if (treeSelection == null) {
				return null;
			}

			if (treeSelection.GetSelected (out TreeModel treeModel, out TreeIter treeIter)) {
				object o = treeModel.GetValue (treeIter, 0);

				return ParseObject (o);
			}

			return null;
		}

		private Sentiment ParseObject (object o)
		{
			if (o == null) {
				return null;
			}

			if (!(o is String val)) {
				return null;
			}

			return SentimentManagerObject.LookUpSentiment (val);

		}


		public OrderFilledRule GetSelectedRule ()
		{

			TreeSelection ts = this.treeview1.Selection;

			if (ts == null) {

				return null;
			}

			if (ts.GetSelected (out TreeModel tm, out TreeIter ti)) {

				// zero is the bool
				object bought = tm.GetValue (ti, 1); // selected
				object sold = tm.GetValue (ti, 2); // 
				object mark = tm.GetValue (ti, 3);
				object markas = tm.GetValue (ti, 4);
				object payless = tm.GetValue (ti, 5);
				object getmore = tm.GetValue (ti, 6);
				object exppayless = tm.GetValue (ti, 7);
				object exgetmore = tm.GetValue (ti, 8);
				object speculate = tm.GetValue (ti, 9);

				return RuleManagerObj.RetreiveFromValues (
					(string)bought, 
					(string)sold, 
					(string)mark, 
					(string)markas, 
					(string)payless, 
					(string)getmore,
					(string)exppayless,
					(string)exgetmore,
					(string)speculate);
			}


			return null;

		}

					                  
				                  
			



		public void DoLogicClicked ()
		{

			string method_sig = clsstr + nameof (DoLogicClicked) + DebugRippleLibSharp.both_parentheses;

			if (this.tokenSource != null) {
				WriteToInfoBox ("A rule script is already running\n");
				return;
			}

			try {

				this.tokenSource = new CancellationTokenSource ();

				CancellationToken token = tokenSource.Token;
				token.ThrowIfCancellationRequested ();

				this.SetIsRunningUI (true);

				NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();
				if (ni == null) {

					MessageDialog.ShowMessage ("Network Warning", "Could not connect to network");
					return;
				}

				/*
				WriteToInfoBox ("");

				if (ni.IsConnected()) {
					WriteToInfoBox ("");
				}
				*/
				RippleWallet rw = walletswitchwidget2.GetRippleWallet ();
				if (rw == null) {
					string messg = "No wallet Selected";
					MessageDialog.ShowMessage (messg);
					WriteToInfoBox (messg);
					return;
				}

				bool ShouldContinue = LeIceSense.DoTrialDialog (rw, LicenseType.MARKETBOT);
				//bool ShouldContinue = LeIceSense.LastDitchAttempt (rw, LicenseType.AUTOMATIC);
				if (!ShouldContinue) {
					WriteToInfoBox ("Stopping");
					return;
				}

				Robotics robot = new Robotics (this.RuleManagerObj);

				Int32? strt = this.ledgerconstraintswidget1.GetStartFromLedger ();

				//WriteToInfoBox ("starting at ledger " + strt ?? "null");

				Int32? endStr = this.ledgerconstraintswidget1.GetEndLedger ();

				int? lim = this.ledgerconstraintswidget1.GetLimit ();


				this.WriteToInfoBox ("Polling data for " + (string)(rw?.GetStoredReceiveAddress () ?? "null") + "\n");
				this.WriteToInfoBox ("Starting from ledger " + (string)(strt?.ToString () ?? "null") + "\n");
				//this.WriteToInfoBox ("");



				Tuple<Int32?, IEnumerable<AutomatedOrder>> tuple = robot.DoLogic (rw, ni, strt, endStr, lim);


				string title = "No filled";
				string message = null;
				if (tuple == null) {
					message = "Do logic returned null";
					MessageDialog.ShowMessage (title, message);
					WriteToInfoBox (message);
				}

				if (tuple?.Item1 == null) {
					message = "Filled orders error";
					MessageDialog.ShowMessage (title, message);
					WriteToInfoBox (message);
					return;
				}

				if (tuple.Item2 == null) {
					message = "Filled orders array is null";
					MessageDialog.ShowMessage (title, message);
					WriteToInfoBox (message);
					return;
				}

				if (!tuple.Item2.Any ()) {
					message = "There are no new filled orders\n";
					MessageDialog.ShowMessage (title, message);
					WriteToInfoBox (message);
					return;
				}

				int num = tuple.Item2.Count ();

				message = num + " suggested orders\n";
				WriteToInfoBox (message);

				Application.Invoke (delegate {
					this.ledgerconstraintswidget1.SetLastKnownLedger (tuple.Item1.ToString ()); //this.label9.Text = 



					LicenseType licenseT = Util.LicenseType.MARKETBOT;
					if (LeIceSense.IsLicenseExempt (tuple.Item2.ElementAt (0).taker_gets) || LeIceSense.IsLicenseExempt (tuple.Item2.ElementAt (0).taker_pays)) {
						licenseT = LicenseType.NONE;
					}

					OrderSubmitWindow win = new OrderSubmitWindow (rw, licenseT);
					win.SetOrders (tuple.Item2);



				});
			} catch (Exception e) {
#if DEBUG
				if (DebugIhildaWallet.FilledRuleManagementWindow) {
					Logging.ReportException (method_sig, e);
				}
#endif
			} finally {
				this.tokenSource = null;
				this.SetIsRunningUI (false);

				Application.Invoke (delegate {
					progressbar1.Fraction = 0;

				});
			}

		}

		private CancellationTokenSource tokenSource = null;
		//private CancellationToken token = this.tokenSource.Token;
		public void AutomateClicked ()
		{
			if (this.tokenSource != null) {
				WriteToInfoBox ("A rule script is already running\n");
				return;
			}

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
				orderSubmitter.OnOrderSubmitted += (object sender, OrderSubmittedEventArgs e) => {
					StringBuilder stringBuilder = new StringBuilder ();


					if (e.Success) {
						stringBuilder.Append ("Submitted Order Successfully ");
						stringBuilder.Append ((string)(e?.RippleOfferTransaction?.hash ?? ""));




					} else {
						stringBuilder.Append ("Failed to submit order ");
						stringBuilder.Append ((string)(e?.RippleOfferTransaction?.hash ?? ""));

					}

					stringBuilder.AppendLine ();

					Application.Invoke (
						delegate {

							progressbar1.Pulse ();
						}
					);

					this.WriteToInfoBox (stringBuilder.ToString ());
				};

				orderSubmitter.OnVerifyingTxBegin += (object sender, VerifyEventArgs e) => {
					StringBuilder stringBuilder = new StringBuilder ();

					stringBuilder.Append ("Verifying transaction ");
					stringBuilder.Append ((string)(e.RippleOfferTransaction.hash ?? ""));

					Application.Invoke (
						delegate {

							progressbar1.Pulse ();
						}
					);

					this.WriteToInfoBox (stringBuilder.ToString ());
						
				};


				orderSubmitter.OnVerifyingTxReturn += (object sender, VerifyEventArgs e) => {
					StringBuilder stringBuilder = new StringBuilder ();
					string messg = null;
					if (e.Success) {
						stringBuilder.Append ("Transaction ");
						stringBuilder.Append ((string)(e?.RippleOfferTransaction?.hash ?? ""));
						stringBuilder.Append ("Verified");


						TextHighlighter.Highlightcolor = TextHighlighter.GREEN;



					} else {
						stringBuilder.Append ("Failed to validate transaction ");
						stringBuilder.Append ((string)(e?.RippleOfferTransaction?.hash ?? ""));

						TextHighlighter.Highlightcolor = TextHighlighter.RED;

					}

					messg = TextHighlighter.Highlight (stringBuilder);

					Application.Invoke (
						delegate {

							progressbar1.Pulse ();
						}
					);

					this.WriteToInfoBox (messg);
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

				RippleIdentifier rippleSeedAddress = rw.GetDecryptedSeed ();

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
					Tuple<bool, IEnumerable<OrderSubmittedEventArgs>> tupleResp = orderSubmitter.SubmitOrders (orders, rw, rippleSeedAddress,  ni);
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
						break;
						//return;
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
			WriteToInfoBox (message);
		}






		/*
		private RippleWallet _rippleWallet {
			get;
			set;
		}
		*/

		private Gtk.ListStore SentimentStore {
			get;
			set;
		}

		private Gtk.ListStore RuleListStore {
			get;
			set;
		}

		RuleManager RuleManagerObj {
			get;
			set;
		}

		SentimentManager SentimentManagerObject {
			get;
			set;
		}


#if DEBUG
		private const string clsstr = nameof (FilledRuleManagementWindow) + DebugRippleLibSharp.colon;
#endif

	}
}

