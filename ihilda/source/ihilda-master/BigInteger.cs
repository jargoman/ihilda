using System;


namespace IhildaWallet
{
	public class BigInteger : Org.BouncyCastle.Math.BigInteger
	{
		public BigInteger (String sval) : base (sval)
		{
				
		}

		public BigInteger (String sval, int rdx) : base (sval, rdx)
		{
		}

		public BigInteger (byte[] bval) : base (bval)
		{
		}

		public BigInteger (byte[] bval, int offset, int length) : base (bval, offset, length)
		{
		}

 		public BigInteger (int sign, byte[] mag) : base (sign, mag)
		{
		}

		public BigInteger (int sign, byte[] bytes, int offset, int length) : base (sign,bytes,offset,length)
		{
		}

		public BigInteger (int numBits, Random rnd) : base (numBits, rnd)
		{
		}

		public BigInteger (int bitLength, int certainty, Random rnd) : base (bitLength, certainty, rnd)
		{
		}

		public BigInteger (UInt64 value) : base (1, getBigEndian(value))
		{
		}

		public BigInteger (UInt32 value) : base (1, getBigEndian(value)) {}

		public BigInteger (UInt16 value) : base (1, getBigEndian(value)) {}

		public BigInteger (byte value) : base (1, new byte[] {value}) {}

		public BigInteger (Int64 value) : base (getBigEndian(value))
		{
		}

		public BigInteger (Int32 value) : base (getBigEndian(value)) {
		}

		public BigInteger (Int16 value) : base (getBigEndian(value)) {} 

		public BigInteger (sbyte value) : base (new byte[] {(byte)value}) {}

		public BigInteger (Org.BouncyCastle.Math.BigInteger value) : base (value.ToByteArray()) {} 

		public static byte[] getBigEndian (Int16 value)
		{
			byte[] bytes = BitConverter.GetBytes (value);
			if (BitConverter.IsLittleEndian) {
				Array.Reverse(bytes);
			}

			return bytes;

		}


		public static byte[] getBigEndian (Int32 value)
		{
			byte[] bytes = BitConverter.GetBytes (value);
			if (BitConverter.IsLittleEndian) {
				Array.Reverse(bytes);
			}

			return bytes;

		}


		public static byte[] getBigEndian (Int64 value)
		{
			byte[] bytes = BitConverter.GetBytes (value);
			if (BitConverter.IsLittleEndian) {
				Array.Reverse(bytes);
			}

			return bytes;

		}

		public static byte[] getBigEndian (UInt64 value)
		{
			byte[] bytes = BitConverter.GetBytes (value);
			if (BitConverter.IsLittleEndian) {
				Array.Reverse(bytes);
			}

			return bytes;

		}

		public static byte[] getBigEndian (UInt32 value)
		{
			byte[] bytes = BitConverter.GetBytes (value);
			if (BitConverter.IsLittleEndian) {
				Array.Reverse(bytes);
			}

			return bytes;
		}

		public static byte[] getBigEndian (UInt16 value)
		{
			byte[] bytes = BitConverter.GetBytes (value);
			if (BitConverter.IsLittleEndian) {
				Array.Reverse(bytes);
			}

			return bytes;
		}

/*
			#region Operators
 
			public static bool operator ==(BigInteger left, BigInteger right)
			{
				return left.Equals(right);
			}
 
			public static bool operator !=(BigInteger left, BigInteger right)
			{
				return !left.Equals(right);
			}
 
			public static bool operator >(BigInteger left, BigInteger right)
			{
				return (left.CompareTo(right) > 0);
			}
 
			public static bool operator >=(BigInteger left, BigInteger right)
			{
				return (left.CompareTo(right) >= 0);
			}

			public static bool operator <(BigInteger left, BigInteger right)
			{
				return (left.CompareTo(right) < 0);
			}
 
			public static bool operator <=(BigInteger left, BigInteger right)
			{
				return (left.CompareTo(right) <= 0);
			}
 
			public static bool operator ==(BigInteger left, decimal right)
			{
				return left.Equals(right);
			}
 
			public static bool operator !=(BigInteger left, decimal right)
			{
				return !left.Equals(right);
			}
 
			public static bool operator >(BigInteger left, decimal right)
			{
				return (left.CompareTo(right) > 0);
			}
 
			public static bool operator >=(BigInteger left, decimal right)
			{
				return (left.CompareTo(right) >= 0);
			}
 
			public static bool operator <(BigInteger left, decimal right)
			{
				return (left.CompareTo(right) < 0);
			}
 
			public static bool operator <=(BigInteger left, decimal right)
			{
				return (left.CompareTo(right) <= 0);
			}
 
			public static bool operator ==(decimal left, BigInteger right)
			{
				return left.Equals(right);
			}
 
			public static bool operator !=(decimal left, BigInteger right)
			{
				return !left.Equals(right);
			}
 
			public static bool operator >(decimal left, BigInteger right)
			{
				return (left.CompareTo(right) > 0);
			}
 
			public static bool operator >=(decimal left, BigInteger right)
			{
				return (left.CompareTo(right) >= 0);
			}
 
			public static bool operator <(decimal left, BigInteger right)
			{
				return (left.CompareTo(right) < 0);
			}
 
			public static bool operator <=(decimal left, BigInteger right)
			{
				return (left.CompareTo(right) <= 0);
			}
 
		#endregion	
*/
		
		public static implicit operator BigInteger(byte value) { return new BigInteger(value); }
		public static implicit operator BigInteger(sbyte value) { return new BigInteger(value); }
		public static implicit operator BigInteger(short value) { return new BigInteger(value); }
		public static implicit operator BigInteger(int value) { return new BigInteger(value); }
		public static implicit operator BigInteger(long value) { return new BigInteger(value); }
		public static implicit operator BigInteger(ushort value) { return new BigInteger(value); }
		public static implicit operator BigInteger(uint value) { return new BigInteger(value); }
		public static implicit operator BigInteger(ulong value) { return new BigInteger(value); }

		//public static implicit operator BigInteger(float value) { return new BigInteger(value); }
		//public static implicit operator BigInteger(double value) { return new BigInteger(value); }
		//public static implicit operator BigInteger(decimal value) { return new BigInteger(value); }
		//public static implicit operator BigInteger(Org.BouncyCastle.Math.BigInteger value) { return new BigInteger(value); }
 		
 		public static explicit operator BigInteger (Org.BouncyCastle.Math.BigInteger value) { return new BigInteger(value); }
 		

		
	}
}

