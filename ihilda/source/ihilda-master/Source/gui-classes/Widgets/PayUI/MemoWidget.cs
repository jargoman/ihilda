using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using RippleLibSharp.Transactions;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class MemoWidget : Gtk.Bin
	{
		public MemoWidget ()
		{
			this.Build ();

			CellRendererToggle rendererToggle = new CellRendererToggle () {
				Activatable = true
			};


			rendererToggle.Toggled += RendererToggle_Toggled;

			CellRendererText cellRendererText = new CellRendererText ();

			treeview1.AppendColumn ("#", cellRendererText, "text", 0);
			treeview1.AppendColumn ("Enabled", rendererToggle, "active", 1);
			treeview1.AppendColumn ("MemoType", cellRendererText, "text", 2);
			treeview1.AppendColumn ("MemoFormat", cellRendererText, "text", 3);
			treeview1.AppendColumn ("MemoData", cellRendererText, "text", 4);

			ListStore = new ListStore (
					typeof (string),
					typeof (bool),
					typeof (string),
		    			typeof (string),
					typeof (string)
				);


			this.addmemobutton.Clicked += (object sender, EventArgs e) => {

				SelectableMemoIndice createdMemo = null;
				using (MemoCreateDialog memoCreateDialog = new MemoCreateDialog ()) {
					try {
						ResponseType resp = (ResponseType)memoCreateDialog.Run ();


						if (resp != ResponseType.Ok) {

							return;
						}
						createdMemo = memoCreateDialog.GetMemoIndice ();
						this.AddMemo (createdMemo);
					} catch (Exception ee) {
						throw ee;
					} finally {
						memoCreateDialog?.Destroy ();
					}
				}




			};

			clearmemobutton.Clicked += (object sender, EventArgs e) => {
				ListStore.Clear ();

				Memos = null;

				this.SetMemos (Memos);

			};

			var memo = Program.GetClientMemo ();
			this.AddMemo (memo);
		}


		Gtk.ListStore ListStore {
			get;
			set;
		}

		private IEnumerable<SelectableMemoIndice> Memos {
			get;
			set;
		}


		void RendererToggle_Toggled (object o, ToggledArgs args)
		{

			int index = Convert.ToInt32 (args.Path);

			if (ListStore.GetIterFromString (out TreeIter iter, args.Path)) {
				bool val = (bool)ListStore.GetValue (iter, 1);
				ListStore.SetValue (iter, 1, !val);


				this.Memos.ElementAt (index).IsSelected = !val;

			}

		}


		public void SetMemos (IEnumerable<SelectableMemoIndice> Memos)
		{

			Gtk.Application.Invoke (
				delegate {
					ListStore.Clear ();

					int i = 0;
					foreach (SelectableMemoIndice memoIndice in Memos) {
						ListStore.AppendValues (
							i++.ToString (),
							memoIndice.IsSelected,
							memoIndice?.GetMemoTypeAscii (),
							memoIndice?.GetMemoFormatAscii (),
							memoIndice?.GetMemoDataAscii ()
						);
					}

					
					this.treeview1.Model = ListStore;

				}
			);

			this.Memos = Memos;

		}

		public void AddMemo (SelectableMemoIndice indice)
		{
			List<SelectableMemoIndice> memoIndices = Memos?.ToList () ?? new List<SelectableMemoIndice> ();
			indice.IsSelected = true;
			memoIndices.Add (indice);

			SetMemos (memoIndices);

		}

		public bool HasSelectedMemos ()
		{
			if (Memos == null) return false;

			return Memos.Any ( x => x.IsSelected);
			
	    		 
		}

		public IEnumerable<MemoIndice> GetSelectedMemos ()
		{
			IEnumerable<MemoIndice> mem = Memos?
				.Where ((SelectableMemoIndice arg) => arg.IsSelected)
				.Select ((SelectableMemoIndice arg) => {
					return new MemoIndice {
						Memo = new RippleMemo () {
							MemoType = arg.Memo.MemoType,
							MemoFormat = arg.Memo.MemoFormat,
							MemoData = arg.Memo.MemoData
						}
					};
				});

			return mem;
		}

	}
}
