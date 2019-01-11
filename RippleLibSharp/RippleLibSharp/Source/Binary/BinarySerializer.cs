using System;
using System.IO;
using System.Text;
using Org.BouncyCastle.Math;
using System.Collections.Generic;
using RippleLibSharp.Util;
using RippleLibSharp.Keys;
using RippleLibSharp.Transactions;

namespace RippleLibSharp.Binary
{
	public class BinarySerializer
	{
		//private static readonly int MIN_OFFSET = -96;
		//private static readonly int MAX_OFFSET = 80;
		//private static readonly long MIN_VALUE = 1000000000000000;
		//private static readonly long MAX_VALUE = 9999999999999999;

		/*
		public BinarySerializer ()
		{
			
		}
		*/

		private static byte [] FromDecimal (decimal d)
		{
			byte [] bytes = new byte [16];

			int [] bits = decimal.GetBits (d);
			int lo = bits [0];
			int mid = bits [1];
			int hi = bits [2];
			int flags = bits [3];

			bytes [0] = (byte)lo;
			bytes [1] = (byte)(lo >> 8);
			bytes [2] = (byte)(lo >> 0x10);
			bytes [3] = (byte)(lo >> 0x18);
			bytes [4] = (byte)mid;
			bytes [5] = (byte)(mid >> 8);
			bytes [6] = (byte)(mid >> 0x10);
			bytes [7] = (byte)(mid >> 0x18);
			bytes [8] = (byte)hi;
			bytes [9] = (byte)(hi >> 8);
			bytes [10] = (byte)(hi >> 0x10);
			bytes [11] = (byte)(hi >> 0x18);
			bytes [12] = (byte)flags;
			bytes [13] = (byte)(flags >> 8);
			bytes [14] = (byte)(flags >> 0x10);
			bytes [15] = (byte)(flags >> 0x18);

			return bytes;
		}


		public static int GetDecimalScale (Decimal d)
		{
#if DEBUG
			string method_sig = clsstr + nameof (GetDecimalScale) + DebugRippleLibSharp.left_parentheses + d.ToString () + DebugRippleLibSharp.right_parentheses;
#endif
			byte [] bytes = FromDecimal (d);




			sbyte scale = (sbyte)bytes [14];


#if DEBUG
			if (DebugRippleLibSharp.BinarySerializer) {
				Logging.WriteLog(method_sig + "scale of Decimal is " + scale.ToString() + "\n");
			}
#endif

			return (int)scale;



		}

		public static BigInteger GetDecimalUnscaledValue (Decimal d)
		{
#if DEBUG
			string method_sig = clsstr + nameof (GetDecimalUnscaledValue) + DebugRippleLibSharp.left_parentheses + d.ToString () + DebugRippleLibSharp.right_parentheses;
#endif

			byte [] bytes = FromDecimal (d);

			byte [] unscaledValueBytes = new byte [12];
			Array.Copy (bytes, unscaledValueBytes, unscaledValueBytes.Length);

			// because decimal is little endian and BigInteger is big endian  // TODO MAKE DAMN SURE!!!
			Array.Reverse (unscaledValueBytes);

			var unscaledValue = new BigInteger (1, unscaledValueBytes);

			if (bytes [15] == 128)
				unscaledValue = unscaledValue.Negate ();  //.Multiply(BigInteger.ValueOf(-1)); // changes the sign

#if DEBUG
			if (DebugRippleLibSharp.BinarySerializer) {
				Logging.WriteLog(method_sig + "unscaled of Decimal is " + unscaledValue.ToString() + "\n");
			}
#endif



			return unscaledValue;
		}

		/*
		public static byte[] flipEndianess ( byte[] input )
		{
			byte[] output = new byte[ input.Length ];

			// basically flip the bytes
			for (int i = 0; i < output.Length; i++) {
				output[i]=input[output.Length-(i+1)];
			}

			return output;
		}
		*/

		/*
		public static byte[] prepareBigIntegerBytes (byte[] input)
		{
			byte[] returnMe = new byte[input.Length+1];
			byte[] flipped = flipEndianess(input);
			System.Array.Copy(flipped,returnMe,flipped.Length);
			returnMe[input.Length]=0; // make the last element zero for the bigint


			return returnMe;
		}
		*/

