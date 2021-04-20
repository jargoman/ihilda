using System;
using Gtk;

namespace IhildaWallet
{
	public partial class DebuggingOptionsDialogWindow : Gtk.Dialog
	{
		public DebuggingOptionsDialogWindow ()
		{
			this.Build ();

			if (debugtreewidget1 == null) {
				if (label1 == null) {
					label1 = new Label ("<b>Ihilda</b>") {
						UseMarkup = true
					};

				}

				debugtreewidget1 = new DebugTreeWidget ();

				this.notebook1.AppendPage (debugtreewidget1, label1);
			}

			if (debuglibrarywidget1 == null) {
				if (label2 == null) {
					label2 = new Label ("<b>RippleLibSharp</b>") {
						UseMarkup = true
					};
				}

				debuglibrarywidget1 = new DebugLibraryWidget ();

				this.notebook1.AppendPage (debuglibrarywidget1, label2);
			}


			button245.Clicked += StartTestClicked;
		}



		void StartTestClicked (object sender, EventArgs e)
		{

		}


		private void WriteToOutPut (string message)
		{
			Gtk.Application.Invoke ( delegate {
				textview1.Buffer.Text += message;

			});
		}
	}
}
