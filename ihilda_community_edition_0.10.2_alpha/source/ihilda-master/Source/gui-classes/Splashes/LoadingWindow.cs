using System;

namespace IhildaWallet.Splashes
{
	public partial class LoadingWindow : Gtk.Window
	{
		public LoadingWindow () : 
				base(Gtk.WindowType.Toplevel)
		{
			this.Build ();
			this.SetPosition (Gtk.WindowPosition.Center);

			this.cancelbutton.Clicked += (object sender, EventArgs e) => {
				this.isCanceled = true;
			};


			this.eventbox1.ModifyBg (Gtk.StateType.Normal, new Gdk.Color (0,0,0));

			image25.Animation = SpinWait.pa;
		}

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public bool isCanceled = false;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

	}
}

