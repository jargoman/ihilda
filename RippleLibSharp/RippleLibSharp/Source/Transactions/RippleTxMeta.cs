using System;
using System.Collections.Generic;
using System.Text;
using RippleLibSharp.Nodes;
using RippleLibSharp.Keys;

using System.Linq;
namespace RippleLibSharp.Transactions
{
	public class RippleTxMeta
	{
		/*
		public RippleTxMeta ()
		{
		}
		*/

		public RippleNodeGroup[] AffectedNodes {
			get;
			set;
		}

		/* int*/
		public UInt32 TransactionIndex {
			get;
			set;
		}
		public string TransactionResult {
			get;
			set;
		}

#pragma warning disable IDE1006 // Naming Styles
		public RippleCurrency delivered_amount { get; set; }
#pragma warning restore IDE1006 // Naming Styles

		public RippleNode GetFilledImmediate ( RippleAddress account ) {
			
			IEnumerable< RippleNode > nodes = from RippleNodeGroup rng in this.AffectedNodes
			        where rng.CreatedNode != null 
				&& rng.CreatedNode.NewFields != null
				&& rng.CreatedNode.NewFields.Account != null
			        && rng.CreatedNode.NewFields.Account.Equals (account)
				&& rng.CreatedNode.LedgerEntryType != null
				&& rng.CreatedNode.LedgerEntryType.Equals("Offer")
			        select rng.GetNode ();



			return nodes.FirstOrDefault ();
		}

		public Tuple<string,string> GetCancelDescription ( RippleAddress account) {
			StringBuilder b = new StringBuilder ();
			StringBuilder s = new StringBuilder ();


			LinkedList <OrderChange> oc = GetCanceledTx (account);

			foreach ( OrderChange d in oc) {
				b.AppendLine ( d.BuyOrderChange );
				s.AppendLine (d.SellOrderChange);
			}


			return new Tuple<string, string> (b.ToString(), s.ToString());
		}


		public Tuple<string,string> GetChangeDescription (RippleAddress account) {
			
			StringBuilder b = new StringBuilder ();
			StringBuilder s = new StringBuilder ();

			LinkedList<OrderChange> cl = GetOrderChanges ( account );

			foreach (OrderChange d in cl) {
				
				b.AppendLine ( d.BuyOrderChange );
				s.AppendLine (d.SellOrderChange);
			}


			return new Tuple<string, string> (b.ToString(), s.ToString());
		}


		public LinkedList<OrderChange> GetOrderChanges ( RippleAddress account ) {
			

			LinkedList<OrderChange> lis = new LinkedList<OrderChange> ();

			foreach (RippleNodeGroup rng in AffectedNodes) {
				RippleNode rn = rng.GetNode ();

				OrderChange res = rn.GetOfferChange(account);

				if (res != null) {
					lis.AddLast (res);
				}
			}
			return lis;
		}


		public LinkedList<OrderChange> GetCanceledTx (RippleAddress account) {
			LinkedList<OrderChange> lis = new LinkedList < OrderChange > ();
			foreach (RippleNodeGroup rng in AffectedNodes) {
				RippleNode rn = rng.GetNode ();

				OrderChange res = rn.GetCanceledTx (account);

				if (res != null) {
					lis.AddLast (res);
				}
			}
			return lis;
		
		
		}






	}
}

