using System;
using Gtk;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PagerWidget : Gtk.Bin
	{

		// keep implementation separate so we can reuse the widget
		public PagerWidget (int pages)
		{
			this.Build ();
			//while(Gtk.Application.EventsPending())
			//	Gtk.Application.RunIteration();

			#if DEBUG
			if (DebugIhildaWallet.PagerWidget) {
				Logging.WriteLog(clsstr + "new ( pages = " + pages.ToString() + " )");
			}
			#endif

			this.first = firstbutton;
			this.last = lastbutton;
			this.previous = previousbutton;
			this.next = nextbutton;



		}

		public PagerWidget () : this (DEFAULT_PAGES)
		{
			/*
			if (Debug.PagerWidget) {
				Logging.write(clsstr + "Default constructor");
			}
			*/

		}

		public void SetNumberOfPages ( int pages ) {
			#if DEBUG
			string method_sig = clsstr + nameof (SetNumberOfPages) + DebugRippleLibSharp.left_parentheses + nameof (Int32) + DebugRippleLibSharp.space_char + nameof (pages) + DebugRippleLibSharp.equals + pages.ToString() + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.PagerWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
			#endif
			Gtk.Application.Invoke ( delegate {
				#if DEBUG
				string event_sig = method_sig + DebugIhildaWallet.gtkInvoke;
				if (DebugIhildaWallet.PagerWidget) {
					Logging.WriteLog(event_sig + DebugRippleLibSharp.begin);
				}
				#endif
				this.totallabel.Text = pages.ToString();
			});
		}

		public void SetCurrentPage ( int page )
		{
			Gtk.Application.Invoke ( delegate {
				this.numberlabel.Text = page.ToString();
			});

		}



#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public static readonly int DEFAULT_PAGES = 0;
#pragma warning restore RECS0122 // Initializing field with default value is redundant



		public Button first;
		public Button next;
		public Button previous;
		public Button last;

		//private int pages = DEFAULT_PAGES;

		#if DEBUG
		private static readonly String clsstr = "PagerWidget : ";
		#endif

	}
}