		/*
		public static void serializeTransaction ()
		{

		} */

		public RippleBinaryObject ReadBinaryObject (MemoryStream ms)
		{
#if DEBUG
			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append (clsstr);
			stringBuilder.Append (nameof (ReadBinaryObject));
			stringBuilder.Append (DebugRippleLibSharp.left_parentheses);
			stringBuilder.Append (nameof (MemoryStream));
			stringBuilder.Append (DebugRippleLibSharp.space_char);
			stringBuilder.Append (nameof (ms));
			stringBuilder.Append (DebugRippleLibSharp.right_parentheses);
			string method_sig = stringBuilder.ToString() ;
			stringBuilder.Clear ();

			if (DebugRippleLibSharp.BinarySerializer) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}
#endif


			RippleBinaryObject serializedObject = new RippleBinaryObject ();


			using (BigEndianReader reader = new BigEndianReader (ms)) {

				try {
					for (int i = 0; i < ms.Length; i++) {
						byte firstByte = reader.ReadByte ();
						var orred = 0xF0 & firstByte;
						int type = (orred) >> 4; // The & (AND) is redundant with unsigned bytes but I'll leave it incase I switch to sbyte

						byte final = (byte)type;

						if (type == 0) {
							type = reader.ReadByte (); // if the type is zero, the next byte is the type
						}

						int field = 0x0f & firstByte; // zero out first four bits of byte. 
						if (field == 0) {
							field = reader.ReadByte ();
							//firstByte=(byte)field; // I commented this out because it's never used
						}

						BinaryFieldType serializedField = BinaryFieldType.Lookup (type, field);
						Object value = ReadPrimitive (reader, serializedField.type);
						serializedObject.fields.Add (serializedField, value);



					}
				} catch (Exception e) {

				}
			}

			return serializedObject;
		}


		protected Object ReadPrimitive (BigEndianReader input, BinaryType primative)
		{
			// I'll comment out the ported java code and replace it with c# equivelent. 
			// I think the java code is "hacked" to accomodate the missing unsinged primitives that c# provides

			if (primative.typeCode == BinaryType.UINT16) {
				//return 0xFFFFFFFF & input.ReadInt16 ();
				return input.ReadUInt16 ();

			} else if (primative.typeCode == BinaryType.UINT32) {
				//return 0xFFFFFFFFFFFFFFFF & input.ReadInt32();
				return input.ReadUInt32 ();

			} else if (primative.typeCode == BinaryType.UINT64) {
				//byte[] eightBytes = input.ReadBytes(8);
				//return new BigInteger(eightBytes);
				return input.ReadUInt64 ();
			} else if (primative.typeCode == BinaryType.HASH128) {
				return input.ReadBytes (16);
			} else if (primative.typeCode == BinaryType.HASH256) {
				return input.ReadBytes (32);
			} else if (primative.typeCode == BinaryType.AMOUNT) {
				return ReadAmount (input);
			} else if (primative.typeCode == BinaryType.VARIABLE_LENGTH) {
				return ReadVariableLength (input);
			} else if (primative.typeCode == BinaryType.ACCOUNT) {
				return ReadAccount (input);
			} else if (primative.typeCode == BinaryType.OBJECT) {
				// TODO implement
				throw new NotImplementedException ("Object type, not yet supported");
			} else if (primative.typeCode == BinaryType.ARRAY) {
				throw new NotImplementedException ("Array type, not yet supported");
			} else if (primative.typeCode == BinaryType.UINT8) {
				return 0xFFFF & input.ReadByte (); // I suppose it is intended for the hex 0xFFFF to be 16 bits long?
			} else if (primative.typeCode == BinaryType.HASH160) {
				return ReadIssuer (input); // not a bug?
			} else if (primative.typeCode == BinaryType.PATHSET) {
				// TODO UNCOMMENT this
				//return readPathSet(input);
			} else if (primative.typeCode == BinaryType.VECTOR256) {
				throw new NotImplementedException ("VECTOR256 type, not yet supported");
			}
			throw new NotImplementedException ("Unsupported primitive " + primative);
		}


