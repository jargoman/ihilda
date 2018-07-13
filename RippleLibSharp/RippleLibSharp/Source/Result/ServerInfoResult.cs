using System;

namespace RippleLibSharp.Result
{
	public class ServerInfoResult
	{
		/*
		public ServerInfoResult ()
		{
		}
		*/

#pragma warning disable IDE1006 // Naming Styles
		public Info info { get; set; }

		public string status { get; set; }
		public string type { get; set; }
	}



	public class Info 
	{
		public string hostid { get; set; }
		public string build_version { get; set; }
		public string complete_ledgers { get; set; }
		public int io_latency_ms { get; set; }
		public LastClose last_close { get; set; }
		public int load_factor { get; set; }
		public StateAccounting state_accounting { get; set; }
		public ValidatedLedgerInfo validated_ledger { get; set; }
		public int validation_quorum { get; set; }
		public int uptime { get; set; }
		public int peers { get; set; }
		public string pubkey_node { get; set; }
		public string server_state { get; set; }

	}



	public class ServerState
	{
		public string duration_us { get; set; }
		public int transitions { get; set; }
	}

	public class StateAccounting
	{
		public ServerState connected { get; set; }
		public ServerState disconnected { get; set; }
		public ServerState full { get; set; }
		public ServerState syncing { get; set; }
		public ServerState tracking { get; set; }
	}

	public class ValidatedLedgerInfo
	{
		public int age { get; set; }
		public double base_fee_xrp { get; set; }
		public string hash { get; set; }
		public int reserve_base_xrp { get; set; }
		public int reserve_inc_xrp { get; set; }
		public UInt32 seq { get; set; }
	}

	public class LastClose
	{
		public double converge_time_s { get; set; }
		public int proposers { get; set; }
	}

	public class ValidatedLedger
	{
		public int age { get; set; }
		public double base_fee_xrp { get; set; }
		public string hash { get; set; }
		public int reserve_base_xrp { get; set; }
		public int reserve_inc_xrp { get; set; }
		public int seq { get; set; }
	}

	public class Connected
	{
		public string duration_us { get; set; }
		public int transitions { get; set; }
	}

	public class Disconnected
	{
		public string duration_us { get; set; }
		public int transitions { get; set; }
	}

	public class Full
	{
		public string duration_us { get; set; }
		public int transitions { get; set; }
	}

	public class Syncing
	{
		public string duration_us { get; set; }
		public int transitions { get; set; }
	}

	public class Tracking
	{
		public string duration_us { get; set; }
		public int transitions { get; set; }
	}

	#pragma warning restore IDE1006 // Naming Styles
}

