using System;
using Gtk;

namespace IhildaWallet
{
	public partial class EncryptionTypeDialog : Gtk.Dialog
	{
		public EncryptionTypeDialog ()
		{
			this.Build ();

			this.Setcombo ();

			ClearInfoBar ();

			this.label1.Markup = "<span>Please choose an encryption type</span>";
			this.label1.Show ();

		}

		public void ClearInfoBar ()
		{
			//this.label1.UseMarkup = true;
			//this.label1.Markup = "";
			this.label1.Markup = "<span>Please choose an encryption type</span>";
			this.label1.Show ();
			//this.label1.Hide ();
		}

		public void Setcombo () {

			string[] values = Enum.GetNames (typeof(EncryptionType));

			ListStore store = new ListStore (typeof(string));

			for (int i = 1; i < values.Length; i++) {
				
				store.AppendValues (values[i]);
			}


			this.comboboxentry1.Model = store;

		}

		public EncryptionType GetComboBoxChoice () {
			string boxtext = this.comboboxentry1.Entry.Text;
			if (string.IsNullOrWhiteSpace (boxtext) || boxtext == nameof ( EncryptionType.Plaintext )) {

				if (!ProgramVariables.darkmode) {
					this.label1.Markup = "<span fgcolor=\"red\">Please choose an encryption type</span>";
				} else {
					this.label1.Markup = "<span fgcolor=\"#FFAABB\">Please choose an encryption type</span>";
				}
				this.label1.Show ();
				return EncryptionType.None;

			}
			EncryptionType et = (EncryptionType) Enum.Parse ( typeof(EncryptionType), boxtext );
			return et;
		}
	}
}

