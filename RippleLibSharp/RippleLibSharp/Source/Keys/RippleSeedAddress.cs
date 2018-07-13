using System;
//using System.Security;
using System.Text;
using RippleLibSharp.Util;


namespace RippleLibSharp.Keys
{
	public class RippleSeedAddress : RippleIdentifier
	{
		public RippleSeedAddress (byte [] payloadBytes) : base (payloadBytes, 33)
		{
			/*
			#if DEBUG
			String method_sig = clsstr + "new, byte[] payloadBytes=";

			if (Debug.RippleSeedAddress) {
				if (Debug.allowInsecureDebugging) {
					Logging.writeLog(method_sig, payloadBytes);
				}

				else {
					Logging.writeLog( method_sig + " { withheld for security reasons } " );
				}


			}
			#endif
			*/
		}

		public RippleSeedAddress (String stringID) : base (stringID) //base (verifyFormat(stringID))
		{
			/*
			#if DEBUG
			if (Debug.RippleSeedAddress) {
				Logging.writeLog(clsstr + "new, String stringID = " + Debug.assertAllowInsecure(stringID) );
			}
			#endif
			*/
		}

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private RipplePrivateKey privateKeyCache = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		public RipplePrivateKey GetPrivateKey (int accountNumber)
		{
#if DEBUG
			string method_sig = clsstr + nameof (GetPrivateKey) + DebugRippleLibSharp.left_parentheses + nameof (accountNumber) + DebugRippleLibSharp.equals + DebugRippleLibSharp.AssertAllowInsecure (accountNumber) + DebugRippleLibSharp.right_parentheses;
			if (DebugRippleLibSharp.RippleSeedAddress) {
				Logging.WriteLog (  method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			if (privateKeyCache == null) {

				RippleDeterministicKeyGenerator generator = new RippleDeterministicKeyGenerator (this.PayloadBytes);
				RipplePrivateKey signingPrivateKey = generator.GetAccountPrivateKey (accountNumber);
				privateKeyCache = signingPrivateKey;
			}
			return privateKeyCache;
		}

		public RippleAddress GetPublicRippleAddress ()
		{
#if DEBUG
			if (DebugRippleLibSharp.RippleSeedAddress) {
				Logging.WriteLog ("RippleSeedAddress.getPublicRippleAddress ()");
			}
#endif

			if (publicRippleAddressCache == null) {
				publicRippleAddressCache = GetPrivateKey (0).GetPublicKey ().GetAddress ();
			}

			return publicRippleAddressCache;
		}

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private RippleAddress publicRippleAddressCache = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant



		/*
		public static string verifyFormat (String str)
		{
			#if DEBUG
			String method_sig = clsstr + "verifyFormat( String str = " + Debug.assertAllowInsecure(str) + ") : " ;

			if (Debug.RippleSeedAddress) {
				Logging.writeLog(method_sig + Debug.begin );
			}
			#endif

			if (!str.StartsWith("s") 
				//|| str.Length != SEEDLENGTH
			) {
				String er = "Invalid seed address string";

				#if DEBUG
				if (Debug.RippleSeedAddress) {
					Logging.writeLog(method_sig + "doesn't begin with \"s\" throwing new Format Exception, message = " + er);
				}
				#endif

				throw new FormatException(er);
			}

			return str;
		}	*/

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private String hiddenString = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		public String ToHiddenString ()
		{
#if DEBUG
			String method_sig = clsstr + nameof (ToHiddenString) + DebugRippleLibSharp.both_parentheses;

			if (DebugRippleLibSharp.RippleSeedAddress) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			if (hiddenString == null) {
#if DEBUG
				if (DebugRippleLibSharp.RippleSeedAddress) {
					Logging.WriteLog (method_sig + nameof (hiddenString) + " == null, creating new");
				}
#endif

				int len = this.ToString ().Length;

				StringBuilder bob = new StringBuilder (len);
				for (int i = 0; i < len; i++) {
					bob.Append ('*');
				}

				hiddenString = bob.ToString ();
			}

#if DEBUG
			if (DebugRippleLibSharp.RippleSeedAddress) {
				Logging.WriteLog (method_sig + "returning " + hiddenString);
			}
#endif

			return hiddenString;
		}

#if DEBUG
		private const string clsstr = nameof (RippleSeedAddress) + DebugRippleLibSharp.colon;
#endif

	}
}

