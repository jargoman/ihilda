/*
 *	License : 
 *
 *	Le Ice Sense 
 *
 *	or 
 *
 *	GNU LESSER GENERAL PUBLIC LICENSE
 *                     Version 3, 29 June 2007
 */


using System;
using System.Text;
using System.Security;
using RippleLibSharp.Util;

namespace RippleLibSharp.Keys
{
	public class RippleAddress : RippleIdentifier
	{

		public const string RIPPLE_ROOT_ACCOUNT = "rHb9CJAWyB4rj91VRWn96DkukG4bwdtyTh";
		public const string RIPPLE_ADDRESS_ZERO = "rrrrrrrrrrrrrrrrrrrrrhoLvTp";
		public const string RIPPLE_ADDRESS_ONE = "rrrrrrrrrrrrrrrrrrrrBZbvji";
		public const string RIPPLE_ADDRESS_NEUTRAL = RIPPLE_ADDRESS_ONE;
		public const string RIPPLE_ADDRESS_NAN = "rrrrrrrrrrrrrrrrrrrn5RM1rHd";
		public const string RIPPLE_ADDRESS_BITSTAMP = "rvYAfWj5gh67oV6fW32ZzP3Aw4Eubs59B";
		public const string RIPPLE_ADDRESS_JRIPPLEAPI = "r32fLio1qkmYqFFYkwdnsaVN7cxBwkW4cT";
		public const string RIPPLE_ADDRESS_PMARCHES = "rEQQNvhuLt1KTYmDWmw12mPvmJD4KCtxmS";
		public const string RIPPLE_ADDRESS_JARGOMAN = "rBuDDpdVBt57JbyfXbs8gjWvp4ScKssHzx";
		public const string RIPPLE_ADDRESS_ICE_ISSUER = "r4H3F9dDaYPFwbrUsusvNAHLz2sEZk4wE5";
		public const string RIPPLE_ADDRESS_DAHLIOO = "rDR4DBpWi5oc9pi6ARRQbR4Yre8fxgiXSx";

		/*
		public static readonly string RIPPLE_ROOT_ACCOUNT ="rHb9CJAWyB4rj91VRWn96DkukG4bwdtyTh";
		public static readonly string RIPPLE_ADDRESS_ZERO ="rrrrrrrrrrrrrrrrrrrrrhoLvTp";
		public static readonly string RIPPLE_ADDRESS_ONE ="rrrrrrrrrrrrrrrrrrrrBZbvji";
		public static readonly string RIPPLE_ADDRESS_NEUTRAL =RIPPLE_ADDRESS_ONE;
		public static readonly string RIPPLE_ADDRESS_NAN ="rrrrrrrrrrrrrrrrrrrn5RM1rHd";
		public static readonly string RIPPLE_ADDRESS_BITSTAMP = "rvYAfWj5gh67oV6fW32ZzP3Aw4Eubs59B";
		public static readonly string RIPPLE_ADDRESS_JRIPPLEAPI = "r32fLio1qkmYqFFYkwdnsaVN7cxBwkW4cT";
		public static readonly string RIPPLE_ADDRESS_PMARCHES = "rEQQNvhuLt1KTYmDWmw12mPvmJD4KCtxmS";
		public static readonly string RIPPLE_ADDRESS_JARGOMAN = "rn3KLJY2AfHP5mjfHfn5QXNUn9VvFqihLK";
		public static readonly string RIPPLE_ADDRESS_ICE_ISSUER = "r4H3F9dDaYPFwbrUsusvNAHLz2sEZk4wE5";
		*/

