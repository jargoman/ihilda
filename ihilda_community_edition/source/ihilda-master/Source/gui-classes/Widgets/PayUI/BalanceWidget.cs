using System;
using System.Collections.Generic;
using Gtk;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class BalanceWidget : Gtk.Bin
	{
		public BalanceWidget ()
		{
			this.Build ();

			//currentInstance = this;


			//scrolledWindow1.ToString();
		}

		//public static BalanceWidget currentInstance;

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public Table table = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		public void SetTable ( List<CurrencyWidget> currencyWidgetList )
		{
			if (currencyWidgetList == null) {
				table.Destroy();
				return;
			}


			#if DEBUG
			if (DebugIhildaWallet.BalanceWidget) {
				Logging.WriteLog ("BalanceWidget : setTable Fired\n" + currencyWidgetList.ToString() + "\n");
			}
			#endif

			CurrencyWidget[] widgets = currencyWidgetList.ToArray();

			if ( table!=null ) {
				table.Destroy();
			}

			uint len = 0;

			try {
				len = checked ((uint) widgets.Length);
			}

			catch (System.OverflowException e) {
				Logging.WriteLog ("Exception thrown in Class BalanceWidget, widgets.Lenth is not a valid unsigned int. " + e.Message);
				
				return;
			}

			if (len == 0) {
				#if DEBUG
				if (DebugIhildaWallet.BalanceWidget) {
					Logging.WriteLog ("BalanceWidget.setTable : No widgets to display\n");
				}
				#endif
				return;
			}

			#if DEBUG
			if (DebugIhildaWallet.BalanceWidget) {
				Logging.WriteLog ("BalanceWidget : Creating Table");
			}
			#endif

			table = new Table (
				len,
				1,
				true
				);

			for (uint y = 0; y < len; y++) {

				table.Attach(widgets[y],0,1,y,y+1);
				widgets[y].Show();

			}


			this.scrolledwindow1.AddWithViewport(table);
			table.Show();
			//MainWindow.currentInstance.ShowAll();
			//NetworkInterface.netwaithandle.Set ();
		}

	}
}

