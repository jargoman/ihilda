using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using Codeplex.Data;
using RippleLibSharp.Keys;
using RippleLibSharp.Util;
using RippleLibSharp.Transactions.TxTypes;
namespace RippleLibSharp.Binary
{
	public class RippleBinaryObject
	{

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public Dictionary<BinaryFieldType, Object> fields = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		static BinarySerializer binSer = new BinarySerializer ();

		public RippleBinaryObject ()
		{
			fields = new Dictionary<BinaryFieldType, Object> ();
		}

		public RippleBinaryObject (RippleBinaryObject ob) : this (ob.fields)
		{

		}

		public RippleBinaryObject (Dictionary<BinaryFieldType, object> fields) : this () // todo is this() necessary?
		{
			//this.fields = new Dictionary<BinaryFieldType, object>();

			// I hope this is sufficient for a copy. TODO fix me for complete copy
			foreach (var pair in fields) {
				this.fields.Add (pair.Key, pair.Value);
			}

		}

		public RippleBinaryObject GetUnsignedCopy ()
		{
			// TODO may be a good idea to completely copy. In this case the values are copied as well, see constructor
			RippleBinaryObject copy = new RippleBinaryObject (this);

			copy.RemoveField (BinaryFieldType.TxnSignature);

			return copy;
		}

		public bool RemoveField (BinaryFieldType bin)
		{

			return fields.Remove (bin);

		}

		public byte [] GenerateHashFromBinaryObject ()
		{
			var bts = binSer.WriteBinaryObject (this);
			byte [] bytesToSign = bts;

			// Prefix bytesToSign with the magic hashing prefix (32bit) 'STX\0'
			byte [] prefixedBytesToHash = new byte [bytesToSign.Length + 4];

			prefixedBytesToHash [0] = (byte)'S';
			prefixedBytesToHash [1] = (byte)'T';
			prefixedBytesToHash [2] = (byte)'X';
			prefixedBytesToHash [3] = (byte)'\n';

			System.Array.Copy (bytesToSign, 0, prefixedBytesToHash, 4, bytesToSign.Length);

			byte [] hashOfBytes = RippleDeterministicKeyGenerator.HalfSHA512 (prefixedBytesToHash);

			return hashOfBytes;

		}

		public byte [] GetTransactionHash ()
		{
			//Convert to bytes again

			var v = binSer.WriteBinaryObject (this);


			byte [] signedbytes = v;

			//Prefix bytesToSign with the magic sigining prefix (32bit) 'TXN\0'
			byte [] prefixedSignedBytes = new byte [signedbytes.Length + 4];

			prefixedSignedBytes [0] = (byte)'T';
			prefixedSignedBytes [1] = (byte)'X';
			prefixedSignedBytes [2] = (byte)'N';
			prefixedSignedBytes [3] = (byte)'\0'; // changed from numeric zero  // TODO THIS might be the tx sign bug !!!! change back?
			//prefixedSignedBytes [3] = (byte)0;
			System.Array.Copy (signedbytes, 0, prefixedSignedBytes, 4, signedbytes.Length);

			// Hash again, this wields the TransactionID
			byte [] hashOfTransaction = RippleDeterministicKeyGenerator.HalfSHA512 (prefixedSignedBytes);
			return hashOfTransaction;
		}

		public Object GetField (BinaryFieldType transactiontype) // why variable named transaction type O_o
		{
#pragma warning disable IDE0018 // Inline variable declaration
			Object obj = null;
#pragma warning restore IDE0018 // Inline variable declaration
			fields.TryGetValue (transactiontype, out obj);

			if (obj == null) {
				return null; // TODO refactor with Maybe object?
			}
			return obj;
		}

		public void PutField (BinaryFieldType field, Object value)
		{
#if DEBUG
			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append (clsstr);

			stringBuilder.Append (nameof (PutField));

			stringBuilder.Append (DebugRippleLibSharp.left_parentheses);

			stringBuilder.Append (nameof (BinaryFieldType));
			stringBuilder.Append (DebugRippleLibSharp.space_char);
			stringBuilder.Append (nameof (field));

			stringBuilder.Append (DebugRippleLibSharp.equals);

			stringBuilder.Append (field.ToString ());

			stringBuilder.Append (DebugRippleLibSharp.comma);

			stringBuilder.Append (typeof (object).ToString ());
			stringBuilder.Append (DebugRippleLibSharp.space_char);
			stringBuilder.Append (nameof (value));
			stringBuilder.Append (DebugRippleLibSharp.equals);
			stringBuilder.Append ((DebugRippleLibSharp.allowInsecureDebugging ? value.ToString () : "hidden"));
			stringBuilder.Append (DebugRippleLibSharp.right_parentheses);

			string method_sig =  stringBuilder.ToString(); 
			if (DebugRippleLibSharp.RippleBinaryObject) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			if (value == null) {
#if DEBUG
				if (DebugRippleLibSharp.RippleBinaryObject) {
					Logging.WriteLog(method_sig + "Can not set " + nameof(BinaryFieldType) + DebugRippleLibSharp.space_char + field.ToString() + "to null");
				}
#endif

				// 
				//throw new ArgumentNullException(field.ToString(), "Can not set BinaryFieldType " + field.ToString() + "to null");
			}
			fields.Add (field, value);
		}

