using System;
using System.Collections.Generic;
using System.Text;

namespace IhildaWallet
{
	public class WalletTreeModel
	{
		public WalletTreeModel (RippleWallet rw)
		{




			StringBuilder sb = new StringBuilder ();

			sb.Clear ();



			if (rw?.AccountType == RippleWalletTypeEnum.Master || rw?.AccountType == RippleWalletTypeEnum.MasterPrivateKey) {

				#region account
				sb.Append ("<span ");

				if (!ProgramVariables.darkmode) {
					sb.Append ("fgcolor=\"green\"><big>");
				} else {
					sb.Append ("fgcolor=\"chartreuse\"><big>");
				}
				
				sb.Append (rw?.GetStoredReceiveAddress () ?? " ");


				sb.Append ("</big></span>");
				#endregion


			
			}

			if (rw?.AccountType == RippleWalletTypeEnum.Regular) {
				sb.Append ("<span ");
				//if (b) {

					/*
					if (!Program.darkmode) {
						sb.Append ("bgcolor=\"lavender\"");
					} else {
						sb.Append ("bgcolor=\"black\"");
					}
					*/
				//}

				if (ProgramVariables.darkmode) {
					sb.Append ("fgcolor=\"chartreuse\">");
				} else {
					sb.Append ("fgcolor=\"green\">");
				}

				sb.Append (rw?.GetStoredReceiveAddress () ?? "Missing master account");
				sb.Append ("</span>");




				sb.AppendLine ();
				sb.Append ("<span ");


				sb.Append ("fgcolor=\"grey\">");
				sb.Append (rw?.Regular_Key_Account ?? "missing regular account key ");
				sb.Append ("</span>");



			}
			//string accType = );




			string name = rw?.WalletName ?? "";
			//if (b) {
				if (ProgramVariables.darkmode) {
					name = "<span fgcolor=\"chartreuse\" ><b>" + name + "</b></span>";
				} else {
					name = "<span fgcolor=\"green\" ><b>" + name + "</b></span>";
				}
			//}

			StringBuilder stringBuilder = new StringBuilder ();

			stringBuilder.Append (name);
			stringBuilder.AppendLine ();
			stringBuilder.Append ("<span foreground=\"grey\">");
			stringBuilder.Append (rw?.AccountType.ToString ());
			stringBuilder.AppendLine ();
			stringBuilder.Append (rw?.GetStoredEncryptionType () ?? "");
			stringBuilder.Append ("</span>");

			WalletName = stringBuilder.ToString ();
			Account = sb.ToString () ?? "";

		}

		public string WalletName { get; set; }

		public string Account { get; set; }

		//public IEnumerable<string> Notifications { get; set; }


	}
}
