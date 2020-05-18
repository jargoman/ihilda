using System;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Result;
using RippleLibSharp.Network;
using System.Threading;

namespace RippleLibSharp.Commands.Accounts
{
	public static class AccountCurrencies
	{
		

		public static Task<Response<AccountCurrenciesResult>> GetResult ( string account, NetworkInterface ni, CancellationToken token, IdentifierTag identifierTag = null ) {

			if (identifierTag == null) {
				identifierTag = new IdentifierTag {
					IdentificationNumber = NetworkRequestTask.ObtainTicket ()
				};
			}
		
			object o = new {
				id = identifierTag,
				command = "account_currencies",
				account
			};

			string request = DynamicJson.Serialize (o);

			Task<Response<AccountCurrenciesResult>> task = NetworkRequestTask.RequestResponse <AccountCurrenciesResult> (identifierTag, request, ni, token);

			//task.Wait ();

			//task.Result.result.send_currencies
			//return task.Result;
			return task;
		}




	}


}

