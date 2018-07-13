using System;
using System.Collections.Generic;
using RippleLibSharp.Nodes;

using Codeplex.Data;


namespace IhildaWallet
{
	public class RoboMem
	{
		/*
		public RoboMem ()
		{
		}
		*/


		public static Dictionary<String, object>  nodeTraceCache = new Dictionary < String, object >();

		public static object LookupNodeTrace (string tx_id) {
			object o = null;

			if (tx_id == null) {
				return null;
			}

			if (nodeTraceCache == null) {
				return null;
			}

			if (nodeTraceCache.Count < 1) {
				return null;
			}

			if (nodeTraceCache.ContainsKey(tx_id)) {
				
				/*bool b = */ nodeTraceCache.TryGetValue (tx_id, out o);


			}

			return o;

		}

		public static void SetNodeTrace (string tx_id, object node) {
			if (tx_id == null) {
				return;
			}

			if (nodeTraceCache == null) {
				return;
			}
			if (nodeTraceCache.ContainsKey(tx_id)) {
				return;
			}

			nodeTraceCache.Add (tx_id, node);


		}

		private void LoadNodeTraceCache () {


		}

		private void SaveNodeTraceCache (  ) {


		}

	}
}

