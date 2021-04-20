using System;

namespace IhildaWallet
{
	public partial class NewButtonDialog : Gtk.Dialog
	{
		public NewButtonDialog ()
		{
			this.Build ();

			this.Modal = true;

			this.secretRadioButton.Toggled += (sender, e) => selection = NewOption.SECRET;
			this.randomRadioButton.Toggled += (sender, e) => selection = NewOption.RANDOM;
			this.ScriptRadioButton.Toggled += (sender, e) => selection = NewOption.SCRIPT;
			this.fileRadioButton.Toggled += (sender, e) => selection = NewOption.FILE;
			this.privateRadioButton.Toggled += (sender, e) => selection = NewOption.PRIVATE;
			this.wordListRadioButton.Toggled += (sender, e) => selection = NewOption.WORDS;
			

			if (ProgramVariables.showPopUps) {
				this.secretRadioButton.TooltipMarkup = "import an exing ripple secret";
				this.randomRadioButton.TooltipMarkup = "generate a random key pair using collected entropy";
				this.ScriptRadioButton.TooltipMarkup = "brute force a vanity wallet using a script";
				this.fileRadioButton.TooltipMarkup = "import a .ice wallet file";
				this.privateRadioButton.TooltipMarkup = "import an existing ripple private key";
				this.wordListRadioButton.TooltipMarkup = "import a wallet using a word list";

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
			FILE,
	    		PRIVATE,
			WORDS
		}
	}
}

