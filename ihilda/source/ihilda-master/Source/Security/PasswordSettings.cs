using System;
using System.Threading;

namespace IhildaWallet
{
	public class PasswordSettings
	{
		public PasswordSettings ()
		{
		}


		public const int MAX_PASS_ATTMPS = 7;
		public const string MAX_PASS_MESSAGE = "Max password attempts";
	}


	// used to cleanly request password from a given wallet
	public class PasswordAttempt
	{

		public DecryptResponse DoRequest (RippleWallet rw, CancellationToken token) {


			DecryptResponse decryptResponse;

			// repeated password attempts
			for (int i = 0; !token.IsCancellationRequested && i < PasswordSettings.MAX_PASS_ATTMPS; i++) {

				// TODO time lock after x attempts


				decryptResponse = rw.GetDecryptedSeed ();

				if (decryptResponse.HasError) {

					// TODO add error object event
					OnError?.Invoke (null, null);
					continue;
				}

				var rippleSeedAddress = decryptResponse.Seed;

				if (rippleSeedAddress?.GetHumanReadableIdentifier () != null) {

					decryptResponse.HasError = false;



					return decryptResponse;
				}


			}



			MaxPassEvent?.Invoke (null, null);


			decryptResponse = new DecryptResponse {
				HasError = true,
				ErrorMessage = PasswordSettings.MAX_PASS_MESSAGE
			};

			return decryptResponse;

		}




		public event EventHandler OnError;
		public event EventHandler MaxPassEvent;
		public event EventHandler InvalidPassEvent;

	}

}