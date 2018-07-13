using System;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Network;
using RippleLibSharp.Result;

namespace RippleLibSharp.Commands.Server
{
	public static class ServerState
	{
		

		public static  Task<Response<ServerStateResult>> GetResult (NetworkInterface ni) {

			int id = NetworkRequestTask.ObtainTicket();
			object o = new {
				id,
				command = "server_state",
			};

			string request = DynamicJson.Serialize (o);

			Task< Response<ServerStateResult>> task = NetworkRequestTask.RequestResponse <ServerStateResult> (id, request, ni);

			//task.Wait ();

			//return task.Result;
			return task;
		}


	}
}

