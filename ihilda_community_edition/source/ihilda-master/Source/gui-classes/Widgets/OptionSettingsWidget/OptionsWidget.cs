using System;
using System.Media;
using System.Threading.Tasks;
using Codeplex.Data;
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

			button1.Clicked += (sender, e) => {
				FileChooserDialog fileChooser = 
					new FileChooserDialog ( 
						"Select Sound", 
						null, 
						FileChooserAction.Open,
						"Cancel", ResponseType.Cancel,
						"Open", ResponseType.Accept
				);

				FileFilter fileFilter = new FileFilter ();
				fileFilter.AddPattern ("*.wav");
				//fileFilter.AddPattern ("*.png");
				fileChooser.AddFilter (fileFilter);

				if (fileChooser?.Run () == (int)ResponseType.Accept) {

					OnOrderFilledLabel.Text = fileChooser.Filename;

				}

				fileChooser?.Destroy ();

			};

			button4.Clicked += (sender, e) => {
				Task.Run ( delegate {
					SoundPlayer soundPlayer = new SoundPlayer (OnOrderFilledLabel.Text);
					soundPlayer.Load ();
					soundPlayer.Play ();
				});
			};

			button2.Clicked += (sender, e) => {
				FileChooserDialog fileChooser =
					new FileChooserDialog (
						"Select Sound",
						null,
						FileChooserAction.Open,
						"Cancel", ResponseType.Cancel,
						"Open", ResponseType.Accept
				);

				FileFilter fileFilter = new FileFilter ();
				fileFilter.AddPattern ("*.wav");
				//fileFilter.AddPattern ("*.png");
				fileChooser.AddFilter (fileFilter);

				if (fileChooser?.Run () == (int)ResponseType.Accept) {

					OnPaymentReceivedLabel.Text = fileChooser.Filename;

				}

				fileChooser?.Destroy ();

			};

			button5.Clicked += (sender, e) => {
				Task.Run (delegate {
					SoundPlayer soundPlayer = new SoundPlayer (OnPaymentReceivedLabel.Text);
					soundPlayer.Load ();
					soundPlayer.Play ();
				});
			};

			button3.Clicked += (sender, e) => {
				FileChooserDialog fileChooser =
					new FileChooserDialog (
						"Select Sound",
						null,
						FileChooserAction.Open,
						"Cancel", ResponseType.Cancel,
						"Open", ResponseType.Accept
				);

				FileFilter fileFilter = new FileFilter ();
				fileFilter.AddPattern ("*.wav");
				//fileFilter.AddPattern ("*.png");
				fileChooser.AddFilter (fileFilter);

				if (fileChooser?.Run () == (int)ResponseType.Accept) {

					OnTxSubmitLabel.Text = fileChooser.Filename;

				}

				fileChooser?.Destroy ();

			};

			button6.Clicked += (sender, e) => {
				Task.Run (delegate {
					SoundPlayer soundPlayer = new SoundPlayer (OnTxSubmitLabel.Text);
					soundPlayer.Load ();
					soundPlayer.Play ();
				});
			};

			button7.Clicked += (sender, e) => {
				FileChooserDialog fileChooser =
					new FileChooserDialog (
						"Select Sound",
						null,
						FileChooserAction.Open,
						"Cancel", ResponseType.Cancel,
						"Open", ResponseType.Accept
				);

				FileFilter fileFilter = new FileFilter ();
				fileFilter.AddPattern ("*.wav");
				//fileFilter.AddPattern ("*.png");
				fileChooser.AddFilter (fileFilter);

				if (fileChooser?.Run () == (int)ResponseType.Accept) {

					OnTxFailLabel.Text = fileChooser.Filename;

				}

				fileChooser?.Destroy ();

			};

			button8.Clicked += (sender, e) => {
				Task.Run (delegate {
					SoundPlayer soundPlayer = new SoundPlayer (OnTxFailLabel.Text);
					soundPlayer.Load ();
					soundPlayer.Play ();
				});
			};

			button9.Clicked += (sender, e) => {
				FileChooserDialog fileChooser =
					new FileChooserDialog (
						"Select Sound",
						null,
						FileChooserAction.Open,
						"Cancel", ResponseType.Cancel,
						"Open", ResponseType.Accept
				);

				FileFilter fileFilter = new FileFilter ();
				fileFilter.AddPattern ("*.wav");
				//fileFilter.AddPattern ("*.png");
				fileChooser.AddFilter (fileFilter);

				if (fileChooser?.Run () == (int)ResponseType.Accept) {

					OnNetFailLabel.Text = fileChooser.Filename;

				}

				fileChooser?.Destroy ();

			};

			button10.Clicked += (sender, e) => {
				Task.Run (delegate {
					SoundPlayer soundPlayer = new SoundPlayer (OnNetFailLabel.Text);
					soundPlayer.Load ();
					soundPlayer.Play ();
				});
			};

			button11.Clicked += (sender, e) => {
				FileChooserDialog fileChooser =
					new FileChooserDialog (
						"Select Sound",
						null,
						FileChooserAction.Open,
						"Cancel", ResponseType.Cancel,
						"Open", ResponseType.Accept
				);

				FileFilter fileFilter = new FileFilter ();
				fileFilter.AddPattern ("*.wav");
				//fileFilter.AddPattern ("*.png");
				fileChooser.AddFilter (fileFilter);

				if (fileChooser?.Run () == (int)ResponseType.Accept) {

					OnAutoFailLabel.Text = fileChooser.Filename;

				}

				fileChooser?.Destroy ();

			};

			button12.Clicked += (sender, e) => {
				Task.Run (delegate {
					SoundPlayer soundPlayer = new SoundPlayer (OnAutoFailLabel.Text);
					soundPlayer.Load ();
					soundPlayer.Play ();
				});
			};

			OnOrderFilledLabel.Text = "";
			OnPaymentReceivedLabel.Text = "";
			OnTxSubmitLabel.Text = "";
			OnTxFailLabel.Text = "";
			OnNetFailLabel.Text = "";
			OnAutoFailLabel.Text = "";


			Task.Run ( delegate {
				SoundSettings soundSettings = SoundSettings.LoadSoundSettings ();
				if (soundSettings == null) {
					return;
				}
				SetSoundUI (soundSettings);
			});
		}

		public void SetSoundUI (SoundSettings soundSettings)
		{
			Application.Invoke ( delegate {
				checkbutton7.Active = soundSettings.HasOnOrderFilled;
				checkbutton8.Active = soundSettings.HasOnPaymentReceived;
				checkbutton9.Active = soundSettings.HasOnTxSubmit;
				checkbutton10.Active = soundSettings.HasOnTxFail;
				checkbutton11.Active = soundSettings.HasOnNetWorkFail;
				checkbutton12.Active = soundSettings.HasOnAutomateFail;

				checkbutton13.Active = soundSettings.FallBack;

				OnOrderFilledLabel.Text = soundSettings.OnOrderFilled;
				OnPaymentReceivedLabel.Text = soundSettings.OnPaymentReceived;
				OnTxSubmitLabel.Text = soundSettings.OnTxSubmit;
				OnTxFailLabel.Text = soundSettings.OnTxFail;
				OnNetFailLabel.Text = soundSettings.OnNetWorkFail;
				OnAutoFailLabel.Text = soundSettings.OnAutomateFail;



			} );
		}


		public void OnSaveButtonClicked ( object sender, EventArgs e) {


			this.splashoptionswidget1.ProcessSplashSettings ();

			//this.consoleoptionswidget1.ProcessConsoleSettings ();

			this.feeoptionswidget1.ProcessFeeOptions();

			this.signoptionswidget1.ProcessSignOptions ();

			this.orderbookoptionswidget1.ProcessOrderBookOptions ();

			SoundSettings soundSettings = new SoundSettings {
				FallBack = checkbutton13.Active,

				HasOnOrderFilled = checkbutton7.Active,
				OnOrderFilled = OnOrderFilledLabel.Text,

				HasOnPaymentReceived = checkbutton8.Active,
				OnPaymentReceived = OnPaymentReceivedLabel.Text,

				HasOnTxSubmit = checkbutton9.Active,
				OnTxSubmit = OnTxSubmitLabel.Text,

				HasOnTxFail = checkbutton10.Active,
				OnTxFail = OnTxFailLabel.Text,

				HasOnNetWorkFail = checkbutton11.Active,
				OnNetWorkFail = OnNetFailLabel.Text,

				HasOnAutomateFail = checkbutton12.Active,
				OnAutomateFail = OnAutoFailLabel.Text,
			};

			SoundSettings.SaveSoundSettings (soundSettings);

		}

		//public static readonly string[] interpreters = new string[] { "JSON", "RIPPLED", CSHARPS"ICEJARGON"};

	}


	public class SoundSettings
	{

		public SoundSettings ()
		{
			FallBack = true;
		}

		public bool HasOnOrderFilled {
			get;
			set;
		}

		

		public string OnOrderFilled {
			get;
			set;
		}


		public bool HasOnPaymentReceived {
			get;
			set;
		}
		public string OnPaymentReceived {
			get;
			set;
		}


		public bool HasOnTxSubmit {
			get;
			set;
		}

		public string OnTxSubmit {
			get;
			set;
		}


		public bool HasOnTxFail {
			get;
			set;
		}

		public string OnTxFail {
			get;
			set;
		}

		public bool HasOnNetWorkFail {
			get;
			set;
		}

		public string OnNetWorkFail {
			get;
			set;
		}

		public bool HasOnAutomateFail {
			get;
			set;
		}

		public string OnAutomateFail {
			get;
			set;
		}

		public bool FallBack {
			get;
			set;
		}

		static SoundSettings ()
		{

			settingsPath = FileHelper.GetSettingsPath (settingsFileName);
		}


		public static void SaveSoundSettings (SoundSettings settings)
		{


			string conf = DynamicJson.Serialize (settings);

			FileHelper.SaveConfig (settingsPath, conf);
		}

		public static SoundSettings LoadSoundSettings ()
		{
			string str = FileHelper.GetJsonConf (settingsPath);
			if (str == null) {
				return null;
			}
			SoundSettings oo = null;
			try {
				oo = DynamicJson.Parse (str);
			} catch (Exception e) {
				Logging.WriteLog (e.Message + e.StackTrace);
				return null;
			}

			return oo;
		}


		public const string settingsFileName = "SoundSettings.jsn";

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		static readonly string settingsPath = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant


	}
}

