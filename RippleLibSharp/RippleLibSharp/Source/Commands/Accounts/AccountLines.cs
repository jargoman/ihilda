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



namespace RippleLibSharp.Commands.Accounts
{
	public static class AccountLines
	{
		

		public static Task<Response<AccountLinesResult>> GetResult ( string account, NetworkInterface ni ) {

			int id = NetworkRequestTask.ObtainTicket();
			object o = new {
				id,
				command = "account_lines",
				account,
				ledger = "current"
			};

			string request = DynamicJson.Serialize (o);

			Task< Response<AccountLinesResult>> task = 
				NetworkRequestTask.RequestResponse <AccountLinesResult> (id, request, ni);

			//task.Wait ();

			//return task.Result;
			return task;

		}


		public static IEnumerable<Response<AccountLinesResult>> GetResultFull (string account, NetworkInterface ni) {

			List<Response<AccountLinesResult>> results = new List<Response<AccountLinesResult>> ();

			Task<Response<AccountLinesResult>> task = GetResult (account, ni);
			if (task == null) {
				return null;
			}
			task.Wait ();

			Response<AccountLinesResult> resp = task.Result;
		
			results.Add (resp);



			AccountLinesResult res = resp.result;



			while (res?.marker != null) {
				
				int id = NetworkRequestTask.ObtainTicket();
				object o = new {
					id,
					command = "account_lines",
					account,
					ledger = "current",
					marker = res?.marker?.GetObject()
				};

				string request = DynamicJson.Serialize (o);

				Task< Response<AccountLinesResult>> task2 = 
					NetworkRequestTask.RequestResponse <AccountLinesResult> (id, request, ni);
				
				task2.Wait ();

				resp = task2.Result;

				results.Add (resp);
				res = resp.result;



			}

			return results;

		}


		public static TrustLine[] GetTrustLines (string account, NetworkInterface ni) {
			if (account == null || ni == null) {
				throw new NullReferenceException ();
			}

			Task<Response<AccountLinesResult>> task = AccountLines.GetResult (account, ni);

			task.Wait ();

			Response <AccountLinesResult> resp = task.Result;
			if (resp == null) {
				return null;
			}

			AccountLinesResult result = resp.result;
			if (result == null) {
				return null;
			}

			return result.lines;


		}


		public static List<RippleCurrency> GetCurrencyBalances (RippleAddress ra, String currency, NetworkInterface ni) {
			

			Task<Response<AccountLinesResult>> task = AccountLines.GetResult (
				ra,
				ni

			);

			if (task?.Result == null) {

				return null;
			}

			Response<AccountLinesResult> response = task.Result;

			if (response.HasError()) {
				return null;
			}

			AccountLinesResult result = response.result;

			List<RippleCurrency> list = result.GetBalanceAsCurrency (currency);

			return list;
		}

		public static Decimal GetCurrencyAsSingleBalance (RippleAddress ra, String currency, NetworkInterface ni) {

			List<RippleCurrency> balances = GetCurrencyBalances (ra, currency, ni);

			Decimal totalbalance = 0;
			if (balances == null) {
				return totalbalance;
			}

			foreach (RippleCurrency rc in balances) {
				totalbalance += rc.amount;
			}

			return totalbalance;

		}


		public static List<string> GetIssuersForCurrency( string cur, RippleAddress address, NetworkInterface ni) {
			List<RippleCurrency> list = GetCurrencyBalances (address, cur, ni);
			if (list == null) {
				return null;
			}

			var v = from RippleCurrency rc in list
					where rc != null
				&& rc.issuer != null
				select rc.issuer;

			return v.ToList ();
		}

		public static RippleCurrency GetBalanceForIssuer ( string cur, RippleAddress issuer, RippleAddress address, NetworkInterface ni ) {
			if (address == null) {
				return null;
			}
			if (issuer == null) {
				return null;
			}


			List<RippleCurrency> balances = GetCurrencyBalances ( address, cur, ni);

			foreach ( RippleCurrency currency in balances ) {
				if (issuer.ToString().Equals(currency?.issuer)) {
					return currency;
				}
			}

			return null;
		}


	}
}

