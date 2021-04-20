using System;
using System.Text;
using System.IO; 
using Codeplex.Data;
using IhildaWallet.Util;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class BalanceTabOptionsWidget : Gtk.Bin
	{
		public BalanceTabOptionsWidget ()
		{
			this.Build ();
		}


		public String[] GetFavorites ()
		{
			#if DEBUG

			string method_sig = clsstr + typeof(String).ToString() + DebugRippleLibSharp.array_brackets + nameof (GetFavorites) + DebugRippleLibSharp.left_parentheses + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.BalanceTabOptionsWidget) {
				Logging.WriteLog( method_sig + DebugRippleLibSharp.beginn);
			}
			#endif

			/*
			String [] arr = new string[6];

			arr[0] = this.comboboxentry1.Entry.Text;
			arr[1] = this.comboboxentry2.Entry.Text;
			arr[2] = this.comboboxentry3.Entry.Text;
			arr[3] = this.comboboxentry4.ActiveText;
			arr[4] = this.comboboxentry5.ActiveText;
			arr[5] = this.comboboxentry6.ActiveText;

			return arr;
			*/

			return __favorites;
		}

		public void SetFavorites (String[] arr)
		{

			#if DEBUG
			string method_sig = clsstr + nameof (SetFavorites) + DebugRippleLibSharp.left_parentheses + nameof (String) + DebugRippleLibSharp.array_brackets + DebugRippleLibSharp.space_char + nameof (arr) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.BalanceTabOptionsWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
			#endif

			Gtk.Application.Invoke( delegate {
				
				this.comboboxentry1.Entry.Text = arr[0];
				this.comboboxentry2.Entry.Text = arr[1];
				this.comboboxentry3.Entry.Text = arr[2];
				this.comboboxentry4.Entry.Text = arr[3];
				this.comboboxentry5.Entry.Text = arr[4];
				this.comboboxentry6.Entry.Text = arr[5];
			});

			this.__favorites = arr;
		}

		private String[] __favorites;


		public static String[] FavoritesFromJson (String json)
		{
			#if DEBUG
			string method_sig = clsstr + nameof(FavoritesFromJson) + DebugRippleLibSharp.colon;
			if (DebugIhildaWallet.BalanceTabOptionsWidget) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}

			#endif
			var dyn = DynamicJson.Parse (json);
			if (dyn.IsDefined ("array")) {

				try 
				{

					String[] array = dyn.array as String[];
					return array;
					#pragma warning disable 0168
				} catch (Exception e) {
					#pragma warning restore 0168

					#if DEBUG
					if (DebugIhildaWallet.BalanceTabOptionsWidget) {
					Logging.ReportException(method_sig, e);
					}
					#endif

					return null;
				}
			}


			return null;

		}



		public static String GetJson (String[] array)
		{
			var obj = new {array};

			return DynamicJson.Serialize(obj);

		}

		public static readonly String[] default_values = {"BTC", "LTC", "USD", "CAD", "EUR", "NMC"};




#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public static String[] actual_values = null;

		public static String jsonConfig = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		public static readonly String configName = "favorites.jsn";
		public static String balanceConfigPath = "";





		public static bool LoadBalanceConfig ()
		{
			#if DEBUG
			string method_sig = clsstr + nameof(LoadBalanceConfig) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.BalanceTabOptionsWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
			#endif

			balanceConfigPath = FileHelper.GetSettingsPath (configName);

			jsonConfig = FileHelper.GetJsonConf (balanceConfigPath);



			// if favorites were set via command line or plugin use those instead
			if (actual_values != null) {
				SaveConfig(actual_values);
				return actual_values != null; // TODO return true? 
			}

			// if there is no config then generate a default one
			if (jsonConfig == null) {
				jsonConfig = GetJson (default_values);
				actual_values = default_values;

			} else {

				actual_values = FavoritesFromJson (jsonConfig);


				return actual_values != null;
				/*
				if (actual_values != null) {
					return true;
				}*/
			}


			return false;

		}

		public static void SetFavoriteParam (String param)
		{
			if (param==null) {
				// todo debug
			}

			String[] values = param.Split(',');

			if (values.Length!=6) {
				Logging.WriteBoth("Commad line argument Favorites must specify six currencies");
				Logging.WriteBoth("Example : favorites=CAD,BTC,EUR,LTC,JED,CNY");
			}

			// todo sanity check. valid currencies?

			actual_values = values;


		}

		public static void SaveConfig (String[] values) {
			jsonConfig = GetJson(values);
			try {
				File.WriteAllText(balanceConfigPath,jsonConfig);
			}
			catch (Exception e) {
				// todo debug
				Logging.WriteLog(e.Message);
			}

		}

		protected void OnComboboxentry1Changed (object sender, EventArgs e)
		{
			// no need to do anything ?
			//throw new System.NotImplementedException ();
		}

		#if DEBUG
		private const string clsstr = nameof (BalanceTabOptionsWidget) + DebugRippleLibSharp.colon;
		#endif
	}
}

