using System;
using Gtk;
using RippleLibSharp.Source.Mnemonics;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public partial class WordListInnitiateDialog : Gtk.Dialog
	{
		public WordListInnitiateDialog ()
		{
			this.Build ();

			this.comboboxentry1.AppendText ("m/44'/144'/0'/0");

			this.textview2.Buffer.Changed += Buffer_Changed;

			this.comboboxentry1.Changed += Comboboxentry1_Changed;

			this.entry1.Changed += Entry1_Changed;

		}

		void Entry1_Changed (object sender, EventArgs e)
		{
			InputChanged ();
		}


		void Comboboxentry1_Changed (object sender, EventArgs e)
		{
			InputChanged ();
		}




		void Buffer_Changed (object sender, EventArgs e)
		{

			InputChanged ();

			
		}


		private void InputChanged ()
		{
			TextHighlighter highlighter = new TextHighlighter ();

			string input = this.textview2.Buffer.Text;

			if (string.IsNullOrWhiteSpace (input)) {
				infolabel.Markup = "Please enter your mnemonic word list";
				return;
			}

			input = input.Trim ();

			var words = input.Split (new char [] { ' ', ',', '_', '\n' });

			var vald = MnemonicWordList.IsValid (words);

			if (!vald.IsValid) {
				highlighter.Highlightcolor = TextHighlighter.RED;

				infolabel.Markup = highlighter.Highlight (vald.Message);
				return;
				
			}


			string dpath = comboboxentry1.Entry.Text;

			if (string.IsNullOrWhiteSpace(dpath)) {

				highlighter.Highlightcolor = TextHighlighter.GREEN;

				infolabel.Markup = highlighter.Highlight (vald.Message) + " Enter the derivation path";

				return;
			}

			bool pathvald = MnemonicWordList.ValidateKeyPath (dpath);

			if (!pathvald) {
				highlighter.Highlightcolor = TextHighlighter.GREEN;
				string yay = highlighter.Highlight (vald.Message) + ". ";

				highlighter.Highlightcolor = TextHighlighter.RED;
				string nay = highlighter.Highlight ("invalid path");

				return;
			}

			buttonOk.Sensitive = true;


			highlighter.Highlightcolor = TextHighlighter.GREEN;
			infolabel.Markup = highlighter.Highlight (vald.Message + ". " + "valid path");


			mnemonicWordList = new MnemonicWordList (words) {
				keyPath = dpath
			};

			string pass = entry1.Text;
			if (string.IsNullOrEmpty (pass)) {
				mnemonicWordList.password = null;
			} else if (string.IsNullOrWhiteSpace(pass)) {
				highlighter.Highlightcolor = TextHighlighter.RED;
				infolabel.Markup = highlighter.Highlight ("Warning password consisting of only whitespace");
			} else {

				mnemonicWordList.password = pass;
			}
		}


		public MnemonicWordList mnemonicWordList = null;

		public static MnemonicWordList DoDialog ()
		{
			using (WordListInnitiateDialog dialog = new WordListInnitiateDialog ()) {

				ResponseType res = (Gtk.ResponseType)dialog.Run ();
				dialog.Hide ();
				dialog.Destroy ();

				if (res == ResponseType.Ok) {
					return dialog.mnemonicWordList;
				}

			}

			return null;
		}


		
	}
}
