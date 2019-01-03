
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using RippleLibSharp.Keys;
using RippleLibSharp.Util;
using RippleLibSharp.Transactions.TxTypes;
using RippleLibSharp.Result;
using RippleLibSharp.Nodes;

namespace RippleLibSharp.Transactions
{
	public class RippleTxStructure : IRawJsonInterface
	{
		/*
		public RippleTxStructure ()
		{
		}
		*/

#pragma warning disable IDE1006 // Naming Styles
		public string hash {

			get;
			set;
		}

		public int ledger_index {
			get;
			set;
		}


		public string date {
			get;
			set;
		}

		public RippleTxMeta meta {
			get;
			set;
		}

		public RippleTransaction tx {
			get;
			set;
		}


		/*
		public RippleTransaction transaction {
			get;
			set;
		}
		*/
		public bool validated {
			get;
			set;
		}

#pragma warning restore IDE1006 // Naming Styles
		public string RawJSON {
			get;
			set;

		}

		public Tuple<string, string> GetReport (RippleAddress queried_addr)
		{
#if DEBUG
			string method_sig = clsstr + nameof(GetReport) + DebugRippleLibSharp.left_parentheses + nameof (RippleAddress) + DebugRippleLibSharp.space_char + nameof (queried_addr) + DebugRippleLibSharp.equals + DebugRippleLibSharp.ToAssertString (queried_addr) + DebugRippleLibSharp.right_parentheses;
			if (DebugRippleLibSharp.RippleTxStructure) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			if (tx.TransactionType == null) {
#if DEBUG
				if (DebugRippleLibSharp.RippleTxStructure) {
					Logging.WriteLog (method_sig + nameof (tx.TransactionType) + " == null\n");
				}
#endif
				return null;
			}

			//Logging.writeLog ("defined");

			if (tx.TransactionType == RippleTransactionType.PAYMENT) {
#if DEBUG
				if (DebugRippleLibSharp.RippleTxStructure) {
					Logging.WriteLog (method_sig + nameof(tx.TransactionType) + DebugRippleLibSharp.equals + nameof(RippleTransactionType.PAYMENT) + "\n");
				}
#endif
				return GetRipplePaymentTransaction (queried_addr);
			}

			if (tx.TransactionType == RippleTransactionType.OFFER_CREATE) {
#if DEBUG
				if (DebugRippleLibSharp.RippleTxStructure) {
					Logging.WriteLog (method_sig + nameof(tx.TransactionType) + DebugRippleLibSharp.equals + nameof( RippleTransactionType.OFFER_CREATE) + "\n");
				}
#endif
				return GetRippleOfferTransaction (queried_addr);
			}

			if (tx.TransactionType == RippleTransactionType.TRUST_SET) {
#if DEBUG
				if (DebugRippleLibSharp.RippleTxStructure) {
					Logging.WriteLog (method_sig + nameof (tx.TransactionType) + DebugRippleLibSharp.equals +  nameof (RippleTransactionType.TRUST_SET) + "\n");
				}
#endif

				return GetRippleTrustSetTransaction (queried_addr);
			}

			if (tx.TransactionType == RippleTransactionType.OFFER_CANCEL) {
#if DEBUG
				if (DebugRippleLibSharp.RippleTxStructure) {
					Logging.WriteLog (method_sig + nameof (tx.TransactionType) + DebugRippleLibSharp.equals + nameof ( RippleTransactionType.OFFER_CANCEL) + "\n");
				}
#endif

				return GetRippleOfferCancelTransaction (queried_addr);
			}

			if (tx.TransactionType == RippleTransactionType.ACCOUNT_SET) {
#if DEBUG
				if (DebugRippleLibSharp.RippleTxStructure) {
					Logging.WriteLog (method_sig + nameof (tx.TransactionType) + DebugRippleLibSharp.equals + nameof (RippleTransactionType.ACCOUNT_SET) + "\n");
				}
#endif
			}

#if DEBUG
			if (DebugRippleLibSharp.RippleTxStructure) {
				Logging.WriteLog (method_sig + "returning null");
			}
#endif
			return null;
		}

		private Tuple<string, string> GetRippleTrustSetTransaction (RippleAddress queried_addr)
		{
#if DEBUG
			string method_sig = clsstr + nameof (GetRippleTrustSetTransaction) + DebugRippleLibSharp.left_parentheses + nameof (RippleAddress) + DebugRippleLibSharp.space_char + nameof(queried_addr) + DebugRippleLibSharp.equals + DebugRippleLibSharp.ToAssertString (queried_addr) + DebugRippleLibSharp.right_parentheses;
			if (DebugRippleLibSharp.RippleTxStructure) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif



			string account = tx.Account;

			RippleCurrency limitamount = tx.LimitAmount;
			string limitissuer = limitamount.ToIssuerString ();

			string amnt = limitamount.value.ToString ();
			string cur = limitamount.currency;



			if (queried_addr == tx.Account) {
				TextHighlighter.Highlightcolor = TextHighlighter.RED;
				account = TextHighlighter.Highlight (account);

			}

			if (queried_addr == limitamount.issuer) {
				TextHighlighter.Highlightcolor = TextHighlighter.RED;
				limitissuer = TextHighlighter.Highlight (limitissuer);

			}

			StringBuilder sb = new StringBuilder ();

			sb.Append (account);
			sb.Append (" trusted ");
			sb.Append (limitissuer);
			sb.Append (" for ");
			sb.Append (amnt);
			sb.Append (" ");
			sb.Append (cur);




			Logging.WriteLog (sb.ToString ());

			string s = sb.ToString ();

			Tuple<string, string> ret = new Tuple<string, string> (s, s);
			return ret;

		}


