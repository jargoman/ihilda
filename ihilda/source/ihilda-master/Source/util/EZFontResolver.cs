using System;
using System.Collections.Generic;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Fonts;

/*
namespace EZFontResolver1
{
	/// <summary>
	/// EZFontResolver is a generic font resolver for PDFsharp.
	/// It implement IFontResolver internally.
	/// To use it, just pass your fonts as filename or byte[]
	/// in calls to AddFont.
	/// </summary>
	public class EZFontResolver : IFontResolver
	{
		EZFontResolver ()
		{ }

		/// <summary>
		/// Gets the one and only EZFontResolver object.
		/// </summary>
		public static EZFontResolver Get {
			get { return _singleton ?? (_singleton = new EZFontResolver ()); }
		}
		private static EZFontResolver _singleton;

		/// <summary>
		/// Adds the font passing a filename.
		/// </summary>
		/// <param name="familyName">Name of the font family.</param>
		/// <param name="style">The style.</param>
		/// <param name="filename">The filename.</param>
		/// <param name="simulateBold">if set to <c>true</c> bold will be simulated.</param>
		/// <param name="simulateItalic">if set to <c>true</c> italic will be simulated.</param>
		/// <exception cref="Exception">
		/// Font file is too big.
		/// or
		/// Reading font file failed.
		/// </exception>
		public void AddFont (string familyName, XFontStyle style, string filename,
		    bool simulateBold = false, bool simulateItalic = false)
		{
			using (var fs = new FileStream (filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				var size = fs.Length;
				if (size > int.MaxValue)
					throw new Exception ("Font file is too big.");
				var length = (int)size;
				var data = new byte [length];
				var read = fs.Read (data, 0, length);
				if (length != read)
					throw new Exception ("Reading font file failed.");
				AddFont (familyName, style, data, simulateBold, simulateItalic);
			}
		}

		/// <summary>
		/// Adds the font passing a byte array containing the font.
		/// </summary>
		/// <param name="familyName">Name of the font family.</param>
		/// <param name="style">The style.</param>
		/// <param name="data">The data.</param>
		/// <param name="simulateBold">if set to <c>true</c> bold will be simulated.</param>
		/// <param name="simulateItalic">if set to <c>true</c> italic will be simulated.</param>
		public void AddFont (string familyName, XFontStyle style, byte [] data,
		    bool simulateBold = false, bool simulateItalic = false)
		{
			// Add the font as we get it.
			AddFontHelper (familyName, style, data, false, false);

			// If the font is not bold and bold simulation is requested, add that, too.
			if (simulateBold && (style & XFontStyle.Bold) == 0) {
				AddFontHelper (familyName, style | XFontStyle.Bold, data, true, false);
			}

			// Same for italic.
			if (simulateItalic && (style & XFontStyle.Italic) == 0) {
				AddFontHelper (familyName, style | XFontStyle.Italic, data, false, true);
			}

			// Same for bold and italic.
			if (simulateBold && (style & XFontStyle.Bold) == 0 &&
			    simulateItalic && (style & XFontStyle.Italic) == 0) {
				AddFontHelper (familyName, style | XFontStyle.BoldItalic, data, true, true);
			}
		}

		void AddFontHelper (string familyName, XFontStyle style, byte [] data, bool simulateBold, bool simulateItalic)
		{
			// Currently we do not need FamilyName and Style.
			// FaceName is a combination of FamilyName and Style.
			var fi = new EZFontInfo {
				//FamilyName = familyName,
				FaceName = familyName.ToLower (),
				//Style = style,
				Data = data,
				SimulateBold = simulateBold,
				SimulateItalic = simulateItalic
			};
			if ((style & XFontStyle.Bold) != 0) {
				// TODO Create helper method to prevent having duplicate string literals?
				fi.FaceName += "|b";
			}
			if ((style & XFontStyle.Italic) != 0) {
				fi.FaceName += "|i";
			}

			// Check if we already have this font.
			var test = GetFont (fi.FaceName);
			if (test != null)
				throw new Exception ("Font " + familyName + " with this style was already registered.");

			_fonts.Add (fi.FaceName.ToLower (), fi);
		}

		#region IFontResolver
		public FontResolverInfo ResolveTypeface (string familyName, bool isBold, bool isItalic)
		{
			string faceName = familyName.ToLower () +
			    (isBold ? "|b" : "") +
			    (isItalic ? "|i" : "");
			EZFontInfo item;
			if (_fonts.TryGetValue (faceName, out item)) {
				var result = new FontResolverInfo (item.FaceName, item.SimulateBold, item.SimulateItalic);
				return result;
			}
			return null;
		}

		public byte [] GetFont (string faceName)
		{
			EZFontInfo item;
			if (_fonts.TryGetValue (faceName, out item)) {
				return item.Data;
			}
			return null;
		}
		#endregion

		private readonly Dictionary<string, EZFontInfo> _fonts = new Dictionary<string, EZFontInfo> ();

		struct EZFontInfo
		{
			//internal string FamilyName;
			internal string FaceName;
			//internal XFontStyle Style;
			internal byte [] Data;
			internal bool SimulateBold;
			internal bool SimulateItalic;
		}
	}
}
*/
