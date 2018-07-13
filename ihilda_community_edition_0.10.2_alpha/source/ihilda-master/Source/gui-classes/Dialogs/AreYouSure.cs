using System;
using System.Threading;
using Gtk;

namespace IhildaWallet
{
	public partial class AreYouSure : Gtk.Dialog
	{
		public AreYouSure (String title, String message)
		{
			this.Build ();
			Initiate (title, message);
		}

		public AreYouSure (String message)
		{
			this.Build ();
			Initiate (null, message);
		}


		private void Initiate (String title, String message)
		{
			

			if (title == null) {
				title = "Are you sure?";
			}
			this.Title = title;


			/*
			this.label11.Text = "<big><b><u>" + title + "</u></b></big>";
			this.label11.UseMarkup = true;
			*/

			//this.textview2.Buffer.Text = message;

			this.label1.Text = message;

			//this.label1.Markup = message;
			this.label1.UseMarkup = true;
			this.Modal = true;

		}

		public static bool AskQuestion (String title, String message)
		{
			
			int rt = (int)ResponseType.None;

				
			using (AreYouSure aus = new AreYouSure (title, message)) {
				rt = aus.Run ();

				aus.Destroy ();

				//
			}	
			



			return rt == (int)ResponseType.Ok;

		}


		public static bool AskQuestionNonGuiThread (String title, String message)
		{
			ManualResetEvent ev = new ManualResetEvent (false);
			int rt = (int)ResponseType.None;
			Gtk.Application.Invoke ( (object sender, EventArgs e) => {

				using (AreYouSure aus = new AreYouSure (title, message)) {
					rt = aus.Run ();

					aus.Destroy ();

					//
				}

				ev.Set();
			});

			ev.WaitOne ();

			return rt == (int)ResponseType.Ok;

		}



	}
}