		private Tuple<string, string> GetRipplePaymentTransaction (RippleAddress queried_addr)
		{
			//RipplePaymentTransaction paytx = /*(RipplePaymentTransaction)*/tx;
#if DEBUG
			string method_sig = clsstr + nameof (GetRipplePaymentTransaction) + DebugRippleLibSharp.left_parentheses + nameof (RippleAddress) + DebugRippleLibSharp.space_char + nameof( queried_addr) + DebugRippleLibSharp.equals + DebugRippleLibSharp.ToAssertString (queried_addr) + DebugRippleLibSharp.right_parentheses;
			if (DebugRippleLibSharp.RippleTxStructure) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}

#endif

			Tuple<string, string> sendandreceive = this.GetPaymentPrintables (queried_addr);
			StringBuilder builder = new StringBuilder ();


			RippleCurrency dom = tx.Amount;

			builder.Append (sendandreceive.Item1);
			builder.Append (" submitted a payment of ");
			builder.Append (dom.ToString ());
			builder.Append (" to ");
			builder.Append (sendandreceive.Item2);
			//builder += " for ";
			//builder +=




#if DEBUG
			if (DebugRippleLibSharp.RippleTxStructure) {
				Logging.WriteLog (builder.ToString ());
				IEnumerable<OrderChange> cl = this.meta.GetOrderChanges (queried_addr);
				Logging.WriteLog ("cl.Count = " + cl.Count());
				foreach (OrderChange d in cl) {
					Logging.WriteLog (d.BuyOrderChange);
				}
			}
#endif

			string s = builder.ToString ();
			return new Tuple<string, string> (s, s);
		}

		private Tuple<string, string> GetRippleOfferCancelTransaction (RippleAddress queried_addr)
		{

#if DEBUG
			string method_sig = 
				clsstr + 
				nameof (GetRippleOfferCancelTransaction) + 
				DebugRippleLibSharp.left_parentheses + nameof(RippleAddress) + DebugRippleLibSharp.space_char + nameof(queried_addr) + DebugRippleLibSharp.equals + DebugRippleLibSharp.ToAssertString (queried_addr) + DebugRippleLibSharp.right_parentheses;
			if (DebugRippleLibSharp.RippleTxStructure) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			StringBuilder builder = new StringBuilder ();
			RippleCurrency tGets = null;
			RippleCurrency tPays = null;

			string acnt = tx.Account;
			if (queried_addr == tx.Account) {
				TextHighlighter.Highlightcolor = TextHighlighter.RED;
				acnt = TextHighlighter.Highlight (acnt); //highlight (acnt);
			}

			RippleNode canceledOrderNode = null;

			if (!Configuration.Config.PreferLinq) {

				foreach (RippleNodeGroup rng in meta.AffectedNodes) {
					if ("Offer".Equals (rng?.GetNode ()?.LedgerEntryType)) {
						canceledOrderNode = rng.GetNode();
						break;

					}
				}
				
			} else {

				var va = from RippleNodeGroup rng in meta.AffectedNodes
					where "Offer".Equals (rng?.GetNode ()?.LedgerEntryType)
					 select rng.GetNode ();

				try {
					canceledOrderNode = va.First ();

				} catch (Exception e) {
#if DEBUG
					if (DebugRippleLibSharp.RippleTxStructure) {
						Logging.ReportException (method_sig, e);
					}
#endif

				}



			}

			if (canceledOrderNode != null) {
				tGets = canceledOrderNode.FinalFields.TakerGets;
				tPays = canceledOrderNode.FinalFields.TakerPays;
			}


			builder.Append (acnt);
			builder.Append (" canceled an order");

			if (canceledOrderNode != null) {

				builder.Append (" to buy ");
				builder.Append (tPays?.ToString () ?? "null");
				builder.Append (" for ");
				builder.Append (tGets?.ToString () ?? "null");

			}

			string s1 = builder.ToString ();

			builder.Clear ();

			builder.Append (acnt);
			builder.Append (" canceled an order");

			if (canceledOrderNode != null) {
				builder.Append (" to sell ");
				builder.Append (tGets?.ToString () ?? "null");
				builder.Append (" for ");
				builder.Append (tPays?.ToString () ?? "null");
			}

			string s2 = builder.ToString ();

			return new Tuple<string, string> (s1, s2);
		}

