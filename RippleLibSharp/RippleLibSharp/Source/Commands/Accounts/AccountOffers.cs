using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Codeplex.Data;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;

namespace RippleLibSharp.Commands.Accounts
{
	public static class AccountOffers
	{

		public static  Task< Response<AccountOffersResult>> GetResult ( string account, NetworkInterface ni ) {

			int id = NetworkRequestTask.ObtainTicket();
			object o = new {
				id,
				command = "account_offers",
				account,
				ledger = "current"
			};

			string request = DynamicJson.Serialize (o);

			Task< Response<AccountOffersResult>> task = NetworkRequestTask.RequestResponse <AccountOffersResult> (id, request, ni);

			//task.Wait ();
			//return task.Result;
			return task;
		}


		public static Task < IEnumerable<Response<AccountOffersResult>> > GetFullOfferList (string account, NetworkInterface ni) {
			return Task.Run ( delegate {  

				List<Response<AccountOffersResult>> list = new List<Response<AccountOffersResult>> ();

				Task<Response<AccountOffersResult>> task = GetResult (account, ni);

				task.Wait ();

				Response<AccountOffersResult> response = task?.Result;



				Offer[] offers = response?.result?.offers;

				if (offers != null && account != null) {	foreach (Offer o in offers) {

						o.Account = account;
				}}

				//IEnumerable<Offer> offers = response?.result?.offers;

				if (response != null) {
					//return list;
					list.Add (response);
				}



				while ( response?.result?.marker != null) {
					int id = NetworkRequestTask.ObtainTicket ();
					object o = new {
						id,
						command = "account_offers",
						account,
						ledger = "current",
						marker = response.result.marker.GetObject()
					};

					string request = DynamicJson.Serialize (o);
					task = NetworkRequestTask.RequestResponse <AccountOffersResult> (id, request, ni);
					task.Wait ();

					response = task?.Result;



					offers = response?.result?.offers;

					if (offers != null && account != null) {	foreach (Offer of in offers) {

							of.Account = account;
						}}

					list.Add(response);

				}

				IEnumerable<Response<AccountOffersResult>> ie = list;

				return ie;

			});


		}
	}
}

