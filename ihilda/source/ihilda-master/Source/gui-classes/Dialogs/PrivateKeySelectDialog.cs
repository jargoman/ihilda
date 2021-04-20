using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using RippleLibSharp.Keys;
using RippleLibSharp.Source.Mnemonics;

namespace IhildaWallet
{
	public partial class PrivateKeySelectDialog : Gtk.Dialog
	{
		public PrivateKeySelectDialog (MnemonicWordList mnemonicWordList)
		{
			this.Build ();


			nextButton.Clicked += delegate {

				start_account += 10;

				if (start_account > 0) {
					previousButton.Sensitive = true;
				}

				RefreshUI ();
			};

			previousButton.Clicked += delegate {
				if (start_account > 1) {
					start_account -= 10;
				}

				if (start_account == 0) {
					previousButton.Sensitive = false;
				}

				RefreshUI ();
			};

			this.mnemonicWordList = mnemonicWordList;

			start_account = 0;
			previousButton.Sensitive = false;

			listStore = new ListStore (typeof (string), /*typeof(bool),*/ typeof (string), typeof (string));

			treeview1.AppendColumn ("Account", new CellRendererText (), "text", 0);
			//treeview1.AppendColumn ("Select", new CellRendererToggle(), "radio");

			treeview1.AppendColumn ("Address", new CellRendererText (), "text", 1);

			treeview1.AppendColumn ("Private Key", new CellRendererText (), "text", 2);


			treeview1.Model = listStore;

			RefreshUI ();
		}

		ListStore listStore;

		public void RefreshUI ()
		{

			int start = start_account;

			IEnumerable<uint> range = from int i in Enumerable.Range (start, 10) select (uint)i;

			IEnumerable<RipplePrivateKey> accs = mnemonicWordList.GetAccounts (range.ToArray());

			listStore.Clear ();
			for (int i = 0; i < accs.Count (); i++) {

				var key = accs.ElementAt (i);

				listStore.AppendValues (
					(i + start_account).ToString (),
					key.GetPublicKey ().GetAddress ().GetHumanReadableIdentifier (),
					key.GetHumanReadableIdentifier ()

				);

			}

			treeview1.Model = listStore;
		}

		public RipplePrivateKey GetSelectedPrivateKey () {
			var s = treeview1.Selection;
			if (s.GetSelected (out TreeIter iter)) {

				var p = listStore.GetPath (iter);
				var i = p.Indices;


				var pr = mnemonicWordList.GetAccount ((uint)(i [0] + start_account  ));

				return pr;

			}

			return null;
		}



		public static RipplePrivateKey DoDialog (MnemonicWordList mnemonic)
		{
			using (PrivateKeySelectDialog dialog = new PrivateKeySelectDialog(mnemonic)) {

				ResponseType response = (ResponseType)dialog.Run ();

				var pk = dialog.GetSelectedPrivateKey ();

				dialog.Hide ();
				dialog.Destroy ();

				if (response == ResponseType.Ok) {

					return pk;
				}
			}

			return null;
		}

		private MnemonicWordList mnemonicWordList;

		private int start_account;
	}
}
