using System;
using Org.BouncyCastle.Math;

namespace RippleLibSharp.Util
{
	public static class TimeHelper
	{
		

		public static DateTime ConvertRippleTimeToUnix ( uint rippleTime ) {


			//return new DateTime ((long)rippleTime);

			BigInteger bigint = new BigInteger ( rippleTime.ToString () );
			BigInteger change = BigInteger.ValueOf ( 946684800L );
			bigint = bigint.Add (change);

		
			long unixTimeStamp = bigint.LongValue;

			System.DateTime dtDateTime = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddSeconds( unixTimeStamp ).ToLocalTime();
			return dtDateTime;





			//dateTime = dateTime.ToUniversalTime ();


			
		}
	}
}

