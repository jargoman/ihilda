using System;
using RippleLibSharp.Transactions;
using RippleLibSharp.Binary;
using System.Text;
using Codeplex.Data;

namespace RippleLibSharp.Transactions.TxTypes
{
	public class RippleSetRegularKey : RippleTransaction
	{
		/*
		public RippleSetRegularKey ()
		{
		}
		*/

		new public RippleBinaryObject GetBinaryObject () {
			RippleBinaryObject rbo = new RippleBinaryObject();
			rbo.PutField(BinaryFieldType.TransactionType, RippleTransactionType.REGULAR_KEY_SET.value.Item1);


			return rbo;
		}

		public override string ToString ()
		{
			StringBuilder stringBuilder = new StringBuilder ();

			stringBuilder.Append ("SetRegularKey");
			stringBuilder.Append (" for Account ");
			stringBuilder.AppendLine (Account ?? "null");
			stringBuilder.Append (" to ");
			stringBuilder.Append (RegularKey ?? "null");
			return stringBuilder.ToString ();
		}

		public override string GetJsonTx ()
		{


			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append ("'");
			stringBuilder.Append (GetJsonTxDotNet ());
			stringBuilder.Append ("'");

			return stringBuilder.ToString ();
		}


		public override string GetJsonTxDotNet ()
		{
			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append ("{\"TransactionType\": \"SetRegularKey\",");
			stringBuilder.Append ("\"Account\": \"" + Account + "\",");
			stringBuilder.Append ("\"Fee\": " + fee.ToJsonString () + ",");
			//stringBuilder.Append ("\"Flags\": " + flags.ToString () + ",");

			if (LastLedgerSequence != 0) {
				stringBuilder.Append ("\"LastLedgerSequence\": " + this.LastLedgerSequence.ToString () + ",");
			}

			stringBuilder.Append ("\"Sequence\": " + Sequence.ToString () + ",");

			if (this.Memos != null) {
				stringBuilder.Append ("\"Memos\": " + DynamicJson.Serialize (this.Memos) + ",");
			}


			stringBuilder.Append ("\"RegularKey\": \"" + RegularKey.ToString () + "\"");


			stringBuilder.Append ("}");



			return stringBuilder.ToString ();
		}



	}
}

