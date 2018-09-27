using System;
using System.Threading.Tasks;
using System.Linq;

using RippleLibSharp.Keys;

using IhildaWallet.Networking;
using RippleLibSharp.Network;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Result;
using RippleLibSharp.Trust;
using RippleLibSharp.Transactions;

using Gtk;
using System.Collections.Generic;
using System.Text;
using RippleLibSharp.Util;
using System.Threading;
using RippleLibSharp.Commands.Stipulate;

namespace IhildaWallet.Util
{
	public class LeIceSense
	{
		/*
		public LeIceSense ()
		{
		}
		*/

		public decimal? GetIce (RippleAddress rippleAdress)
		{

#if DEBUG
			StringBuilder stringBuilder = null;
			string method_sig = null;

			if (DebugIhildaWallet.LeIceSence) {
				stringBuilder = new StringBuilder ();
				stringBuilder.Append (clsstr);
				stringBuilder.Append (nameof (GetIce));
				stringBuilder.Append (DebugRippleLibSharp.left_parentheses);
				stringBuilder.Append (nameof (rippleAdress));
				stringBuilder.Append (DebugRippleLibSharp.right_parentheses);
				method_sig = stringBuilder.ToString ();
				stringBuilder.Append (DebugRippleLibSharp.beginn);
				Logging.WriteLog ( stringBuilder.ToString());
				stringBuilder.Clear ();
			}

#endif


			string addr = rippleAdress?.ToString ();
			if (addr == null) {


#if DEBUG

				if (DebugIhildaWallet.LeIceSence) {
					stringBuilder.Append (method_sig);
					stringBuilder.AppendLine ("addr == null, returning null\n");
					Logging.WriteLog ( stringBuilder.ToString() );
					stringBuilder.Clear ();
				}
#endif
				return null;
			}

#if DEBUG
			if (DebugIhildaWallet.LeIceSence) {
				stringBuilder.Append (method_sig);
				stringBuilder.Append ("addr = ");
				stringBuilder.AppendLine (addr);
				//stringBuilder.Append ("\n");
				Logging.WriteLog ( stringBuilder.ToString() );
				stringBuilder.Clear ();
			}
#endif

			//AccountLines.sync(addr);

			NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();
			if (ni == null) {
				
#if DEBUG
				if (DebugIhildaWallet.LeIceSence) {
					stringBuilder.Append (method_sig);
					stringBuilder.AppendLine ("ni == null, returning\n");
					Logging.WriteLog ( stringBuilder.ToString() );
					stringBuilder.Clear ();
				}
#endif
				return null;
			} 

			Task<Response<AccountLinesResult>> task = AccountLines.GetResult (addr, ni);


#if DEBUG
			if ( DebugIhildaWallet.LeIceSence) {
				stringBuilder.Append (method_sig);
				stringBuilder.AppendLine ("waiting \n");
				Logging.WriteLog ( stringBuilder.ToString());
				stringBuilder.Clear ();
			}
#endif

			if (task == null) {
				
#if DEBUG
				if (DebugIhildaWallet.LeIceSence) {
					stringBuilder.Append (method_sig);
					stringBuilder.AppendLine ("task == null");
					stringBuilder.Clear ();
				}
#endif
				return null;
			}

			task.Wait ();


#if DEBUG
			if (DebugIhildaWallet.LeIceSence) {
				stringBuilder.Append (method_sig);
				stringBuilder.AppendLine ("done waiting \n");
				Logging.WriteLog ( stringBuilder.ToString() );
				stringBuilder.Clear ();
			}
#endif

			Response<AccountLinesResult> response = task.Result;

			if (response == null) {

				#if DEBUG


				if (DebugIhildaWallet.LeIceSence) {
					stringBuilder.Append (method_sig);
					stringBuilder.AppendLine ("response == null");
					stringBuilder.Clear ();
				}


				#endif
				return null;
			}

			if (response.HasError ()) {
				if (response?.error_code == 19) {

					StringBuilder sb = new StringBuilder ();

					sb.Append ("The specified account ");
					sb.Append (rippleAdress?.ToString () ?? " { null } ");
					sb.Append (" is not found. Either it does not exist or has not been funded with ");
					sb.AppendLine (RippleCurrency.NativeCurrency);

					MessageDialog.ShowMessage (sb.ToString());

#if DEBUG
					stringBuilder.Append (method_sig);
					stringBuilder.AppendLine (sb.ToString());
					Logging.WriteLog (stringBuilder.ToString());
					stringBuilder.Clear ();

#endif
					sb.Clear ();
				}
				return 0.0m;
			}

			if (response.result == null) {
				return null;
			}

			AccountLinesResult result = response.result;


			TrustLine [] tla = result.lines;

			return GetIceFromList (tla);


		}

