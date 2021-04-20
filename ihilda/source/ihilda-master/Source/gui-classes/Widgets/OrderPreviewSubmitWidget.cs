
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using IhildaWallet.Networking;
using IhildaWallet.Util;
using RippleLibSharp.Binary;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Commands.Server;
using RippleLibSharp.Commands.Subscriptions;
using RippleLibSharp.Commands.Tx;
using RippleLibSharp.Keys;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;
using RippleLibSharp.Transactions.TxTypes;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class OrderPreviewSubmitWidget : Gtk.Bin
	{
		public OrderPreviewSubmitWidget ()
		{
			this.Build ();
			if (walletswitchwidget1 == null) {
				walletswitchwidget1 = new WalletSwitchWidget ();
				walletswitchwidget1.Show ();

				hbox1.Add (walletswitchwidget1);
			}

			walletswitchwidget1.WalletChangedEvent += (object source, WalletChangedEventArgs eventArgs) => {
				RippleWallet rippleWallet = eventArgs.GetRippleWallet ();
				string acc = rippleWallet?.GetStoredReceiveAddress ();
				if (acc == null) {
					return;
				}

				if (_default_offers != null) {
					foreach (AutomatedOrder ao in _default_offers) {
						ao.Account = acc;
					}
				}

				if (this._offers != null) {
					foreach (AutomatedOrder ao in _offers) {
						ao.Account = acc;
					}
				}
			};

			Liststore = new ListStore (
				typeof (bool),  // Select
				typeof (string), // Number
				typeof (string), // Buy 
				typeof (string), // Sell
				typeof (string), // Price
						 //typeof (string), // Cost
				typeof (string), // Color
				typeof (string),  // Status
				typeof (string)); // Result


			Gtk.CellRendererToggle toggle = new CellRendererToggle {
				Activatable = true
			};

			toggle.Toggled += ItemToggled;

			CellRendererText txtr = new CellRendererText {
				Editable = true
			};




			//this.treeview1.AppendColumn ();
			this.treeview1.AppendColumn ("Select", toggle, "active", 0);
			this.treeview1.AppendColumn ("#", txtr, "markup", 1);
			this.treeview1.AppendColumn ("Buy", txtr, "markup", 2);
			this.treeview1.AppendColumn ("Sell", txtr, "markup", 3);
			this.treeview1.AppendColumn ("Price", txtr, "markup", 4);
			//this.treeview1.AppendColumn ("Cost", txtr, "markup", 5);
			this.treeview1.AppendColumn ("Mark", txtr, "markup", 5);
			this.treeview1.AppendColumn ("Status", txtr, "markup", 6);
			this.treeview1.AppendColumn ("Result", txtr, "markup", 7);

			this.SubmitButton.Clicked += delegate {
				//ThreadStart ts = new ThreadStart(  );
				//Thread tt = new Thread(ts);
				//tt.Start();

				Task.Run (
					(System.Action)SubmitButton_Clicked
				);
			};

			//SubmitButton_Clicked;

			this.stopbutton.Clicked += Stopbutton_Clicked;
			this.removebutton.Clicked += Removebutton_Clicked;
			this.analysisbutton.Clicked += Analysisbutton_Clicked;
			this.applyRuleButton.Clicked += ApplyRuleButtonClicked;
			this.marketbutton.Clicked += Marketbutton_Clicked;
			this.resetbutton.Clicked += Rename_Clicked;
			this.selectbutton.Clicked += Selectbutton_Clicked;
			this.button625.Clicked += Combine_Clicked;


			this.treeview1.ButtonReleaseEvent += (object o, ButtonReleaseEventArgs args) => {
#if DEBUG
				if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
					Logging.WriteLog (
						"ButtonReleaseEvent at x = "
						+ args.Event.X.ToString ()
						+ " y=" + args.Event.Y.ToString ()
					);
				}
#endif


				int x = Convert.ToInt32 (args.Event.X);
				int y = Convert.ToInt32 (args.Event.Y);
				if (!treeview1.GetPathAtPos (x, y, out TreePath path)) {
					return;
				}

				if (!Liststore.GetIter (out TreeIter iter, path)) {
					return;
				}

				int index = path.Indices [0];

				AutomatedOrder ao = _offers [index];
				if (ao == null) {

					return;
				}

				if (args.Event.Button == 3) {
					Logging.WriteLog ("Right click \n");

					OrderRightClicked (ao, index);

					//args.Event.
				}
			};

			label2.UseMarkup = true;

			button209.Clicked += (object sender, EventArgs e) => {

				/*
				foreach (AutomatedOrder offer in _offers) {
					if (offer.IsValidated) {

						offer.Selected = false;

					}
				} 
				*/

				var offs = _offers.Select ((arg) => {
					if (arg.IsValidated) {

						arg.Selected = false;

					}
					return arg;
				});
				SetOffers (offs);

			};

			progressbar3.PulseStep = 0.5;


			this.ruleToSelectedbutton.Clicked += RuleToSelectedbutton_Clicked;
		}

		private void Combine_Clicked (object sender, EventArgs e)
		{

			List<AutomatedOrder> orders = new List<AutomatedOrder> ();

			string getslastCur = null;
			string getslastIssuer = null;
			string payslastCur = null;
			string payslastIssuer = null;

			string lastMark = null;

			Decimal getsAmount = Decimal.Zero;
			Decimal paysAmount = Decimal.Zero;

			bool allSame = true;
			int numSelected = 0;
			foreach (AutomatedOrder ao in _offers) {
				if (ao.Selected) {
					numSelected++;
					if (getslastCur != null && !getslastCur.Equals (ao.taker_gets.currency)) {
						allSame = false;
						break;
					}

					if (getslastIssuer != null && !getslastIssuer.Equals (ao.taker_gets.issuer)) {
						allSame = false;
						break;
					}

					if (payslastCur != null && !payslastCur.Equals (ao.taker_pays.currency)) {
						allSame = false;
						break;
					}

					if (payslastIssuer != null && !payslastIssuer.Equals (ao.taker_pays.issuer)) {
						allSame = false;
						break;
					}

					if (lastMark != null && !lastMark.Equals (ao.BotMarking)) {
						allSame = false;
						break;
					}
					getslastCur = ao.taker_gets.currency;
					getslastIssuer = ao.taker_gets.issuer;

					payslastCur = ao.taker_pays.currency;
					payslastIssuer = ao.taker_pays.issuer;

					lastMark = ao.BotMarking;

					getsAmount += ao.taker_gets.amount;
					paysAmount += ao.taker_pays.amount;
				} else {
					orders.Add (new AutomatedOrder (ao));
				}

			}

			if (numSelected < 2) {
				MessageDialog.ShowMessage (
				"Invalid selection",
				"You must select two or more like orders to combine");
				return;
			}

			if (!allSame) {
				MessageDialog.ShowMessage (
				"Non matching",
				"To combine two or more orders they must match.\nI.e Both counter and base must be the same currency and issuer. Same bot marking");
				return;
			}

			RippleCurrency gets =
			(getslastCur == "XRP")
				? new RippleCurrency (getsAmount)
				    : new RippleCurrency (getsAmount, getslastIssuer, getslastCur);

			RippleCurrency pays =
				(payslastCur == "XRP")
				? new RippleCurrency (paysAmount)
				: new RippleCurrency (paysAmount, payslastIssuer, payslastCur);

			AutomatedOrder automatedOrder = new AutomatedOrder {
				Account = walletswitchwidget1.GetRippleWallet ().GetStoredReceiveAddress (),
				taker_gets = gets,
				taker_pays = pays,
				BotMarking = lastMark
			};

			orders.Add (automatedOrder);

			this.SetOffers (orders);
		}

		void Selectbutton_Clicked (object sender, EventArgs e)
		{
			bool allselected = true;

			foreach (AutomatedOrder ao in _offers) {
				if (!ao.Selected) {
					allselected = false;
					break;

				}
			}

			/*
			foreach (AutomatedOrder ao in _offers) {
				ao.Selected = !allselected;
			}
			*/


			var delayed = _offers.Select ((AutomatedOrder arg) => { arg.Selected = !allselected; return arg; });
			this.SetOffers (delayed);

		}

		void Rename_Clicked (object sender, EventArgs e)
		{

			List<AutomatedOrder> list = new List<AutomatedOrder> ();

			foreach (AutomatedOrder order in _default_offers) {
				list.Add (new AutomatedOrder (order));

			}

			this.SetOffers (list.ToArray ());
		}



		public void IndexSubmit (object ind)
		{


#if DEBUG

			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append (nameof (IndexSubmit));
			stringBuilder.Append (DebugRippleLibSharp.left_parentheses);
			stringBuilder.Append (nameof (System.Object));
			stringBuilder.Append (DebugRippleLibSharp.space_char);
			stringBuilder.Append (nameof (ind));
			stringBuilder.Append (DebugRippleLibSharp.equals);
			stringBuilder.Append (ind ?? ind);
			stringBuilder.Append (DebugRippleLibSharp.right_parentheses);

			string method_sig = stringBuilder.ToString ();
#endif

			tokenSource?.Cancel ();
			tokenSource = new CancellationTokenSource ();
			CancellationToken token = tokenSource.Token;

			int? index = ind as int?;
			if (index == null) {
				return;
			}

			// in case you need the offer object
			// AutomatedOrder ao = _offers[(int)index];


			// TODO double check address
			RippleWallet rw = walletswitchwidget1.GetRippleWallet ();
			if (rw == null) {
#if DEBUG
				if (DebugIhildaWallet.BuyWidget) {
					Logging.WriteLog (method_sig + "w == null, returning\n");
				}
#endif
			}




			NetworkInterface ni = NetworkController.GetNetworkInterfaceGuiThread ();
			if (ni == null) {
				return;
			}

			//int? f = FeeSettings.getFeeFromSettings (ni);
			//if (f == null) {
			//	return;
			//}


			uint se = Convert.ToUInt32 (
				AccountInfo.GetSequence (
					rw.GetStoredReceiveAddress (),
					ni,
					token)
			);



			PasswordAttempt passwordAttempt = new PasswordAttempt ();

			passwordAttempt.InvalidPassEvent += (object sender, EventArgs e) => {
				bool shou = AreYouSure.AskQuestionNonGuiThread (
				"Invalid password",
				"Unable to decrypt seed. Invalid password.\nWould you like to try again?"
				);
			};

			passwordAttempt.MaxPassEvent += (object sender, EventArgs e) => {
				string mess = "Max password attempts";

				MessageDialog.ShowMessage (mess);
				//WriteToOurputScreen ("\n" + mess + "\n");
			};


			DecryptResponse response = passwordAttempt.DoRequest (rw, token);




			RippleIdentifier rsa = response.Seed;
			if (rsa?.GetHumanReadableIdentifier () == null) {
				return;
			}



			SignOptions opts = LoadSignOptions (token);
			SoundSettings settings = SoundSettings.LoadSoundSettings ();


			bool succeed = this.SubmitOrderAtIndex ((int)index, se, opts, ni, token, rsa);

			if (succeed) {
				if (settings != null && settings.HasOnTxSubmit && settings.OnTxSubmit != null) {

					Task.Run (delegate {

						SoundPlayer player = new SoundPlayer (settings.OnTxSubmit);
						player.Load ();
						player.Play ();
					});

				}
			} else {
				if (settings != null && settings.HasOnTxFail && settings.OnTxFail != null) {

					Task.Run (delegate {

						SoundPlayer player = new SoundPlayer (settings.OnTxFail);
						player.Load ();
						player.Play ();
					});

				}
			}
		}


		public void OrderRightClicked (AutomatedOrder ao, int index)
		{

			Menu menu = new Menu ();

			MenuItem edit = new MenuItem ("Edit");
			edit.Show ();
			menu.Add (edit);

			edit.Activated += (object sender, EventArgs e) => {
#if DEBUG
				if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
					Logging.WriteLog ("edit selected");
				}
#endif





			};

			if (!ao.TakerPays.IsNative) {
				MenuItem paysissuer = new MenuItem ("Change issuer for buying " + ao?.TakerPays?.currency ?? "");
				paysissuer.Show ();
				menu.Add (paysissuer);

				paysissuer.Activated += (object sender, EventArgs e) => {
#if DEBUG
					if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
						Logging.WriteLog ("Change receiving issuer selected");
					}
#endif

					RippleAddress ra = IssuerSubmitDialog.DoDialog ("takerpays message");
					if (ra == null) {
						return;
					}

					_offers [index].TakerPays.issuer = ra;

					this.SetOffers (_offers);
				};

			}

			if (!ao.TakerGets.IsNative) {
				MenuItem getsissuer = new MenuItem ("Change issuer for selling " + ao?.TakerGets?.currency ?? "");
				getsissuer.Show ();
				menu.Add (getsissuer);

				getsissuer.Activated += (object sender, EventArgs e) => {
#if DEBUG
					if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
						Logging.WriteLog ("Change paying issuer selected");
					}
#endif


					RippleAddress ra = IssuerSubmitDialog.DoDialog ("takergets message");
					if (ra == null) {
						return;
					}

					_offers [index].TakerGets.issuer = ra;

					this.SetOffers (_offers);

				};

			}


			Gtk.MenuItem submit = new MenuItem ("Submit");
			submit.Show ();
			menu.Add (submit);

			submit.Activated += (object sender, EventArgs e) => {
#if DEBUG
				if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
					Logging.WriteLog ("submit selected");
				}
