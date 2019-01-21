using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Network;
using RippleLibSharp.Result;

namespace RippleLibSharp.Commands.Server
{
	public static class ServerInfo
	{

		public static  Task<Response<ServerInfoResult>> GetResult (NetworkInterface ni, CancellationToken token, IdentifierTag identifierTag = null) {
			if (identifierTag == null) {
				identifierTag = new IdentifierTag {
					IdentificationNumber = NetworkRequestTask.ObtainTicket ()
				};
			}

			object o = new {
				id = identifierTag,
				command = "server_info",
			};

			string request = DynamicJson.Serialize (o);

			Task< Response<ServerInfoResult>> task = NetworkRequestTask.RequestResponse <ServerInfoResult> ( identifierTag, request, ni, token );

		
			return task;
		}

		public static FeeAndLastLedgerResponse GetFeeAndLedgerSequence (NetworkInterface ni, CancellationToken token) {

			FeeAndLastLedgerResponse feeAndLastLedger = new FeeAndLastLedgerResponse ();

			StringBuilder stringBuilder = new StringBuilder ();
			try {

				

				Task< Response<ServerInfoResult>> task = GetResult (ni, token);


				stringBuilder.Append ("Could not retrieve Fee and last ledger\n");
				if (task == null) {
					stringBuilder.Append ("ServerInfo returned null task\n");
					feeAndLastLedger.ErrorMessage += stringBuilder.ToString ();
					return feeAndLastLedger;
				}

				task.Wait (150000, token);

				Response<ServerInfoResult> res = task?.Result;
				if (res == null) {
					stringBuilder.Append ("ServerInfo returned null result");
					feeAndLastLedger.ErrorMessage += stringBuilder.ToString ();
					return feeAndLastLedger;
				}

				if (res.HasError()) {
					// TODO
					stringBuilder.Append (res.error_message);
					feeAndLastLedger.ErrorMessage += stringBuilder.ToString ();
					return feeAndLastLedger;
				}
				ServerInfoResult serverInfoResult = res.result;
				if (serverInfoResult == null) {
					return null;
				
				}
				double native_base_fee;

				native_base_fee = serverInfoResult.info.validated_ledger.base_fee_xrp; /*new decimal (serverinfo.info.validated_ledger.base_fee_xrp);*/

				// INSANE. different convention for the fee... 0.00001 XRP vs 10 drops 
				ulong transaction_fee = (ulong)((native_base_fee * 1000000) * serverInfoResult.info.load_factor);

				//Tuple<string, UInt32> ret = new Tuple<string, UInt32> (transaction_fee.ToString (), serverInfoResult.info.validated_ledger.seq);


				feeAndLastLedger.Fee = transaction_fee.ToString ();

				var inf = serverInfoResult.info;
				if (inf == null) {
					feeAndLastLedger.ErrorMessage += "Server info obj null";
					feeAndLastLedger.HasError = true;
					return feeAndLastLedger;
				}

				var val = inf.validated_ledger;
				if (val == null) {
					feeAndLastLedger.ErrorMessage += "Validated ledger is null\n";
					feeAndLastLedger.HasError = true;
					return feeAndLastLedger;
				}

				feeAndLastLedger.LastLedger = inf.validated_ledger.seq;


				return feeAndLastLedger;

			}

#pragma warning disable 0168
			catch ( Exception e ) {
#pragma warning disable 0168


#if DEBUG
				if (e != null) {
					Util.Logging.WriteLog ( e.Message + "\n" + e.StackTrace );

					if (e.InnerException != null) {
						Util.Logging.WriteLog (e.InnerException.Message + "\n" + e.StackTrace);
					}
				}

#endif

				return null;
			}
		}


			
	}


	public class FeeAndLastLedgerResponse
	{
		public string Fee {
			get;
			set;
		}

		public UInt32 LastLedger {
			get;
			set;
		}

		public string Message {
			get;
			set;
		}

		public string ErrorMessage {
			get;
			set;
		}

		public bool HasError
		{
			get {
				if (ErrorMessage != null) {
					//return true;
				}

				if (Fee == null) {
					return true;
				}

				return _HasError;
			}
			set { _HasError = value; }
			
		}
		private bool _HasError = false;

	}
}