		private decimal GetIceFromList (TrustLine [] tla)
		{

			//from tla select TrustLine t where 

			var trust = from TrustLine t in tla
						where
						t != null
						&& LeIceSense.LICENSE_CURRENCY.Equals (t.currency)
						&& LeIceSense.LICENSE_ISSUER.Equals (t.account)
						select t;

			foreach (TrustLine t in trust) {
				if (t == null)
					continue;

				return t.GetBalanceAsDecimal ();

			}

			return 0m;

		}

		public static bool IsLicenseExempt (RippleCurrency rippleCurrency)
		{

			if (rippleCurrency == null) {
				return false;
			}
			if (
				((LeIceSense.LICENSE_CURRENCY == rippleCurrency?.currency) && (LeIceSense.LICENSE_ISSUER == rippleCurrency?.issuer))
				|| rippleCurrency.IsNative
			) {
				return true;
			}

			return false;

		}

		public static bool IsLicenseCurrency (RippleCurrency rippleCurrency)
		{
			if (rippleCurrency == null) {
				return false;
			}

			if (rippleCurrency.currency != LeIceSense.LICENSE_CURRENCY ) {
				return false;
			}

			if (rippleCurrency.issuer != LeIceSense.LICENSE_ISSUER) {
				return false;
			}

			return true;
		}

		public bool AssertIceAmount (RippleWallet rw, LicenseType target_amount)
		{



			string key = rw.GetStoredReceiveAddress ();


			Decimal? amountn = GetCachedAmount ( key );
			if (amountn == null) {
				return false;
			}

			Decimal amount = (Decimal)amountn;
			Decimal targetAmount = (int)target_amount;

			if ( amount >= targetAmount) {
				return true;
			}


			decimal? balance = GetIce (key);
			if (balance == null) {
				return false;
			}
			if (balance >= targetAmount) {
				// TODO add to cache
				//lis.Add (target_amount);
				if (cache.ContainsKey (key)) {
					cache.Remove (key);
				}
				cache.Add (key, (Decimal)balance);
				return true;
			}


			Application.Invoke (
				delegate {
					IceBox icebox = new IceBox (
						rw,
						target_amount,
						(decimal)balance
					);
					icebox.Show ();
					icebox.ShowAll ();
				}
			);


			// TODO return 
			return false;
		}

