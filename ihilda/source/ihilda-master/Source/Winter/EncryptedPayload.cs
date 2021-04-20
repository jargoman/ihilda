using System;
namespace IhildaWallet
{
	public class EncryptedPayload
	{
		public EncryptedPayload ()
		{
		}

		public string userId { get; set; }
		public string payloadtype { get; set; }
		public byte [] bytes { get; set; }


		public string clientRsaPublicKey { get; set; }
	}
}