		/*
		public static readonly RippleAddress RIPPLE_ROOT_ACCOUNT=new RippleAddress("rHb9CJAWyB4rj91VRWn96DkukG4bwdtyTh");
		public static readonly RippleAddress RIPPLE_ADDRESS_ZERO=new RippleAddress("rrrrrrrrrrrrrrrrrrrrrhoLvTp");
		public static readonly RippleAddress RIPPLE_ADDRESS_ONE=new RippleAddress("rrrrrrrrrrrrrrrrrrrrBZbvji");
		public static readonly RippleAddress RIPPLE_ADDRESS_NEUTRAL=RIPPLE_ADDRESS_ONE;
		public static readonly RippleAddress RIPPLE_ADDRESS_NAN=new RippleAddress("rrrrrrrrrrrrrrrrrrrn5RM1rHd");
		public static readonly RippleAddress RIPPLE_ADDRESS_BITSTAMP = new RippleAddress("rvYAfWj5gh67oV6fW32ZzP3Aw4Eubs59B");
		public static readonly RippleAddress RIPPLE_ADDRESS_JRIPPLEAPI=new RippleAddress("r32fLio1qkmYqFFYkwdnsaVN7cxBwkW4cT");
		public static readonly RippleAddress RIPPLE_ADDRESS_PMARCHES=new RippleAddress("rEQQNvhuLt1KTYmDWmw12mPvmJD4KCtxmS");
		public static readonly RippleAddress RIPPLE_ADDRESS_JARGOMAN=new RippleAddress("rn3KLJY2AfHP5mjfHfn5QXNUn9VvFqihLK");
		public static readonly RippleAddress RIPPLE_ADDRESS_ICE_ISSUER=new RippleAddress("r4H3F9dDaYPFwbrUsusvNAHLz2sEZk4wE5");
		*/

		public RippleAddress (byte [] payloadBytes) : base (payloadBytes, 0)
		{

		}

		public RippleAddress (String stringID) : base (  /* verifyAddressString( */ stringID  /*)*/)
		{

		}


		public static string VerifyAddressString (String str)
		{
			if (str == null) {
				throw new FormatException ("Ripple Address value is null");
			}

			//if (!str.StartsWith("r")) {
			if (!str.StartsWith ("r", StringComparison.CurrentCulture)) {
				throw new FormatException ("Invalid ripple address string");
			}

			//if (str.Length != ADDRESSLENGTH) {
			//throw new FormatException ("Invalid ripple address length");

			//}

			if (str.Equals ("")) {
				throw new FormatException ("Ripple Address vale is empty");
			}

			return str;
		}



		public static implicit operator string (RippleAddress ra)
		{
			if (ra == null) return null;
			return ra.ToString ();
		}


		public static implicit operator RippleAddress (String s)
		{

#if DEBUG
			if (DebugRippleLibSharp.RippleAddress) {
				StringBuilder stringBuilder = new StringBuilder ();
				stringBuilder.Append (clsstr);
				stringBuilder.Append (nameof (RippleAddress));
				stringBuilder.Append (DebugRippleLibSharp.left_parentheses);
				stringBuilder.Append (nameof (String));
				stringBuilder.Append (DebugRippleLibSharp.space_char);
				stringBuilder.Append (nameof (s));
				stringBuilder.Append (DebugRippleLibSharp.equals);
				stringBuilder.Append (s);
				stringBuilder.Append (DebugRippleLibSharp.right_parentheses);
				string method_sig =  stringBuilder.ToString();
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif


			if (s == null)
				return null;

			try {
				return new RippleAddress (s);
			}
#pragma warning disable 0168
			catch (Exception e) {
#pragma warning restore 0168

				return null;
			}
		}


		/*
		public static explicit operator RippleAddress(String s) {
			if (Debug.RippleAddress) {
				string method_sig = clsstr + "implicit operator RippleAddress(String s) : ";
				Logging.writeLog(method_sig + Debug.beginn);
			}
			return new RippleAddress(s);
		}
		*/

		public static RippleIdentifier GetHighAccount (RippleAddress a, RippleAddress b)
		{
			return a > b ? a : b;
		}

		//private static int ADDRESSLENGTH = 34; 
#if DEBUG
		private const string clsstr = nameof (RippleAddress) + DebugRippleLibSharp.colon;

#endif
	}
}

