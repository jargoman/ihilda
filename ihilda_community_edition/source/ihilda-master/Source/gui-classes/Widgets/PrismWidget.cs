using System;
using System.Text;
using Gtk;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PrismWidget : Gtk.Bin
	{
		public PrismWidget ()
		{
			this.Build ();

			InitPrism ();
		}

		public void InitPrism () {


			ListStore colorlist = new ListStore(typeof(string));
			ListStore animallist = new ListStore (typeof(string));
			ListStore elementlist = new ListStore (typeof(string));
			ListStore planetlist = new ListStore (typeof(string));
			ListStore cardlist = new ListStore (typeof(string));
			ListStore suitlist = new ListStore (typeof(string));

			string[] colors = Enum.GetNames (typeof(ColorCrypts));
			string[] animals = Enum.GetNames (typeof(Animals));
			string[] elements = Enum.GetNames (typeof(Elements));
			string[] planets = Enum.GetNames (typeof(Planet));
			string[] cards = Enum.GetNames (typeof(Cards));
			string[] suits = Enum.GetNames (typeof(Suits));


			foreach (string s in colors) {
				colorlist.AppendValues (s);
			}

			foreach (string s in animals) {
				animallist.AppendValues (s);
			}

			foreach (string s in elements) {
				elementlist.AppendValues (s);
			}
		
			foreach (string s in planets) {
				planetlist.AppendValues (s);
			}

			foreach (string s in cards) {
				cardlist.AppendValues (s);
			}

			foreach (string s in suits) {
				suitlist.AppendValues (s);
			}



			colorentry.Model = colorlist;
			colorentry.Changed += (object sender, EventArgs e) => {
				colorentry.ModifyBase (StateType.Normal);
			};
			animalentry.Model = animallist;
			animalentry.Changed += (object sender, EventArgs e) => {
				animalentry.ModifyBase (StateType.Normal);
			};
			elemententry.Model = elementlist;
			elemententry.Changed += (object sender, EventArgs e) => {
				elemententry.ModifyBase (StateType.Normal);
			};
			planetentry.Model = planetlist;
			planetentry.Changed += (object sender, EventArgs e) => {
				planetentry.ModifyBase (StateType.Normal);
			};
			cardentry.Model = cardlist;
			cardentry.Changed += (object sender, EventArgs e) => {
				cardentry.ModifyBase (StateType.Normal);
			};
			suitentry.Model = suitlist;
			suitentry.Changed += (object sender, EventArgs e) => {
				suitentry.ModifyBase (StateType.Normal);
			};

		}

		public void HideInfoBarLabels ()
		{
			this.label2.Markup = "";
			this.label2.Hide ();
		}

		public Tuple<ColorCrypts, Animals, Elements, Planet, Cards, Suits> CollectPrisms () {

#if DEBUG
			string method_sig = clsstr + nameof (CollectPrisms) + DebugRippleLibSharp.both_parentheses;
#endif

			string cl = colorentry.Entry.Text;
			string an = animalentry.Entry.Text;
			string el = elemententry.Entry.Text;
			string pl = planetentry.Entry.Text;
			string cr = cardentry.Entry.Text;
			string su = suitentry.Entry.Text;

			bool hasError = false;

			StringBuilder stringBuilder = new StringBuilder (Program.darkmode ? "<span fgcolor=\"FFAABB\">" : "<span fgcolor=\"red\">");
			Gdk.Color orchid = new Gdk.Color (218, 112, 214);
			ColorCrypts color = default(ColorCrypts);
			try {
				color = (ColorCrypts)Enum.Parse (typeof (ColorCrypts), cl);
			} catch (Exception e) {

#if DEBUG
				if (DebugIhildaWallet.PrismWidget) {
					Logging.ReportException (method_sig, e);
				}
#endif
				hasError = true;
				stringBuilder.Append ("Invalid Color ");
				stringBuilder.AppendLine (cl);

				colorentry.ModifyBase (StateType.Normal, orchid);

			}

			Animals animal = default (Animals);
			try {
				animal = (Animals)Enum.Parse (typeof (Animals), an);
			} catch (Exception e) {


#if DEBUG
				if (DebugIhildaWallet.PrismWidget) {
					Logging.ReportException (method_sig, e);
				}
#endif



				hasError = true;
				stringBuilder.Append ("Invalid Animal ");
				stringBuilder.AppendLine (an);

				animalentry.ModifyBase (StateType.Normal, orchid);
			}

			Elements elements = default (Elements);

			try {
				elements = (Elements)Enum.Parse (typeof (Elements), el);
			} catch (Exception e) {

#if DEBUG
				if (DebugIhildaWallet.PrismWidget) {
					Logging.ReportException (method_sig, e);
				}
#endif


				hasError = true;
				stringBuilder.Append ("Invalid Element ");
				stringBuilder.AppendLine (el);

				elemententry.ModifyBase (StateType.Normal, orchid);
			}

			Planet planet = default (Planet);

			try {
				planet = (Planet)Enum.Parse (typeof (Planet), pl);
			} catch (Exception e) {

#if DEBUG
				if (DebugIhildaWallet.PrismWidget) {
					Logging.ReportException (method_sig, e);
				}
#endif


				hasError = true;
				stringBuilder.Append ("Invalid Planet ");
				stringBuilder.AppendLine (pl);

				planetentry.ModifyBase (StateType.Normal, orchid);
			}

			Cards card = default (Cards);
			try {
				card = (Cards)Enum.Parse (typeof (Cards), cr);
			} catch (Exception e) {

#if DEBUG
				if (DebugIhildaWallet.PrismWidget) {
					Logging.ReportException (method_sig, e);
				}
#endif


				hasError = true;
				stringBuilder.Append ("Invalid Card Rank ");
				stringBuilder.AppendLine (cr);

				cardentry.ModifyBase (StateType.Normal, orchid);
			}

			Suits suit = default (Suits);
			try {
				suit = (Suits)Enum.Parse (typeof (Suits), su);
			} catch (Exception e) {

#if DEBUG
				if (DebugIhildaWallet.PrismWidget) {
					Logging.ReportException (method_sig, e);
				}
#endif
				hasError = true;
				stringBuilder.Append ("Invalid Card Suit ");
				stringBuilder.AppendLine (su);

				suitentry.ModifyBase (StateType.Normal, orchid);
			}

			if (hasError) {
				stringBuilder.Append ("</span>");
				label2.Markup = stringBuilder.ToString ();
				label2.Show ();
				return null;
			}

			var v = new Tuple<ColorCrypts, Animals, Elements, Planet, Cards, Suits> (
				color,
				animal,
				elements,
				planet,
				card,
				suit
			);

			return v;
		}


#if DEBUG
		private const string clsstr = nameof (PrismWidget) + DebugRippleLibSharp.colon;
#endif
	}
}