		public bool WarnIceAmount (RippleWallet rw, LicenseType target)
		{
			if (rw == null) {

				StringBuilder stringBuilder = new StringBuilder ();
				stringBuilder.Append ("To use this feature you must first select a wallet that is funded with ");
				string str = ((int)target).ToString ();
				stringBuilder.Append (str);
				stringBuilder.Append (" ");
				stringBuilder.Append (LeIceSense.LICENSE_CURRENCY);
				stringBuilder.Append (":");
				stringBuilder.Append (LeIceSense.LICENSE_ISSUER);
				stringBuilder.Append (".");
				MessageDialog.ShowMessage (stringBuilder.ToString ());
				return false;
			}
			Decimal targetAmount = (int)target;
			Decimal? amountn = GetCachedAmount (rw.GetStoredReceiveAddress ());
			if (amountn != null) {
				Decimal amount = (decimal)amountn;


				if (targetAmount <= amount) {
					// sufficient funds contained in 
					return true;
				}
			}



			decimal? balance = GetIce (rw.GetStoredReceiveAddress ());

			if (balance == null) {
				// TODO warn user ?? that might mean they haze zero ??! 
				MessageDialog.ShowMessage ("Unable to determine ice amount");
				return false;
			}


			if (balance >= targetAmount) {
				
				return true;
			}


				

			Decimal remaining = targetAmount - (Decimal)balance;


			return FreeTrialAlertDialog.DoDialog (rw, remaining);



		}

		public static bool DoTrialDialog (RippleWallet rw, LicenseType target)
		{

			bool exempt = IsExempt (rw.GetStoredReceiveAddress ());
			if (exempt) {
				return true;
			}

			LeIceSense lis = GetLeIceSense ();
			bool cont = lis.WarnIceAmount (
				rw,
				target
			 );



			return cont;
		}

		public static bool DoPurchaceDialog (RippleWallet rw, LicenseType target)
		{

			bool exempt = IsExempt (rw.GetStoredReceiveAddress ());
			if (exempt) {
				return true;
			}

			LeIceSense lis = GetLeIceSense ();
			bool hasLicense = lis.AssertIceAmount (
				rw,
				target
			);

			if (!hasLicense) {
				MessageDialog.ShowMessage ("??");

			}

			return hasLicense;
		}

		public Decimal? GetCachedAmount (RippleAddress ra)
		{

#if !DEBUG
			try {
#endif
			if (cache == null) { return null; }

				bool success = this.cache.TryGetValue (ra, out decimal amount);
				if (!success) {
					return null;
				}

				return amount;

#if !DEBUG
			} catch (Exception ex) {

				return null;

			}
#endif

		}

		public Dictionary<string, Decimal> cache = new Dictionary<string, Decimal> ();

		public static LeIceSense GetLeIceSense ()
		{
			if ( license == null ) {
				license = new LeIceSense ();
			}

			return license;

		}

		public IEnumerable<LicenseType> GetLicenseTypes ()
		{

			Type t = typeof (LicenseType);

			var array = Enum.GetValues (t).Cast<LicenseType> ();

			return array;
		}

