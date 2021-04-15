// gpl3

using System;
//using System.Data.Linq;
using System.Security.Cryptography;
using Org.BouncyCastle;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Crypto.Signers;

using Org.BouncyCastle.Math;

using RippleLibSharp.Util;
using RippleLibSharp.Binary;

using System.Security;

// done

namespace RippleLibSharp.Keys
{

	public class RippleIdentifier
	{
#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private String humanReadableIdentifier = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		//private byte[] _payloadBytes = null;


		protected byte [] PayloadBytes {
			get;
			set;
		}


		public readonly byte identifierType; // the first byte is the identifier




		public RippleIdentifier (
			byte [] payloadBytes, 
			byte identifierType)
		{
			this.PayloadBytes = payloadBytes ?? throw new ArgumentNullException (nameof (payloadBytes));
			this.identifierType = identifierType;

			//this.humanReadableIdentifier = ToString ();
		}



		public RippleIdentifier (String humanreadable)
		{

#if DEBUG
			if (DebugRippleLibSharp.RippleIdentifier) {
				Logging.WriteLog("RippleIdentifier.const (string humanreadable = " + DebugRippleLibSharp.AssertAllowInsecure(humanreadable) + " );\n");
			}
#endif

			//this.humanReadableIdentifier = humanreadable;
			//byte[] stridBytes = Base58.decode (humanreadable);
			byte [] stringIdBytes = Base58.Decode (humanreadable);

			byte [] checksumArray = DoubleSha256 (stringIdBytes, 0, stringIdBytes.Length - 4);

#if DEBUG

			if (DebugRippleLibSharp.RippleIdentifier) {
				Logging.WriteLog(
					"checksumArray = " +
					checksumArray[0].ToString() + " " +
					checksumArray[1].ToString() + " " +
					checksumArray[2].ToString() + " " +
					checksumArray[3].ToString() + "\n" +
					"last four bytes = " +
					stringIdBytes [stringIdBytes.Length - 4].ToString() + " " +
					stringIdBytes [stringIdBytes.Length - 3].ToString() + " " +
					stringIdBytes [stringIdBytes.Length - 2].ToString() + " " +
					stringIdBytes [stringIdBytes.Length - 1].ToString() + "\n"
				);
			}

#endif

			if (
				checksumArray == null
				|| checksumArray.Length < 4
				|| stringIdBytes == null
				|| stringIdBytes.Length < 4
				|| checksumArray [0] != stringIdBytes [stringIdBytes.Length - 4]
				|| checksumArray [1] != stringIdBytes [stringIdBytes.Length - 3]
				|| checksumArray [2] != stringIdBytes [stringIdBytes.Length - 2]
				|| checksumArray [3] != stringIdBytes [stringIdBytes.Length - 1]) {

				throw new CryptographicException ("Checksum failed on identifier " + humanreadable);

			}

			PayloadBytes = new byte [stringIdBytes.Length - 5];
			System.Array.Copy (stringIdBytes, 1, PayloadBytes, 0, PayloadBytes.Length);
			identifierType = stringIdBytes [0];

#if DEBUG
			if (DebugRippleLibSharp.RippleIdentifier) {
				Logging.WriteLog ( "payloadBytes.length = " + PayloadBytes.Length ); 
			}
#endif

		}

		//@Override

		//new 7

		public string GetHumanReadableIdentifier ()
		{
			byte [] versionPayloadChecksumBytes = new byte [1 + PayloadBytes.Length + 4];
			versionPayloadChecksumBytes [0] = this.identifierType;
			Array.Copy (PayloadBytes, 0, versionPayloadChecksumBytes, 1, PayloadBytes.LongLength);

			byte [] hashBytes = DoubleSha256 (versionPayloadChecksumBytes, 0, 1 + PayloadBytes.Length);

			System.Array.Copy (hashBytes, 0, versionPayloadChecksumBytes, 1 + PayloadBytes.Length, 4);

			string retMe = Base58.Encode (versionPayloadChecksumBytes);

			if (retMe == null) {
				throw new NullReferenceException ();
			}

			return retMe;
		}

