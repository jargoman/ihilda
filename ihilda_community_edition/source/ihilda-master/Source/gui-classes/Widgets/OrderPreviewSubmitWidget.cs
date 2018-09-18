using System;
using System.Threading;
using System.Threading.Tasks;

using System.Linq;

using System.Collections;
using System.Collections.Generic;

using Gtk;
using RippleLibSharp.Transactions;
using RippleLibSharp.Transactions.TxTypes;

using RippleLibSharp.Network;
using IhildaWallet.Networking;

using RippleLibSharp.Keys;
using RippleLibSharp.Result;

using RippleLibSharp.Util;

using RippleLibSharp.Nodes;
using System.Text;

using RippleLibSharp.Commands.Tx;
using RippleLibSharp.Commands.Server;
using RippleLibSharp.Commands.Accounts;
using IhildaWallet.Util;
using RippleLibSharp.Binary;

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
				typeof (string), // Cost
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
			this.treeview1.AppendColumn ("Cost", txtr, "markup", 5);
			this.treeview1.AppendColumn ("Mark", txtr, "markup", 6);
			this.treeview1.AppendColumn ("Status", txtr, "markup", 7);
			this.treeview1.AppendColumn ("Result", txtr, "markup", 8);

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
			this.resetbutton.Clicked += Button197_Clicked;
			this.selectbutton.Clicked += Selectbutton_Clicked;



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

			foreach (AutomatedOrder ao in _offers) {
				ao.Selected = !allselected;
			}

			this.SetOffers (_offers);

		}

		void Button197_Clicked (object sender, EventArgs e)
		{
			this.SetOffers (_default_offers);
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


			uint se = Convert.ToUInt32 (RippleLibSharp.Commands.Accounts.AccountInfo.GetSequence (rw.GetStoredReceiveAddress (), ni));


			RippleIdentifier rsa = rw.GetDecryptedSeed ();
			this.SubmitOrderAtIndex ((int)index, se, ni, rsa);
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

				Thread th = new Thread (new ParameterizedThreadStart (IndexSubmit));



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

		void ApplyRuleButtonClicked (object sender, EventArgs e)
		{
			
			for (int i = 0; i < _offers.Length; i++) {
				bool b = _offers [i].Red;
				if (b) {
					_offers [i].TakerGets /= 1.005m;
					_offers [i].TakerPays *= 1.005m;
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
					//diff = Math.Abs (dif);

					// 
					bool spreadTooSmall = resaleEstimate > costj;
					if (spreadTooSmall) {
						_offers [i].Red = true;
						_offers [j].Red = true;


						HighLightPrice (i.ToString (), price.ToString ());
						HighLightCost (i.ToString (), cost.ToString ());

						HighLightPrice (j.ToString (), pricej.ToString ());
						HighLightCost (j.ToString (), costj.ToString ());
					}
				}

			}
			//SetOffers (_offers);




		}


		private void HighLightCost (string path, string message)
		{
			if (message == null)
				message = "";

			TextHighlighter.Highlightcolor = TextHighlighter.RED;
			string s = TextHighlighter.Highlight (message);


			this.SetCost (path, s);



		}

		private void SetCost (string path, string message)
		{
			Gtk.Application.Invoke ((object sender, EventArgs e) => {
				if (Liststore.GetIterFromString (out TreeIter iter, path)) {
					Liststore.SetValue (iter, 5, message);



				}
			});
		}

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

			TextHighlighter.Highlightcolor = TextHighlighter.RED;
			string s = TextHighlighter.Highlight (message);


			SetPrice (path, s);


		}

		void Removebutton_Clicked (object sender, EventArgs e)
		{

			LinkedList<AutomatedOrder> orders = new LinkedList<AutomatedOrder> ();
			foreach (AutomatedOrder ao in _offers) {
				if (!ao.Selected) {
					orders.AddLast (ao);
				}
			}

			this.SetOffers (orders.ToArray ());

		}

		void Stopbutton_Clicked (object sender, EventArgs e)
		{
			this.stop = true;

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
				return;
			}

			this._offers = new AutomatedOrder [offers.Count ()];




			for (int i = 0; i < offers.Count (); i++) {

				AutomatedOrder o = offers.ElementAt (i);   //[i];
				if (o == null) {
					// TODO 
				}

				Decimal price = o.TakerPays.GetNativeAdjustedPriceAt (o.TakerGets);
				Decimal cost = o.TakerGets.GetNativeAdjustedPriceAt (o.TakerPays);

				Liststore.AppendValues (
					o.Selected,
					(i + 1).ToString(),
					o.TakerPays.ToString (),
					o.TakerGets.ToString (),

					price.ToString (),
					cost.ToString (),
					o.BotMarking?.ToString ()

				);

				//o.selected = true;

				this._offers [i] = o;

			}


			treeview1.Model = Liststore;

			Analysisbutton_Clicked (null,null);


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

			TextHighlighter.Highlightcolor = colorName;
			string s = TextHighlighter.Highlight (/*"Success : " + */message);

			Gtk.Application.Invoke ((object sender, EventArgs e) => {
				if (Liststore.GetIterFromString (out TreeIter iter, path)) {
					Liststore.SetValue (iter, 7, s);



				}
			});




		}

		public void SetResult (string path, string message, string colorName)
		{
			
			TextHighlighter.Highlightcolor = colorName;
			string s = TextHighlighter.Highlight (message ?? "null");

			Application.Invoke ((object sender, EventArgs e) => {
				if (Liststore.GetIterFromString (out TreeIter iter, path)) {
					Liststore.SetValue (iter, 8, s);
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
					Liststore.SetValue (iter, 7, "");
					Liststore.SetValue (iter, 8, "");


				}
			});
		}

		public bool SubmitOrderAtIndex (int index, uint sequence, NetworkInterface ni, RippleIdentifier rsa)
		{

#if DEBUG
			string method_sig = clsstr + nameof (SubmitOrderAtIndex) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			try {
				ClearIndex (index);

				//
				this.SetStatus (index.ToString (), "Queued", TextHighlighter.GREEN);


				UInt32? lastFee = null;
			retry:
				AutomatedOrder off = _offers [index];


				#region clientmemo


				MemoIndice memoIndice = Program.GetClientMemo ();

				off.AddMemo (memoIndice);

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

					off.AddMemo (markIndice);
				} 

#endregion


				RippleOfferTransaction tx = new RippleOfferTransaction (off.Account, off);



				SignOptions opts = null;




				do {

					opts = SignOptions.LoadSignOptions ();
					if (opts != null) break;


					ManualResetEvent manualReset = new ManualResetEvent (false);
					manualReset.Reset ();

					ResponseType type = ResponseType.None;
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


					manualReset.WaitOne ();

					switch (type) {
					case ResponseType.Ok:

						continue;
					default:
						return false;

					}



				}

				// the way to break the loop is to click cancel or to have feesetting load correctly
				while (true);




				this.SetStatus (index.ToString (), "Requesting Fee", TextHighlighter.GREEN);

				Tuple<UInt32, UInt32> tupe = FeeSettings.GetFeeAndLastLedgerFromSettings (ni, lastFee);

				if (stop) {

					this.SetResult (index.ToString (), "Aborted", TextHighlighter.RED);
					stop = false;
					return false;
				}
				//UInt32 f = tupe.Item1; 
				UInt32 f = tupe.Item1;
				tx.fee = f.ToString ();

				tx.Sequence = sequence; // note: don't update se++ with forloop, update it with each order 



				uint lls = 0;
				if (opts != null) {
					lls = opts.LastLedgerOffset;
				}

				if (lls < 5) {
					lls = SignOptions.DEFAUL_LAST_LEDGER_SEQ;
				}


				tx.LastLedgerSequence = tupe.Item2 + lls;

				if (tx.fee.amount == 0 || tx.Sequence == 0) {
					this.SetResult (index.ToString (), "Invalid Fee or Sequence", TextHighlighter.RED);
					throw new Exception ();
				}



				if (opts.UseLocalRippledRPC) {
					this.SetStatus (index.ToString (), "Signing using rpc", TextHighlighter.GREEN);
					try {
						tx.SignLocalRippled (rsa);
					} catch (Exception ex) {
#if DEBUG
						if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
							Logging.ReportException (method_sig, ex);
						}
#endif
						this.SetStatus (index.ToString (), "Error signing over rpc. Is rippled running?", TextHighlighter.RED);
						return false;
					}
					this.SetStatus (index.ToString (), "Signed rpc", TextHighlighter.GREEN);
				} else {
					this.SetStatus (index.ToString (), "Signing using RippleLibSharp", TextHighlighter.GREEN);
					try {
						tx.Sign (rsa);
					} catch ( Exception exception ) {
						
#if DEBUG
						if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
							Logging.ReportException (method_sig, exception);
						}
#endif
						this.SetStatus (index.ToString (), "Error signing using RippleLibSharp.", TextHighlighter.RED);
						return false;
					}
					this.SetStatus (index.ToString (), "Signed RippleLibSharp", TextHighlighter.GREEN);

				}

				if (tx.GetSignedTxBlob() == null) {
					this.SetResult (index.ToString(), "Error signing transaction", TextHighlighter.RED);
					return false;
				}
				  

				if (stop) {
					this.SetResult (index.ToString (), "Aborted", TextHighlighter.RED);
					stop = false;
					return false;
				}



				Task<Response<RippleSubmitTxResult>> task = null;

				try {
					task = NetworkController.UiTxNetworkSubmit (tx, ni);
					this.SetStatus (index.ToString (), "Submitted via websocket", TextHighlighter.GREEN);
					task.Wait ();


				} catch (Exception e) {

					Logging.WriteLog (e.Message);
					this.SetResult (index.ToString (), "Network Error", TextHighlighter.RED);
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

					string warningMessage = "response == null";
					this.SetResult (index.ToString (), warningMessage, TextHighlighter.GREEN);


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

					StringBuilder sb = new StringBuilder ();
					sb.Append ("Error response : ");
					sb.Append (response?.error_message ?? "");

					this.SetResult (index.ToString (), sb.ToString (), TextHighlighter.RED);
#if DEBUG
					if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
						Logging.WriteLog ("Error submitting transaction ");
						Logging.WriteLog (tx.ToJson ());
						Logging.WriteLog (sb.ToString ());
					}
