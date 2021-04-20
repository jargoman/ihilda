using System;
namespace IhildaWallet
{
	public partial class SignOptionsDialog : Gtk.Dialog
	{
		public SignOptionsDialog ()
		{
			this.Build ();
			if (this.signoptionswidget1 == null) {
				this.signoptionswidget1 = new SignOptionsWidget ();
				signoptionswidget1.Show ();
				vbox2.Add ( signoptionswidget1 );
			}
		}

		public void ProcessSignOptionsWidget ()
		{
			// TODO return saved SignOption?
			this.signoptionswidget1.ProcessSignOptions ();
		}
	}
}