		public override String ToString ()
		{
			if (humanReadableIdentifier == null) {

				string s = GetHumanReadableIdentifier ();
				try {
					RippleIdentifier rip = new RippleIdentifier (s);

					if (!s.Equals (rip.GetHumanReadableIdentifier ())) {
						throw new CryptographicException ();
					}
				} catch (Exception e) {

					throw e;
				}

				humanReadableIdentifier = s;

			}



			return humanReadableIdentifier;

		}


		public override int GetHashCode ()
		{

			byte [] versionPayloadChecksumBytes = new byte [1 + PayloadBytes.Length + 4];
			versionPayloadChecksumBytes [0] = this.identifierType;
			Array.Copy (PayloadBytes, 0, versionPayloadChecksumBytes, 1, PayloadBytes.LongLength);

			byte [] hashBytes = DoubleSha256 (versionPayloadChecksumBytes, 0, 1 + PayloadBytes.Length);

			return BitConverter.ToInt16 (hashBytes, 0);

		}




		public static byte [] DoubleSha256 (byte [] bytesToDoubleHash, int offset, int length)
		{
			Sha256Digest digest = new Sha256Digest ();

			digest.Reset ();

			byte [] firstrun = new byte [digest.GetDigestSize ()];
			digest.BlockUpdate (bytesToDoubleHash, offset, length);
			digest.DoFinal (firstrun, 0);

			digest.Reset ();

			byte [] result = new byte [digest.GetDigestSize ()];
			digest.BlockUpdate (firstrun, 0, firstrun.Length);

			digest.DoFinal (result, 0);

			return result;
		}

		public byte [] GetBytes ()
		{
			return PayloadBytes;
		}

		public string AsHex ()
		{
			return Base58.ByteArrayToHexString (this.GetBytes ());
		}

		/*
		public int hashCode ()
		{
			int prime = 31;
			int result = 1;

		}
		*/

		//@Override
		public override Boolean Equals (Object obj)
		{
			if (Object.ReferenceEquals (this, obj))
				return true;
			if (obj == null)
				return false;
			/*if (GetType() != obj.GetType ())
				return false;*/

			if (obj is string) {

				if (((string)obj).Equals (this.ToString ())) {
					return true;
				}

				return false;


			}

			if (!(obj is RippleIdentifier))
				throw new FieldAccessException (obj.GetType ().ToString () + " is not a " + nameof (RippleIdentifier) + ". This is a bug");

			RippleIdentifier other = (RippleIdentifier)obj;


			byte [] otherArray = other.PayloadBytes;

			/*
			if (this.payloadBytes.Length != otherArray.Length) 
			{
				return false;
			}

			for ( int i = 0; i < this.payloadBytes.Length; i++) {

				if (this.payloadBytes[i] != otherArray[i])
					return false;

			}
			*/

			// the bouncycastle array equals prevents timing attacks
			return Org.BouncyCastle.Utilities.Arrays.ConstantTimeAreEqual (otherArray, this.PayloadBytes);

			//return true;
		}

		public static bool operator == (RippleIdentifier left, RippleIdentifier right)
		{
			if (System.Object.ReferenceEquals (left, right)) {
				return true;
			}

			if (left is null || right is null) {
				return false;
			}

			return left.Equals (right);
		}



		public static bool operator != (RippleIdentifier left, RippleIdentifier right)
		{
			// Do not change to (left != right) 
			return !(left == right);
		}


		public static bool operator < (RippleIdentifier left, RippleIdentifier right)
		{

			BigInteger l = Base58.DecodeToBigInteger (left.ToString ());
			BigInteger r = Base58.DecodeToBigInteger (right.ToString ());

			return l.CompareTo (r) < 0;
		}

		public static bool operator > (RippleIdentifier left, RippleIdentifier right)
		{
			BigInteger l = Base58.DecodeToBigInteger (left.ToString ());
			BigInteger r = Base58.DecodeToBigInteger (right.ToString ());

			return l.CompareTo (r) > 0;

		}

		public static bool operator <= (RippleIdentifier left, RippleIdentifier right)
		{
			return (left == right) || (left > right);

		}

		public static bool operator >= (RippleIdentifier left, RippleIdentifier right)
		{

			return (left == right) || (left > right);
		}


#if DEBUG
		private const string clsstr = nameof (RippleIdentifier) + DebugRippleLibSharp.colon;
#endif
	}
}

