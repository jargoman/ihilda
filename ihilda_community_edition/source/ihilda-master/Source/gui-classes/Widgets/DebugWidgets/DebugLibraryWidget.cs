using System;
using System.Reflection;
using Gtk;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class DebugLibraryWidget : Gtk.Bin
	{
		public DebugLibraryWidget ()
		{
			this.Build ();


#if DEBUG
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

					string debAllow = nameof (DebugRippleLibSharp.allowInsecureDebugging);
					if (name == debAllow && val) {
						bool answer = AreYouSure.AskQuestion ("WARNING", "Warning setting " + debAllow + " to true gives the logging and debugging system permission to print or log an accounts secret. Are you sure you want to set this value to true? \n");
						if (!answer) {
							return;
						}
					}

					_listStore.SetValue (iter, 0, !val);
					DebugIhildaWallet.SetDebug (name, !val);

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

		ListStore _listStore = null;

		public void InitDebugOptions ()
		{

			TextHighlighter highlighter = new TextHighlighter ();

			FieldInfo [] fields = typeof (DebugRippleLibSharp).GetFields ();
			_listStore.Clear ();
			foreach (FieldInfo f in fields) {

				if (f.FieldType != typeof (bool)) {
					continue;
				}
				string name = f.Name;
				if (nameof (DebugRippleLibSharp.allowInsecureDebugging) == name) {
					highlighter.Highlightcolor = TextHighlighter.RED;
					name = highlighter.Highlight (name);
				}
				object v = f.GetValue (null);
				bool val = (bool)v;
				_listStore.AppendValues (val, name);
			}

			this.treeview1.Model = _listStore;

#endif
		}


	}
}
