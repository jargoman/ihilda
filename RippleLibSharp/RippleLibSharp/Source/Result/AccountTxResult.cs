using System;
using System.Collections.Generic;
using RippleLibSharp.Transactions;

namespace RippleLibSharp.Result
{
	public class AccountTxResult
	{
#pragma warning disable IDE1006 // Naming Styles
		public string account { get; set; }

		public int ledger_index_max { get; set; }
		public int ledger_index_min { get; set; }
		public int limit { get; set; }

		public Marker marker { get; set; }

		public int offset { get; set; }
		public RippleTxStructure[] transactions { get; set; }
		public bool validated { get; set; }

	}

	public class AffectedNode
	{
		public CreatedNode CreatedNode { get; set; }
		public ModifiedNode ModifiedNode { get; set; }
		public DeletedNode DeletedNode { get; set; }
	}

	public class DeletedNode
	{
		public FieldsGroup FinalFields { get; set; }
		public string LedgerEntryType { get; set; }
		public string LedgerIndex { get; set; }
		public FieldsGroup PreviousFields { get; set; }
	}

	public class ModifiedNode
	{
		public FieldsGroup FinalFields { get; set; }
		public string LedgerEntryType { get; set; }
		public string LedgerIndex { get; set; }
		public FieldsGroup PreviousFields { get; set; }
		public string PreviousTxnID { get; set; }
		public int PreviousTxnLgrSeq { get; set; }
	}

	public class CreatedNode
	{
		public string LedgerEntryType { get; set; }
		public string LedgerIndex { get; set; }
		public FieldsGroup NewFields { get; set; }
	}


		
	public class FieldsGroup
	{
		public string BookDirectory { get; set; }
		public string BookNode { get; set; }
		public string Account { get; set; }
		public object Balance { get; set; }
		public int Sequence { get; set; }
		public int? Flags { get; set; }
		public RippleCurrency HighLimit { get; set; }
		public string HighNode { get; set; }
		public string LowNode { get; set; }
		public RippleCurrency LowLimit { get; set; }
		public RippleCurrency TakerGets { get; set; }
		public  RippleCurrency TakerPays { get; set; }
		public string Owner { get; set; }
		public int? OwnerCount { get; set; }
		public string RootIndex { get; set; }
		public string IndexNext { get; set; }
		public int? LowQualityIn { get; set; }
		public int? LowQualityOut { get; set; }
		public string IndexPrevious { get; set; }

		#pragma warning restore IDE1006 // Naming Styles

	}
}

