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
using RippleLibSharp.Commands.Accounts;
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

			// Set the "force wake" button to wake up the bot by cancelling the sleep thread
			forcewakebutton.Clicked += (object sender, EventArgs e) => {



				if (WakeTokenSource != null) {
					WakeTokenSource?.Cancel ();

					string mes = "Wake requested";
					WriteToInfoBox (mes);

					WriteToOurputScreen (mes + "\n");

				} else {

					string mes = "No thread to wake";
					WriteToInfoBox (mes);

					WriteToOurputScreen (mes + "\n");


				}




			};


			// set the built in wallet widget and set an event to allow it to switch wallets 
			walletswitchwidget1.SetRippleWallet (rw);
			walletswitchwidget1.WalletChangedEvent += (object source, WalletChangedEventArgs eventArgs) => {

				RippleWallet rippleWallet = eventArgs.GetRippleWallet ();
				this.SetRippleWallet (rippleWallet);

				string message = "Wallet changed to " + rippleWallet.GetStoredReceiveAddress ();

				WriteToInfoBox (message);
				WriteToOurputScreen (message + "\n");
			};




			// ListStore used for the rules treeview
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

			// ListStore used for the sentiment treeview
			this.SentimentStore = new Gtk.ListStore (typeof (string), typeof (string));

			//TextHighlighter.Highlightcolor = TextHighlighter.GREEN;
			//string coloredAdd = TextHighlighter.Highlight (rw.GetStoredReceiveAddress());
			//label1.Markup = "Rules for " + coloredAdd;


			// reusable toggle cell for the rule treeview
			CellRendererToggle ruleToggleCell = new CellRendererToggle {
				Radio = false
			};




			// Tell the toggle cell how to toggle itself
			ruleToggleCell.Toggled += (object o, ToggledArgs args) => {

				// Without a rule store nothing can be done
				if (RuleListStore == null) {
					// TODO error scenerio

					return;
				}

				// Get path if clicked toggle cell
				TreePath path = new TreePath (args.Path);

				/*
				if (!RuleListStore.GetIter (out TreeIter iter, path)) {
					return;
				}
				*/

				// get toggle cell index
				int index = path.Indices [0];

				// retrieve rule from list of rules
				OrderFilledRule ofr = this.RuleManagerObj.RulesList.ElementAt (index);

				// If rule not found, return
				if (ofr == null) {
					// TODO error scenerio
					return;
				}


				// toggle the rule in the list
				ofr.IsActive = !ofr.IsActive;

				// toggle the cell. 
				// I think this is wrong
				//ruleToggleCell.Active = ofr.IsActive;
				//Task.Run ((System.Action)this.RuleManagerObj.SaveRules);

				Task.Run (
					delegate {
						// save rules to disk
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

				TextHighlighter highlighter = new TextHighlighter ();

				Sentiment sentiment = GetSelectedSentiment ();


				sentiment.Rating = ((SentimentRatingEnum)Enum.Parse (typeof (SentimentRatingEnum), args.NewText)).ToString ();
				//SentimentManagerObject.RemoveSentiment(sentiment)



				this.SentimentManagerObject.SaveSentiments ();
				SetSentiments (this.SentimentManagerObject.SentimentList);

				StringBuilder stringBuilder = new StringBuilder ();

				stringBuilder.Append (sentiment.Match);
				stringBuilder.Append (" sentiment set to ");

				/*
				switch (sentiment.GetEnum()) {

				case SentimentRatingEnum.Trash:
					highlighter.Highlightcolor = TextHighlighter.RED;
					break;

				case SentimentRatingEnum.Very_Bearish:
				case SentimentRatingEnum.Bearish:
					highlighter.Highlightcolor = TextHighlighter.LIGHT_RED;
					break;

				case SentimentRatingEnum.Neutral:
					break;

				case SentimentRatingEnum.Bullish:
				case SentimentRatingEnum.Very_Bullish:
					highlighter.Highlightcolor = TextHighlighter.GREEN;
					break;

				case SentimentRatingEnum.Mooning:
					highlighter.Highlightcolor = TextHighlighter.PURPLE;
					break;

				}
				*/

				WriteToInfoBox (stringBuilder.ToString () + sentiment.GetMarkupString ());

				stringBuilder.Append (sentiment.Rating);
				stringBuilder.AppendLine ();

				WriteToOurputScreen (stringBuilder.ToString ());

			};

			cellRendererCombo.Model = comboModel;
			cellRendererCombo.TextColumn = 0;
			//cellRendererCombo.
			cellRendererCombo.HasEntry = false;
			//cellRendererCombo.Mode = CellRendererMode.Editable;
			//cellRendererCombo.Sensitive = true;

			this.treeview1.AppendColumn ("Active", ruleToggleCell, "active", 0);
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
					// TODO
					//
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
					WriteToOurputScreen (srmessage);
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


				StringBuilder stringBuilder = new StringBuilder ();


				if (tokenSource == null) {

					stringBuilder.Append ("Thread has already been stopped");

					WriteToInfoBox (stringBuilder.ToString ());

					stringBuilder.AppendLine ();

					WriteToOurputScreen (stringBuilder.ToString ());

					return;
				}


				if (tokenSource.auto) {


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

					stringBuilder.Clear ();

					if (!should) {

						stringBuilder.Append ("Cancellation haulted");

						WriteToInfoBox (stringBuilder.ToString ());

						stringBuilder.AppendLine ();

						WriteToOurputScreen (stringBuilder.ToString ());

						return;
					}
				}


				stringBuilder.Clear ();

				stringBuilder.Append ("Cancelling automation");

				WriteToInfoBox (stringBuilder.ToString ());

				stringBuilder.AppendLine ();

				WriteToOurputScreen (stringBuilder.ToString ());

				tokenSource?.Cancel ();
				tokenSource = null;
			};

			this.button1.Clicked += (object sender, EventArgs e) => {

				StringBuilder stringBuilder = new StringBuilder ();

				if (tokenSource != null) {

					stringBuilder.Append ("Another thread is currently running already");

					WriteToInfoBox (stringBuilder.ToString ());

					stringBuilder.AppendLine ();

					WriteToOurputScreen (stringBuilder.ToString ());

					//MessageDialog.ShowMessage ();
					return;
				}

				this.tokenSource = new BotCancellTokenSource () {
					auto = false
				};

				CancellationToken token = tokenSource.Token;


				// UI text
				{
					stringBuilder.Append ("Syncing Orders Cache.... ");
					WriteToInfoBox (stringBuilder.ToString ());

					stringBuilder.AppendLine ();
					WriteToOurputScreen (stringBuilder.ToString ());

					stringBuilder.Clear ();
				}

				WalletSwitchWidget walletSwitchWidget = this.walletswitchwidget1;

				//progressbar1.Pulse ();

				RippleWallet wallet = walletSwitchWidget?.GetRippleWallet ();
				if (wallet == null) {
					stringBuilder.Append ("No wallet selected");
					WriteToInfoBox (stringBuilder.ToString ());

					stringBuilder.AppendLine ();

					WriteToOurputScreen (stringBuilder.ToString ());

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



						DoPumpUI ();


						accountSequnceCache.OnOrderCacheEvent += (object sen, OrderCachedEventArgs ocev) => {



							DoPumpUI ();

							if (ocev.UIpump) {
								return;
							}

							TextHighlighter highlighter = new TextHighlighter {
								Highlightcolor =
								ocev.GetSuccess ?
								(ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN) :
								TextHighlighter.RED
							};

							string message = highlighter.Highlight (ocev.Message);

							WriteToInfoBox (message);

							WriteToOurputScreen (message);

						};



						accountSequnceCache.SyncOrdersCache (token);
						DoPumpUI ();

						string m = "Finished Syncing orders cache \n";
						WriteToInfoBox (m);

						WriteToOurputScreen (m + "\n");

					} catch (Exception ex) {
						this.WriteToOurputScreen (ex.Message);
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

						string message = "Automation thread has stopped";
						//MessageDialog.ShowMessage ("Automation stopped", message);

						WriteToInfoBox (message);

						WriteToOurputScreen (message + "\n");
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

					string m = "No account has been selected for cache deletion";

					WriteToInfoBox (m);

					MessageDialog.ShowMessage (
						"Select an account",
						m
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


			this.button1998.Clicked += (sender, e) => {

				RippleWallet rippleWallet = walletswitchwidget1.GetRippleWallet ();

				if (rippleWallet == null) {
					// TODO alert user to select a wallet
					return;
				}

				Task.Run (delegate {

					NetworkInterface ni = NetworkController.GetNetworkInterfaceGuiThread ();

					// TODO integrate cancel token
					var task = AccountCurrencies.GetResult (
						rippleWallet.GetStoredReceiveAddress (),
						ni,
						new CancellationTokenSource ().Token
					);

					task.Wait ();

					var list = task.Result.result.receive_currencies;

					foreach (var currency in list) {
						this.SentimentManagerObject.AddSentiment (
							new Sentiment () {
								Match = currency,
								Rating = nameof (SentimentRatingEnum.Neutral)
							}
						);
					}

					this.SentimentManagerObject.SaveSentiments ();

					SetSentiments (this.SentimentManagerObject.SentimentList);
				});


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

			if (!string.IsNullOrWhiteSpace (secondsStr)) {
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

			this.tokenSource?.Cancel ();

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

			Task.Run (delegate {

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

		StringBuilder outPutScreenBuffer = new StringBuilder ();
		List<string> lines = new List<string> ();
		public void WriteToOurputScreen (string message)
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

				outPutScreenBuffer.Clear ();
				foreach (String s in lines) {

					outPutScreenBuffer.Append (s);
				}

				if (label6 != null) {
					//if (infoBoxBuffer.Length > 0) {
					label6.Markup = outPutScreenBuffer.ToString ();
					//}

				}

				/*
				if (scrolledwindow1 != null) {
					scrolledwindow1.Vadjustment.Value = scrolledwindow1.Vadjustment.Upper;
				}
				*/
			});
		}

		public void WriteToInfoBox (string message)
		{
			Application.Invoke (
				delegate {

					infoLabel.Markup = message;
				}
			);
		}


		public void DoPumpUI ()
		{
			Gtk.Application.Invoke (
				delegate {

					progressbar1?.Pulse ();
				}
			);

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


			StringBuilder stringBuilder = new StringBuilder ();

			if (this.tokenSource != null) {

				stringBuilder.Append ("A rule script is already running\n");
				WriteToInfoBox (stringBuilder.ToString ());

				stringBuilder.AppendLine ();
				WriteToOurputScreen (stringBuilder.ToString ());

				Gtk.Application.Invoke (delegate {
					this.canselbutton.Show ();

				});


				return;
			}

			try {

				// set the token source for cancelling task
				this.tokenSource = new BotCancellTokenSource () { auto = false };

				// Retrieve a cancel token
				CancellationToken token = tokenSource.Token;

				// Throw an exception if the task is cancelled
				token.ThrowIfCancellationRequested ();

				// Set UI that a task is running
				this.SetIsRunningUI (true);


				// get network interface return if not valid
				NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();
				if (ni == null) {

					stringBuilder.Append ("Could not connect to network");
					WriteToInfoBox (stringBuilder.ToString ());

					MessageDialog.ShowMessage ("Network Warning", stringBuilder.ToString ());
					this.SetIsRunningUI (false);

					// remove the token, redundant since finally clause does this
					tokenSource = null;

					return;
				}

				// get selected wallet
				RippleWallet rw = walletswitchwidget1.GetRippleWallet ();
				if (rw == null) {
					string messg = "No wallet Selected";
					MessageDialog.ShowMessage (messg);
					WriteToOurputScreen (messg);
					WriteToInfoBox (messg);
					this.SetIsRunningUI (false);
					return;
				}


				/*
				// Pro feature // will be true if MarketBot fee paid
				bool ShouldContinue = LeIceSense.DoTrialDialog (rw, LicenseType.MARKETBOT);


				//bool ShouldContinue = LeIceSense.LastDitchAttempt (rw, LicenseType.AUTOMATIC);

				// if fee not paid exit
				if (!ShouldContinue) {
					// TODO print fee requirement
					WriteToOurputScreen ("This requires ihilda pro \n");
					this.SetIsRunningUI (false);
					return;
				}

				*/

				// New trading bot 
				Robotics robot = new Robotics (this.RuleManagerObj);

				// retrieve starting ledger from UI
				long? strt = this.ledgerconstraintswidget1.GetStartFromLedger ();


				// if retrieved start ledger is not null and repeating warn the user
				// save the value for next run
				if (strt != null) {
					if (strt == previousStartLedger) {
						stringBuilder.Clear ();
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

				// retrieve end ledger from UI
				long? endStr = this.ledgerconstraintswidget1.GetEndLedger ();

				// retrieve transaction limit from UI
				int? lim = this.ledgerconstraintswidget1.GetLimit ();

				// Print progress to UI

				stringBuilder.Clear ();
				stringBuilder.AppendLine ();
				stringBuilder.Append ("Polling data for ");
				stringBuilder.Append (rw?.GetStoredReceiveAddress () ?? "null");
				stringBuilder.AppendLine ();

				this.WriteToInfoBox (stringBuilder.ToString ());
				this.WriteToOurputScreen (stringBuilder.ToString ());


				// retrieve last saved ledger from hard disk
				WalletLedgerSave ledgerSave = WalletLedgerSave.LoadLedger (rw?.BotLedgerPath);
				uint? lastSavedLedger = ledgerSave?.Ledger;


				// If the specified ledger isnt null, confirm to the user their value was received
				if (strt != null) {

					stringBuilder.Clear ();

					stringBuilder.AppendLine ();
					stringBuilder.Append ("Starting from ledger ");
					stringBuilder.Append (strt?.ToString () ?? "null");

					WriteToInfoBox (stringBuilder.ToString ());


					stringBuilder.AppendLine ();

					this.WriteToOurputScreen (stringBuilder.ToString ());




				} else {


					if (lastSavedLedger == null || lastSavedLedger == 0) {
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
					} else {



						stringBuilder.Clear ();
						stringBuilder.Append ("Using last known ledger ");
						stringBuilder.Append (lastSavedLedger?.ToString () ?? "null");
						stringBuilder.Append (" + 1");


						string mss = stringBuilder.ToString ();
						WriteToInfoBox (mss);

						stringBuilder.Clear ();

						stringBuilder.AppendLine ("Starting ledger is null");
						stringBuilder.AppendLine (mss);

						// else tell user which ledger to start from
						this.WriteToOurputScreen (stringBuilder.ToString ());



						// add one since last ledger was already processed. 
						//lastSavedLedger++;
					}
				}




				// Tell the bot how to print to the window
				robot.OnMessage += (object sender, MessageEventArgs e) => {

					// write the bots message to the output screen
					this.WriteToOurputScreen (e.Message);

					DoPumpUI ();
				};

				var task = Task.Run (delegate {

					// DoLogic is run with the U.I settings or null. 
					return robot?.DoLogic (rw, ni, strt, endStr, lim, token); ;
				}, token);


				while (
					!token.IsCancellationRequested &&
					    !task.IsCompleted &&
					    !task.IsCanceled &&
					!task.IsFaulted
				) {

					task.Wait (250, token);


					//progressbar1?.Pulse ();
					DoPumpUI ();
				}



				if (token.IsCancellationRequested) {
					this.SetIsRunningUI (false);
					return;
				}

				DoLogicResponse tuple = task?.Result;



				if (token.IsCancellationRequested) {
					return;
				}


				string title = "No filled";
				//string message = null;
				if (tuple == null) {

					stringBuilder.Clear ();
					stringBuilder.Append ("Do logic returned null");

					WriteToInfoBox (stringBuilder.ToString ());

					MessageDialog.ShowMessage (title, stringBuilder.ToString ());
					stringBuilder.AppendLine ();
					WriteToOurputScreen (stringBuilder.ToString ());
				} else

				if (tuple.LastLedger == null) {

					stringBuilder.Clear ();
					stringBuilder.Append ("Filled orders error");
					MessageDialog.ShowMessage (title, stringBuilder.ToString ());
					stringBuilder.AppendLine ();
					WriteToOurputScreen (stringBuilder.ToString ());
					this.SetIsRunningUI (false);
					return;
				} else


				if (tuple.FilledOrders == null || !tuple.FilledOrders.Any ()) {

					stringBuilder.Clear ();
					stringBuilder.Append ("There are no new filled orders");
					WriteToInfoBox (stringBuilder.ToString ());
					MessageDialog.ShowMessage (title, stringBuilder.ToString ());

					stringBuilder.AppendLine ();
					WriteToOurputScreen (stringBuilder.ToString ());
					//return;
				} else {

					int num = tuple.FilledOrders.Count ();

					stringBuilder.Clear ();
					stringBuilder.Append (num);
					stringBuilder.Append (" suggested orders");

					WriteToInfoBox (stringBuilder.ToString ());

					stringBuilder.AppendLine ();
					WriteToOurputScreen (stringBuilder.ToString ());
				}

				Application.Invoke (delegate {
					this.ledgerconstraintswidget1.SetLastKnownLedger (tuple.LastLedger.ToString ()); //this.label9.Text = 
				});


				if (tuple?.FilledOrders?.FirstOrDefault () == null) {
					this.SetIsRunningUI (false);
					return;
				}

				stringBuilder.Clear ();
				stringBuilder.Append ("Preparing orders and order submit window");

				WriteToInfoBox (stringBuilder.ToString ());

				stringBuilder.AppendLine ();
				WriteToOurputScreen (stringBuilder.ToString ());


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


							/*
							win = new OrderSubmitWindow (rw, LicenseType.MARKETBOT) {
								Visible = false
							};
							*/

							win = new OrderSubmitWindow (rw) {
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

					DoPumpUI ();


					try {
						stringBuilder.Clear ();

						stringBuilder.Append ("Processing orders");

						WriteToInfoBox (stringBuilder.ToString ());
						stringBuilder.Append (". This may take some time");

						WriteToOurputScreen (stringBuilder.ToString ());
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
							WriteToOurputScreen (".");


							DoPumpUI ();


						}

					} catch (Exception e) {

						throw e;

					} finally {

						DoPumpUI ();


						WriteToOurputScreen ("\n");
					}

					// should not print if an exceptional event occured.

					stringBuilder.Clear ();
					stringBuilder.Append ("Orders Processed");

					WriteToInfoBox (stringBuilder.ToString ());

					stringBuilder.AppendLine ();

					WriteToOurputScreen (stringBuilder.ToString ());
				}

			} catch (TaskCanceledException cancelException) {
#if DEBUG
				if (DebugIhildaWallet.FilledRuleManagementWindow) {
					Logging.ReportException (method_sig, cancelException);
				}
#endif


				stringBuilder.Clear ();
				stringBuilder.Append ("Task cancelled");

				WriteToInfoBox (stringBuilder.ToString ());

				stringBuilder.AppendLine ();
				WriteToOurputScreen (stringBuilder.ToString ());
				this.SetIsRunningUI (false);

			} catch (OperationCanceledException opCanException) {
#if DEBUG
				if (DebugIhildaWallet.FilledRuleManagementWindow) {
					Logging.ReportException (method_sig, opCanException);
				}
#endif
				stringBuilder.Clear ();
				stringBuilder.Append ("Operation cancelled");

				WriteToInfoBox (stringBuilder.ToString ());

				stringBuilder.AppendLine ();
				WriteToOurputScreen (stringBuilder.ToString ());
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

				stringBuilder.Clear ();

				stringBuilder.Append ("Automation thread has stopped");
				//MessageDialog.ShowMessage ("Automation stopped", message);

				WriteToInfoBox (stringBuilder.ToString ());

				stringBuilder.AppendLine ();
				WriteToOurputScreen (stringBuilder.ToString ());
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

			StringBuilder stringBuilder = new StringBuilder ();


			// Create a tokensource and token for cancelling bot
			this.tokenSource = new BotCancellTokenSource () { auto = true };
			CancellationToken token = tokenSource.Token;

			// Get the selected wallet from widget else return
			RippleWallet rw = walletswitchwidget1.GetRippleWallet ();
			if (rw == null) {

				stringBuilder.Clear ();
				stringBuilder.Append ("No wallet Selected");

				WriteToInfoBox (stringBuilder.ToString ());

				stringBuilder.AppendLine ();
				MessageDialog.ShowMessage (stringBuilder.ToString ());

#if DEBUG
				if (DebugIhildaWallet.FilledRuleManagementWindow) {
					Logging.WriteLog (stringBuilder.ToString ());
				}
#endif


				return;
			}


			// Get network interface else return
			NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();
			if (ni == null) {

				stringBuilder.Clear ();
				stringBuilder.Append ("Could not connect to network");


				MessageDialog.ShowMessage ("Network Warning", stringBuilder.ToString ());

#if DEBUG
				if (DebugIhildaWallet.FilledRuleManagementWindow) {
					Logging.WriteLog (method_sig, stringBuilder.ToString ());
				}
#endif

				WriteToInfoBox (stringBuilder.ToString ());

				stringBuilder.AppendLine ();

				WriteToOurputScreen (stringBuilder.ToString ());

				return;
			}



			// License hard coded to AUTOMATIC constant
			LicenseType licenseT = LicenseType.AUTOMATIC;



			// Check license for last time and return if fail
			bool should = LeIceSense.LastDitchAttempt (rw, licenseT);
			if (!should) {

				return;
			}



			#region AUTOWARNING
			bool cont = false;
			using (ManualResetEvent manualResetEvent = new ManualResetEvent (false)) {
				manualResetEvent.Reset ();

				Gtk.Application.Invoke (
				    delegate {
					    // TODO more explicit warning
					    
					    StringBuilder warningStr = new StringBuilder ();

					    if (ProgramVariables.darkmode) {
						    warningStr.Append ("<markup><span foreground=\"red\"><big><b>WARNING!</b></big></span> : This <b>TRADING BOT</b> will execute orders automatically for account <b>");
					    } else {
						    warningStr.Append ("<markup><span foreground=\"#FFAABB\"><big><b>WARNING!</b></big></span> : This <b>TRADING BOT</b> will execute orders automatically for account <b>");
					    }

					    warningStr.Append (rw.GetStoredReceiveAddress ());
					    warningStr.Append ("</b></markup>");





					    progressbar1?.Pulse ();
			    			cont = AreYouSure.AskQuestion (
						"Warning !!!",
						warningStr.ToString ());


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
			#endregion

			// load sound settings from hard disk
			SoundSettings settings = SoundSettings.LoadSoundSettings ();

			try {

				this.SetIsRunningUI (true);

				// Object that can submit transactions
				OrderSubmitter orderSubmitter = new OrderSubmitter ();


				// tell orderSubnitter to print fee if it's too high
				orderSubmitter.OnFeeSleep += (object sender, FeeSleepEventArgs e) => {
					StringBuilder sb = new StringBuilder ();

					switch (e.State) {

					case FeeSleepState.Begin:
						sb.Append ("Fee ");
						sb.Append (e?.FeeAndLastLedger?.Fee.ToString () ?? "null");
						sb.Append (" is too high, waiting on lower fee");

						WriteToInfoBox (sb.ToString ());

						this.WriteToOurputScreen (
							sb.ToString ()
			    			);
						break;

					case FeeSleepState.PumpUI:


						this.WriteToOurputScreen (".");
						break;

					case FeeSleepState.Wake:

						sb.Append ("Using Fee ");
						sb.Append (e?.FeeAndLastLedger?.Fee.ToString () ?? "null");

						WriteToInfoBox (sb.ToString ());

						this.WriteToOurputScreen ("\n");
						break;

					}

					DoPumpUI ();
				};

				// tell order submitter to print transaction outcome
				orderSubmitter.OnOrderSubmitted += (object sender, OrderSubmittedEventArgs e) => {

					StringBuilder sb = new StringBuilder ();

					string outcome = e.Success ? "Submitted Order Successfully " : "Failed to submit order ";
					string seq = e?.RippleOfferTransaction?.Sequence.ToString () ?? "null";

					// output screen
					{
						sb.AppendLine ();
						sb.AppendLine (outcome);

						sb.Append ("Selling ");
						sb.Append (e.RippleOfferTransaction.TakerGets.amount.ToString ());
						sb.Append (" ");
						sb.AppendLine (e.RippleOfferTransaction.TakerGets.currency);

						sb.Append ("Buying ");
						sb.Append (e.RippleOfferTransaction.TakerPays.amount.ToString ());
						sb.Append (" ");
						sb.AppendLine (e.RippleOfferTransaction.TakerPays.currency);

						sb.Append ("Sequence : ");
						sb.AppendLine (seq);

						sb.Append ("Hash : ");
						sb.AppendLine (e?.RippleOfferTransaction?.hash ?? "");

						sb.AppendLine (e.Message);

						this.WriteToOurputScreen (sb.ToString ());
					}

					sb.Clear ();
					sb.Append (outcome);
					sb.Append ("Sequence : ");
					sb.Append (seq);
					WriteToInfoBox (sb.ToString ());


					DoPumpUI ();


				};


				// tell orderSubmitter to print on order validating
				orderSubmitter.OnVerifyingTxBegin += (object sender, VerifyEventArgs e) => {

					StringBuilder sb = new StringBuilder ();


					string m = "Verifying transaction ";
					string seq = e?.RippleOfferTransaction?.Sequence.ToString () ?? "null";

					// output screen
					{
						sb.AppendLine ();
						sb.AppendLine (m);

						sb.Append ("Selling ");
						sb.Append (e.RippleOfferTransaction.TakerGets.amount.ToString ());
						sb.Append (" ");
						sb.AppendLine (e.RippleOfferTransaction.TakerGets.currency);

						sb.Append ("Buying ");
						sb.Append (e.RippleOfferTransaction.TakerPays.amount.ToString ());
						sb.Append (" ");
						sb.AppendLine (e.RippleOfferTransaction.TakerPays.currency);

						sb.Append ("Sequence : ");
						sb.AppendLine (seq);

						sb.Append ("Hash : ");
						sb.AppendLine (e?.RippleOfferTransaction?.hash ?? "");
						sb.AppendLine ();

						this.WriteToOurputScreen (sb.ToString ());

					}

					DoPumpUI ();

					sb.Clear ();
					sb.Append (m);
					sb.Append ("Sequence : ");
					sb.Append (seq);
					WriteToInfoBox (sb.ToString ());






				};


				// Tell orderSubmitter to print validation outcome
				orderSubmitter.OnVerifyingTxReturn += (object sender, VerifyEventArgs e) => {
					StringBuilder sb = new StringBuilder ();

					TextHighlighter highlighter = new TextHighlighter ();

					string seq = e?.RippleOfferTransaction?.Sequence.ToString () ?? "null";
					string hash = e?.RippleOfferTransaction?.hash ?? "";

					string messg = null;
					if (e.Success) {

						string t = "Transaction ";
						string v = " Verified";

						sb.Append (t);
						sb.Append (seq);
						sb.AppendLine (v);

						WriteToInfoBox (sb.ToString ());

						highlighter.Highlightcolor = ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN;

						sb.Clear ();
						sb.Append (t);
						sb.Append (hash);
						sb.AppendLine (v);


					} else {

						string f = "Failed to validate transaction ";
						sb.Append (f);
						sb.Append ("with sequence");
						sb.AppendLine (seq);



						highlighter.Highlightcolor = TextHighlighter.RED;

						WriteToInfoBox (sb.ToString ());

						sb.Clear ();
						sb.Append (f);
						sb.AppendLine (hash);
					}

					//stringBuilder.AppendLine ();

					messg = highlighter.Highlight (sb);

					DoPumpUI ();

					this.WriteToOurputScreen (messg);
				};

				// Tell orderSubmitter to print verify tx message
				orderSubmitter.OnVerifyTxMessage += (object sender, VerifyEventArgs e) => {
					//StringBuilder stringBuilder = new StringBuilder ();
					//stringBuilder.Append ();

					WriteToOurputScreen (e.Message);
					WriteToInfoBox (e.Message);
				};


				DoPumpUI ();


				// print to UI
				{
					stringBuilder.Clear ();
					stringBuilder.Append ("Running automation script for address ");
					stringBuilder.Append (rw?.GetStoredReceiveAddress () ?? "");

					WriteToInfoBox (stringBuilder.ToString ());

					stringBuilder.AppendLine ();
					this.WriteToOurputScreen (
						 stringBuilder.ToString ()
					);
				}


				// New trading bot
				Robotics robot = new Robotics (this.RuleManagerObj);

				// tell the bot how to print messages to the window
				robot.OnMessage += (object sender, MessageEventArgs e) => {
					this.WriteToOurputScreen (e?.Message);
					this.WriteToInfoBox (e?.Message);
				};

				// get the start ledger from UI
				Int64? strt = this.ledgerconstraintswidget1.GetStartFromLedger ();

				// if start has been specified
				if (strt != null && strt > 0) {

					// if specified start is repeated
					if (strt == previousStartLedger) {
						StringBuilder sb = new StringBuilder ();
						sb.Append ("You've supplied the same starting ledger twice consecutively. ");
						sb.Append ("Although this may have been intended it's a common human error. ");
						sb.Append ("Are you sure you'd like to continue?");
						bool GoOn = AreYouSure.AskQuestionNonGuiThread (
							"Same start ledger",
							sb.ToString ()
						);

						if (!GoOn) {
							return;
						}
					}

					previousStartLedger = (int?)strt;

				}



				// get end ledger from UI
				long? endStr = this.ledgerconstraintswidget1.GetEndLedger ();

				// limit from UI
				int? lim = this.ledgerconstraintswidget1.GetLimit ();


				bool success = false;



				PasswordAttempt passwordAttempt = new PasswordAttempt ();

				passwordAttempt.InvalidPassEvent += (object sender, EventArgs e) => {

					string inv = "Invalid password";

					WriteToInfoBox (inv);

					bool shou = AreYouSure.AskQuestionNonGuiThread (
					inv,
					"Unable to decrypt seed. Invalid password.\nWould you like to try again?"
					);

				};

				passwordAttempt.MaxPassEvent += (object sender, EventArgs e) => {
					string mess = "Max password attempts";

					WriteToInfoBox (mess);

					MessageDialog.ShowMessage (mess);
					WriteToOurputScreen ("\n" + mess + "\n");
				};


				DecryptResponse response = passwordAttempt.DoRequest (rw, token);


				// initial decrypt attempt
				RippleIdentifier rippleSeedAddress = response.Seed; //rw.GetDecryptedSeed ();

				if (rippleSeedAddress == null) {
					//string msg = "missing seed";
					return;
				}

				if (StopWhenConvenient) return;






				BotSleepSettings botSleepSettings = null;


				while (!token.IsCancellationRequested && !StopWhenConvenient) {
					ManualResetEvent manualResetEvent = new ManualResetEvent (false);
					manualResetEvent.Reset ();

					Gtk.Application.Invoke (delegate {

						botSleepSettings = ParseSleepSettings ();
						manualResetEvent.Set ();
					});




					WaitHandle.WaitAny (new WaitHandle [] { manualResetEvent, token.WaitHandle }, 5000); // after 5 seconds it's probably hung and sleep time is not a critical error

					if (endStr != null) {
						if (strt > endStr) {
							// TODO



							stringBuilder.Clear ();
							stringBuilder.Append ("Maximum ledger ");
							stringBuilder.Append (endStr);
							stringBuilder.Append (" reached");

							WriteToInfoBox (stringBuilder.ToString ());

							stringBuilder.AppendLine ();

							this.WriteToOurputScreen (stringBuilder.ToString ());

							break;
						}
					}

					DoPumpUI ();


					stringBuilder.Clear ();
					stringBuilder.Append ("\nPolling data for ");
					stringBuilder.Append ((rw?.GetStoredReceiveAddress () ?? "null"));

					var mm = stringBuilder.ToString ();

					WriteToInfoBox (mm);

					stringBuilder.Clear ();
					stringBuilder.AppendLine ();

					stringBuilder.AppendLine (mm);

					this.WriteToOurputScreen (
						stringBuilder.ToString ());


					WalletLedgerSave ledgersave = WalletLedgerSave.LoadLedger (rw.BotLedgerPath);



					uint? last = ledgersave?.Ledger;
					if (strt != null) {

						stringBuilder.Clear ();
						stringBuilder.Append ("Starting from ledger ");
						stringBuilder.Append (strt?.ToString () ?? "null");

						WriteToInfoBox (stringBuilder.ToString ());

						string mmm = stringBuilder.ToString ();

						stringBuilder.Clear ();
						stringBuilder.AppendLine ();
						stringBuilder.AppendLine (mmm);

						this.WriteToOurputScreen (stringBuilder.ToString ());

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


						// print UI
						{
							stringBuilder.Clear ();
							stringBuilder.Append ("Using last known ledger ");
							stringBuilder.Append (last);

							string ss = stringBuilder.ToString ();
							WriteToInfoBox (ss);

							stringBuilder.Clear ();
							stringBuilder.AppendLine ();
							stringBuilder.AppendLine ("Starting ledger is null");
							stringBuilder.AppendLine ();
							stringBuilder.AppendLine (ss);

							this.WriteToOurputScreen ("\n\n Using last known ledger " + last + "\n");
						}
					}








					DoLogicResponse tuple = null;

					// once dologic is called it's not convenient to stop
					do {
						tuple = robot.DoLogic (rw, ni, strt, endStr, lim, token);

						DoPumpUI ();


					} while (!token.IsCancellationRequested && tuple == null /* && !StopWhenConvenient */);


					/*
					if (token.IsCancellationRequested || StopWhenConvenient) {
						return;
					}
					*/


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

					}
					);

					DoPumpUI ();

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


					// print to UI
					{
						int numb = orders.Count ();

						stringBuilder.Clear ();
						stringBuilder.Append ("Submitting ");
						stringBuilder.Append (numb.ToString ());
						stringBuilder.Append (" orders");

						WriteToInfoBox (stringBuilder.ToString ());

						stringBuilder.AppendLine ();
						this.WriteToOurputScreen (stringBuilder.ToString ());

						DoPumpUI ();
					}

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

					DoPumpUI ();

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

						WriteToInfoBox (unexpected);

						MessageDialog.ShowMessage ("Scripting error", unexpected);
						WriteToOurputScreen (unexpected);
						return;
					}
					/*
					if (responses.) {

					}
					*/

					success = responses.Succeeded;
					if (!success) {

						// pring UI
						{
							stringBuilder.Clear ();
							stringBuilder.Append ("Error submitting orders");
							WriteToInfoBox (stringBuilder.ToString ());

							stringBuilder.AppendLine ();

							this.WriteToOurputScreen (stringBuilder.ToString ());
							this.WriteToOurputScreen (responses?.Message ?? "null");
							this.WriteToOurputScreen (responses?.TroubleResponse?.Message ?? "null");
						}


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



					// pring UI
					{
						stringBuilder.Clear ();

						stringBuilder.Append ("Orders submitted successfully");


						WriteToInfoBox (stringBuilder.ToString ());

						stringBuilder.AppendLine ();
						this.WriteToOurputScreen (stringBuilder.ToString ());

						DoPumpUI ();
					}

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

				this.WriteToOurputScreen (e.Message);

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

				string message = "Automation thread has stopped";

				WriteToInfoBox (message);

				MessageDialog.ShowMessage ("Automation stopped", message);

				WriteToOurputScreen (message + "\n");



			}


		}


		private void DoLedgerWait (uint ledgers, CancellationToken token)
		{

			string infoMessage = "Waiting for " + ledgers + " ledgers ";
			this.WriteToOurputScreen (infoMessage + "\n");

			WakeTokenSource?.Cancel ();
			WakeTokenSource = new CancellationTokenSource ();
			var wakeToken = WakeTokenSource.Token;
			for (int i = 0; i < ledgers && !token.IsCancellationRequested && !wakeToken.IsCancellationRequested && !StopWhenConvenient; i++) {
				DoPumpUI ();
				// TODO set max max sleep time. Example 60 seconds or 
				WaitHandle.WaitAny (new WaitHandle [] { LedgerTracker.LedgerResetEvent, token.WaitHandle, wakeToken.WaitHandle });
				this.WriteToOurputScreen (infoMessage + i.ToString () + "\n");


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



			WriteToInfoBox (infoMessage);
			this.WriteToOurputScreen (infoMessage);

			try {
				StringBuilder sb = new StringBuilder ();

				int dots = 0;

				WakeTokenSource?.Cancel ();
				WakeTokenSource = new CancellationTokenSource ();
				var wakeToken = WakeTokenSource.Token;
				for (int sec = 0; sec < seconds && !token.IsCancellationRequested && !wakeToken.IsCancellationRequested && !StopWhenConvenient; sec++) {


					if (sec % 5 == 0) {
						this.WriteToOurputScreen (".");

						dots++;

						if (dots > 5) {
							dots = 0;
						}

						sb.Clear ();
						sb.Append (infoMessage);
						sb.Append (".....".Take (dots));

						WriteToInfoBox (sb.ToString ());


					}




					WaitHandle.WaitAny (new WaitHandle [] { token.WaitHandle, wakeToken.WaitHandle }, 1000);
					//Task.Delay (250).Wait ();
					DoPumpUI ();
					//}


				}
			} catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException) {
				return;
			} finally {
				this.WriteToOurputScreen ("\n");
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

				string mess = "Speep settings saved";



			} catch (Exception e) {

#if DEBUG
				//Logging.ReportException ();
#endif

			}
		}

		public static BotSleepSettings Load (string address)
		{
			string path = Path.Combine (FileHelper.WALLET_TRACK_PATH, address + ".slp");

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

