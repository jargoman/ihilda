using System;
using RippleLibSharp.Transactions;
using RippleLibSharp.Binary;

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


	}
}