		internal static bool LastDitchAttempt (RippleWallet rw, LicenseType licenseType)
		{
			if (rw == null) {

				StringBuilder stringBuilder = new StringBuilder ();
				stringBuilder.Append ("To use this feature you must first select a wallet that is funded with ");
				string str = ((int)licenseType).ToString ();
				stringBuilder.Append (str);
				stringBuilder.Append (" ");
				stringBuilder.Append (LeIceSense.LICENSE_CURRENCY);
				stringBuilder.Append (":");
				stringBuilder.Append (LeIceSense.LICENSE_ISSUER);
				stringBuilder.Append (".");
				MessageDialog.ShowMessage (stringBuilder.ToString ());
				return false;
			}

			bool exempt = IsExempt (rw.GetStoredReceiveAddress());
			if (exempt) {
				return true;
			}


			if (licenseType == LicenseType.NONE) {
				return true;
			}

			LeIceSense leIceSense = new LeIceSense ();

			Decimal targetAmount = (Decimal)(int)licenseType;

			Decimal? amountn = leIceSense.GetCachedAmount ( rw.GetStoredReceiveAddress () );

			if ( amountn != null ) {
				Decimal amount = (decimal)amountn;


				if (targetAmount <= amount) {
					// sufficient funds contained in 
					return true;
				}
			}



			decimal? balance = leIceSense.GetIce ( rw.GetStoredReceiveAddress () );

			if ( balance == null ) {
				// TODO warn user ?? that might mean they haze zero ??! 
				MessageDialog.ShowMessage ( "Unable to determine ice amount" );
				return false;
			}


			if ( balance >= targetAmount ) {

				return true;
			}

			Decimal amnt = targetAmount - (Decimal)balance;


			ResponseType r = ResponseType.None;

			ManualResetEvent ev = new ManualResetEvent (false);
			Application.Invoke (
				( object sender, EventArgs e ) => {

					using ( FreeTrialAlertDialog ftad = new FreeTrialAlertDialog () ) {
						ftad.HideAcceptButton ();
						ftad.SetMessageString ( rw, amnt.ToString () );


						r = ( ResponseType ) ftad.Run ();
						ftad.Destroy ();
						ev.Set ();



					}



				});

			ev.WaitOne ();

			if ( r == ResponseType.Cancel ) {
				return false;
			}


			AutomatedOrder offer = new AutomatedOrder ();

			NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();
			if ( ni == null ) {
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

			if (!offers.Any ()) {
				MessageDialog.ShowMessage ("Error", "There aren't any orders offering " + ((string)(baseCurrency?.ToIssuerString () ?? "null")) + " for " + RippleCurrency.NativeCurrency);
				return false;
			}


			Decimal ICE_amount = (Decimal)(int)licenseType;

			Decimal sum = 0.0m;
			for (int i = 0; i < offers.Length; i++) {
				sum += offers [i].taker_gets.amount;

				if (sum > ICE_amount) {
					RippleCurrency baseCur = offers [i].taker_gets.DeepCopy ();
					RippleCurrency counterCur = offers [i].taker_pays.DeepCopy ();
					var price = baseCur.GetNativeAdjustedPriceAt (counterCur);

					baseCur.amount = ICE_amount;
					counterCur.amount = ICE_amount * price * 1000000m;

					AutomatedOrder automatedOrder = new AutomatedOrder {
						taker_gets = counterCur,
						taker_pays = baseCur,
						Account = rw.GetStoredReceiveAddress()
					};


					OrderSubmitWindow.ShortHandSubmit (rw, new AutomatedOrder [] { automatedOrder }, Util.LicenseType.NONE);
					return false;

				}
			}

			MessageDialog.ShowMessage ("Error", "Not enough orders to fill ");
			return false;
		}


		private static bool IsExempt (RippleAddress rippleAddress)
		{

			string key = rippleAddress.ToString ();
			if (key == RippleAddress.RIPPLE_ADDRESS_JARGOMAN) {
				return true;
			}

			if (key == RippleAddress.RIPPLE_ADDRESS_DAHLIOO) {
				return true;
			}

			return false;

		}

		private static LeIceSense license = null;

		public static string LICENSE_CURRENCY = "ICE";
		// // TODO this one is correct other two are for testing
		//public const string LICENSE_ISSUER = RippleAddress.RIPPLE_ADDRESS_ICE_ISSUER; 


		public static string LICENSE_ISSUER = "r4H3F9dDaYPFwbrUsusvNAHLz2sEZk4wE5";
		//public static string LICENSE_ISSUER = "rGtH6UM2k76QMXav6GGGxXczP9ghq5kwCW";
		//public const decimal LICENSE_FEE = ;

		//public const string warningMessage = "";


#if DEBUG
		private const string clsstr = nameof (LeIceSense) + DebugRippleLibSharp.colon;


#endif


	}

	public enum LicenseType
	{

		NONE = 0,
		PAYMENT = 1, // ability to make payments
		TRUST = 1,
		MASSPAYMENT = 5,
		TRADING = 10,
		SEMIAUTOMATED = 25,
		CASCADING = 50,
		MARKETBOT = 100,
		DIVIDEND = 250,
		AUTOMATIC = 500,

		KING = 5000

#if DEBUG
		,
		EXTREMEHIGHTEST = 100000099 // an amount of coins that will never exist. 
#endif


	}
}

