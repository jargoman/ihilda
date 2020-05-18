using System;
namespace IhildaWallet
{
	public static class ProgramVariables
	{
		public static readonly string appname = "ihilda";
		public static readonly string version = "1.0.3";
		public static readonly string verboseName = appname + "_community_edition_" + version;

		public static readonly string webUrl = "http://192.168.1.195:45455";

		public static readonly string winterName = "winter";

		public static bool showPopUps = true;
		public static bool network = true;
		public static bool darkmode = false;
		public static bool preferLinq = false;
		public static bool parallelVerify = false;
		public static bool usePager = false;
		public static string botMode = null;
		internal static int ledger;
		internal static int endledger;

		public static Winter winter = null;
	}
}
