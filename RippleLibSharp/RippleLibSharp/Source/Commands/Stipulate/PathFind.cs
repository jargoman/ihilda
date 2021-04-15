using System;
using System.Threading;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;

namespace RippleLibSharp.Commands.Stipulate
{
	public static class PathFind
	{
		public static  Task<Response<PathFindResult>> GetResult ( string source_account, string destination_account, RippleCurrency destination_amount, NetworkInterface ni, CancellationToken token, IdentifierTag identifierTag = null) {

			if (identifierTag == null) {
				identifierTag = new IdentifierTag {
					IdentificationNumber = NetworkRequestTask.ObtainTicket ()
				};
			}

			object d = null;

			if (destination_amount.IsNative) {

				d = (destination_amount.amount).ToString ();

			} else {

				d = new {
					value = destination_amount.amount,
					currency = destination_amount.currency,
					issuer = destination_amount.issuer
				};

			}

			object o = new {
				id = identifierTag,
				command = "path_find",
				subcommand = "create",
				source_account = source_account,
				destination_account = destination_account,
				destination_amount = d
			};

			string request = DynamicJson.Serialize (o);

			Task< Response<PathFindResult>> task = NetworkRequestTask.RequestResponse <PathFindResult> (identifierTag, request, ni, token);

			return task;
		}
	}
}