#endif

				Thread th = new Thread (
					new ParameterizedThreadStart (IndexSubmit)
				);



				th.Start (index);
			};

			MenuItem rule = new MenuItem ("Apply Rule");
			rule.Show ();
			menu.Add (rule);

			rule.Activated += (object sender, EventArgs e) => {

				ao.TakerGets /= 1.005m;
				ao.TakerPays *= 1.005m;

				this._offers [index] = ao;

				this.SetOffers (_offers);

				this.analysisbutton.Click ();
			};


			menu.Popup ();
		}






		void Marketbutton_Clicked (object sender, EventArgs e)
		{

		}

		private void DoMarketCompare ()
		{



			AutomatedOrder [] orders = _offers;

			for (int i = 0; i < orders.Length; i++) {

			}

		}

		void RuleToSelectedbutton_Clicked (object sender, EventArgs e)
		{
			ApplyRuleImplementation (true);
		}

		void ApplyRuleButtonClicked (object sender, EventArgs e)
		{
			ApplyRuleImplementation (false);
		}

		void ApplyRuleImplementation (bool selected)
		{

			string acc = walletswitchwidget1.GetRippleWallet ().Account;

			SentimentManager sentimentManager = new SentimentManager (acc);
			sentimentManager.LoadSentiments ();


			for (int i = 0; i < _offers.Length; i++) {

				bool b = selected ? _offers [i].Selected : _offers [i].Red;
				if (b) {

					string getsCur = _offers [i].taker_pays.currency; // opposite is intended
					string paysCur = _offers [i].taker_gets.currency;

					Sentiment getsSent = sentimentManager.LookUpSentiment (getsCur);
					Sentiment paysSent = sentimentManager.LookUpSentiment (paysCur);

					int getsShare = 0;
					int paysShare = 0;


					if (getsSent != null) {
						SentimentRatingEnum en = (SentimentRatingEnum)Enum.Parse (typeof (SentimentRatingEnum), getsSent.Rating);
						getsShare = (int)en;
					} else {
						getsShare = (int)SentimentRatingEnum.Neutral;
					}

					if (paysSent != null) {
						SentimentRatingEnum en = (SentimentRatingEnum)Enum.Parse (typeof (SentimentRatingEnum), paysSent.Rating);
						paysShare = (int)en;
					} else {
						paysShare = (int)SentimentRatingEnum.Neutral;
					}

					int totalShare = getsShare + paysShare;

					if (totalShare != 0) {
						double profitShare = 0.005 / totalShare;

						_offers [i].TakerGets /= (Decimal)(1 + (profitShare * getsShare));
						_offers [i].TakerPays *= (Decimal)(1 + (profitShare * paysShare));
					} else {
						_offers [i].TakerGets /= 1.0025m;
						_offers [i].TakerPays *= 1.0025m;
					}
				}
			}

			this.SetOffers (_offers);

			this.analysisbutton.Click ();
		}



		void Analysisbutton_Clicked (object sender, EventArgs e)
		{




			for (int a = 0; a < _offers.Length; a++) {

				_offers [a].Red = false;


			}

			StringBuilder priceString = new StringBuilder ();
			StringBuilder priceStringj = new StringBuilder ();

			bool hasred = false;
			for (int i = 0; i < _offers.Length; i++) {

				for (int j = i + 1; j < _offers.Length; j++) {

					if (!_offers [i].taker_gets.currency.Equals (_offers [j].TakerPays.currency)) {
						continue;
					}

					if (!_offers [i].TakerPays.currency.Equals (_offers [j].TakerGets.currency)) {
						continue;
					}



					decimal price = _offers [i].TakerPays.GetNativeAdjustedPriceAt (_offers [i].TakerGets);
					decimal pricej = _offers [j].TakerPays.GetNativeAdjustedPriceAt (_offers [j].TakerGets);

					decimal cost = _offers [i].TakerPays.GetNativeAdjustedCostAt (_offers [i].TakerGets);
					decimal costj = _offers [j].TakerPays.GetNativeAdjustedCostAt (_offers [j].TakerGets);


					decimal spread = 1.01m;

					decimal resaleEstimate = price * spread;


					bool spreadTooSmall = resaleEstimate > costj;
					if (spreadTooSmall) {
						_offers [i].Red = true;
						_offers [j].Red = true;

						hasred = true;

						priceString.Clear ();
						priceString.Append (price);
						priceString.AppendLine ();
						priceString.Append ("<span fgcolor=\"grey\">");
						priceString.Append (cost);
						priceString.Append ("</span>");

						priceStringj.Clear ();
						priceStringj.Append (pricej);
						priceStringj.AppendLine ();
						priceStringj.Append ("<span fgcolor=\"grey\">");
						priceStringj.Append (costj);
						priceStringj.Append ("</span>");


						HighLightPrice (i.ToString (), priceString.ToString ());
						//HighLightCost (i.ToString (), cost.ToString ());

						HighLightPrice (j.ToString (), priceStringj.ToString ());
						//HighLightCost (j.ToString (), costj.ToString ());
					}
				}

			}

			if (hasred) {

				// gui invoke may be needed for timing rather than being on the right thread

				string message = ProgramVariables.darkmode ? "<span fgcolor=\"#FFAABB\">Conflicting orders. Trading with self</span>" : "<span fgcolor=\"red\">Conflicting orders. Trading with self</span>";
				SetInfoBar (message);
			}
			//SetOffers (_offers);




		}

		/*
		private void HighLightCost (string path, string message)
		{
			if (message == null)
				message = "";

			TextHighlighter.Highlightcolor = TextHighlighter.RED;
			string s = TextHighlighter.Highlight (message);


			this.SetCost (path, s);



		} */

		/*
		private void SetCost (string path, string message)
		{
			Gtk.Application.Invoke ((object sender, EventArgs e) => {
				if (Liststore.GetIterFromString (out TreeIter iter, path)) {
					Liststore.SetValue (iter, 5, message);



				}
			});
		}
		*/

		private void SetPrice (string path, string message)
		{
			Application.Invoke ((object sender, EventArgs e) => {
				if (Liststore.GetIterFromString (out TreeIter iter, path)) {
					Liststore.SetValue (iter, 4, message);



				}
			});


		}



		private void HighLightPrice (string path, string message)
		{
			if (message == null)
				message = "";

			TextHighlighter highlighter = new TextHighlighter {
				Highlightcolor = TextHighlighter.RED
			};

			string s = highlighter.Highlight (message);


			SetPrice (path, s);


		}

		void Removebutton_Clicked (object sender, EventArgs e)
		{
			/*
			LinkedList<AutomatedOrder> orders = new LinkedList<AutomatedOrder> ();
			foreach (AutomatedOrder ao in _offers) {
				if (!ao.Selected) {
					orders.AddLast (ao);
				}
			}
	    		*/

			if (_offers == null) {
				return;
			}
			var orders = from off in _offers where !off.Selected select off;

			this.SetOffers (orders.ToArray ());

		}

		void Stopbutton_Clicked (object sender, EventArgs e)
		{
			this.tokenSource.Cancel ();
			this.tokenSource.Dispose ();
			this.tokenSource = null;

		}

		void SubmitButton_Clicked ()
		{
			this.SubmitAll ();
		}

		public void SetOffers (AutomatedOrder offer)
		{
			this.SetOffers (new AutomatedOrder [] { offer });
		}

		public void SetOffers (IEnumerable<AutomatedOrder> offers)
		{
#if DEBUG
			string method_sig = clsstr + nameof (SetOffers) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif


			Liststore.Clear ();
			if (offers == null) {

				string messge = ProgramVariables.darkmode ? "<span fgcolor=\"#FFAABB\">Error null offer list\n</span>" : "<span fgcolor=\"red\">Error null offer list\n</span>";

				SetInfoBar (messge);
				return;
			}


			this._offers = new AutomatedOrder [offers.Count ()];




			for (int i = 0; i < offers.Count (); i++) {

				AutomatedOrder o = offers.ElementAt (i);   //[i];
				if (o == null) {
					// TODO 
				}

				if (string.IsNullOrWhiteSpace (o.Account)) {
					o.Account = this.walletswitchwidget1?.GetRippleWallet ()?.Account;
				}

				Decimal price = o.TakerPays.GetNativeAdjustedPriceAt (o.TakerGets);
				Decimal cost = o.TakerGets.GetNativeAdjustedPriceAt (o.TakerPays);

				StringBuilder priceString = new StringBuilder ();
				priceString.Append (price);
				priceString.AppendLine ();
				priceString.Append ("<span fgcolor=\"grey\">");
				priceString.Append (cost);
				priceString.Append ("</span>");


				StringBuilder getsString = new StringBuilder ();
				if (!o.TakerGets.IsNative) {
					getsString.Append (o.TakerGets.amount.ToString ());
				} else {
					getsString.Append ((o.TakerGets.amount / 1000000).ToString ());
				}
				getsString.Append (" ");
				getsString.Append (o.TakerGets.currency);
				if (!o.TakerGets.IsNative) {
					getsString.AppendLine ();
					getsString.Append (o.TakerGets.issuer);
				}

				StringBuilder paysString = new StringBuilder ();
				if (!o.TakerPays.IsNative) {
					paysString.Append (o.TakerPays.amount.ToString ());
				} else {
					paysString.Append ((o.TakerPays.amount / 1000000).ToString ());
				}
				paysString.Append (" ");
				paysString.Append (o.TakerPays.currency);
				if (!o.TakerPays.IsNative) {
					paysString.AppendLine ();
					paysString.Append (o.TakerPays.issuer);
				}


				bool selected = o.Selected;
				string num = (i + 1).ToString ();
				string pay = paysString.ToString ();
				string get = getsString.ToString ();
				string priceStr = priceString.ToString ();
				string marking = o.BotMarking?.ToString ();

				if (!o.IsValidated) {

					Gtk.Application.Invoke (delegate {

						Liststore.AppendValues (
							selected,
							num,

							pay,
							get,
							priceStr,
							//cost.ToString (),
							marking


						);
					});

				} else {

					Gtk.Application.Invoke (delegate {
						Liststore.AppendValues (
							selected,
							num,
							pay,
							get,
							priceStr,
							marking, // ?? ""  // should we add the null check?
							"Validated"
						);
					});
				}

				//o.selected = true;

				this._offers [i] = o;

			}

			Application.Invoke (delegate {

				treeview1.Model = Liststore;
				label2.Visible = false;
			});


			Analysisbutton_Clicked (null, null);


		}

		public AutomatedOrder [] GetOffers ()
		{

			return _offers;
		}

		private AutomatedOrder [] _offers {
			get;
			set;
		}

		/*
		private CellRendererToggle[] _toggles {
			get;
			set;
		}
		*/


		private void ItemToggled (object sender, ToggledArgs args)
		{

			string s = args.Path;
			int index = Convert.ToInt32 (args.Path);

			if (Liststore.GetIterFromString (out TreeIter iter, args.Path)) {
				bool val = (bool)Liststore.GetValue (iter, 0);
				Liststore.SetValue (iter, 0, !val);


				_offers [index].Selected = !val;
			}
		}

		private void SetStatus (string path, string message, string colorName)
		{
			if (message == null)
				message = "";

			TextHighlighter highlighter = new TextHighlighter ();


			highlighter.Highlightcolor = colorName;
			string s = highlighter.Highlight (/*"Success : " + */message);

			Gtk.Application.Invoke ((object sender, EventArgs e) => {
				if (Liststore.GetIterFromString (out TreeIter iter, path)) {
					Liststore.SetValue (iter, 6, s);



				}
			});




		}

		public void SetResult (string path, string message, string colorName)
		{
			TextHighlighter highlighter = new TextHighlighter ();

			highlighter.Highlightcolor = colorName;
			string s = highlighter.Highlight (message ?? "null");

			Application.Invoke ((object sender, EventArgs e) => {
				if (Liststore.GetIterFromString (out TreeIter iter, path)) {
					Liststore.SetValue (iter, 7, s);
				}
			});




		}




		private Gtk.ListStore Liststore {
			get;
			set;
		}

		public void ClearIndex (int index)
		{
			//Gtk.Application.Invoke ((object sender, EventArgs e) => {


			//} );

			Application.Invoke ((object sender, EventArgs e) => {
				if (Liststore.GetIterFromString (out TreeIter iter, index.ToString ())) {
					Liststore.SetValue (iter, 6, "");
					Liststore.SetValue (iter, 7, "");


				}
			});
		}


		public SignOptions LoadSignOptions (CancellationToken token)
		{

			SignOptions opts = null;




			do {

				opts = SignOptions.LoadSignOptions ();
				if (opts != null) break;
				ResponseType type;
				using (ManualResetEvent manualReset = new ManualResetEvent (false)) {
					manualReset.Reset ();
					type = ResponseType.None;
					Gtk.Application.Invoke (delegate {
						using (SignOptionsDialog signOptionsDialog = new SignOptionsDialog ()) {
							type = (ResponseType)signOptionsDialog.Run ();
							if (type == ResponseType.Ok) {
								signOptionsDialog.ProcessSignOptionsWidget ();
							}
							signOptionsDialog.Destroy ();
						}
						manualReset.Set ();

					});

					WaitHandle.WaitAny (new [] {
						manualReset,
						token.WaitHandle
		    			});
				}


				switch (type) {
				case ResponseType.Ok:

					continue;
				default:
					return null;

				}



			}

			// the way to break the loop is to click cancel or to have feesetting load correctly
			while (!token.IsCancellationRequested);

			return opts;
		}

		public FeeSettings LoadFeeSettings (CancellationToken token)
		{
			FeeSettings feeSettings = FeeSettings.LoadSettings ();
			if (feeSettings == null) {

				string message = ProgramVariables.darkmode ? "<span fgcolor=\"#FFAABB\">" : "<span fgcolor=\"red\">" +
							"Could not load xrp fee preferences. " +
							"Configure fee settings at : " +
							"{wallet manager > Options Tab > Options > XRP fee options}" +
							"</span>";
				SetInfoBar (message);

			}

			return feeSettings;
		}

		public bool SubmitOrderAtIndex (int indexx, uint sequence, SignOptions opts, NetworkInterface ni, CancellationToken token, RippleIdentifier rsa)
		{
			int index = indexx;
#if DEBUG
			string method_sig = clsstr + nameof (SubmitOrderAtIndex) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif


			TextHighlighter greenHighlighter = new TextHighlighter {
				Highlightcolor = ProgramVariables.darkmode ?
					TextHighlighter.CHARTREUSE :
					TextHighlighter.GREEN
			};

			TextHighlighter redHighlighter = new TextHighlighter {
				Highlightcolor = ProgramVariables.darkmode ?
					TextHighlighter.LIGHT_RED :
		    			TextHighlighter.RED
			};


			StringBuilder stringBuilder = new StringBuilder ();



			#region index_str
			// builds string "<b>Tx 1</b> : "
			stringBuilder.Append ("Tx ");
			stringBuilder.Append ((index + 1).ToString ());
			string bld = TextHighlighter.MakeBold (stringBuilder.ToString());

			stringBuilder.Clear ();

			stringBuilder.Append (bld);

			stringBuilder.Append (" : ");

			string txAtIndexStr = stringBuilder.ToString ();
			#endregion





			stringBuilder.Clear ();

			stringBuilder.Append ("Submitting ");
			stringBuilder.Append (txAtIndexStr);
			//stringBuilder.Append ("</span>");

			string infoBarMessage = stringBuilder.ToString ();
			this.SetInfoBar (greenHighlighter.Highlight(infoBarMessage));


			try {
				ClearIndex (index);

				UInt32? lastFee = null;



			retry:

				/*
				string queStr = "Queued";

				this.SetStatus (index.ToString (), queStr, TextHighlighter.GREEN);

				stringBuilder.Clear ();
				stringBuilder.Append (txAtIndexStr);
				stringBuilder.Append (queStr);

				string ms2 = stringBuilder.ToString ();
				Application.Invoke (
					delegate {

						this.label2.Markup = ms2;
						this.label2.Visible = true;

					}
				); */


				#region fee
				string feeReq = "Requesting Fee";


				stringBuilder.Clear ();
				stringBuilder.Append (txAtIndexStr);
				stringBuilder.Append (feeReq);


				string ms3 = greenHighlighter.Highlight( stringBuilder.ToString ());

				this.SetStatus (
					index.ToString (),
					feeReq,
		    			ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);

				this.SetInfoBar (ms3);
				#endregion

				ParsedFeeAndLedgerResp tupe = null;

				AutomatedOrder off = _offers [index];
				RippleOfferTransaction tx = new RippleOfferTransaction (off.Account, off);

				using (Task getFeeTask = Task.Run (delegate {
					try {
						FeeSettings feeSettings = LoadFeeSettings (token);

						string indexStr = index.ToString ();
						feeSettings.OnFeeSleep += (object sender, FeeSleepEventArgs e) => {

							if (e == null) {
#if DEBUG
								if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
									Logging.WriteLog (method_sig, nameof (FeeSleepEventArgs) + " is null");
								}
								return;
#endif
							}

							if (e.State != FeeSleepState.Begin) {
								return;
							}

							StringBuilder sb = new StringBuilder ();

							sb.Append ("Fee ");
							sb.Append (e?.FeeAndLastLedger?.Fee.ToString () ?? "null");
							sb.Append (" is too high, waiting on lower fee");

							string feestr = sb.ToString ();


							this.SetResult (
								indexStr,
								feestr,
								    ProgramVariables.darkmode ? TextHighlighter.YELLOW : TextHighlighter.BLACK);

							sb.Clear ();
							if (txAtIndexStr != null) {
								sb.Append (txAtIndexStr);
							}
							sb.Append (feestr);

							string ms4 = sb.ToString ();

							this.SetInfoBar (ms4);
						};

						tupe = feeSettings.GetFeeAndLastLedgerFromSettings (ni, token, lastFee);

					} catch (Exception e) when (e is TaskCanceledException || e is OperationCanceledException) {
						return;
					} catch (Exception e) {
						return;
					}
				}, token)) {





					#region clientmemo


					MemoIndice memoIndice = Program.GetClientMemo ();
					tx.AddMemo (memoIndice);
					//off.AddMemo (memoIndice);

					#endregion
		    			


					#region markmemo
					if (off.BotMarking != null) {
						MemoIndice markIndice = new MemoIndice {
							Memo = new RippleMemo {
								MemoType = Base58.StringToHex ("ihildamark"),
								MemoFormat = Base58.StringToHex (""),
								MemoData = Base58.StringToHex (off?.BotMarking ?? "")
							}
						};
						tx.AddMemo (markIndice);
						//off.AddMemo (markIndice);
					}

					#endregion












					if (token.IsCancellationRequested) {


						string canstr = "Aborted";
						this.SetResult (index.ToString (), canstr, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

						stringBuilder.Clear ();

						if (ProgramVariables.darkmode) {
							stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
						} else {
							stringBuilder.Append ("<span fgcolor=\"red\">");
						}

						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (canstr);
						stringBuilder.Append ("</span>");
						string ms6 = stringBuilder.ToString ();

						this.SetInfoBar (ms6);
						return false;
					}


					int feeDots = 0;
					while (getFeeTask != null && !getFeeTask.IsCompleted && !getFeeTask.IsCanceled && !getFeeTask.IsFaulted && !token.IsCancellationRequested) {

						feeDots++;

						StringBuilder feeReq2 = new StringBuilder (feeReq);
						feeReq2.Append (new string ('.', feeDots));


						stringBuilder.Clear ();
						if (ProgramVariables.darkmode) {
							stringBuilder.Append ("<span fgcolor=\"chartreuse\">");
						} else {
							stringBuilder.Append ("<span fgcolor=\"green\">");
						}
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (feeReq2);
						stringBuilder.Append ("</span>");

						string mssg = stringBuilder.ToString ();
						this.SetStatus (
							index.ToString (),
							feeReq2.ToString (),
							ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN
						);

						this.SetInfoBar (mssg);

						getFeeTask.Wait (1000, token);

						if (feeDots == 10) {
							feeDots = 0;
						}

					}
				}


				if (tupe == null) {

					string feeEr = "Error retrieving fee and last ledger sequence";
					this.SetResult (index.ToString (), feeEr, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();
					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (feeEr);

					string ms5 = stringBuilder.ToString ();

					this.SetInfoBar (ms5);
					return false;
				}

				if (tupe.HasError) {


					string feeEr = "Error retrieving fee and last ledger sequence";

					this.SetResult (index.ToString (), feeEr, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();
					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (feeEr);
					stringBuilder.Append (tupe?.ErrorMessage);

					string ms5 = stringBuilder.ToString ();

					this.SetInfoBar (ms5);
					return false;
				}

				//UInt32 f = tupe.Item1; 
				UInt32 f = (UInt32)tupe.Fee;
				tx.fee = f.ToString ();

				tx.Sequence = sequence; // note: don't update se++ with forloop, update it with each order 



				uint lls = 0;
				if (opts != null) {
					lls = opts.LastLedgerOffset;
				}


				// document that last ledger below 5 is not respected // TODO maybe change
				if (lls < 5) {
					lls = SignOptions.DEFAUL_LAST_LEDGER_SEQ;
				}


				tx.LastLedgerSequence = (UInt32)tupe.LastLedger + lls;

				if (tx.fee.amount == 0) {

					string invstr = "Invalid Fee";
					this.SetResult (index.ToString (), invstr, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();
					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (invstr);
					stringBuilder.Append ("</span>");
					string ms7 = stringBuilder.ToString ();
					this.SetInfoBar (ms7);
					goto retry;
				}





				if (tx.Sequence == 0) {

					string invstr = "Invalid Sequence";
					this.SetResult (index.ToString (), invstr, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (invstr);
					stringBuilder.Append ("</span>");
					string ms7 = stringBuilder.ToString ();
					this.SetInfoBar (ms7);
					goto retry;
				}

				string approvedFee = tx.fee.ToString ();
				switch (opts.SigningLibrary) {
				case "Rippled":
					stringBuilder.Clear ();
					stringBuilder.Append ("Signing using rpc");


					this.SetStatus (index.ToString (), stringBuilder.ToString (), ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);

					string rpsStr = stringBuilder.ToString ();

					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"chartreuse\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"green\">");

					}
					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (stringBuilder.ToString ());
					stringBuilder.Append ("</span>");

					string ms8 = stringBuilder.ToString ();
					this.SetInfoBar (ms8);
					using (var signTask = Task.Run<bool> (delegate {

						try {
							tx.SignLocalRippled (rsa);
							return true;
						} catch (Exception ex) {
#if DEBUG
							if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
								Logging.ReportException (method_sig, ex);
							}
#endif

							string rpcErr = "Error signing over rpc. Is rippled running?";

							this.SetStatus (index.ToString (), rpcErr, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

							stringBuilder.Clear ();

							if (ProgramVariables.darkmode) {
								stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
							} else {
								stringBuilder.Append ("<span fgcolor=\"red\">");
							}

							stringBuilder.Append (txAtIndexStr);
							stringBuilder.Append (rpcErr);
							stringBuilder.Append ("</span>");
							string ms9 = stringBuilder.ToString ();

							this.SetInfoBar (ms9);
							return false;
						}
					})) {

						StringBuilder stbuild = new StringBuilder ();
						int x = 0;
						while (!signTask.IsCompleted && !token.IsCancellationRequested && !signTask.IsFaulted && !token.IsCancellationRequested) {
							stbuild.Clear ();
							stbuild.Append (rpsStr);

							stbuild.Append (new String ('.', x));

							if (x++ > 7) {
								x = 0;

							}

							token.WaitHandle.WaitOne (1000);

							this.SetStatus (index.ToString (), stbuild.ToString (), ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);

							stringBuilder.Clear ();
							if (ProgramVariables.darkmode) {
								stringBuilder.Append ("<span fgcolor=\"chartreuse\">");
							} else {
								stringBuilder.Append ("<span fgcolor=\"green\">");
							}
							stringBuilder.Append (txAtIndexStr);
							stringBuilder.Append (rpsStr);
							stringBuilder.Append ("</span>");

							string signmessg = stringBuilder.ToString ();
							this.SetInfoBar (signmessg);
						}

						if (!signTask.Result) {
							return false;
						}
					}



					string rpcStr = "Signed rpc";
					this.SetStatus (index.ToString (), rpsStr, ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);
					stringBuilder.Clear ();
					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"chartreuse\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"green\">");
					}
					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (rpcStr);
					stringBuilder.Append ("</span>");

					string ms10 = stringBuilder.ToString ();
					this.SetInfoBar (ms10);
					break;
				case "RippleLibSharp":
					string rlsstr = "Signing using RippleLibSharp";

					this.SetStatus (index.ToString (), rlsstr, ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);

					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"chartreuse\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"green\">");
					}
					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (rlsstr);
					stringBuilder.Append ("</span>");

					string ms11 = stringBuilder.ToString ();
					this.SetInfoBar (ms11);
					try {

						//tx.SignRippleDotNet (rsa);
						tx.SignRippleLibSharp (rsa);
					} catch (Exception exception) {

#if DEBUG
						if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
							Logging.ReportException (method_sig, exception);
						}
#endif

						string errrls = "Error signing using RippleLibSharp";
						this.SetStatus (index.ToString (), errrls, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

						stringBuilder.Clear ();
						if (ProgramVariables.darkmode) {
							stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
						} else {
							stringBuilder.Append ("<span fgcolor=\"red\">");
						}
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (errrls);
						stringBuilder.Append ("</span>");

						string ms12 = stringBuilder.ToString ();

						this.SetInfoBar (ms12);
						return false;
					}

					string sgnstr = "Signed RippleLibSharp";
					this.SetStatus (index.ToString (), sgnstr, ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);

					stringBuilder.Clear ();
					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"chartreuse\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"green\">");
					}
					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (sgnstr);
					stringBuilder.Append ("</span>");

					string ms13 = stringBuilder.ToString ();
					this.SetInfoBar (ms13);
					break;
				case "RippleDotNet":
					string rlsstr2 = "Signing using RippleDotNet";

					this.SetStatus (index.ToString (), rlsstr2, ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);

					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"chartreuse\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"green\">");
					}
					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (rlsstr2);
					stringBuilder.Append ("</span>");

					string ms112 = stringBuilder.ToString ();
					this.SetInfoBar (ms112);
					try {

						tx.SignRippleDotNet (rsa);
						//tx.Sign (rsa);
					} catch (Exception exception) {

#if DEBUG
						if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
							Logging.ReportException (method_sig, exception);
						}
