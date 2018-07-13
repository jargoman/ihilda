using System;
using Codeplex.Data;
using RippleLibSharp.Transactions.TxTypes;

namespace RippleLibSharp.Transactions
{
	
	public class RippleSubmitTxResult
	{
		/*
		public RippleSubmitTxResult ()
		{
		}

		*/

#pragma warning disable IDE1006 // Naming Styles
		public string engine_result { get; set; }

		public int engine_result_code { get; set; }
		public string engine_result_message { get; set; }
		public string tx_blob { get; set; }

		/*
		public string tx_json { 
			get {
				return _tx_json;
			
			}
			set {
				try {
					if (DynamicJson.CanParse (value)) {
						tx_json_parsed = DynamicJson.Parse (value);
					}
				} catch (Exception e) {

				}
				_tx_json = value;
			
			}
		}
		*/

		public RippleTransaction tx_json { get; set; }

		//private string _tx_json = null;
	}



	public class TxJson
	{
		public string Account { get; set; }
		public string Fee { get; set; }
		public int Flags { get; set; }
		public int Sequence { get; set; }
		public string SigningPubKey { get; set; }
		public RippleCurrency TakerGets { get; set; }
		public RippleCurrency TakerPays { get; set; }
		public string TransactionType { get; set; }
		public string TxSignature { get; set; }
		public string hash { get; set; }
	}

	public class Request
	{
		public string command { get; set; }
		public int id { get; set; }
		public string secret { get; set; }
		public TxJson tx_json { get; set; }

#pragma warning restore IDE1006 // Naming Styles
	}

}

