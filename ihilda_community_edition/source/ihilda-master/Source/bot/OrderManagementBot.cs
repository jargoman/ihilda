using System;

using System.Threading.Tasks;
using System.Threading;

using System.Collections.Generic;

using System.Collections.Concurrent;

using Codeplex.Data;

using RippleLibSharp.Transactions;

using RippleLibSharp.Result;
using RippleLibSharp.Nodes;
using System.Linq;

using RippleLibSharp.Commands.Tx;

using RippleLibSharp.Commands.Accounts;

using RippleLibSharp.Transactions.TxTypes;

using RippleLibSharp.Network;

using IhildaWallet.Networking;

using RippleLibSharp.Keys;
using RippleLibSharp.Util;
using System.IO;

namespace IhildaWallet
{
	public class OrderManagementBot
	{
		public OrderManagementBot (RippleWallet rw, NetworkInterface ni)
		{

			this.NetInterface = ni;

			this.Wallet = rw;


			//this.orderDict = new ConcurrentDictionary< string, AutomatedOrder > ();
		}



		public IEnumerable<RippleNode> GetNodesFilledOrderBook (IEnumerable<RippleTxStructure> off)
		{

			#region modified
			LinkedList<RippleNode> nodes = new LinkedList<RippleNode> ();

			IEnumerable<RippleNodeGroup []> aff = from RippleTxStructure st in off
							      select st.meta.AffectedNodes;

			string acct = this.Wallet.GetStoredReceiveAddress ();
			foreach (RippleNodeGroup [] arng in aff) {

				IEnumerable<RippleNode> ar =
					from RippleNodeGroup rng in arng where
										 "Offer" == rng?.DeletedNode?.LedgerEntryType


											 && acct == rng?.DeletedNode?.FinalFields?.Account

										 && rng.DeletedNode.FinalFields.TakerGets != null
										
										 && rng.DeletedNode.PreviousFields?.TakerGets != null
										 && rng.DeletedNode.FinalFields.TakerGets.amount < rng.DeletedNode.PreviousFields.TakerGets.amount

					select rng.GetNode ();

				foreach (RippleNode nd in ar) {
					if (nd != null) {

						nodes.AddLast (nd);
					}
				}




			}
			#endregion

			return nodes;
		}


		public List<AutomatedOrder> GetOrdersFilledImmediately (IEnumerable<RippleTxStructure> off)
		{
			List<AutomatedOrder> ords = new List<AutomatedOrder> ();

			string account = Wallet?.GetStoredReceiveAddress ();
			if (account == null) {
				// TODO
				throw new NullReferenceException ();
			}

			#region created
			IEnumerable<RippleTxStructure> offernews = from RippleTxStructure st in off
					where st?.tx?.Account == account && st.tx.TransactionType == "OfferCreate"
					select st;


			foreach (RippleTxStructure strc in offernews) {

				IEnumerable<RippleNode> cr = 
					from RippleNodeGroup rng in strc.meta.AffectedNodes where 
					                                rng?.CreatedNode?.NewFields?.Account == account && rng.CreatedNode.LedgerEntryType == "Offer"
							     select rng.GetNode ();


				if (!cr.Any ()) {
					AutomatedOrder ao = AutomatedOrder.ReconsctructFromTransaction (strc.tx);
					ords.Add (ao);
				}

			}

			#endregion

			return ords;
		}

		public IEnumerable<AutomatedOrder> UpdateTx (IEnumerable<RippleTxStructure> off)
		{



			//this.SyncOrdersCache (this.Wallet.GetStoredReceiveAddress());

			OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Determining orders filled immediately\n" });
			IEnumerable<AutomatedOrder> filledImmediately = GetOrdersFilledImmediately (off);
			if (filledImmediately == null) {
				filledImmediately = new List<AutomatedOrder> ();
			}



			if (!filledImmediately.Any ()) {
				OnMessage?.Invoke (this, new MessageEventArgs () { Message = "No orders filled immediately\n" });
			}

			OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Determining orders filled from orderbook\n" });
			IEnumerable<AutomatedOrder> filledOrderBook = null;
			IEnumerable<RippleNode> nodes = GetNodesFilledOrderBook (off);
			if (!nodes.Any ()) {
				OnMessage?.Invoke (this, new MessageEventArgs () { Message = "No orders filled from orderbook\n" });
				if (!filledImmediately.Any ()) {
					//OnMessage?.Invoke (this, new MessageEventArgs () { Message = "No orders filled from orderbook\n" });
					return filledImmediately; // returns empty list
				}
			} else {
				OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Tracing nodes to source\n" });
				filledOrderBook = TraceNodesToSource (nodes);

			}

			if (filledOrderBook == null) {
				filledOrderBook = new List<AutomatedOrder> ();
			}




			IEnumerable<AutomatedOrder> total = filledImmediately.Concat (filledOrderBook);

			return total;

			//

		}


