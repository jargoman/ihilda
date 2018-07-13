using System;
using System.Threading;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Binary;
using RippleLibSharp.Keys;
using RippleLibSharp.Paths;

using RippleLibSharp.Util;
using RippleLibSharp.Result;
using RippleLibSharp.Network;

using RippleLibSharp.LocalRippled;

using RippleLibSharp.Commands.Accounts;

namespace RippleLibSharp.Transactions.TxTypes
{
	public class RippleTransaction
	{
		public RippleTransaction ()
		{



			tfPartialPayment = 0x00020000;
			tfNoDirectRipple = 0x00010000;
			tfLimitQuality = 0x00040000;

		}

#region properties_declaration
		#pragma warning disable IDE1006 // Naming Styles
		public String Account { get; set; }



		//private UInt32 seq = 0;
		public UInt32 OfferSequence {
			get;
			set;
		}
		public UInt32 Sequence {
			get;
			set;

		}


		public UInt32 flags {

			get;
			set;
		}
		public UInt32 LastLedgerSequence {
			get;
			set;
		}
		public UInt32 date {
			get;
			set;
		}


		public RippleMemo [] Memos {
			get;
			set;
		}
		public RippleCurrency fee {
			get { return _fee; }
			set { _fee = value; }
		}

		public RippleCurrency Fee {
			get { return _fee; }
			set { _fee = value; }
		}

		private RippleCurrency _fee {
			get;
			set;
		}
		public string ledger_index {
			get;
			set;

		}


		public string SigningPubKey {
			get;
			set;
		}
		public string hash {
			get;
			set;
		}

		public string AccountTxnID {
			get;
			set;
		}

		public string TransactionType {
			get;
			set;
		}


		RippleSigner signer {
			get;
			set;
		}



		protected string SignedTransactionBlob {
			get;
			set;
		}


		public RippleCurrency LimitAmount {
			get;
			set;
		}


		public UInt32 qualityIn {
			get;
			set;
		}
		public UInt32 qualityOut {
			get;
			set;
		}


		public string RegularKey {
			get;
			set;
		}



		public string Destination {
			get;
			set;
		}





		public UInt32 tfPartialPayment {
			get;
			set;
		}
		public UInt32 tfNoDirectRipple {
			get;
			set;
		}

		public UInt32 tfLimitQuality {
			get;
			set;
		}

		public UInt32 DestinationTag {
			get;
			set;
		}

		// TODO why was this sendmax and not SendMax. Maybe two properties are needed. 
		public RippleCurrency SendMax {
			get { return _sendmax; }
			set { _sendmax = value; }
		}

		private RippleCurrency _sendmax = null;


		/*
		  public RippleCurrency sendmax {
			get;
			set;
		}
		 */

		public string InvoiceID {
			get;
			set;
		}

		public RippleCurrency DeliverMin {
			get;
			set;
		}

		public RipplePathElement [] [] Paths {
			get;
			set;
		}

		public RippleCurrency Amount {
			get;
			set;
		}





		public RippleCurrency TakerGets {
			get;
			set;

		}



		public RippleCurrency TakerPays {
			get;
			set;
		}


		public RippleTxMeta meta {
			get;
			set;
		}

		public bool? validated {
			get;
			set;
		}

		/*
		public string PreviousTxnId {

			get;
			set;
		}
		*/
#pragma warning restore IDE1006 // Naming Styles
#endregion

		public String GetSignedTxBlob ()
		{
			return SignedTransactionBlob;
		}


		public virtual RippleBinaryObject GetBinaryObject ()
		{
			return null;
		}

		public string Sign (RippleSeedAddress seed)
		{


			BinarySerializer bs = new BinarySerializer ();

			RippleBinaryObject rbo = GetBinaryObject ();
			rbo = rbo.GetObjectSorted();



			RipplePrivateKey rpk = seed.GetPrivateKey (0);
			RippleTxSigner rtxs = new RippleTxSigner (rpk);

			rbo = rtxs.Sign (rbo);

			var v = bs.WriteBinaryObject (rbo);

			byte [] signedTXBytes = v; // was to array

			//RippleBinaryObject test = bs.readBinaryObject( new System.IO.MemoryStream (signedTXBytes));

			String blob = Base58.ByteArrayToHexString (signedTXBytes);

			this.SignedTransactionBlob = blob;

			return blob;
		}

		public string SignLocalRippled (RippleSeedAddress seed)
		{

			string jsn = GetJsonTx ();

			if (seed == null) {
				throw new ArgumentNullException ();
			}

			if (jsn == null) {
				throw new NotImplementedException ();
			}

			CommandLineExecuter cle = new CommandLineExecuter ();

			string output = cle.SignJSON (jsn, seed.ToString ());

			if (output == null) {
				return null;
			}

			Response<RippleSubmitTxResult> r = DynamicJson.Parse (output);

			if (r.error != null) {

				Logging.WriteLog (r.error_message);
				return null;
			}

			this.SignedTransactionBlob = r.result.tx_blob;
			this.hash = r.result.tx_json.hash;

			Logging.WriteLog ("blob = " + this.SignedTransactionBlob);

			return this.SignedTransactionBlob;

		}

		public virtual string GetJsonTx ()
		{
			return null;
		}



		public UInt32 AutoRequestFee (NetworkInterface ni)
		{
			Tuple<string, UInt32> tupe = RippleLibSharp.Commands.Server.ServerInfo.GetFeeAndLedgerSequence (ni);
			fee = tupe.Item1;
			return tupe.Item2;
		}

		public uint AutoRequestSequence (RippleAddress rw, NetworkInterface ni)
		{
			if (rw == null) {
				Sequence = 0;
				return 0;
														
			}

			if (ni == null) {
				Sequence = 0;
				return 0;
			}
			Sequence = AccountInfo.GetSequence (rw?.ToString (), ni) ?? 0;

			return Sequence;
		}


		protected Response<T> SubmitToNetwork<T> (NetworkInterface ni)
		{

			if (this.SignedTransactionBlob == null) {
				// todo report error. Transaction must be signed first. 
				return null;
			}

			int ticket = NetworkRequestTask.ObtainTicket ();

			object ob = new {
				id = ticket,
				command = "submit",
				tx_blob = this.SignedTransactionBlob
			};

			String json = DynamicJson.Serialize (ob);

			Logging.WriteLog ("Sending " + this.TransactionType + " transaction: " + json + "\n");

			//Logging.write("Amount : " + amount.ToString() + "  Sendmax : " + sendmax.ToString());


			if (ni == null) {

			}

			//RippleCurrency dom = LimitAmount;
			//ni.sendToServer (json);



			var tsk = NetworkRequestTask.RequestResponse<T> (ticket, json, ni);

			tsk.Wait ();



			Response<T> res = tsk.Result;


			return res;

		}


		public Response<RippleSubmitTxResult> Submit (NetworkInterface ni)
		{

			return this.SubmitToNetwork<RippleSubmitTxResult> (ni);




		}


		/*
		public Task< Response< RippleSubmitTxResult > > submitAsTask ( NetworkInterface ni ) {
			return Task< RippleSubmitTxResult >.Run (
				delegate {
					return submit(ni);

				}
			);
		}
		*/





		public string ToJson ()
		{

			string s = DynamicJson.Serialize (this);

			return s;
		}



#if DEBUG
		private const string clsstr = nameof (RippleTransaction) + DebugRippleLibSharp.colon;
#endif
	}
}

