using System;
using Codeplex.Data;
using RippleLibSharp.Keys;
using RippleLibSharp.Util;
using RippleLibSharp.Binary;
using RippleLibSharp.Network;
using System.Text;

namespace RippleLibSharp.Transactions.TxTypes
{
	public class RippleOfferTransaction : RippleTransaction
	{
		public RippleOfferTransaction ()
		{

		}

		public RippleOfferTransaction (RippleAddress account, Offer off) 
		{
			
			this.SetOffer (account, off);
		}

		public void SetOffer(RippleAddress account, Offer off) {
			this.TakerPays = off.TakerPays.DeepCopy();
			this.TakerGets = off.TakerGets.DeepCopy();

			this.Account = account;

			this.Memos = off.Memos;
		}

		public RippleOfferTransaction (RippleBinaryObject serObj)
		{
			if (serObj.GetTransactionType () != RippleTransactionType.OFFER_CREATE) {
				throw new Exception ("The RippleBinaryObject is not an offer transaction, but a " + serObj.GetTransactionType ().ToString ());
			}

			this.Account = ((RippleAddress) serObj.GetField(BinaryFieldType.Account)).ToString();

			this.TakerGets = (RippleCurrency) serObj.GetField( BinaryFieldType.TakerGets );
			this.TakerPays = (RippleCurrency) serObj.GetField ( BinaryFieldType.TakerPays );

			Sequence = (UInt32)serObj.GetField (BinaryFieldType.Sequence);

			Memos = (MemoIndice[])serObj.GetField (BinaryFieldType.Memos);  // This might be a tough cookie to keep type safety

			fee = (RippleCurrency)serObj.GetField (BinaryFieldType.Fee);

			flags = (UInt32)serObj.GetField (BinaryFieldType.Flags);
		}


		public override RippleBinaryObject GetBinaryObject ()
		{


			RippleBinaryObject rbo = new RippleBinaryObject();

			// UINT16
			rbo.PutField(BinaryFieldType.TransactionType, RippleTransactionType.OFFER_CREATE.value.Item1);

			// UINT32
			rbo.PutField(BinaryFieldType.Flags, this.flags);

			// UINT 32
			rbo.PutField(BinaryFieldType.Sequence, this.Sequence);

			// UINT32
			rbo.PutField (BinaryFieldType.LastLedgerSequence, this.LastLedgerSequence);

			// Amount
			rbo.PutField(BinaryFieldType.TakerPays, this.TakerPays);

			// Amount
			rbo.PutField(BinaryFieldType.TakerGets, this.TakerGets);


			// Amount
			rbo.PutField(BinaryFieldType.Fee, fee);


			// Account
			rbo.PutField(BinaryFieldType.Account, (RippleAddress)this.Account);





			return rbo;
		}


		public override string GetJsonTx () {


			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append("'");
			stringBuilder.Append (GetJsonTxDotNet());
			stringBuilder.Append ("'");

			return stringBuilder.ToString();
		}

		public override string GetJsonTxDotNet ()
		{
			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append ("{\"TransactionType\": \"OfferCreate\",");
			stringBuilder.Append ("\"Account\": \"" + Account + "\",");
			stringBuilder.Append ("\"Fee\": " + fee.ToJsonString () + ",");
			stringBuilder.Append ("\"Flags\": " + flags.ToString () + ",");

			if (LastLedgerSequence != 0) {
				stringBuilder.Append ("\"LastLedgerSequence\": " + this.LastLedgerSequence.ToString () + ",");
			}
			stringBuilder.Append ("\"Sequence\": " + Sequence.ToString () + ",");

			if (this.Memos != null) {
				stringBuilder.Append ("\"Memos\": " + DynamicJson.Serialize (this.Memos) + ",");
			}

			stringBuilder.Append ("\"TakerGets\": " + TakerGets.ToJsonString () + ",");
			stringBuilder.Append ("\"TakerPays\": " + TakerPays.ToJsonString () + "");

			stringBuilder.Append ("}");



			return stringBuilder.ToString ();
		}

		/*
		public void submit ( NetworkInterface ni )
		{

			if (this.signedTransactionBlob == null) {
				// todo report error. Transaction must be signed first. 
				return;
			}

			//if (this.offer == null) {
				// todo. debug
				return;
			//}

			if (TakerPays == null) {
				// todo debug
				return;
			}

			if (TakerGets == null) {
				// todo debug
				return;
			}

			object ob = new {
				command = "submit",
				tx_blob = this.signedTransactionBlob
			};

			String json = DynamicJson.Serialize (ob);

			Logging.writeLog ("Sending offer transaction: " + json + "\n");

			//Logging.write("Amount : " + amount.ToString() + "  Sendmax : " + sendmax.ToString());



			if (ni != null) {
				

				ni.sendToServer (json);

			}





		}
	*/

		public override string ToString ()
		{
			StringBuilder stringBuilder = new StringBuilder ();

			stringBuilder.Append ("Sell ");
			stringBuilder.Append (TakerGets.ToString());
			stringBuilder.Append (" for ");
			stringBuilder.Append (TakerPays.ToString());

			return stringBuilder.ToString ();
		}


	}
}

