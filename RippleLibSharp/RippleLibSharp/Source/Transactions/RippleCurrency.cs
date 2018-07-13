using System;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Keys;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using RippleLibSharp.Util;


namespace RippleLibSharp.Transactions
{
	public class RippleCurrency
	{
#pragma warning disable IDE1006 // Naming Styles
		public Decimal amount {

			get {
				if (IsNative) {
					return /*_amount*/ Math.Round (_amount);
				}

				return /*_amount*/Math.Round (_amount, 15);

				//return _amount; 
			}
			set { _amount = value; }
		}




		private Decimal _amount {
			get;
			set;
		}


		//public RippleAddress issuer = null;
		public string issuer {
			get;
			set;
		}

		public String currency {
			get;
			set;
		}

		public object value {
			get {


				return amount;
			}
			set {

				if (value is decimal) {
					amount = (decimal)value;
				} else if (value is string) {

					amount = 0; // SET IT TO ZERO INCASE SOMEONE REUSES THE SAME OBJECT, defaulting to zero is expected behaviour and safest
					decimal? deci = RippleCurrency.ParseDecimal ((string)value);
					if (deci != null) {
						amount = (decimal)deci;
					}


				}
			}

		}
#pragma warning restore IDE1006 // Naming Styles
		public static readonly int MIN_SCALE = -96;
		public static readonly int MAX_SCALE = 80;

		public static readonly ulong MAX_MANTISSA = 9999999999999999UL;
		public static readonly ulong MIN_MANTISSA = 1000000000000000UL;

		/*public DenominatedIssuedCurrency (String nativeAmount) {
			
		}*/

		public RippleCurrency ()
		{

		}


		public RippleCurrency (String nativeAmount)
		{

			amount = 0; // SET IT TO ZERO INCASE SOMEONE REUSES THE SAME OBJECT, defaulting to zero is expected behaviour and safest

			decimal? deci = RippleCurrency.ParseDecimal (nativeAmount);
			if (deci != null) {
				amount = (decimal)deci;
			}

			this.SetNative ();
		}

		/*
		public RippleCurrency (double amount, RippleAddress issuer, String currencyStr) : this ((decimal)amount,issuer,currencyStr) {
			

		}
		*/

		public RippleCurrency (Decimal amount, RippleAddress issuer, String currencyStr)
		{
			this.value = amount;
			this.issuer = issuer;

			/*
			if (currencyStr == null || currencyStr.Trim().Equals("")) {
				throw new FormatException("IOU amount must have a currency value");
			}

			if (currencyStr.ToLower().Trim().Equals(RippleCurrency.nativeCurrency)) {
				throw new FormatException(RippleCurrency.nativeCurrency + " is not a valid Currency value");
			}
			*/

			this.currency = currencyStr;
			this.IsNative = false;
		}

		/*
		public RippleCurrency (double nativeAmount) : this ((decimal)nativeAmount) {

		}
		*/

		public RippleCurrency (Decimal nativeAmount)
		{
			this.value = nativeAmount;
			//this.isNative = true;
			this.SetNative ();

		}

		public override int GetHashCode ()
		{
			// TODO implent hashcode
			return base.GetHashCode ();
		}

		public override bool Equals (Object obj)
		{
			if (obj == null) {
				return false;
			}


			if (!(obj is RippleCurrency dino)) {
				return false;
			}

			return this.Equals (dino);
		}

		public bool Equals (RippleCurrency denarious)
		{
			
			if ((object)denarious == null) { // we cast to object because == COULD BE OVERLOADED !!!
				return false;
			}

			if (!(this.IsSameCurrency (denarious))) {
				return false;
			}

			if (!(this.IsSameIssuer (denarious))) {
				return false;
			}

			return (this.value == denarious.value);
		}

		public bool IsSameCurrency (RippleCurrency dino)
		{
			if (dino == null) {
				return false;
			}

			if (this.IsNative) {
				return dino.IsNative;
			}

			// todo null pointer exception?
			if (this.currency == null) {
				return (dino.currency == null);
			}

			return this.currency.Equals (dino.currency);
		}

		public bool IsSameIssuer (RippleCurrency dino)
		{
			if (dino == null) {
				return false;
			}

			if (this.IsNative) {
				return dino.IsNative;
			}

			if (this.issuer == null) {
				return (dino.issuer == null);
			}

			return this.issuer.Equals (dino.issuer);
		}

