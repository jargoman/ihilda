using System;
using System.IO;
using RippleLibSharp.Keys;
using RippleLibSharp.Transactions;

namespace IhildaWallet
{
	public class RippleWalletVariables
	{
		public RippleWalletVariables ()
		{
		}

		public string WalletName {
			get;
			set;
		}

		protected RippleSeedAddress Seed {
			get;
			set;
		}

		protected RipplePrivateKey PrivateKey {
			get;
			set;
		}

		public String Encrypted_PrivateKey {
			get;
			set;
		}

		public int NextTransactionSequenceNumber {
			get;
			set;
		}


		public String Encrypted_Wallet {
			get;
			set;
		}// encryptes bytes encoded as ripple Identifier

		public String Encryption_Type {
			get;
			set;
		}

		public String Account {
			get;
			set;
		}

		public RippleWalletTypeEnum AccountType {
			get;
			set;
		}


		public String Salt {
			get;
			set;
		}



		public uint? NotificationLedger {
			get;
			set;
		}

		public string WalletPath {
			get;
			set;
		}


		public string BotLedgerPath {
			get {
				return Path.Combine (FileHelper.WALLET_TRACK_PATH, WalletName + ".bot");
			}
		}

		public string NotificationLadgerPath {
			get {
				return Path.Combine (FileHelper.WALLET_TRACK_PATH, WalletName + ".led");
			}

		}

		public string Notification {
			get;
			set;
		}

		public RippleCurrency LastKnownNativeBalance {
			get;
			set;
		}

		public string BalanceNote {
			get;
			set;
		}

		public byte CouldNotUpdateBalanceCount {
			get;
			set;
		}


		public bool HasWalletError {
			get { return _hasError || (string.IsNullOrWhiteSpace(WalletError)); }
			set { _hasError = value; }
		}

		public string WalletError {
			get;
			set;
		}

		protected bool _hasError = false;

		#region regularkey

		public RippleSeedAddress Regular_Seed {
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

	}
}
