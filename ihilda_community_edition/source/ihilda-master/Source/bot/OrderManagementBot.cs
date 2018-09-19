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

namespace IhildaWallet
{
	public class OrderManagementBot
	{
		public OrderManagementBot (RippleWallet rw, NetworkInterface ni)
		{

			this.NetInterface = ni;

			this.Wallet = rw;

			this.settingsPath = FileHelper.GetSettingsPath ( rw.ToString () + settingsFileName );

			//this.orderDict = new ConcurrentDictionary< string, AutomatedOrder > ();
		}



		public IEnumerable< RippleNode > GetNodesFilledOrderBook ( IEnumerable <RippleTxStructure> off ) {

			#region modified
			LinkedList<RippleNode> nodes = new LinkedList<RippleNode> ();

			IEnumerable<RippleNodeGroup[]> aff  = from RippleTxStructure st in off
				select st.meta.AffectedNodes;

			string acct = this.Wallet.GetStoredReceiveAddress ();
			foreach ( RippleNodeGroup[] arng in aff ) {
				
				IEnumerable<RippleNode> ar = from RippleNodeGroup rng in arng
						where  
					"Offer".Equals (rng?.DeletedNode?.LedgerEntryType)
				
					
				        && acct.Equals (rng?.DeletedNode?.FinalFields?.Account)

					&& rng.DeletedNode.FinalFields.TakerGets != null
					&& rng.DeletedNode.PreviousFields != null
					&& rng.DeletedNode.PreviousFields.TakerGets != null
					&& rng.DeletedNode.FinalFields.TakerGets.amount < rng.DeletedNode.PreviousFields.TakerGets.amount

					select rng.GetNode();

				foreach (RippleNode nd in ar) {
					if (nd != null) {

						nodes.AddLast (nd);
					}
				}




			}
			#endregion

			return nodes;
		}


		public LinkedList <AutomatedOrder> GetOrdersFilledImmediately ( IEnumerable <RippleTxStructure> off ) {
			LinkedList <AutomatedOrder> ords = new LinkedList<AutomatedOrder>();

			#region created
			IEnumerable<RippleTxStructure> offernews  = from RippleTxStructure st in off

					where st.tx.Account != null
				&& st.tx.Account.Equals(Wallet.GetStoredReceiveAddress())
				&& st.tx.TransactionType != null
				&& st.tx.TransactionType.Equals("OfferCreate")

				select st;


			foreach (RippleTxStructure strc in offernews) {

				IEnumerable<RippleNode> cr = from RippleNodeGroup rng in strc.meta.AffectedNodes
						where rng.CreatedNode != null
					&& rng.CreatedNode.NewFields != null
					&& rng.CreatedNode.NewFields.Account != null
					&& rng.CreatedNode.NewFields.Account.Equals (Wallet.GetStoredReceiveAddress())
					&& rng.CreatedNode.LedgerEntryType != null
					&& rng.CreatedNode.LedgerEntryType.Equals ("Offer")
					select rng.GetNode ();

				RippleNode no = cr.FirstOrDefault();

				if (no == null) {
					AutomatedOrder ao = AutomatedOrder.ReconsctructFromTransaction(strc.tx);
					ords.AddLast(ao);
				}

			}

			#endregion

			return ords;
		}

		public IEnumerable< AutomatedOrder > UpdateTx ( IEnumerable<RippleTxStructure> off ) {



			//this.SyncOrdersCache (this.Wallet.GetStoredReceiveAddress());

			IEnumerable <AutomatedOrder> filledImmediately = GetOrdersFilledImmediately (off);



			IEnumerable <RippleNode> nodes = GetNodesFilledOrderBook (off);

			IEnumerable <AutomatedOrder> filledOrderBook = TraceNodesToSource (nodes);

			IEnumerable <AutomatedOrder> total = filledImmediately.Concat (filledOrderBook);


			return total;

			//

		}


		private IEnumerable <AutomatedOrder> TraceNodesToSource (IEnumerable <RippleNode> nodes) {
			List< AutomatedOrder > list = new List<AutomatedOrder> ();

			Dictionary<String, AutomatedOrder> cachedOffers = Load ();

			foreach ( RippleNode node in nodes ) {
				AutomatedOrder order = null;
				//bool b = this.orderDict.TryRemove ( node.getBotId(), out order );
				bool b = false;

				String bot_id = node?.GetBotId ();


				if (bot_id != null && cachedOffers != null) { 
					b = cachedOffers.TryGetValue (bot_id, out order); 
				}


				if ( !b || order == null ) {


					// Unfortunately tracing the order can fail due to server not having full ledger history. 
					// Somehow repeated attempts may eventually work. 
					// Far better approad is trying a server with full history


					order = TraceFilledOrderToCreationRobustly (node);


				}

				list.Add( order );

			}

			return list;
		}


