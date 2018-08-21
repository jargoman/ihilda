using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Gtk;
using RippleLibSharp.Transactions;
using RippleLibSharp.Commands.Stipulate;
using RippleLibSharp.Result;

using RippleLibSharp.Network;
using IhildaWallet.Networking;
using System.Linq;

namespace IhildaWallet
{
	public partial class FreeTrialAlertDialog : Gtk.Dialog
	{
		public FreeTrialAlertDialog ()
		{
			this.Build ();
		}

		public void HideAcceptButton ()
		{
			buttonOk.Hide ();
		}


		public void SetMessageString (RippleWallet rw, string amount)
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append ("This feature requires fee of ");
			sb.Append (amount);
			sb.Append (" for account ");
			sb.Append (rw.GetStoredReceiveAddress ());
			sb.Append (". \nClick \"purchase\" to buy now or click \"trial\" to enter trial mode.\n");
			//messageString = sb.ToString ();
			textview2.Buffer.Text = sb.ToString ();

		}

		public static bool DoDialog (RippleWallet rw, Decimal amount)
		{
			ResponseType r = ResponseType.None;

			ManualResetEvent ev = new ManualResetEvent (false);
			Application.Invoke (
				(object sender, EventArgs e) => {

					using (FreeTrialAlertDialog ftad = new FreeTrialAlertDialog ()) {

						ftad.SetMessageString (rw, amount.ToString ());


						r = (ResponseType)ftad.Run ();
						ftad.Destroy ();
						ev.Set ();



					}



				});

			ev.WaitOne ();


			switch (r) {
			case ResponseType.Cancel:

				return false;

			case ResponseType.Accept:
				return DoPurchaseDialog (rw, amount);
				//return true;

			case ResponseType.Ok:

				// 
				return true;

			default:
				return true;
			}

			//return true;

		}

		public static bool DoPurchaseDialog (RippleWallet rippleWallet, Decimal ICE_amount)
		{


			AutomatedOrder offer = new AutomatedOrder ();

			NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();
			if (ni == null) {
				// TODO DEBUG
				return false;
			}

			RippleCurrency counterCurrency = new RippleCurrency {
				currency = RippleCurrency.NativeCurrency,
				IsNative = true
			};

			RippleCurrency baseCurrency = new RippleCurrency {
				currency = Util.LeIceSense.LICENSE_CURRENCY,
				issuer = Util.LeIceSense.LICENSE_ISSUER
			};

			Task<Response<BookOfferResult>> task = BookOffers.GetResult (baseCurrency, counterCurrency, ni);
			task.Wait ();

			Response<BookOfferResult> response = task.Result;
			if (response == null) {
				// TODO
				MessageDialog.ShowMessage ("Error", "response == null");
				return false;
			}
			BookOfferResult result = response.result;
			if (result == null) {
				// TODO
				MessageDialog.ShowMessage ("Error", "result == null");
				return false;
			}

			Offer [] offers = result.offers;
			if (offers == null) {
				MessageDialog.ShowMessage ("Error", "offers == null");
				return false;
			}

			if (!offers.Any()) {
				MessageDialog.ShowMessage ("Error", "There aren't any orders offering " + baseCurrency.ToIssuerString() + " for " + RippleCurrency.NativeCurrency);
				return false;
			}

			Decimal sum = 0.0m;
			for (int i = 0; i < offers.Length; i++) {
				sum += offers [i].taker_gets.amount;

				if (sum > ICE_amount) {
					RippleCurrency baseCur = offers [i].taker_gets.DeepCopy();
					RippleCurrency counterCur = offers [i].taker_pays.DeepCopy();
					var price = baseCur.GetNativeAdjustedPriceAt (counterCur);

					baseCur.amount = ICE_amount;
					counterCur.amount = ICE_amount * price * 1000000;

					AutomatedOrder automatedOrder = new AutomatedOrder {
						taker_gets = counterCur,
						taker_pays = baseCur,
						Account = rippleWallet.Account
					};


					return OrderSubmitWindow.ShortHandSubmit ( rippleWallet, new AutomatedOrder [] { automatedOrder }, Util.LicenseType.NONE);


				}
			}

			MessageDialog.ShowMessage ("Error", "Not enough orders to fill ");
			return false;
		}

		//private string messageString = null;
	}
}