		public RippleCurrency (int nativeAmount)
		{
			this.value = new decimal (nativeAmount);
			this.SetNative ();
		}

		public RippleCurrency (long nativeAmount)
		{
			this.value = new decimal (nativeAmount);
			this.SetNative ();
		}


		/*
		public Decimal getPriceAt (RippleCurrency counter)
		{
			
			return this.amount / counter.amount;
		}
		*/


		public Decimal GetNativeAdjustedPriceAt (RippleCurrency counter)
		{
			if (counter.amount == Decimal.Zero) {
				return 0;
			}

			if (this.amount == Decimal.Zero) {
				return 0;
			}
			if (IsNative) {
				if (counter.IsNative) {
					throw new InvalidOperationException ("using " + RippleCurrency.NativeCurrency + " as both base and counter makes no sense??");
				}

				return counter.amount / (this.amount / 1000000);
			}

			if (counter.IsNative) {
				return (counter.amount / 1000000) / this.amount;
			}

			return counter.amount / this.amount;

		}

		public Decimal GetNativeAdjustedCostAt (RippleCurrency counter)
		{

			if (counter.amount == Decimal.Zero) {
				return 0;
			}

			if (this.amount == Decimal.Zero) {
				return 0;
			}

			if (IsNative) {
				if (counter.IsNative) {
					throw new InvalidOperationException ("using " + RippleCurrency.NativeCurrency + " as both base and counter makes no sense??");
				}

				return (this.amount / 1000000) / counter.amount;
			}

			if (counter.IsNative) {
				return this.amount / (counter.amount / 1000000);
			}

			return this.amount / counter.amount;

		}




		public Boolean IsNative {
			get;
			set;
		}

		private void SetNative ()
		{
			IsNative = true;
			currency = NativeCurrency;
		}



		public Boolean IsNegative ()
		{
			if ((decimal)value < Decimal.Zero) {
				return true;
			}
			return false;
		}

		public RippleCurrency DeepCopy ()
		{
			RippleCurrency dic;

			if (this.IsNative) {
				dic = new RippleCurrency (this.amount);
			} else {
				dic = new RippleCurrency (this.amount, new RippleAddress (issuer), this.currency);
			}

			return dic;
		}

		public override String ToString ()
		{
			if (this.IsNative) {
				if (amount < 999m && amount > -999m) {
					return value.ToString () + " " + RippleCurrency.NativePip;
				}
				return (amount / 1000000m).ToString () + " " + RippleCurrency.NativeCurrency;
			}

			StringBuilder sb = new StringBuilder ();
			sb.Append (value.ToString ());
			sb.Append (" ");
			sb.Append (currency);
			sb.Append (" ");
			if ((issuer != null)) {
				sb.Append (issuer);
			}

			return sb.ToString ();
		}

		public Object GetAnonObjectWithoutAmount ()
		{
			Object anon = null;

			if (this.IsNative) {
				anon = new {
					currency = RippleCurrency.NativeCurrency
				};
			} else {

				String ish = this.issuer;
				anon = new {
					currency,
					issuer = ish
				};
			}

			return anon;
		}

		public static Decimal? ParseDecimal (String str /*, String messageVar , bool warnuser */)
		{



			Decimal? dec = null;
			try {
				dec = Decimal.Parse (str, System.Globalization.NumberStyles.Float); //Convert.ToDecimal(str);
				return dec;
			}

#pragma warning disable 0168
			catch (FormatException ex) {
#pragma warning restore 0168


				//String m = messageVar + " is fomated incorrectly./n" + messageVar + "must be a valid decimal number\n";
				//if (warnuser) {
				//MessageDialog.showMessage ( m);
				//}

				//else {
#if DEBUG
				if (DebugRippleLibSharp.RippleCurrency) {
					//Logging.writeLog(m);
					Logging.WriteLog (ex.Message);
				}
#endif
				//}
				// todo debug
				return null;
			}

#pragma warning disable 0168
			catch (OverflowException ex) {
#pragma warning restore 0168

				//String m = messageVar + " is greater than a Decimal? No one's got that much money\n";
				//if (warnuser) {
				//MessageDialog.showMessage (m);
				//}

				//else {
#if DEBUG
				if (DebugRippleLibSharp.RippleCurrency) {
					//Logging.writeLog(m);
					Logging.WriteLog (ex.Message);
				}
#endif
				//}
				// todo debug
				return null;
			}

#pragma warning disable 0168
			catch (Exception ex) {
#pragma warning restore 0168

				// todo debug
				//MessageDialog.showMessage (messageVar + " is fomated incorrectly.\n It must be a valid decimal number\n");
				//String m = "Unknown exception parsing " + messageVar + " to decimal number\n" + ex.Message;
				//if (warnuser) {
				//MessageDialog.showMessage (m);
				//}

				//else {
#if DEBUG
				if (DebugRippleLibSharp.RippleCurrency) {
					//Logging.writeLog(m);
					Logging.WriteLog (ex.Message);
				}
#endif
				//}
				return null;
			}


			//return dec;  // unreachable code
		}