		public IEnumerable<AutomatedOrder> GetBuyBackOrders ( IEnumerable< AutomatedOrder> orders) {


	
			RuleManager rulemanager = new RuleManager ( this.Wallet.GetStoredReceiveAddress() );

			rulemanager.LoadRules ();


			LinkedList<AutomatedOrder> backs = new LinkedList <AutomatedOrder>(); 

			foreach (OrderFilledRule rule in rulemanager.RulesList) {
				
				if (!rule.IsActive) {
					continue;
				}

				foreach ( AutomatedOrder o in orders ) {
				


					if ( rule.DetermineMatch( o ) ) {
						AutomatedOrder ao = Profiteer.GetBuyBack ( o, rule.RefillMod );

						string markNext = MarkAsCommand.DoNextMark (o.BotMarking, rule.MarkAs);
						ao.BotMarking = markNext;

						backs.AddLast (ao);
					}
				}	
			
			}


			return backs;



		}

		private AutomatedOrder TraceFilledOrderToCreationRobustly ( RippleNode node ) {
			//LinkedList < RippleNode > failedNodes = new LinkedList < RippleNode > ();
			AutomatedOrder order = null;
			int attempts = 3;
			for ( int i = 0; i < attempts; i++ ) {

				order = TraceFilledOfferToCreation(node);

				if (order != null) {
					break;
				}
				Logging.WriteBoth ("sleeping for 1");
				Thread.Sleep ( 1000 );
			}

			return order;

			//failedNodes.AddLast(node);

		}

		public AutomatedOrder TraceFilledOfferToCreation (RippleNode node) {
			
			if (node == null) {
				
				return null;
			}

			string targetAccount = node.FinalFields.Account;

			string PreviousTxnID = node.GetPreviousTxnID ();
			uint targetsequence = node.FinalFields.Sequence;

			// Note using the address is not needed since we are using hash's
			object cached = RoboMem.LookupNodeTrace (PreviousTxnID);
			if (cached != null) {

				if (cached is RippleNode ) {
					return TraceFilledOfferToCreation ((RippleNode)cached);
				}

				if (cached is AutomatedOrder) {
					return (AutomatedOrder)cached;
				}


				//return cached;
			}


			Task<Response<RippleTransaction>> requestTask = tx.GetRequest( PreviousTxnID, this.NetInterface );


			requestTask.Wait ();

			Response<RippleTransaction> response = requestTask.Result;

			RippleTransaction txRes = response.result;

			

			RippleTxMeta meta = default(RippleTxMeta);
			//RippleTransaction txRes = null;
			if (txRes == null) {

				// instead of giving up we are going to try the data api instead. 
				// response<string is correct>
				Task<Response<string>> dataTask = tx.GetRequestDataApi (PreviousTxnID);
				dataTask.Wait ();

				Response<string> dataResp = dataTask?.Result;
				RippleTxStructure txStruct = dataResp?.transaction;
				txRes = txStruct?.tx;
				meta = txStruct.meta;
				if (txRes == null) {
					return null;
				}

			} else {
				meta = txRes?.meta;
			}

			if ( targetAccount.Equals( txRes.Account ) && RippleTransactionType.OFFER_CREATE == txRes.TransactionType ) {
				AutomatedOrder ao = AutomatedOrder.ReconsctructFromTransaction (txRes);
				if (ao != null) {
					RoboMem.SetNodeTrace(PreviousTxnID, ao);
				}
				return ao;

			}

			RippleNodeGroup[] nodes = meta?.AffectedNodes;
			if (nodes == null) {
				return null;
			}

			var n = from RippleNodeGroup gr in nodes
			        where gr.GetNode().FinalFields != null 
					&& gr.GetNode().FinalFields.Sequence == targetsequence
				
			        select gr.GetNode ();

			RippleNode p = n.First ();

			if (p != null) {
				RoboMem.SetNodeTrace (PreviousTxnID, p);
			}

			return TraceFilledOfferToCreation (p);

		}


		public AutomatedOrder[] SelfArbatroge ( AutomatedOrder[] orders ) {

			Dictionary< string, LinkedList<AutomatedOrder> > dic = new Dictionary< string, LinkedList< AutomatedOrder >> ();

			foreach (AutomatedOrder ao in orders) {
				string key = ao.TakerPays.currency + ao.TakerPays.currency;
				if (!dic.ContainsKey (key)) {
					dic.Add (key, new LinkedList< AutomatedOrder>() );
				}


				dic.TryGetValue (key, out LinkedList<AutomatedOrder> sublist);

				sublist.AddLast (ao);

			}

			return null;

		}


