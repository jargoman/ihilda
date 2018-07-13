using System;

namespace RippleLibSharp.Result
{
	public class RpcWalletProposeResult
	{
		/*
		public RpcWalletProposeResult ()
		{
		}
		*/

#pragma warning disable IDE1006 // Naming Styles
		public string account_id { get; set; }
		public string key_type { get; set; }
		public string master_key { get; set; }
		public string master_seed { get; set; }
		public string master_seed_hex { get; set; }
		public string public_key { get; set; }
		public string public_key_hex { get; set; }
		public string status { get; set; }
#pragma warning restore IDE1006 // Naming Styles
	}
}

