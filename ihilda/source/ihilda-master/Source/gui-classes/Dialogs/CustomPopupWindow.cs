using System;
using Gtk;

namespace IhildaWallet
{
	public partial class CustomPopupWindow : Gtk.Window
	{
		public CustomPopupWindow () :
		base (Gtk.WindowType.Toplevel)
		{

			//this.TypeHint = Gdk.WindowTypeHint.;
			this.Build ();
			//this.label1.ModifyBase( Gtk.StateType.Normal, new Gdk.Color(255,0,0));
			//this.label1.ModifyBg (Gtk.StateType.Normal, new Gdk.Color(255,0,0));
			//this.Decorated = false;

			this.TypeHint = Gdk.WindowTypeHint.Notification;

			//Menu menu = new Menu ();



			//menu.Add (mi);


			//label1.UseMarkup = true;
		}

		public void SetMessage ( string message ) {
			this.label1.Markup = message;

		}


	}
}

