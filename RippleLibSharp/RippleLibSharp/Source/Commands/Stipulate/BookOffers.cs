using System;
using System.Text;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;
using RippleLibSharp.Keys;

namespace RippleLibSharp.Commands.Stipulate
{
	public static class BookOffers
	{

		public static  Task<Response<BookOfferResult>> GetResult (
			RippleCurrency taker_gets,
			RippleCurrency taker_pays,
			RippleAddress taker,
			int?  limit,
			NetworkInterface ni
		
		) {

			int id = NetworkRequestTask.ObtainTicket();


			string gts = DynamicJson.Serialize (taker_gets.GetAnonObjectWithoutAmount());
			string pys = DynamicJson.Serialize (taker_pays.GetAnonObjectWithoutAmount());

			// 
			StringBuilder sb = new StringBuilder ();

			sb.Append ("{");
			sb.Append ("\"id\": ");
			sb.Append (id.ToString());
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

			Task< Response<BookOfferResult>> task = NetworkRequestTask.RequestResponse <BookOfferResult> (id, request, ni);

			//task.Wait ();

			//return task.Result;
			return task;
		}

		public static  Task<Response<BookOfferResult>> GetResult (
			RippleCurrency taker_gets,
			RippleCurrency taker_pays,
			RippleAddress taker,
			NetworkInterface ni
		) {
			return GetResult (taker_gets, taker_pays, taker, null, ni);
		}

		public static  Task<Response<BookOfferResult>> GetResult (
			RippleCurrency taker_gets,
			RippleCurrency taker_pays,
			int? limit,
			NetworkInterface ni
		) {
			return GetResult (taker_gets, taker_pays, null, limit, ni);
		}

		public static  Task<Response<BookOfferResult>> GetResult (
			RippleCurrency taker_gets,
			RippleCurrency taker_pays,
			NetworkInterface ni
		) {
			return GetResult (taker_gets, taker_pays, null, null, ni);
		}


	
	}
}

