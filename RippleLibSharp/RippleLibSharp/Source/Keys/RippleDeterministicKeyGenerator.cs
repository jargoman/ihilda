using System;
using System.Security;

using System.IO;
//using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

using RippleLibSharp.Binary;
using RippleLibSharp.Util;
using System.Text;

namespace RippleLibSharp.Keys
{
	public class RippleDeterministicKeyGenerator
	{
		public static ECDomainParameters SECP256k1_PARAMS;
		protected byte [] seedBytes;

		static RippleDeterministicKeyGenerator (/*String stringID*/) // STATIC constructor
		{
#if DEBUG
			string method_sig = clsstr + nameof (RippleDeterministicKeyGenerator) + DebugRippleLibSharp.both_parentheses;
			if (DebugRippleLibSharp.RippleDeterministicKeyGenerator) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);

			}
#endif

			string curveName = "secp256k1";

			X9ECParameters paramater = SecNamedCurves.GetByName (curveName);
			SECP256k1_PARAMS = new ECDomainParameters (paramater.Curve, paramater.G, paramater.N, paramater.H);

			/*
			if (Debug.RippleDeterministicKeyGenerator) {
				testVectors();

			}
			*/
		}

		public RippleDeterministicKeyGenerator (RippleSeedAddress secret) : this (secret.GetBytes ())
		{
			//this.seedBytes = secret.getBytes();

		}

		public RippleDeterministicKeyGenerator (byte [] bytesSeed)
		{
			if (bytesSeed.Length != 16) {
				throw new FormatException ("The seed size should be 128 bit, was " + bytesSeed.Length * 8);
			}

			this.seedBytes = bytesSeed;
		}

		public static byte [] HalfSHA512 (byte [] byteToHash)
		{

#if DEBUG

			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append (clsstr);
			stringBuilder.Append (nameof (HalfSHA512));
			stringBuilder.Append (DebugRippleLibSharp.left_parentheses);
			stringBuilder.Append (typeof (byte).ToString ());
			stringBuilder.Append (DebugRippleLibSharp.array_brackets);
			stringBuilder.Append (nameof (byteToHash));
			stringBuilder.Append (DebugRippleLibSharp.right_parentheses);
			string method_sig = stringBuilder.ToString ();

			if (DebugRippleLibSharp.RippleDeterministicKeyGenerator) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			byte [] ret32 = null;
			try {

				//SHA512 sha = new SHA512Managed();
				//result = sha.ComputeHash(byteToHash);

				Sha512Digest digest = new Sha512Digest ();

				digest.BlockUpdate (byteToHash, 0, byteToHash.Length);
				byte [] result = new byte [digest.GetDigestSize ()];

				digest.DoFinal (result, 0);



				ret32 = new byte [32];
				Array.Copy (result, ret32, 32); // copy half the bytes

			} catch (Exception e) {

#if DEBUG
				Logging.WriteLog (method_sig + "\n" + e.Message);
#endif


				throw e;
			}

			return ret32; // return ret32 not result
		}


		public byte [] GetPrivateRootKeyBytes ()
		{

			// TODO portable? endianess? testing?
			for (int seq = 0; ; seq++) {
				MemoryStream mem = new MemoryStream (4);

				BigEndianWriter bew = new BigEndianWriter (mem);

				bew.Write (seedBytes);
				bew.Write (seq);


				bew.Flush ();

				mem.Flush ();

				byte [] seedAndSeqBytes = mem.ToArray ();

				if (seedAndSeqBytes.Length != seedBytes.Length + 4) {
					throw new Exception ("I'd really like to know if this thows. seedAndSeqBytes.Length = " + seedAndSeqBytes.Length);
				}

				byte [] privateGeneratorBytes = HalfSHA512 (seedAndSeqBytes);
				BigInteger privateGeneratorBI = new BigInteger (1, privateGeneratorBytes);

				if (privateGeneratorBI.CompareTo (SECP256k1_PARAMS.N) == -1) {
					return privateGeneratorBytes;
				}
			}
		}


		public ECPoint GetPublicGeneratorPoint ()
		{
			byte [] privateGeneratorBytes = GetPrivateRootKeyBytes ();
			ECPoint publicGenerator = new RipplePrivateKey (privateGeneratorBytes).GetPublicKey ().GetPublicPoint ();
			return publicGenerator;
		}

		public RipplePrivateKey GetAccountPrivateKey (int accountNumber)
		{
			BigInteger privateRootKeyBI = new BigInteger (1, GetPrivateRootKeyBytes ());
			// TODO factor out the common part with the public key

			ECPoint publicGeneratorPoint = GetPublicGeneratorPoint ();

			byte [] publicGeneratorBytes = publicGeneratorPoint.GetEncoded ();

			/*
			MemoryStream ms = new MemoryStream(4);
			BigEndianWriter bew = new BigEndianWriter(ms);
			bew.Write(accountNumber);
			bew.Flush();
			ms.Flush();

			byte[] accountNumberBytes = ms.ToArray();
			*/
			BigInteger pubGenSeqSubSeqHashBI;

			for (int subSequence = 0; ; subSequence++) {
				MemoryStream mem = new MemoryStream (publicGeneratorBytes.Length + 4 + 4);
				BigEndianWriter ben = new BigEndianWriter (mem);  // luckily I rewrote this. I found a serious bug :O

				ben.Write (publicGeneratorBytes);
				ben.Write (accountNumber);
				ben.Write (subSequence);
				ben.Flush ();
				mem.Flush ();

				byte [] pubGenAccountSubSeqBytes = mem.ToArray ();
				byte [] publicGeneratorAccountSeqHashBytes = HalfSHA512 (pubGenAccountSubSeqBytes);

				pubGenSeqSubSeqHashBI = new BigInteger (1, publicGeneratorAccountSeqHashBytes);
				if (pubGenSeqSubSeqHashBI.CompareTo (SECP256k1_PARAMS.N) == -1 && !pubGenSeqSubSeqHashBI.Equals (BigInteger.Zero)) {
					break;
				}


			}

			BigInteger privateKeyForAccount = privateRootKeyBI.Add (pubGenSeqSubSeqHashBI).Mod (SECP256k1_PARAMS.N);
			return new RipplePrivateKey (privateKeyForAccount);
		}


		public RipplePublicKey GetAccountPublicKey (int accountNumber)
		{
			ECPoint publicGeneratorPoint = GetPublicGeneratorPoint ();
			byte [] publicGeneratorBytes = publicGeneratorPoint.GetEncoded ();

			byte [] publicGeneratorAccountSeqHashBytes;
			for (int subSequence = 0; ; subSequence++) {
				MemoryStream ms = new MemoryStream (publicGeneratorBytes.Length + 4 + 4);
				BigEndianWriter ben = new BigEndianWriter (ms);

				ben.Write (publicGeneratorBytes);
				ben.Write (accountNumber);
				ben.Write (subSequence);

				ben.Flush ();
				ms.Flush ();

				byte [] pubGenAccountSubSeqBytes = ms.ToArray ();

				publicGeneratorAccountSeqHashBytes = HalfSHA512 (pubGenAccountSubSeqBytes);
				BigInteger pubGenSeqSubSeqHashBI = new BigInteger (1, publicGeneratorAccountSeqHashBytes);

				if (pubGenSeqSubSeqHashBI.CompareTo (SECP256k1_PARAMS.N) == -1) { // TODO should this also test for non zero value like the above function? 
					break;
				}
			}
			ECPoint temporaryPublicPoint = new RipplePrivateKey (publicGeneratorAccountSeqHashBytes).GetPublicKey ().GetPublicPoint ();
			ECPoint accountPublicKeyPoint = publicGeneratorPoint.Add (temporaryPublicPoint);
			byte [] publicKeyBytes = accountPublicKeyPoint.GetEncoded ();
			return new RipplePublicKey (publicKeyBytes);

		}

		public RipplePublicGeneratorAddress GetPublicGeneratorFamily ()
		{
			byte [] publicGeneratorBytes = GetPublicGeneratorPoint ().GetEncoded ();
			return new RipplePublicGeneratorAddress (publicGeneratorBytes);
		}

		public static bool TestVectors ()
		{
			Logging.WriteLog ("testVectors suite : begin \n");

			byte [] masterseedhex = { 0x71, 0xED, 0x06, 0x41, 0x55, 0xFF, 0xAD, 0xFA, 0x38, 0x78, 0x2C, 0x5E, 0x01, 0x58, 0xCB, 0x26 };
			String humanseed = "shHM53KPZ87Gwdqarm1bAmPeXg8Tn";

			RippleSeedAddress seed = new RippleSeedAddress (masterseedhex);

			RippleSeedAddress seed2 = new RippleSeedAddress (humanseed);

			if (seed.Equals (seed2)) {
				Logging.WriteLog ("Test Vectors : seed == seed2");
			}

			Logging.WriteLog ("hex seed = " + masterseedhex.ToString () + "\n human seed = " + seed.ToString ());

			RippleAddress privatekey = seed.GetPublicRippleAddress ();

			Logging.WriteLog ("seed shHM53KPZ87Gwdqarm1bAmPeXg8Tn becomes " + privatekey.ToString ());

			return true;
		}

#if DEBUG
		private const string clsstr = nameof (RippleDeterministicKeyGenerator) + DebugRippleLibSharp.colon;
#endif
	}
}

