using System;
using RippleLibSharp.Binary;

namespace RippleLibSharp.Transactions
{
	public class RippleMemo
	{
		
		/*
		public RippleMemo ()
		{
		}
		*/


		public string MemoData {
			get;
			set;
		}

		public String MemoFormat {
			get;
			set;
		}

		public String MemoType {
			get;
			set;
		}



	}

	public class MemoIndice
	{

		public RippleMemo Memo {
			get;
			set;
		}

		public string GetMemoDataAscii ()
		{
			return Base58.HexToAscii (Memo?.MemoData);

		}

		public string GetMemoFormatAscii ()
		{
			return Base58.HexToAscii (Memo?.MemoFormat);
		}

		public string GetMemoTypeAscii ()
		{
			return Base58.HexToAscii (Memo?.MemoType);
		}

	}
}

