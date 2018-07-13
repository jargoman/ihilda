using System;
using Codeplex.Data;
using RippleLibSharp.Network;
using RippleLibSharp.Transactions;
using RippleLibSharp.Transactions.TxTypes;

namespace RippleLibSharp.Network
{
	public class PingObject
	{

#pragma warning disable IDE1006 // Naming Styles
		public int id { get; set; }


		public string status { get; set; }

		public string type { get; set; }

		public string error { get; set; }
		public int error_code { get; set; }
		public string error_message { get; set; }

#pragma warning restore IDE1006 // Naming Styles

		public bool HasError () {

			if (status == null) {
				return false;
			}

			return "error".Equals (status);

		}


	}


}

namespace RippleLibSharp.Result
{
	public class Response<T> : PingObject {

#pragma warning disable IDE1006 // Naming Styles
		public T result { get; set; }

		public RippleTxStructure transaction {
			get;
			set;
		}

		public Request request { get; set; }

#pragma warning restore IDE1006 // Naming Styles

		public Response<T> SetFromJsonResp (Response<Json_Response> jsonResp) {
			if (jsonResp == null) {
				// TODO debug
				return null;
			}

			this.id = jsonResp.id;

			this.status = jsonResp.status;

			this.type = jsonResp.type;

			this.error = jsonResp.error;
			this.error_code = jsonResp.error_code;
			this.error_message = jsonResp.error_message;

			if (jsonResp.result != null) {
				Json_Response resp = jsonResp.result;

				result = DynamicJson.Parse (resp.json_text, System.Text.Encoding.Default);
			}

			return this;

		}
	}


	public class Request {
#pragma warning disable IDE1006 // Naming Styles
		public string account { get; set; }

		public string command { get; set; }
		public int id { get; set; }
		public string secret { get; set; }
		public string ledger { get; set; }
		public RippleTransaction tx_json { get; set; }

	}

	public class Json_Response {
		public Json_Response () {

		}

		public Json_Response (string s) {
			json_text = s;
		}

		public string json_text {
			get;
			set;
		}
	}

#pragma warning restore IDE1006 // Naming Styles

}