		private IEnumerable<AutomatedOrder> TraceNodesToSource (IEnumerable<RippleNode> nodes)
		{
			List<AutomatedOrder> list = new List<AutomatedOrder> ();

			AccountSequenceCache accountSequnceCache = AccountSequenceCache.GetCacheForAccount (this.Wallet.GetStoredReceiveAddress ());

			Dictionary<String, AutomatedOrder> cachedOffers = accountSequnceCache.SequenceCache; //.Load (Wallet.GetStoredReceiveAddress ());

			foreach (RippleNode node in nodes) {
				AutomatedOrder order = null;
				//bool b = this.orderDict.TryRemove ( node.getBotId(), out order );
				bool b = false;

				String bot_id = node?.GetBotId ();


				if (bot_id != null && cachedOffers != null) {
					b = cachedOffers.TryGetValue (bot_id, out order);
				}

				if (b) {
					OnMessage?.Invoke (this, new MessageEventArgs () { Message = bot_id + " found in cache\n" });
					list.Add (order);
					continue;
				}


				if (order == null) {


					// Unfortunately tracing the order can fail due to server not having full ledger history. 
					// Somehow repeated attempts may eventually work. 
					// Far better approad is trying a server with full history

					if (bot_id != null) {
						OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Tracing " + bot_id + " to source\n" });
					} else {
						OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Tracing filled order node to source transaction\n" });
					}

					order = TraceFilledOrderToCreationRobustly (node);


				}

				if (order == null) {
					string errmessg = "Could not retrieve order\n";
					OnMessage?.Invoke (this, new MessageEventArgs () { Message = errmessg });
					throw new NullReferenceException (errmessg);
				}


				OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Updating orders cache : " + order.Bot_ID + "\n" });
				accountSequnceCache.UpdateOrdersCache (order);
				list.Add (order);

			}

			OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Saving orders cache to file\n" });
			accountSequnceCache.Save ();

			return list;
		}


		public IEnumerable<AutomatedOrder> GetBuyBackOrders (IEnumerable<AutomatedOrder> orders)
		{



			RuleManager rulemanager = new RuleManager (this.Wallet.GetStoredReceiveAddress ());

			rulemanager.LoadRules ();


			LinkedList<AutomatedOrder> backs = new LinkedList<AutomatedOrder> ();

			foreach (OrderFilledRule rule in rulemanager.RulesList) {

				if (!rule.IsActive) {
					continue;
				}

				foreach (AutomatedOrder o in orders) {



					if (rule.DetermineMatch (o)) {
						AutomatedOrder ao = Profiteer.GetBuyBack (o, rule.RefillMod);

						string markNext = MarkAsCommand.DoNextMark (o.BotMarking, rule.MarkAs);
						ao.BotMarking = markNext;

						backs.AddLast (ao);
					}
				}

			}


			return backs;



		}

		private AutomatedOrder TraceFilledOrderToCreationRobustly (RippleNode node)
		{


			string account = node.FinalFields.Account;
			uint seq = node.FinalFields.Sequence;

			// we try one method first half the time and the other method is tried first the other half
			// to avoid hogging an info soure and getting banned or disconnected by anti spam anti ddos. 

			AutomatedOrder order = null;


			//LinkedList < RippleNode > failedNodes = new LinkedList < RippleNode > ();

			int attempts = 3;
			for (int i = 0; i < attempts; i++) {

				order = TraceFilledOfferToCreation (node);

				if (order != null) {
					break;
				}

				OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Tracing order to source failed. Trying data api\n" });
				order = LookUpSequenceDataApi (account, seq);
				if (order != null) {
					break;
				}

				Logging.WriteBoth ("sleeping for 1");
				Thread.Sleep (1000);

			}







			return order;
		}

