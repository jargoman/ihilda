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
			RippleDeterministicKeyGenerator generator = new RippleDeterministicKeyGenerator ();

			publicSigningKey = generator.SECP256k1_PARAMS.Curve.DecodePoint(signingPubKey);


			Asn1InputStream decoder = new Asn1InputStream(signatureDEREncodedBytes);
			//LazyDerSequence seq = new LazyDerSequence();
			//DERse

			decoder.Flush ();

			//var derS = decoder.ReadObject ().GetDerEncoded ();

			

			DerSequence seq = (Org.BouncyCastle.Asn1.DerSequence)decoder.ReadObject();
			//DerInteger r = (DerInteger)seq[0];
			//DerInteger s = (DerInteger)seq[1]; // try seq[1].ToAsn1Object(); if cast fails
	    		
			DerInteger rr = (DerInteger)seq [0].ToAsn1Object ();
			//var v = seq.
			DerInteger ss = (DerInteger)seq [1].ToAsn1Object ();
			this.r = rr.PositiveValue;
			this.s = ss.PositiveValue;

			//var deco = decoder.ReadObject ().GetDerEncoded ();
				

		}


		public byte[] EncodeToDER ()
		{
			try {



				// TODO determine what the buffersize should be
				MemoryStream ms = new MemoryStream();
				DerSequenceGenerator seq = new DerSequenceGenerator(ms);
				//DerSequenceGenerator gf = new DerSequenceGenerator (
				seq.AddObject(new DerInteger(r));
				seq.AddObject(new DerInteger(s));

				//var op = seq.GetRawOutputStream ();
				
				ms.Flush ();
				seq.Close();
				var pos = ms.Position;

				byte [] buff = ms.GetBuffer ();
				byte [] ret = new byte [pos];

				Array.Copy (buff, ret, pos);

				
				return ret;

			} catch (Exception e) {
				// TODO debug
				throw e;
			}

		}
	}
}

