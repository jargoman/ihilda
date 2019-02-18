
/**
* Copyright 2013 jargoman. // this is a port of pmarches RippleBase58.java 
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Math;
using RippleLibSharp.Util;
//using System.Numerics;


namespace RippleLibSharp.Binary
{
	public static class Base58
	{


		public static readonly String ALPHABET = "rpshnaf39wBUDNEGHJKLM4PQRST7VWXYZ2bcdeCg65jkm8oFqi1tuvAxyz";  /// 

		private static readonly BigInteger BASE = BigInteger.ValueOf (58);

		public static bool IsBase58 (String str)
		{



#if DEBUG
			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append (clsstr);
			stringBuilder.Append (nameof(IsBase58));
			stringBuilder.Append (DebugRippleLibSharp.left_parentheses);
			stringBuilder.Append (str ?? DebugRippleLibSharp.null_str);
			stringBuilder.Append (DebugRippleLibSharp.right_parentheses);

			string method_sig = stringBuilder.ToString ();
			stringBuilder.Clear ();
#endif

			foreach (char c in str) {
				if (!IsBase58 (c)) {
#if DEBUG
					if (DebugRippleLibSharp.Base58) {
						stringBuilder.Append (method_sig);
						stringBuilder.Append ("\"");
						stringBuilder.Append (c.ToString ());
						stringBuilder.Append ("\" is not a Base58 value returning false");
						Logging.WriteLog (stringBuilder.ToString ());
						stringBuilder.Clear ();
					}
#endif
					return false;
				} 
			}

#if DEBUG
			if (DebugRippleLibSharp.Base58) {
				stringBuilder.Append (method_sig);
				stringBuilder.Append ("returning true");
				Logging.WriteLog (stringBuilder.ToString ());
				stringBuilder.Clear ();
			}
#endif

			return true;

		}

		public static bool IsBase58 (Char c)
		{
			foreach (char a in ALPHABET) {
				if (a.Equals (c)) { return true; }
			}

			return false;
		}

		public static String Encode (byte [] input)
		{
			if (input == null) {
				throw new ArgumentNullException ();
			}

			BigInteger bi = new BigInteger (1, input);
			StringBuilder s = new StringBuilder ();

			try {
				while (bi.CompareTo (BASE) >= 0) { // while 
					BigInteger mod = bi.Mod (BASE);

					//
					//s.Insert(0,ALPHABET[(int)mod]); // old implementation // this is the cast that could throw an overflow exception
					s.Insert (0, ALPHABET [mod.IntValue]); // The new implementation shouldn't throw an exception. It uses BouncyCastles BigInteger

					//bi = (bi - mod) / BASE; // old implementation
					bi = bi.Subtract (mod).Divide (BASE);
				}
				// found a bug in old implementation it was s.Insert(0,ALPHABET[0]); the second zero being the bug. Must have been ide autocomplete
				s.Insert (0, ALPHABET [bi.IntValue]);

				//} catch (OverflowException e) { // old
			} catch (Exception e) {
				// TODO debug
				// Far less likely an exception would be thrown. Might as well keep a generic catch for potentially unforseen bugs. Divide by zero? ?? who knows. 
				throw e;

			}

			foreach (byte anInput in input) {
				if (anInput == 0) {
					s.Insert (0, ALPHABET [0]);
				} else {
					break;
				}
			}

			return s.ToString ();
		}

		public static byte [] Decode (string input)
		{
#if DEBUG
			String method_sig = clsstr + nameof(Decode) + DebugRippleLibSharp.left_parentheses + DebugRippleLibSharp.AssertAllowInsecure (input) + DebugRippleLibSharp.right_parentheses;

			if (DebugRippleLibSharp.Base58) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			byte [] bytes = DecodeToBigInteger (input).ToByteArray ();

#if DEBUG
			if (DebugRippleLibSharp.Base58 && DebugRippleLibSharp.allowInsecureDebugging) {

				/*
				var sb = new StringBuilder (clsstr);

				//"ToByteArray() = { ");
				foreach (var b in bytes) {
					sb.Append (b + ", ");
				}
				sb.Append ("}\n");

				Logging.write (sb.ToString ());
				*/

				Logging.WriteLog (method_sig + "ToByteArray() = { ", bytes);


			}
#endif

			// We may have got one more byte than we wanted, if the high bit of the next-to-last byte was not zero. This
			// is because BigIntegers are represented with twos-compliment notation, thus if the high bit of the last
			// byte happens to be 1 another 8 zero bits will be added to ensure the number parses as positive. 
			//Detect that case here and chop it off.

			Boolean stripSignByte = bytes.Length > 1 && bytes [0] == 0 && (sbyte)bytes [1] < 0;



			int leadingZeros = 0;
			for (int i = 0; i < input.Length && input [i] == ALPHABET [0]; i++) { // Why ALPHABET[0]? I guess if the base alphabet changes the algorithm stays the same
				leadingZeros++;
			}

			byte [] tmp = new byte [bytes.Length - (stripSignByte ? 1 : 0) + leadingZeros]; // I think I understand this madness 
			System.Array.Copy (bytes, stripSignByte ? 1 : 0, tmp, leadingZeros, tmp.Length - leadingZeros); //

			//byte[] tmp = new byte[bytes.Length - (stripSignByte ? leadingZeros : 0)]; // I think I understand this madness 
			//System.Array.Copy(bytes, stripSignByte ? leadingZeros : 0, tmp, 0, tmp.Length - leadingZeros); //

