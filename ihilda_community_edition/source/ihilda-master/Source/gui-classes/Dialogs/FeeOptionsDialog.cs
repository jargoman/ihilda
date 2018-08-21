using System;
namespace IhildaWallet
{
	public partial class FeeOptionsDialog : Gtk.Dialog
	{
		public FeeOptionsDialog ()
		{
			this.Build ();
		}

		public void ProcessFeeOptionsWidget ()
		{
			this.feeoptionswidget1.ProcessFeeOptions ();
		}


	}
}
