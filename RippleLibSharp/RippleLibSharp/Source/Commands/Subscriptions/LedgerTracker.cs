using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RippleLibSharp.Commands.Server;
using RippleLibSharp.Network;

namespace RippleLibSharp.Commands.Subscriptions
{
	public static class LedgerTracker
	{

		/*
		static LedgerTracker ()
		{
			Task.Run ( delegate {

				WaitForLedgerExpire ();
			});
		}
		*/

		public static void SetLedger ( LedgerClosed ledger )
		{
			LastLedgerClosed = ledger;
			Task.Run ( delegate {
				//if (OnLedgerClosed != null) {
					OnLedgerClosed?.Invoke (null, ledger);
				//}

				
				
			});

			LedgerResetEvent?.Set ();


			
		}




		public static void SetServerState (ServerStateEventArgs serverState) {
			ServerStateEv = serverState;
			Task.Run ( delegate {

				//if (OnServerStateChanged != null) {
					OnServerStateChanged?.Invoke (null, serverState);
				//}
			});
			ServerStateEvent?.Set ();
			
		}

		public static LedgerClosed LastLedgerClosed {
			get {
				if (_LastLedgerClosed == null) {
					return null;
				}


				TimeSpan timeSpan = DateTime.Now - _LastLedgerClosed.ReceivedTime;
				if ( timeSpan.TotalSeconds > 30) {
					return null;
				}

				return _LastLedgerClosed; 
			}
			set {
				if (value != null) {
					_last_index = value.ledger_index;
					value.ReceivedTime = DateTime.Now;
					
				}
				_LastLedgerClosed = value;
			}
		}
		private static uint _last_index = default (uint);
		private static LedgerClosed _LastLedgerClosed = null;

		public static ServerStateEventArgs ServerStateEv {
			get;
			set;
		}

		public static CancellationTokenSource TokenSource {
			get;
			set;
		}

		public static UInt32? GetRecentLedgerOrNull ()
		{
			LedgerClosed led = LastLedgerClosed;

			return led?.ledger_index;
	    		

		}


		//private static uint? lastRetrieved = null;
		public static FeeAndLastLedgerResponse GetFeeAndLastLedger (CancellationToken token) {

			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append ("Could not retrieve Fee and last ledger from Ledger tracker\n");
			FeeAndLastLedgerResponse feeResp = new FeeAndLastLedgerResponse ();

			LedgerClosed ledger = LastLedgerClosed;

			if (ledger == null) {
				stringBuilder.Append ("Last Stored Ledger is null");
				feeResp.ErrorMessage += stringBuilder.ToString ();
				return feeResp;
			}
			

			ServerStateEventArgs serverState = ServerStateEv;
			if (serverState == null) {
				stringBuilder.Append ("Last stored server state is null");
				feeResp.ErrorMessage += stringBuilder.ToString ();
				return feeResp;
			}


			//if (ledger.ledger_index == lastRetrieved) {
				

			//}

			//lastRetrieved = ledger.ledger_index;

			double native_base_fee;

			native_base_fee = serverState.base_fee;

			ulong transaction_fee = (ulong)((native_base_fee * serverState.load_factor) / serverState.load_base);

			//Tuple<string, UInt32> ret = new Tuple<string, UInt32> (transaction_fee.ToString (), ledger.ledger_index);

			feeResp.Fee = transaction_fee.ToString ();
			feeResp.LastLedger = ledger.ledger_index;

			return feeResp;
		


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


		public DateTime ReceivedTime {
			get;
			set;
		}


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
