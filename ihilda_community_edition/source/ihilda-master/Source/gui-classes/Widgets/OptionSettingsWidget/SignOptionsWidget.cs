using System;
using System.Threading.Tasks;
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
			Task.Run ((Action)SetUI);

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

				string signwith = signOptions.SigningLibrary;
				if (signwith == null) {
					signwith = "RippleDotNet";
				}

				switch (signwith) {
				case "RippleLibSharp":
					radiobutton1.Active = true;
					break;
				case "Rippled":
					radiobutton2.Active = true;
					break;
				case "RippleDotNet":
					radiobutton3.Active = true;
					break;
				}
			});



		}


		public void ProcessSignOptions () {
			// TODO save from UI


			string ledger = comboboxentry1.Entry.Text;

			string lib = null;
			bool b = this.radiobutton1.Active;
			if (b) {
				lib = "RippleLibSharp";
			}

			bool b2 = this.radiobutton2.Active;
			if (b2) {
				lib = "Rippled";
			}

			bool b3 = this.radiobutton3.Active;
			if (b3) {
				lib = "RippleDotNet";
			}
			//so.lastLedgerOffset = ledger;
			bool valid = uint.TryParse (ledger, out uint l);

			SignOptions so = new SignOptions {
				SigningLibrary = lib
			};

			if (valid) {
				so.LastLedgerOffset = l;
			}

			Task.Run ( delegate {
				SignOptions.SaveSignOptions (so);

			});

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
		


		public string SigningLibrary {
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

