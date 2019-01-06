using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using RippleLibSharp.Transactions;
using Gtk;
using RippleLibSharp.Util;
using System.Text;
using System.Threading;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class TradePairTree : Gtk.Bin
	{
		public TradePairTree ()
		{
			this.Build ();

			listStore = new ListStore (
				typeof (string), //  base currency
				typeof (string),  // base issuer
				typeof (string), // Counter currency
				typeof (string)); // counter issuer

			CellRendererText cellRenderer = new CellRendererText {
				Sensitive = true,
				Editable = true
			};

			this.treeview1.AppendColumn ("Base", cellRenderer, "text", 0);
			this.treeview1.AppendColumn ("Issuer", cellRenderer, "text", 1);
			this.treeview1.AppendColumn ("Counter", cellRenderer, "text", 2);
			this.treeview1.AppendColumn ("Issuer", cellRenderer, "text", 3);

			this.treeview1.ButtonReleaseEvent += (object o, ButtonReleaseEventArgs args) => {

				Logging.WriteLog ("ButtonReleaseEvent at x=" + args.Event.X.ToString () + " y=" + args.Event.Y.ToString ());
				TradePair tp = GetFromPos (args.Event.X, args.Event.Y);

				if (tp == null) {
					return;
				}

				TradePair sel = GetSelected ();

				if (sel.Equals (tp)) {

				}

				TradePairManager.SelectedTradePair = tp;

				if (args.Event.Button == 3) {

					#region trade
					Menu menu = new Menu ();

					MenuItem tradeMenu = new MenuItem ("Trade");
					tradeMenu.Show ();

					tradeMenu.ButtonPressEvent += delegate {
						if (TradePairManagerWindow.CurrentInstance == null) {
							// TODO

							return;
						}

						Task.Run (
							(System.Action)TradePairManagerWindow.CurrentInstance.Trade
						);
					};

					menu.Add (tradeMenu);
					#endregion

					#region orderBook


					MenuItem orderBookMenu = new MenuItem ("View Orderbook");
					orderBookMenu.Show ();

					orderBookMenu.ButtonPressEvent += delegate {


						if (tp == null) {
							return;
						}
						OrderBookWindow obw = new OrderBookWindow (WalletManager.GetRippleWallet());

						Task.Run ( delegate {
							obw.SetTradePair (tp);


						});


					};

					menu.Add (orderBookMenu);
					#endregion

					#region depth
					MenuItem depthMenu = new MenuItem ("View Depth Chart");
					depthMenu.Show ();

					depthMenu.ButtonPressEvent += delegate {


						if (tp == null) {
							return;
						}
						DepthChartWindow dcw = new DepthChartWindow (WalletManager.GetRippleWallet(), tp);

						dcw.GetWidget ().UpdateBooks (new CancellationToken());
					};

					menu.Add (depthMenu);
					#endregion

					#region rippleCharts
					MenuItem rippleChart = new MenuItem ("View on " + URLexplorer.xrpChartsUrl);
					rippleChart.Show ();

					rippleChart.ButtonPressEvent += delegate {
						if (tp == null) {
							return;
						}
						string baseCurrency = tp.Currency_Base.ToIssuerString ();
						string counterCurrency = tp.Currency_Counter.ToIssuerString ();

						string arguments = @"?interval=1h&range=1w&type=candlestick";

						StringBuilder stringBuiler = new StringBuilder ();

						stringBuiler.Append (URLexplorer.proto);
						stringBuiler.Append (URLexplorer.xrpChartsUrl);
						stringBuiler.Append ("/#/markets/");
						stringBuiler.Append (baseCurrency);
						stringBuiler.Append ("/");
						stringBuiler.Append (counterCurrency);
						stringBuiler.Append (arguments);

						URLexplorer.OpenUrl (stringBuiler.ToString ());

					};

					menu.Add (rippleChart);
					#endregion

					#region edit
					MenuItem editMenu = new MenuItem ("Edit TradePair");
					editMenu.Show ();

					editMenu.ButtonPressEvent += delegate {
						if (TradePairManagerWindow.CurrentInstance == null) {
							// TODO

							return;
						}
						Task.Run (
							delegate {
								TradePairManagerWindow.CurrentInstance.EditTradePair (null, null);
							}

						);
					};
					menu.Add (editMenu);
					#endregion

					#region delete
					MenuItem deleteMenu = new MenuItem ("Delete TradePair");
					deleteMenu.Show ();

					deleteMenu.ButtonPressEvent += delegate {

						TradePairManager tpm = TradePairManager.currentInstance;
						if (tp == null) {
							return;
						}

						bool sure = AreYouSure.AskQuestion ("Remove TradePair", "Are you sure you would like to remove this tradepair?");
						if (!sure) { // lol
							return;
						}

						tpm.RemoveTradePair (tp);
						tpm.SaveTradePairs ();

						TradePairManagerWindow.CurrentInstance?.UpdateUI ();

						//updateUI ();
					};

					menu.Add (deleteMenu);
					#endregion



					menu.Popup ();
				}


			};

		}

		public void SetValues (IEnumerable<TradePair> tradePairs)
		{



			Application.Invoke (delegate {

				foreach (TradePair tp in tradePairs) {
					string base_cur = tp?.Currency_Base?.currency ?? "";
					string base_issuer = tp?.Currency_Base?.issuer ?? "";

					string counter_cur = tp?.Currency_Counter?.currency ?? "";
					string counter_issuer = tp?.Currency_Counter?.issuer ?? "";

					this.listStore.AppendValues (base_cur, base_issuer, counter_cur, counter_issuer);
				}

				treeview1.Model = listStore;
			}
			);

		}

		public void ClearValues ()
		{
			Application.Invoke ((sender, e) => listStore.Clear ());
		}

		public TradePair GetFromPos (double xx, double yy)
		{
			int x = Convert.ToInt32 (xx);
			int y = Convert.ToInt32 (yy);
			if (!treeview1.GetPathAtPos (x, y, out TreePath path)) {
				return null;
			}

			if (!listStore.GetIter (out TreeIter iter, path)) {
				return null;
			}

			object o = listStore.GetValue (iter, 0);
			object p = listStore.GetValue (iter, 1);
			object q = listStore.GetValue (iter, 2);
			object r = listStore.GetValue (iter, 3);

			return ParseObject (o, p, q, r);

		}



		private TradePair ParseObject (object zer, object one, object two, object three)
		{
#if DEBUG
			String method_sig = clsstr + "parseObject ( zer = " + DebugIhildaWallet.ToAssertString (zer) + ", one = " + DebugIhildaWallet.ToAssertString (one) + ") : ";
#endif
			if (zer == null || two == null) {
#if DEBUG
				if (DebugIhildaWallet.TradePairTree) {
					Logging.WriteLog (method_sig + "one of the currency objects == null");
				}
#endif
				return null;
			}

			RippleCurrency a = RippleCurrency.ParseCurrency (zer, one);
			RippleCurrency b = RippleCurrency.ParseCurrency (two, three);


			if (a == null || b == null) {
#if DEBUG
				if (DebugIhildaWallet.TradePairTree) {
					Logging.WriteLog (method_sig + "a or b == null");
				}
#endif
				return null;
			}

			String key = TradePair.DetermineDictKeyFromCurrencies (a, b);
			if (key != null) {

				//if (PluginController.currentInstance != null) {
				TradePair tp = TradePairManager.currentInstance.LookUpTradePair (key);
#if DEBUG
				if (DebugIhildaWallet.TradePairTree) {
					Logging.WriteLog (method_sig + "Selected TradePair is " + DebugIhildaWallet.ToAssertString (tp));
				}
#endif
				return tp;
				//}

				//else {
				//	if (Debug.WalletTree) {
				//		Logging.write("PluginController.");
				//	}
				//}
			}
#if DEBUG
			if (DebugIhildaWallet.WalletTree) {
				Logging.WriteLog (method_sig + "val == null, returning null");
			}
#endif

			return null;

			/*
			 // removed because it was unreachable
			#if DEBUG
			if (Debug.WalletTree) {
				Logging.writeLog(method_sig + "returning null");
			}
			#endif
			return null;
			*/

		}

		public TradePair GetSelected ()
		{
#if DEBUG
			String method_sig = clsstr + nameof (GetSelected) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.TradePairTree) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			TreeSelection ts = treeview1.Selection;

			if (ts == null) {
#if DEBUG
				if (DebugIhildaWallet.TradePairTree) {
					Logging.WriteLog (method_sig + "Selected item is null");

				}
#endif
				return null;
			}




			if (ts.GetSelected (out TreeModel tm, out TreeIter ti)) {
#if DEBUG
				if (DebugIhildaWallet.TradePairTree) {
					Logging.WriteLog (method_sig + "retrieved value");
				}
#endif

				object o = tm.GetValue (ti, 0);
				object p = tm.GetValue (ti, 1);
				object q = tm.GetValue (ti, 2);
				object r = tm.GetValue (ti, 3);

				return ParseObject (o, p, q, r);
			}
#if DEBUG
			if (DebugIhildaWallet.TradePairTree) {
				Logging.WriteLog (method_sig + "failed to retreive string from UI, returning null");
			}
#endif
			return null;
		}



		private ListStore listStore;

#if DEBUG
		private const string clsstr = nameof (TradePairTree) + DebugRippleLibSharp.colon;
#endif

	}
}