		private Tuple<string, string> GetPaymentPrintables (RippleAddress queried_addr)
		{

			string sending_address = this.tx.Account;
			string receiving_address = this.tx.Destination;

#if DEBUG
			string method_sig = clsstr + nameof( GetPaymentPrintables) + DebugRippleLibSharp.left_parentheses + nameof (RippleAddress) + DebugRippleLibSharp.space_char + nameof (queried_addr) + DebugRippleLibSharp.equals + DebugRippleLibSharp.ToAssertString (queried_addr) + DebugRippleLibSharp.right_parentheses;
#endif

			if (queried_addr == tx.Account) {
				TextHighlighter.Highlightcolor = TextHighlighter.RED;
				sending_address = TextHighlighter.Highlight (sending_address);

			}

			if (queried_addr == tx.Destination) {
				TextHighlighter.Highlightcolor = TextHighlighter.RED;
				receiving_address = TextHighlighter.Highlight (receiving_address);
			}

#if DEBUG
			if (DebugRippleLibSharp.RippleTxStructure) {
				Logging.WriteLog (method_sig + nameof (sending_address) + DebugRippleLibSharp.equals + DebugRippleLibSharp.ToAssertString (sending_address) + "\n");
				Logging.WriteLog (method_sig + nameof (receiving_address) + DebugRippleLibSharp.equals + DebugRippleLibSharp.ToAssertString (receiving_address) + "\n");
				Logging.WriteLog (method_sig + nameof (queried_addr) + DebugRippleLibSharp.equals + DebugRippleLibSharp.ToAssertString (queried_addr) + "\n");
			}
#endif

			return new Tuple<string, string> (sending_address, receiving_address);


		}

		private bool IsPartyToTx (RippleAddress queried_addr)
		{

#if DEBUG
			string addr = queried_addr?.ToString () ?? DebugRippleLibSharp.null_str;
			string method_sig = clsstr + nameof (IsPartyToTx) + DebugRippleLibSharp.left_parentheses + nameof (RippleAddress) + addr + DebugRippleLibSharp.right_parentheses;
			if (DebugRippleLibSharp.RippleTxStructure) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			if (queried_addr == tx.Account) {
				return true;
			}

			if (queried_addr == tx.Destination) {
				return true;
			}

			return false;

		}



		private Tuple<string, string> GetRippleOfferTransaction (RippleAddress queried_addr)
		{

#if DEBUG
			string method_sig = clsstr + nameof (GetRippleOfferTransaction) + DebugRippleLibSharp.left_parentheses + nameof (RippleAddress) + DebugRippleLibSharp.space_char+ nameof(queried_addr) + DebugRippleLibSharp.equals + DebugRippleLibSharp.ToAssertString (queried_addr) + DebugRippleLibSharp.right_parentheses;
			if (DebugRippleLibSharp.RippleTxStructure) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif
			StringBuilder buybuilder = new StringBuilder ();
			StringBuilder sellbuilder = new StringBuilder ();

			string account = tx.Account;


			RippleCurrency tgets = tx.TakerGets;
			RippleCurrency tpays = tx.TakerPays;

			Decimal cost = tgets.GetNativeAdjustedCostAt (tpays);
			Decimal price = tpays.GetNativeAdjustedPriceAt (tgets);


			if (queried_addr == tx.Account) {
				TextHighlighter.Highlightcolor = TextHighlighter.RED;
				account = TextHighlighter.Highlight (account);
			}

			buybuilder.Append (account);
			buybuilder.Append (" created an offer to buy ");
			buybuilder.Append (tpays.ToString ());
			buybuilder.Append (" for ");
			buybuilder.Append (tgets.ToString ());

			buybuilder.Append (" Price : ");
			buybuilder.Append (price.ToString ());


			sellbuilder.Append (account);
			sellbuilder.Append (" created an offer to sell ");
			sellbuilder.Append (tgets.ToString ());
			sellbuilder.Append (" for ");
			sellbuilder.Append (tpays.ToString ());

			sellbuilder.Append (" Cost : ");
			sellbuilder.Append (cost.ToString ());

			sellbuilder.Append ("ledger #" + tx.ledger_index);





#if DEBUG


			if (DebugRippleLibSharp.RippleTxStructure) {
				Logging.WriteLog (buybuilder.ToString ());
				Logging.WriteLog (sellbuilder.ToString ());

				IEnumerable<OrderChange> cl = this.meta.GetOrderChanges (queried_addr);
				foreach (OrderChange d in cl) {
					Logging.WriteLog (d.BuyOrderChange);
				}
			}
#endif

			string s1 = buybuilder.ToString ();
			string s2 = sellbuilder.ToString ();
			return new Tuple<string, string> (s1, s2);

		}

#if DEBUG
		private const string clsstr = nameof (RippleTxStructure) + DebugRippleLibSharp.colon;

#endif
	}
}

