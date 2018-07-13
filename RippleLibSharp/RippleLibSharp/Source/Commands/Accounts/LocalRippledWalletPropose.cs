using System;

using System.Threading.Tasks;

using RippleLibSharp.Result;

using RippleLibSharp.LocalRippled;

using RippleLibSharp.Util;

using Codeplex.Data;

namespace RippleLibSharp.Commands.Accounts
{
	public static class LocalRippledWalletPropose
	{
		

		public static Response<RpcWalletProposeResult> GetResult ( string bseed) {

			//string json = "{\"method\":\"wallet_propose\", \"params\": [{\"seed\": \"" +
			//	bseed + 
			//	"\", \"key_type\": \"secp256k1\"}]}";


				

			CommandLineExecuter cle = new CommandLineExecuter ();

			string output = cle.WalletPropose (bseed);

			if (output == null) {
				return null;
			}

			Response<RpcWalletProposeResult> r = DynamicJson.Parse (output);

			if (r.error != null) {

				Logging.WriteLog (r.error_message);
				return null;
			}

			return r;
		}
	}
}

