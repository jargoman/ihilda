using System;
using RippleLibSharp.Transactions;

namespace RippleLibSharp.Result
{
	public class AccountInfoResult
	{
#pragma warning disable IDE1006 // Naming Styles
		public AccountData account_data { get; set; }
		public int ledger_current_index { get; set; }
		public bool validated { get; set; }
#pragma warning restore IDE1006 // Naming Styles
		public RippleCurrency GetNativeBalance () {
			string b = account_data?.Balance;
			if (b == null) {
				return null;
			}
			return new RippleCurrency (b);
		}

	}

	public class AccountData
	{
		public string Account { get; set; }
		public string Balance { get; set; }
		public int Flags { get; set; }
		public string LedgerEntryType { get; set; }
		public int OwnerCount { get; set; }
		public string PreviousTxnID { get; set; }
		public int PreviousTxnLgrSeq { get; set; }
		public UInt32 Sequence { get; set; }
#pragma warning disable IDE1006 // Naming Styles
		public string index { get; set; }
#pragma warning restore IDE1006 // Naming Styles
	}





}

