using System;
using Codeplex.Data;

using System.Threading.Tasks;

using RippleLibSharp.Binary;
using RippleLibSharp.Util;
using RippleLibSharp.Keys;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;
using System.Text;

namespace RippleLibSharp.Transactions.TxTypes
{
	public class RippleTrustSetTransaction : RippleTransaction
	{
		//public RippleAddress account;


		//UInt32 qualityIn = null;
		//UInt32 qualityOut = null;
		public RippleTrustSetTransaction ()
		{

		}

		public RippleTrustSetTransaction (RippleTransaction tx)
		{
#if DEBUG


			String method_sig = clsstr + nameof(RippleTrustSetTransaction) + DebugRippleLibSharp.left_parentheses + nameof (RippleTransaction) + DebugRippleLibSharp.space_char + nameof(tx) + DebugRippleLibSharp.right_parentheses;
			if (DebugRippleLibSharp.RippleTrustSetTransaction) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}

#endif

			if (tx == null) {
				throw new NullReferenceException ();
			}

			if (RippleTransactionType.TRUST_SET != tx.TransactionType) {
				throw new InvalidCastException ();
			}

			this.Account = tx.Account;
			this.LimitAmount = tx.LimitAmount;

			this.qualityIn = tx.qualityIn;
			this.qualityOut = tx.qualityOut;

			this.flags = tx.flags;

			this.fee = tx.fee;
		}

		public RippleTrustSetTransaction (RippleAddress account, RippleCurrency LimitAmount)
		{
#if DEBUG
			String method_sig = clsstr + nameof(RippleTrustSetTransaction) + DebugRippleLibSharp.left_parentheses + nameof(account) + DebugRippleLibSharp.equals + (account?.ToString () ?? "null") + DebugRippleLibSharp.comma + nameof (LimitAmount) + DebugRippleLibSharp.equals + LimitAmount?.ToString() ?? "null" + DebugRippleLibSharp.right_parentheses;

			if (DebugRippleLibSharp.RippleTrustSetTransaction) {
				Logging.WriteLog (method_sig);
			}
#endif

			this.Account = account;
			this.LimitAmount = LimitAmount;



		}



		public RippleTrustSetTransaction (RippleAddress account, RippleCurrency LimitAmount, UInt32? qualityIn, UInt32? qualityOut) : this (account, LimitAmount)
		{
#if DEBUG
			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append (clsstr);
			stringBuilder.Append (nameof (RippleTrustSetTransaction));
			stringBuilder.Append (DebugRippleLibSharp.colon);
			stringBuilder.Append (nameof (qualityIn));
			stringBuilder.Append (DebugRippleLibSharp.equals);
			stringBuilder.Append (DebugRippleLibSharp.ToAssertString (qualityIn));
			stringBuilder.Append (DebugRippleLibSharp.comma);
			stringBuilder.Append (nameof (qualityOut));
			stringBuilder.Append (DebugRippleLibSharp.equals);
			stringBuilder.Append (DebugRippleLibSharp.ToAssertString (qualityOut));
			stringBuilder.Append (DebugRippleLibSharp.colon);
			String method_sig =  stringBuilder.ToString () ;
			if (DebugRippleLibSharp.RippleTrustSetTransaction) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			this.qualityIn = qualityIn == null ? 0 : (UInt32)qualityIn;
			this.qualityOut = qualityOut == null ? 0 : (UInt32)qualityOut;

			this.flags = 0x00040000;

			//this.sequenceNumber = sequencenumber;
		}

		public RippleTrustSetTransaction (RippleBinaryObject serObj)
		{
			if (serObj.GetTransactionType () != RippleTransactionType.TRUST_SET) {
				throw new Exception ("The RippleBinaryObject is not a trustset transaction, but a " + serObj.GetTransactionType ().ToString ());
			}

			this.Account = (RippleAddress)serObj.GetField (BinaryFieldType.Account);
			this.LimitAmount = (RippleCurrency)serObj.GetField (BinaryFieldType.LimitAmount);

			Sequence = (UInt32)serObj.GetField (BinaryFieldType.Sequence);

			this.qualityIn = (UInt32)serObj.GetField (BinaryFieldType.QualityIn);
			this.qualityOut = (UInt32)serObj.GetField (BinaryFieldType.QualityOut);

			fee = (RippleCurrency)serObj.GetField (BinaryFieldType.Fee);

			flags = (UInt32)serObj.GetField (BinaryFieldType.Flags);
		}

		public override RippleBinaryObject GetBinaryObject ()
		{
			RippleBinaryObject rbo = new RippleBinaryObject ();

			// UINT
			rbo.PutField (BinaryFieldType.TransactionType, RippleTransactionType.TRUST_SET.value.Item1);

			// UINT32
			rbo.PutField (BinaryFieldType.Flags, this.flags);

			//UINT32
			rbo.PutField (BinaryFieldType.Sequence, this.Sequence);

			// UINT32
			rbo.PutField (BinaryFieldType.LastLedgerSequence, this.LastLedgerSequence);

			rbo.PutField (BinaryFieldType.Account, (RippleAddress)this.Account);

			rbo.PutField (BinaryFieldType.LimitAmount, this.LimitAmount);


			rbo.PutField (BinaryFieldType.Fee, this.fee);

			//if (this.qualityIn!=null) {
			rbo.PutField (BinaryFieldType.QualityIn, qualityIn);
			//}
			//if (this.qualityOut!=null) {
			rbo.PutField (BinaryFieldType.QualityOut, qualityOut);
			//}



			return rbo;
		}

		public override string GetJsonTxDotNet ()
		{
			string s = "{\"TransactionType\": \"TrustSet\"," +
				"\"Account\": \"" + Account + "\"," +
				"\"Fee\": " + fee.ToJsonString () + "," +
				"\"Flags\": " + flags.ToString () + "," +
				"\"LastLedgerSequence\": " + this.LastLedgerSequence.ToString () + "," +
				"\"Sequence\": " + Sequence.ToString () + "," +
				"\"LimitAmount\": " + LimitAmount.ToJsonString () + /*"," +*/
										    /*"\"Destination\": \"" + this.Destination + "\"" +*/
				"}";

			return s;
		}


		public override string GetJsonTx ()
		{
			string s = "'{\"TransactionType\": \"TrustSet\"," +
				"\"Account\": \"" + Account + "\"," +
				"\"Fee\": " + fee.ToJsonString () + "," +
				"\"Flags\": " + flags.ToString () + "," +
				"\"LastLedgerSequence\": " + this.LastLedgerSequence.ToString () + "," +
				"\"Sequence\": " + Sequence.ToString () + "," +
				"\"LimitAmount\": " + LimitAmount.ToJsonString () + /*"," +*/
																	/*"\"Destination\": \"" + this.Destination + "\"" +*/
				"}'";

			return s;
		}

		public override string ToString ()
		{

			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append ("Trust ");
			stringBuilder.Append (LimitAmount.issuer ?? "null");
			stringBuilder.Append (" for ");
			stringBuilder.Append (LimitAmount.amount);
			stringBuilder.Append (" ");
			stringBuilder.Append (LimitAmount.currency);

			return stringBuilder.ToString ();
		}



#if DEBUG
		private const string clsstr = nameof (RippleTrustSetTransaction) + DebugRippleLibSharp.colon;
#endif

	}
}

