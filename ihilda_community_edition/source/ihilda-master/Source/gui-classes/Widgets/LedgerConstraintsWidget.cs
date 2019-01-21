using System;
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

			comboboxentry4.Changed += (object sender, EventArgs e) => {
				comboboxentry4.ModifyBase (StateType.Normal);
				comboboxentry4.Entry.ModifyBase (StateType.Normal);
			};
		}



		public void SetLastKnownLedger (string s)
		{

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

		public void HighLightLimit ()
		{
			Application.Invoke (delegate {
				Gdk.Color orchid = new Gdk.Color (218, 112, 214);
				comboboxentry4.ModifyBase (StateType.Normal, orchid);
				comboboxentry4.Entry.ModifyBase (StateType.Normal, orchid);
			});
		}

		public int? GetLimit ()
		{
			string s = null;
			using (ManualResetEvent mre = new ManualResetEvent (false)) {
				mre.Reset ();

				Application.Invoke (delegate {
					s = this.comboboxentry4.Entry.Text;
					mre.Set ();
				});


				mre.WaitOne ();
			}

			bool b = int.TryParse (s, out int i);
			if (!b) {
				return null;
			}

			return i;
		}

		public Int32? GetStartFromLedger ()
		{
			string s = null;
			using (ManualResetEvent mre = new ManualResetEvent (false)) {
				mre.Reset ();

				Application.Invoke ((sender, e) => {
					s = this.comboboxentry6.Entry.Text;
					mre.Set ();
				});

				mre.WaitOne ();
			}

			bool b = Int32.TryParse (s, out int st);

			if (!b) {

				return null;
			}

			return st;

		}

		public Int32? GetEndLedger ()
		{
			string s;
			using (ManualResetEvent mre = new ManualResetEvent (false)) {
				mre.Reset ();
				s = null;
				Application.Invoke ((sender, e) => {
					s = this.comboboxentry5.Entry.Text;
					mre.Set ();
				});


				mre.WaitOne ();
			}

			bool b = Int32.TryParse (s, out int st);

			if (!b) {
				return null;
			}

			return st;

		}

		/*
		public LedgerConstraint GetLedgerConstraint ()
		{
			ManualResetEvent mre = new ManualResetEvent (false);
			mre.Reset ();

			string mins, maxs, limits;
			bool? b = null;


			Application.Invoke ((sender, e) => {
				mins = this.comboboxentry5.Entry.Text;
				maxs = this.comboboxentry6.Entry.Text;
				limits = this.comboboxentry4.Entry.Text;
				b = checkbutton1.Active;
				mre.Set ();
			});

			LedgerConstraint ledgerConstraint = new LedgerConstraint ();

		}*/

		public bool? GetForward ()
		{
			bool? b = null;
			using (ManualResetEvent mre = new ManualResetEvent (false)) {
				mre.Reset ();

				Application.Invoke ((sender, e) => {
					b = checkbutton1.Active;
					mre.Set ();
				});

				mre.WaitOne ();
			}

			return b;
		}

		public void HideTopRow ()
		{
			//this.label10.Text = "";
			//this.label9.Text = "";

			this.label10.Hide ();
			this.label10.Visible = false;
			this.label9.Hide ();
			this.label9.Visible = false;
		}

		public void HideForward ()
		{

			this.checkbutton1.Hide ();
			this.checkbutton1.Visible = false;

		}
	}


	public class LedgerConstraint
	{
		public int Min {
			get;
			set;
		}

		public int Max {
			get;
			set;
		}

		public int Limit {
			get;
			set;
		}

		public bool Forward {
			get;
			set;
		}
	}

}

