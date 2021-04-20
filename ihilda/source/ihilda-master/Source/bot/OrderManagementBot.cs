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
		public OrderManagementBot (RippleWallet rw, NetworkInterface ni, CancellationToken token)
		{

			this.NetInterface = ni;

			this.Wallet = rw;

			this.token = token;

			this._AccountSequnceCache = AccountSequenceCache.GetCacheForAccount (rw.GetStoredReceiveAddress ());

		
		}


		private readonly CancellationToken token;
		public IEnumerable<RippleNode> GetNodesFilledOrderBook (IEnumerable<RippleTxStructure> off)
		{

			string acct = this.Wallet.GetStoredReceiveAddress ();


			if (!ProgramVariables.preferLinq) {

				#region modified
				List<RippleNode> nodes = new List<RippleNode> ();

				IEnumerable<RippleNodeGroup []> aff = from RippleTxStructure st in off
								      select st.meta.AffectedNodes;


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

							nodes.Add (nd);
						}
					}




				}
				#endregion

				return nodes;

			} else {
				var nodes = off
				    .Where ((RippleTxStructure arg) => arg.meta.AffectedNodes != null)
				.SelectMany ((RippleTxStructure arg) => arg.meta.AffectedNodes)
				    .Where ((RippleNodeGroup rng) =>
					     "Offer" == rng?.DeletedNode?.LedgerEntryType &&
						acct == rng?.DeletedNode?.FinalFields?.Account &&
						     rng.DeletedNode.FinalFields.TakerGets != null &&
						    rng.DeletedNode.PreviousFields?.TakerGets != null &&
						rng.DeletedNode.FinalFields.TakerGets.amount < rng.DeletedNode.PreviousFields.TakerGets.amount
				)
				    .Select ((RippleNodeGroup arg) => arg.GetNode ());

				return nodes;
			}
			//return nodes;
		}


		public IEnumerable<AutomatedOrder> GetOrdersFilledImmediately (IEnumerable<RippleTxStructure> off)
		{
			//List<AutomatedOrder> ords = new List<AutomatedOrder> ();

			string account = Wallet?.GetStoredReceiveAddress ();
			if (account == null) {
				// TODO
				throw new NullReferenceException ();
			}


			if (!ProgramVariables.preferLinq) {

				#region created

				List<AutomatedOrder> ords = new List<AutomatedOrder> (); 

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
				return ords;
				#endregion

			} else {

				var ords = off
				.Where ((RippleTxStructure arg) => arg?.tx?.Account == account && arg.tx.TransactionType == "OfferCreate")
				.Where ((RippleTxStructure arg) => {
					IEnumerable<RippleNode> cr =
					       from RippleNodeGroup rng in arg.meta.AffectedNodes where
						       rng?.CreatedNode?.NewFields?.Account == account
							       && rng.CreatedNode.LedgerEntryType == "Offer"
					       select rng.GetNode ();
					return !cr.Any ();
				})
				.Select ((RippleTxStructure arg) => AutomatedOrder.ReconsctructFromTransaction (arg.tx));

				return ords;
			}
			//return ords;
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



			Dictionary<String, AutomatedOrder> cachedOffers = _AccountSequnceCache?.SequenceCache; //.Load (Wallet.GetStoredReceiveAddress ());

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

				if (_AccountSequnceCache != null) {
					OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Updating orders cache : " + order.Bot_ID + "\n" });
					_AccountSequnceCache.UpdateOrdersCache (order);

				}

				list.Add (order);

			}

			if (_AccountSequnceCache != null) {
				OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Saving orders cache to file\n" });
				_AccountSequnceCache.Save ();
			}
			return list;
		}


		public IEnumerable<AutomatedOrder> GetBuyBackOrders (IEnumerable<AutomatedOrder> orders)
		{

			string account = this.Wallet.GetStoredReceiveAddress ();

			RuleManager rulemanager = new RuleManager (account);

			rulemanager.LoadRules ();
			var lis = rulemanager.RulesList;

			SentimentManager sentimentManager = new SentimentManager (account);
			sentimentManager.LoadSentiments ();

			if (!ProgramVariables.preferLinq) {

				List<AutomatedOrder> backs = new List<AutomatedOrder> ();

				foreach (OrderFilledRule rule in lis) {

					if (!rule.IsActive) {
						continue;
					}

					foreach (AutomatedOrder o in orders) {



						if (rule.DetermineMatch (o)) {
							AutomatedOrder ao = Profiteer.GetBuyBack (o, rule.RefillMod, sentimentManager);

							string markNext = MarkAsCommand.DoNextMark (o.BotMarking, rule.MarkAs);
							ao.BotMarking = markNext;

							ao.Previous_Bot_ID = o.Bot_ID;

							backs.Add (ao);
						}
					}

				}

				return backs;

			} else {

				var backs = lis.Where ((OrderFilledRule arg) => arg.IsActive).SelectMany ((arg) =>
						 orders.Where (arg.DetermineMatch).Select ((arg2) => {
							 AutomatedOrder ao = Profiteer.GetBuyBack (arg2, arg.RefillMod, sentimentManager);

						//string markNext = );
						ao.BotMarking = MarkAsCommand.DoNextMark (arg2.BotMarking, arg.MarkAs);

							 ao.Previous_Bot_ID = arg2.Bot_ID;

							 return ao;
						 })
					);

				return backs;
			}

			



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
			Task<Response<string>> seqTask = tx.GetTxFromAccountAndSequenceDataAPI (account, seq, token);
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

#if DEBUG
			string method_sig = clsstr + nameof (TraceFilledOfferToCreation) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.OrderManagementBot) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

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


			Task<Response<RippleTransaction>> requestTask = tx.GetRequest (PreviousTxnID, this.NetInterface, token);

			requestTask.Wait (250, token);

			while (requestTask != null && !requestTask.IsCompleted && !requestTask.IsCanceled && !requestTask.IsFaulted) {
				try {
					if (!requestTask.IsCompleted) OnMessage?.Invoke ( this, new MessageEventArgs () { Message = "Waiting on network" });
					for (int i = 0; i < 10 && !requestTask.IsCompleted && !requestTask.IsCanceled && !requestTask.IsFaulted; i++) {
						OnMessage?.Invoke ( this, new MessageEventArgs () { Message = "." });
						requestTask.Wait (1000, token);
					}

				} catch (Exception e) {
#if DEBUG
					Logging.ReportException (method_sig, e);
#endif
				}

				finally {
					OnMessage?.Invoke ( this, new MessageEventArgs () { Message = "\n" });
				}


			}

			if (requestTask.Result == null) {
				return null; 
			}

			Response<RippleTransaction> response = requestTask.Result;

			RippleTransaction txRes = response.result;



			RippleTxMeta meta = default (RippleTxMeta);
			//RippleTransaction txRes = null;
			if (txRes == null) {

				// instead of giving up we are going to try the data api instead. 
				// response<string is correct>
				//Thread.Sleep (5000);
				Task<Response<string>> dataTask = tx.GetRequestDataApi (PreviousTxnID, token);
				if (dataTask == null) {
					return null;
				}

				dataTask.Wait (250, token);
				while (dataTask != null && !dataTask.IsCompleted && !dataTask.IsFaulted && !dataTask.IsCompleted) {
					try {
						OnMessage?.Invoke (this, new MessageEventArgs () { Message = "Waiting on network" });
						for (int i = 0; i < 10 && dataTask != null && !dataTask.IsCompleted && !dataTask.IsFaulted && !dataTask.IsCompleted; i++) {
							OnMessage?.Invoke (this, new MessageEventArgs () { Message = "." });
							dataTask.Wait (1000, token);
						}
					} catch (Exception e) {

#if DEBUG
						Logging.ReportException (method_sig, e);
#endif

					} finally {
						OnMessage?.Invoke (this, new MessageEventArgs () { Message = "\n" });
					}
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

			IEnumerable<RippleNode> n = from RippleNodeGroup gr in nodes
				where gr.GetNode ().FinalFields != null
				&& gr.GetNode ().FinalFields.Sequence == targetsequence

				select gr.GetNode ();

			RippleNode p = n.FirstOrDefault ();

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


		private AccountSequenceCache _AccountSequnceCache {
			get;
			set;
		}

	}



}

