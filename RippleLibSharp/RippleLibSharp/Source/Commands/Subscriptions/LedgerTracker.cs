using System;
using System.Threading;
using System.Threading.Tasks;
using RippleLibSharp.Result;

namespace RippleLibSharp.Commands.Subscriptions
{
	public static class LedgerTracker
	{
		public static void SetLedger ( LedgerClosed ledger )
		{
			LastLedgerClosed = ledger;
			Task.Run ( delegate {
				if (OnLedgerClosed != null) {
					OnLedgerClosed.Invoke (null, ledger);
				}
				
			});

			LedgerResetEvent.Set ();
		}

		public static void SetServerState (SubscribeServerResult result)
		{

		}

		public static void SetServerState (ServerStateEventArgs serverState) {
			ServerStateEv = serverState;
			Task.Run ( delegate {

				if (OnServerStateChanged != null) {
					OnServerStateChanged.Invoke (null, serverState);
				}
			});
			ServerStateEvent.Set ();
		}

		public static LedgerClosed LastLedgerClosed {
			get;
			set;
		}

		public static ServerStateEventArgs ServerStateEv {
			get;
			set;
		}

		public static CancellationTokenSource TokenSource {
			get;
			set;
		}


		public static Tuple<string, UInt32> GetFeeAndLastLedger (CancellationToken token) {

			LedgerClosed ledger = LastLedgerClosed;

			if (ledger == null) {
				return null;
			}
			

			ServerStateEventArgs serverState = ServerStateEv;
			if (serverState == null) {
				return null;
			}

			double native_base_fee;

			native_base_fee = serverState.base_fee;

			ulong transaction_fee = (ulong)((native_base_fee * serverState.load_factor) / serverState.load_base);

			Tuple<string, UInt32> ret = new Tuple<string, UInt32> (transaction_fee.ToString (), ledger.ledger_index);

			return ret;



		}

		public static AutoResetEvent LedgerResetEvent = new AutoResetEvent (true);
		public static AutoResetEvent ServerStateEvent = new AutoResetEvent (true);

		public static event EventHandler<ServerStateEventArgs> OnServerStateChanged;
		public static event EventHandler <LedgerClosed> OnLedgerClosed;

	}

	public class LedgerClosed : EventArgs
	{
#pragma warning disable IDE1006 // Naming Styles
		public int fee_base { get; set; }
		public int fee_ref { get; set; }
		public string ledger_hash { get; set; }
		public uint ledger_index { get; set; }
		public int ledger_time { get; set; }
		public int reserve_base { get; set; }
		public int reserve_inc { get; set; }
		public int txn_count { get; set; }
		public string type { get; set; }
		public string validated_ledgers { get; set; }
#pragma warning restore IDE1006 // Naming Styles





	}

	

	public class ServerStateEventArgs : EventArgs
	{

#pragma warning disable IDE1006 // Naming Styles
		public string type { get; set; }
		public int base_fee { get; set; }
		public int load_base { get; set; }
		public int load_factor { get; set; }
		public int load_factor_fee_escalation { get; set; }
		public int load_factor_fee_queue { get; set; }
		public int load_factor_fee_reference { get; set; }
		public int load_factor_server { get; set; }
		public string server_status { get; set; }
#pragma warning restore IDE1006 // Naming Styles


	}
}
