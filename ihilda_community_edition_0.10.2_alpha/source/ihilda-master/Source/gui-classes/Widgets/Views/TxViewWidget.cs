﻿using System;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Codeplex.Data;


using RippleLibSharp.Transactions;
using RippleLibSharp.Keys;
using RippleLibSharp.Result;
using RippleLibSharp.Commands.Accounts;

using RippleLibSharp.Network;
using IhildaWallet.Networking;

using RippleLibSharp.Nodes;
using RippleLibSharp.Util;
using Gtk;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class TxViewWidget : Gtk.Bin
	{

#if DEBUG
		private const string clsstr = nameof (TxViewWidget) + DebugRippleLibSharp.colon;
#endif

		public TxViewWidget ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (TxViewWidget) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.TxViewWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif
			this.Build ();




			if (ledgerconstraintswidget3 == null) {
				ledgerconstraintswidget3 = new LedgerConstraintsWidget ();
				ledgerconstraintswidget3.Show ();

				expander1.Add (ledgerconstraintswidget3);
			}

			if (txwidget1 == null) {
				txwidget1 = new TxWidget ();
				txwidget1.Show ();

				vbox2.Add (txwidget1);
			}

			if (txwidget2 == null) {
				txwidget2 = new TxWidget ();
				txwidget2.Show ();

				vbox2.Add (txwidget2);
			}

			if (txwidget3 == null) {
				txwidget3 = new TxWidget ();
				txwidget3.Show ();

				vbox2.Add (txwidget3);
			}

			if (txwidget4 == null) {
				txwidget4 = new TxWidget ();
				txwidget4.Show ();

				vbox2.Add (txwidget4);
			}

			if (txwidget5 == null) {
				txwidget5 = new TxWidget ();
				txwidget5.Show ();

				vbox2.Add (txwidget5);
			}

			if (txwidget6 == null) {
				txwidget6 = new TxWidget ();
				txwidget6.Show ();

				vbox2.Add (txwidget6);
			}

			if (txwidget7 == null) {
				txwidget7 = new TxWidget ();
				txwidget7.Show ();

				vbox2.Add (txwidget7);
			}

			if (txwidget8 == null) {
				txwidget8 = new TxWidget ();
				txwidget8.Show ();

				vbox2.Add (txwidget8);
			}

			if (txwidget9 == null) {
				txwidget9 = new TxWidget ();
				txwidget9.Show ();

				vbox2.Add (txwidget9);
			}

			if (txwidget10 == null) {
				txwidget10 = new TxWidget ();
				txwidget10.Show ();

				vbox2.Add (txwidget10);
			}

			if (pagerwidget1 == null) {
				pagerwidget1 = new PagerWidget ();
				pagerwidget1.Show ();
				vbox3.PackEnd (pagerwidget1, false, false,1);
				//vbox3.Add (pagerwidget1);
			}

			vbox2.Show ();
			//scrolledwindow1.AddWithViewport (vbox2);

			this.infoBarLabel.Text = "";
			// show all might show this. Oh well. 
			this.infoBarLabel.Hide ();

			this.ledgerconstraintswidget3.HideTopRow ();

			//this.Visible = true;
#if DEBUG
			if (DebugIhildaWallet.TxViewWidget) {
				Logging.WriteLog (method_sig + DebugIhildaWallet.buildComp);
			}
#endif

			SpinWaitObj = new SpinWait ();
			SpinWaitObj.Hide ();



			pagerwidget1.first.Clicked += (object sender, EventArgs e) => {
#if DEBUG
				string event_sig = method_sig + "first button clicked" + DebugRippleLibSharp.colon;
				if (DebugIhildaWallet.TxViewWidget) {
					Logging.WriteLog (event_sig + DebugRippleLibSharp.begin);
				}
#endif


				Task.Run ((System.Action)FirstClicked);

			};

			pagerwidget1.previous.Clicked += (object sender, EventArgs e) => {
#if DEBUG
				string event_sig = method_sig + "previous button clicked" + DebugRippleLibSharp.colon;
				if (DebugIhildaWallet.TxViewWidget) {
					Logging.WriteLog (event_sig + DebugRippleLibSharp.begin);
				}
#endif

				Task.Run ((System.Action)PreviousClicked);
			};

			pagerwidget1.next.Clicked += (object sender, EventArgs e) => {
#if DEBUG
				string event_sig = method_sig + "next button clicked" + DebugRippleLibSharp.colon;
				if (DebugIhildaWallet.TxViewWidget) {
					Logging.WriteLog (event_sig + DebugRippleLibSharp.begin);
				}
#endif


				Task.Run ((System.Action)NextClicked);

			};

			pagerwidget1.last.Clicked += (object sender, EventArgs e) => {
#if DEBUG
				string event_sig = method_sig + "last button clicked" + DebugRippleLibSharp.colon;
				if (DebugIhildaWallet.TxViewWidget) {
					Logging.WriteLog (event_sig + DebugRippleLibSharp.begin);
				}
#endif

				Task.Run ((System.Action)LastClicked);
			};

			this.syncbutton.Clicked += (object sender, EventArgs e) => {
#if DEBUG
				string event_sig = clsstr + "this.syncbutton.Clicked";
				if (DebugIhildaWallet.TxViewWidget) {
					Logging.WriteLog (event_sig + DebugRippleLibSharp.beginn);
				}
#endif
				string addr = this.AccountEntry.Entry.Text;
#if DEBUG
				if (DebugIhildaWallet.TxViewWidget) {
					Logging.WriteLog (event_sig + nameof (addr) + DebugRippleLibSharp.equals + addr);
				}
#endif
				RippleAddress ra = null;
				try {
					ra = new RippleAddress (addr);
				}

#pragma warning disable 0168
				catch (FormatException fe) {
#pragma warning restore 0168

#if DEBUG
					if (DebugIhildaWallet.TxViewWidget) {
						Logging.ReportException (method_sig, fe);
					}
#endif

					MessageDialog.ShowMessage (addr + " is not a properlay formatted ripple address\n");
					return;
				}

#pragma warning disable 0168
				catch (Exception ex) {
#pragma warning restore 0168

#if DEBUG
					if (DebugIhildaWallet.TxViewWidget) {
						Logging.ReportException (method_sig, ex);
					}
#endif
					MessageDialog.ShowMessage ("Error processing ripple address\n");
					return;
				}



				if (ra == null) {
#if DEBUG
					if (DebugIhildaWallet.TxViewWidget) {
						Logging.WriteLog (event_sig + "ra == null");
					}
#endif
					return;
				}

				//this.spinwait.ShowAll();

				SubscribeParam sp = new SubscribeParam (ra.ToString (), 0);

				ParameterizedThreadStart pst = new ParameterizedThreadStart (Subscribe);
				Thread thr = new Thread (pst);
				thr.Start (sp);

				//subscribe(ra, 0);

			};

		}

		class SubscribeParam
		{
			public SubscribeParam (String addres, int max_num)
			{
				this.address = addres;
				this.max_num = max_num;
			}

			public string address;
			public int max_num;
		}

		public void HideUnusedWidgets ()
		{
			this.ledgerconstraintswidget3.HideTopRow ();
		}

		private void Subscribe (object o)
		{
			// NON GUI thread


#if DEBUG
			string method_sig = clsstr + nameof (Subscribe) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.TxViewWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);

			}
