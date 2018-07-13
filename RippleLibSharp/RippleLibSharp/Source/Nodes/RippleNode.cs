using System;
using System.Text;
using RippleLibSharp.Keys;
using RippleLibSharp.Util;
using RippleLibSharp.Transactions;
using RippleLibSharp.Binary;


namespace RippleLibSharp.Nodes
{
	public class RippleNode
	{


		public RippleFieldGroup NewFields {
			get;
			set;
		}

		public RippleFieldGroup PreviousFields {
			get;
			set;
		}

		public RippleFieldGroup FinalFields {
			get;
			set;
		}

		public string LedgerEntryType {
			get;
			set;
		}

		public string LedgerIndex {
			get;
			set;
		}

#pragma warning disable IDE1006 // Naming Styles
		public BinaryFieldType nodeType {
#pragma warning restore IDE1006 // Naming Styles
			get;
			set;
		}

		public string PreviousTxnID {
			get;
			set;
		}

		public string GetPreviousTxnID ()
		{
			if (PreviousTxnID != null) {
				return PreviousTxnID;
			}

			if (FinalFields != null && FinalFields.PreviousTxnID != null) {
				return FinalFields.PreviousTxnID;
			}

			return null;
		}


		public void GetOfferCreateResult (RippleAddress addr)
		{
			if (this.NewFields.Account != null && this.NewFields.Account.Equals (addr)) {
				//relavent = true;
			}


		}

		/*
		private OrderChange getNewOfferChange( RippleAddress ra ) {
			return null;
		}
		*/

		public OrderChange GetCanceledTx (RippleAddress addr)
		{
#if DEBUG
			string method_sig = clsstr + "getCanceledTx : ";
#endif

			if (addr == null) {
				return null;
			}

			if (LedgerEntryType == null || !LedgerEntryType.Equals ("Offer")) {
				return null;
			}

			if (FinalFields == null || PreviousFields != null) {
#if DEBUG
				if (DebugRippleLibSharp.RippleNode) {
					Logging.WriteLog (method_sig + "FinalFields == null\n");
				}
#endif
				return null;
			}

			if (this.FinalFields.Account == null || !addr.Equals (this.FinalFields?.Account)) {
				return null;
			}

			if (this.nodeType != BinaryFieldType.DeletedNode) {
				return null;
			}


			RippleCurrency pays = FinalFields.TakerPays;
			RippleCurrency gets = FinalFields.TakerGets;

			OrderChange orderChange = new OrderChange {
				Account = addr,
				TakerPays = pays,
				TakerGets = gets
			};

			string account = addr.ToString ();

			TextHighlighter.Highlightcolor = TextHighlighter.RED;

			account = TextHighlighter.Highlight (account);

			StringBuilder sb = new StringBuilder ();
			StringBuilder sb2 = new StringBuilder ();





			decimal g = gets.amount;
			decimal p = pays.amount;

			if (p == 0 || g == 0) {
				return null;
			}

			if (pays.IsNative) {
				p = p / 1000000m;
			}
			if (gets.IsNative) {
				g = g / 1000000m;
			}

			decimal price = g / p;


			sb.Append (account);
			sb.Append ("'s Order #");
			sb.Append (this.FinalFields.Sequence.ToString ());
			sb.Append (" offering ");
			sb.Append (pays);
			sb.Append (" for ");
			sb.Append (gets);
			sb.Append (" at price ");
			sb.Append (price);

			sb.Append (" was Canceled due to lack of funds \n\n");


			price = p / g;

			sb2.Append (account);
			sb2.Append ("'s Order #");
			sb2.Append (this.FinalFields.Sequence.ToString ());
			sb2.Append (" offering ");
			sb2.Append (pays);
			sb2.Append (" for ");
			sb2.Append (gets);
			sb2.Append (" at price ");
			sb2.Append (price);

			sb2.Append (" was Canceled due to lack of funds \n\n");




			orderChange.BuyOrderChange = sb.ToString ();
			orderChange.SellOrderChange = sb2.ToString ();

			orderChange.TakerPays = pays;
			orderChange.TakerGets = gets;


			return orderChange;


		}

