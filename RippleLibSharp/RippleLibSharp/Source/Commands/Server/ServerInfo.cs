using System;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Network;
using RippleLibSharp.Result;

namespace RippleLibSharp.Commands.Server
{
	public static class ServerInfo
	{

		public static  Task<Response<ServerInfoResult>> GetResult (NetworkInterface ni) {

			int id = NetworkRequestTask.ObtainTicket();
			object o = new {
				id,
				command = "server_info",
			};

			string request = DynamicJson.Serialize (o);

			Task< Response<ServerInfoResult>> task = NetworkRequestTask.RequestResponse <ServerInfoResult> (id, request, ni);

		
			return task;
		}

		public static Tuple<string, UInt32> GetFeeAndLedgerSequence (NetworkInterface ni) {
			try {

				Task< Response<ServerInfoResult>> task = GetResult (ni);

				if (task == null) {
					return null;
				}

				task.Wait (15000);

				Response<ServerInfoResult> res = task.Result;
				if (res == null) {
					return null;
				}

				if (res.HasError()) {
					// TODO

					return null;
				}
				ServerInfoResult serverInfoResult = res.result;
				if (serverInfoResult == null) {
					return null;
				
				}
				double native_base_fee;

				native_base_fee = serverInfoResult.info.validated_ledger.base_fee_xrp; /*new decimal (serverinfo.info.validated_ledger.base_fee_xrp);*/

				ulong transaction_fee = (ulong)((native_base_fee * 1000000) * serverInfoResult.info.load_factor);

				Tuple<string, UInt32> ret = new Tuple<string, UInt32> (transaction_fee.ToString (), serverInfoResult.info.validated_ledger.seq);

				return ret;

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
}

