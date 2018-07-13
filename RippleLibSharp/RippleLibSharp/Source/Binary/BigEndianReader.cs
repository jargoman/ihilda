using System;
using System.IO;
using System.Text;

namespace RippleLibSharp.Binary
{
	public class BigEndianReader : BinaryReader
	{
		public BigEndianReader (Stream stream) : base(stream) 
		{
		}

		public BigEndianReader (Stream stream, Encoding encoding) : base(stream, encoding)
		{
		}

		private byte[] a16 = new byte[2];
		private byte[] a32 = new byte[4];
		private byte[] a64 = new byte[8];

		 public override Int16 ReadInt16 ()
		{
			if (BitConverter.IsLittleEndian) {
				a16 = base.ReadBytes (2);
				Array.Reverse (a16);
				return BitConverter.ToInt16 (a16, 0);
			}

			return base.ReadInt16();

        }
 
        public override int ReadInt32 ()
		{
			if (BitConverter.IsLittleEndian) {
				a32 = base.ReadBytes (4);
				Array.Reverse (a32);
				return BitConverter.ToInt32 (a32, 0);
			} 

			return base.ReadInt32();

        }
 
        public override Int64 ReadInt64 ()
		{
			if (BitConverter.IsLittleEndian) {
				a64 = base.ReadBytes (8);
				Array.Reverse (a64);
				return BitConverter.ToInt64 (a64, 0);
			}
			return base.ReadInt64();

        }
 
        public override UInt16 ReadUInt16 ()
		{
			if (BitConverter.IsLittleEndian) {
				a16 = base.ReadBytes (2);
				Array.Reverse (a16);
				return BitConverter.ToUInt16 (a16, 0);
			}
			return base.ReadUInt16();

        }
 
        public override UInt32 ReadUInt32 ()
		{
			if (BitConverter.IsLittleEndian) {
				a32 = base.ReadBytes (4);
				Array.Reverse (a32);
				return BitConverter.ToUInt32 (a32, 0);
			}

			return base.ReadUInt32();

        }
 
        public override Single ReadSingle ()
		{
			if (BitConverter.IsLittleEndian) {
				a32 = base.ReadBytes (4);
				Array.Reverse (a32);
				return BitConverter.ToSingle (a32, 0);
			}

			return base.ReadSingle();

        }
 
        public override UInt64 ReadUInt64 ()
		{
			if (BitConverter.IsLittleEndian) {
				a64 = base.ReadBytes (8);
				Array.Reverse (a64);
				return BitConverter.ToUInt64 (a64, 0);
			}

			return base.ReadUInt64();

        }
 
        public override Double ReadDouble ()
		{
			if (BitConverter.IsLittleEndian) {
				a64 = base.ReadBytes (8);
				Array.Reverse (a64);
				return BitConverter.ToUInt64 (a64, 0);
			}

			return base.ReadDouble();

        }
 
        public string ReadStringToNull ()
		{
			//if (BitConverter.IsLittleEndian) {
				string result = "";
				char c;
				for (int i = 0; i < base.BaseStream.Length; i++) {
					if ((c = (char)base.ReadByte ()) == 0) {
						break;
					}
					result += c.ToString ();
				}
				return result;
			//}


        }

	}
}

