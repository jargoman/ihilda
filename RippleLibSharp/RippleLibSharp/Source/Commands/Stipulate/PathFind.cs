using System;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;

namespace RippleLibSharp.Commands.Stipulate
{
	public static class PathFind
	{
		public static  Task<Response<PathFindResult>> GetResult ( string source_account, string destination_account, RippleCurrency destination_amount, NetworkInterface ni) {

			int id = NetworkRequestTask.ObtainTicket();
			object o = new {
				id,
				command = "path_find",
				subcommand = "create",
				source_account,
				destination_account,
				destination_amount
			};

			string request = DynamicJson.Serialize (o);

			Task< Response<PathFindResult>> task = NetworkRequestTask.RequestResponse <PathFindResult> (id, request, ni);

			return task;
		}
	}
}

