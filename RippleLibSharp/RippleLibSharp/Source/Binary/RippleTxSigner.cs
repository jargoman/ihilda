using System;
using System.IO;
//using System.Security;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Asn1.Sec;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;

using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Generators;

using RippleLibSharp.Keys;

namespace RippleLibSharp.Binary
{
	public class RippleTxSigner
	{
#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private RipplePrivateKey privateKey;/* = null;*/
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		public RippleTxSigner (RipplePrivateKey privateKey)
		{
			this.privateKey = privateKey;	


		}

		public RippleBinaryObject Sign ( RippleBinaryObject serObjToSign )
		{

			if (serObjToSign == null) {
				throw new ArgumentNullException (nameof (serObjToSign), "Parmeter not serObjToSign is null");
			}
			if (serObjToSign.GetField (BinaryFieldType.TxnSignature) != null) {
				throw new Exception ("Object already signed");
			}

			RippleBinaryObject signedRBO = new RippleBinaryObject(serObjToSign);
			signedRBO.PutField (BinaryFieldType.SigningPubKey, privateKey.GetPublicKey().GetPublicPoint().GetEncoded());

			byte[] hashOfRBOBytes = signedRBO.GenerateHashFromBinaryObject();
			ECDSASignature signature = SignHash(hashOfRBOBytes);

			signedRBO.PutField(BinaryFieldType.TxnSignature, signature.EncodeToDER());
			return signedRBO;
		}


		private ECDSASignature SignHash (byte[] hashOfBytes) {
			if (hashOfBytes.Length!=32) {
				throw new FormatException("can sign only a hash of 32 bytes");
			}

			ECDsaSigner signer = new ECDsaSigner();

			Org.BouncyCastle.Crypto.Parameters.ECPrivateKeyParameters privKey = privateKey.GetECPrivateKey();
			signer.Init(true,privKey);

			BigInteger[] RandS = signer.GenerateSignature(hashOfBytes);
			return new ECDSASignature ((BigInteger)RandS.GetValue(0), (BigInteger)RandS.GetValue(1), privateKey.GetPublicKey().GetPublicPoint());
		}

		public Boolean IsSignatureVerified (RippleBinaryObject serObj)
		{
			try {
				byte[] signatureBytes = (byte[]) serObj.GetField(BinaryFieldType.TxnSignature);

				if ( signatureBytes==null) {
					throw new Exception ("The specified  has no signature");
				}

				byte[] signingPubKeyBytes = (byte[]) serObj.GetField(BinaryFieldType.SigningPubKey);

				if (signingPubKeyBytes==null) {
					throw new Exception("The specified  has no public key associated to the signature");
				}

				RippleBinaryObject unsignedRBO = serObj.GetUnsignedCopy();
				byte[] hashToVeryfy = unsignedRBO.GenerateHashFromBinaryObject();

				ECDsaSigner signer = new ECDsaSigner();
				ECDSASignature signature = new ECDSASignature(signatureBytes, signingPubKeyBytes);
				if (signature.publicSigningKey==null) {
					// shouldn't ever happen
					throw new Exception("ECDSASignature publicSigningKey is null");
				}
				//Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters
				signer.Init(false, new Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters(signature.publicSigningKey,RippleDeterministicKeyGenerator.SECP256k1_PARAMS));
				
				return signer.VerifySignature(hashToVeryfy,signature.r, signature.s);
			} catch (Exception e) {
				//TODO debug
				throw e;
			}
		}
	}
}

