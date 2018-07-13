using System;

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
			return "<span foreground=" + Highlightcolor + ">" + s + "</span>";
		}



		public static string Highlightcolor {
			get;
			set;
		}


		public static string RED = "\"red\"";
		public static string BLUE = "\"blue\"";
		public static string GREEN = "\"green\"";
		public static string YELLOW = "\"yellow\"";
		public static string PURPLE = "\"purple\"";
		public string ORANGE = "\"orange\"";

#if DEBUG
		private const string clsstr = nameof (TextHighlighter) + DebugRippleLibSharp.colon;
#endif
	}
}