#endif


					return false;

				}



				RippleSubmitTxResult res = response?.result;

				if (res == null) {

					this.SetResult (index.ToString (), "res == null, Bug?", TextHighlighter.RED);
					return false;
				}



				string een = res?.engine_result;

				if (een == null) {
					this.SetStatus (index.ToString (), "engine_result null", TextHighlighter.RED);
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
					this.SetStatus (index.ToString (), "null exception", TextHighlighter.RED);
					return false;
				} catch (OverflowException overFlowException) {
#if DEBUG
					if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
						Logging.ReportException (method_sig, overFlowException);
					}
#endif
					this.SetStatus (index.ToString (), "Overflow Exception", TextHighlighter.RED);
					return false;
				} catch (ArgumentException argumentException) {
#if DEBUG
					if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
						Logging.ReportException (method_sig, argumentException);
					}
#endif

					this.SetStatus (index.ToString (), "Argument Exception", TextHighlighter.RED);
					return false;
				} catch (Exception e) {
#if DEBUG
					if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
						Logging.ReportException (method_sig, e);
					}
#endif
					this.SetStatus (index.ToString (), "Unknown Exception", TextHighlighter.RED);
					return false;
				}

				switch (ter) {

				case Ter.tefALREADY:
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.GREEN);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.GREEN);
					return true;

				case Ter.terQUEUED:

					Thread.Sleep (1000);
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.GREEN);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.GREEN);


					this.VerifyTx (index, tx, ni);
					return true;

				case Ter.tesSUCCESS:
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.GREEN);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.GREEN);
					this.VerifyTx (index, tx, ni);

					return true;

				case Ter.terPRE_SEQ:
				case Ter.tefPAST_SEQ:
				case Ter.tefMAX_LEDGER:
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.RED);
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
					Thread.Sleep (6000);
					this.SetStatus (index.ToString (), res.engine_result + " retrying", TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.RED);
					goto retry;


				case Ter.temBAD_AMOUNT:
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.RED);
					return false;

				case Ter.tecNO_ISSUER:
				case Ter.temBAD_ISSUER:
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.RED);
					return false;

				case Ter.tecUNFUNDED_OFFER:
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.RED);
					return false;

				case Ter.tecUNFUNDED:
				case Ter.tecINSUF_RESERVE_OFFER:
				case Ter.tecINSUF_RESERVE_LINE:
				case Ter.tecINSUFFICIENT_RESERVE:
				case Ter.tecNO_LINE_INSUF_RESERVE:

					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.RED);
					return false;

				case Ter.temBAD_AUTH_MASTER:
				case Ter.tefBAD_AUTH_MASTER:
				case Ter.tefMASTER_DISABLED:
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.RED);
					return false;


				case Ter.terNO_ACCOUNT:
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.RED);
					return false;

				case Ter.tecNO_AUTH: // Not authorized to hold IOUs.
				case Ter.tecNO_LINE:
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.RED);
					return false;

				case Ter.tecFROZEN:
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.RED);
					return false;

				case Ter.tefFAILURE:
					// TODO what to do?
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.RED);
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
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.RED);
					return false;

				case Ter.tecPATH_DRY:
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.RED);
					return false;

				case Ter.tecPATH_PARTIAL:
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.RED);
					return false;

				case Ter.tecOVERSIZE:
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.RED);
					return false;

				case Ter.tefINTERNAL:
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.RED);
					return false;

				case Ter.tefEXCEPTION:
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.RED);
					return false;

				case Ter.tefBAD_LEDGER:
					// report bug to ripple labs
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.RED);
					return false;

				case Ter.tecDIR_FULL:
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.RED);
					return false;

				case Ter.tecCLAIM:
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.RED);
					return false;

				case Ter.tecEXPIRED:
					this.SetStatus (index.ToString (), res.engine_result, TextHighlighter.RED);
					this.SetResult (index.ToString (), res.engine_result_message, TextHighlighter.RED);
					return false;

				default:
					this.SetStatus (index.ToString (), "Response " + res.engine_result + " not imlemented", TextHighlighter.RED);
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
				this.SetResult ( index.ToString (), mssg, TextHighlighter.RED);

				MessageDialog.ShowMessage (mssg, e.Message + e.StackTrace);
				return false;
				//return false;
			}

		}


		public void VerifyTx (int index, RippleOfferTransaction offerTransaction, NetworkInterface ni)
		{



			Task.Run (() => {
				Thread.Sleep (1000);
				this.SetStatus (index.ToString (), "validating tx", TextHighlighter.GREEN);
				Thread.Sleep (5000);
				for (int i = 0; i < 100; i++) {

					Tuple<string, uint> tuple = ServerInfo.GetFeeAndLedgerSequence (ni);

					Task<Response<RippleTransaction>> task = tx.GetRequest (offerTransaction.hash, ni);
					if (task == null) {
						// TODO Debug
						this.SetResult (index.ToString (), "Error : task == null", TextHighlighter.RED);
						return;
					}
					task.Wait ();

					Response<RippleTransaction> response = task.Result;
					if (response == null) {
						this.SetResult (index.ToString (), "Error : response == null", TextHighlighter.RED);
						return;
					}
					RippleTransaction transaction = response.result;

					if (transaction == null) {
						this.SetResult (index.ToString (), "Error : transaction == null", TextHighlighter.RED);
					}

					if (transaction.validated != null && (bool)transaction.validated) {

						this.SetResult (index.ToString (), "validated", TextHighlighter.GREEN);
						return;
					}

					if (tuple.Item2 > offerTransaction.LastLedgerSequence) {
						this.SetResult (index.ToString (), "failed to validate before LastLedgerSequence exceeded", TextHighlighter.RED);
					}

					this.SetResult (index.ToString (), "Not validated yet " + i.ToString (), TextHighlighter.RED);
					Thread.Sleep (3000);
				}
			});



		}

		public void SubmitAll ()
		{
			
#if DEBUG
			string method_sig = clsstr + nameof (SubmitAll) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			AllSubmitted = false;

			// TODO verify inteneded address
			RippleWallet rw = this.walletswitchwidget1.GetRippleWallet ();
			if (rw == null) {
#if DEBUG
				if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
					Logging.WriteLog (method_sig + "w == null, returning\n");
				}
#endif

				return;
			}




			NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();
			if (ni == null) {
				// TODO network interface
				return;
			}

			bool ShouldContinue = LeIceSense.LastDitchAttempt (rw, _licenseType);
			if (!ShouldContinue) {
				return;
			}
			//int? f = FeeSettings.getFeeFromSettings (ni);
			//if (f == null) {
			//	return;
			//}


			uint sequence = Convert.ToUInt32 (AccountInfo.GetSequence (rw.GetStoredReceiveAddress (), ni));


			//LinkedList< AutomatedOrder > failedorders = new LinkedList<AutomatedOrder>();
			//LinkedList< AutomatedOrder > filledorders = new LinkedList<AutomatedOrder>();
			//LinkedList<AutomatedOrder> terQuedorers = new LinkedList<AutomatedOrder> ();
			RippleIdentifier rsa = rw.GetDecryptedSeed ();
			for (int index = 0; index < _offers.Length; index++) {

				if (stop) {
					stop = false;
					return;
				}


				AutomatedOrder off = _offers [index];
				if (!off.Selected) {
					continue;
				}


				bool suceeded = this.SubmitOrderAtIndex (index, sequence++, ni, rsa);
				if (!suceeded) {
					return;
				}
			}

			AllSubmitted = true;


		}

		public bool AllSubmitted {
			get;
			set;
		}

		public void SetRippleWallet (RippleWallet rippleWallet)
		{
			this.walletswitchwidget1.SetRippleWallet (rippleWallet);
		}
		/*
		private RippleWallet _rippleWallet {
			get;
			set;
		}
		*/

		private AutomatedOrder [] _default_offers;

		public void SetDefaultOrders (IEnumerable<AutomatedOrder> orders)
		{
			_default_offers = orders.ToArray ();
		}

		internal void SetLicenseType (LicenseType licenseType)
		{
			this._licenseType = licenseType;
		}


		private LicenseType _licenseType {
			get;
			set;
		}

		private bool stop = false;

		#if DEBUG
		private const string clsstr = nameof (OrderPreviewSubmitWidget) + DebugRippleLibSharp.colon;
		#endif
	}



}

