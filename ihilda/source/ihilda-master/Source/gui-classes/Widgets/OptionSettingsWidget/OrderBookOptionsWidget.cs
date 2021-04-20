using System;
using System.Threading.Tasks;
using Codeplex.Data;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class OrderBookOptionsWidget : Gtk.Bin
	{
		public OrderBookOptionsWidget ()
		{
			this.Build ();

			Task.Run ((System.Action)SetUI); 

		}

		private void SetUI ()
		{
			// TODO radio buttons
			OrderBookOptions bookOptions = OrderBookOptions.LoadOrderBookOptions (); ;

			if (bookOptions == null) {
				// TODO alert user to set the sign options?
				return;
			}

			Gtk.Application.Invoke (delegate {
				comboboxentry1.Entry.Text = bookOptions.Limit.ToString ();
				comboboxentry2.Entry.Text = bookOptions.LedgerDelay.ToString ();
				checkbutton1.Active = bookOptions.AutoRefresh;
			});



		}

		internal void ProcessOrderBookOptions ()
		{
			string lim = comboboxentry1.ActiveText;
			string num = comboboxentry2.ActiveText;
			bool b = checkbutton1.Active;

			bool b1 = UInt32.TryParse (lim, out uint limit);
			if (!b1) {
				// TODO user error
				return;

			}

			bool b2 = UInt32.TryParse (num, out uint number);
			if (!b2) {
				return;
			}
			OrderBookOptions bookOptions = new OrderBookOptions {
				Limit = limit,
				LedgerDelay = number,
				AutoRefresh = b
			};


			Task.Run ( delegate {

				OrderBookOptions.SaveOrderBookOptions (bookOptions);
			});

		}


	}

	public class OrderBookOptions
	{

		static OrderBookOptions ()
		{

			settingsPath = FileHelper.GetSettingsPath (settingsFileName);
		}


		public static void SaveOrderBookOptions (OrderBookOptions settings)
		{


			string conf = DynamicJson.Serialize (settings);

			FileHelper.SaveConfig (settingsPath, conf);
		}

		public static OrderBookOptions LoadOrderBookOptions ()
		{
			string str = FileHelper.GetJsonConf (settingsPath);
			if (str == null) {
				return null;
			}
			OrderBookOptions oo = null;
			try {
				oo = DynamicJson.Parse (str);
			} catch (Exception e) {
				Logging.WriteLog (e.Message + e.StackTrace);
				return null;
			}

			return oo;
		}


		public bool AutoRefresh {
			get;
			set;
		}

		public UInt32 Limit {
			get;
			set;
		} 

		public UInt32 LedgerDelay {
			get;
			set;
		}

		public const string settingsFileName = "OrderBookOptions.jsn";

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		static readonly string settingsPath = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant


	}
}
