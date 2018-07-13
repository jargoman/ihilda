using System;

namespace RippleLibSharp.Keys
{
	public class RipplePublicGeneratorAddress : RippleIdentifier
	{
		public RipplePublicGeneratorAddress (byte[] payloadBytes) : base (payloadBytes, 41)
		{
		}
	}
}

