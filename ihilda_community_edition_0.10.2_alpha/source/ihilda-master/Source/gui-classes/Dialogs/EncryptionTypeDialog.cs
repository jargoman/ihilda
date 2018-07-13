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
		}

		public void Setcombo () {

			string[] values = Enum.GetNames (typeof(EncryptionType));

			ListStore store = new ListStore (typeof(string));

			foreach (var v in values) {
				store.AppendValues (v);
			}


			this.comboboxentry1.Model = store;

		}

		public EncryptionType GetComboBoxChoice () {
			string boxtext = this.comboboxentry1.Entry.Text;

			EncryptionType et = (EncryptionType)Enum.Parse (typeof(EncryptionType), boxtext);
			return et;
		}
	}
}

