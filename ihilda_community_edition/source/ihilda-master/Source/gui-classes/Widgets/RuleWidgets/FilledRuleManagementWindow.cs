using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Codeplex.Data;
using Gtk;
using IhildaWallet.Networking;
using IhildaWallet.Util;
using RippleLibSharp.Commands.Subscriptions;
using RippleLibSharp.Keys;
using RippleLibSharp.Network;
using RippleLibSharp.Util;

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

			this.SetIsRunningUI (false);

			if (walletswitchwidget1 == null) {
				walletswitchwidget1 = new WalletSwitchWidget ();
				walletswitchwidget1.Show ();

				hbox4.Add (walletswitchwidget1);
			}


			if (ledgerconstraintswidget1 == null) {

				ledgerconstraintswidget1 = new LedgerConstraintsWidget ();

				expander1.Add (ledgerconstraintswidget1);
				ledgerconstraintswidget1.Show ();
			}

			ledgerconstraintswidget1.HideForward ();

			//_rippleWallet = rw;

			this.label6.UseMarkup = true;


			walletswitchwidget1.WalletChangedEvent += (object source, WalletChangedEventArgs eventArgs) => {

				RippleWallet rippleWallet = eventArgs.GetRippleWallet ();
				this.SetRippleWallet (rippleWallet);
			};


			walletswitchwidget1.SetRippleWallet (rw);

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

			forcewakebutton.Clicked += (object sender, EventArgs e) => {
				WakeTokenSource?.Cancel ();
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
				//Task.Run ((System.Action)this.RuleManagerObj.SaveRules);

				Task.Run (
					delegate {
						this.RuleManagerObj.SaveRules ();
						SetRules (this.RuleManagerObj.RulesList);
					}
				);


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


				sentiment.Rating = ((SentimentRatingEnum)Enum.Parse (typeof (SentimentRatingEnum), args.NewText)).ToString ();
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
			this.sentimenttreeview.AppendColumn ("Sentiment", cellRendererCombo, "markup", 1);
			//this.sentimenttreeview.


			this.stopWhenConvenientCheckbutton.Clicked += (object sender, EventArgs e) => {
				this.StopWhenConvenient = stopWhenConvenientCheckbutton.Active;
			};
			this.StopWhenConvenient = stopWhenConvenientCheckbutton.Active;

			this.addbutton.Clicked += (object sender, EventArgs e) => {

				var walletSwitchWidget = this.walletswitchwidget1;

				RippleWallet wallet = walletSwitchWidget?.GetRippleWallet ();


				OrderFilledRule rule = RuleCreateDialog.DoDialog (wallet);

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

				var sentiment = GetSelectedSentiment ();
				if (sentiment == null) {

				}
				this.SentimentManagerObject.RemoveSentiment (sentiment);

				SetSentiments (this.SentimentManagerObject.SentimentList);
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


				if (this.tokenSource != null) {

					string srmessage = "A rule script is already running\n";
					WriteToInfoBox (srmessage);
					this.SetIsRunningUI (false);

#if DEBUG
					if (DebugIhildaWallet.FilledRuleManagementWindow) {
						//Logging.WriteLog (method_sig, srmessage);
					}
#endif

					MessageDialog.ShowMessage ("Automation thread is already running");
					return;
				}

				


				StopWhenConvenient = false;
				stopWhenConvenientCheckbutton.Active = false;
				stopWhenConvenientCheckbutton.Visible = true;

				Task.Run ((System.Action)AutomateClicked);

			};

			canselbutton.Clicked += (object sender, EventArgs e) => {
				if (tokenSource == null) {
					MessageDialog.ShowMessage ("Thread has already been stopped\n");
					return;
				}
				if (tokenSource.auto) {
					StringBuilder stringBuilder = new StringBuilder ();

					if (!ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"orchid\">");
					}
					stringBuilder.Append ("Warning :  ");
					stringBuilder.AppendLine ("Forcing automation to stop abruptly may cause automation to lose track of progress.");
					stringBuilder.AppendLine ("It's better to use the \"Stop When Convenient\" checkbox");
					stringBuilder.Append ("</span>");


					bool should = AreYouSure.AskQuestion ("Cancel", stringBuilder.ToString ());
					
					if (!should) {
						return;
					}
				}


				tokenSource?.Cancel ();
				tokenSource = null;
			};

			this.button1.Clicked += (object sender, EventArgs e) => {
				if (tokenSource != null) {
					MessageDialog.ShowMessage ("A thread is currently running\n");
					return;
				}

				this.tokenSource = new BotCancellTokenSource () {
					auto = false
				};

				CancellationToken token = tokenSource.Token;

				WriteToInfoBox ("Syncing Orders Cache.... \n");
				WalletSwitchWidget walletSwitchWidget = this.walletswitchwidget1;

				//progressbar1.Pulse ();

				RippleWallet wallet = walletSwitchWidget?.GetRippleWallet ();
				if (wallet == null) {
					MessageDialog.ShowMessage ("No wallet selected\n");
					return;
				}



				deleteordersbutton.Visible = false;
				dologicbutton.Visible = false;
				automatebutton.Visible = false;
				canselbutton.Visible = true;
				button1.Visible = false;

				//progressbar1.Pulse ();

				Task.Run (delegate {

					try {


						token.ThrowIfCancellationRequested ();


						this.SetIsRunningUI (true);


						AccountSequenceCache accountSequnceCache = AccountSequenceCache.GetCacheForAccount (this.walletswitchwidget1.GetRippleWallet ().GetStoredReceiveAddress ());

						Application.Invoke (delegate {
							progressbar1.Pulse ();

						});


						accountSequnceCache.OnOrderCacheEvent += (object sen, OrderCachedEventArgs ocev) => {

							Application.Invoke (delegate {
								progressbar1.Pulse ();

							});

							if (ocev.UIpump) {
								return;
							}

							TextHighlighter.Highlightcolor =
								ocev.GetSuccess ?
								(ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN) :
								TextHighlighter.RED;

							string message = TextHighlighter.Highlight (ocev.Message);
							WriteToInfoBox (message);

						};



						accountSequnceCache.SyncOrdersCache ( token);
						Application.Invoke (delegate {
							progressbar1.Pulse ();

						});

						WriteToInfoBox ("Finished Syncing orders cache \n");
					} catch (Exception ex) {
						this.WriteToInfoBox (ex.Message);
					} finally {
						this.tokenSource = null;
						this.SetIsRunningUI (false);

						Application.Invoke (delegate {
							progressbar1.Fraction = 0;
							deleteordersbutton.Visible = true;
							dologicbutton.Visible = true;
							automatebutton.Visible = true;
							canselbutton.Visible = false;
							button1.Visible = true;
						});

						string message = "Automation thread has stopped\n";
						//MessageDialog.ShowMessage ("Automation stopped", message);
						WriteToInfoBox (message);
					}
					/*
					Gtk.Application.Invoke (
						delegate {


						}
					);*/

				});



			};

			//this.exitbutton.Clicked += (object sender, EventArgs e) => this.Destroy();

			//this.button17

			this.deleteordersbutton.Clicked += (sender, e) => {

				string account = this.walletswitchwidget1?.GetRippleWallet ()?.GetStoredReceiveAddress ();
				if (account == null) {
					MessageDialog.ShowMessage (
						"Select an account",
						"No account has been selected for cache deletion"
					);
					return;
				}
				bool sure = AreYouSure.AskQuestion (
					"Delete cache",
					"Are you sure you want to delete the orders cache for account " +
					    account +
					    "?");

				if (!sure) {
					return;
				}
				AccountSequenceCache.DeleteSettingsFile (account);
			};


			this.savebutton.Clicked += (sender, e) => {

#if DEBUG
				string event_sig = clsstr + nameof (FilledRuleManagementWindow) + DebugRippleLibSharp.both_parentheses;
#endif

				//this.RuleManagerObj.


				string account = this.walletswitchwidget1?.GetRippleWallet ()?.GetStoredReceiveAddress ();
				if (account == null) {
					MessageDialog.ShowMessage (
						"Select an account",
						"Can not export rule list. No account has been selected."
					);
					return;
				}

				FileChooserDialog fcd = new FileChooserDialog ("Export Rules",
							this,
							FileChooserAction.Save,
							"Cancel", ResponseType.Cancel,
							"Save", ResponseType.Accept);


				if (fcd?.Run () == (int)ResponseType.Accept) {
#if DEBUG
					if (DebugIhildaWallet.FilledRuleManagementWindow) {
						Logging.WriteLog (event_sig + "user chose to export to file " + fcd.Filename + "\n");
					}
#endif

					RuleManager ruleManager = new RuleManager (account);

					ruleManager.SaveRules (fcd.Filename);

				}

				fcd?.Destroy ();

			};

			button101.Clicked += (object sender, EventArgs e) => {

				string account = this.walletswitchwidget1?.GetRippleWallet ()?.GetStoredReceiveAddress ();
				if (account == null) {
					MessageDialog.ShowMessage (
						"Select an account",
						"Can not import rule list. No account has been selected."
					);
					return;
				}

				RuleManager ruleManager = new RuleManager (account);
				ruleManager.LoadRules ();
				if (ruleManager.RulesList.Any ()) {
					bool sure = AreYouSure.AskQuestion (
						"Overwrite Rules",
						"Importing a rule list will overwrite current settings. Are you sure you want to continue?"
					);

					if (!sure) {
						return;
					}
				}



				FileChooserDialog fcd = new FileChooserDialog (
					"Import Rules",
					this,
					FileChooserAction.Open,
					"Cancel",
					ResponseType.Cancel,
					"Open",
					ResponseType.Accept);

				if (fcd?.Run () == (int)ResponseType.Accept) {
					string p = fcd?.Filename;
					if (p == null) {
						return;
					}

					ruleManager.SaveRules (p);

				}

			};

			savesleepbutton.Clicked += (object sender, EventArgs e) => {
				BotSleepSettings settings = ParseSleepSettings ();
				string address = walletswitchwidget1.GetRippleWallet ().GetStoredReceiveAddress ();

				if (address == null) {
					// todo
					return;
				}

				if (settings != null) {
					BotSleepSettings.Save (address, settings);
				}
			};

			this.DeleteEvent += OnDeleteEvent;

			
			this.delAllButton.Clicked += (object sender, EventArgs e) => {
				var sure = AreYouSure.AskQuestion ("Delete All", "<span fgcolor=\"red\">Are you sure you want to DELETE ALL WALLETS?</span>");
				if (sure) {
					this.RuleManagerObj.ClearRules ();
					this.SetRules (this.RuleManagerObj.RulesList);
				}
			};

		}

		public BotSleepSettings ParseSleepSettings ()
		{

			String parseError = "Parse Error";

			BotSleepSettings botSleepSettings = new BotSleepSettings ();

			string secondsStr = this.sleepsecentry.Text;
			string ledgerWaitStr = this.waitledgeentry.Text;

			bool hasSleep = this.sleepseccheckbutton.Active;
			bool hasLedge = this.sleepledgecheckbutton.Active;

			if (!string.IsNullOrWhiteSpace(secondsStr)) {
				bool validSeconds = UInt32.TryParse (secondsStr, out uint seconds);
				if (!validSeconds) {
					// TODO
					// 

					MessageDialog.ShowMessage (parseError, "Could not parse number of seconds");
					return null;
				}

				botSleepSettings.SleepSeconds = seconds;
			}

			if (!string.IsNullOrEmpty (ledgerWaitStr)) {

				bool validWait = UInt32.TryParse (ledgerWaitStr, out uint waitLedger);
				if (!validWait) {
					MessageDialog.ShowMessage (parseError, "Could not parse number of ledgers to wait for. ");
					return null;
				}

				botSleepSettings.LedgerWait = waitLedger;
			}

			botSleepSettings.HasSleepSeconds = hasSleep;
			botSleepSettings.HasLedgerWait = hasLedge;

			

			return botSleepSettings;

		}


		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{

			if (this.tokenSource != null) {
				bool sure = AreYouSure.AskQuestion ("Task running", "A task is currently running. Are you sure you want to close the window?");
				if (!sure) {
					a.RetVal = true;
					return;
				}
			}

			//base.OnDeleteEvent (a); // should this be called?
			this.Destroy ();
		}
		private void SetIsRunningUI (bool isRunning)
		{
			string message =
			"<span font-size=\"large\"><b>Automation Status </b>: "
				+
				(string)(isRunning ?
				    (ProgramVariables.darkmode ? "<span fgcolor=\"chartreuse\">Running</span>" : "<span fgcolor=\"green\">Running</span>") :
					(ProgramVariables.darkmode ? "<span fgcolor=\"#FFAABB\">Stopped</span>" : " <span fgcolor=\"red\">Stopped</span>"))
		    		+
		    		"</span>"
			;

			Application.Invoke (delegate {
				walletswitchwidget1.Sensitive = !isRunning;
				label4.Markup = message;
				deleteordersbutton.Visible = !isRunning;
				canselbutton.Visible = isRunning;
				automatebutton.Visible = !isRunning;
				dologicbutton.Visible = !isRunning;
				button1.Visible = !isRunning;
			});

		}


		private void SetRippleWallet (RippleWallet rw)
		{

			string account = rw.GetStoredReceiveAddress ();
			Task.Run (delegate {




				var ruleManager = new RuleManager (account);
				RuleManagerObj = ruleManager;
				ruleManager.LoadRules ();




				SetRules (ruleManager.RulesList);


			});

			Task.Run (
				delegate {
					SentimentManagerObject = new SentimentManager (account);
					SentimentManagerObject.LoadSentiments ();
					SetSentiments (this.SentimentManagerObject.SentimentList);
				}
			);

			Task.Run (delegate {
				WalletLedgerSave ledgerSave = WalletLedgerSave.LoadLedger (rw?.BotLedgerPath);

				this.ledgerconstraintswidget1.SetLastKnownLedger (ledgerSave.Ledger.ToString ());


			});

			Task.Run ( delegate {

				BotSleepSettings settings = BotSleepSettings.Load (account);

				if (settings == null) {
					return;
				}

				bool sec = settings.HasSleepSeconds ?? false;
				bool led = settings.HasLedgerWait ?? false;
				string slp = settings.SleepSeconds?.ToString () ?? "";
				string wt = settings.LedgerWait?.ToString () ?? "";

				Gtk.Application.Invoke (
					(sender, e) => {
						sleepseccheckbutton.Active = sec;
						sleepledgecheckbutton.Active = led;

						sleepsecentry.Text = slp;
						waitledgeentry.Text = wt;
					}
				);




			});
		}

		StringBuilder infoBoxBuffer = new StringBuilder ();
		List<string> lines = new List<string> ();
		public void WriteToInfoBox (string message)
		{
			if (message == null) {
				return;
			}

			string msg = message;



			Application.Invoke (delegate {


				lines.Add (msg);

				if (lines.Count > 1000) {
					lines.RemoveAt (0);
				}

				infoBoxBuffer.Clear ();
				foreach (String s in lines) {

					infoBoxBuffer.Append (s);
				}

				if (label6 != null) {
					//if (infoBoxBuffer.Length > 0) {
						label6.Markup = infoBoxBuffer.ToString ();
					//}
					
				}

				/*
				if (scrolledwindow1 != null) {
					scrolledwindow1.Vadjustment.Value = scrolledwindow1.Vadjustment.Upper;
				}
				*/
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
						string s = sent?.GetMarkupString () ?? "";

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
			return o != null && (o is String val) ? SentimentManagerObject.LookUpSentiment (val) : null;
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






		private long? previousStartLedger = null;
		public void DoLogicClicked ()
		{

#if DEBUG
			string method_sig = clsstr + nameof (DoLogicClicked) + DebugRippleLibSharp.both_parentheses;
#endif

			if (this.tokenSource != null) {
				WriteToInfoBox ("A rule script is already running\n");
				return;
			}

			try {

				this.tokenSource = new BotCancellTokenSource () { auto = false };

				CancellationToken token = tokenSource.Token;
				token.ThrowIfCancellationRequested ();

				this.SetIsRunningUI (true);

				NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();
				if (ni == null) {
					MessageDialog.ShowMessage ("Network Warning", "Could not connect to network");
					this.SetIsRunningUI (false);
					return;
				}


				RippleWallet rw = walletswitchwidget1.GetRippleWallet ();
				if (rw == null) {
					string messg = "No wallet Selected";
					MessageDialog.ShowMessage (messg);
					WriteToInfoBox (messg);
					this.SetIsRunningUI (false);
					return;
				}

				// Pro feature
				bool ShouldContinue = LeIceSense.DoTrialDialog (rw, LicenseType.MARKETBOT);


				//bool ShouldContinue = LeIceSense.LastDitchAttempt (rw, LicenseType.AUTOMATIC);

				if (!ShouldContinue) {
					// TODO print fee requirement
					WriteToInfoBox ("This requires ihilda pro \n");
					this.SetIsRunningUI (false);
					return;
				}

				Robotics robot = new Robotics (this.RuleManagerObj);

				long? strt = this.ledgerconstraintswidget1.GetStartFromLedger ();

				if (strt != null) {
					if (strt == previousStartLedger) {
						StringBuilder stringBuilder = new StringBuilder ();
						stringBuilder.Append ("You've supplied the same starting ledger twice consecutively. ");
						stringBuilder.Append ("Although this may have been intended it's a common human error. ");
						stringBuilder.Append ("Are you sure you'd like to continue?");
						bool GoOn = AreYouSure.AskQuestionNonGuiThread (
							"Same start ledger",
							stringBuilder.ToString ()
						);

						if (!GoOn) {
							this.SetIsRunningUI (false);
							return;
						}
					}

					previousStartLedger = strt;

				}

				long? endStr = this.ledgerconstraintswidget1.GetEndLedger ();

				int? lim = this.ledgerconstraintswidget1.GetLimit ();


				this.WriteToInfoBox ("Polling data for " + (rw?.GetStoredReceiveAddress () ?? "null") + "\n");

				WalletLedgerSave ledgerSave = WalletLedgerSave.LoadLedger (rw?.BotLedgerPath);
				uint? lastSavedLedger = ledgerSave?.Ledger;
				if (strt != null) {
					this.WriteToInfoBox ("\nStarting from ledger " + (strt?.ToString () ?? "null") + "\n");
				} else {


					this.WriteToInfoBox ("\nStarting ledger is null\n Using last known ledger " + (lastSavedLedger?.ToString() ?? "null") + "\n");
				}

				

				if ( lastSavedLedger == null || lastSavedLedger == 0) {
					bool b = AreYouSure.AskQuestionNonGuiThread (
						"Process entire transaction history?",
						"You haven't specified a starting ledger and no previous lastledger value has been saved. " +
						"This will cause the automation to process all transaction history and may not be what you intended" +
						"Should the script continue?"

					);

					if (!b) {

						this.SetIsRunningUI (false);
						return;
					}
				}

				robot.OnMessage += (object sender, MessageEventArgs e) => {
					this.WriteToInfoBox (e.Message);

					Gtk.Application.Invoke (delegate {

						progressbar1?.Pulse ();
					}
					);
				};

				var task = Task.Run (delegate {
					return robot?.DoLogic (rw, ni, strt, endStr, lim, token); ;
				}, token);


				while (!token.IsCancellationRequested && ShouldContinue && !task.IsCompleted && !task.IsCanceled && !task.IsFaulted) {

					task.Wait (250, token);
					progressbar1?.Pulse ();
				}

				if (!ShouldContinue) {
					tokenSource.Cancel ();
					return;
				}

				if (token.IsCancellationRequested) {
					this.SetIsRunningUI (false);
					return;
				}

				DoLogicResponse tuple = task?.Result;



				if (token.IsCancellationRequested || !ShouldContinue) {
					return;
				}
				

				string title = "No filled";
				string message = null;
				if (tuple == null) {
					message = "Do logic returned null\n";
					MessageDialog.ShowMessage (title, message);
					WriteToInfoBox (message);
				} else

				if (tuple.LastLedger == null) {
					message = "Filled orders error\n";
					MessageDialog.ShowMessage (title, message);
					WriteToInfoBox (message);
					this.SetIsRunningUI (false);
					return;
				} else
				

				if (tuple.FilledOrders == null || !tuple.FilledOrders.Any ()) {
					message = "There are no new filled orders\n";
					MessageDialog.ShowMessage (title, message);
					WriteToInfoBox (message);
					//return;
				} else {

					int num = tuple.FilledOrders.Count ();

					message = num + " suggested orders\n";
					WriteToInfoBox (message);
				}

				Application.Invoke (delegate {
					this.ledgerconstraintswidget1.SetLastKnownLedger (tuple.LastLedger.ToString ()); //this.label9.Text = 
				});


				if (tuple?.FilledOrders?.FirstOrDefault () == null) {
					this.SetIsRunningUI (false);
					return;
				}

				WriteToInfoBox ("Preparing orders and order submit window\n");


				var finalTask = Task.Run (delegate {

					OrderSubmitWindow win = null;
					using (ManualResetEvent manualResetEvent = new ManualResetEvent (false)) {
						manualResetEvent.Reset ();

						Application.Invoke (delegate {

							/*
							LicenseType licenseT = Util.LicenseType.MARKETBOT;
							if (LeIceSense.IsLicenseExempt (tuple.Item2.ElementAt (0).taker_gets) || LeIceSense.IsLicenseExempt (tuple.Item2.ElementAt (0).taker_pays)) {
							    licenseT = LicenseType.NONE;
							}
							*/

							win = new OrderSubmitWindow (rw, LicenseType.MARKETBOT) {
								Visible = false
							};

							manualResetEvent.Set ();
						});

						token.WaitHandle.WaitOne (100); // sleep a second
						manualResetEvent.WaitOne (1000 * 60 * 5);

						WaitHandle.WaitAny (new WaitHandle [] { manualResetEvent, token.WaitHandle });

						if (!token.IsCancellationRequested) {
							manualResetEvent.Reset ();
						} else {
							win?.Destroy ();
							win = null;
							this.SetIsRunningUI (false);
						}

						win.SetOrders (tuple.FilledOrders);

						Application.Invoke (delegate {
							win.Show ();
							manualResetEvent.Set ();
						});

						token.WaitHandle.WaitOne (100); // sleep a second
						manualResetEvent.WaitOne (1000 * 60 * 5);
						WaitHandle.WaitAny (new WaitHandle [] { manualResetEvent, token.WaitHandle });
					}
				});


				while (
					finalTask != null
					&& !finalTask.IsCanceled
					    && !finalTask.IsCompleted
					    && !finalTask.IsFaulted
					&& !token.IsCancellationRequested
				) {
					Application.Invoke (delegate {

						progressbar1.Pulse ();
					});


					try {
						WriteToInfoBox ("Processing may take some time");
						for (
							// declaration
							int seconds = 0;

							// conditional
							finalTask != null
							&& !finalTask.IsCanceled
					    		&& !finalTask.IsCompleted
					    		&& !finalTask.IsFaulted
							&& !token.IsCancellationRequested
							;
							// ireration
							seconds++

			    				) {
							finalTask.Wait (1000, token);
							WriteToInfoBox (".");
							Application.Invoke (delegate {

								progressbar1.Pulse ();
							});


						}

					} catch (Exception e) {

						throw e;

					} finally {

						Application.Invoke (delegate {

							progressbar1.Pulse ();
						});


						WriteToInfoBox ("\n");
					}

					// should not print if an exceptional event occured. 
					WriteToInfoBox ("Orders Processed\n");
				}

			} catch (TaskCanceledException cancelException) {
#if DEBUG
				if (DebugIhildaWallet.FilledRuleManagementWindow) {
					Logging.ReportException (method_sig, cancelException);
				}
#endif

				WriteToInfoBox ("Task cancelled\n");
				this.SetIsRunningUI (false);

			} catch (OperationCanceledException opCanException) {
#if DEBUG
				if (DebugIhildaWallet.FilledRuleManagementWindow) {
					Logging.ReportException (method_sig, opCanException);
				}
#endif

				WriteToInfoBox ("Operation cancelled\n");
				this.SetIsRunningUI (false);
			} catch (Exception e) {
#if DEBUG
				if (DebugIhildaWallet.FilledRuleManagementWindow) {
					Logging.ReportException (method_sig, e);
				}
#endif
			} finally {

				tokenSource?.Cancel ();
				this.tokenSource = null;
				this.SetIsRunningUI (false);

				Application.Invoke (delegate {
					progressbar1.Fraction = 0;

				});

				string message = "Automation thread has stopped\n";
				//MessageDialog.ShowMessage ("Automation stopped", message);
				WriteToInfoBox (message);
			}

		}


		private class BotCancellTokenSource : CancellationTokenSource
		{
			public bool auto = false;
		}


		private BotCancellTokenSource tokenSource = null;
		//private CancellationToken token = this.tokenSource.Token;
		public void AutomateClicked ()
		{


#if DEBUG
			string method_sig = nameof (AutomateClicked) + DebugRippleLibSharp.both_parentheses;

			if (DebugIhildaWallet.FilledRuleManagementWindow) {
				Logging.WriteLog (method_sig, DebugRippleLibSharp.beginn);
			}
#endif


			this.tokenSource = new BotCancellTokenSource () { auto = true };
			CancellationToken token = tokenSource.Token;

			RippleWallet rw = walletswitchwidget1.GetRippleWallet ();
			if (rw == null) {

				string nwmessage = "No wallet Selected\n";
				MessageDialog.ShowMessage (nwmessage);

#if DEBUG
				if (DebugIhildaWallet.FilledRuleManagementWindow) {
					Logging.WriteLog (nwmessage);
				}
#endif
				//shouldContinue = false;

				return;
			}



			NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();
			if (ni == null) {

				string netmessage = "Could not connect to network";
				MessageDialog.ShowMessage ("Network Warning", netmessage);

#if DEBUG
				if (DebugIhildaWallet.FilledRuleManagementWindow) {
					Logging.WriteLog (method_sig, netmessage);
				}
#endif

				return;
			}

			LicenseType licenseT = LicenseType.AUTOMATIC;

			bool should = LeIceSense.LastDitchAttempt (rw, licenseT);
			if (!should) {

				return;
			}

			bool cont = false;
			using (ManualResetEvent manualResetEvent = new ManualResetEvent (false)) {
				manualResetEvent.Reset ();

				Gtk.Application.Invoke (
				    delegate {
			    // TODO more explicit warning

			    progressbar1?.Pulse ();
					    cont = AreYouSure.AskQuestion (
				"Warning !!!",
				(string)(ProgramVariables.darkmode ?
				"<markup><span foreground=\"red\"><big><b>WARNING!</b></big></span> : This <b>TRADING BOT</b> will execute orders automatically for account <b>" :
				"<markup><span foreground=\"#FFAABB\"><big><b>WARNING!</b></big></span> : This <b>TRADING BOT</b> will execute orders automatically for account <b>" )

				+ rw.GetStoredReceiveAddress ()
				+ "</b></markup>");
					    progressbar1?.Pulse ();
					    manualResetEvent.Set ();


				    }
				);


				manualResetEvent.WaitOne ();
			}

			if (!cont) {
				//shouldContinue = false;
				return;
			}


			SoundSettings settings = SoundSettings.LoadSoundSettings ();

			try {

				this.SetIsRunningUI (true);


				OrderSubmitter orderSubmitter = new OrderSubmitter ();

				orderSubmitter.OnFeeSleep += (object sender, FeeSleepEventArgs e) => {
					switch (e.State) {

					case FeeSleepState.Begin:
						this.WriteToInfoBox (
							"Fee " +
							(string)(e?.FeeAndLastLedger?.Fee.ToString () ?? "null") +
							" is too high, waiting on lower fee"
			    			);
						break;

					case FeeSleepState.PumpUI:
						this.WriteToInfoBox (".");
						break;

					case FeeSleepState.Wake:
						this.WriteToInfoBox ("\n");
						break;

					}

					Application.Invoke (
						delegate {

							progressbar1?.Pulse ();

						}
					);
				};

				orderSubmitter.OnOrderSubmitted += (object sender, OrderSubmittedEventArgs e) => {

					StringBuilder stringBuilder = new StringBuilder ();

					stringBuilder.AppendLine ();
					stringBuilder.AppendLine (e.Success ? "Submitted Order Successfully " : "Failed to submit order ");

					stringBuilder.Append ("Sequence : ");
					stringBuilder.AppendLine (e?.RippleOfferTransaction?.Sequence.ToString() ?? "");

					stringBuilder.Append ("Hash : ");
					stringBuilder.AppendLine (e?.RippleOfferTransaction?.hash ?? "");

					stringBuilder.AppendLine (e.Message);


					Application.Invoke (
						delegate {

							progressbar1?.Pulse ();
						}
					);

					this.WriteToInfoBox (stringBuilder.ToString ());
				};

				orderSubmitter.OnVerifyingTxBegin += (object sender, VerifyEventArgs e) => {

					StringBuilder stringBuilder = new StringBuilder ();

					stringBuilder.AppendLine ("\nVerifying transaction ");
					stringBuilder.Append ("Sequence : ");
					stringBuilder.AppendLine (e?.RippleOfferTransaction?.Sequence.ToString () ?? "");

					stringBuilder.Append ("Hash : ");
					stringBuilder.AppendLine (e?.RippleOfferTransaction?.hash ?? "");
					stringBuilder.AppendLine ();




					Application.Invoke (
						delegate {

							progressbar1?.Pulse ();
						}
					);

					this.WriteToInfoBox (stringBuilder.ToString ());

				};


				orderSubmitter.OnVerifyingTxReturn += (object sender, VerifyEventArgs e) => {
					StringBuilder stringBuilder = new StringBuilder ();
					string messg = null;
					if (e.Success) {
						stringBuilder.Append ("Transaction ");
						stringBuilder.Append (e?.RippleOfferTransaction?.hash ?? "");
						stringBuilder.AppendLine (" Verified");


						TextHighlighter.Highlightcolor = ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN;




					} else {
						stringBuilder.Append ("Failed to validate transaction ");
						stringBuilder.AppendLine (e?.RippleOfferTransaction?.hash ?? "");

						TextHighlighter.Highlightcolor = TextHighlighter.RED;

					}

					//stringBuilder.AppendLine ();

					messg = TextHighlighter.Highlight (stringBuilder);

					Application.Invoke (
						delegate {

							progressbar1?.Pulse ();
						}
					);

					this.WriteToInfoBox (messg);
				};

				orderSubmitter.OnVerifyTxMessage += (object sender, VerifyEventArgs e) => {
					//StringBuilder stringBuilder = new StringBuilder ();
					//stringBuilder.Append ();

					WriteToInfoBox (e.Message);
				};


				Application.Invoke (delegate {

					progressbar1?.Pulse ();
				});

				this.WriteToInfoBox (
					"Running automation script for address " +
					(rw?.GetStoredReceiveAddress () ?? "") +
					"\n"
				);

				Robotics robot = new Robotics (this.RuleManagerObj);

				robot.OnMessage += (object sender, MessageEventArgs e) => {
					this.WriteToInfoBox (e?.Message);
				};

				Int64? strt = this.ledgerconstraintswidget1.GetStartFromLedger ();

				if (strt != null && strt > 0) {
					if (strt == previousStartLedger) {
						StringBuilder stringBuilder = new StringBuilder ();
						stringBuilder.Append ("You've supplied the same starting ledger twice consecutively. ");
						stringBuilder.Append ("Although this may have been intended it's a common human error. ");
						stringBuilder.Append ("Are you sure you'd like to continue?");
						bool GoOn = AreYouSure.AskQuestionNonGuiThread (
							"Same start ledger",
							stringBuilder.ToString ()
						);

						if (!GoOn) {
							return;
						}
					}

					previousStartLedger = (int?)strt;

				}




				long? endStr = this.ledgerconstraintswidget1.GetEndLedger ();

				int? lim = this.ledgerconstraintswidget1.GetLimit ();

				bool success = false;

				RippleIdentifier rippleSeedAddress = rw.GetDecryptedSeed ();
				while (rippleSeedAddress?.GetHumanReadableIdentifier () == null && !token.IsCancellationRequested && !StopWhenConvenient) {
					bool shou = AreYouSure.AskQuestionNonGuiThread (
					"Invalid password",
					"Unable to decrypt seed. Invalid password.\nWould you like to try again?"
					);

					if (!shou) {
						return;
					}

					rippleSeedAddress = rw.GetDecryptedSeed ();
				}


				BotSleepSettings botSleepSettings = null;


				while (!token.IsCancellationRequested && !StopWhenConvenient) {
					ManualResetEvent manualResetEvent = new ManualResetEvent (false);
					manualResetEvent.Reset ();

					Gtk.Application.Invoke ( delegate {

						botSleepSettings = ParseSleepSettings ();
						manualResetEvent.Set ();
					} );




					WaitHandle.WaitAny ( new WaitHandle [] { manualResetEvent, token.WaitHandle }, 5000); // after 5 seconds it's probably hung and sleep time is not a critical error

					if (endStr != null) {
						if (strt > endStr) {
							// TODO

							string maxMessage = "Maximum ledger " + endStr + " reached \n";



							this.WriteToInfoBox (maxMessage);
							break;
						}
					}

					Application.Invoke (delegate {
						progressbar1?.Pulse ();

					});

					this.WriteToInfoBox (
						"Polling data for "
						+ (rw?.GetStoredReceiveAddress () ?? "null")
						+ "\n");


					WalletLedgerSave ledgersave = WalletLedgerSave.LoadLedger (rw.BotLedgerPath);



					uint? last = ledgersave?.Ledger;
					if (strt != null) {
						this.WriteToInfoBox ("\nStarting from ledger " + (strt?.ToString () ?? "null") + "\n");
					} else {



						if (last == null || last == 0) {
							bool b = AreYouSure.AskQuestionNonGuiThread (
								"Process entire transaction history?",
								"You haven't specified a starting ledger and no previous lastledger value has been saved. " +
								"This will cause the automation to process all transaction history and may not be what you intended" +
								"Should the script continue?"

							);

							if (!b) {
								return;
							}
						}

						this.WriteToInfoBox ("\nStarting ledger is null\n Using last known ledger " + last + "\n");
					}






					
		    			
					DoLogicResponse tuple = null;

					do {
						tuple = robot.DoLogic (rw, ni, strt, endStr, lim, token);

						Application.Invoke (delegate {

							progressbar1.Pulse ();
						});


					} while (!token.IsCancellationRequested && tuple == null && !StopWhenConvenient);



					if (token.IsCancellationRequested || StopWhenConvenient) {
						return;
					}


					if (tuple == null) {


						if (settings != null && settings.HasOnAutomateFail && settings.OnAutomateFail != null) {

							Task.Run (delegate {

								SoundPlayer player =
								new SoundPlayer (settings.OnAutomateFail);
								player.Load ();
								player.Play ();
							});

						}


#if DEBUG
						MessageDialog.ShowMessage ("Automate : DoLogic tuple == null");
#endif



						break;
					}

					Application.Invoke (
					delegate {
						
						this.ledgerconstraintswidget1.SetLastKnownLedger (tuple?.LastLedger?.ToString ());
						progressbar1.Pulse ();
					}
					);

					strt = tuple.LastLedger + 1;


					IEnumerable<AutomatedOrder> orders = tuple.FilledOrders;
					if (orders == null || !orders.Any ()) {
						if (botSleepSettings == null || (botSleepSettings.HasSleepSeconds != true && botSleepSettings.HasLedgerWait != true)) {

							uint seconds = 60;

							DoSleep (seconds, token);

						} else if (botSleepSettings.HasLedgerWait == true && botSleepSettings.HasSleepSeconds == true) {
							uint seconds = botSleepSettings.SleepSeconds ?? 60;


							var tempToken = new CancellationTokenSource ();
							var token2 = tempToken.Token;
							var task = Task.Run ( 
								delegate {
									var w = botSleepSettings.LedgerWait ?? 10;
									DoLedgerWait (w, token2);
								}

							, token);

							task.Wait ((int)seconds * 1000, token);
							tempToken.Cancel ();


						} else if (botSleepSettings.HasLedgerWait == true) {

							uint w = botSleepSettings.LedgerWait ?? 10;
							DoLedgerWait (w, token);

						} else {

							uint seconds = botSleepSettings.SleepSeconds ?? 60;

							DoSleep (seconds, token);
						}
						//success = true;
						continue;
					}

					int numb = orders.Count ();
					string submitMessage = "Submitting " + numb.ToString () + " orders\n";
					this.WriteToInfoBox (submitMessage);
					Application.Invoke (delegate {

						progressbar1?.Pulse ();
					});

			
					MultipleOrdersSubmitResponse responses = null;

					bool parallelSubmit = ProgramVariables.parallelVerify;

					if (!parallelSubmit) {
						responses = orderSubmitter.SubmitOrders (
								orders, rw, rippleSeedAddress, ni, token
							);
					} else {
						responses = orderSubmitter.SubmitOrdersParallelImproved (
								orders, rw, rippleSeedAddress, ni, token
							);
					}

					Application.Invoke (delegate {

						progressbar1?.Pulse ();
					});

					if (responses == null) {
						// 

						if (settings != null && settings.HasOnAutomateFail && settings.OnAutomateFail != null) {

							Task.Run (delegate {

								SoundPlayer player =
								new SoundPlayer (settings.OnAutomateFail);
								player.Load ();
								player.Play ();
							});

						}

						string unexpected = "Application behaved unexpectedly";
						MessageDialog.ShowMessage ("Scripting error", unexpected);
						WriteToInfoBox (unexpected);
						return;
					}
		    			/*
					if (responses.) {

					}
					*/	    
		    			
					success = responses.Succeeded;
					if (!success) {
						string errMess = "Error submitting orders\n";
						this.WriteToInfoBox (errMess);
						this.WriteToInfoBox (responses?.Message ?? "null");
						this.WriteToInfoBox (responses?.TroubleResponse?.Message ?? "null");
						//this.WriteToInfoBox (responses?.TroubleResponse?.);
						//shouldContinue = false;

						if (settings != null && settings.HasOnAutomateFail && settings.OnAutomateFail != null) {

							Task.Run (delegate {

								SoundPlayer player =
								new SoundPlayer (settings.OnAutomateFail);
								player.Load ();
								player.Play ();
							});

						}
						break;
						//return;
					}

					string successMessage = "Orders submitted successfully\n";
					this.WriteToInfoBox (successMessage);
					Application.Invoke (delegate {

						progressbar1?.Pulse ();
					});

					/*
					var cache = AccountSequenceCache.GetCacheForAccount (rw.GetStoredReceiveAddress ());

					cache.RemoveAndSavePrevious(orders);
						*/

				}

			} catch (Exception canEx) when (canEx is OperationCanceledException || canEx is TaskCanceledException) {
#if DEBUG
				if (DebugIhildaWallet.FilledRuleManagementWindow) {
					Logging.ReportException (method_sig, canEx);
				}
#endif
				return;

			} catch (Exception e) {

				this.WriteToInfoBox (e.Message);

				//SoundSettings settings = SoundSettings.LoadSoundSettings ();
				if (settings != null && settings.HasOnAutomateFail && settings.OnAutomateFail != null) {

					Task.Run (delegate {

						SoundPlayer player =
						new SoundPlayer (settings.OnAutomateFail);
						player.Load ();
						player.Play ();
					});

				}

				return;

			} finally {
				//tokenSource.Dispose ();
				this.tokenSource?.Cancel ();
				this.tokenSource = null;
				this.SetIsRunningUI (false);

				Application.Invoke (delegate {
					progressbar1.Fraction = 0;
					stopWhenConvenientCheckbutton.Visible = false;
					stopWhenConvenientCheckbutton.Active = false;
					StopWhenConvenient = false;
				});

				string message = "Automation thread has stopped\n";
				MessageDialog.ShowMessage ("Automation stopped", message);
				WriteToInfoBox (message);



			}


		}


		private void DoLedgerWait (uint ledgers, CancellationToken token)
		{

			string infoMessage = "Waiting for " + ledgers + " ledgers ";
			this.WriteToInfoBox (infoMessage + "\n");

			WakeTokenSource?.Cancel ();
			WakeTokenSource = new CancellationTokenSource ();
			var wakeToken = WakeTokenSource.Token;
			for (int i = 0; i < ledgers && !token.IsCancellationRequested && !wakeToken.IsCancellationRequested && !StopWhenConvenient; i++) {
				Gtk.Application.Invoke (
					delegate {
						progressbar1?.Pulse ();

					}
				);
				// TODO set max max sleep time. Example 60 seconds or 
				WaitHandle.WaitAny ( new WaitHandle [] { LedgerTracker.LedgerResetEvent, token.WaitHandle, wakeToken.WaitHandle });
				this.WriteToInfoBox (infoMessage + i.ToString () + "\n");

				
			}
		}


		private void DoSleep (uint seconds, CancellationToken token)
		{


			string infoMessage = "Sleeping for " + seconds + " seconds";

#if DEBUG
			if (DebugIhildaWallet.FilledRuleManagementWindow) {
				Logging.WriteLog (infoMessage);
			}

#endif



			this.WriteToInfoBox (infoMessage);

			try {
				WakeTokenSource?.Cancel ();
				WakeTokenSource = new CancellationTokenSource ();
				var wakeToken = WakeTokenSource.Token;
				for (int sec = 0; sec < seconds && !token.IsCancellationRequested && !wakeToken.IsCancellationRequested && !StopWhenConvenient; sec++) {


					if (sec % 5 == 0) {
						this.WriteToInfoBox (".");
					}
					

					WaitHandle.WaitAny ( new WaitHandle [] { token.WaitHandle, wakeToken.WaitHandle }, 1000 );
					//Task.Delay (250).Wait ();
					Application.Invoke (delegate {
						progressbar1?.Pulse ();

					});
					//}


				}
			} catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException) {
				return;
			} finally {
				this.WriteToInfoBox ("\n");
			}
		}

		private bool StopWhenConvenient;



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


		private CancellationTokenSource WakeTokenSource = new CancellationTokenSource ();

#if DEBUG
		private const string clsstr = nameof (FilledRuleManagementWindow) + DebugRippleLibSharp.colon;
#endif

	}



	public class BotSleepSettings
	{
		public bool? HasSleepSeconds {
			get;
			set;
		}

		public bool? HasLedgerWait {
			get;
			set;
		}

		public uint? SleepSeconds {
			get;
			set;
		}

		public uint? LedgerWait {
			get;
			set;
		}

		public static void Save (string address, BotSleepSettings settings)
		{

			try {
				string path = Path.Combine (FileHelper.WALLET_TRACK_PATH, address + ".slp");

				string serialized = DynamicJson.Serialize (settings);

				File.WriteAllText (path, serialized);
			} catch (Exception e) {

			}
		}

		public static BotSleepSettings Load (string address)
		{
			string path  = Path.Combine (FileHelper.WALLET_TRACK_PATH, address + ".slp");

			return LoadSettings (path);
		}

		public static BotSleepSettings LoadSettings (String path)
		{
#if DEBUG
			string method_sig = nameof (BotSleepSettings) + DebugRippleLibSharp.colon + nameof (LoadSettings);

			if (DebugIhildaWallet.FilledRuleManagementWindow) {
				Logging.WriteLog (method_sig + path + DebugRippleLibSharp.beginn);
			}

#endif

			try {

				Logging.WriteLog ("Looking for sleep settings at " + path + "\n");

				if (File.Exists (path)) { // 
					Logging.WriteLog ("Settings found!\n");

					String wah = File.ReadAllText (path, Encoding.UTF8);

					BotSleepSettings settings = DynamicJson.Parse (wah);
					return settings;
				}


			} catch (Exception e) {
				Logging.WriteLog (e.Message);
			}

			return null;


		}

	}
}

