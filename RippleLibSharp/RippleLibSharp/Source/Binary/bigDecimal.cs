/*
 * This class was abandoned because it was becoming too time consuming. If reimplemented it would replace the Decimal amount in DenominatedCurrency
 * 
 * License : this class is fully permissive. Anyone is free to use this for any reason



using System;
//using System.Numerics;
using Org.BouncyCastle.Math;
using System.Data.Linq;

namespace RippleLibSharp
{
	public struct bigDecimal : IConvertible, IFormattable, IComparable, IComparable<bigDecimal>, IEquatable<bigDecimal>
	{
			public static readonly bigDecimal MinusOne = new bigDecimal(BigInteger.ValueOf(-1), 0);
			public static readonly bigDecimal Zero = new bigDecimal(BigInteger.Zero, 0);
			public static readonly bigDecimal One = new bigDecimal(BigInteger.One, 0);
 
			private readonly BigInteger _unscaledValue;
			private readonly int _scale;
 
			public bigDecimal(double value)
			: this((decimal)value) { }
 
			public bigDecimal(float value)
			: this((decimal)value) { }
 
			public bigDecimal(decimal value)
			{
				var bytes = FromDecimal(value);
 
				var unscaledValueBytes = new byte[12];
				Array.Copy(bytes, unscaledValueBytes, unscaledValueBytes.Length);

				// because decimal is little endian an
				Array.Reverse(unscaledValueBytes);
				
				var unscaledValue = new BigInteger(unscaledValueBytes);
				var scale = bytes[14];
 
				if (bytes[15] == 128)
				unscaledValue = unscaledValue.Multiply(BigInteger.ValueOf(-1));
 
				_unscaledValue = unscaledValue;
				_scale = scale;
			}
 
			public bigDecimal(int value)
			: this(BigInteger.ValueOf(value), 0) { }
 
			public bigDecimal(long value)
			: this(BigInteger.ValueOf(value), 0) { }
 
			public bigDecimal(uint value)
			: this(BigInteger.ValueOf(value), 0) { }
 
			public bigDecimal(ulong value)
			: this(new BigInteger (value.ToString()), 0) { }
 
			public bigDecimal(BigInteger unscaledValue, int scale)
			{
				_unscaledValue = unscaledValue;
				_scale = scale;
			}
 
			public bigDecimal ( byte[] value )
			{
				byte[] number = new byte [value.Length - 4];
				byte[] flags = new byte[4];
 
				Array.Copy(value, 0, number, 0, number.Length);
				Array.Copy(value, value.Length - 4, flags, 0, 4);
 
				// endianness
				Array.Reverse(number);

				_unscaledValue = new BigInteger(number);
				_scale = BitConverter.ToInt32(flags, 0);

				
			}
 
			public bool IsEven { get { 
				return (_unscaledValue.Abs().Mod(BigInteger.Two).LongValue == (long) 0); 
			} }

			public bool IsOne { get { 
				return _unscaledValue.Equals(BigInteger.ValueOf(1)); 
			} }



			//public bool IsPowerOfTwo { get { 
			//	return _unscaledValue.IsPowerOfTwo; 
			//} }


			public bool IsZero { get { 
				return _unscaledValue.Equals(BigInteger.Zero); 
			} }

			public int Sign { get { 
				return _unscaledValue.SignValue; 
			} }

 
			public override string ToString()
			{
				string number = _unscaledValue.ToString();
 
				if (_scale > 0)
					return number.Insert(number.Length - _scale, ".");
 
				return number;
			}
 
			public byte[] ToByteArray()
			{
				var unscaledValue = _unscaledValue.ToByteArray();
				var scale = BitConverter.GetBytes(_scale);
 
				var bytes = new byte[unscaledValue.Length + scale.Length];
				Array.Copy(unscaledValue, 0, bytes, 0, unscaledValue.Length);
				Array.Copy(scale, 0, bytes, unscaledValue.Length, scale.Length);
 
				return bytes;
			}
 
			private static byte[] FromDecimal(decimal d)
			{
				byte[] bytes = new byte[16];
 
				int[] bits = decimal.GetBits(d);
				int lo = bits[0];
				int mid = bits[1];
				int hi = bits[2];
				int flags = bits[3];
 				
				bytes[0] = (byte)lo;
				bytes[1] = (byte)(lo >> 8);
				bytes[2] = (byte)(lo >> 0x10);
				bytes[3] = (byte)(lo >> 0x18);
				bytes[4] = (byte)mid;
				bytes[5] = (byte)(mid >> 8);
				bytes[6] = (byte)(mid >> 0x10);
				bytes[7] = (byte)(mid >> 0x18);
				bytes[8] = (byte)hi;
				bytes[9] = (byte)(hi >> 8);
				bytes[10] = (byte)(hi >> 0x10);
				bytes[11] = (byte)(hi >> 0x18);
				bytes[12] = (byte)flags;
				bytes[13] = (byte)(flags >> 8);
				bytes[14] = (byte)(flags >> 0x10);
				bytes[15] = (byte)(flags >> 0x18);
 
				return bytes;
			}
 
			#region Operators
 
			public static bool operator ==(bigDecimal left, bigDecimal right)
			{
				return left.Equals(right);
			}
 
			public static bool operator !=(bigDecimal left, bigDecimal right)
			{
				return !left.Equals(right);
			}
 
			public static bool operator >(bigDecimal left, bigDecimal right)
			{
				return (left.CompareTo(right) > 0);
			}
 
			public static bool operator >=(bigDecimal left, bigDecimal right)
			{
				return (left.CompareTo(right) >= 0);
			}

			public static bool operator <(bigDecimal left, bigDecimal right)
			{
				return (left.CompareTo(right) < 0);
			}
 
			public static bool operator <=(bigDecimal left, bigDecimal right)
			{
				return (left.CompareTo(right) <= 0);
			}
 
			public static bool operator ==(bigDecimal left, decimal right)
			{
				return left.Equals(right);
			}
 
			public static bool operator !=(bigDecimal left, decimal right)
			{
				return !left.Equals(right);
			}
 
			public static bool operator >(bigDecimal left, decimal right)
			{
				return (left.CompareTo(right) > 0);
			}
 
			public static bool operator >=(bigDecimal left, decimal right)
			{
				return (left.CompareTo(right) >= 0);
			}
 
			public static bool operator <(bigDecimal left, decimal right)
			{
				return (left.CompareTo(right) < 0);
			}
 
			public static bool operator <=(bigDecimal left, decimal right)
			{
				return (left.CompareTo(right) <= 0);
			}
 
			public static bool operator ==(decimal left, bigDecimal right)
			{
				return left.Equals(right);
			}
 
			public static bool operator !=(decimal left, bigDecimal right)
			{
				return !left.Equals(right);
			}
 
			public static bool operator >(decimal left, bigDecimal right)
			{
				return (left.CompareTo(right) > 0);
			}
 
			public static bool operator >=(decimal left, bigDecimal right)
			{
				return (left.CompareTo(right) >= 0);
			}
 
			public static bool operator <(decimal left, bigDecimal right)
			{
				return (left.CompareTo(right) < 0);
			}
 
			public static bool operator <=(decimal left, bigDecimal right)
			{
				return (left.CompareTo(right) <= 0);
			}
 
			#endregion
 
			#region Explicity and Implicit Casts
 
			public static explicit operator byte(bigDecimal value) { return value.ToType<byte>(); }
			public static explicit operator sbyte(bigDecimal value) { return value.ToType<sbyte>(); }
			public static explicit operator short(bigDecimal value) { return value.ToType<short>(); }
			public static explicit operator int(bigDecimal value) { return value.ToType<int>(); }
			public static explicit operator long(bigDecimal value) { return value.ToType<long>(); }
			public static explicit operator ushort(bigDecimal value) { return value.ToType<ushort>(); }
			public static explicit operator uint(bigDecimal value) { return value.ToType<uint>(); }
			public static explicit operator ulong(bigDecimal value) { return value.ToType<ulong>(); }
			public static explicit operator float(bigDecimal value) { return value.ToType<float>(); }
			public static explicit operator double(bigDecimal value) { return value.ToType<double>(); }
			public static explicit operator decimal(bigDecimal value) { return value.ToType<decimal>(); }
			public static explicit operator BigInteger(bigDecimal value)
			{

				
				var scaleDivisor = BigInteger.Ten.Pow(value._scale);
				var scaledValue = value._unscaledValue.Divide(scaleDivisor);
				return scaledValue;
			}
 
			public static implicit operator bigDecimal(byte value) { return new bigDecimal(value); }
			public static implicit operator bigDecimal(sbyte value) { return new bigDecimal(value); }
			public static implicit operator bigDecimal(short value) { return new bigDecimal(value); }
			public static implicit operator bigDecimal(int value) { return new bigDecimal(value); }
			public static implicit operator bigDecimal(long value) { return new bigDecimal(value); }
			public static implicit operator bigDecimal(ushort value) { return new bigDecimal(value); }
			public static implicit operator bigDecimal(uint value) { return new bigDecimal(value); }
			public static implicit operator bigDecimal(ulong value) { return new bigDecimal(value); }
			public static implicit operator bigDecimal(float value) { return new bigDecimal(value); }
			public static implicit operator bigDecimal(double value) { return new bigDecimal(value); }
			public static implicit operator bigDecimal(decimal value) { return new bigDecimal(value); }
			public static implicit operator bigDecimal(BigInteger value) { return new bigDecimal(value, 0); }
 
			#endregion
 
			public T ToType<T>() where T : struct
			{
				return (T)((IConvertible)this).ToType(typeof(T), null);
			}
 
			object IConvertible.ToType(Type conversionType, IFormatProvider provider)
			{
				BigInteger scaleDivisor = BigInteger.Ten.Pow(this._scale);
				BigInteger remainder = this._unscaledValue.Remainder(scaleDivisor);
				BigInteger scaledValue = _unscaledValue.Divide(scaleDivisor);
 
				if (scaledValue.CompareTo(Decimal.MaxValue) > 0)
					throw new ArgumentOutOfRangeException("value", "The value " + this._unscaledValue + " cannot fit into " + conversionType.Name + ".");
 				if (scaledValue.CompareTo(BigInteger.ValueOf(long.MaxValue)) > 0) 
					throw new ArgumentOutOfRangeException("value", "The value " + this._unscaledValue + " cannot fit into long value for conversion.");
				if (scaleDivisor.CompareTo(BigInteger.ValueOf (long.MaxValue)) > 0)
				throw new ArgumentOutOfRangeException("scaleDivisor", "The vale of scaleDivisor cannot fit in long value for conversion.");

				decimal leftOfDecimal = (decimal)scaledValue.LongValue;
				decimal rightOfDecimal = ((decimal)remainder.LongValue) / ((decimal)scaleDivisor.LongValue);
 
				decimal value = leftOfDecimal + rightOfDecimal;
				return Convert.ChangeType(value, conversionType);
			}
 
			public override bool Equals(object obj)
			{
				return ((obj is bigDecimal) && Equals((bigDecimal)obj));
			}
 
			public override int GetHashCode()
			{
				return _unscaledValue.GetHashCode() ^ _scale.GetHashCode();
			}
 
			#region IConvertible Members
 
			TypeCode IConvertible.GetTypeCode()
			{
				return TypeCode.Object;
			}
 
			bool IConvertible.ToBoolean(IFormatProvider provider)
			{
				return Convert.ToBoolean(this);
			}
 
			byte IConvertible.ToByte(IFormatProvider provider)
			{
				return Convert.ToByte(this);
			}
 
			char IConvertible.ToChar(IFormatProvider provider)
			{
				throw new InvalidCastException("Cannot cast BigDecimal to Char");
			}
 
			DateTime IConvertible.ToDateTime(IFormatProvider provider)
			{
				throw new InvalidCastException("Cannot cast BigDecimal to DateTime");
			}
 
			decimal IConvertible.ToDecimal(IFormatProvider provider)
			{
				return Convert.ToDecimal(this);
			}

			double IConvertible.ToDouble(IFormatProvider provider)
			{
				return Convert.ToDouble(this);
			}
 
			short IConvertible.ToInt16(IFormatProvider provider)
			{
				return Convert.ToInt16(this);
			}
 
			int IConvertible.ToInt32(IFormatProvider provider)
		{
				return Convert.ToInt32(this);
		}
 
		long IConvertible.ToInt64(IFormatProvider provider)
		{
				return Convert.ToInt64(this);
		}
 
		sbyte IConvertible.ToSByte(IFormatProvider provider)			
		{
				return Convert.ToSByte(this);
		}

		float IConvertible.ToSingle(IFormatProvider provider)
		{
				return Convert.ToSingle(this);
		}
 
		string IConvertible.ToString(IFormatProvider provider)
		{
				return Convert.ToString(this);
		}
 
		ushort IConvertible.ToUInt16(IFormatProvider provider)
		{
				return Convert.ToUInt16(this);
		}
 
		uint IConvertible.ToUInt32(IFormatProvider provider)
		{
				return Convert.ToUInt32(this);
		}
 
		ulong IConvertible.ToUInt64(IFormatProvider provider)
		{
				return Convert.ToUInt64(this);
		}
 
			#endregion
 
			#region IFormattable Members
 
		public string ToString(string format, IFormatProvider formatProvider)
		{
				throw new NotImplementedException();
		}
 
			#endregion
 
			#region IComparable Members
 
		public int CompareTo(object obj)
		{
				if (obj == null)
					return 1;
 
				if (!(obj is bigDecimal))
					throw new ArgumentException("Compare to object must be a BigDecimal", "obj");
 
				return CompareTo((bigDecimal)obj);
		}
 
			#endregion
 
			#region IComparable<BigDecimal> Members
 
		public int CompareTo(bigDecimal other)
		{
				int unscaledValueCompare = this._unscaledValue.CompareTo(other._unscaledValue);
				int scaleCompare = this._scale.CompareTo(other._scale);
 
				// if both are the same value, return the value
				if (unscaledValueCompare == scaleCompare)
					return unscaledValueCompare;
 
				// if the scales are both the same return unscaled value
				if (scaleCompare == 0)
					return unscaledValueCompare;
 
				
				BigInteger scaledValue = this._unscaledValue.Divide( BigInteger.Ten.Pow(this._scale));

				//BigInteger otherScaledValue = BigInteger.Divide(other._unscaledValue, BigInteger.Pow(new BigInteger(10), other._scale));
				BigInteger otherScaledValue = other._unscaledValue.Divide( BigInteger.Ten.Pow( other._scale));

				return scaledValue.CompareTo(otherScaledValue);
		}
 
		#endregion
 
		#region IEquatable<BigDecimal> Members
 		public bool Equals(bigDecimal other)
		{
			return this._scale.CompareTo(other._scale) == 0 && this._unscaledValue.CompareTo(other._unscaledValue) == 0;
		}


 
		#endregion

		public BigInteger unscaledValue ()
		{
			return this._unscaledValue;
		}

		public int scale() {
			return _scale;
		}
	}
}

*/