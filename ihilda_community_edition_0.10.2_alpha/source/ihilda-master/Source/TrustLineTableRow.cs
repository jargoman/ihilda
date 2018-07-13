using System;
using RippleLibSharp.Trust;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public class TrustLineTableRow
	{
		

		public static Object GetTableXIndex (TrustLine trust, int x) {
			#if DEBUG
			string method_sig = clsstr + nameof (GetTableXIndex) + DebugRippleLibSharp.left_parentheses + x.ToString() + DebugRippleLibSharp.right_parentheses;

			if (DebugIhildaWallet.TrustLine) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin );
			}
			#endif
			switch (x) {

				case 0:
				return trust.account;
				//break;

				case 1:
				return trust.balance;
				//break;

				case 2:
				return trust.currency;
				//break;

				case 3:
				return trust.limit;
				//break;
				case 4:
				return trust.limit_peer;
				//break;
				case 5:
				return trust.quality_in;
				//break;
				case 6:
				return trust.quality_out;
				//break;
			//case 7:
				//return tru

				default:
				#if DEBUG
				Logging.WriteLog (method_sig + "Line.getTableXIndex index out of bounds\n");
				#endif
				return null;
				//break;
			}


		}

		public static Object GetTableXIndex (TrustLine trust, uint x)
		{
			return GetTableXIndex(trust, (int)x);
		}

		public static String[] titles = { "Account","Balance","Currency","Limit","Limit Peer","Quality In","Quality Out", "Revoke" };

		public const uint numColumns = 8;

		#if DEBUG
		private const string clsstr = nameof (TrustLine) + DebugRippleLibSharp.colon;
		#endif
	}
}

