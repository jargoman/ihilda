using System;
using RippleLibSharp.Keys;
using Gtk;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AddressEntry : Gtk.Bin
	{
		public AddressEntry ()
		{
			this.Build ();

			this.comboboxentry.Entry.Changed += Comboboxentry_Entry_Changed;
		}

		public RippleAddress GetAddress () {

			string s = this.comboboxentry?.Entry?.Text;
			if (s == null) {
				return null;
			}

			RippleAddress ra = null;

			try {
				ra = new RippleAddress(s);
			}

			catch {
				ra = null;
			}

			/*
			finally {
				
			}
			*/

			return ra;
		}

		void Comboboxentry_Entry_Changed (object sender, EventArgs e)
		{


			RippleAddress ra = GetAddress ();

			Gdk.Color color = new Gdk.Color ();
			Gdk.Color.Parse ("red", ref color);
			if (ra == null) {
				this.comboboxentry?.Entry?.ModifyBg (Gtk.StateType.Normal, color);
			} else {
				this.comboboxentry?.Entry?.ModifyBg (Gtk.StateType.Normal);
			}
		}




		// 
	}
}

