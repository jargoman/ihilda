using System;
using System.Text;

namespace IhildaWallet
{
	public static class MarkAsCommand
	{
		public const string ADD = "+=";
		public const string SUBTRACT = "-=";
		public const string MULTIPLY = "*=";
		public const string DIVIDE = "/=";
		public const string ADDONE = "++";
		public const string SUBTRACTONE = "--";

		public static string DoNextMark (string mark, string markAs)
		{
			if (markAs == "*") {
				return mark;
			}

			if (string.IsNullOrWhiteSpace(markAs)) {
				return "";
			}

			string inv = "Invalid ";
			string expctd = ". Expected format ";
			StringBuilder stringBuilder = new StringBuilder ();

			stringBuilder.Append (inv);
			stringBuilder.Append ("markas");
			stringBuilder.Append (expctd);

			string invmarkas = stringBuilder.ToString ();

			stringBuilder.Clear ();

			stringBuilder.Append (inv);
			stringBuilder.Append ("mark");
			stringBuilder.Append (expctd);

			string invmark = stringBuilder.ToString ();

			string num = "{number}";



			if (markAs.StartsWith (MarkAsCommand.ADD)) {

				string param1 = mark;
				bool success = int.TryParse (param1, out int result1);
				if (!success) {

					stringBuilder.Clear ();
					stringBuilder.Append (invmark);
					stringBuilder.Append (MarkAsCommand.ADD);
					stringBuilder.Append (num);
					string message = stringBuilder.ToString ();
					stringBuilder.Clear ();
					throw new InvalidCastException (message);

				}


				string param2 = markAs.Substring (2);
				success = int.TryParse (param2, out int result2);
				if (!success) {

					stringBuilder.Clear ();
					stringBuilder.Append (invmarkas);
					stringBuilder.Append (MarkAsCommand.ADD);
					stringBuilder.Append (num);
					string message = stringBuilder.ToString ();
					stringBuilder.Clear ();
					throw new InvalidCastException (message);
				}

				return (result1 + result2).ToString();


			} else if (markAs.StartsWith (MarkAsCommand.ADDONE)) {

				string param1 = mark;
				bool success = int.TryParse (param1, out int result1);
				if (!success) {

					stringBuilder.Clear ();
					stringBuilder.Append (invmark);
					stringBuilder.Append (MarkAsCommand.ADDONE);
					stringBuilder.Append (num);
					string message = stringBuilder.ToString ();
					stringBuilder.Clear ();
					throw new InvalidCastException (message);

				}

				return (++result1).ToString();

			} else if (markAs.StartsWith (MarkAsCommand.DIVIDE)) {
				
				string param1 = mark;
				bool success = int.TryParse (param1, out int result1);
				if (!success) {

					stringBuilder.Clear ();
					stringBuilder.Append (invmark);
					stringBuilder.Append (MarkAsCommand.DIVIDE);
					stringBuilder.Append (num);
					string message = stringBuilder.ToString ();
					stringBuilder.Clear ();
					throw new InvalidCastException (message);

				}


				string param2 = markAs.Substring (2);
				success = int.TryParse (param2, out int result2);
				if (!success) {

					stringBuilder.Clear ();
					stringBuilder.Append (invmarkas);
					stringBuilder.Append (MarkAsCommand.DIVIDE);
					stringBuilder.Append (num);
					string message = stringBuilder.ToString ();
					stringBuilder.Clear ();
					throw new InvalidCastException (message);
				}

				return (result1 / result2).ToString();

			} else if (markAs.StartsWith (MarkAsCommand.MULTIPLY)) {

				string param1 = mark;
				bool success = int.TryParse (param1, out int result1);
				if (!success) {

					stringBuilder.Clear ();
					stringBuilder.Append (invmark);
					stringBuilder.Append (MarkAsCommand.MULTIPLY);
					stringBuilder.Append (num);
					string message = stringBuilder.ToString ();
					stringBuilder.Clear ();
					throw new InvalidCastException (message);

				}


				string param2 = markAs.Substring (2);
				success = int.TryParse (param2, out int result2);
				if (!success) {

					stringBuilder.Clear ();
					stringBuilder.Append (invmarkas);
					stringBuilder.Append (MarkAsCommand.MULTIPLY);
					stringBuilder.Append (num);
					string message = stringBuilder.ToString ();
					stringBuilder.Clear ();
					throw new InvalidCastException (message);
				}

				return (result1 * result2).ToString();


			} else if (markAs.StartsWith (MarkAsCommand.SUBTRACT)) {

				string param1 = mark;
				bool success = int.TryParse (param1, out int result1);
				if (!success) {

					stringBuilder.Clear ();
					stringBuilder.Append (invmark);
					stringBuilder.Append (MarkAsCommand.SUBTRACT);
					stringBuilder.Append (num);
					string message = stringBuilder.ToString ();
					stringBuilder.Clear ();
					throw new InvalidCastException (message);

				}


				string param2 = markAs.Substring (2);
				success = int.TryParse (param2, out int result2);
				if (!success) {

					stringBuilder.Clear ();
					stringBuilder.Append (invmarkas);
					stringBuilder.Append (MarkAsCommand.SUBTRACT);
					stringBuilder.Append (num);
					string message = stringBuilder.ToString ();
					stringBuilder.Clear ();
					throw new InvalidCastException (message);
				}

				return (result1 - result2).ToString();

			} else if (markAs.StartsWith (MarkAsCommand.SUBTRACTONE)) {

				string param1 = mark;
				bool success = int.TryParse (param1, out int result1);
				if (!success) {

					stringBuilder.Clear ();
					stringBuilder.Append (invmark);
					stringBuilder.Append (MarkAsCommand.SUBTRACTONE);
					stringBuilder.Append (num);
					string message = stringBuilder.ToString ();
					stringBuilder.Clear ();
					throw new InvalidCastException (message);

				}

				return (--result1).ToString();
			}


			return markAs;

		}

	}

	public static class MatchMarkModifer
	{
		public const string EQUALS = "=";
		public const string GREATER = ">";
		public const string SMALLER = "<";
		public const string NOT = "!";
	}
}