#endif
			try {

				if (!(o is SubscribeParam para)) {
					return;
				}
#if DEBUG
				if (DebugIhildaWallet.TxViewWidget) {
					Logging.WriteLog (method_sig + "para.address=" + para.address);

				}
#endif

				//NetworkInterface ni = NetworkController.getNetworkInterfaceGuiThread();
				NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();

				if (ni == null) {
					NetworkController.DoNetworkingDialogNonGUIThread ();
					return;
				}

				int? min = ledgerconstraintswidget3.GetStartFromLedger ();
				int? max = ledgerconstraintswidget3.GetEndLedger ();
				int? limit = ledgerconstraintswidget3.GetLimit ();
				bool? b = ledgerconstraintswidget3.GetForward ();

				if (min == null && max == null && limit == null) {
					DoFullSync (para.address, ni);
					return;
				}

				Task<Response<AccountTxResult>> tsk =
					AccountTx.GetResult (
						para.address,
						min?.ToString () ?? (-1).ToString (),
						max?.ToString () ?? (-1).ToString (),
						limit ?? 200,
						b ?? false,
						ni);


				tsk.Wait ();

				Response<AccountTxResult> res = tsk.Result;

				AccountTxResult txr = res.result;
				if (txr == null || txr.transactions == null) {
					return;
				}

				SetTransactions (txr.transactions, para.address);
			} catch (Exception e) {

#if DEBUG
				Logging.ReportException (method_sig, e);
#endif

			} finally {
				HideSpinner ();
			}


		}

		private void DoFullSync (string account, NetworkInterface ni)
		{
#if DEBUG
			string method_sig = clsstr + nameof (DoFullSync) + DebugRippleLibSharp.both_parentheses;
#endif
			try {

				Task<IEnumerable<Response<AccountTxResult>>> task =
					AccountTx.GetFullTxResult (account, ni);

				if (task == null) {
					Gtk.Application.Invoke (
						(object sender, EventArgs e) => {
							this.infoBarLabel.Text = "Bug: AccountTx returned null task";
							this.infoBarLabel.Show ();
						}
					);

					return;
				}
				task.Wait ();




				IEnumerable<Response<AccountTxResult>> res = task.Result;

				if (res == null) {
					Gtk.Application.Invoke (
						(object sender, EventArgs e) => {
							this.infoBarLabel.Text = "Bug: AccountTx Task returned null value";
							this.infoBarLabel.Show ();
						}
					);
					return;
				}

				IEnumerable<RippleTxStructure> ie = res.AsParallel ().AsOrdered ().SelectMany (x => x.result.transactions).AsSequential ();
				this.SetTransactions (ie, account);
				/*
				List <RippleTxStructure> list = new List<RippleTxStructure>();

				foreach ( Response<AccountTxResult> response in res ) {

					AccountTxResult txr = response.result;
					if (txr == null || txr.transactions == null) {
						break;
					}

					list.AddRange( txr.transactions );
				}


				this.setTransactions(list.AsEnumerable(), account);
				*/
				Gtk.Application.Invoke (
					delegate {
						this.pagerwidget1.first.Activate (); // causes UI
					}
				);

			} catch (Exception e) {

#if DEBUG
				Logging.ReportException (method_sig, e);
#endif

				Gtk.Application.Invoke (
					(object sender, EventArgs event_args) => {
						this.infoBarLabel.Text = "Bug: AccountTx Task returned null value";
						this.infoBarLabel.Show ();
					}
				);

				StringBuilder sb = new StringBuilder ();
				sb.Append ("Exception thrown : ");
				sb.Append (e?.Message);
				sb.AppendLine ();
				sb.Append (e?.StackTrace);

				MessageDialog.ShowMessage (sb.ToString ());
			} finally {

				HideSpinner ();
			}

		}

		private void HideSpinner ()
		{
			Gtk.Application.Invoke (

				(object sender, EventArgs e) => {
					SpinWaitObj.Hide ();

					this.ledgerconstraintswidget3.HideTopRow ();
				}
			);
		}

		public void SetTransactions (IEnumerable<RippleTxStructure> txstructureArray, string account)
		{
			//Gtk.VBox vb = new Gtk.VBox (false, 10);
#if DEBUG
			string method_sig = clsstr + nameof (SetTransactions) + DebugRippleLibSharp.colon;
			if (DebugIhildaWallet.TxViewWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			if (!txstructureArray.Any ()) {
#if DEBUG
				if (DebugIhildaWallet.TxViewWidget) {
					Logging.WriteLog (method_sig + "txs.Length < 1)");
				}
#endif




				StringBuilder message = new StringBuilder (110); // set to a few bytes larger than the string 104 chars
				message.Append ("<span fgcolor=\"red\"> account ");
				message.Append (account);
				message.Append (" has not been funded with ");
				message.Append (RippleCurrency.NativeCurrency);
				message.Append ("</span>\n");

				string s = message.ToString ();
				Gtk.Application.Invoke (
					delegate {



						this.infoBarLabel.Markup = s;


						this.infoBarLabel.Show ();

					}

				);

				return;
			}




			LinkedList<TxSummary> reports = new LinkedList<TxSummary> ();

			var list = from RippleTxStructure t in txstructureArray where t != null select t;

			foreach (RippleTxStructure txresult in list) {

				Tuple<string, string> rps = txresult.GetReport (account);



				TxSummary txsummary = new TxSummary {
					RawJSON = txresult?.RawJSON,

					Tx_Type = txresult?.tx?.TransactionType?.ToString ()
				};


				AutomatedOrder automatedOrder = null;
				string offercreatesummation = "";
				if (txsummary.Tx_Type != null && txsummary.Tx_Type.Equals ("OfferCreate")) {

					RippleNode node = txresult.meta.GetFilledImmediate (account);

					if (node != null) {

						automatedOrder = AutomatedOrder.ReconstructFromNode (node);
					}

					if (automatedOrder == null) {

						offercreatesummation = "offer filled completely";
					}



				}

				uint? dte = txresult?.tx.date;
				txsummary.Time = dte == null ? 0 : (UInt32)dte;

				txsummary.Tx_id = txresult?.tx?.hash;

				if (rps != null) {
					txsummary.Tx_Summary_Buy = rps?.Item1 ?? "";
					txsummary.Tx_Summary_Sell = rps?.Item2 ?? "";
				}

				txsummary.Tx_Summary_Buy += offercreatesummation;
				txsummary.Tx_Summary_Sell += offercreatesummation;


				Tuple<string, string> orderDescriptionList = txresult.meta.GetChangeDescription (account);
				Tuple<string, string> canceledDescriptionList = txresult.meta.GetCancelDescription (account);

				txsummary.Meta_Summary_Buy += orderDescriptionList.Item1;
				txsummary.Meta_Summary_Sell += orderDescriptionList.Item2;

				txsummary.Meta_Summary_Buy += canceledDescriptionList.Item1;
				txsummary.Meta_Summary_Sell += canceledDescriptionList.Item2;

				LinkedList<OrderChange> oc = txresult.meta.GetOrderChanges (account);
				txsummary.Changes = oc.ToArray ();

				reports.AddLast (txsummary);

			}

			cache.Set (reports.ToArray ());
			int x = cache.GetNumPages;
			this.pagerwidget1.SetNumberOfPages (x);



		}

		public void SetRippleAddress (RippleAddress ra)
		{

			AccountEntry.Entry.Text = ra?.ToString () ?? "";

			this.ClearGui ();
			//this.subscribe(ra, 10);
		}

		public PageCache<TxSummary> cache = new PageCache<TxSummary> ("TxView");

		public void FirstClicked ()
		{

#if DEBUG
			string method_sig = clsstr + nameof (FirstClicked) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.TxViewWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			SetGui (cache.GetfirstCache ());
			pagerwidget1.SetCurrentPage (cache.GetFirst());
			cache.SetFirst ();

			cache.Preload ();
		}

		public void LastClicked ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (LastClicked) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.TxViewWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			SetGui (cache.GetLastCache ());
			pagerwidget1.SetCurrentPage (cache.GetLast ());
			cache.SetLast ();
			cache.Preload ();
		}

		public void PreviousClicked ()
		{
#if DEBUG
			string method_sig = clsstr + nameof(PreviousClicked) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.TxViewWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			if (cache == null) {
				return;
			}

			TxSummary [] trlat = cache.GetPreviousCache ();
			if (trlat != null) {
				SetGui (trlat);

				pagerwidget1.SetCurrentPage (cache.GetPrevious ());
				cache.SetPrevious ();
				cache.Preload ();
			}
		}

		public void NextClicked ()
		{
#if DEBUG
			string method_sig = clsstr + nameof(NextClicked) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.TxViewWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			if (cache == null) {
				// todo debug
				return;
			}
			TxSummary [] trlar = cache.GetNextCache ();
			if (trlar != null) {
				SetGui (trlar);

				pagerwidget1.SetCurrentPage (cache.GetNext ());
				cache.SetNext ();
				cache.Preload ();
			}
		}

		private void ClearGui ()
		{

			Gtk.Application.Invoke ((object sender, EventArgs eventArgs) => {
				this.txwidget1?.Hide ();
				this.txwidget2?.Hide ();
				this.txwidget3?.Hide ();
				this.txwidget4?.Hide ();
				this.txwidget5?.Hide ();
				this.txwidget6?.Hide ();

				this.txwidget7?.Hide ();

				this.txwidget8?.Hide ();

				this.txwidget9?.Hide ();

				this.txwidget10?.Hide ();

				this.infoBarLabel?.Hide ();
			});

		}


		private void SetGui (TxSummary [] txr)
		{
#if DEBUG
			string method_sig = clsstr + nameof (SetGui) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.TxViewWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif
		
			ClearGui ();

			if (txr == null) {
				return;
			}

			Gtk.Application.Invoke ( (object sender, EventArgs e) => {




				for (int i = 0; i < txr.Length; i++) {
#if DEBUG
					if (DebugIhildaWallet.TxViewWidget) {
						Logging.WriteLog (method_sig + "case " + i.ToString () + DebugRippleLibSharp.colon);
					}
#endif
					switch (i) {
					case 0:
						//this.txwidget1.summary 
						this.txwidget1.Summary = txr [i];
						this.txwidget1.Show ();

						continue;
					case 1:
						this.txwidget2.Summary = txr [i];
						this.txwidget2.Show ();
						continue;
					case 2:
						this.txwidget3.Summary = txr [i];
						this.txwidget3.Show ();
						continue;
					case 3:
						this.txwidget4.Summary = txr [i];
						this.txwidget4.Show ();
						continue;
					case 4:
						this.txwidget5.Summary = txr [i];
						this.txwidget5.Show ();
						continue;
					case 5:
						this.txwidget6.Summary = txr [i];
						this.txwidget6.Show ();
						continue;
					case 6:
						this.txwidget7.Summary = txr [i];
						this.txwidget7.Show ();
						continue;
					case 7:
						this.txwidget8.Summary = txr [i];
						this.txwidget8.Show ();
						continue;
					case 8:
						this.txwidget9.Summary = txr [i];
						this.txwidget9.Show ();
						continue;
					case 9:
						this.txwidget10.Summary = txr [i];
						this.txwidget10.Show ();
						continue;
					default:
						break;
					}

				}
			});
		}


		private SpinWait SpinWaitObj {
			get;
			set;

		}



		//private string lastSyncedAddress = null;


	}
}