		public static UInt64? ParseUInt64 (String str /*, String messageVar, bool warnuser*/)
		{
			try {

				UInt64? amountl = Convert.ToUInt64 (str);
				return amountl;
			}

#pragma warning disable 0168
			catch (FormatException ex) {

				//if (warnuser) {
				//MessageDialog.showMessage (messageVar + " is fomated incorrectly for sending drops.\n It must be a valid integer\n");
				//}
				return null;

			} catch (OverflowException ex) {
				//if (warnuser) {
				//MessageDialog.showMessage (messageVar + " is greater than an unsignd long. No one's got that much money\n");
				//}
				return null;
			} catch (Exception ex) {
				//if (warnuser) {
				//MessageDialog.showMessage (messageVar + "Unknown error formatting " + messageVar + "\n");
				//}
				return null;
			}
#pragma warning restore 0168

		}

		public static UInt32? ParseUInt32 (String str)
		{
			try {

				UInt32? amountl = Convert.ToUInt32 (str);
				return amountl;
			}

#pragma warning disable 0168
			catch (FormatException ex) {
				//if (warnuser) {
				//MessageDialog.showMessage (messageVar + " is fomated incorrectly.\n It must be a valid unsigned 32 bit integer\n");
				//}
				return null;

			} catch (OverflowException ex) {
				//if (warnuser) {
				//MessageDialog.showMessage (messageVar + " is greater than an unsigned int 32\n");
				//}
				return null;
			} catch (Exception ex) {
				//if (warnuser) {
				//MessageDialog.showMessage ("Unknown error formatting " + messageVar + "\n");
				//}
				return null;
			}

#pragma warning restore 0168
		}

		public static Int32? ParseInt32 (String str /*, String messageVar, bool warnuser*/)
		{
			try {

				Int32? amountl = Convert.ToInt32 (str);
				return amountl;
			}

#pragma warning disable 0168
			catch (FormatException ex) {
				//if (warnuser) {
				//MessageDialog.showMessage (messageVar + " is fomated incorrectly.\n It must be a valid 32 bit integer\n");
				//}
				return null;

			} catch (OverflowException ex) {
				//if (warnuser) {
				//MessageDialog.showMessage (messageVar + " is greater than a 32 bit integer\n");
				//}
				return null;
			} catch (Exception ex) {
				//if (warnuser) {
				//MessageDialog.showMessage ("Unknown error formatting " + messageVar + "\n");
				//}
				return null;
			}
#pragma warning restore 0168
		}



		public static RippleCurrency FromJsonString (String str)
		{

			Logging.WriteLog ("str = " + str);

			// Best check it's json to avoid freezing
			//if (str.Contains ("currency") && str.StartsWith ("{") && str.EndsWith ("}")) {
			if (str.Contains ("currency") && str.StartsWith ("{", StringComparison.CurrentCulture) && str.EndsWith ("}", StringComparison.CurrentCulture)) {
				dynamic d = DynamicJson.Parse (str);

				if (d.IsDefined ("currency")) {
					//Logging.write("huh??");
					String currency = d.currency;
					String issuer = d.issuer;
					String value = d.value;

					Logging.WriteLog (currency + issuer + value);

					RippleAddress rad = new RippleAddress (issuer);

					Decimal? decim = ParseDecimal (value);

					if (decim == null) {
						return null;
					}

					return new RippleCurrency (((Decimal)decim), rad, currency);
				}
			}

				 //try {

				 //Logging.write("huh?");


				 else {
				//Logging.write("uy");
				//String am = d;
				Decimal? decim = ParseDecimal (str);
				if (decim == null) {
					return null;
				}

				return new RippleCurrency ((Decimal)decim);


			}


			//} catch (Exception e) {

			return null;
			//}
		}

