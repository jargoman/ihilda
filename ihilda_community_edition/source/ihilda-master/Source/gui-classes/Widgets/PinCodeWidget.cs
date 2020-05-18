using System;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PinCodeWidget : Gtk.Bin
	{
		public PinCodeWidget ()
		{
			this.Build ();

			this.button1.Clicked += (object sender, EventArgs e) => {
				entry.Text += button1.Label;
			};
				
			this.button2.Clicked += (object sender, EventArgs e) => {
				entry.Text += button2.Label;
			};

			this.button3.Clicked += (object sender, EventArgs e) => {
				entry.Text += button3.Label;
			};

			this.button4.Clicked += (object sender, EventArgs e) => {
				entry.Text += button4.Label;
			};

			this.button5.Clicked += (object sender, EventArgs e) => {
				entry.Text += button5.Label;
			};

			this.button6.Clicked += (object sender, EventArgs e)  => {
				entry.Text += button6.Label;
			};

			this.button7.Clicked += (object sender, EventArgs e) => {
				entry.Text += button7.Label;
			};

			this.button8.Clicked += (object sender, EventArgs e) => {
				entry.Text += button8.Label;
			};

			this.button9.Clicked += (object sender, EventArgs e) => {
				entry.Text += button9.Label;
			};

			this.buttonzero.Clicked +=  ( object sender, EventArgs e) => {
				entry.Text += buttonzero.Label;
			};

			this.buttona.Clicked += (object sender, EventArgs e) => {
				entry.Text += buttona.Label;
			};

			this.buttonh.Clicked += (object sender, EventArgs e) => {
				entry.Text += buttonh.Label;
			};

			this.resetbutton.Clicked += (object sender, EventArgs e) => {
				entry.Text = "";
			};

			this.deletebutton.Clicked += (object sender, EventArgs e) => {
				entry.Text = entry.Text.Remove(entry.Text.Length - 1);
			};
		}

		public void HideInfoBarLabels ()
		{
			this.label13.Markup = "";
			this.label13.Hide ();
		}

		public string GetEntryString () {

			string str = this.entry.Text;
			if (string.IsNullOrWhiteSpace (str)) {
				this.label13.Markup = ProgramVariables.darkmode ? "<span fgcolor=\"#FFAABB\">Pincode can not be blank</span>" : "<span fgcolor=\"red\">Pincode can not be blank</span>";
				this.label13.Show ();
			};
			return str;
		}
	}
}

