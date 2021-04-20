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



		private static Dictionary<String, object>  nodeTraceCache = new Dictionary < String, object >();
		private static object cacheLock = new object ();
		public static object LookupNodeTrace (string tx_id) {
			object o = null;

			if (tx_id == null) {
				return null;
			}

			lock (cacheLock) {
				if (nodeTraceCache == null) {
					return null;
				}

				if (nodeTraceCache.Count < 1) {
					return null;
				}

				if (nodeTraceCache.ContainsKey (tx_id)) {

					/*bool b = */
					nodeTraceCache.TryGetValue (tx_id, out o);


				}
			}

			return o;

		}

		public static void SetNodeTrace (string tx_id, object node) {
			if (tx_id == null) {
				return;
			}

			lock (cacheLock) {
				if (nodeTraceCache == null) {
					return;
				}
				if (nodeTraceCache.ContainsKey (tx_id)) {
					return;
				}

				nodeTraceCache.Add (tx_id, node);
			}


		}

		private void LoadNodeTraceCache () {


		}

		private void SaveNodeTraceCache (  ) {


		}

	}
}