		protected RippleAddress ReadAccount (BigEndianReader input)
		{
			byte [] accountBytes = ReadVariableLength (input);
			return new RippleAddress (accountBytes);
		}

		byte [] ReadVariableLength (BigEndianReader input)
		{
			int byteLen = 0;
			byte firstByte = input.ReadByte ();
			byte secondByte = 0;
			if (firstByte <= 192) {  // why 192?
				byteLen = firstByte;
			} else if (firstByte <= 240) {
				secondByte = input.ReadByte ();
				byteLen = 193 + ((firstByte - 193) * 256) + secondByte; // TODO come back to this ported code and make sense of this lol

			} else if (firstByte < 254) {
				secondByte = input.ReadByte ();
				byte thirdByte = input.ReadByte ();
				byteLen = 12481 + ((firstByte - 241) * 65536) + (secondByte * 256) + thirdByte; // one of the bytes obviously marks the length?
			} else {
				// TODO Error checking
				throw new Exception ("firstByte=" + firstByte + ", value reserved");
			}

			//byte[] variableBytes = new byte[byteLen];
			return input.ReadBytes (byteLen); // the wonderful moment the code starts to make sense. 
		}


		protected RippleCurrency ReadAmount (BigEndianReader input)
		{

			// TODO go back over this and make sure calculations are precise enough for financials. 
			// Also take into account the sign byte

			//long offsetNativeSignMagnitudeBytes = input.ReadInt64 ();
			ulong offsetNativeSignMagnitudeBytes = input.ReadUInt64 ();

			//1 bit for Native // I'm confused because of the sign bit

			// I added a cast to long.

			//Boolean isNaticeAmount = false;

			// TODO figure this out. ?????
			//unchecked {
			//	isNativeAmount = ((long)0x8000000000000000 & offsetNativeSignMagnitudeBytes) == 0;
			// 
			//}

			Boolean isNativeAmount = (0x8000000000000000 & offsetNativeSignMagnitudeBytes) == 0; // zero's everything but the isNative bit

			// checked
			//isNativeAmount = (0x8000000000000000 & offsetNativeSignMagnitudeBytes) == 0;


			int sign = ((0x4000000000000000 & offsetNativeSignMagnitudeBytes) == 0) ? -1 : 1;

			ulong precast = offsetNativeSignMagnitudeBytes & 0x3FC0000000000000;

			int offset = (int)(precast >> 54);

			ulong longMagnitude = offsetNativeSignMagnitudeBytes & 0x3FFFFFFFFFFFFF;

			// confused even more because of scale

			if (isNativeAmount) {
				Decimal magnitude = longMagnitude;
				magnitude = magnitude * sign;
				return new RippleCurrency (magnitude);

			} else {
				String currencyStr = ReadCurrency (input);
				RippleAddress issuer = ReadIssuer (input);

				if (offset == 0 || longMagnitude == 0) {
					return new RippleCurrency (Decimal.Zero, issuer, currencyStr);
				}

				int decimalPosition = 97 - offset;

				if (decimalPosition < RippleCurrency.MIN_SCALE || decimalPosition > RippleCurrency.MAX_SCALE) {
					throw new ArgumentOutOfRangeException ("invalid scale " + decimalPosition);
				}

				//BigInteger biMagnitude = longMagnitude;


				Decimal fractionalValue = new decimal (longMagnitude) * new decimal (Math.Pow (10, -decimalPosition));  // veryfy this is correct
				if (sign < 0) {
					fractionalValue *= -1m;
				}

				return new RippleCurrency (fractionalValue, issuer, currencyStr);
			}

		}

		protected RippleAddress ReadIssuer (BigEndianReader input)
		{
			byte [] issuerbytes = input.ReadBytes (20);

			// TODO If the issuer is all 0, this means any issuer

			return new RippleAddress (issuerbytes);
		}

		protected String ReadCurrency (BigEndianReader input)
		{
			char[] unknown = input.ReadChars(20);

			//char [] currency = input.ReadChars (8);

			return new String (unknown, 12, 3);

			//TODO See https://ripple.com/wiki/Currency_Format for format
		}

