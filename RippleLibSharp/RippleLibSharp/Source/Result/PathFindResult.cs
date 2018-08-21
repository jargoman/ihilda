using System;
using RippleLibSharp.Paths;
using RippleLibSharp.Transactions;
using RippleLibSharp.Network;

namespace RippleLibSharp.Result
{
	public class PathFindResult : Identifiable
	{
		/*
		public PathFindResult ()
		{
		}
		*/

#pragma warning disable IDE1006 // Naming Styles
		public Alternative [] alternatives { get; set; }

		public string destination_account { get; set; }
		public RippleCurrency destination_amount { get; set; }
		public bool full_reply { get; set; }
		public IdentifierTag id { get; set; }
		public string source_account { get; set; }
	

	}


	public class Alternative
	{
		public RipplePathElement[][] paths_computed { get; set; }
		public RippleCurrency source_amount { get; set; }

#pragma warning restore IDE1006 // Naming Styles
	}
}

