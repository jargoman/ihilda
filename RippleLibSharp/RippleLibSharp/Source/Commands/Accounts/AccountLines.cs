using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Codeplex.Data;
using RippleLibSharp.Network;
using RippleLibSharp.Result;

using RippleLibSharp.Keys;
using RippleLibSharp.Transactions;

using RippleLibSharp.Trust;
using System.Threading;

namespace RippleLibSharp.Commands.Accounts
{
	public static class AccountLines
	{
		

		public static Task<Response<AccountLinesResult>> GetResult ( string account, NetworkInterface ni, CancellationToken token, IdentifierTag identifierTag = null ) {

			if (identifierTag == null) {
				identifierTag = new IdentifierTag {
					IdentificationNumber = NetworkRequestTask.ObtainTicket ()
				};
			}

			object o = new {
				id = identifierTag,
				command = "account_lines",
				account,
				ledger = "current"
			};

			string request = DynamicJson.Serialize (o);

			Task< Response<AccountLinesResult>> task = 
				NetworkRequestTask.RequestResponse <AccountLinesResult> ( identifierTag, request, ni, token );

			//task.Wait ();

			//return task.Result;
			return task;

		}


		public static IEnumerable<Response<AccountLinesResult>> GetResultFull (string account, NetworkInterface ni, CancellationToken token) {

			List<Response<AccountLinesResult>> results = new List<Response<AccountLinesResult>> ();

			Task<Response<AccountLinesResult>> task = GetResult (account, ni, token);
			if (task == null) {
				return null;
			}
			task.Wait (token);

			Response<AccountLinesResult> resp = task.Result;
		
			results.Add (resp);



			AccountLinesResult res = resp.result;



			while (res?.marker != null && !token.IsCancellationRequested) {
				
				IdentifierTag identifierTag = new IdentifierTag {
					IdentificationNumber = NetworkRequestTask.ObtainTicket ()
				};

				object o = new {
					id = identifierTag,
					command = "account_lines",
					account,
					ledger = "current",
					marker = res?.marker?.GetObject()
				};

				string request = DynamicJson.Serialize (o);

				Task< Response<AccountLinesResult>> task2 = 
					NetworkRequestTask.RequestResponse <AccountLinesResult> (identifierTag, request, ni, token);
				
				task2.Wait (token);

				resp = task2.Result;

				results.Add (resp);
				res = resp.result;



			}

			return results;

		}


		public static TrustLine[] GetTrustLines (string account, NetworkInterface ni, CancellationToken token) {
			if (account == null || ni == null) {
				throw new NullReferenceException ();
			}

			Task<Response<AccountLinesResult>> task = AccountLines.GetResult (account, ni, token);

			task.Wait (token);

			Response <AccountLinesResult> resp = task.Result;
			if (resp == null) {
				return null;
			}

			AccountLinesResult result = resp.result;

			return result?.lines;

		}


		public static IEnumerable<RippleCurrency> GetCurrencyBalances (RippleAddress ra, String currency, NetworkInterface ni, CancellationToken token) {
			

			Task<Response<AccountLinesResult>> task = AccountLines.GetResult (
				ra,
				ni,
				token

			);

			if (task?.Result == null) {

				return null;
			}

			Response<AccountLinesResult> response = task.Result;

			if (response.HasError()) {
				return null;
			}

			AccountLinesResult result = response.result;

			IEnumerable<RippleCurrency> list = result.GetBalanceAsCurrency (currency);

			return list;
		}

		public static Decimal GetCurrencyAsSingleBalance (RippleAddress ra, String currency, NetworkInterface ni, CancellationToken token) {

			IEnumerable<RippleCurrency> balances = GetCurrencyBalances (ra, currency, ni, token);
			Decimal totalbalance = 0;
			if (!Configuration.Config.PreferLinq) {

				if (balances == null) {
					return totalbalance;
				}

				foreach (RippleCurrency rc in balances) {
					totalbalance += rc.amount;
				}
			} else {

				totalbalance = balances.Sum ((RippleCurrency arg) => arg.amount);
			}
			return totalbalance;

		}


		public static IEnumerable<string> GetIssuersForCurrency( string cur, RippleAddress address, NetworkInterface ni, CancellationToken token) {
			IEnumerable<RippleCurrency> list = GetCurrencyBalances (address, cur, ni, token);
			if (list == null) {
				return null;
			}

			if (Configuration.Config.PreferLinq) {
				var v = from RippleCurrency rc in list
					where rc != null
				&& rc.issuer != null
					select rc.issuer;

				return v;
			} else {
				var ret = new List<string> ();
				foreach (RippleCurrency currency in list) {
					if (currency == null) {
						continue;
					}

					if (currency.issuer == null) {
						continue;
					}
					ret.Add (currency.issuer);
				}

				return ret;
			}
		}

		public static RippleCurrency GetBalanceForIssuer ( string cur, RippleAddress issuer, RippleAddress address, NetworkInterface ni, CancellationToken token ) {
			if (address == null) {
				return null;
			}
			if (issuer == null) {
				return null;
			}

			IEnumerable<RippleCurrency> balances = GetCurrencyBalances (address, cur, ni, token);
			if (Configuration.Config.PreferLinq) {

				balances = balances.Where ((RippleCurrency currency) => issuer.ToString ().Equals (currency?.issuer));
				return balances.FirstOrDefault ();
			} else {
			
				foreach ( RippleCurrency currency in balances ) {
					if (issuer.ToString().Equals(currency?.issuer)) {
						return currency;
					}
				}

				return null;
			}


		}


	}
}

