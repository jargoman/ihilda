using System;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Result;
using RippleLibSharp.Network;

namespace RippleLibSharp.Commands.Accounts
{
	public static class AccountCurrencies
	{
		

		public static Task<Response<AccountCurrenciesResult>> GetResult ( string account, NetworkInterface ni ) {

			int id = NetworkRequestTask.ObtainTicket();
			object o = new {
				id,
				command = "account_currencies",
				account
			};

			string request = DynamicJson.Serialize (o);

			Task<Response<AccountCurrenciesResult>> task = NetworkRequestTask.RequestResponse <AccountCurrenciesResult> (id, request, ni);

			//task.Wait ();

			//return task.Result;
			return task;
		}




	}


}

