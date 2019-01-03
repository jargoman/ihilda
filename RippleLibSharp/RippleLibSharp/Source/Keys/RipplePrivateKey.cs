using System;
using System.IO;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math.EC.Abc;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Crypto.Generators;

// done

namespace RippleLibSharp.Keys
{
	public class RipplePrivateKey : RippleIdentifier
	{
		//Boolean isDeterministic=false;
		private RipplePublicKey publickey;

		public RipplePrivateKey (string stringID) : base (stringID)
		{

		}

		public RipplePrivateKey (byte[] privateKeyBytes) : base (privateKeyBytes, 34)
		{
			// All the Action happens in the super constructor 

			if (privateKeyBytes.Length!=32) {
				// TODO debug
				throw new FormatException("The private key must be of length 32 bytes");
			}
		}


		public RipplePrivateKey (BigInteger privateKeyForAccount) : base (BigIntegerToBytes(privateKeyForAccount,32),34)
		{
			/*
			byte[] bigIntegerBytes = privateKeyForAccount.ToByteArray();
			int nbBytesMissing = 33 - bigIntegerBytes.Length;
			Array.Copy(bigIntegerBytes,nbBytesMissing+1, payloadBytes,0,bigIntegerBytes.Length-1);
			*/
		}



		public static byte[] BigIntegerToBytes (BigInteger biToConvert, int nbBytesToReturn)
		{

			//toArray will return the minimum number of bytes required to encode the biginteger in two's complement.
			//Could be less than the expected number of bytes

			byte[] twosComplement = biToConvert.ToByteArray ();
			byte[] bytesToReturn = new byte[nbBytesToReturn];

			/*
			if (biToConvert.SignValue < 0) {
				// not sure about this one
				throw new NotImplementedException ("bigIntegerToBytes does not implement the retrieval of negative values");
			}


			// not sure about this either
			if (twosComplement [twosComplement.Length - 1] == 0) {
				// there's an empty byte at the end (little endian) to denote a positive number
				byte[] twosComplementWithoutSign = new byte[twosComplement.Length - 1];
				System.Array.Copy (twosComplement, twosComplementWithoutSign, twosComplementWithoutSign.Length);
				twosComplement = twosComplementWithoutSign;
			}
			*/

			if ((biToConvert.BitLength + 7) / 8 != twosComplement.Length) {
				byte[] twosComplementWithoutSign = new byte[twosComplement.Length - 1];
				System.Array.Copy(twosComplement, 1, twosComplementWithoutSign, 0, twosComplementWithoutSign.Length);
				twosComplement = twosComplementWithoutSign;
			}

			int nbBytesOfPaddingRequired = nbBytesToReturn - twosComplement.Length;

			if (nbBytesOfPaddingRequired < 0) {
				throw new IndexOutOfRangeException("nbBytesToReturn "+nbBytesToReturn+" is too small");
			}

			// big endian
			System.Array.Copy(twosComplement, 0, bytesToReturn, nbBytesOfPaddingRequired, twosComplement.Length);

			// little endian
			//System.Array.Copy(twosComplement, bytesToReturn, twosComplement.Length);

			return bytesToReturn;

		}

		public RipplePublicKey GetPublicKey ()
		{
			if (publickey!=null) {
				return publickey;
			}

			RippleDeterministicKeyGenerator generator = new RippleDeterministicKeyGenerator ();
		    	var domain = generator.SECP256k1_PARAMS;
			//BigInteger privateBI = new BigInteger( 1, this.PayloadBytes); 
			//new BigInteger(BinarySerializer.prepareBigIntegerBytes(this.payloadBytes));
				/*														  
			ECPoint uncompressed = RippleDeterministicKeyGenerator.SECP256k1_PARAMS.G.Multiply(privateBI);


			//ECKeyGenerationParameters keyParams = new ECKeyGenerationParameters(RippleDeterministicKeyGenerator.SECP256k1_PARAMS, privateBI);

			Org.BouncyCastle.Crypto.Parameters.ECPrivateKeyParameters keyParams =
				new ECPrivateKeyParameters (privateBI, RippleDeterministicKeyGenerator.SECP256k1_PARAMS);

			ECKeyPairGenerator generator = new ECKeyPairGenerator ();


			var publicPoint = RippleDeterministicKeyGenerator.SECP256k1_PARAMS.Curve.CreatePoint (
				uncompressed.XCoord.ToBigInteger(),
				uncompressed.YCoord.ToBigInteger (),
				false
			);

			publickey = new RipplePublicKey ( publicPoint.GetEncoded(false) );
*/
			BigInteger d = new BigInteger (1, PayloadBytes);
			ECPoint q = domain.G.Multiply (d);

			var publicParams = new ECPublicKeyParameters (q, domain);


			
			byte[] encoded = publicParams.Q.GetEncoded (true);
			
			this.publickey = new RipplePublicKey (encoded);
			//return encoded;

			return publickey;
		}

		public ECPrivateKeyParameters GetECPrivateKey ()
		{
			BigInteger privateBI = new BigInteger(
				1, 
				this.PayloadBytes
			);

			RippleDeterministicKeyGenerator generator = new RippleDeterministicKeyGenerator ();

			ECPrivateKeyParameters privKey = 
				new ECPrivateKeyParameters (
					privateBI, 
					generator.SECP256k1_PARAMS
		    		);


			return privKey;
		}
	}
}