		public string ToJsonString ()
		{

			if (this.IsNative) {
				return "\"" + this.amount.ToString () + "\"";
			}
			return "{\"currency\": \"" + this.currency +
				"\",\"issuer\": \"" + this.issuer +
				"\",\"value\": \"" + this.amount.ToString () + "\"}";
		}

		public static RippleCurrency FromDynamic (dynamic d)
		{
#if DEBUG
			string method_sig = clsstr + "DenominatedIssuedCurrency (dynamic d) : ";
			if (DebugRippleLibSharp.RippleCurrency) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			RippleCurrency dic = null;
			RippleAddress ra = null;
			Decimal? dec = null;

			string cur = null;
			string amt = null;
			string iss = null;

			try {
				if (d is string) amt = d as string;

				if (amt != null) {
					try {

						dec = ParseDecimal (amt);

						dic = new RippleCurrency ((decimal)dec);
						return dic;
					}

#pragma warning disable 0168
					catch (Exception e) {
#pragma warning restore 0168

#if DEBUG
						if (DebugRippleLibSharp.RippleCurrency) {
							Logging.WriteLog (method_sig + "exception thrown parcing " + amt + "\n");
							Logging.WriteLog (e.Message);
						}
#endif
						return null;
					}
				}

				if (d.IsDefined ("currency")) {
#if DEBUG
					if (DebugRippleLibSharp.RippleCurrency) {
						Logging.WriteLog (method_sig + "currency is defined\n");
					}
#endif
					cur = d.currency;
					if (cur == null) {
#if DEBUG
						if (DebugRippleLibSharp.RippleCurrency) {
							Logging.WriteLog (method_sig + "cur == null\n");
						}
#endif
						return null;
					}
					if (d.IsDefined ("value")) {
#if DEBUG
						if (DebugRippleLibSharp.RippleCurrency) {
							Logging.WriteLog (method_sig + "value is defined\n");
						}
#endif
						amt = d.value;

						dec = RippleCurrency.ParseDecimal (amt);
						if (dec == null) {
							return null;
						}
					}
					if (d.IsDefined ("issuer")) {
						iss = d.issuer;
						try {
							ra = new RippleAddress (iss);
						}

#pragma warning disable 0168
						catch (Exception e) {
#pragma warning restore 0168

							ra = null;
#if DEBUG
							if (DebugRippleLibSharp.RippleCurrency) {
								Logging.WriteLog (method_sig + "exception thrown parcing \n");
								Logging.WriteLog (e.Message);
							}
#endif
							// not returning

						}
					}



					decimal deci = (decimal)dec;
					dic = new RippleCurrency (deci, ra, cur);

				} else {
#if DEBUG
					if (DebugRippleLibSharp.RippleCurrency) {
						Logging.WriteLog (method_sig + "currency is not defined\n");
					}
#endif

				}

			}

#pragma warning disable 0168
			catch (Exception e) {
#pragma warning restore 0168

#if DEBUG
				if (DebugRippleLibSharp.RippleCurrency) {
					Logging.WriteLog (method_sig + "exception handling dynamic property \n");
					Logging.WriteLog (e.Message);
				}
#endif
			}
			return null;
		}

		public string ToIssuerString ()
		{
			/* don't use debugs assert string */
			if (IsNative) {
				return NativeCurrency;
			}

			return string.Format ("{0}:{1}", currency ?? "", issuer ?? "");
		}

