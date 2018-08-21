﻿using System;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Result;
using RippleLibSharp.Network;

namespace RippleLibSharp.Commands.Accounts
{
	public static class AccountCurrencies
	{
		

		public static Task<Response<AccountCurrenciesResult>> GetResult ( string account, NetworkInterface ni, IdentifierTag identifierTag = null ) {

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

			Task<Response<AccountCurrenciesResult>> task = NetworkRequestTask.RequestResponse <AccountCurrenciesResult> (identifierTag, request, ni);

			//task.Wait ();

			//return task.Result;
			return task;
		}




	}


}

