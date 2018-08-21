using System;
using Gtk;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PrismWidget : Gtk.Bin
	{
		public PrismWidget ()
		{
			this.Build ();

			initPrism ();
		}

		public void initPrism () {


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
			animalentry.Model = animallist;
			elemententry.Model = elementlist;
			planetentry.Model = planetlist;
			cardentry.Model = cardlist;
			suitentry.Model = suitlist;

		}


		public Tuple<ColorCrypts, Animals, Elements, Planet, Cards, Suits> collectPrisms () {

			string cl = colorentry.Entry.Text;
			string an = animalentry.Entry.Text;
			string el = elemententry.Entry.Text;
			string pl = planetentry.Entry.Text;
			string cr = cardentry.Entry.Text;
			string su = suitentry.Entry.Text;


			ColorCrypts color = (ColorCrypts)Enum.Parse (typeof(ColorCrypts), cl);
			Animals animal = (Animals)Enum.Parse (typeof(Animals), an); 
			Elements elements = (Elements)Enum.Parse (typeof(Elements), el); 
			Planet planet = (Planet) Enum.Parse (typeof(Planet), pl);
			Cards card = (Cards)Enum.Parse (typeof(Cards), cr); 
			Suits suit = (Suits)Enum.Parse (typeof(Suits), su);

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

	}
}

