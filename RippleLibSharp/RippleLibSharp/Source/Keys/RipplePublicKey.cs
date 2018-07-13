//TODO license pmarches port

using System;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Math.EC;

// done

namespace RippleLibSharp.Keys
{
	public class RipplePublicKey : RippleIdentifier
	{
		public RipplePublicKey ( byte[] publicKeyBytes ) : base (publicKeyBytes, 35)
		{
			if (publicKeyBytes.Length!=33) {
				throw new FormatException("The public key must be of length 33 bytes was of length " + publicKeyBytes.Length.ToString());
			}
		}

		public RippleAddress GetAddress ()
		{
				// Hashing of the publicKey is performed with a single SHA256 instead of
                // the typical ripple HalfSHA512

			Sha256Digest sha = new Sha256Digest();

			sha.BlockUpdate(PayloadBytes, 0, PayloadBytes.Length);
			byte[] sha256PubKeyBytes = new byte[32];
			sha.DoFinal(sha256PubKeyBytes,0);

			RipeMD160Digest digest = new RipeMD160Digest();
			digest.BlockUpdate(sha256PubKeyBytes,0,sha256PubKeyBytes.Length);
			byte[] accountIdBytes = new byte[20];
			digest.DoFinal(accountIdBytes,0);

			return new RippleAddress (accountIdBytes) ; //  wow awesome
		}

		public ECPoint GetPublicPoint() {
			return RippleDeterministicKeyGenerator.SECP256k1_PARAMS.Curve.DecodePoint(PayloadBytes);
		}
	}
}

