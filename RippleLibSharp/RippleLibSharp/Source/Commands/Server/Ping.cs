using System;
using System.Threading;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Network;
using RippleLibSharp.Result;

namespace RippleLibSharp.Commands.Server
{
	public static class Ping
	{

		public static  Task<Response<PingObject>> getResult (NetworkInterface ni, CancellationToken token, IdentifierTag identifierTag = null) {
			if (identifierTag == null) {
				identifierTag = new IdentifierTag {
					IdentificationNumber = NetworkRequestTask.ObtainTicket ()
				};
			}

			object o = new {
				id = identifierTag,
				command = "ping",
			};

			string request = DynamicJson.Serialize (o);

			Task< Response<PingObject>> task = NetworkRequestTask.RequestResponse <PingObject> (identifierTag, request, ni, token);

			return task;
		}
	}
}

