using System;
using RippleLibSharp.Transactions;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ConsoleOptionsWidget : Gtk.Bin
	{
		public ConsoleOptionsWidget ()
		{
			this.Build ();

			this.interpreterCombobox.Model = ConsoleInterpreter.GetInterpretersListStore();

			this.interpreterCombobox.Changed += (object sender, EventArgs e) => {

			};
		}


		public void ProcessConsoleSettings () {
			ConsoleOptions co = new ConsoleOptions {
				Remember = remembercheckbutton.Active,

				Print_debug = printdebugcheckbutton.Active,

				Max_lines = SplashOptionsWidget.ParseInt (entry3.Text),

				Interpreter = interpreterCombobox.ActiveText
			};
		}
	}


	public class ConsoleOptions {

		public bool Remember {
			set;
			get;
		}

		public int? Max_lines {
			get;
			set;
		}

		public bool Print_debug {
			get;
			set;
		}

		public string Interpreter {
			get;
			set;
		}
	}
}

