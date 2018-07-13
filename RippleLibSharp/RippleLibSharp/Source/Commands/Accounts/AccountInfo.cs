using System;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Result;
using RippleLibSharp.Network;
using RippleLibSharp.Transactions;
using RippleLibSharp.Keys;

namespace RippleLibSharp.Commands.Accounts
{
	public static class AccountInfo
	{
		

		public static Task<Response<AccountInfoResult>> GetResult ( string account, NetworkInterface ni ) {

			int id = NetworkRequestTask.ObtainTicket();
			object o = new {
				id,
				command = "account_info",
				account
			};

			string request = DynamicJson.Serialize (o);

			Task<Response<AccountInfoResult>> task = NetworkRequestTask.RequestResponse <AccountInfoResult> (id, request, ni);

			//task.Wait ();
			//return task.Result;
			return task;
		}

		public static UInt32? GetSequence (string account, NetworkInterface ni ) {
			Task<Response<AccountInfoResult>> t = GetResult (account, ni);

			t.Wait ();

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


		public static RippleCurrency GetNativeBalance (RippleAddress ra, NetworkInterface ni ) {


			Task<Response<AccountInfoResult>> task = AccountInfo.GetResult (
				ra,
				ni

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

