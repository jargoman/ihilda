using System;
using RippleLibSharp.Binary;

namespace RippleLibSharp.Transactions.TxTypes
{
	public class RippleAccountSetTransaction : RippleTransaction
	{
		/*
		public RippleAccountSetTransaction ()
		{
		}
		*/


		public override RippleBinaryObject GetBinaryObject () {
			RippleBinaryObject rbo = new RippleBinaryObject();
			rbo.PutField(BinaryFieldType.TransactionType, RippleTransactionType.ACCOUNT_SET.value.Item1);


			return rbo;
		}


	}
}