		private static bool allowExtraSymbols = true;
		public static RippleCurrency FromIssuerCode (String parseme)
		{
#if DEBUG
			String method_sig = clsstr + nameof(FromIssuerCode) + DebugRippleLibSharp.left_parentheses + nameof (String) + DebugRippleLibSharp.space_char + nameof (parseme) + DebugRippleLibSharp.equals + DebugRippleLibSharp.ToAssertString (parseme) + DebugRippleLibSharp.right_parentheses;
#endif
			if (parseme == null) {
				return null;
			}

			String [] code = allowExtraSymbols ? parseme.Split ('/') : parseme.Split ('/', '\\', ':', '|', '-', '&', '+');
			if (code == null) {
				return null;
			}

			if (code.Length != 2) {
				// todo not a valid trade pair
#if DEBUG
				if (DebugRippleLibSharp.TradePair) {
					Logging.WriteLog (method_sig + "pair.Length != 2, user entered incorrect trade pair value");
				}
#endif
				return null;
			}

			if (code [0] == null) {
				// Todo... return null instead
				code [0] = "";
			}

			if (code [1] == null) {
				code [1] = "";
			}

			RippleAddress rip;
			try {
				rip = new RippleAddress (code [1]);
			}

#pragma warning disable 0168
			catch (Exception e) {
#pragma warning restore 0168

				return null; // todo user entered invalid issuer
			}

			RippleCurrency dic = new RippleCurrency (Decimal.Zero, rip, code [0]);
			return dic;

		}

		public static implicit operator string (RippleCurrency d)
		{
#if DEBUG
			string method_sig = clsstr + "implicit operator string(DenominatedIssuedCurrency d = " + DebugRippleLibSharp.ToAssertString (d) + " ) : ";
			if (DebugRippleLibSharp.RippleCurrency) {
				Logging.WriteLog (method_sig);
			}
#endif
			//TODO maybe delete the null check or have it return null non string "without quotes". It's useful for debugging at the moment

			if (d == null) {
				return "null";
			}


			return d.ToString ();
		}
		public static implicit operator RippleCurrency (string d)
		{
#if DEBUG
			string method_sig = clsstr + "implicit operator DenominatedIssuedCurrency(string d = " + DebugRippleLibSharp.ToAssertString (d) + ") : ";
			if (DebugRippleLibSharp.RippleCurrency) {
				Logging.WriteLog (method_sig);
			}
#endif
			return new RippleCurrency (d);
		}

		/*
		 * 
		 * /// doesn't even make sense 

		public static explicit operator DenominatedIssuedCurrency(object d) {
			if (d is string) {
				string s = d as string;
				return new DenominatedIssuedCurrency(s);
			}

			//if (d is object) {
			DenominatedIssuedCurrency r = d as DenominatedIssuedCurrency;
			return r.deepCopy ();
			//}
		}*/

		private static bool CheckSameCurrencyType (RippleCurrency c1, RippleCurrency c2)
		{
			if (c1 == null || c2 == null) {
				return false;
			}

			bool same = true;
			if (c1.IsNative != c2.IsNative) {
				ArithmeticException ex = new ArithmeticException ();
				same = false;
				throw ex;

			}

			if (c1.issuer != c2.issuer) {
				Logging.WriteLog ("warning: you are adding two currencies ");
				same = false;
			}

			//
			return same;
		}

		public static RippleCurrency operator + (RippleCurrency c1, RippleCurrency c2)
		{
			if (c1 == null || c2 == null) {
				return null;
			}



			// TODO what to do if the issuers are not the same 
#if DEBUG

			bool same = CheckSameCurrencyType (c1, c2);

			if (!same) {
				Logging.WriteLog ("Warning: issuers " + c1.issuer + " and " + c2.issuer + " are not the same");
			}
#endif


			RippleCurrency r = c1.DeepCopy ();
			r.amount += c2.amount;

			return r;
		}

		public static RippleCurrency operator - (RippleCurrency c1, RippleCurrency c2)
		{
			if (c1 == null || c2 == null) {
				return null;
			}


#if DEBUG
			bool same = CheckSameCurrencyType (c1, c2);

			if (!same) {
				Logging.WriteLog ("Warning: issuers " + c1.issuer + " and " + c2.issuer + " are not the same");
			}
#endif


			RippleCurrency r = c1.DeepCopy ();
			r.amount -= c2.amount;
			return r;
		}

		public static RippleCurrency operator % (RippleCurrency c1, RippleCurrency c2)
		{
			if (c1 == null || c2 == null) {
				return null;
			}


#if DEBUG
			bool same = CheckSameCurrencyType (c1, c2);
			if (!same) {
				Logging.WriteLog ("Warning: issuers " + c1.issuer + " and " + c2.issuer + " are not the same");
			}
#endif
			RippleCurrency r = c1.DeepCopy ();
			r.amount %= c2.amount;
			return r;
		}

