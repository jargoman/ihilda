﻿using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Keys;
using RippleLibSharp.Network;

using IhildaWallet.Networking;

using RippleLibSharp.Result;
using RippleLibSharp.Commands.Accounts;

using RippleLibSharp.Transactions;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public class Robotics
	{
		public Robotics (RuleManager rules)
		{

			this.RuleManagerObj = rules;

		}

		public Tuple <Int32?, IEnumerable <AutomatedOrder>> DoLogic (RippleWallet wallet, NetworkInterface ni, Int32? ledgerstart, Int32? ledgerend, Int32? limit)
		{
#if DEBUG
			string method_sig = clsstr + nameof (DoLogic) + DebugRippleLibSharp.both_parentheses;
#endif
				
			string ledgerMax = ledgerend?.ToString() ?? (-1).ToString ();
			string ledgerMin = ledgerstart?.ToString();
			if (ledgerMin == null) {
				
				if (RuleManagerObj?.LastKnownLedger == null) {
					// TODO
				}
				int lastRuleLedger = ((int)(RuleManagerObj?.LastKnownLedger));
				ledgerMin = lastRuleLedger.ToString();

			}
			int lim = limit ?? 200;

			Task<Response<AccountTxResult>> task = null;

			try {
				task =
					AccountTx.GetResult (
						wallet.GetStoredReceiveAddress (),
						ledgerMin,
						ledgerMax,
						lim,
						true,
						ni);
				if (task == null) {
					//return null;
					throw new NullReferenceException ();
				}


				task.Wait ();

			} catch (Exception e) {
				Logging.WriteBoth (e.Message);
				MessageDialog.ShowMessage ("Network exception");

				return null;
			}

			Response<AccountTxResult> res = task.Result;

			if (res == null) {
				return null;
			}



			AccountTxResult accTxResult = res.result;

			if (accTxResult == null) {
				return null;
			}


#if DEBUG

			string debug =
				"ledgermax" +
				accTxResult.ledger_index_max.ToString () +

				"ledgermin" +
				accTxResult.ledger_index_min.ToString ();

			Logging.WriteLog (debug);

#endif

			RippleTxStructure [] txs = accTxResult.transactions;

			OrderManagementBot omb = new OrderManagementBot (wallet, ni);



			IEnumerable<AutomatedOrder> total = null;

			try {

				total = omb.UpdateTx (txs);
			}

#pragma warning disable 0168
			catch (Exception e) {
#pragma warning restore 0168

#if DEBUG
				if (DebugIhildaWallet.Robotics) {
					Logging.ReportException (method_sig, e);
				}
#endif
				//MessageDialog.showMessage ("Exception processing tx's");
				MessageDialog.ShowMessage ("Exception processing tx's\n" + e.ToString () + e.StackTrace);
				return null;
			}


			if (total == null) {
				MessageDialog.ShowMessage ("Error processing tx's\n" + " total == null\n");
				return null;
			}

			//updateFilledOrders (total);

			IEnumerable<AutomatedOrder> orders = null;

			try {
				orders = omb.GetBuyBackOrders (total);

#pragma warning disable 0168
			} catch (Exception e) {
#pragma warning restore 0168

#if DEBUG
				if (DebugIhildaWallet.Robotics) {
					Logging.ReportException (method_sig, e);
				}
#endif

				StringBuilder message = new StringBuilder ();
				message.Append ("Exception calculating buyorders...\nMessage : \n");
				message.Append (e.Message);
				message.Append ("\n");
				message.Append ("Stakctrace : \n");
				message.Append (e.StackTrace);
				MessageDialog.ShowMessage (message.ToString ());
				return null;
			}



			RuleManagerObj.LastKnownLedger = accTxResult.ledger_index_max;
			RuleManagerObj.SaveRules ();

			Tuple<Int32?, IEnumerable <AutomatedOrder>> tuple = new Tuple<int?, IEnumerable <AutomatedOrder>> (accTxResult.ledger_index_max, orders);

			return tuple;


		}

		/*
		public void LaunchPreviewWidget (IEnumerable<AutomatedOrder> orders)
		{
			Gtk.Application.Invoke ( (object sender, EventArgs e) => {




			});
		}
		*/

		public RuleManager RuleManagerObj {
			get;
			set;
		}


#if DEBUG
		private const string clsstr = nameof (Robotics) + DebugRippleLibSharp.colon;
#endif




	}


}
