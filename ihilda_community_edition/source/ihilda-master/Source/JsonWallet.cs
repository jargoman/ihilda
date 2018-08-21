using System;

namespace IhildaWallet
{
	public class JsonWallet
	{
		
#region master
		public string Secret {
			get;
			set;
		}

		public string Account {
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

#endregion



#region regularkey

		public string Regular_Key_Secret {
			get;
			set;
		}

		public string Regular_Key_Account {
			get;
			set;
		}

		public string Encrypted_Regular_Wallet {
			get;
			set;
		}

		public string Regular_Key_Encryption_Type {
			get;
			set;
		}

#endregion


		public string Network {
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

		public string AccountType { 
			get; 
			set; 
		}
	}
}

