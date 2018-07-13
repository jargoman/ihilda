using System;

namespace RippleLibSharp.Paths
{
	public class RipplePathElement
	{

#pragma warning disable IDE1006 // Naming Styles
		public string account { get; set; }

		public string currency { get; set; }

		public string issuer { get; set; }

		public int type { get; set; }

		public string type_hex { get; set; }
#pragma warning restore IDE1006 // Naming Styles

	}
}

