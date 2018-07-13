using System;
using System.IO;

namespace RippleLibSharp.Binary
{
	
	public class BigEndianWriter : BinaryWriter
	{
		private byte[] a16 = new byte[2];
        private byte[] a32 = new byte[4];
        private byte[] a64 = new byte[8];
 
        public BigEndianWriter(Stream output) : base(output) { }
 
        public override void Write (Int16 value)
		{
			if (BitConverter.IsLittleEndian) {
				a16 = BitConverter.GetBytes (value);
				Array.Reverse (a16);
				base.Write (a16);
			} else {
				base.Write(value);
			}
        }
 
        public override void Write (Int32 value)
		{
			if (BitConverter.IsLittleEndian) {
				a32 = BitConverter.GetBytes (value);
				Array.Reverse (a32);
				base.Write (a32);
			} else {
				base.Write(value);
			}

        }
 
        public override void Write (Int64 value)
		{
			if (BitConverter.IsLittleEndian) {
				a64 = BitConverter.GetBytes (value);
				Array.Reverse (a64);
				base.Write (a64);
			} else {
				base.Write(value);
			}
        }
 
        public override void Write (UInt16 value)
		{
			if (BitConverter.IsLittleEndian) {
				a16 = BitConverter.GetBytes (value);
				Array.Reverse (a16);
				base.Write (a16);
			} else {
				base.Write(value);
			}
        }
 
        public override void Write (UInt32 value)
		{
			if (BitConverter.IsLittleEndian) {
				a32 = BitConverter.GetBytes (value);
				Array.Reverse (a32);
				base.Write (a32);
			} else {
				base.Write(value);
			}
        }
 
        public override void Write (Single value)
		{
			if (BitConverter.IsLittleEndian) {
				a32 = BitConverter.GetBytes (value);
				Array.Reverse (a32);
				base.Write (a32);
			} else {
				base.Write(value);
			}
        }
 
        public override void Write (UInt64 value)
		{
			if (BitConverter.IsLittleEndian) {
				a64 = BitConverter.GetBytes (value);
				Array.Reverse (a64);
				base.Write (a64);
			} else {
				base.Write(value);
			}
        }
 
	/*
        public override void Write (Double value)
		{
			if (BitConverter.IsLittleEndian) {
				a64 = BitConverter.GetBytes (value);
				Array.Reverse (a64);
				base.Write (a64);
			} else {
				base.Write(value);
			}
        }
        */
	}


}

