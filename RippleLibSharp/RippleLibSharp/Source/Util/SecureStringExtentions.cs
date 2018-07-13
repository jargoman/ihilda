using System;
using System.Security;

namespace RippleLibSharp.Util
{
	public static class SecureStringExtentions
	{
		
		public static SecureString ToSecureString(this char[] _self)
		{
			SecureString knox = new SecureString();
			foreach (char c in _self)
			{
				knox.AppendChar(c);
			}
			return knox;
		}


		public static SecureString ToSecureString(this string _self)
		{
			SecureString knox = new SecureString();
			char[] chars = _self.ToCharArray();
			foreach (char c in chars)
			{
				knox.AppendChar(c);
			}
			return knox;
		}
	}
}

