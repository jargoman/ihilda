using System;
using RippleLibSharp.Transactions;
using RippleLibSharp.Transactions.TxTypes;

using RippleLibSharp.Keys;
using RippleLibSharp.Binary;

namespace RippleLibSharp.Transactions.TxTypes
{
	public class RippleCancelTransaction : RippleTransaction
	{
		/*
		public RippleCancelTransaction ()
		{


		}
		*/


		public override RippleBinaryObject GetBinaryObject () {

			RippleBinaryObject rbo = new RippleBinaryObject();

			rbo.PutField(BinaryFieldType.TransactionType, RippleTransactionType.OFFER_CANCEL.value.Item1);


			rbo.PutField(BinaryFieldType.Account, ( RippleAddress )this.Account);

			//rbo.putField(BinaryFieldType.LimitAmount, this.amount);

			rbo.PutField(BinaryFieldType.OfferSequence, this.OfferSequence);

			rbo.PutField(BinaryFieldType.Fee, this.fee);

			rbo.PutField(BinaryFieldType.Flags, this.flags);

			rbo.PutField (BinaryFieldType.LastLedgerSequence, this.LastLedgerSequence);

			return rbo;
		}

		public override string GetJsonTx () {
			string s = "'{\"TransactionType\": \"OfferCancel\"," + 
				"\"Account\": \"" + Account + "\"," + 
				"\"Fee\": " + fee.ToJsonString() + "," + 
				//"\"Flags\": " + flags.ToString() + "," + 
				"\"LastLedgerSequence\": " + this.LastLedgerSequence.ToString() + "," +
				"\"Sequence\": " + Sequence.ToString() + "," + 
				"\"OfferSequence\": " + OfferSequence.ToString() + "" + 
				"}'";

			return s;
		}

	}
}

