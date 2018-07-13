using System;

namespace IhildaWallet
{
	public class JsonWallet
	{
		
		public string Secret {
			get;
			set;
		}

		public string Account {
			get;
			set;
		}

		public string Network {
			get;
			set;
		}

		public string Encrypted_Wallet {
			get;
			set;
		}

		public string Encryption_Type {
			get;
			set;
		}

		public string Name {
			get;
			set;
		}

		public string Salt {
			get;
			set;
		}

		public uint? LastKnownLedger {
			get;
			set;
		}


	}
}

