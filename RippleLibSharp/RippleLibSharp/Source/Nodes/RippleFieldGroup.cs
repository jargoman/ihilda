using System;
using RippleLibSharp.Transactions;

namespace RippleLibSharp.Nodes
{
	public class RippleFieldGroup
	{

		public UInt32 Flags {
			get;
			set;
		}
		public string Account {
			get;
			set;
		}
		public string Owner {
			get;
			set;
		}
		public RippleCurrency Balance {
			get;
			set;
		}
		public string OwnerNode {
			get;
			set;
		}
		public UInt32 OwnerCount {
			get;
			set;
		}
		public UInt32 Sequence {
			get;
			set;
		}
		public string ExchangeRate {
			get;
			set;
		}
		public string RootIndex {
			get;
			set;
		}
		public string TakerGetsCurrency {
			get;
			set;
		}
		public string TakerGetsIssuer {
			get;
			set;
		}
		public string TakerPaysCurrency {
			get;
			set;
		}
		public string TakerPaysIssuer {
			get;
			set;
		}

		public string IndexPrevious {
			get;
			set;
		}
		public string LedgerEntryType {
			get;
			set;
		}
		public string LedgerIndex {
			get;
			set;
		}
		public string PreviousTxnID {
			get;
			set;
		}
		public UInt32 PreviousTxnLgrSeq {
			get;
			set;
		}
		public string BookDirectory {
			get;
			set;
		}
		public RippleCurrency TakerGets {
			get;
			set;
		}
		public RippleCurrency TakerPays {
			get;
			set;
		}

		public RippleCurrency HighLimit {
			get;
			set;
		}

		public RippleCurrency LowLimit {
			get;
			set;
		}

		public string HighNode {
			get;
			set;
		}


		public string LowNode {
			get;
			set;
		}

		public string BookNode {
			get;
			set;
		}

		public string IndexNext {
			get;
			set;
		}

		public MemoIndice [] Memos {
			get;
			set;
		}
	}
}

