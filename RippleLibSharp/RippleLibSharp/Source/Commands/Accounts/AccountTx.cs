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


		public static Task<FullTxResponse > GetFullTxResult (
			string account,

			NetworkInterface ni,
			CancellationToken token
		) {

			return GetFullTxResult (account, (-1).ToString(), (-1).ToString(), ni, token);
		}

		public static Task<FullTxResponse> GetFullTxResult (
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

					FullTxResponse fullTxResponse = new FullTxResponse ();
					bool forward = true; // almost certain it has to be true
					List<Response<AccountTxResult>> list = new List<Response<AccountTxResult>> ();

					fullTxResponse.Responses = list;

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

						fullTxResponse.HasError = true;
						fullTxResponse.ErrorMessage += "task == null\n";

						return fullTxResponse;
						//return null;
					}

					task.Wait (token);


					Response<AccountTxResult> res = task.Result;
					if (task.Result == null) {
						//TODO
						fullTxResponse.HasError = true;
						fullTxResponse.ErrorMessage += "Tasked returned null value\n";

						return fullTxResponse;
					}

		    			

					list.Add (res);

					if (res.HasError ()) {
						fullTxResponse.HasError = true;
						fullTxResponse.ErrorMessage += res.error_message;
						fullTxResponse.TroubleResponse = res;
						return fullTxResponse;
					}

					AccountTxResult accountTx = res.result;

					if (accountTx == null) {
						fullTxResponse.HasError = true;
						fullTxResponse.ErrorMessage += "Server Returned null value\n";
						return fullTxResponse;
					}

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

							fullTxResponse.HasError = true;
							fullTxResponse.ErrorMessage += "task == null\n";

							return fullTxResponse;

							//return null;
						}

						task.Wait (token);




						res = task.Result;
						if (task.Result == null) {
							// TODO
							Logging.WriteLog ("task.result == null");


							fullTxResponse.HasError = true;
							fullTxResponse.ErrorMessage += "Tasked returned null value\n";

							return null;
						}

						list.Add (res);

						if (res.HasError ()) {
							fullTxResponse.HasError = true;
							fullTxResponse.ErrorMessage += res.error_message;
							fullTxResponse.TroubleResponse = res;
							return fullTxResponse;
						}

						accountTx = res.result;


						if (accountTx == null) {
							fullTxResponse.HasError = true;
							fullTxResponse.ErrorMessage += "Server Returned null value\n";
							return fullTxResponse;
						}

						limit -= accountTx.transactions.Count ();

					}

		    			
					return fullTxResponse;

					//return list.AsEnumerable ();
				}
				, token);
		}

		public static Task< FullTxResponse > GetFullTxResult (
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
					FullTxResponse fullTxResponse = new FullTxResponse ();
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

						fullTxResponse.HasError = true;
						fullTxResponse.ErrorMessage += "task == null\n";

						return fullTxResponse;
					}

					task.Wait(token);


					Response<AccountTxResult> res = task.Result;
					if (res == null) {


						fullTxResponse.HasError = true;
						fullTxResponse.ErrorMessage += "Tasked returned null value\n";
						
						return fullTxResponse;
					}

		    			

					list.Add(res);

					if (res.HasError()) {
						fullTxResponse.HasError = true;
						fullTxResponse.ErrorMessage += res.error_message;
						fullTxResponse.TroubleResponse = res;
						return fullTxResponse;
					}

					AccountTxResult accountTx = res.result;

					if (accountTx == null) {
						fullTxResponse.HasError = true;
						fullTxResponse.ErrorMessage += "Server Returned null value\n";
						return fullTxResponse;
					}

					

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

							fullTxResponse.HasError = true;
							fullTxResponse.ErrorMessage += "task == null\n";

							return fullTxResponse;

						}

						task.Wait(token);




						res = task.Result;
						if (task.Result == null) {
							// TODO
							Logging.WriteLog("task.result == null");
							fullTxResponse.HasError = true;
							fullTxResponse.ErrorMessage += "Tasked returned null value\n";

							return fullTxResponse;

						}


						list.Add(res);

						if (res.HasError ()) {
							fullTxResponse.HasError = true;
							fullTxResponse.ErrorMessage += res.error_message;
							fullTxResponse.TroubleResponse = res;
							return fullTxResponse;
						}

						accountTx = res.result; // not redundant, needed for while loop condition 

						if (accountTx == null) {
							fullTxResponse.HasError = true;
							fullTxResponse.ErrorMessage += "Server Returned null value\n";
							return fullTxResponse;
						}


					}

					fullTxResponse.Responses = list.ToArray ();
					return fullTxResponse;
					//return list.AsEnumerable();
				}
			);
		}

	}

	public class FullTxResponse
	{

		public string ErrorMessage {
			get { return _err_mess; }
			set { _err_mess = value; }
		}

		private string _err_mess = null;

		public bool HasError {
			get {
				if (_err_mess != null) {
					return true;
				}

				return _has_err;
			}
			set {
				_has_err = value;
			}
		}

		private bool _has_err = false;
		public IEnumerable<Response<AccountTxResult>> Responses {
			get;
			set;
		}

		public Response<AccountTxResult> TroubleResponse {
			get;
			set;
		}

		
	}
}

