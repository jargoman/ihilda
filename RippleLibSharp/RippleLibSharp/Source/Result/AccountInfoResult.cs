using System;
using System.Threading;
using RippleLibSharp.Commands.Server;
using RippleLibSharp.Network;
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

		public RippleCurrency GetReserveRequirements (NetworkInterface ni, CancellationToken token)
		{

			var task = ServerInfo.GetResult (ni, token);

			task.Wait (token);

			Response<ServerInfoResult> resp = task.Result;

			if (resp == null) {
				return null;
			}

			if (resp.HasError()) {
				return null;
			}

			ServerInfoResult res = resp.result;


			Info info = res.info;

			ValidatedLedgerInfo val = info.validated_ledger;



			int am = val.reserve_base_xrp + ( val.reserve_inc_xrp  * this.account_data.OwnerCount);
			Decimal d = am * 1000000m;
			return new RippleCurrency(d);
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

