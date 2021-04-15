using System.Text;
using Codeplex.Data;
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

		public override string ToString ()
		{
			StringBuilder stringBuilder = new StringBuilder ();

			stringBuilder.Append ("Account set ");

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
			stringBuilder.Append ("{\"TransactionType\": \"AccountSet\",");
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


			if (this.Domain != null) {
				stringBuilder.Append ("\"Domain\": " + this.Domain + ",");
			}

			if (ClearFlag != null) {
				stringBuilder.Append ("\"ClearFlag\": " + ClearFlag.ToString () + ",");
			}

			if (this.SetFlag != null) {
				stringBuilder.Append ("\"SetFlag\": " + SetFlag.ToString () + ",");
			}


			if (this.TransferRate != null) {
				stringBuilder.Append ("\"TransferRate\": " + TransferRate.ToString () + ",");
			}

			if (EmailHash != null) {
				stringBuilder.Append ("\"EmailHash\": " + EmailHash + ",");
			}

			if (this.MessageKey != null) {
				stringBuilder.Append ("\"MessageKey\": \"" + MessageKey.ToString () + "\"");
			}


			if (this.TickSize != null) {
				stringBuilder.Append ("\"TickSize\": " + TickSize.ToString () + ",");
			}


			//stringBuilder.Append ("\"Sequence\": " + Sequence.ToString ());




			stringBuilder.Append ("}");



			return stringBuilder.ToString ();
		}



		public uint? TransferRate {
			get;
			set;
		}

		public uint? ClearFlag {
			get;
			set;
		}

		public uint? SetFlag {
			get;
			set;
		}

		public string Domain {
			get;
			set;
		}

		public string MessageKey {
			get;
			set;
		}

		public string EmailHash {
			get;
			set;
		}

		public byte? TickSize {
			get;
			set;
		} 

	}

	public enum AccountSetFlags
	{
		asfAccountTxnID = 5,
		asfDefaultRipple = 8,
		asfDepositAuth = 9,
		asfDisableMaster = 4,
		asfDisallowXRP = 3,
		asfGlobalFreeze = 7,
		asfNoFreeze = 6,
		asfRequireAuth = 2,
		asfRequireDest = 1
	}
	
}

