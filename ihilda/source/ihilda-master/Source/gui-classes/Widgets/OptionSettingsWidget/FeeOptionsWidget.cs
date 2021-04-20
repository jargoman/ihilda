using System;
using System.Threading;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Commands.Server;
using RippleLibSharp.Commands.Subscriptions;
using RippleLibSharp.Network;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class FeeOptionsWidget : Gtk.Bin
	{
		public FeeOptionsWidget ()
		{
			this.Build ();

			Task.Run ((Action)InitGUI);

		}

		public void InitGUI () {

			FeeSettings settings = FeeSettings.LoadSettings ();



			if (settings == null) {
				return;
			}

			Gtk.Application.Invoke ( delegate {
				this.checkbutton1.Active = settings.Specify != null;
				if (this.checkbutton1.Active) {
					this.entry1.Text = settings.Specify.ToString ();
					this.entry1.Sensitive = true;
				} else {
					this.entry1.Sensitive = false;
				}

				this.checkbutton1.Clicked += (object sender, EventArgs e) => {

					this.entry1.Sensitive = this.checkbutton1.Active;
				};

				this.checkbutton2.Active = settings.Multiplier != null;
				if (this.checkbutton2.Active) {
					this.entry2.Text = settings.Multiplier.ToString ();
					this.entry2.Sensitive = true;
				} else {
					this.entry2.Sensitive = false;
				}

				this.checkbutton2.Clicked += (object sender, EventArgs e) => {
					this.entry2.Sensitive = this.checkbutton2.Active;
				};

				this.checkbutton5.Active = settings.RetryFactor != null;
				if (this.checkbutton5.Active) {
					this.entry5.Text = settings.RetryFactor.ToString ();
					this.entry5.Sensitive = true;
				} else {
					this.entry5.Sensitive = false;
				}

				this.checkbutton5.Clicked += (object sender, EventArgs e) => {
					this.entry5.Sensitive = this.checkbutton5.Active;
				};

				this.checkbutton3.Active = settings.Warn != null;
				if (this.checkbutton3.Active) {
					this.entry3.Text = settings.Warn.ToString ();
					this.entry3.Sensitive = true;
				} else {
					this.entry3.Sensitive = false;
				}

				this.checkbutton3.Clicked += (object sender, EventArgs e) => {
					this.entry3.Sensitive = this.checkbutton3.Active;
				};


				this.checkbutton4.Active = settings.Wait != null;
				if (this.checkbutton4.Active) {
					this.entry4.Text = settings.Wait.ToString ();
					this.entry4.Sensitive = true;
				} else {
					this.entry4.Sensitive = false;
				}

				this.checkbutton4.Clicked += (object sender, EventArgs e) => {
					this.entry4.Sensitive = this.checkbutton4.Active;
				};


			});


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

			//FeeSettings.Settings = fs;
	    		Task.Run ( delegate {

				FeeSettings.SaveSettings (fs);

			});

		}
	}


	public class FeeSettings {

		static FeeSettings () {
			settingsPath = FileHelper.GetSettingsPath ( settingsFileName );
			//Settings = LoadSettings ();
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
			//FeeSettings.Settings = settings;

			string conf = DynamicJson.Serialize (settings);

			FileHelper.SaveConfig (settingsPath, conf);
		}

		public static ParsedFeeAndLedgerResp ParseFee (
			NetworkInterface ni, 
			CancellationToken token
		) {
			ParsedFeeAndLedgerResp parsed = new ParsedFeeAndLedgerResp ();
			//Tuple<string, UInt32> tupe = null;
			FeeAndLastLedgerResponse feeResp = null;
			if (token.IsCancellationRequested) {
				throw new TaskCanceledException ( nameof(FeeSettings) + " : " + nameof (ParseFee) + " : Task Cancelation recieved\n");
			}
			// try to retrieve from last server state subscribe response
			feeResp = LedgerTracker.GetFeeAndLastLedger (token);

			if (feeResp == null) {
				// explicitly poll the server if subscribe is not working
				parsed.ErrorMessage += "Fee resp null\n";
				parsed.ErrorMessage += "Explicitly polling fee and last ledger\n";
				feeResp = ServerInfo.GetFeeAndLedgerSequence (ni, token);
			} else if (feeResp.HasError) {
				// explicitly poll the server if subscribe is not working


				parsed.ErrorMessage += feeResp.ErrorMessage;
				parsed.ErrorMessage += "Explicitly polling fee and last ledger\n";
				feeResp = ServerInfo.GetFeeAndLedgerSequence (ni, token);
			}

			if (feeResp == null) {
				parsed.ErrorMessage += feeResp.ErrorMessage;
				parsed.ErrorMessage += "Parse fee : feeResp == null\n";
				parsed.HasError = true;
				return parsed;
			}

			if (feeResp.HasError) {
				parsed.HasError = true;
				parsed.ErrorMessage += feeResp.ErrorMessage;
				return parsed;
			}

			parsed.LastLedger = feeResp.LastLedger;


			bool success = UInt32.TryParse (feeResp.Fee, out uint f);

			if (!success) {
				// TODO debug
				var x = new InvalidCastException ();
				//x.Message = ;
				//throw x;
				parsed.ErrorMessage += "Fee returned from network can not be parsed to an int\n";
				parsed.HasError = true;
				return parsed;
			}

			parsed.Fee = f;

			return parsed;
		}

		public ParsedFeeAndLedgerResp GetFeeAndLastLedgerFromSettings (NetworkInterface ni, CancellationToken token, UInt32? lastFee = null) {



			
			int nullFeeCount = 0;

			int feeRetry = 0;
		START:
			ParsedFeeAndLedgerResp fs = ParseFee (ni, token);


			if (fs == null) {

				// if we keep getting a null response then return null if MAX_NULL_FEE reached
				if (nullFeeCount++ > MAX_NULL_FEE) {
					return null;
				}
				goto START;
			} else {
				// reset the null counter to 0 if response isn't null
				nullFeeCount = 0;
			}

			if (fs.HasError) {
				return fs;
			}


			if (this.Specify != null) {
				//var tupe = parseFee (ni);
				// you have to get the last ledger anyway

				// we already know the last fee was explicitly specified so we blindly increase it by the retry factor
				fs = this.RetryFactor != null && lastFee != null
					? new ParsedFeeAndLedgerResp () { Fee = (uint)(this.Specify * this.RetryFactor), LastLedger = fs.LastLedger }
					: new ParsedFeeAndLedgerResp () { Fee = (uint)this.Specify, LastLedger = fs.LastLedger};
				// we are going to wait for the lowest fee specified 
				goto Wait;

			}



			//fs = ParseFee (ni, token);
	    		/*
			if (fs == null) {

				if (nullFeeCount++ > 5) {
					return null;
				}

				goto START;
			}*/

			if (this.Multiplier != null) {

				//f *= (int)settings.multiplier;

				//fs = new Tuple<uint, uint> (, fs.Item2);
				fs = new ParsedFeeAndLedgerResp () { Fee = fs.Fee * (UInt32)this.Multiplier, LastLedger = fs.LastLedger };

			}

			if (this.RetryFactor != null) {
				if (lastFee != null) {

					UInt32 lastAmountFactored = (UInt32)lastFee * (UInt32)this.RetryFactor;
					 

					UInt32 newSuggestedAmount = (UInt32)fs.Fee * (UInt32)this.RetryFactor;

					bool newSuggestionIshigher = lastAmountFactored > newSuggestedAmount;

					UInt32 highestSuggestion = newSuggestionIshigher ? lastAmountFactored : newSuggestedAmount;
					UInt32 lowestSuggestion = newSuggestionIshigher ? newSuggestedAmount : lastAmountFactored;

					// if waiting for lower fee is specified
					if ( this.Wait != null ) {

						// if the suggestion is less than wait limit
						if (highestSuggestion < this.Wait) {
							// I think we should fasttrack it
							//fs = new Tuple<uint, uint> (highestSuggestion, fs.LastLedger);
							fs = new ParsedFeeAndLedgerResp () { Fee = highestSuggestion, LastLedger = fs.LastLedger };
							goto Fasttrack;
						}

						if (lowestSuggestion < this.Wait) {

							if (lowestSuggestion > lastFee) {
								
								fs = new ParsedFeeAndLedgerResp () { Fee = lowestSuggestion, LastLedger = fs.LastLedger };
								goto Fasttrack;
							}

						}

						if (feeRetry > 20) {
							fs = new ParsedFeeAndLedgerResp () { Fee = (UInt32)this.Wait, LastLedger = fs.LastLedger };
							goto Fasttrack;
						}
						goto START;

					}


				}

			}


			Wait:
			if (this.Wait != null) {

				if ( fs.Fee > this.Wait ) {
					if (feeRetry++ == MAX_FEE_RETRY_ATTEMPTS) {
						fs.HasError = true;
						fs.ErrorMessage += "max retry attemps reached";
						return fs;
					}


					FeeSleepEventArgs feeSleepEventArgs = new FeeSleepEventArgs {
						FeeAndLastLedger = fs
					};

					feeSleepEventArgs.State = FeeSleepState.Begin;

					OnFeeSleep?.Invoke (this, feeSleepEventArgs);

					//LedgerTracker.ServerStateEvent.WaitOne ();


						
					//feeSleepEventArgs.State = FeeSleepState.PumpUI;
					//for (int i = 0; i < 3; i++) {
					//	OnFeeSleep?.Invoke (this, feeSleepEventArgs);
					//	token.WaitHandle.WaitOne (1000);
						//LedgerTracker.ServerStateEv
					//}
					WaitHandle.WaitAny (new WaitHandle [] { LedgerTracker.LedgerResetEvent, LedgerTracker.ServerStateEvent, token.WaitHandle }, 3000);

						
					

					feeSleepEventArgs.State = FeeSleepState.Wake;
					OnFeeSleep?.Invoke (this, feeSleepEventArgs);

					goto START;
				}
			}

			Fasttrack:

			if (this.Warn != null) {



				if (fs.Fee > this.Warn ) {
					var v = 
						AreYouSure.AskQuestionNonGuiThread (
							"Approve High Fee", 
							"The current fee is " + 
							fs.Fee.ToString() + 
							" drops. Do you wish to submit the transaction anyway?" );

					if (!v) {

						fs.HasError = true;
						fs.ErrorMessage += "User declined high fee\n";
						return fs;
					}
				}
			}


			return fs;
		}


		public event EventHandler<FeeSleepEventArgs> OnFeeSleep;
		/*
		public static FeeSettings Settings {
			get;
			set;
		}
		*/

		public const string settingsFileName = "FeeSettings.jsn";

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		static string settingsPath = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		private const int MAX_FEE_RETRY_ATTEMPTS = 20 * 10;  // 20 = 1 minute of trying 

		private const int MAX_NULL_FEE = 10;

			#if DEBUG
		private const string clsstr = nameof (FeeSettings) + DebugRippleLibSharp.colon;
		#endif 
	}

	public class FeeSleepEventArgs : EventArgs
	{
		internal FeeSleepState State {
			get;
			set;
		}

		public ParsedFeeAndLedgerResp FeeAndLastLedger {
			get;
			set;
		}

		public int PumpNumber {
			get;
			set;
		}

	}

	public class ParsedFeeAndLedgerResp
	{
		public UInt32 Fee {
			get;
			set;
		}

		public UInt32 LastLedger {
			get;
			set;
		}

		public string Message {
			get;
			set;
		}

		public string ErrorMessage {
			get;
			set;
		}

		public bool HasError {
			get {
				if (ErrorMessage != null) {
				//	return true;
				}

				
				return _hasError;
			}
			set {
				_hasError = value;
			}

		}

		private bool _hasError = false;
	} 

	public enum FeeSleepState
	{

		Begin,
		PumpUI,
		Wake

	}

}

