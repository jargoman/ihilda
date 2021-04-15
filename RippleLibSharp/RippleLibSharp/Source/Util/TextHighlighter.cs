using System;
using System.Text;

namespace RippleLibSharp.Util
{
	public class TextHighlighter
	{

		public TextHighlighter ()
		{
		}

		public TextHighlighter (string color) {
			this.Highlightcolor = color;
		}

		public string Highlight (string s)
		{
			string str = null;

			if (Highlightcolor != null) {
				lock (lockObj) {

					StringBuilder stringBuilder = _stringBuilder;
					stringBuilder.Clear ();

					stringBuilder.Append ("<span foreground = ");
					stringBuilder.Append (Highlightcolor);
					stringBuilder.Append (">");
					stringBuilder.Append (s);
					stringBuilder.Append ("</span>");

					str = stringBuilder.ToString ();
					stringBuilder.Clear ();
				}
				
			}


			return  str;
		}

		public string Highlight (StringBuilder s)
		{

			string str = null;

			lock (lockObj) {

				StringBuilder stringBuilder = _stringBuilder;
				stringBuilder.Clear ();

				stringBuilder.Append ("<span foreground = ");
				stringBuilder.Append (Highlightcolor);
				stringBuilder.Append (">");
				stringBuilder.Append (s.ToString ());
				stringBuilder.Append ("</span>");

				str = stringBuilder.ToString ();
				stringBuilder.Clear ();
			}



			return str;
		}


		public static string MakeBold (string s)
		{
			StringBuilder stringBuilder = new StringBuilder ();

			stringBuilder.Append ("<b>");
			stringBuilder.Append (s);
			stringBuilder.Append ("</b>");

			return stringBuilder.ToString ();
		}

		public string Highlightcolor {
			get;
			set;
		}

		public static string BLACK = "\"black\"";
		public static string RED = "\"red\"";
		public static string LIGHT_RED = "\"#FFAABB\"";
		public static string BLUE = "\"blue\"";
		public static string GREEN = "\"green\"";
		public static string YELLOW = "\"yellow\"";
		public static string PURPLE = "\"purple\"";
		//public static string ORANGE = "\"orange\"";
		public static string ORANGE = "\"orange\"";
		public static string CHARTREUSE = "\"chartreuse\"";

		private readonly object lockObj = new object ();

		private readonly StringBuilder _stringBuilder = new StringBuilder();


#if DEBUG
		private const string clsstr = nameof (TextHighlighter) + DebugRippleLibSharp.colon;
#endif
	}
}

