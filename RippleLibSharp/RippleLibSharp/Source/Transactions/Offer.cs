using System;

using System.Collections.Generic;
using System.Text;
using RippleLibSharp.Util;
using RippleLibSharp.Nodes;

using RippleLibSharp.Transactions.TxTypes;
using RippleLibSharp.Result;

namespace RippleLibSharp.Transactions
{
	public class Offer : IRawJsonInterface
	{
		/*
		public Offer ()
		{
			
			
		}
		*/


		public string Account {
			get;
			set;
		}

		public string RawJSON {
			get;
			set;
		}



		public RippleCurrency TakerGets { 
			get { 
				return _tgets; }
			set { _tgets = value; }
		}
		public RippleCurrency TakerPays { 
			get { return _tpays; }
			set { _tpays = value; }
		}

#pragma warning disable IDE1006 // Naming Styles
		public RippleCurrency taker_gets {

			get { return _tgets; }
			set { _tgets = value; }
		}
		public RippleCurrency taker_pays { 
			get { return _tpays; }
			set { _tpays = value; }
		}

		private RippleCurrency _tgets;
		private RippleCurrency _tpays;

		public UInt32 flags { get { return _flags; } set { _flags = value; } }
		public UInt32 Flags { get { return _flags; } set { _flags = value; } }
#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private UInt32 _flags = 0;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		public string quality { get; set; }
		//public int seq { get; set; } ??? why was this here?


		public string BookDirectory { get; set; }
		public string BookNode { get; set; }

		public string LedgerEntryType { get; set; }
		public string OwnerNode { get; set; }
		public string PreviousTxnID { get; set; }
		public UInt32 PreviousTxnLgrSeq { get; set; }
		public UInt32 Sequence { get { return _seq; } set { _seq = value;} }
		public UInt32 seq { get { return _seq; } set { _seq = value;}}

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private UInt32 _seq = 0;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		public string index { get; set; }
		public string owner_funds { get; set; }

		public RippleCurrency taker_gets_funded { get; set; }
		public RippleCurrency taker_pays_funded { get; set; }
		public UInt32? Expiration { get; set; }

#pragma warning restore IDE1006 // Naming Styles

		public Offer Copy () {
			Offer ret = new Offer {
				Account = this.Account,
				taker_gets = this.taker_gets.DeepCopy (),
				taker_pays = this.taker_pays.DeepCopy (),

				Sequence = this.Sequence,
				PreviousTxnID = this.PreviousTxnID,

				flags = this.flags,

				quality = this.quality,

				BookDirectory = this.BookDirectory,

				LedgerEntryType = this.LedgerEntryType,

				OwnerNode = this.OwnerNode,

				PreviousTxnLgrSeq = this.PreviousTxnLgrSeq,

				index = this.index,

				owner_funds = this.owner_funds,

				taker_gets_funded = this.taker_gets_funded,
				taker_pays_funded = this.taker_pays_funded,

				Expiration = this.Expiration
			};
			return ret;
		}


		/*
		private static T reconstructFromType <T> (T t) {  // where T implements ... haha nice try, 
			Offer o = new Offer ();

			o.Account = t.FinalFields.Account;
			o.TakerGets = t.FinalFields.TakerGets;
			o.TakerPays = t.FinalFields.TakerPays;

			o.Flags = t.FinalFields.Flags;
			//o.quality = node.FinalFields.  // TODO ??
			//o.BookDirectory = node.FinalFields.BookDirectory; ?
			//o.BookNode = node.

			o.LedgerEntryType = t.LedgerEntryType;

			o.OwnerNode = t.FinalFields.OwnerNode;

			o.PreviousTxnID = t.FinalFields.PreviousTxnID;
			o.PreviousTxnLgrSeq = t.FinalFields.PreviousTxnLgrSeq;

			o.Sequence = t.FinalFields.sequence;

			return o;

		}
		*/

		/*
		private StringBuilder sb = null;
		public override String ToString ()
		{
			if (sb == null) {
				StringBuilder sb = new StringBuilder ();
			} else {
				sb.Clear();
			}

			sb.Append("Buy : ");

			sb.Append(TakerPays == null ? "null" : TakerPays.ToString());

			sb.Append(" For :");

			sb.Append (TakerGets == null ? "null" : TakerGets.ToString());


			return sb.ToString();

		}
		*/
	}
}

