
using System.Threading;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions.TxTypes;

namespace RippleLibSharp.Commands.Tx
{
#pragma warning disable IDE1006 // Naming Styles
	public static class tx
#pragma warning restore IDE1006 // Naming Styles
	{
		public static Task<Response<RippleTransaction>> GetRequest ( string tx_id, NetworkInterface ni, CancellationToken token, IdentifierTag identifierTag = null)
		{
			if (identifierTag == null) {
				identifierTag = new IdentifierTag {
					IdentificationNumber = NetworkRequestTask.ObtainTicket ()
				};
			}

			object o =
				new {
				id = identifierTag,
				command = "tx",
				transaction = tx_id
			};

			string request = DynamicJson.Serialize (o);

			Task< Response<RippleTransaction>> task = 
				NetworkRequestTask.RequestResponse < RippleTransaction> (identifierTag, request, ni, token);


			return task;
		}


		public static Task<Response<string>> GetTxFromAccountAndSequenceDataAPI (string account, uint sequence, CancellationToken token)
		{
			return Task.Run (
				delegate {

					//DoThrottlingWait ();

					string req = baseapi + accountscommand + account + "/" + txcommand + sequence + options;
					Response<string> resp = DataApi.GetResponseObject<Response<string>> (req, token);

					//if () {

					//}
					//}
					return resp;
				}
			, token);
		}


		// HAS to be string. Not RippleTxStructure. Or rather lets hope string is the only potential result
		// result returns a string and if successful transction is populated with a txstructure. 
		public static Task<Response<string>> GetRequestDataApi (string tx_id, CancellationToken token) {
			return Task.Run (
				delegate {

					//int attempt = 0;
					//while (attempt++ < 3) {
					// limit data api calls to avoid getting 

					//DoThrottlingWait ();

					string req = baseapi + txcommand + tx_id + options;
						Response<string> resp = DataApi.GetResponseObject<Response<string>> (req, token);

						//if () {

						//}
					//}
					return resp;
				}
			, token);

		}





		static string baseapi = "https://data.ripple.com/v2/";

		static string txcommand = "transactions/";

		static string accountscommand = "accounts/";

		static string options = "?binary=false";

	}
}

