using System;
using RippleLibSharp.Binary;
using RippleLibSharp.Transactions;

namespace IhildaWallet
{
	public partial class MemoCreateDialog : Gtk.Dialog
	{
		public MemoCreateDialog ()
		{
			this.Build ();
		}


		public SelectableMemoIndice GetMemoIndice ()
		{

			string type = Base58.StringToHex (entry1.Text);
			string format = Base58.StringToHex (entry2.Text);
			string data = Base58.StringToHex (textview2.Buffer.Text);

			SelectableMemoIndice indice = new SelectableMemoIndice {
				Memo = new RippleMemo () {
					MemoType = entry1.Text,
		    			MemoFormat = entry2.Text,
					MemoData = textview2.Buffer.Text
				}
			};

			return indice;

		}

		public class SelectableMemoIndice : MemoIndice
		{
			public bool IsSelected = false;


		}
	}
}
