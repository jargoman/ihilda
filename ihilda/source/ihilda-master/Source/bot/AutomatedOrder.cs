using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RippleLibSharp.Binary;
using RippleLibSharp.Nodes;
using RippleLibSharp.Transactions;
using RippleLibSharp.Transactions.TxTypes;

namespace IhildaWallet
{
	public class AutomatedOrder : Offer
	{
		public AutomatedOrder ()
		{
		}



		/*
		public AutomatedOrder (RippleAddress account, Offer o ) { 




			setFromOffer (o);
			this.Account = Account;

		}
		*/

		public AutomatedOrder (AutomatedOrder o)
		{
			SetFromOffer (o);
			this.Selected = o.Selected;
			this.BotMarking = o.BotMarking;
			//this.Bot_ID = o.Bot_ID;
		}

		public AutomatedOrder (Offer o)
		{
			SetFromOffer (o);
		}

		private void SetFromOffer (Offer o)
		{

			if (o == null) {
				// TODO

				return;
			}
			this.Account = o.Account;

			this.taker_gets = o.taker_gets.DeepCopy ();
			this.taker_pays = o.taker_pays.DeepCopy ();



			this.Flags = o.Flags;
			//o.quality = node.FinalFields.  // TODO ??
			//o.BookDirectory = node.FinalFields.BookDirectory; ?
			//o.BookNode = node.

			this.LedgerEntryType = this.LedgerEntryType;
			this.OwnerNode = o.OwnerNode;
			this.PreviousTxnID = o.PreviousTxnID;
			this.PreviousTxnLgrSeq = o.PreviousTxnLgrSeq;
			this.Sequence = o.Sequence;
			this.Memos = o.Memos; // TODO 

			

		}

		/*
		public void AddClientMemo ()
		{
			AddMemo (Program.GetClientMemo ());
		}

		public void AddMemo (MemoIndice memoIndice)
		{

			if (memoIndice == null) {
				return;
			}
			List<MemoIndice> memos = new List<MemoIndice> ();
			if (this.Memos != null) {
				memos.AddRange (this.Memos);
			}


			memos.Add (memoIndice);
			this.Memos = memos.ToArray ();
		} */


		public override string ToString ()
		{
			StringBuilder stringBuilder = new StringBuilder ();
			//stringBuilder.Append ();
			return base.ToString ();
		}


		public AutomatedOrder [] Split (int num)
		{
			if (num < 0) {
				throw new ArgumentException ();

			}
			AutomatedOrder [] orders = new AutomatedOrder [num];

			for (int i = 0; i < num; i++) {

				RippleCurrency gets = null;
				RippleCurrency pays = null;

				if (taker_gets.IsNative) {
					gets = new RippleCurrency (this.TakerGets.amount / num);
				} else {
					gets = new RippleCurrency (this.TakerGets.amount / num, this.TakerGets.issuer, this.TakerGets.currency);
				}

				if (taker_pays.IsNative) {
					pays = new RippleCurrency (this.TakerPays.amount / num);
				} else {
					pays = new RippleCurrency (this.TakerPays.amount / num, this.taker_pays.issuer, this.taker_pays.currency);
				}

				AutomatedOrder automatedOrder = new AutomatedOrder {
					TakerGets = gets,
					TakerPays = pays,
					Account = this.Account
				};

				orders [i] = automatedOrder;
			}

			return orders;
		}

		//public string Account { get; set; }

		public bool Red {
			get;
			set;
		}

		public bool Selected { get; set; }

		public bool Filled {
			get;
			set;
		}

		public string BotMarking {
			get;
			set;
		}

		public OrderSubmittedEventArgs SubmittedEventArgs {
			get;
			set;
		}




		public string Bot_ID => Account == null ? null : Account + Sequence.ToString ();

		public string Previous_Bot_ID {
			get;
			set;
		}

		public bool Succeeded { get; set; }

		//public bool IsValidated { get; set; }

		public bool IsValidated = false;



		public static AutomatedOrder ReconstructFromNode (RippleNode node)
		{


			/*
			switch (node.nodeType) {
			case BinaryFieldType.CreatedNode:
				return reconstructFromCreatedNode (node);
			case BinaryFieldType.ModifiedNode:
				return ReconsctructFromModifiedNode (node);
			case BinaryFieldType.DeletedNode:
			}
			*/

			if (node.nodeType == BinaryFieldType.CreatedNode) {
				return ReconstructFromCreatedNode (node);
			}

			if (node.nodeType == BinaryFieldType.ModifiedNode) {
				return ReconsctructFromModifiedNode (node);
			}

			if (node.nodeType == BinaryFieldType.DeletedNode) {

			}

			throw new NotImplementedException ();

		}


