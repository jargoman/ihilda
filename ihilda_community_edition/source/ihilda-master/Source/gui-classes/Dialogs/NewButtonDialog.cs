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

			if (Program.showPopUps) {
				this.radiobutton1.TooltipMarkup = "import an exing ripple secret";
				this.radiobutton2.TooltipMarkup = "generate a random key pair using collected entropy";
				this.radiobutton3.TooltipMarkup = "brute force a vanity wallet using a script";
				this.radiobutton4.TooltipMarkup = "import a .ice wallet file";

				label2.TooltipMarkup = "choose an option";
			}
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