		public AutomatedOrder LookUpSequenceDataApi (string account, uint seq)
		{
			Task<Response<string>> seqTask = tx.GetTxFromAccountAndSequenceDataAPI (account, seq);
			if (seqTask == null) {
				// don't sweat it, we can try other ways to look up the order. 
				return null;
			}

			seqTask.Wait ();

			Response<string> response = seqTask.Result;
			if (response == null) {
				return null;
			}

			var txstruct = response?.transaction;
			//txstruct.tx.
			AutomatedOrder ao = AutomatedOrder.ReconsctructFromTransaction (txstruct?.tx);

			return ao;
		}

		public AutomatedOrder TraceFilledOfferToCreation (RippleNode node)
		{

			if (node == null) {

				return null;
			}

			string targetAccount = node.FinalFields.Account;

			string PreviousTxnID = node.GetPreviousTxnID ();
			uint targetsequence = node.FinalFields.Sequence;

			// Note using the address is not needed since we are using hash's
			object cached = RoboMem.LookupNodeTrace (PreviousTxnID);
			if (cached != null) {

				if (cached is RippleNode) {
					return TraceFilledOfferToCreation ((RippleNode)cached);
				}

				if (cached is AutomatedOrder) {
					return (AutomatedOrder)cached;
				}


				//return cached;
			}


			Task<Response<RippleTransaction>> requestTask = tx.GetRequest (PreviousTxnID, this.NetInterface);

			requestTask.Wait (250);
			while (!requestTask.IsCompleted) {
				OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Waiting on network\n" });
				requestTask.Wait (1000);
			}



			Response<RippleTransaction> response = requestTask.Result;

			RippleTransaction txRes = response.result;



			RippleTxMeta meta = default (RippleTxMeta);
			//RippleTransaction txRes = null;
			if (txRes == null) {

				// instead of giving up we are going to try the data api instead. 
				// response<string is correct>
				//Thread.Sleep (5000);
				Task<Response<string>> dataTask = tx.GetRequestDataApi (PreviousTxnID);
				if (dataTask == null) {
					return null;
				}

				dataTask.Wait (250);
				while (!dataTask.IsCompleted) {
					OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Waiting on network\n" });
					dataTask.Wait (1000);
				}



				Response<string> dataResp = dataTask?.Result;
				RippleTxStructure txStruct = dataResp?.transaction;
				txRes = txStruct?.tx;
				meta = txStruct.meta;
				if (txRes == null) {
					OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Transaction result is null\n" });
					return null;
				}

			} else {
				meta = txRes?.meta;
			}

			if (targetAccount.Equals (txRes.Account) && RippleTransactionType.OFFER_CREATE == txRes.TransactionType) {
				AutomatedOrder ao = AutomatedOrder.ReconsctructFromTransaction (txRes);
				if (ao != null) {
					RoboMem.SetNodeTrace (PreviousTxnID, ao);
				}
				return ao;

			}

			RippleNodeGroup [] nodes = meta?.AffectedNodes;
			if (nodes == null) {
				return null;
			}

			var n = from RippleNodeGroup gr in nodes
				where gr.GetNode ().FinalFields != null
				&& gr.GetNode ().FinalFields.Sequence == targetsequence

				select gr.GetNode ();

			RippleNode p = n.First ();

			if (p != null) {
				RoboMem.SetNodeTrace (PreviousTxnID, p);
			}

			return TraceFilledOfferToCreation (p);

		}


		public AutomatedOrder [] SelfArbatroge (AutomatedOrder [] orders)
		{

			Dictionary<string, LinkedList<AutomatedOrder>> dic = new Dictionary<string, LinkedList<AutomatedOrder>> ();

			foreach (AutomatedOrder ao in orders) {
				string key = ao.TakerPays.currency + ao.TakerPays.currency;
				if (!dic.ContainsKey (key)) {
					dic.Add (key, new LinkedList<AutomatedOrder> ());
				}


				dic.TryGetValue (key, out LinkedList<AutomatedOrder> sublist);

				sublist.AddLast (ao);

			}

			return null;

		}


		public event EventHandler<MessageEventArgs> OnMessage;

		/*
		public class MessageEventArgs : EventArgs
		{

			public string Message {
				get;
				set;
			}
		}
		*/




		/*public static int latestLedger = 0;*/
		//public ConcurrentDictionary < string, AutomatedOrder > orderDict = null;




		//private string 
#pragma warning disable RECS0122 // Initializing field with default value is redundant
		//private static string settingsPath = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

#if DEBUG
		private const string clsstr = nameof (OrderManagementBot) + DebugRippleLibSharp.colon;
#endif






		NetworkInterface NetInterface {
			get;
			set;
		}

		private RippleWallet Wallet {
			get;
			set;
		}




	}



}

