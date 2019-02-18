using System;
using RippleLibSharp.Keys;

namespace IhildaWallet
{
	public interface IEncrypt
	{

		//byte[] encrypt ( byte[] message, String password );

		//byte[] decrypt ( byte [] cipher, String password );

		byte[] Encrypt ( RippleIdentifier identifier, byte[] salt );

		byte[] Decrypt ( byte [] cipher, byte[] salt, RippleAddress ra );

		string Password {
			get;
			set;
		}

		String Name { get; /*set;*/}
	}
}

