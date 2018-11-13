using System;
using System.IO;

namespace IhildaWallet
{
	public class Compliance
	{
		public Compliance ()
		{


		}

		public static bool DoUserAgreement ()
		{

			string agreepath = Path.Combine (FileHelper.COMPLIANCE_FOLDER_PATH, AGREEMENT_SETTINGS_FILE);
			if (File.Exists(agreepath)) {
				return true;
			}

			bool agree = AggreementDialog.DoDialog ();

			if (agree) {
				File.WriteAllText (agreepath, "User has agreed to user software licensing agreement\n");
				return true;
			}

			return false;
		}

		public static string AGREEMENT_SETTINGS_FILE = "USER_AGREEMENT.txt";
	}


}
