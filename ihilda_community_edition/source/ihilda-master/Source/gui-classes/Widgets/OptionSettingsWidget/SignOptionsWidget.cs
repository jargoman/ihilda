using System;
using Codeplex.Data;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class SignOptionsWidget : Gtk.Bin
	{
		public SignOptionsWidget ()
		{
			this.Build ();
			//SignOptions signOptions = SignOptions.LoadSignOptions ();
			SetUI ();
		}

		private void SetUI ()
		{
			// TODO radio buttons
			SignOptions signOptions = SignOptions.LoadSignOptions ();

			if (signOptions == null) {
				// TODO alert user to set the sign options?
				return;
			}

			Gtk.Application.Invoke ( delegate {
				comboboxentry1.Entry.Text = signOptions.LastLedgerOffset.ToString ();
				if (signOptions.UseLocalRippledRPC) {
					radiobutton2.Active = true;
				} else {
					radiobutton1.Active = true;
				}

			});



		}


		public void ProcessSignOptions () {
			// TODO save from UI


			string ledger = comboboxentry1.Entry.Text;
			bool b = this.radiobutton1.Active;

			//so.lastLedgerOffset = ledger;
			bool valid = uint.TryParse (ledger, out uint l);

			SignOptions so = new SignOptions {
				UseLocalRippledRPC = !b
			};

			if (valid) {
				so.LastLedgerOffset = l;
			}


			SignOptions.SaveSignOptions (so);
		}


	}

	public class SignOptions {


		static SignOptions () {
			settingsPath = FileHelper.GetSettingsPath (settingsFileName);
			//SignOptionsObj = LoadSignOptions ();
		}

		/*
		public SignOptions () {

		}
		*/



		public bool UseLocalRippledRPC {
			get;
			set;
		}

		public UInt32 LastLedgerOffset {
			get;
			set;
		}

		public static void SaveSignOptions ( SignOptions settings ) {
			

			string conf = DynamicJson.Serialize (settings);

			FileHelper.SaveConfig (settingsPath, conf);
		}

		public static SignOptions LoadSignOptions () {
			string str = FileHelper.GetJsonConf (settingsPath);
			if (str == null) {
				return null;
			}
			SignOptions so = null;
			try {
				so=DynamicJson.Parse(str);
			}

			catch (Exception e) {
				Logging.WriteLog (e.Message + e.StackTrace);
				return null;
			}

			return so;
		}

		/*
		public static SignOptions SignOptionsObj {
			get;
			set;
		}
		*/



		public const string settingsFileName = "SignatureOptions.jsn";

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		static readonly string settingsPath = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

#if DEBUG
		private const string clsstr = nameof (SignOptions) + DebugRippleLibSharp.colon;
#endif
		public static uint DEFAUL_LAST_LEDGER_SEQ = 7;
	}
}