#if DEBUG
			if (DebugRippleLibSharp.Base58) {
				Logging.WriteLog (method_sig + nameof (stripSignByte) + DebugRippleLibSharp.equals + stripSignByte.ToString ());

				if (DebugRippleLibSharp.allowInsecureDebugging) {
					Logging.WriteLog (method_sig + nameof(leadingZeros) + DebugRippleLibSharp.equals + leadingZeros.ToString ());
					Logging.WriteLog (method_sig + nameof(tmp) + DebugRippleLibSharp.array_brackets + DebugRippleLibSharp.equals, tmp);
				}

				/*
				var sb = new StringBuilder ("tmp[] = { ");
				foreach (var b in tmp) {
					sb.Append (b + ", ");
				}
				sb.Append ("}\n");
				

				Logging.write (sb.ToString ());
				*/

			}
#endif

			return tmp;
		}

		public static BigInteger DecodeToBigInteger (String input)
		{
#if DEBUG
			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append (nameof (DecodeToBigInteger));
			stringBuilder.Append (DebugRippleLibSharp.left_parentheses);
			stringBuilder.Append (nameof (input));
			stringBuilder.Append (DebugRippleLibSharp.equals);
			stringBuilder.Append (DebugRippleLibSharp.AssertAllowInsecure (input));
			stringBuilder.Append (DebugRippleLibSharp.right_parentheses);

			String method_sig = stringBuilder.ToString() ;

			if (DebugRippleLibSharp.Base58) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			BigInteger bigInt = BigInteger.Zero;

			//char[] inp = input.ToCharArray();

			String alph = ALPHABET;
#if DEBUG
			if (DebugRippleLibSharp.Base58) {
				Logging.WriteLog (method_sig + nameof(ALPHABET) + DebugRippleLibSharp.equals + alph);
			}
#endif

#if DEBUG
			StringBuilder sb = new StringBuilder ();
#endif

			for (int i = input.Length - 1; i >= 0; i--) {
				int alphaIndex = alph.IndexOf (input [i]);

#if DEBUG
				if (DebugRippleLibSharp.Base58 && DebugRippleLibSharp.allowInsecureDebugging) {
					sb.Append (input [i]);
					sb.Append (" = ");
					sb.Append (alphaIndex.ToString ());
					sb.Append (", ");
				}
#endif

				if (alphaIndex == -1) {
					throw new IndexOutOfRangeException ("Illegal character " + input [i] + " at " + i);
				}

				// old implementation
				//BigInteger addme = alphaIndex * BigInteger.Pow (BASE, input.Length - 1 + i );
				//bi = bi + addme;

				// new bouncy castle BigInteger Implementation is exatly the same as ported code :)
				bigInt = bigInt.Add (BigInteger.ValueOf (alphaIndex).Multiply (BASE.Pow (input.Length - 1 - i)));
			}

#if DEBUG
			if (DebugRippleLibSharp.Base58 && DebugRippleLibSharp.allowInsecureDebugging) {
				Logging.WriteLog (sb.ToString ());
				Logging.WriteLog (method_sig + DebugRippleLibSharp.returning + bigInt + DebugRippleLibSharp.colon + DebugRippleLibSharp.ToAssertString (bigInt));
			}
#endif

			return bigInt;

		}

		/*
		public static byte [] StringToByteArrayFastest (string hex)
		{
			if (hex.Length % 2 == 1)
				throw new Exception ("The binary key cannot have an odd number of digits");

			byte [] arr = new byte [hex.Length >> 1];

			for (int i = 0; i < hex.Length >> 1; ++i) {
				arr [i] = (byte)((GetHexVal (hex [i << 1]) << 4) + (GetHexVal (hex [(i << 1) + 1])));
			}

			return arr;
		}

		public static int GetHexVal (char hex)
		{
			int val = (int)hex;
			//For uppercase A-F letters:
			return val - (val < 58 ? 48 : 55);
			//For lowercase a-f letters:
			//return val - (val < 58 ? 48 : 87);
			//Or the two combined, but a bit slower:
			//return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
		}
		*/

		public static string StringToHex ( string str)
		{
			if (str == null) {
				return null;
			}

			return ByteArrayToHexString ( Encoding.ASCII.GetBytes (str) );

		}

		public static string HexToAscii ( string hex )
		{
			if (hex == null) {
				return null;
			}

			byte [] bytes = StringToByteArray (hex);

			return Encoding.ASCII.GetString (bytes);



		}

		public static byte [] StringToByteArray (string hex)
		{
			return Enumerable.Range (0, hex.Length)
					 .Where (x => x % 2 == 0)
					 .Select (x => Convert.ToByte (hex.Substring (x, 2), 16))
					 .ToArray ();
		}


		public static string ByteArrayToHexString (byte [] bytes)
		{
			StringBuilder result = new StringBuilder (bytes.Length * 2);
			string HexAlphabet = "0123456789ABCDEF";

			foreach (byte b in bytes) {
				//result.Append (HexAlphabet [(int)(b >> 4)]);
				//result.Append (HexAlphabet [(int)(b & 0x0F)]);
				result.Append (HexAlphabet [b >> 4]);
				result.Append (HexAlphabet [b & 0x0F]);
			}

			return result.ToString ();
		}

		// We could move this later. I put it here so not to add another class just for a single function. Same with the hex function above
		public static string TruncateTrailingZerosFromString (String str)
		{
			if (string.IsNullOrWhiteSpace(str)) {
				return str;
			}

			str.Trim ();

			if (str.Contains (".")) return str.TrimEnd ('0').TrimEnd ('.');

			return str;
		}

#if DEBUG

		public static readonly string clsstr = nameof(Base58) + DebugRippleLibSharp.colon;
#endif

	}
}

