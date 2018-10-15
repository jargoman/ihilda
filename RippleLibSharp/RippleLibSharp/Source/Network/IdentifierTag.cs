using System;

using Codeplex.Data;
using System.Text;
using RippleLibSharp.Util;
namespace RippleLibSharp.Network
{
	public class IdentifierTag
	{
		public IdentifierTag ()
		{
		}

		public int IdentificationNumber {
			get;
			set;
		}

		public string IdentificationMessage {
			get;
			set;
		}

		public T GetMessageAsObject <T> ()
		{
			T ret = DynamicJson.Parse (IdentificationMessage);

			return ret;

		}

		public virtual string ToJsonString ()
		{
			string str = DynamicJson.Serialize (this);

			return str;
		}

		public override string ToString ()
		{
			StringBuilder stringBuilder = new StringBuilder (nameof (IdentificationNumber));

			stringBuilder.Append (" : ");
			stringBuilder.AppendLine (IdentificationNumber.ToString ());
			stringBuilder.Append (nameof (IdentificationMessage));
			stringBuilder.Append (" : ");
			stringBuilder.Append (IdentificationMessage?.ToString () ?? "null");

			return stringBuilder.ToString (); 
		}

	}

	public interface Identifiable
	{
		IdentifierTag id { get; set; }
	}
}