		/*
		protected RipplePathSet readPathSet ( BigEndianReader input) //ArraySegment<Byte> bb
		{
			RipplePathSet pathSet = new RipplePathSet ();
			RipplePath path = null;
			//using (MemoryStream ms = new MemoryStream ( bb.Array )) { 
				//using (BinaryReader input = new BinaryReader(ms)) {

					while (true) {
						byte pathElementType = input.ReadByte();
						if (pathElementType==(byte)0x00) {
							break;
						}

						if (path==null) {
							path = new RipplePath();

							pathSet.Add(path);
						}

						if (pathElementType==(byte)0xff) {
							path = null;
							continue;
						}

						RipplePathElement pathElement = new RipplePathElement();
						path.Add(pathElement);

						// NOTE bit tinkering is not my thing. I know so many languages I get them confused. 
						// this code was ported from java. The line (pathElementType&0x01)!=0 and such may be a 
						// hack to deal with the sign bit. Java lacks unsigned data types but c# doesn't lack these types

						if ((pathElementType&0x01)!=0) { // Account bit is set
							pathElement.account = readIssuer(input);
						}

						if ((pathElementType&0x10)!=0) {
							pathElement.currency = readCurrency(input);
						}

						if ((pathElementType&0x20)!=0) {
							pathElement.issuer = readIssuer(input);
						}
					}

					return pathSet;
				//}
			//}
		} */

		public byte [] WriteBinaryObject (RippleBinaryObject serializedObj)
		{
#if DEBUG
			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append (clsstr);
			stringBuilder.Append (nameof (WriteBinaryObject));
			stringBuilder.Append (DebugRippleLibSharp.left_parentheses);
			stringBuilder.Append (nameof (serializedObj));
			stringBuilder.Append (DebugRippleLibSharp.right_parentheses);
			string method_sig =  stringBuilder.ToString();

			if (DebugRippleLibSharp.BinarySerializer) {
				stringBuilder.Append (DebugRippleLibSharp.beginn);
				Logging.WriteLog( stringBuilder.ToString() );
				stringBuilder.Clear ();

				stringBuilder.Append (method_sig);
				stringBuilder.Append (nameof (serializedObj));
				stringBuilder.Append (DebugRippleLibSharp.equals);
				stringBuilder.AppendLine ();
				stringBuilder.Append (serializedObj?.ToJSONString () ?? DebugRippleLibSharp.null_str);

				Logging.WriteLog (  stringBuilder.ToString() );
				stringBuilder.Clear ();
			}
#endif

			byte [] arr = null;

			MemoryStream memstream = new MemoryStream ();
			using (BigEndianWriter output = new BigEndianWriter (memstream)) {

				List<BinaryFieldType> sortedFeilds = serializedObj.GetSortedFields ();
				foreach (BinaryFieldType field in sortedFeilds) {
					byte typeHalfByte = 0;
					if (field.type.typeCode <= 15) {
						typeHalfByte = (byte)(field.type.typeCode << 4);
					}
					byte fieldHalfByte = 0;
					if (field.value <= 15) {
						fieldHalfByte = (byte)(field.value & 0x0F);
					}

					output.Write ((byte)(typeHalfByte | fieldHalfByte));  // ooohh the moment of zen I see the bytes being OR'ed together
																		  // Note it seems the values are written to stream even if they are zero.


					if (typeHalfByte == 0) {
						output.Write ((byte)field.type.typeCode);
					}

					if (fieldHalfByte == 0) {
						output.Write ((byte)field.value);
					}

					object valu = serializedObj.GetField (field);

#if DEBUG
					if (DebugRippleLibSharp.BinarySerializer) {
						stringBuilder.Append (method_sig);
						stringBuilder.Append (field.ToString ());
						stringBuilder.Append (DebugRippleLibSharp.equals);
						stringBuilder.Append ("Type");
						stringBuilder.Append (DebugRippleLibSharp.colon);
						stringBuilder.Append (field.type.ToString ());
						stringBuilder.Append (", Value");
						stringBuilder.Append (DebugRippleLibSharp.colon);
						stringBuilder.Append (valu.ToString ());
						Logging.WriteLog( stringBuilder.ToString() );
						stringBuilder.Clear ();
					}
#endif

					WritePrimitive (output, field.type, valu);




				}

				output.Flush ();
				memstream.Flush ();
				output.Flush ();
				memstream.Flush ();

				arr = memstream.ToArray ();

			} // end using BinaryWriter



			return arr;
		}


