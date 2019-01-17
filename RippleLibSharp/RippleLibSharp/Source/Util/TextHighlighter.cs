using System;
using System.Text;

namespace RippleLibSharp.Util
{
	public class TextHighlighter
	{
		/*
		public TextHighlighter ()
		{
		}
		*/

		public static string Highlight (string s)
		{
			string str = null;
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



			return  str;
		}

		public static string Highlight (StringBuilder s)
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




		public static string Highlightcolor {
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

		private static object lockObj = new object ();
		private static StringBuilder _stringBuilder = new StringBuilder();


#if DEBUG
		private const string clsstr = nameof (TextHighlighter) + DebugRippleLibSharp.colon;
#endif
	}
}

