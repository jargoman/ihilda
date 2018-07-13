using System;

namespace RippleLibSharp.Result
{
	public class ServerStateResult
	{
		/*
		public ServerStateResult ()
		{
		}
		*/

#pragma warning disable IDE1006 // Naming Styles
		public State state { get; set; }


	}

	public class State 
	{
		public int load_base { get; set; }

		public int load_factor_fee_escalation { get; set; }
		public int load_factor_fee_queue { get; set; }
		public int load_factor_fee_reference { get; set; }
		public int load_factor_server { get; set; }
		public string build_version { get; set; }
		public string complete_ledgers { get; set; }
		public int io_latency_ms { get; set; }
		public LastClose last_close { get; set; }
		public int load_factor { get; set; }
		public StateAccounting state_accounting { get; set; }
		public ValidatedLedgerState validated_ledger { get; set; }
		public int validation_quorum { get; set; }
		public int uptime { get; set; }
		public int peers { get; set; }
		public string pubkey_node { get; set; }
		public string server_state { get; set; }

	}


	public class ValidatedLedgerState
	{
		public int base_fee { get; set; }
		public int close_time { get; set; }
		public string hash { get; set; }
		public int reserve_base { get; set; }
		public int reserve_inc { get; set; }
		public UInt32 seq { get; set; }
	}

	#pragma warning restore IDE1006 // Naming Styles
}