#endif

						string errrls = "Error signing using RippleDotNet";
						this.SetStatus (index.ToString (), errrls, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

						stringBuilder.Clear ();
						if (ProgramVariables.darkmode) {
							stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
						} else {
							stringBuilder.Append ("<span fgcolor=\"red\">");
						}
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (errrls);
						stringBuilder.Append ("</span>");

						string ms12 = stringBuilder.ToString ();

						this.SetInfoBar (ms12);
						return false;
					}

					string signstr = "Signed RippleDotNet";
					this.SetStatus (index.ToString (), signstr, ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);

					stringBuilder.Clear ();
					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"chartreuse\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"green\">");
					}
					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (signstr);
					stringBuilder.Append ("</span>");

					string ms132 = stringBuilder.ToString ();
					this.SetInfoBar (ms132);
					break;
				}

				if (tx.GetSignedTxBlob () == null) {

					string errstr = "Error signing transaction";
					this.SetResult (index.ToString (), errstr, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();
					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span = fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (errstr);
					stringBuilder.Append ("</span>");

					string m14 = stringBuilder.ToString ();

					this.SetInfoBar (m14);

					return false;
				}


				if (token.IsCancellationRequested) {

					string abostr = "Aborted";
					this.SetResult (index.ToString (), abostr, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();


					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span = fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (abostr);
					stringBuilder.Append ("</span>");


					string ms15 = stringBuilder.ToString ();

					this.SetInfoBar (ms15);
					return false;
				}



				Task<Response<RippleSubmitTxResult>> task = null;

				try {
					task = NetworkController.UiTxNetworkSubmit (tx, ni, token);

					string subWit = "Submitted via websocket";
					this.SetStatus (index.ToString (), subWit, ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);

					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"chartreuse\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"green\">");
					}
					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (subWit);
					stringBuilder.Append ("</span>");

					string submitStr = stringBuilder.ToString ();
					this.SetInfoBar (submitStr);


					int MAXDOTS = 5;
					int x = 0;

					//StringBuilder message = new StringBuilder ();
					string wtmessg = "Waiting on network";
					while (
						task != null &&
						!task.IsCompleted &&
						!task.IsCanceled &&
						!task.IsFaulted &&
						!token.IsCancellationRequested
						) {

						stringBuilder.Clear ();
						stringBuilder.Append (wtmessg);

						int dots = x++ % MAXDOTS;

						// the first time 0 dots then 1 then 2,3,4,5

						stringBuilder.Append (new string ('.', x));

						string tmpstr = stringBuilder.ToString ();
						this.SetResult (index.ToString (), tmpstr, ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);

						stringBuilder.Clear ();

						if (ProgramVariables.darkmode) {
							stringBuilder.Append ("<span fgcolor=\"chartreuse\">");
						} else {
							stringBuilder.Append ("<span fgcolor=\"green\">");
						}
						stringBuilder.Append (txAtIndexStr);
						stringBuilder.Append (tmpstr);
						stringBuilder.Append ("</span>");

						string wtms = stringBuilder.ToString ();

						this.SetInfoBar (wtms);

						task.Wait (1000, token);
					}


				} catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException) {

#if DEBUG
					if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
						Logging.ReportException (method_sig, ex);
					}
