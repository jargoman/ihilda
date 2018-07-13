using System;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Network;
using RippleLibSharp.Result;

namespace RippleLibSharp.Commands.Server
{
	public static class Ping
	{

		public static  Task<Response<PingObject>> getResult (NetworkInterface ni) {

			int id = NetworkRequestTask.ObtainTicket();
			object o = new {
				id,
				command = "ping",
			};

			string request = DynamicJson.Serialize (o);

			Task< Response<PingObject>> task = NetworkRequestTask.RequestResponse <PingObject> (id, request, ni);

			return task;
		}
	}
}

