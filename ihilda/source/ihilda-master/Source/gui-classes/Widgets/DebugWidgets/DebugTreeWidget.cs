using System;
using IhildaWallet.Util;
using Gtk;
using System.Reflection;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class DebugTreeWidget : Gtk.Bin
	{
		public DebugTreeWidget ()
		{
			this.Build ();

			_listStore = new ListStore (typeof (bool), typeof (string));

			Gtk.CellRendererToggle toggle = new CellRendererToggle {
				Activatable = true
			};

			toggle.Toggled += (object o, ToggledArgs args) => {
				//string s = args.Path;
				int index = Convert.ToInt32 (args.Path);

				if (_listStore.GetIterFromString (out TreeIter iter, args.Path)) {
					bool val = (bool)_listStore.GetValue (iter, 0);
					string name = (string)_listStore.GetValue (iter, 1);
					_listStore.SetValue (iter, 0, !val);

#if DEBUG
					DebugIhildaWallet.SetDebug (name, !val);
#endif

					//this._payments_tuple.Item2 [index] = !val;
				}
			};

			CellRendererText txtr = new CellRendererText {
				Editable = false
			};

			treeview1.AppendColumn ("Enabled", toggle, "active", 0);
			treeview1.AppendColumn ("Class", txtr, "markup", 1);

			InitDebugOptions ();
		}

		public void InitDebugOptions ()
		{
#if DEBUG

			FieldInfo[] fields = typeof (DebugIhildaWallet).GetFields ();
			_listStore.Clear ();
			foreach (FieldInfo f in fields) {

				if (f.FieldType != typeof (bool)) {
					continue;
				}
				string name = f.Name;
				object v = f.GetValue (null);
				bool val = (bool)v;
				_listStore.AppendValues (val, name );
			}

			this.treeview1.Model = _listStore;

#endif
		}

		ListStore _listStore = null;

	}
}
