using System;
using System.Linq;
using System.Collections.Generic;
using Gtk;

namespace IhildaWallet
{
	public partial class WebLinksWindow : Gtk.Window
	{
		public WebLinksWindow ( IEnumerable<WebLinkItem> linkItems) :
				base (Gtk.WindowType.Toplevel)
		{

			
			this.Build ();

			this._LinkItems = linkItems.ToArray ();

			var listStore = new ListStore (
				typeof (string), // title
				typeof (string) // link
			);

			CellRendererText txtr = new CellRendererText {
				Editable = false
			};

			treeview1.AppendColumn ("Youtuber Name", txtr, "markup", 0);
			treeview1.AppendColumn ("Weblink", txtr, "text", 1);

			foreach (var link in linkItems) {

				listStore.AppendValues (link.Title, link.Link);
			}

			this.treeview1.Model = listStore;

			this.treeview1.ButtonReleaseEvent += (object o, ButtonReleaseEventArgs args) => {
				Logging.WriteLog ("ButtonReleaseEvent at x=" + args.Event.X.ToString () + " y=" + args.Event.Y.ToString ());


				int x = Convert.ToInt32 (args.Event.X);
				int y = Convert.ToInt32 (args.Event.Y);
				if (!treeview1.GetPathAtPos (x, y, out TreePath path)) {
					return;
				}

				if (!listStore.GetIter (out TreeIter iter, path)) {
					return;
				}

				int index = path.Indices [0];

				var item = _LinkItems [index];
				if (item == null) {

					return;
				}

				if (args.Event.Button == 3) {
					Logging.WriteLog ("Right click \n");

					//OrderRightClicked (ao, index);


				} else {

					URLexplorer.OpenUrl (item.Link);

				}
				
			};
		}

		public WebLinkItem [] _LinkItems { get; set; }
	}


	public class WebLinkItem
	{
		public WebLinkItem (string title, string link)
		{
			Title = title;
			Link = link;
		}

		public string Title { get; set; }
		public string Link { get; set; }


	}
}