		public static RippleCurrency operator * (RippleCurrency c1, RippleCurrency c2)
		{
			if (c1 == null || c2 == null) {
				return null;
			}


#if DEBUG
			bool same = CheckSameCurrencyType (c1, c2);
			if (!same) {
				Logging.WriteLog ("Warning: issuers " + c1.issuer + " and " + c2.issuer + " are not the same");
			}
#endif
			RippleCurrency r = c1.DeepCopy ();
			r.amount *= c2.amount;
			return r;
		}

		public static RippleCurrency operator / (RippleCurrency c1, RippleCurrency c2)
		{
			if (c1 == null || c2 == null) {
				return null;
			}


#if DEBUG
			bool same = CheckSameCurrencyType (c1, c2);
			if (!same) {
				Logging.WriteLog ("Warning: issuers " + c1.issuer + " and " + c2.issuer + " are not the same");
			}
#endif
			RippleCurrency r = c1.DeepCopy ();
			r.amount /= c2.amount;
			return r;
		}

		public static RippleCurrency operator + (RippleCurrency c1, Decimal c2)
		{
			if (c1 == null) {
				return null;
			}

			RippleCurrency r = c1.DeepCopy ();

			r.amount += c2;

			return r;
		}

		public static RippleCurrency operator - (RippleCurrency c1, Decimal c2)
		{
			if (c1 == null) {
				return null;
			}

			RippleCurrency r = c1.DeepCopy ();

			r.amount -= c2;

			return r;
		}

		public static RippleCurrency operator % (RippleCurrency c1, Decimal c2)
		{
			if (c1 == null) {
				return null;
			}

			RippleCurrency r = c1.DeepCopy ();

			r.amount %= c2;

			return r;
		}

		public static RippleCurrency operator * (RippleCurrency c1, Decimal c2)
		{
			if (c1 == null) {
				return null;
			}

			RippleCurrency r = c1.DeepCopy ();

			r.amount *= c2;

			return r;
		}

		public static RippleCurrency operator / (RippleCurrency c1, Decimal c2)
		{
			if (c1 == null) {
				return null;
			}

			RippleCurrency r = c1.DeepCopy ();

			r.amount /= c2;

			return r;
		}



		public static RippleCurrency ParseCurrency (object cur, object iss)
		{


			RippleCurrency rc = null;

			if (!(cur is string currency)) {
				return null;
			}

			currency = currency.ToUpper ();

			if (currency.Equals (NativeCurrency)) {

				return new RippleCurrency (0);
			}

			RippleAddress ra = null;

			if (iss is string issuer) {
				//TODO no issuer support?
				try {
					ra = new RippleAddress (issuer);
				} catch (Exception e) {
#if DEBUG
					Logging.WriteLog (e.Message + e.StackTrace);
#endif

					ra = null;
				}


			}


			try {

				rc = new RippleCurrency (0.0m, ra, currency);

			} catch (Exception e) {

#if DEBUG
				Logging.WriteLog (e.Message + e.StackTrace);
#endif

				return null;
			}

			return rc;
		}


		public void UpdateBalance (string account, NetworkInterface networkInterface)
		{

			// How it's mother f'n done. 
			if (this.IsNative) {

				Task<Response<AccountInfoResult>> task = AccountInfo.GetResult (account, networkInterface);
				task.Wait ();
				Response<AccountInfoResult> response = task.Result;
				AccountInfoResult accountInfoResult = response.result;
				this.amount = accountInfoResult.GetNativeBalance ().amount;
			} else {
				RippleCurrency balance = AccountLines.GetBalanceForIssuer (this.currency, this.issuer, account, networkInterface);
				this.amount = balance.amount;
			}


		}

		// gets the amount of native rather than pips. 
		/*
		public Decimal getNormalizedAmount () {
			if (isNative) {

			} else {
				return this.
			}
		}
		*/

		public const string NativeCurrency = "XRP";
		public const string NativePip = "drops";
#if DEBUG
		private const string clsstr = nameof (RippleCurrency) + DebugRippleLibSharp.colon;
#endif
	}
}

