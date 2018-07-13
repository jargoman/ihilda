using System;
using RippleLibSharp.Util;


namespace RippleLibSharp.Transactions.TxTypes
{
	public class RippleTransactionType
	{
		// 
		// Enums in c# can only be primitive data types. 
		// I want them to be full fledged classes as in java
		// this is the solution I came up with :/

		public static readonly RippleTransactionType PAYMENT = new RippleTransactionType (0x00, "Payment");
		public static readonly RippleTransactionType CLAIM = new RippleTransactionType (0x01, "Claim"); //TODO verify these strings are the correct value...
		public static readonly RippleTransactionType WALLET_ADD = new RippleTransactionType (0x02, "WalletAdd");
		public static readonly RippleTransactionType ACCOUNT_SET = new RippleTransactionType (0x03, "AccountSet");
		public static readonly RippleTransactionType PASSWORD_FUND = new RippleTransactionType (0x04, "PasswordFund");
		public static readonly RippleTransactionType REGULAR_KEY_SET = new RippleTransactionType (0x05, "RegularKeySet");
		public static readonly RippleTransactionType NICKNAME_SET = new RippleTransactionType (0x06, "NicknameSet");
		public static readonly RippleTransactionType OFFER_CREATE = new RippleTransactionType (0x07, "OfferCreate");
		public static readonly RippleTransactionType OFFER_CANCEL = new RippleTransactionType (0x08, "OfferCancel");
		public static readonly RippleTransactionType CONTRACT = new RippleTransactionType (0x09, "Contract");
		public static readonly RippleTransactionType CONTRACT_REMOVE = new RippleTransactionType (0x0a, "ContractRemove");
		public static readonly RippleTransactionType TRUST_SET = new RippleTransactionType (0x14, "TrustSet");
		public static readonly RippleTransactionType FEATURE = new RippleTransactionType (0x64, "Feature");
		public static readonly RippleTransactionType FEE = new RippleTransactionType (0x65, "Fee");

		static RippleTransactionType ()
		{

		}


#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private static RippleTransactionType [] values = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant


		public static RippleTransactionType [] GetValues ()
		{
#if DEBUG
			if (DebugRippleLibSharp.RippleTransactionType) {

			}
#endif

			LoadTransactionTypes ();

			return RippleTransactionType.values;
		}

		public static void LoadTransactionTypes ()
		{
			if (RippleTransactionType.values == null) {

				RippleTransactionType.values = new RippleTransactionType [] {
					RippleTransactionType.PAYMENT,
					RippleTransactionType.CLAIM,
					RippleTransactionType.WALLET_ADD,
					RippleTransactionType.ACCOUNT_SET,
					RippleTransactionType.PASSWORD_FUND,
					RippleTransactionType.REGULAR_KEY_SET,
					RippleTransactionType.NICKNAME_SET,
					RippleTransactionType.OFFER_CREATE,
					RippleTransactionType.OFFER_CANCEL,
					RippleTransactionType.CONTRACT,
					RippleTransactionType.CONTRACT_REMOVE,
					RippleTransactionType.TRUST_SET,
					RippleTransactionType.FEATURE,
					RippleTransactionType.FEE
				};
			}
		}

		RippleTransactionType (UInt16 bin, string str)
		{
			this.value = new Tuple<UInt16, string> (bin, str);
		}


		public readonly Tuple<UInt16, string> value;

		RippleTransactionType (Tuple<UInt16, string> value)
		{
			//this.uint16Value = txTypeByteValue;

			this.value = value;

		}

		public override string ToString ()
		{
			return value.Item2;
		}



		public static RippleTransactionType FromType (UInt16 txType)
		{
			foreach (RippleTransactionType tt in values) {
				if (tt.value.Item1 == txType) {
					return tt;
				}
			}

			return null;

		}

		public static RippleTransactionType FromType (string str)
		{
#if DEBUG
			if (DebugRippleLibSharp.RippleTransactionType) {

			}
#endif
			foreach (RippleTransactionType tt in values) {
				if (tt.value.Item2.Equals (str)) {
					return tt;
				}
			}

			return null;

		}

		public static RippleTransactionType FromType (Tuple<UInt16, string> tupe)
		{
			return FromType (tupe.Item1);

		}

		public static RippleTransactionType FromType (object o)
		{
			if (o is string) {
				string s = o as string;

				//return fromType (o as string);
				return FromType (s);
			}

			if (o is UInt16) {
				return FromType ((UInt16)o);
			}

			if (o is Tuple<UInt16, string>) {
				return FromType ((Tuple<UInt16, string>)o);
			}

			// TODO // throw an eception ? this should 

			return null;
		}

		public override int GetHashCode ()
		{
			// TODO implement hash code
			return this.value.Item1;
		}

		public override bool Equals (Object obj)
		{
#if DEBUG
			string method_sig = clsstr + nameof (Equals) + DebugRippleLibSharp.left_parentheses + nameof (Object) + DebugRippleLibSharp.space_char + nameof (obj) + DebugRippleLibSharp.equals + DebugRippleLibSharp.ToAssertString (obj) + DebugRippleLibSharp.right_parentheses;
#endif

			if (obj == null) {
#if DEBUG
				if (DebugRippleLibSharp.RippleTransactionType) {
					Logging.WriteLog (method_sig + nameof (obj) + " == null");
				}
#endif
				return false;
			}

			if (obj is string) {
#if DEBUG
				if (DebugRippleLibSharp.RippleTransactionType) {
					Logging.WriteLog (method_sig + nameof(obj) + " is string");
				}
#endif
				string s = obj as string;
				return s.Equals (this.value.Item2);

			}

			if (obj is Int16) {
#if DEBUG
				if (DebugRippleLibSharp.RippleTransactionType) {
					Logging.WriteLog (method_sig + nameof(obj) + " is Int16");
				}
#endif
				Int16 i = (Int16)obj;
				return i == value.Item1;
			}

			if (obj is Tuple<UInt16, string>) {






				if (!(obj is Tuple<UInt16, string> tu)) {

					return false;
				}

				return (tu.Item1 == this.value.Item1);
			}

			RippleTransactionType transactionType = obj as RippleTransactionType;
			if (transactionType == (RippleTransactionType)null) {
				return false;
			}

			return this.Equals (transactionType);
		}

		public bool Equals (RippleTransactionType tst)
		{
			if ((Object)tst == null) {
				return false;
			}

			return this.value.Item1 == tst.value.Item1;
		}

		public static bool operator == (RippleTransactionType left, RippleTransactionType right)
		{
			return left.Equals (right);
		}

		public static bool operator != (RippleTransactionType left, RippleTransactionType right)
		{
			return !left.Equals (right);
		}

		public static bool operator == (RippleTransactionType left, string right)
		{
			return left.Equals (right);
		}

		public static bool operator != (RippleTransactionType left, string right)
		{
			return !left.Equals (right);
		}

		public static bool operator == (string left, RippleTransactionType right)
		{
			return right.Equals (left);
		}

		public static bool operator != (string left, RippleTransactionType right)
		{
			return !right.Equals (left);
		}

		public static implicit operator RippleTransactionType (String s)
		{
			return FromType (s);
		}

		public static implicit operator string (RippleTransactionType rtt)
		{
			return rtt.value.Item2;
		}

		public static implicit operator UInt16 (RippleTransactionType rtt)
		{
			return rtt.value.Item1;
		}

		public static implicit operator RippleTransactionType (UInt16 ui)
		{
			return FromType (ui);
		}

#if DEBUG
		private const string clsstr = nameof (RippleTransactionType) + DebugRippleLibSharp.colon;
#endif
	}
}

