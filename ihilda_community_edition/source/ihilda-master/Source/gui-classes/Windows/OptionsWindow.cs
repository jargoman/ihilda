using System;

namespace IhildaWallet
{
	public partial class OptionsWindow : Gtk.Window
	{
		public OptionsWindow () : 
				base(Gtk.WindowType.Toplevel)
		{
			
			this.Build ();

			if (this.optionswidget2 == null) {
				this.optionswidget2 = new OptionsWidget ();

				this.optionswidget2.Show ();

				this.Add (optionswidget2);
			}

		}

		public void GotoFee ()
		{
			this.optionswidget2.GotoFee ();
		}


	}
}

