using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using RippleLibSharp.Util;

namespace RippleLibSharp.Commands.Accounts
{
	public static class AccountTx
	{


		public static Task<Response<AccountTxResult>> GetResult (
			string account,
			string ledger_index_min,
			string ledger_index_max,
			int? limit,

			/*count = false,*/

			bool forward,
			NetworkInterface ni,
			CancellationToken token,
			IdentifierTag identifierTag = null
		) {
			if (identifierTag == null) {
				identifierTag = new IdentifierTag {
					IdentificationNumber = NetworkRequestTask.ObtainTicket ()
				};
			}

			object o = new {
				id = identifierTag,
				command = "account_tx",
				account,
				ledger_index_min,
				ledger_index_max,
				binary = false,
				//count = false,
				limit = limit,
				forward
			};

			string request = DynamicJson.Serialize (o);

			Task< Response<AccountTxResult>> task = 
				NetworkRequestTask.RequestResponse < AccountTxResult> (identifierTag, request, ni, token);


			return task;
		}


		public static Task< IEnumerable< Response < AccountTxResult >> > GetFullTxResult (
			string account,

			NetworkInterface ni,
			CancellationToken token
		) {
			
			return GetFullTxResult (account, (-1).ToString(), (-1).ToString(), ni, token);
		}

		public static Task<IEnumerable<Response<AccountTxResult>>> GetFullTxResult (
			string account,
			string ledger_index_min,
			string ledger_index_max,
			int limit,
			NetworkInterface ni,
			CancellationToken token
		)
		{
			return Task.Run (
				delegate {
					bool forward = true; // almost certain it has to be true
					List<Response<AccountTxResult>> list = new List<Response<AccountTxResult>> ();



					IdentifierTag identifierTag = new IdentifierTag {
						IdentificationNumber = NetworkRequestTask.ObtainTicket ()
					};


					object o = new {
						id = identifierTag,
						command = "account_tx",
						account,
						ledger_index_min,
						ledger_index_max,
						limit,
						binary = false,
						/*count = false,*/
						forward
					};

					string request = DynamicJson.Serialize (o);

					Task<Response<AccountTxResult>> task =
						NetworkRequestTask.RequestResponse<AccountTxResult> (identifierTag, request, ni, token);

					if (task == null) {
						//TODO
						return null;
					}

					task.Wait (token);


					Response<AccountTxResult> res = task.Result;
					if (task.Result == null) {
						//TODO
						return null;
					}

					list.Add (res);

					AccountTxResult accountTx = res.result;

					limit -= accountTx.transactions.Count ();

					while (accountTx?.marker != null && limit > 0 && !token.IsCancellationRequested) {
						//Thread.Sleep(18000);

						identifierTag = new IdentifierTag {
							IdentificationNumber = NetworkRequestTask.ObtainTicket ()
						};


						o = new {
							id = identifierTag,
							command = "account_tx",
							account,
							//ledger_index_min = accountTx.marker,
							ledger_index_max,
							binary = false,
							limit,
							forward,
							marker = accountTx.marker.GetObject ()
						};

						request = DynamicJson.Serialize (o);
						task = null; // set it to null so you know it failed rather than still having old value
						task = NetworkRequestTask.RequestResponse<AccountTxResult> (identifierTag, request, ni, token);


						if (task == null) {
							//TODO
							Logging.WriteLog ("task == null");
							//break;
							return null;
						}

						task.Wait (token);




						res = task.Result;
						if (task.Result == null) {
							// TODO
							Logging.WriteLog ("task.result == null");
							//break;
							return null;
						}

						list.Add (res);

						accountTx = res.result;

						limit -= accountTx.transactions.Count ();

					}

					return list.AsEnumerable ();
				}
				, token);
		}

		public static Task< IEnumerable< Response < AccountTxResult >> > GetFullTxResult (
			string account, 
			string ledger_index_min, 
			string ledger_index_max,
			/*count = false,*/
			//int limit,

			NetworkInterface ni,
			CancellationToken token
			//IdentifierTag identifierTag = null

		) 

		 {
			return Task.Run(
				delegate {
					bool forward = true; // almost certain it has to be true
					List<Response<AccountTxResult>> list = new List<Response<AccountTxResult>>();



					IdentifierTag identifierTag= new IdentifierTag {
						IdentificationNumber = NetworkRequestTask.ObtainTicket ()
					};


					object o = new {
						id = identifierTag,
						command = "account_tx",
						account,
						ledger_index_min,
						ledger_index_max,
						binary = false,
						/*count = false,*/
						forward
					};

					string request = DynamicJson.Serialize (o);

					Task< Response<AccountTxResult>> task = 
						NetworkRequestTask.RequestResponse < AccountTxResult> (identifierTag, request, ni, token);

					if (task == null) {
						//TODO
						return null;
					}

					task.Wait(token);


					Response<AccountTxResult> res = task.Result;
					if (task.Result == null) {
						//TODO
						return null;
					}

					list.Add(res);

					AccountTxResult accountTx = res.result;


					while (accountTx?.marker != null) {
						//Thread.Sleep(18000);

						identifierTag = new IdentifierTag {
							IdentificationNumber = NetworkRequestTask.ObtainTicket ()
						};


						o = new {
							id = identifierTag,
							command = "account_tx",
							account,
							//ledger_index_min = accountTx.marker,
							ledger_index_max,
							binary = false,
							/*limit = 100,*/
							forward,
							marker = accountTx.marker.GetObject()
						};

						request = DynamicJson.Serialize (o);
						task = null; // set it to null so you know it failed rather than still having old value
						task = NetworkRequestTask.RequestResponse < AccountTxResult> (identifierTag, request, ni, token);


						if (task == null) {
							//TODO
							Logging.WriteLog("task == null");
							return null; // all or nothing
							//break;
						}

						task.Wait(token);




						res = task.Result;
						if (task.Result == null) {
							// TODO
							Logging.WriteLog("task.result == null");
							return null;
							//break;
						}


						list.Add(res);

						accountTx = res.result; // not redundant, needed for while loop condition 



					}

					return list.AsEnumerable();
				}
			);
		}

	}
}

