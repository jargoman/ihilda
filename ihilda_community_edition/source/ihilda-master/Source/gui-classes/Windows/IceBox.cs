using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RippleLibSharp.Network;
using IhildaWallet.Networking;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;
using RippleLibSharp.Commands.Stipulate;
using IhildaWallet.Util;

namespace IhildaWallet
{
	public partial class IceBox : Gtk.Window
	{
		public IceBox (RippleWallet rippleWallet, LicenseType license, Decimal balance) :
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();

			this.target = license;
			//this.balance = balance;

			difference = (decimal)target - balance;

			StringBuilder target_cur = new StringBuilder ();

			target_cur.Append (LeIceSense.LICENSE_CURRENCY);
			target_cur.Append (":");
			target_cur.Append (LeIceSense.LICENSE_ISSUER);

			StringBuilder sb = new StringBuilder ();



			sb.Append ("This feature require an account balance of ");
			sb.Append (((int)target).ToString ());
			sb.Append (" ");
			sb.Append (target_cur.ToString());

			sb.Append (".\nYour current balance is ");
			sb.Append(balance.ToString());
			sb.Append (" ");
			sb.Append (target_cur.ToString());
			sb.Append (".\nWould you like to purchase ");
			sb.Append (this.difference.ToString());
			sb.Append ( " " );
			sb.Append (target_cur.ToString());
			sb.Append (" to continue?");

			textview1.Buffer.Text = sb.ToString ();



			this.button331.Clicked += Button331_Clicked;

			this.button332.Clicked += (object sender, EventArgs e) => this.Destroy();
		}

		private LicenseType target = LicenseType.PAYMENT;
		//private Decimal balance = 0m;
		private Decimal difference = 0m;

		void Button331_Clicked (object sender, EventArgs e)
		{
			RippleWallet rw = _rippleWallet;

			// TODO security/stability check. 
			//What are the implications of the selectedwallet changing between pathfind request and signature
			//

			if (rw == null ) {
				// todo debug
				return;
			}

			string destination_account = rw.GetStoredReceiveAddress();


			string currency = LeIceSense.LICENSE_CURRENCY;
			string issuer = LeIceSense.LICENSE_ISSUER;


			difference = Math.Round (difference);

			RippleCurrency rc = new RippleCurrency (difference, issuer, currency);

			//Thread thread = new Thread (
			Task.Run (
				delegate {
					NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();
					Task< Response<PathFindResult> > task = PathFind.GetResult (
						                                         rw.GetStoredReceiveAddress (),
						                                         destination_account,
						                                         rc,
						                                         ni

					                                         );

					task.Wait ();

					Response<PathFindResult> res = task.Result;

					PathFindResult resul = res.result;

					this.pathstree1.SetPathFindResult (resul);
				}
			);
			//);

			//thread.Start ();






		}

		private RippleWallet _rippleWallet = null;

	}
}