		public void SubmmitOrders ( IEnumerable<AutomatedOrder> orders ) {

			foreach (AutomatedOrder ao in orders) {


				RippleOfferTransaction tx = new RippleOfferTransaction(ao.Account, ao);


				tx.AutoRequestSequence (ao.Account, this.NetInterface);

				Tuple<UInt32,UInt32> f = FeeSettings.GetFeeAndLastLedgerFromSettings (this.NetInterface);
				if (f == null) {
					return;
				}
				tx.fee = f.Item1.ToString ();

				tx.LastLedgerSequence = f.Item2;

				RippleIdentifier rsa = this.Wallet.GetDecryptedSeed ();

				tx.Sign(rsa);

				Task< Response <RippleSubmitTxResult>> task = NetworkController.UiTxNetworkSubmit (tx, this.NetInterface);
				task.Wait ();


			}
		}

		public void SyncOrdersCache (string account) {
			if (account == null) {
				return;
			}

			if (this.NetInterface == null) {
				return;
			}

			Task<IEnumerable <Response<AccountOffersResult>>> task = AccountOffers.GetFullOfferList (account, this.NetInterface);
			task.Wait ();

			//List<Offer> orders = new List<Offer> ();
			IEnumerable <Response<AccountOffersResult>> responses = task.Result;

			/*
			IEnumerable< IEnumerable<AutomatedOrder> > offs_list = 
			                where 
				selectmany (from AutomatedOrder off in res.result.offers
					select new AutomatedOrder(off));
			*/

			IEnumerable <AutomatedOrder> offs_list = 
				responses.Where (res => res?.result?.offers != null)
					.SelectMany(  
						x => x.result.offers
						.Select( y => new AutomatedOrder(y) )   
					);

			if (!offs_list.Any ()) {
				return;
			}

			UpdateOrdersCache (offs_list);
			/*  linq obsoleted code delete after testing linq
			foreach (var offls in offs_list) {
				orders.AddRange (offls);
			}


			if (orders == null) {
				return;
			}
			*/

			//IEnumerable<AutomatedOrder> aorders = AutomatedOrder.convertFromIEnumerableOrder (/*account,*/ orders);
			//foreach (AutomatedOrder offer in orders) {
			//	aorders.Add ( new AutomatedOrder (account, offer));
			//}
			//updateOrdersCache (aorders.AsEnumerable());



		}

		public void UpdateOrdersCache (IEnumerable<AutomatedOrder> orders) {
			 
			if (orders == null) {
				return;
			}

			if (!orders.Any ()) {
				return;
			}



			Dictionary<string, AutomatedOrder> dict = Load ();
			if (dict == null) {
				dict = new Dictionary<string, AutomatedOrder> (orders.Count ());
			}

			/*
			if ( dict.Count > 0) {
				return;
			}
			*/

			foreach (AutomatedOrder order in orders) {

				string id = order.Bot_ID;

				if ( dict.ContainsKey (id)) {
					continue;
				}

				dict.Add (id, order);
			}



			this.Save ( dict.Values );

		}

		public void Save (IEnumerable<AutomatedOrder> offers) {
			//ConfStruct confstruct = new ConfStruct( orderDict.Values );
			ConfStruct confstruct = new ConfStruct( offers );

			string conf = DynamicJson.Serialize (confstruct);

			FileHelper.SaveConfig (settingsPath, conf);
		}

		public Dictionary<String, AutomatedOrder> Load () {



			string str = FileHelper.GetJsonConf (settingsPath);

			if (str == null) {
				return null;
			}
			ConfStruct jsconf = null;
			try {
				jsconf = DynamicJson.Parse (str);

			}

			catch (Exception e) {
				Logging.WriteLog (e.Message + e.StackTrace);
				return null;
			}

			if (jsconf == null) {
				return null;
			}

			Offer[] ords = jsconf.Orders;

			Dictionary< String, AutomatedOrder > orderDict = new Dictionary< string, AutomatedOrder > ( ords.Length );

			//orderDict.Clear ();
			foreach ( AutomatedOrder order in ords) {
				string key = order.Bot_ID;

				if ( key != null) {
					//orderDict.TryAdd (key, order);
					orderDict.Add(key, order);
				}
			}

			return orderDict;

		}

		/*public static int latestLedger = 0;*/
		//public ConcurrentDictionary < string, AutomatedOrder > orderDict = null;

		public const string settingsFileName = "OrderManagementBot.jsn";


		//private string 
#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private string settingsPath = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

#if DEBUG
		private const string clsstr = nameof (OrderManagementBot) + DebugRippleLibSharp.colon;
		#endif





		private class ConfStruct
		{
			public ConfStruct ( IEnumerable<AutomatedOrder> ords ) {

				int count = ords.Count();

				var it = ords.GetEnumerator();



				this.Orders = new AutomatedOrder[ count ];

				for (int i = 0; i < count; i++) {
					it.MoveNext();
					Orders[i] = it.Current;

				}

			}

			public ConfStruct () {

			}


			public AutomatedOrder[] Orders {
				get;
				set;
			}

			/*
			public int latestLedger {
				get;
				set;
			}
			*/

		}

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

