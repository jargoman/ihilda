﻿using System;
using System.Threading;
using Gtk;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class LedgerConstraintsWidget : Gtk.Bin
	{
		public LedgerConstraintsWidget ()
		{
			this.Build ();
		}

		public void SetLastKnownLedger (string s) {

			Gtk.Application.Invoke (delegate {



				if (s == null) {
					label9.Text = "";
					label9.Hide ();
					label10.Hide ();
				}


				this.label9.Text = s;
				this.label9.Show ();
				this.label10.Show ();
			});
		}

		public int? GetLimit ( ) {
			ManualResetEvent mre = new ManualResetEvent (false);
			mre.Reset ();
			string s = null;

			Application.Invoke ( delegate {
				s = this.comboboxentry4.Entry.Text;
				mre.Set();
			});


			mre.WaitOne ();


			bool b = int.TryParse (s, out int i);
			if (!b) {
				return null;
			}

			return i;
		}

		public Int32? GetStartFromLedger () {

			ManualResetEvent mre = new ManualResetEvent (false);
			mre.Reset ();
			string s = null;

			Application.Invoke ((sender, e) => {
				s = this.comboboxentry6.Entry.Text;
				mre.Set ();
			});

			mre.WaitOne ();




			bool b = Int32.TryParse (s, out int st);

			if (!b) {

				return null;
			}

			return st;

		}

		public Int32? GetEndLedger () {

			ManualResetEvent mre = new ManualResetEvent (false);
			mre.Reset ();
			string s = null;

			Application.Invoke ((sender, e) => {
				s = this.comboboxentry5.Entry.Text;
				mre.Set ();
			});


			mre.WaitOne ();




			bool b = Int32.TryParse (s, out int st);

			if (!b) {
				return null;
			}

			return st;

		}

		public bool? GetForward () {
			ManualResetEvent mre = new ManualResetEvent(false);
			mre.Reset ();
			bool? b = null;
			Application.Invoke ((sender, e) => {
				b = checkbutton1.Active;
				mre.Set ();
			});

			mre.WaitOne ();

			return b;
		}

		public void HideTopRow () {
			//this.label10.Text = "";
			//this.label9.Text = "";

			this.label10.Hide ();
			this.label10.Visible = false;
			this.label9.Hide ();
			this.label9.Visible = false;
		}

		public void HideForward () {
			
			this.checkbutton1.Hide();
			this.checkbutton1.Visible = false;

		}
	}
}
