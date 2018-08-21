using System;
using Gtk;
using System.Collections.Generic;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WalletWidget : Gtk.Button
	{
		public WalletWidget ()
		{
			this.Build ();

			//RadioButton rb = new RadioButton();

			//rb.
		}


		public void setWallet (RippleWallet rw)
		{
			Application.Invoke ( 
			    delegate {
					namelabel.Text = rw.WalletName;
					
					accountlabel.Text = rw.GetStoredReceiveAddress();
					encryptionlabel.Text = rw.GetStoredEncryptionType();
				}
			);

		}

		public void selectMe ()
		{
			//CellRenderer();
		}


		// we can reuse the class as the titlebar
		public void setAsTitleBar ()
		{
			Application.Invoke(
				delegate {
				namelabel.Text = "<b><u>" + namelabel.Text + "</u></b>";
				namelabel.UseMarkup = true;
				//namelabel.ShowAll();
				accountlabel.Text = "<b><u>" + accountlabel.Text + "</u></b>";
				accountlabel.UseMarkup = true;
				encryptionlabel.Text = "<b><u>" + encryptionlabel.Text + "</u></b>";
				encryptionlabel.UseMarkup = true;



				}
			);
		}
	}
}