#endif
					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span = fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append ("Operation Cancelled");

					stringBuilder.Append ("</span>");

					string cancmess = stringBuilder.ToString ();

					this.SetInfoBar (cancmess);
					return false;
				} catch (Exception e) {

					// TODO catch actual net exception
					Logging.WriteLog (e.Message);
					this.SetResult (index.ToString (), "Network Error", ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					return false;
				}

				/*
				finally {
					//Logging.writeLog (method_sig + "sleep");
					 // may or may not keep a slight delay here for orders to process
				}
				*/

				Response<RippleSubmitTxResult> response = task?.Result;

				if (response == null) {

					string warningMessage = "Order submit returned null. response == null";
					this.SetResult (index.ToString (), warningMessage, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"green\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"chartreuse\">");
					}
					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (warningMessage);
					stringBuilder.Append ("</span>");

					string msg = stringBuilder.ToString ();

					this.SetInfoBar (msg);

#if DEBUG
					if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
						Logging.WriteLog ("Error submitting transaction : " + warningMessage);
						Logging.WriteLog (tx.ToJson ());
					}
#endif
					MessageDialog.ShowMessage ("No response from server");
					return false;
				}


				//if ( response.status == null || !response.status.Equals ("success")) {
				if (response.HasError ()) {

					stringBuilder.Clear ();
					stringBuilder.Append ("Error response : ");
					stringBuilder.Append (response?.error_message ?? "");

					string hasErrstr = stringBuilder.ToString ();

					this.SetResult (index.ToString (), hasErrstr, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (hasErrstr);
					stringBuilder.Append ("</span>");

#if DEBUG
					if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
						Logging.WriteLog ("Error submitting transaction ");
						Logging.WriteLog (tx.ToJson ());
						Logging.WriteLog (hasErrstr);
					}
#endif
					string haserrmess = stringBuilder.ToString ();
					this.SetInfoBar (hasErrstr);
					return false;

				}



				RippleSubmitTxResult res = response?.result;

				if (res == null) {
					string bugstr = "res == null, Bug?";
					this.SetResult (index.ToString (), bugstr, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}
					stringBuilder.Append (txAtIndexStr);

					stringBuilder.Append (bugstr);
					stringBuilder.Append ("</span>");

					string ms17 = stringBuilder.ToString ();

					this.SetInfoBar (ms17);

					return false;
				}



				string een = res?.engine_result;

				if (een == null) {

					string engnull = "engine_result null";

					this.SetStatus (index.ToString (), engnull, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (engnull);
					stringBuilder.Append ("</span>");

					string ms18 = stringBuilder.ToString ();

					this.SetInfoBar (ms18);

					return false;
				}




				Ter ter;

				try {
					ter = (Ter)Enum.Parse (typeof (Ter), een, true);
					//ter = (Ter)Ter.Parse (typeof(Ter), een, true);

				} catch (ArgumentNullException exc) {
#if DEBUG
					if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
						Logging.ReportException (method_sig, exc);
					}
#endif

					string argnull = "null exception";
					this.SetStatus (index.ToString (), argnull, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (argnull);
					stringBuilder.Append ("</span>");


					string ms19 = stringBuilder.ToString ();

					this.SetInfoBar (ms19);

					return false;
				} catch (OverflowException overFlowException) {
#if DEBUG
					if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
						Logging.ReportException (method_sig, overFlowException);
					}
#endif

					string flowstr = "Overflow Exception";

					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (flowstr);
					stringBuilder.Append ("</span>");

					string ms20 = stringBuilder.ToString ();

					this.SetInfoBar (ms20);
					this.SetStatus (index.ToString (), flowstr, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					return false;

				} catch (ArgumentException argumentException) {
#if DEBUG
					if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
						Logging.ReportException (method_sig, argumentException);
					}
#endif
					string argexc = "Argument Exception";

					this.SetStatus (index.ToString (), argexc, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (argexc);
					stringBuilder.Append ("</span>");


					string ms21 = stringBuilder.ToString ();
					this.SetInfoBar (ms21);
					return false;
				} catch (Exception e) {
#if DEBUG
					if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
						Logging.ReportException (method_sig, e);
					}
#endif
					string unkexc = "Unknown Exception";

					this.SetStatus (index.ToString (), unkexc, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}
					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (unkexc);
					stringBuilder.Append ("</span>");

					string ms22 = stringBuilder.ToString ();
					this.SetInfoBar (ms22);
					return false;
				}

				switch (ter) {

				case Ter.tefALREADY:
					this.SetStatus (index.ToString (), res.engine_result, ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);

					stringBuilder.Clear ();
					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"chartreuse\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"green\">");
					}
					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string alstr = stringBuilder.ToString ();

					this.SetInfoBar (alstr);

					this._offers [index].IsValidated = true;

					return true;

				case Ter.terQUEUED:

					//Thread.Sleep (50);

					token.WaitHandle.WaitOne (100);
					/*
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.GREEN);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.GREEN);


					this.VerifyTx (index, tx, ni, token);
					return true;
					*/
					goto case Ter.tesSUCCESS;

				case Ter.tesSUCCESS:
					this.SetStatus (index.ToString (), res.engine_result, ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);
					stringBuilder.Clear ();
					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"chartreuse\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"green\">");
					}
					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string sustr = stringBuilder.ToString ();

					this.SetInfoBar (sustr);


					this.VerifyTx (index, tx, off.Previous_Bot_ID, ni, token);



					return true;

				case Ter.terPRE_SEQ:
				case Ter.tefPAST_SEQ:
				case Ter.tefMAX_LEDGER:
					this.SetStatus (index.ToString (), res.engine_result, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string maxstr = stringBuilder.ToString ();

					this.SetInfoBar (maxstr);

					return false;

				case Ter.terRETRY:
				case Ter.telCAN_NOT_QUEUE:
				case Ter.telCAN_NOT_QUEUE_BALANCE:
				case Ter.telCAN_NOT_QUEUE_FEE:
				case Ter.telCAN_NOT_QUEUE_FULL:
				case Ter.telINSUF_FEE_P:
				case Ter.terFUNDS_SPENT:

					if (lastFee != null) {
						return false;
					}

					lastFee = (UInt32)tx.fee.amount;

					int secs = 6;
					stringBuilder.Clear ();

					stringBuilder.Append ("Waiting ");
					stringBuilder.Append (secs.ToString ());
					stringBuilder.Append ("seconds");



					token.WaitHandle.WaitOne (1000);

					// secs - 1 because you already waited a second
					for (int i = 0; i < secs - 1; i++) {

						string waitingForSecsString = stringBuilder.ToString ();

						this.SetResult (index.ToString (), stringBuilder.ToString (), TextHighlighter.BLACK);

						string msg = txAtIndexStr + waitingForSecsString;
						this.SetInfoBar (msg);

						token.WaitHandle.WaitOne (1000);
						stringBuilder.Append (".");
					}

					//Thread.Sleep (6000);
					this.SetStatus (index.ToString (), res.engine_result + " retrying", ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();
					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string retstr = stringBuilder.ToString ();

					this.SetInfoBar (retstr);


					goto retry;


				case Ter.temBAD_AMOUNT:
					this.SetStatus (index.ToString (), res.engine_result, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string badamstr = stringBuilder.ToString ();

					this.SetInfoBar (badamstr);



					return false;

				case Ter.tecNO_ISSUER:
				case Ter.temBAD_ISSUER:
					this.SetStatus (index.ToString (), res.engine_result, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}
					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string noissstr = stringBuilder.ToString ();

					this.SetInfoBar (noissstr);


					return false;

				case Ter.tecUNFUNDED_OFFER:
					this.SetStatus (index.ToString (), res.engine_result, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string unfstr = stringBuilder.ToString ();

					this.SetInfoBar (unfstr);


					return false;

				case Ter.tecUNFUNDED:
				case Ter.tecINSUF_RESERVE_OFFER:
				case Ter.tecINSUF_RESERVE_LINE:
				case Ter.tecINSUFFICIENT_RESERVE:
				case Ter.tecNO_LINE_INSUF_RESERVE:

					this.SetStatus (index.ToString (), res.engine_result, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string nolinestr = stringBuilder.ToString ();

					this.SetInfoBar (nolinestr);


					return false;

				case Ter.temBAD_AUTH_MASTER:
				case Ter.tefBAD_AUTH_MASTER:
				case Ter.tefMASTER_DISABLED:
					this.SetStatus (index.ToString (), res.engine_result, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();
					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string masterdisstr = stringBuilder.ToString ();

					this.SetInfoBar (masterdisstr);
					return false;


				case Ter.terNO_ACCOUNT:
					this.SetStatus (index.ToString (), res.engine_result, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string noaccstr = stringBuilder.ToString ();

					this.SetInfoBar (noaccstr);
					return false;

				case Ter.tecNO_AUTH: // Not authorized to hold IOUs.
				case Ter.tecNO_LINE:
					this.SetStatus (index.ToString (), res.engine_result, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					stringBuilder.Clear ();
					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}
					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string noauthstr = stringBuilder.ToString ();

					this.SetInfoBar (noauthstr);

					return false;

				case Ter.tecFROZEN:
					this.SetStatus (index.ToString (), res.engine_result, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					stringBuilder.Clear ();
					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string frozenstr = stringBuilder.ToString ();

					this.SetInfoBar (frozenstr);
					return false;

				case Ter.tefFAILURE:
					// TODO what to do?
					this.SetStatus (index.ToString (), res.engine_result, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();
					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string failurestr = stringBuilder.ToString ();

					this.SetInfoBar (failurestr);

					return false;

				case Ter.temBAD_FEE:
				case Ter.temMALFORMED:
				case Ter.temINVALID:
				case Ter.temBAD_SIGNATURE:
				case Ter.temBAD_PATH:
				case Ter.temBAD_PATH_LOOP:
				case Ter.temBAD_SEQUENCE:
				case Ter.temBAD_SRC_ACCOUNT:
				case Ter.temDST_IS_SRC:
				case Ter.temDST_NEEDED:
				case Ter.temREDUNDANT:
				case Ter.temRIPPLE_EMPTY:
				case Ter.temDISABLED:
				case Ter.tecOWNERS:
				case Ter.tecINVARIANT_FAILED:
					this.SetStatus (index.ToString (), res.engine_result, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					stringBuilder.Clear ();
					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string invstr = stringBuilder.ToString ();

					this.SetInfoBar (invstr);

					return false;

				case Ter.tecPATH_DRY:
					this.SetStatus (index.ToString (), res.engine_result, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					stringBuilder.Clear ();
					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string drystr = stringBuilder.ToString ();

					this.SetInfoBar (drystr);
					return false;

				case Ter.tecPATH_PARTIAL:
					this.SetStatus (index.ToString (), res.engine_result, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string partstr = stringBuilder.ToString ();

					this.SetInfoBar (partstr);

					return false;

				case Ter.tecOVERSIZE:
					this.SetStatus (index.ToString (), res.engine_result, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string oversizestr = stringBuilder.ToString ();

					this.SetInfoBar (oversizestr);
					return false;

				case Ter.tefINTERNAL:
					this.SetStatus (index.ToString (), res.engine_result, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();
					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					} else {
						stringBuilder.Append ("<spanvfgcolor=\"#FFAABB\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string internalstr = stringBuilder.ToString ();

					this.SetInfoBar (internalstr);
					return false;

				case Ter.tefEXCEPTION:
					this.SetStatus (index.ToString (), res.engine_result, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();
					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string excstr = stringBuilder.ToString ();

					this.SetInfoBar (excstr);

					return false;

				case Ter.tefBAD_LEDGER:
					// report bug to ripple labs
					this.SetStatus (index.ToString (), res.engine_result, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();
					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string badledstr = stringBuilder.ToString ();

					this.SetInfoBar (badledstr);

					return false;

				case Ter.tecDIR_FULL:
					this.SetStatus (index.ToString (), res.engine_result, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();
					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string dirfullstr = stringBuilder.ToString ();

					this.SetInfoBar (dirfullstr);
					return false;

				case Ter.tecCLAIM:
					this.SetStatus (index.ToString (), res.engine_result, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string claimstr = stringBuilder.ToString ();

					this.SetInfoBar (claimstr);
					return false;

				case Ter.tecEXPIRED:
					this.SetStatus (index.ToString (), res.engine_result, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();

					if (ProgramVariables.darkmode) {
						stringBuilder.Append ("<span fgcolor=\"#FFAABB\">");
					} else {
						stringBuilder.Append ("<span fgcolor=\"red\">");
					}

					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (res.engine_result_message);
					stringBuilder.Append ("</span>");
					string expstr = stringBuilder.ToString ();

					this.SetInfoBar (expstr);

					return false;

				default:

					stringBuilder.Clear ();
					stringBuilder.Append ("Response ");
					stringBuilder.Append (res.engine_result);
					stringBuilder.Append (" not imlemented");

					string statstr = stringBuilder.ToString ();
					this.SetStatus (index.ToString (), statstr, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

					stringBuilder.Clear ();
					stringBuilder.Append (txAtIndexStr);
					stringBuilder.Append (statstr);

					string areWethereYet = stringBuilder.ToString ();
					this.SetInfoBar (areWethereYet);

					return false;

				}


				/*
				 * 
				if ( res.tx_json == null || res.tx_json.meta == null || res.tx_json.meta.AffectedNodes == null) {
					return false;
				}

				IEnumerable<RippleNode> nds = from RippleNodeGroup rng in res.tx_json.meta.AffectedNodes 
					where rng.CreatedNode != null

				//&& rng.CreatedNode.FinalFields != null
				&& rng.CreatedNode.NewFields.Account != null
				&& rng.CreatedNode.NewFields.Account == rw.getStoredReceiveAddress()
				&& rng.CreatedNode.LedgerEntryType != null
				&& rng.CreatedNode.LedgerEntryType.Equals( "OfferCreate" )

				select rng.getnode();




				// A newly created order wouldn't cause any new orders except it's own, so there is one or none
				foreach ( RippleNode rn in nds) {

					//OrderChange oc = rn.getOfferChange (rw.getStoredReceiveAddress());
					AutomatedOrder filled = null;
					filled = AutomatedOrder.reconstructFromNode (rn);
					filledorders.AddLast (filled);
					continue; 
				}

				*/

			} catch (Exception cancExc) when (cancExc is TaskCanceledException || cancExc is OperationCanceledException) {
#if DEBUG
				if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
					Logging.ReportException (method_sig, cancExc);
				}
#endif

				string mssg = "Operation cancelled";
				this.SetResult (index.ToString (), mssg, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

				this.SetInfoBar (mssg);

				return false;
			}

#pragma warning disable 0168
			catch (Exception e) {
#pragma warning restore 0168


#if DEBUG
				if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
					Logging.ReportException (method_sig, e);
				}
#endif
				string mssg = "Exception Thrown in code";
				this.SetResult (index.ToString (), mssg, ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);

				this.SetInfoBar (mssg);


				MessageDialog.ShowMessage (mssg, e.Message + e.StackTrace);
				return false;
				//return false;
			}

		}

		public void SetInfoBar (string message)
		{
			Application.Invoke (delegate {
				if (label2 == null) {
					return;
				}
				this.label2.Markup = message;
				this.label2.Visible = true;

			});
		}


		public void VerifyTx (int index, RippleOfferTransaction offerTransaction, string prevBotID, NetworkInterface ni, CancellationToken token)
		{



			Task.Run (() => {

				int verifyWaitTime = 6;

				StringBuilder stringBuilder = new StringBuilder ();
				string valStr = "Validating Tx";

				for (int i = verifyWaitTime; i > 0; i--) {

					stringBuilder.Clear ();
					stringBuilder.Append (valStr);
					stringBuilder.Append (" in ");
					stringBuilder.Append (i);

					this.SetStatus (index.ToString (), stringBuilder.ToString (), ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);
					token.WaitHandle.WaitOne (1000);
				}


				//Thread.Sleep (1000);


				this.SetStatus (index.ToString (), valStr, ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);


				for (int i = 0; i < 100; i++) {


					//FeeAndLastLedgerResponse feeResp = ServerInfo.GetFeeAndLedgerSequence (ni, token);
					Task<Response<RippleTransaction>> task = tx.GetRequest (offerTransaction.hash, ni, token);



					Task<uint?> ledgerTask = Task.Run (
						delegate {

							for (int attempt = 0; attempt < 5; attempt++) {
								uint? led = LedgerTracker.GetRecentLedgerOrNull ();

								if (led == null) {

									FeeAndLastLedgerResponse feeResp = ServerInfo.GetFeeAndLedgerSequence (ni, token);
									led = feeResp?.LastLedger;


								}
								if (led != null) {
									return led;
								}
							}

							return null;
						}
					);


					if (task == null) {
						// TODO Debug
						this.SetResult (index.ToString (), "Error : task == null", ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
						return;
					}
					task.Wait (token);

					Response<RippleTransaction> response = task.Result;
					if (response == null) {
						this.SetResult (index.ToString (), "Error : response == null", ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
						return;
					}
					RippleTransaction transaction = response.result;

					if (transaction == null) {
						this.SetResult (index.ToString (), "Error : transaction == null", ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					}

					if (transaction.validated != null && (bool)transaction.validated) {

						this.SetResult (index.ToString (), "Validated", ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN);
						this._offers [index].IsValidated = true;

						button209.Visible = true;

						//AutomatedOrder ao = AutomatedOrder.ReconsctructFromTransaction (offerTransaction);

						//sequenceCache.RemoveAndSave (ao.p);

						AccountSequenceCache sequenceCache = AccountSequenceCache.GetCacheForAccount (offerTransaction.Account);
						//sequenceCache.UpdateOrdersCache (ao);
						sequenceCache.RemoveAndSave (prevBotID);
						return;
					}

					if (ledgerTask != null) {
						ledgerTask.Wait (1000 * 60 * 2, token);
					}


					//
					uint? ledger = ledgerTask?.Result;

					if (ledger != null && ledger > offerTransaction.LastLedgerSequence) {
						this.SetResult (index.ToString (), "failed to validate before LastLedgerSequence exceeded", ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					}

					string str = "Not validated ";
					stringBuilder.Clear ();
					stringBuilder.Append (str);
					stringBuilder.Append (i.ToString ());

					if (i < 2) {
						this.SetResult (index.ToString (), stringBuilder.ToString (), TextHighlighter.ORANGE);
					} else {
						this.SetResult (index.ToString (), stringBuilder.ToString (), ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
					}
					//Thread.Sleep (3000);
					for (int ind = 0; ind < 1; ind++) {
						token.WaitHandle.WaitOne (1000);
						stringBuilder.Append (".");
						if (i < 2) {
							this.SetResult (index.ToString (), stringBuilder.ToString (), TextHighlighter.ORANGE);
						} else {
							this.SetResult (index.ToString (), stringBuilder.ToString (), ProgramVariables.darkmode ? TextHighlighter.LIGHT_RED : TextHighlighter.RED);
						}
					}


				}
			}, token);



		}

		private CancellationTokenSource tokenSource = null;
		public void SubmitAll ()
		{

#if DEBUG
			string method_sig = clsstr + nameof (SubmitAll) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			tokenSource?.Cancel ();
			tokenSource = new CancellationTokenSource ();
			CancellationToken token = tokenSource.Token;



			TextHighlighter greenHighlighter = new TextHighlighter {
				Highlightcolor = ProgramVariables.darkmode ?
					TextHighlighter.CHARTREUSE :
					TextHighlighter.GREEN
			};

			TextHighlighter redHighlighter = new TextHighlighter {
				Highlightcolor = ProgramVariables.darkmode ?
					TextHighlighter.LIGHT_RED :
		    			TextHighlighter.RED
			};


			SetInfoBar (greenHighlighter.Highlight("Submiting all offers"));


			Gtk.Application.Invoke (delegate {



				progressbar3?.Pulse ();
			});


			AllSubmitted = false;

			SoundSettings settings = SoundSettings.LoadSoundSettings ();
			SoundPlayer onTxFailPlayer = null;
			SoundPlayer onSumitPlayer = null;

			if (settings.HasOnTxFail && settings.OnTxFail != null) {

				Task.Run (delegate {

					onTxFailPlayer = new SoundPlayer (settings.OnTxFail);
					onTxFailPlayer.Load ();

				});

			}


			if (settings.HasOnTxSubmit && settings.OnTxSubmit != null) {

				Task.Run (delegate {

					onSumitPlayer = new SoundPlayer (settings.OnTxSubmit);
					onSumitPlayer.Load ();

				});

			}



			// TODO verify inteneded address
			RippleWallet rw = walletswitchwidget1?.GetRippleWallet ();
			if (rw == null) {
#if DEBUG
				if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
					Logging.WriteLog (method_sig + "w == null, returning\n");
				}
#endif

				if (!ProgramVariables.darkmode) {
					SetInfoBar ("<span fgcolor=\"red\">No wallet selected.</span>");
				} else {
					SetInfoBar ("<span fgcolor=\"#FFAABB\">No wallet selected.</span>");
				}

				SetInfoBar ( redHighlighter.Highlight( "No Wallet Selected" ));

				Application.Invoke (delegate {

					if (progressbar3 == null) {
						return;
					}
					this.progressbar3.Fraction = 0;

				});


				return;
			}




			NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();
			if (ni == null) {
				// TODO network interface
				
				SetInfoBar ( redHighlighter.Highlight ("No network."));

				Application.Invoke (delegate {

					if (progressbar3 == null) {
						return;
					}
					this.progressbar3.Fraction = 0;

				});

				return;
			}



			SetInfoBar (greenHighlighter.Highlight("Verifying License"));

			Application.Invoke (delegate {

				if (progressbar3 == null) {
					return;
				}
				this.progressbar3.Fraction = 0.01;
			});


			//bool ShouldContinue = false;
			uint sequence = 0;
			RippleIdentifier rsa = null;
			SignOptions signOptions = null;
			//Task contTask = null;
			Task<SignOptions> signOptionsTask = null;
			Task seqTask = null;
			try {

				/*
				contTask = Task.Run (
					delegate {
						ShouldContinue = LeIceSense.LastDitchAttempt (rw, _licenseType);


					}
				);
				*/

				sequence = 0;
				seqTask = Task.Run (delegate {
					sequence =
					Convert.ToUInt32 (
					    AccountInfo.GetSequence (
						    rw.GetStoredReceiveAddress (),
						    ni,
						token
						    )
					);

				});

				// TODO investigate possible "unable to load sign options due to incorrect password
				signOptionsTask = Task.Run (
					delegate {

						return LoadSignOptions (token);
					});


				//contTask?.Wait (token);

				/*

				if (!ShouldContinue) {

					string messg =
						 "Insufficient "
		    				+ LeIceSense.LICENSE_CURRENCY
		    				+ ". Requires "
		    				+ _licenseType.ToString ();

					SetInfoBar (redHighlighter.Highlight( messg));
					return;
				}
				*/


				SetInfoBar ( greenHighlighter.Highlight ("Requesting password"));

				Application.Invoke (delegate {


					if (progressbar3 == null) {
						return;
					}
					this.progressbar3.Fraction = 0.02;

				});

				token.WaitHandle.WaitOne (10);


				Application.Invoke (delegate {

					if (progressbar3 == null) {
						return;
					}
					progressbar3.Fraction = 0.03;
				});


				PasswordAttempt passwordAttempt = new PasswordAttempt ();

				passwordAttempt.InvalidPassEvent += (object sender, EventArgs e) => {

					SetInfoBar (greenHighlighter.Highlight("Invalid password"));

					Application.Invoke (delegate {

						progressbar3?.Pulse ();
					});

					bool should = AreYouSure.AskQuestionNonGuiThread (
				    		"Invalid password",
				    		"Unable to decrypt seed. Invalid password.\nWould you like to try again?"
				    	);


				};

				passwordAttempt.MaxPassEvent += (object sender, EventArgs e) => {
					string mess = "Max password attempts";

					MessageDialog.ShowMessage (mess);

					SetInfoBar (redHighlighter.Highlight(mess));

					Application.Invoke (delegate {
						if (progressbar3 == null) {
							return;
						}
						progressbar3.Fraction = 0.04;
					});
					//WriteToOurputScreen ("\n" + mess + "\n");
				};


				DecryptResponse response = passwordAttempt.DoRequest (rw, token);




				rsa = response.Seed;
				if (rsa?.GetHumanReadableIdentifier () == null) {



					return;
				}

				try {
					StringBuilder stringBuilder = new StringBuilder ();

					string msg = "Preparing Orders";

					SetInfoBar (greenHighlighter.Highlight( msg));

					Application.Invoke (delegate {

						if (progressbar3 == null) {
							return;
						}
						this.progressbar3.Fraction = 0.05;

					});


					for (
					    	int i = 0;

				     		((seqTask != null && !seqTask.IsCanceled && !seqTask.IsCompleted && !seqTask.IsFaulted)
						|| (signOptionsTask != null && !signOptionsTask.IsCanceled && !signOptionsTask.IsCompleted && !signOptionsTask.IsFaulted))
		    				&& !token.IsCancellationRequested;

						 i++

				    	) {
						if (i == 10) {
							i = 0;
						}

						Task.WaitAll (new [] { seqTask, signOptionsTask }, 1000, token);

						stringBuilder.Clear ();
						stringBuilder.Append (msg);


						stringBuilder.Append (new String ('.', i));



						SetInfoBar (greenHighlighter.Highlight( stringBuilder.ToString ()));

					}

				} catch (Exception e) {

#if DEBUG
					if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
						Logging.ReportException (method_sig, e);
					}
#endif

					throw e;

				}
				signOptions = signOptionsTask?.Result;
			} catch (Exception e) {
#if DEBUG
				if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
					Logging.ReportException (method_sig, e);
				}
#endif
			} finally {
				// contTask?.Dispose ();
				signOptionsTask?.Dispose ();
				seqTask?.Dispose ();

			}
			//	} // end using sign options task
			//}  // end using seq
			//} // end cont task


			if (signOptions == null) {

				SetInfoBar (redHighlighter.Highlight( "Unable to load sign options" ));
				return;
			}


			double progressStep = 0.9 / _offers.Length;




			for (int index = 0; index < _offers.Length; index++) {

				if (token.IsCancellationRequested) {
					SetInfoBar (redHighlighter.Highlight("Submit all task has been cancelled"));

					return;
				} else {
					Gtk.Application.Invoke (delegate {
						if (progressbar3 == null) {
							return;
						}
						progressbar3.Fraction += progressStep;
					});
				}


				AutomatedOrder off = _offers [index];
				if (!off.Selected) {
					continue;
				}


				bool suceeded = this.SubmitOrderAtIndex (index, sequence++, signOptions, ni, token, rsa);
				if (!suceeded) {
					if (settings.HasOnTxFail && settings.OnTxFail != null) {

						Task.Run ((System.Action)onTxFailPlayer.Play);

					}
					return;
				} else {
					if (settings.HasOnTxSubmit && settings.OnTxSubmit != null) {

						Task.Run ((System.Action)onSumitPlayer.Play);

					}
				}
			}

			SetInfoBar (greenHighlighter.Highlight ("All orders have been submitted successfully"));

			Gtk.Application.Invoke (delegate {

				if (progressbar3 == null) {
					return;
				}
				this.progressbar3.Fraction = 0.96;
			});

			AllSubmitted = true;

			ConfirmValidationOfSelected (token);
		}


		public void ConfirmValidationOfSelected (CancellationToken token)
		{

			var offs = _offers;
			if (offs == null) {
				if (ProgramVariables.darkmode) {
					SetInfoBar ("<span fgcolor=\"#FFAABB\">Unable to verify selected orders. _offer == null</span>");
				} else {
					SetInfoBar ("<span fgcolor=\"red\">Unable to verify selected orders. _offer == null</span>");
				}
				return;
			}


			for (int i = 0; i < 60 * 20; i++) {

				//Task.Delay (1000).Wait (token);
				token.WaitHandle.WaitOne (1000);


				var v = from off in offs where off != null && off.Selected select off;
				if (v == null) {
					return;
				}
				if (!v.Any (x => !x.IsValidated)) {

					if (ProgramVariables.darkmode) {
						SetInfoBar ("<span fgcolor=\"chartreuse\">All selected orders have been validated</span>");
					} else {
						SetInfoBar ("<span fgcolor=\"green\">All selected orders have been validated</span>");
					}

					Gtk.Application.Invoke (delegate {

						if (progressbar3 == null) {
							return;
						}
						this.progressbar3.Fraction = 1;
					});
					return;
				} else {

					if (ProgramVariables.darkmode) {
						SetInfoBar ("<span fgcolor=\"yellow\">All orders have not been validated yet</span>");
					} else {
						SetInfoBar ("<span fgcolor=\"orange\">All orders have not been validated yet</span>");
					}

					Gtk.Application.Invoke (delegate {

						if (progressbar3 == null) {
							return;
						}
						this.progressbar3.Fraction = 1;
					});
				}
			}

		}

		public bool AllSubmitted {
			get;
			set;
		}

		public void SetRippleWallet (RippleWallet rippleWallet)
		{
			this.walletswitchwidget1?.SetRippleWallet (rippleWallet);
		}
		/*
		private RippleWallet _rippleWallet {
			get;
			set;
		}
		*/

		public AutomatedOrder [] _default_offers;

		public void SetDefaultOrders (IEnumerable<AutomatedOrder> orders)
		{

			orders = from o in orders select new AutomatedOrder (o);

			_default_offers = orders.ToArray ();
		}


		/*
		internal void SetLicenseType (LicenseType licenseType)
		{
			this._licenseType = licenseType;
		}




		private LicenseType _licenseType {
			get;
			set;
		}
		*/


		//private bool stop = false;

#if DEBUG
		private const string clsstr = nameof (OrderPreviewSubmitWidget) + DebugRippleLibSharp.colon;
#endif
	}



}

