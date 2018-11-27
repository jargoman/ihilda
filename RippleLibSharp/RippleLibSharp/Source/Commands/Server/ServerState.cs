using System;
using System.Threading;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Network;
using RippleLibSharp.Result;

namespace RippleLibSharp.Commands.Server
{
	public static class ServerState
	{
		

		public static  Task<Response<ServerStateResult>> GetResult (NetworkInterface ni, CancellationToken token, IdentifierTag identifierTag = null) {

			if (identifierTag == null) {
				identifierTag = new IdentifierTag {
					IdentificationNumber = NetworkRequestTask.ObtainTicket ()
				};
			}

			object o = new {
				id = identifierTag,
				command = "server_state",
			};

			string request = DynamicJson.Serialize (o);

			Task< Response<ServerStateResult>> task = NetworkRequestTask.RequestResponse <ServerStateResult> (identifierTag, request, ni, token);

			//task.Wait ();

			//return task.Result;
			return task;
		}


	}
}

