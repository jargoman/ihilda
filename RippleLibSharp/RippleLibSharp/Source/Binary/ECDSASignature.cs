// gpl3

using System;
using System.IO;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Parameters;

using RippleLibSharp.Keys;

namespace RippleLibSharp.Binary
{
	public class ECDSASignature
	{
#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public BigInteger r = null;
		public BigInteger s = null;

		public ECPoint publicSigningKey = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant



		public ECDSASignature (BigInteger r, BigInteger s, ECPoint publicSigningKey)
		{
			this.r = r;
			this.s = s;
			this.publicSigningKey = publicSigningKey;
		}

		public ECDSASignature (byte[] signatureDEREncodedBytes, byte[] signingPubKey)
		{
			publicSigningKey = RippleDeterministicKeyGenerator.SECP256k1_PARAMS.Curve.DecodePoint(signingPubKey);


			Asn1InputStream decoder = new Asn1InputStream(signatureDEREncodedBytes);
			//LazyDerSequence seq = new LazyDerSequence();
			//DERse
			DerSequence seq = (Org.BouncyCastle.Asn1.DerSequence)decoder.ReadObject();
			//DerInteger r = (DerInteger)seq[0];
			//DerInteger s = (DerInteger)seq[1]; // try seq[1].ToAsn1Object(); if cast fails

			DerInteger rr = (DerInteger)seq [0].ToAsn1Object ();
			DerInteger ss = (DerInteger)seq [1].ToAsn1Object ();
				
			this.r = rr.PositiveValue;
			this.s = ss.PositiveValue;
		}

		public byte[] EncodeToDER ()
		{
			try {
				MemoryStream ms = new MemoryStream(72);
				DerSequenceGenerator seq = new DerSequenceGenerator(ms);
				seq.AddObject(new DerInteger(r));
				seq.AddObject(new DerInteger(s));
			
				seq.Close();

				return ms.GetBuffer();
			} catch (Exception e) {
				// TODO debug
				throw e;
			}

		}
	}
}

