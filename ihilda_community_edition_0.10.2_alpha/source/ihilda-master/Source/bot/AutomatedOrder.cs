﻿using System;
using System.Collections.Generic;
using RippleLibSharp.Transactions;

using RippleLibSharp.Transactions.TxTypes;

using RippleLibSharp.Nodes;

using RippleLibSharp.Keys;
using RippleLibSharp.Binary;
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

		public AutomatedOrder (AutomatedOrder o) {
			SetFromOffer (o);
			this.Selected = o.Selected;
			//this.Bot_ID = o.Bot_ID;
		}

		public AutomatedOrder ( Offer o ) {
			SetFromOffer (o);
		}

		private void SetFromOffer (Offer o) {
			this.Account = o.Account;

			this.taker_gets = o.taker_gets.DeepCopy();
			this.taker_pays = o.taker_pays.DeepCopy();



			this.Flags = o.Flags;
			//o.quality = node.FinalFields.  // TODO ??
			//o.BookDirectory = node.FinalFields.BookDirectory; ?
			//o.BookNode = node.

			this.LedgerEntryType = this.LedgerEntryType;
			this.OwnerNode = o.OwnerNode;
			this.PreviousTxnID = o.PreviousTxnID;
			this.PreviousTxnLgrSeq = o.PreviousTxnLgrSeq;
			this.Sequence = o.Sequence;
		}


		public AutomatedOrder[] Split (int num)
		{
			if (num < 0) {
				throw new ArgumentException ();

			}
			AutomatedOrder [] orders = new AutomatedOrder [num];

			for (int i = 0; i < num; i++ ) {
				
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

				AutomatedOrder automatedOrder = new AutomatedOrder ();

				automatedOrder.TakerGets = gets;
				automatedOrder.TakerPays = pays;
				automatedOrder.Account = this.Account;

				orders [i] = automatedOrder;
			}

			return orders;
		}

		//public string Account { get; set; }

		public bool Red {
			get;
			set;
		}

		public bool Selected {
			get;
			set;
		}

		public bool Filled {
			get;
			set;
		}


		public string Bot_ID {
			get { 
				if (Account == null ) {
					return null;
				}
				return Account + Sequence.ToString(); 
			}
			//set { }
		}


		public static AutomatedOrder ReconstructFromNode (RippleNode node) {


			/*
			switch (node.nodeType) {
			case BinaryFieldType.CreatedNode:
				return reconstructFromCreatedNode (node);
			case BinaryFieldType.ModifiedNode:
				return ReconsctructFromModifiedNode (node);
			case BinaryFieldType.DeletedNode:
			}
			*/

			if ( node.nodeType == BinaryFieldType.CreatedNode) {
				return ReconstructFromCreatedNode (node);
			}

			if (node.nodeType == BinaryFieldType.ModifiedNode) {
				return ReconsctructFromModifiedNode (node);
			}

			if (node.nodeType == BinaryFieldType.DeletedNode) {

			}

			throw new NotImplementedException ();

		}


		private static AutomatedOrder ReconsctructFromModifiedNode (RippleNode node) {
			AutomatedOrder o = new AutomatedOrder {
				Account = node.FinalFields.Account,
				TakerGets = node.FinalFields.TakerGets,
				TakerPays = node.FinalFields.TakerPays,

				Flags = node.FinalFields.Flags,

				LedgerEntryType = node.LedgerEntryType,
				OwnerNode = node.FinalFields.OwnerNode,
				PreviousTxnID = node.FinalFields.PreviousTxnID,
				PreviousTxnLgrSeq = node.FinalFields.PreviousTxnLgrSeq,
				Sequence = node.FinalFields.Sequence
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

		private static AutomatedOrder ReconstructFromCreatedNode (RippleNode node) {
			AutomatedOrder o = new AutomatedOrder {
				Account = node.NewFields.Account,
				TakerGets = node.NewFields.TakerGets,
				TakerPays = node.NewFields.TakerPays,

				Flags = node.NewFields.Flags,

				LedgerEntryType = node.LedgerEntryType,
				OwnerNode = node.NewFields.OwnerNode,
				PreviousTxnID = node.NewFields.PreviousTxnID,
				PreviousTxnLgrSeq = node.NewFields.PreviousTxnLgrSeq,
				Sequence = node.NewFields.Sequence
			};

			//o.quality = node.FinalFields.  // TODO ??
			//o.BookDirectory = node.FinalFields.BookDirectory; ?
			//o.BookNode = node.



			return o;
		}


		public static AutomatedOrder ReconsctructFromTransaction ( RippleTransaction tx ) {
			AutomatedOrder ao = new AutomatedOrder {
				Account = tx.Account,
				TakerGets = tx.TakerGets,
				TakerPays = tx.TakerPays,

				flags = tx.flags,

				Sequence = tx.Sequence
			};

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

		public static IEnumerable<AutomatedOrder> ConvertFromIEnumerableOrder ( IEnumerable<Offer> input ) {
			if (input == null) {
				return null;
			}
				



			List<AutomatedOrder> list = new List<AutomatedOrder> ();

			foreach (Offer o in input) {
				list.Add (new AutomatedOrder(o));
			}

			//IEnumerable<AutomatedOrder> ret = list;

			return list;
		}


	}
}