		private static AutomatedOrder ReconsctructFromModifiedNode (RippleNode node)
		{
			AutomatedOrder o = new AutomatedOrder {
				Account = node.FinalFields.Account,
				TakerGets = node.FinalFields.TakerGets,
				TakerPays = node.FinalFields.TakerPays,

				Flags = node.FinalFields.Flags,

				LedgerEntryType = node.LedgerEntryType,
				OwnerNode = node.FinalFields.OwnerNode,
				PreviousTxnID = node.FinalFields.PreviousTxnID,
				PreviousTxnLgrSeq = node.FinalFields.PreviousTxnLgrSeq,
				Sequence = node.FinalFields.Sequence,
				Memos = node.FinalFields.Memos
			};

			//o.quality = node.FinalFields.  // TODO ??
			//o.BookDirectory = node.FinalFields.BookDirectory; ?
			//o.BookNode = node.



			//o.index =  ??
			//o.taker_gets_funded = node.FinalFields ??
			//o.fl
			//o.Expiration = node.FinalFields ?

			return o;
		}

		private static AutomatedOrder ReconstructFromCreatedNode (RippleNode node)
		{
			if (node == null) {
				return null;
			}

			//node.NewFields.
			IEnumerable<MemoIndice> mems = null;

			if (node?.NewFields?.Memos != null) {
				mems = from MemoIndice memo in node.NewFields.Memos where memo.GetMemoTypeAscii () == "ihildamark" select memo;
			}

			MemoIndice memoIndice = mems?.FirstOrDefault ();

			string mark = memoIndice?.GetMemoDataAscii ();



			AutomatedOrder o = new AutomatedOrder {
				Account = node.NewFields.Account,
				TakerGets = node.NewFields.TakerGets,
				TakerPays = node.NewFields.TakerPays,

				Flags = node.NewFields.Flags,

				LedgerEntryType = node.LedgerEntryType,
				OwnerNode = node.NewFields.OwnerNode,
				PreviousTxnID = node.NewFields.PreviousTxnID,
				PreviousTxnLgrSeq = node.NewFields.PreviousTxnLgrSeq,
				Sequence = node.NewFields.Sequence,
				Memos = node.NewFields.Memos
			};

			if (mark != null) {
				o.BotMarking = mark;
			}


			//o.quality = node.FinalFields.  // TODO ??
			//o.BookDirectory = node.FinalFields.BookDirectory; ?
			//o.BookNode = node.



			return o;
		}


		public static AutomatedOrder ReconsctructFromTransaction (RippleTransaction tx)
		{

			if (tx == null) {
				return null;
			}
			IEnumerable<MemoIndice> mems = null;

			if (tx.Memos != null) {
				mems = from MemoIndice memo in tx.Memos where memo.GetMemoTypeAscii () == "ihildamark" select memo;
			}
			MemoIndice memoIndice = mems?.FirstOrDefault ();

			string mark = memoIndice?.GetMemoDataAscii ();


			AutomatedOrder ao = new AutomatedOrder {
				Account = tx.Account,
				TakerGets = tx.TakerGets,
				TakerPays = tx.TakerPays,
				Memos = tx.Memos,
				flags = tx.flags,
				LedgerEntryType = "OfferCreate", // TODO verify correct
				Sequence = tx.Sequence,
				//BotMarking = mark

			};

			if (mark != null) {
				ao.BotMarking = mark;
			}

			//o.quality = node.FinalFields.  // TODO ??
			//o.BookDirectory = node.FinalFields.BookDirectory; ?
			//o.BookNode = node.

			//o.LedgerEntryType =tx.LedgerEntryType; ??

			//o.OwnerNode = tx.OwnerNode; ??

			//ao.PreviousTxnID = tx.PreviousTxnId;
			//o.PreviousTxnLgrSeq = tx.PreviousTxnLgrSeq;





			//o.index =  ??

			//o.taker_gets_funded = node.FinalFields ??

			//o.fl

			//o.Expiration = node.FinalFields ?

			return ao;
		}

		public static AutomatedOrder GetOpposingOrder (Offer o)
		{
			AutomatedOrder ao = new AutomatedOrder {
				taker_gets = o?.taker_pays?.DeepCopy (),
				taker_pays = o?.taker_gets?.DeepCopy ()
			};

			return ao;
		}

		public static IEnumerable<AutomatedOrder> ConvertFromIEnumerableOrder (IEnumerable<Offer> input)
		{
			if (input == null) {
				return null;
			}



			/*
			List<AutomatedOrder> list = new List<AutomatedOrder> ();

			foreach (Offer o in input) {
				list.Add (new AutomatedOrder (o));
			}
	    		*/
			var list = input.Select ((arg) => new AutomatedOrder (arg));


			return list;
		}


	}
}

