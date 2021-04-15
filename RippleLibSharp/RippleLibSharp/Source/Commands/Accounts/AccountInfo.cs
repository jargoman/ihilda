using System;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Result;
using RippleLibSharp.Network;
using RippleLibSharp.Transactions;
using RippleLibSharp.Keys;
using System.Threading;

namespace RippleLibSharp.Commands.Accounts
{
	public static class AccountInfo
	{
		

		public static Task<Response<AccountInfoResult>> GetResult ( string account, NetworkInterface ni, CancellationToken token, IdentifierTag identifierTag = null ) {

			if (identifierTag == null) {
				identifierTag = new IdentifierTag {
					IdentificationNumber = NetworkRequestTask.ObtainTicket ()
				};
			}

			object o = new {
				id = identifierTag,
				command = "account_info",
				account
			};

			string request = DynamicJson.Serialize (o);

			Task<Response<AccountInfoResult>> task = NetworkRequestTask.RequestResponse <AccountInfoResult> (identifierTag, request, ni, token);

			//task.Wait ();
			//return task.Result;
			return task;
		}



		public static UInt32? GetSequence (string account, NetworkInterface ni, CancellationToken token ) {
			Task<Response<AccountInfoResult>> t = GetResult (account, ni, token);

			t.Wait (token);

			Response<AccountInfoResult> res = t?.Result;

			if (res == null) {
				return null;
			}

			if (res.HasError()) {
				// TODO advanced networkig 
				if (res.error_code == 17) {
					return 0;
				}
				return null;
			}

			AccountInfoResult inf = res.result;

			return inf.account_data.Sequence;
		}


		public static RippleCurrency GetNativeBalance (RippleAddress ra, NetworkInterface ni, CancellationToken token ) {


			Task<Response<AccountInfoResult>> task = AccountInfo.GetResult (
				ra,
				ni,
				token

			);

			if (task?.Result == null) {

				return null;
			}

			Response<AccountInfoResult> response = task.Result;

			if (response.HasError()) {
				return null;
			}

			AccountInfoResult result = response.result;

			RippleCurrency rc = result.GetNativeBalance ();

			return rc;

		}

	}
}

