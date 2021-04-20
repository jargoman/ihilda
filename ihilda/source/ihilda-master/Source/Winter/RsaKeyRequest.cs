using System;
namespace IhildaWallet
{
	public class RsaKeyRequest
	{
		public RsaKeyRequest ()
		{
		}

		public string userId { get; set; }
		public string initialToken { get; set; }
		public string onetimepassword { get; set; }


		//public string handShake { get; set; }
	}
}
