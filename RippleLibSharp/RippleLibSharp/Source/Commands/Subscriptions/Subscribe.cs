using System.Threading;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Network;
using RippleLibSharp.Result;

namespace RippleLibSharp.Commands.Subscriptions
{
	public static class Subscribe
	{

		// none of this really works as expected
		public static Task<Response<SubscribeServerResult>> LedgerSubscribe (
			NetworkInterface ni,
			CancellationToken token,
	    		SubscribeResponsesConsumer responsesConsumer,
			IdentifierTag identifierTag = null

		)
		{

			if (identifierTag == null) {
				identifierTag = new IdentifierTag {
					IdentificationNumber = NetworkRequestTask.ObtainTicket ()
				};
			}

			object o = new {
				//id = identifierTag, // doesn't do anything...
				command = "subscribe",
				streams = new string [] { "ledger" } // somethings not working idk

			};

			string request = DynamicJson.Serialize (o);



			Task<Response<SubscribeServerResult>> task = NetworkRequestTask.RequestResponse<SubscribeServerResult> (identifierTag, request, ni, token, responsesConsumer);

			return task;
		}

		public static Task<Response<SubscribeServerResult>> ServerSubscribe (
			NetworkInterface ni,
			CancellationToken token,
	    		SubscribeResponsesConsumer responsesConsumer,
			IdentifierTag identifierTag = null

		)
		{

			if (identifierTag == null) {
				identifierTag = new IdentifierTag {
					IdentificationNumber = NetworkRequestTask.ObtainTicket ()
				};
			}

			object o = new {
				//id = identifierTag, // doesn't do anything...
				command = "subscribe",
				streams = new string [] { "server" } // somethings not working idk

			};

			string request = DynamicJson.Serialize (o);

			Task<Response<SubscribeServerResult>> task = NetworkRequestTask.RequestResponse<SubscribeServerResult> (identifierTag, request, ni, token, responsesConsumer);
			task.ContinueWith((arg) => {
				
				//arg.Result.
			});

			return task;
		}


		public static Task<Response<AccountTxResult>> AccountSubscribe (
			string[] accounts,
			//string ledger_index_min,
			//string ledger_index_max,
			//int? limit,

			/*count = false,*/

			//bool forward,
			NetworkInterface ni,
			CancellationToken token,
	    		SubscribeResponsesConsumer responsesConsumer,
			IdentifierTag identifierTag = null
	    		
		) {

			if (identifierTag == null) {
				identifierTag = new IdentifierTag {
					IdentificationNumber = NetworkRequestTask.ObtainTicket ()
				};
			}

			object o = new {
				id = identifierTag,
				command = "subscribe",
				accounts,
			 	//streams = new string [] { "transactions"} // somethings not working idk

			};

			string request = DynamicJson.Serialize (o);

			Task<Response<AccountTxResult>> task = 
				NetworkRequestTask.RequestResponse<AccountTxResult> (identifierTag, request, ni, token, responsesConsumer);

			return task;
		}
	}


	public class SubscribeServerResult
	{
#pragma warning disable IDE1006 // Naming Styles
		public string hostid { get; set; }
		public int load_base { get; set; }
		public int load_factor { get; set; }
		public string pubkey_node { get; set; }
		public string random { get; set; }
		public string server_status { get; set; }
#pragma warning restore IDE1006 // Naming Styles
	}

	/*
	public class RootObject
	{
		public  result { get; set; }
		public string status { get; set; }
		public string type { get; set; }
	}*/
}
