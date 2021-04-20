using System;

namespace IhildaWallet
{
	public partial class SpinWait : Gtk.Window
	{
		public SpinWait () :
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			this.image1.Animation = pa;
		}


		public static Gdk.PixbufAnimation pa = new Gdk.PixbufAnimation(System.Reflection.Assembly.Load(ProgramVariables.appname), nameof (IhildaWallet) + ".Images.ajax-loader-200x200.gif");
	}
}

