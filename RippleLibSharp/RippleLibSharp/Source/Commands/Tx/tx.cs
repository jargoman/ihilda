using System;

using RippleLibSharp.Network;

using System.Threading.Tasks;
using System.Threading;

using RippleLibSharp.Result;
using RippleLibSharp.Transactions;
using RippleLibSharp.Transactions.TxTypes;

using Codeplex.Data;

namespace RippleLibSharp.Commands.Tx
{
#pragma warning disable IDE1006 // Naming Styles
	public static class tx
#pragma warning restore IDE1006 // Naming Styles
	{
		public static Task<Response<RippleTransaction>> GetRequest ( string tx_id, NetworkInterface ni )
		{
			int id = NetworkRequestTask.ObtainTicket();
			object o =
				new {
				id,
				command = "tx",
				transaction = tx_id
			};

			string request = DynamicJson.Serialize (o);

			Task< Response<RippleTransaction>> task = 
				NetworkRequestTask.RequestResponse < RippleTransaction> (id, request, ni);


			return task;
		}


		// HAS to be string. Not RippleTxStructure. Or rather lets hope string is the only potential result
		// result returns a string and if successful transction is populated with a txstructure. 
		public static Task<Response<string>> GetRequestDataApi (string tx_id) {
			return Task.Run (
				delegate {
					string req = api + tx_id + options;
					Response<string> resp = DataApi.GetResponseObject <Response<string>>(req);

					return resp;
				}
			);

		}


		static string api = "https://data.ripple.com/v2/transactions/";
		static string options = "?binary=false";

	}
}

