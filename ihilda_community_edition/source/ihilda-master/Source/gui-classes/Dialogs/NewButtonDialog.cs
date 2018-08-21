using System;

namespace IhildaWallet
{
	public partial class NewButtonDialog : Gtk.Dialog
	{
		public NewButtonDialog ()
		{
			this.Build ();

			this.Modal = true;

			this.radiobutton1.Toggled += (sender, e) => selection = NewOption.SECRET;
			this.radiobutton2.Toggled += (sender, e) => selection = NewOption.RANDOM;
			this.radiobutton3.Toggled += (sender, e) => selection = NewOption.SCRIPT;
			this.radiobutton4.Toggled += (sender, e) => selection = NewOption.FILE;
		}

		public NewOption GetSelection ()
		{
			return selection;
		}

		private NewOption selection = NewOption.SECRET;

		public enum NewOption {
			RANDOM,
			SECRET,
			SCRIPT,
			FILE
		}
	}
}

