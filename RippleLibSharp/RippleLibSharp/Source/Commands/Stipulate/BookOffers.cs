using System;
using System.Text;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;
using RippleLibSharp.Keys;
using System.Threading;

namespace RippleLibSharp.Commands.Stipulate
{
	public static class BookOffers
	{
		
		public static  Task<Response<BookOfferResult>> GetResult (
			RippleCurrency taker_gets,
			RippleCurrency taker_pays,
			RippleAddress taker,
			uint?  limit,
			NetworkInterface ni,
			CancellationToken token,
			IdentifierTag identifierTag = null
		
		) {

			if (identifierTag == null) {
				identifierTag = new IdentifierTag {
					IdentificationNumber = NetworkRequestTask.ObtainTicket ()
				};
			}

			//int id = NetworkRequestTask.ObtainTicket();


			string gts = DynamicJson.Serialize (taker_gets.GetAnonObjectWithoutAmount());
			string pys = DynamicJson.Serialize (taker_pays.GetAnonObjectWithoutAmount());

			// 
			StringBuilder sb = new StringBuilder ();

			sb.Append ("{");
			sb.Append ("\"id\": ");
			sb.Append (identifierTag.ToJsonString());
			sb.Append (",");

			sb.Append ("\"command\": \"book_offers\",");

			if (taker != null) {
				sb.Append ("\"taker\": \"");
				sb.Append (taker.ToString());
				sb.Append ("\",");
			}

			sb.Append ("\"taker_gets\": ");
			sb.Append (gts);

			sb.Append (",\"taker_pays\": ");
			sb.Append (pys);
			//sb.Append (",");



			if (limit != null) {
				sb.Append (",");
				sb.Append ("\"limit\": ");
				sb.Append (limit.ToString());

			}




			sb.Append ("}");

			string request = sb.ToString ();

			Task< Response<BookOfferResult>> task = NetworkRequestTask.RequestResponse <BookOfferResult> (identifierTag, request, ni, token);

			//task.Wait ();

			//return task.Result;
			return task;
		}

		public static  Task<Response<BookOfferResult>> GetResult (
			RippleCurrency taker_gets,
			RippleCurrency taker_pays,
			RippleAddress taker,
			NetworkInterface ni,
			CancellationToken token,
			IdentifierTag identifierTag = null
		) {
			return GetResult (taker_gets, taker_pays, taker, null, ni, token, identifierTag);
		}

		public static  Task<Response<BookOfferResult>> GetResult (
			RippleCurrency taker_gets,
			RippleCurrency taker_pays,
			uint? limit,
			NetworkInterface ni,
			CancellationToken token,
			IdentifierTag identifierTag = null
		) {
			return GetResult (taker_gets, taker_pays, null, limit, ni, token, identifierTag);
		}

		public static  Task<Response<BookOfferResult>> GetResult (
			RippleCurrency taker_gets,
			RippleCurrency taker_pays,
			NetworkInterface ni,
			CancellationToken token,
			IdentifierTag identifierTag = null
		) {
			return GetResult (taker_gets, taker_pays, null, null, ni, token, identifierTag);
		}


	}
}

