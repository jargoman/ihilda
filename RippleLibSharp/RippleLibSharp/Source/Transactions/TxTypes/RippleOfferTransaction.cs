using System;
using Codeplex.Data;
using RippleLibSharp.Keys;
using RippleLibSharp.Util;
using RippleLibSharp.Binary;
using RippleLibSharp.Network;

namespace RippleLibSharp.Transactions.TxTypes
{
	public class RippleOfferTransaction : RippleTransaction
	{
		public RippleOfferTransaction (RippleAddress account, Offer off) 
		{
			
			this.SetOffer (account, off);
		}

		public void SetOffer(RippleAddress account, Offer off) {
			this.TakerPays = off.TakerPays;
			this.TakerGets = off.TakerGets;

			this.Account = account;
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
			string s = "'{\"TransactionType\": \"OfferCreate\"," + 
				"\"Account\": \"" + Account + "\"," + 
				"\"Fee\": " + fee.ToJsonString() + "," + 
				"\"Flags\": " + flags.ToString() + "," + 
				"\"LastLedgerSequence\": " + this.LastLedgerSequence.ToString() + "," +
				"\"Sequence\": " + Sequence.ToString() + "," + 
				"\"TakerGets\": " + TakerGets.ToJsonString() + "," + 
				"\"TakerPays\": " + TakerPays.ToJsonString() + "" +
				"}'";

			return s;
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




	}
}

