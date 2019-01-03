using System;
using System.Security.Cryptography;
using System.Security;

namespace RippleLibSharp.Util
{
	public class ByteProtector
	{
		/*
		public ByteProtector ()
		{
		}
		*/


		public static byte [] Protect( byte [] data )
		{
			try
			{
                // Encrypt the data using DataProtectionScope.CurrentUser. The result can be decrypted
                //  only by the same current user.
                //return ProtectedData.Protect( data, s_aditionalEntropy, DataProtectionScope.CurrentUser );
                //ProtectedMemory.Protect( data, MemoryProtectionScope.SameProcess );

                throw new NotImplementedException("Byte protection (enrypting byte arrays) is not supported");
			} 
			catch (CryptographicException e)
			{
				Console.WriteLine("Data was not encrypted. An error occurred.");
				Console.WriteLine(e.ToString());

				//throw e;
				//return null;
			}

			return data;
		}


		public static byte [] Unprotect( byte [] data )
		{
			try
			{
                //Decrypt the data using DataProtectionScope.CurrentUser.
                //return ProtectedData.Unprotect( data, s_aditionalEntropy, DataProtectionScope.CurrentUser );

                //ProtectedMemory.Protect( data, MemoryProtectionScope.SameProcess );

                throw new NotImplementedException("Byte protection (decrypting byte arrays) is not supported");
			} 
			catch (CryptographicException e)
			{
				Console.WriteLine("Data was not decrypted. An error occurred.");
				Console.WriteLine(e.ToString());

				//throw e;
				//return null;
			}

			return data;
		}
	}
}

