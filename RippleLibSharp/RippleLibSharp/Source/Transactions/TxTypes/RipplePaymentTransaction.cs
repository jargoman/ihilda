using System;

using Codeplex.Data;
//using IhildaWallet;
using RippleLibSharp.Keys;
using RippleLibSharp.Binary;
using RippleLibSharp.Util;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using System.Text;

namespace RippleLibSharp.Transactions.TxTypes
{
	public class RipplePaymentTransaction : RippleTransaction
	{

		//public String signedTransactionBlob = null;



		//public String txHash;
		//String signature;


		public RipplePaymentTransaction ()
		{

		}

		public RipplePaymentTransaction (RippleAddress Account, RippleAddress Destination, RippleCurrency amount, RippleCurrency sendmax)
		{
			this.Account = Account;
			this.Destination = Destination;
			this.Amount = amount;

			this.SendMax = sendmax;
		}

		public RipplePaymentTransaction (RippleBinaryObject serObj)
		{

			if (serObj == null) {
				throw new NullReferenceException (
					"Can not construct a payment transacton from a null binaryObject" 

				);
			}
			if (serObj.GetTransactionType () != RippleTransactionType.PAYMENT) {
				throw new Exception (
					"The RippleBinaryObject is not a payment transaction, but a " 
					+ serObj?.GetTransactionType ()?.ToString () ?? " null value"
				);
			}


			object a = serObj.GetField ( BinaryFieldType.Account );
			if (a != null) {
				RippleAddress aa = a as RippleAddress;
				string ads = a as string;
				if (aa != null) {
					Account = aa;
				} else if (ads != null) {
					try {
						Account = new RippleAddress (ads);
					} catch (Exception e) {
						Logging.WriteLog (e.Message + "\n" + e.StackTrace);
					}
				} else {
					throw new InvalidCastException ();
				}
			}



			object d = serObj.GetField (BinaryFieldType.Destination);
			if (d != null) {
				RippleAddress dd = d as RippleAddress;
				string dds = d as string;
				if (dd != null) {
					Destination = dd;
				} else if (dds != null) {
					try {
						Destination = new RippleAddress(dds);
					} catch (Exception e) {
						Logging.WriteLog (e.Message + "\n" + e.StackTrace);
					}
				} else {
					throw new InvalidCastException ();
				}
			}


			object objamount = serObj.GetField (BinaryFieldType.Amount);
			if (objamount != null) {
				Amount = (RippleCurrency)objamount;
			}


			object seqObject = serObj.GetField (BinaryFieldType.Sequence);
			if (seqObject != null) {
				Sequence = (UInt32)seqObject;
			}

			object feeObj = serObj.GetField (BinaryFieldType.Fee);
			if (feeObj != null) {
				fee = (RippleCurrency)feeObj;
			}

			object flagsObj = serObj.GetField (BinaryFieldType.Flags);
			if (flagsObj != null) {
				flags = (UInt32)flagsObj;
			}


			object sm = serObj.GetField (BinaryFieldType.SendMax);
			if (sm != null) {
				this.SendMax = (RippleCurrency)sm;
			}

			//int[] s = new int[0];
		}

		public override RippleBinaryObject GetBinaryObject ()
		{
			
			RippleBinaryObject rbo = new RippleBinaryObject();


			rbo.PutField(BinaryFieldType.TransactionType, RippleTransactionType.PAYMENT.value.Item1);
			//rbo.putField(BinaryFieldType.Flags, this.flags);
			rbo.PutField(BinaryFieldType.Sequence, this.Sequence);
			rbo.PutField (BinaryFieldType.LastLedgerSequence, this.LastLedgerSequence);



			rbo.PutField(BinaryFieldType.Amount, this.Amount);
			if (this.fee != null) {
				rbo.PutField(BinaryFieldType.Fee, this.fee);
			}
			if (this.SendMax!=null) {
				rbo.PutField(BinaryFieldType.SendMax, this.SendMax);
			}
			rbo.PutField(BinaryFieldType.Account, this.Account);

			rbo.PutField(BinaryFieldType.Destination, this.Destination);








			return rbo;
		}

		/*
		public override string GetJsonTx () {
			string s = "'{\"TransactionType\": \"Payment\"," + 
				"\"Account\": \"" + Account + "\"," + 
				"\"Fee\": " + fee.ToJsonString() + "," + 
				"\"Flags\": " + flags.ToString() + "," + 
				"\"LastLedgerSequence\": " + this.LastLedgerSequence.ToString() + "," +
				"\"Sequence\": " + Sequence.ToString() + "," + 
				"\"Amount\": " + Amount.ToJsonString() + "," + 
				"\"Destination\": \"" + this.Destination + "\"" +
				"}'";

			return s;
		}
		*/

		public override string GetJsonTx ()
		{


			StringBuilder stringBuilder = new StringBuilder ();

			stringBuilder.Append ("'{\"TransactionType\": \"Payment\",");
			stringBuilder.Append ("\"Account\": \"" + Account + "\",");
			stringBuilder.Append ("\"Fee\": " + fee.ToJsonString () + ",");
			if (flags != 0) {
				stringBuilder.Append ("\"Flags\": " + flags.ToString () + ",");
			}
			stringBuilder.Append ("\"LastLedgerSequence\": " + this.LastLedgerSequence.ToString () + ",");
			stringBuilder.Append ("\"Sequence\": " + Sequence.ToString () + ",");
			stringBuilder.Append ("\"Amount\": " + Amount.ToJsonString () + ",");
			if (SendMax != null) {
				stringBuilder.Append ("\"SendMax\": " + SendMax.ToJsonString () + ",");
			}

			if (Paths != null) {

				var pth = DynamicJson.Serialize (Paths);
				stringBuilder.Append ("\"Paths\": " + pth + ",");
			}
			stringBuilder.Append ("\"Destination\": \"" + this.Destination + "\"");
			stringBuilder.Append ("}'");

			string s = stringBuilder.ToString ();

			return s;
		}

		/*
		public Response<RippleTxResult> submit (NetworkInterface ni)
		{

			return this.submitToNetwork <RippleTxResult> (ni);

		}


*/




		new public void Submit (NetworkInterface ni)
		{

			if (this.SignedTransactionBlob == null) {
				// todo report error. Transaction must be signed first. 
				return;
			}

			object ob = new {
				command = "submit",
				tx_blob = this.SignedTransactionBlob
			};

			String json = DynamicJson.Serialize (ob);

			#if DEBUG
			Logging.WriteLog ("Sending payment: " + json + "\n");

			Logging.WriteLog ( "Amount : " + DebugRippleLibSharp.ToAssertString (Amount) + "  Sendmax : " + SendMax.ToString () );

			#endif



			if (ni != null) {
				RippleCurrency amnt = Amount;
				RippleCurrency sndmx = SendMax;



				ni.SendToServer (json);

			}
		}




	}
}

