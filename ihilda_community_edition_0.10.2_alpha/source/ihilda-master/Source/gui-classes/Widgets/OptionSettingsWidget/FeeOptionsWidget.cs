﻿using System;
using System.Threading;
using RippleLibSharp.Util;
using RippleLibSharp.Network;
using RippleLibSharp.Transactions;
using Codeplex.Data;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class FeeOptionsWidget : Gtk.Bin
	{
		public FeeOptionsWidget ()
		{
			this.Build ();


			InitGUI ();
		}

		public void InitGUI () {
			FeeSettings settings = FeeSettings.Settings;

			if (settings == null) {
				return;
			}

			this.checkbutton1.Active = settings.Specify != null;
			if (this.checkbutton1.Active) {
				this.entry1.Text = settings.Specify.ToString();
			}

			this.checkbutton2.Active = settings.Multiplier != null;
			if (this.checkbutton2.Active) {
				this.entry2.Text = settings.Multiplier.ToString();
			}

			this.checkbutton5.Active = settings.RetryFactor != null;
			if (this.checkbutton5.Active) {
				this.entry5.Text = settings.RetryFactor.ToString ();
			}
			this.checkbutton3.Active = settings.Warn != null;
			if (this.checkbutton3.Active) {
				this.entry3.Text = settings.Warn.ToString();
			}

			this.checkbutton4.Active = settings.Wait != null;
			if (this.checkbutton4.Active) {
				this.entry4.Text = settings.Wait.ToString();
			}
		}



		public void ProcessFeeOptions () {


			FeeSettings fs = new FeeSettings ();


			if (this.checkbutton1.Active) {
				if (uint.TryParse (entry1.Text, out uint specify)) {
					fs.Specify = specify;
				} else {
					fs.Specify = null;
				}
			}



			if (this.checkbutton2.Active) {
				if (double.TryParse (entry2.Text, out double multipl)) {
					fs.Multiplier = multipl;
				} else {
					fs.Multiplier = null;
				}
			}

			if (this.checkbutton5.Active) {

				if (double.TryParse (entry5.Text, out double retryFact)) {
					fs.RetryFactor = retryFact;
				} else {
					fs.RetryFactor = null;
				}

			}

			if (this.checkbutton3.Active) {
				if (uint.TryParse (entry3.Text, out uint wrn)) {
					fs.Warn = wrn;
				} else {
					fs.Warn = null;
				}
			}



			if (this.checkbutton4.Active) {
				if (uint.TryParse (entry4.Text, out uint wat)) {
					fs.Wait = wat;
				} else {
					fs.Wait = null;
				}
			}

			FeeSettings.Settings = fs;

			FeeSettings.SaveSettings (fs);
		}
	}


	public class FeeSettings {

		static FeeSettings () {
			settingsPath = FileHelper.GetSettingsPath ( settingsFileName );
			Settings = LoadSettings ();
		}

		public UInt32? Specify {
			get;
			set;
		}

		public double? Multiplier {
			get;
			set;
		}

		public double? RetryFactor {
			get;
			set;
		}

		public UInt32? Warn {
			get;
			set;
		}

		public UInt32? Wait {
			get;
			set;
		}


		public static FeeSettings LoadSettings () {
			string str = FileHelper.GetJsonConf (settingsPath);
			if (str == null) {
				return null;
			}
			FeeSettings sets = null;
			try {
				sets = DynamicJson.Parse (str);

			}

			catch (Exception e) {
				Logging.WriteLog (e.Message + e.StackTrace);
				return null;
			}

			return sets;


		}

		public static void SaveSettings ( FeeSettings settings ) {
			FeeSettings.Settings = settings;

			string conf = DynamicJson.Serialize (settings);

			FileHelper.SaveConfig (settingsPath, conf);
		}

		public static Tuple<UInt32, UInt32> ParseFee (NetworkInterface ni) {
			Tuple< string, UInt32 > tupe = RippleLibSharp.Commands.Server.ServerInfo.GetFeeAndLedgerSequence (ni);

			if (tupe == null) {
				return null;
			}

			if (!UInt32.TryParse (tupe.Item1, out uint f)) {
				// TODO debug
				var x = new InvalidCastException ();
				//x.Message = "fee returned from network can not be parsed to an int";
				throw x;

				//return null;
			}

			return new Tuple<UInt32, UInt32>(f, tupe.Item2);
		}

		public static Tuple<UInt32,UInt32> GetFeeAndLastLedgerFromSettings (NetworkInterface ni, UInt32? lastFee = null) {


			if (Settings == null) {

				return ParseFee (ni);
			}


			int feeRetry = 0; 
			START:
			Tuple<UInt32,UInt32> fs = ParseFee (ni);





			if (Settings.Specify != null) {
				//var tupe = parseFee (ni);
				// you have to get the last ledger anyway

				// we already know the last fee was explicitly specified so we blindly increase it by the retry factor
				if (Settings.RetryFactor != null && lastFee != null) {

					fs = new Tuple<uint, uint> ((uint)(Settings.Specify * Settings.RetryFactor), fs.Item2);


				} else {
					fs = new Tuple<uint, uint> ((uint)Settings.Specify, fs.Item2);
				}

				// we are going to wait for the lowest fee specified 
				goto Wait;

			}



			fs = ParseFee (ni);

			if (Settings.Multiplier != null) {

				//f *= (int)settings.multiplier;

				fs = new Tuple<uint, uint> (fs.Item1 * (UInt32)Settings.Multiplier, fs.Item2);


			}

			if (Settings.RetryFactor != null) {
				if (lastFee != null) {

					UInt32 lastAmountFactored = (UInt32)lastFee * (UInt32)Settings.RetryFactor;
					 

					UInt32 newSuggestedAmount = fs.Item1 * (UInt32)Settings.RetryFactor;

					bool newSuggestionIshigher = lastAmountFactored > newSuggestedAmount ;

					UInt32 highestSuggestion = newSuggestionIshigher ? lastAmountFactored : newSuggestedAmount;
					UInt32 lowestSuggestion = newSuggestionIshigher ? newSuggestedAmount : lastAmountFactored;
					// if waiting for lower fee is specified
					if ( Settings.Wait != null ) {

						// if the suggestion is less than wait limit
						if (highestSuggestion < Settings.Wait) {
							// I think we should fasttrack it
							fs = new Tuple<uint, uint> (highestSuggestion, fs.Item2);
							goto Fasttrack;
						}

						if (lowestSuggestion < Settings.Wait) {

							if (lowestSuggestion > lastFee) {
								fs = new Tuple<uint, uint> (lowestSuggestion, fs.Item2);
								goto Fasttrack;
							}

						}

						if (feeRetry > 20) {
							fs = new Tuple<uint, uint> ((UInt32)Settings.Wait, fs.Item2);
							goto Fasttrack;
						}
						goto START;

					}


				}

			}


			Wait:
			if (Settings.Wait != null) {
				
				if ( fs.Item1 > Settings.Wait ) {
					if (feeRetry++ == MAX_FEE_RETRY_ATTEMPTS) {
						return null;
					}
					Thread.Sleep (3000);
					goto START;
				}
			}

			Fasttrack:

			if (Settings.Warn != null) {



				if (fs.Item1 > Settings.Warn ) {
					var v = AreYouSure.AskQuestionNonGuiThread("Approve High Fee", "The current fee is " + fs.Item1.ToString() + " drops. Do you wish to submit the transaction anyway?");
					if (!v) {
						return null;
					}
				}
			}


			return fs;
		}

		public static FeeSettings Settings {
			get;
			set;
		}

		public const string settingsFileName = "FeeSettings.jsn";

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		static string settingsPath = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		private const int MAX_FEE_RETRY_ATTEMPTS = 20 * 5;  // 20 = 1 minute of trying 

			#if DEBUG
		private const string clsstr = nameof (FeeSettings) + DebugRippleLibSharp.colon;
		#endif 
	}



}