		protected void WritePrimitive (BigEndianWriter output, BinaryType primitive, Object value)
		{
#if DEBUG
			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append (clsstr);
			stringBuilder.Append (nameof (WritePrimitive));
			stringBuilder.Append (DebugRippleLibSharp.left_parentheses);
			stringBuilder.Append (nameof (BigEndianWriter));
			stringBuilder.Append (DebugRippleLibSharp.space_char);
			stringBuilder.Append (nameof (output));
			stringBuilder.Append (DebugRippleLibSharp.comma);
			stringBuilder.Append (nameof (BinaryType));
			stringBuilder.Append (DebugRippleLibSharp.space_char);
			stringBuilder.Append (nameof (primitive));
			stringBuilder.Append (DebugRippleLibSharp.comma);
			stringBuilder.Append (nameof (Object));
			stringBuilder.Append (DebugRippleLibSharp.space_char);
			stringBuilder.Append (nameof(value));
			stringBuilder.Append (DebugRippleLibSharp.right_parentheses);

			string method_sig = stringBuilder.ToString();
			if (DebugRippleLibSharp.BinarySerializer) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
				Logging.WriteLog(method_sig + "expected BinaryType is " + primitive.ToString() + ", actual value's type is " + value?.GetType()?.ToString() ?? DebugRippleLibSharp.null_str);
			}
#endif

