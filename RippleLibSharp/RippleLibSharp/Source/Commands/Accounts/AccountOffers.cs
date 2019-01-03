using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Codeplex.Data;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;
using System.Threading;

namespace RippleLibSharp.Commands.Accounts
{
	public static class AccountOffers
	{

		public static  Task< Response<AccountOffersResult>> GetResult ( string account, NetworkInterface ni, CancellationToken token, IdentifierTag identifierTag = null ) {
			if (identifierTag == null) {
				identifierTag = new IdentifierTag {
					IdentificationNumber = NetworkRequestTask.ObtainTicket ()
				};
			}

			object o = new {
				id = identifierTag,
				command = "account_offers",
				account,
				ledger = "current"
			};

			string request = DynamicJson.Serialize (o);

			Task< Response<AccountOffersResult>> task = NetworkRequestTask.RequestResponse <AccountOffersResult> (identifierTag, request, ni, token);

			//task.Wait ();
			//return task.Result;
			return task;
		}


		public static Task < IEnumerable<Response<AccountOffersResult>> > GetFullOfferList (string account, NetworkInterface ni, CancellationToken token) {
			return Task.Run ( delegate {

				List<Response<AccountOffersResult>> list = new List<Response<AccountOffersResult>> ();

				 
				IdentifierTag identifierTag = new IdentifierTag {
					IdentificationNumber = NetworkRequestTask.ObtainTicket ()
				};

				Task<Response<AccountOffersResult>> task = GetResult (account, ni, token);

				if (task == null) {
					//return list;

					// todo throw
				}

				task.Wait (token);

				Response<AccountOffersResult> response = task.Result;

				if (response == null) {
					return list;
				}

				IEnumerable <Offer> offers = response?.result?.offers;

				if (offers != null && account != null) {

					string acc = account;
					if (Configuration.Config.PreferLinq) {
						response.result.offers = offers.Select ((Offer arg) => {
							arg.Account = acc;
							return arg;
						});
					} else {

						foreach (Offer offer in offers) {
							offer.Account = account;

						}
						
					}    
				}

				//IEnumerable<Offer> offers = response?.result?.offers;

				if ( response != null ) {
					//return list;
					list.Add (response);
				}



				while ( response?.result?.marker != null) {


					identifierTag = new IdentifierTag {
						IdentificationNumber = NetworkRequestTask.ObtainTicket ()
					};


					object o = new {
						id = identifierTag,
						command = "account_offers",
						account,
						ledger = "current",
						marker = response.result.marker.GetObject()
					};

					string request = DynamicJson.Serialize (o);
					task = NetworkRequestTask.RequestResponse <AccountOffersResult> (identifierTag, request, ni, token);
					task.Wait (token);

					response = task?.Result;



					offers = response?.result?.offers;

					if (offers != null && account != null) {
						// linq never worked ??

						if (Configuration.Config.PreferLinq) {
							response.result.offers = offers.Select ((Offer arg) => {
								arg.Account = account;
								return arg;
							});
						} else {


							foreach (Offer offer in offers) {
								offer.Account = account;

							}
						}
					}

					list.Add (response);
					

				}



				return list.AsEnumerable();

			}, token);


		}
	}
}

