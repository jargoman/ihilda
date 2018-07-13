using System;
using Gtk;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OptionsWidget : Gtk.Bin
	{
		public OptionsWidget ()
		{
			this.Build ();

			if (splashoptionswidget1 == null) {
				splashoptionswidget1 = new SplashOptionsWidget ();
				splashoptionswidget1.Show ();

				if (label16 == null) {
					label16 = new Label ("<b>Splash Settings</b>") {
						UseMarkup = true
					};
				}

				notebook1.AppendPage (splashoptionswidget1, label16);
			}

			if (feeoptionswidget1 == null) {
				feeoptionswidget1 = new FeeOptionsWidget ();
				feeoptionswidget1.Show ();

				if (label2 == null) {
					label2 = new Label ("<b>XRP Fee Options<b>") {
						UseMarkup = true
					};
				}

				notebook1.AppendPage (feeoptionswidget1, label2);

			}
			/*
			if (consoleoptionswidget1 == null) {
				consoleoptionswidget1 = new ConsoleOptionsWidget ();
				consoleoptionswidget1.Show ();

				if (label13 == null) {
					label13 = new Label ("<b>Console Settings</b>") {
						UseMarkup = true
					};
				}

				notebook1.AppendPage (consoleoptionswidget1, label13);
			}
			*/

			if (signoptionswidget1 == null) {
				signoptionswidget1 = new SignOptionsWidget ();
				signoptionswidget1.Show ();

				if (label6 == null) {
					label6 = new Label ("<b>Signature</b>") {
						UseMarkup = true
					};
				}

				notebook1.AppendPage (signoptionswidget1, label6);
			}


			this.savebutton.Clicked += OnSaveButtonClicked;
		}


		public void OnSaveButtonClicked ( object sender, EventArgs e) {


			this.splashoptionswidget1.ProcessSplashSettings ();

			//this.consoleoptionswidget1.ProcessConsoleSettings ();

			this.feeoptionswidget1.ProcessFeeOptions();

			this.signoptionswidget1.ProcessSignOptions ();
		}

	//public static readonly string[] interpreters = new string[] { "JSON", "RIPPLED", CSHARPS"ICEJARGON"};

	}
}