			// ok going to comment much of this ported code and take advantage of c#'s unsigned types
			if (primitive.typeCode == BinaryType.UINT16) {

				UInt16 intValue = (UInt16)value;
				output.Write ((byte)(intValue >> 8 & 0xFF));
				output.Write ((byte)(intValue & 0xFF));
				//output.Write (intValue);

			} else if (primitive.typeCode == BinaryType.UINT32) {
				UInt32 longValue = (UInt32)value;
				if (longValue > (long)0xFFFFFFFF) {
					throw new InternalBufferOverflowException ("UINT32 overflow for value " + value);
				}
				output.Write ((byte)(longValue >> 24 & 0xFF));
				output.Write ((byte)(longValue >> 16 & 0xFF));
				output.Write ((byte)(longValue >> 8 & 0xFF));
				output.Write ((byte)(longValue & 0xFF));

				//Logging.write(value.GetType().ToString());

				//output.Write ((UInt32)value);

			} else if (primitive.typeCode == BinaryType.UINT64) {
				// spend days porting RipplePrivateKey.bigIntegerToBytes() only to clue in there is an uint in c# :O
				//byte[] biBytes = (uint) value; //  RipplePrivateKey.bigIntegerToBytes((BigInteger) value, 8);
				UInt64 ulvalue = (UInt64)value;
				output.Write (ulvalue);

			} else if (primitive.typeCode == BinaryType.HASH128) {
				byte [] sixteenBytes = (byte [])value;  // change to sbyte?
				if (sixteenBytes.Length != 16) {
					throw new InvalidCastException ("Value " + value + " is not a HASH128");
				}
				output.Write (sixteenBytes);

			} else if (primitive.typeCode == BinaryType.HASH256) {
				byte [] thirtyTwoBytes = (byte [])value;
				if (thirtyTwoBytes.Length != 32) {
					throw new InvalidCastException ("Value " + value + " is not a HASH256");
				}
			} else if (primitive.typeCode == BinaryType.AMOUNT) {

				WriteAmount (output, (RippleCurrency)value);

			} else if (primitive.typeCode == BinaryType.VARIABLE_LENGTH) {
				WriteVariableLength (output, ((byte [])value));
			} else if (primitive.typeCode == BinaryType.ACCOUNT) {
				RippleAddress ra = null;
				if (value.GetType () == "".GetType ()) {
					string v = (string)value;
					ra = new RippleAddress (v);
				} else {
					ra = (RippleAddress)value;
				}
				WriteAccount (output, ra);
			} else if (primitive.typeCode == BinaryType.OBJECT) {
				throw new NotImplementedException ("Object type, not yet supported");
			} else if (primitive.typeCode == BinaryType.ARRAY) {
				throw new NotImplementedException ("Array type, not yet supported");
			} else if (primitive.typeCode == BinaryType.UINT8) {
				int intValue = (int)value;
				if (intValue > 0xFF) {
					throw new OverflowException ("UINT8 overflow for value " + value);
				}
				output.Write ((byte)value);
			} else if (primitive.typeCode == BinaryType.HASH160) {
				WriteIssuer (output, (RippleAddress)value);
			} else if (primitive.typeCode == BinaryType.PATHSET) {
				//TODO uncomment this !!!!
				//writePathSet (output, (RipplePathSet)value);
			} else if (primitive.typeCode == BinaryType.VECTOR256) {
				throw new NotImplementedException ("VECTOR256 type, not yet supported");
			} else {
				throw new NotSupportedException ("Unsupported primitive " + primitive);
			}
		}

		/*
		protected void writePathSet (BigEndianWriter output, RipplePathSet pathSet)
		{

			for (int i = 0; i < pathSet.Count; i++) {
				RipplePath path=pathSet[i];
				for (int j=0; j<path.Count; j++) {
					RipplePathElement pathElement = path[j];
					byte pathElementType=0;
					if (pathElement.account!=null) {
						pathElementType|=0x01;  // TODO why 0x01 when BinaryType.account = 0x08??
					}
					if (pathElement.currency!=null) {
						pathElementType|=0x10;   // I assume they are bit flags. I remember seeing this on the wiki
					}
					if (pathElement.issuer!=null) {
						pathElementType|=0x20;
					}
					output.Write(pathElementType);

					if (pathElement.account!=null) {
						writeIssuer(output, (RippleAddress)pathElement.account);
					}

					if (pathElement.currency!=null) {
						writeCurrency(output, pathElement.currency);
					}

					if (pathElement.issuer!=null) {
						writeIssuer(output, (RippleAddress)pathElement.issuer);
					}

					if ( (i+1==pathSet.Count) && (j+1==path.Count) ) {
						goto END; // that's right, I used a goto. Sue me lol
					}
				}

				output.Write((byte) 0xFF);

			}

			END:
				output.Write((byte) 0);
		}*/

		protected void WriteIssuer (BigEndianWriter output, RippleAddress value)
		{
#if DEBUG
			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append (clsstr);
			stringBuilder.Append (nameof(WriteIssuer));
			stringBuilder.Append (DebugRippleLibSharp.left_parentheses);
			stringBuilder.Append (nameof (BigEndianReader));
			stringBuilder.Append (DebugRippleLibSharp.space_char);
			stringBuilder.Append (nameof(output));
			stringBuilder.Append (DebugRippleLibSharp.comma);
			stringBuilder.Append (nameof (RippleAddress));
			stringBuilder.Append (DebugRippleLibSharp.space_char);
			stringBuilder.Append (nameof (value));
			stringBuilder.Append (DebugRippleLibSharp.right_parentheses);

			string method_sig = stringBuilder.ToString ();

			if (DebugRippleLibSharp.BinarySerializer) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			byte [] issuerBytes = value.GetBytes ();

#if DEBUG
			if (DebugRippleLibSharp.BinarySerializer) {
				Logging.WriteLog("BinarySerializer : writeIssuer : issuerBytes.Length = " + issuerBytes.Length.ToString());
			}
#endif
			output.Write (issuerBytes);
		}

		protected void WriteAccount (BigEndianWriter output, RippleAddress address)
		{
			WriteVariableLength (output, address.GetBytes ());
		}




		protected void WriteAmount (BigEndianWriter output, RippleCurrency denominatedCurrency)
		{
#if DEBUG
			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append (clsstr);
			stringBuilder.Append (nameof(WriteAmount));
			stringBuilder.Append (DebugRippleLibSharp.left_parentheses);
			stringBuilder.Append (nameof (BigEndianWriter));
			stringBuilder.Append (DebugRippleLibSharp.space_char);
			stringBuilder.Append (nameof(output));
			stringBuilder.Append (DebugRippleLibSharp.comma);
			stringBuilder.Append (nameof (RippleCurrency));
			stringBuilder.Append (DebugRippleLibSharp.space_char);
			stringBuilder.Append (nameof (denominatedCurrency));
			stringBuilder.Append (DebugRippleLibSharp.right_parentheses);
			string method_sig = stringBuilder.ToString ();
			stringBuilder.Clear ();
			if (DebugRippleLibSharp.BinarySerializer) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			ulong offsetNativeSignMagnitudeBytes = 0;
			if (denominatedCurrency.amount > 0m) {

#if DEBUG
				if (DebugRippleLibSharp.BinarySerializer) {
					Logging.WriteLog (method_sig + nameof(denominatedCurrency.amount) + " > 0m");
				}
#endif
				offsetNativeSignMagnitudeBytes |= 0x4000000000000000;  // Note I thought this was incorrect untill I read this https://ripple.com/wiki/Binary_Format#Native_Currency
			}


			if (denominatedCurrency.IsNative) {
#if DEBUG
				if (DebugRippleLibSharp.BinarySerializer) {
					Logging.WriteLog(method_sig + nameof(denominatedCurrency) + " is Native currency");
				}
#endif

				if (denominatedCurrency.amount > ulong.MaxValue) {
					throw new OverflowException (nameof (denominatedCurrency.amount) + "is larger than long.MaxValue");
				}

				ulong drops = (ulong)denominatedCurrency.amount;

				offsetNativeSignMagnitudeBytes |= drops;



				output.Write (offsetNativeSignMagnitudeBytes);

			} else {
#if DEBUG
				if (DebugRippleLibSharp.BinarySerializer) {
					Logging.WriteLog(method_sig + nameof(denominatedCurrency) + "is Non Native");
				}
#endif

				offsetNativeSignMagnitudeBytes |= 0x8000000000000000;


				//string am = denominatedCurrency.amount;

				//if (am.Contains('.')) {
				//	int ind = am.IndexOf ('.');


				//}

				BigInteger unscaledValue = GetDecimalUnscaledValue (denominatedCurrency.amount);


				if (!(unscaledValue.Equals (BigInteger.Zero))) {
#if DEBUG
					if (DebugRippleLibSharp.BinarySerializer) {
						Logging.WriteLog(method_sig + nameof(unscaledValue) + " != 0");
					}
#endif

					int scale = GetDecimalScale (denominatedCurrency.amount); // I don't think there's a such thing as a negative scale. I could be wrong. 
					UInt64 mantissa = (UInt64)unscaledValue.Abs ().LongValue;


					while (mantissa > RippleCurrency.MAX_MANTISSA) {
						mantissa /= 10;
						scale--;  // scale is flipped
								  //scale++;

						if (scale > RippleCurrency.MAX_SCALE) {
							throw new OverflowException (nameof (scale) + "is greater than MAX_SCALE");
						}
					}

					while (mantissa < RippleCurrency.MIN_MANTISSA) {
						mantissa *= 10;
						scale++;
						//scale--;

						if (scale < RippleCurrency.MIN_SCALE) {
							throw new OverflowException (nameof (scale) + "is smaller than MIN_SCALE");
						}
					}


#if DEBUG
					if (DebugRippleLibSharp.BinarySerializer) {
						Logging.WriteLog("BinarySerializer : writeAmount : mantissa = " + mantissa.ToString());
					}
#endif

					ulong orme = 97UL - (ulong)scale; // scale is flipped

					orme <<= 54;
					orme |= mantissa;

					offsetNativeSignMagnitudeBytes |= orme;

				}
#if DEBUG
				if (DebugRippleLibSharp.BinarySerializer) {
					byte[] bits = BitConverter.GetBytes(offsetNativeSignMagnitudeBytes);

					if (BitConverter.IsLittleEndian) {
						Array.Reverse(bits);
					}

					String bitstring = "";
					foreach (byte b in bits) {
						bitstring +=  Convert.ToString(b, 2).PadLeft(8,'0') + " ";
					}


					Logging.WriteLog(method_sig + nameof(offsetNativeSignMagnitudeBytes) + " = " + bitstring);
				}
#endif

				output.Write (offsetNativeSignMagnitudeBytes);
				WriteCurrency (output, denominatedCurrency.currency);
				WriteIssuer (output, denominatedCurrency.issuer);
			}
		}

		public Decimal ParseDecimal ()
		{
			return 0m;
		}


		// TODO Unit test this function
		protected void WriteVariableLength (BigEndianWriter output, byte [] value)
		{
			int len = value.Length;
			if (value.Length < 192) {
				output.Write ((byte)len);
			} else if (value.Length < 12480) { //193 + (b1-193)*256 + b2

				len -= 193;
				int firstByte = (len >> 8) + 193;
				int secondByte = (len & 0xff);
				output.Write ((byte)firstByte);
				output.Write ((byte)secondByte);


			} else if (value.Length < 918744) {

				len -= 12481;
				int first = (241 + (len >> 16));
				int second = (len >> 8) & 0xff;
				int third = len & 0xff;
				output.Write ((byte)first);
				output.Write ((byte) second);
				output.Write ((byte) third);

				/*
				int firstByte = (value.Length / 65536) + 241;
				output.Write ((byte)firstByte);
				int secondByte = (value.Length - firstByte) / 256;
				output.Write ((byte)secondByte);
				int thirdByte = value.Length - firstByte - secondByte - 12481;
				output.Write ((byte)thirdByte);
				*/	
			}
			output.Write (value);
		}

		protected void WriteCurrency (BigEndianWriter output, String currency)
		{
#if DEBUG
			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append (clsstr);
			stringBuilder.Append (nameof (WriteCurrency));
			stringBuilder.Append (DebugRippleLibSharp.left_parentheses);
			stringBuilder.Append (nameof (BigEndianWriter));
			stringBuilder.Append (DebugRippleLibSharp.space_char);
			stringBuilder.Append (nameof (output));
			stringBuilder.Append (DebugRippleLibSharp.comma);
			stringBuilder.Append (nameof (String));
			stringBuilder.Append (DebugRippleLibSharp.space_char);
			stringBuilder.Append (nameof (currency));
			stringBuilder.Append (DebugRippleLibSharp.right_parentheses);
			string method_sig =  stringBuilder.ToString();

			if (DebugRippleLibSharp.BinarySerializer) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin + DebugRippleLibSharp.colon + nameof(currency) + DebugRippleLibSharp.equals + currency);
			}