		public RippleTransactionType GetTransactionType ()
		{
			Object txTypeObj = GetField (BinaryFieldType.TransactionType);
			if (txTypeObj == null) {
				throw new NullReferenceException ("No transaction type field found");
			}

			//return RippleTransactionType.fromType( (byte)txTypeObj );
			// I'm removing the cast to byte. I think it's actually a TRANSACTIONTYPE object

			return RippleTransactionType.FromType (txTypeObj);
		}


		public String ToJSONString ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (ToJSONString) + DebugRippleLibSharp.both_parentheses;
			if (DebugRippleLibSharp.RippleBinaryObject) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			String json = "{";

			int num = fields.Count;

			int count = 1; // We're starting at one because unlike arrays fields.Count returns the count and not count - 1

			if (num == 0) {
				return "{}";
			}

			// yahaaaa!!!! doing things the old fashioned way!

			foreach (KeyValuePair<BinaryFieldType, Object> field in fields) {
				BinaryType primative = field.Key.type;

				if (primative == null) {
#if DEBUG
					if (DebugRippleLibSharp.RippleBinaryObject) {
						Logging.WriteLog( nameof (BinaryType) + DebugRippleLibSharp.space_char + nameof (primative) + "should not be null");
					}
#endif

					throw new ArgumentNullException ();
				}


#if DEBUG
				if (DebugRippleLibSharp.RippleBinaryObject) {
					Logging.WriteLog( nameof(primative) + DebugRippleLibSharp.equals + primative.ToString());
				}
#endif


				if (field.Value == null) {
#if DEBUG
					if (DebugRippleLibSharp.RippleBinaryObject) {
						Logging.WriteLog(nameof (field.Value) + "is null");
					}
#endif
				} else {
#if DEBUG
					if (DebugRippleLibSharp.RippleBinaryObject) {
						//if () {
						//}
						Logging.WriteLog( nameof (field.Value) + " is " + field.Value.ToString());
					}
#endif
				}


				if (primative.typeCode == BinaryType.UINT8 || primative.typeCode == BinaryType.UINT16 ||
					primative.typeCode == BinaryType.UINT32 || primative.typeCode == BinaryType.UINT64) {

					json += "\"" + primative.ToString () + "\":" + field.Value.ToString ();


				} else {
					json += "\"" + primative.ToString () + "\":\"" + field.Value.ToString () + "\"";
				}

				if (count != num) {
					json += ",";
				}

				num++;
			}

			return json;

		}

		public RippleBinaryObject GetObjectSorted ()
		{
			List<BinaryFieldType> fie = GetSortedFields ();

			RippleBinaryObject sorted = new RippleBinaryObject ();


			foreach (BinaryFieldType bft in fie) {
#pragma warning disable IDE0018 // Inline variable declaration
				object o = null;
#pragma warning restore IDE0018 // Inline variable declaration
				bool success = this.fields.TryGetValue (bft, out o);
				if (!success || o == null) {
					throw new FieldAccessException ("Unknown error sorting " + nameof (BinaryFieldType));
				}

				sorted.PutField (bft, o);
			}

			return sorted;
		}

		public List<BinaryFieldType> GetSortedFields ()
		{

			List<BinaryFieldType> unsortedFields = new List<BinaryFieldType> (fields.Keys);
			/*
			unsortedFields.Sort();

			return (List<BinaryFieldType>)unsortedFields;
			*/

			List<BinaryFieldType> sortedFields = new List<BinaryFieldType> ();

			BinaryFieldType [] orderarray = BinaryFieldType.GetValues ();




			// Todo verify removal of items durring iteration is ok, may have to use an iterator type
			foreach (BinaryFieldType next in orderarray) {
				foreach (BinaryFieldType field in unsortedFields) {
					if (next.type == field.type && next.value == field.value) {
						sortedFields.Add (field);
						//unsortedFields.Remove (field);
					}
				}
			}

			/*
			if (unsortedFields.Count != 0) {
				throw new MissingFieldException("Class : RippleBinaryObject Method : getSortedField() : Some fields have remained unsorted");
			}
			*/



			return sortedFields;

		}

#if DEBUG
		private const string clsstr = nameof (RippleBinaryObject) + DebugRippleLibSharp.colon;
#endif

	}
}