		public OrderChange GetOfferChange (RippleAddress addr)
		{
#if DEBUG
			string method_sig = clsstr + nameof(GetOfferChange) +  nameof (RippleAddress) + DebugRippleLibSharp.space_char + nameof (addr) + DebugRippleLibSharp.ToAssertString (addr) + DebugRippleLibSharp.right_parentheses;
			if (DebugRippleLibSharp.RippleNode) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			if (LedgerEntryType == null || !LedgerEntryType.Equals ("Offer")) {
				return null;
			}

			if (VerifyNulls ()) return null;


			if (this.FinalFields.Account == null || !this.FinalFields.Account.Equals (addr.ToString ())) {
				return null;
			}



			RippleCurrency pays = PreviousFields.TakerPays - this.FinalFields.TakerPays;
			RippleCurrency gets = PreviousFields.TakerGets - this.FinalFields.TakerGets;


			string account = addr.ToString ();

			TextHighlighter.Highlightcolor = TextHighlighter.RED;
			account = TextHighlighter.Highlight (account);



			StringBuilder sb = new StringBuilder ();
			StringBuilder sb2 = new StringBuilder ();



			if (pays == null || gets == null) {
				string m1 = "pays == ";

				sb.Append (m1);
				sb2.Append (m1);

				string m2 = pays?.ToString () ?? "null";

				sb.Append (m2);
				sb2.Append (m2);

				string m3 = "gets == ";

				sb.Append (m3);
				sb2.Append (m3);

				string m4 = gets?.ToString () ?? "null";

				sb.Append (m4);
				sb2.Append (m4);

				goto ENDING;
			}

			// duplicate of below
			//oc.TakerPays = pays;
			//oc.TakerGets = gets;





			decimal g = gets.amount;
			decimal p = pays.amount;



			if (pays.IsNative) {
				p = p / 1000000m;
			}
			if (gets.IsNative) {
				g = g / 1000000m;
			}



			if (p == 0 || g == 0) {
#if DEBUG
				Logging.WriteLog (method_sig + "p == " + p + ", g == " + g);


#endif


				string message = "Divide by zero exception ";

				sb.Append (message);
				sb2.Append (message);

			} else {

				decimal price = g / p;

				sb.Append (account);
				sb.Append (" bought ");
				sb.Append (pays);
				sb.Append (" for ");
				sb.Append (gets);
				sb.Append (" : price ");
				sb.Append (price);

				price = p / g;

				sb2.Append (account);
				sb2.Append (" sold ");
				sb2.Append (gets);
				sb2.Append (" for ");
				sb2.Append (pays);
				sb2.Append (" : price ");
				sb2.Append (price);

				if (this.nodeType == BinaryFieldType.DeletedNode) {
					string s = "this order has filled";
					sb.Append (s);
					sb2.Append (s);
				}
			}


		ENDING:

			OrderChange oc = new OrderChange {
				Account = addr,
				BuyOrderChange = sb.ToString (),
				SellOrderChange = sb2.ToString (),
				TakerPays = pays,
				TakerGets = gets

			};





			return oc;
		}


		private RippleCurrency GetRippleStateChange (RippleAddress addr)
		{
			if (VerifyNulls ()) return null;

			if (FinalFields.HighLimit.issuer == addr) {
				RippleCurrency fin = FinalFields.Balance;
				RippleCurrency prev = PreviousFields.Balance;

				return (fin - prev) * -1;

			}

			if (FinalFields.LowLimit.issuer == addr) {
				RippleCurrency fin = FinalFields.Balance;
				RippleCurrency prev = PreviousFields.Balance;

				return fin - prev;
			}

			return null;
		}

		private RippleCurrency GetAccountRootChange (RippleAddress addr)
		{



			if (VerifyNulls ()) return null;

			if (FinalFields.Account == addr) {
				RippleCurrency fin = FinalFields.Balance;
				RippleCurrency prev = PreviousFields.Balance;

				return fin - prev;
			}

			return null;
		}

		/*
		public string getBalanceChanges (RippleAddress addr) {


#if DEBUG
			string method_sig = clsstr + "getBalanceChanges (RippleAddress addr = " + Debug.toAssertString(addr) + ")";
			if (Debug.RippleNode) {
				Logging.writeLog (method_sig + Debug.beginn);
			}
#endif

			StringBuilder sb = new StringBuilder ();
			sb.AppendLine (nodeType.ToString());
			sb.AppendLine ( LedgerEntryType );

			RippleCurrency ret = null;
			switch (LedgerEntryType) {
			case "AccountRoot":
				ret = getAccountRootChange (addr);
				break;
			case "RippleState":
				ret = getRippleStateChange (addr);
				break;

			case "Offer":
				string s = getOfferChange (addr);
				sb.AppendLine (s);
				break;
			}

			//return ret;

			sb.AppendLine (ret);

			return sb.ToString ();

			//string s = getOrderChanges (addr);

			//if ( verifyNulls() ) return null;




			//TODO, I'm doing this to test, put this in a try catch if this method proves to be useful and used for production

			//RippleCurrency res = fin - prev;

			//return res;

		}

*/

		private bool VerifyNulls ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (VerifyNulls) + DebugRippleLibSharp.both_parentheses;
			if (DebugRippleLibSharp.RippleNode) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif


			if (FinalFields == null) {
#if DEBUG
				if (DebugRippleLibSharp.RippleNode) {
					Logging.WriteLog (method_sig + nameof (FinalFields) + " == null\n");
				}
#endif
				return true;
			}

			if (PreviousFields == null) {
#if DEBUG
				if (DebugRippleLibSharp.RippleNode) {
					Logging.WriteLog (method_sig + nameof (PreviousFields) + " == null\n");
				}
#endif
				return true;
			}

			return false;

		}

		public RippleNode GetFilledNode (RippleAddress radd)
		{

			if (LedgerEntryType == null || !LedgerEntryType.Equals ("Offer")) {
				return null;
			}

			if (this.nodeType != BinaryFieldType.DeletedNode) {
				return null;
			}

			if (this.FinalFields.Account == null || !this.FinalFields.Account.Equals (radd)) {
				return null;
			}

			if (!(this.FinalFields.TakerGets.amount < this.PreviousFields.TakerGets.amount)) {
				return null;
			}



			return this;
		}


		public string GetBotId ()
		{
			return this.FinalFields.Account + this.FinalFields.Sequence;
		}

#if DEBUG
		private const string clsstr = nameof (RippleNode) + DebugRippleLibSharp.colon;
#endif
	}
}