#endif

			byte [] currencyBytes = new byte [20];
			// I don't think this is needed. Trying everything to get this to work :/
			for (int i = 0; i < currencyBytes.Length; i++) {
				currencyBytes [i] = 0;
			}
			char [] str = currency.ToCharArray ();



			// TODO what to do if ripple implements currency codes larger than 3 bytes. 
			/*
			if (currency.Length > 3) {
				throw new NotImplementedException ("Currency codes greater than 3 chars are not currently supported");
			}
			*/


			byte [] sour = new byte [3];
			try {
				int i = 0;
				foreach (char c in str) {
					sour [i++] = Convert.ToByte (c);
				}
			} catch (OverflowException e) {
				// TODO debug
				throw e;
			}


			//byte[] sour = Encoding.UTF8.GetBytes(currency);

			//byte[] sour = ASCIIEncoding.ASCII.GetBytes(currency);

#if DEBUG
			if (DebugRippleLibSharp.BinarySerializer) {
				Logging.WriteLog(method_sig + "Ascii length" + DebugRippleLibSharp.equals + sour.Length);
			}
#endif

			/*
			byte[] source = Encoding.BigEndianUnicode.GetBytes(currency);
			if (Debug.BinarySerializer) {
				Logging.write("BinarySerializer : writeCurrency : bigendianUnicode length = " + source.Length);
			}
			*/


			Array.Copy (sour, 0, currencyBytes, 12, 3);


			output.Write (currencyBytes);

		}

#if DEBUG
		private const string clsstr = nameof (BinarySerializer) + DebugRippleLibSharp.colon;
#endif

	}

}

