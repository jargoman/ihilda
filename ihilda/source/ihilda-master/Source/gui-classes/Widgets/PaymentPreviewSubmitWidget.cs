using System;
using System.Threading.Tasks;
using RippleLibSharp.Transactions.TxTypes;
using RippleLibSharp.Network;
using RippleLibSharp.Keys;
using IhildaWallet.Networking;
using RippleLibSharp.Util;
using System.Collections.Generic;
using IhildaWallet.Util;
using System.Linq;
using System.Threading;
using System.Media;
using RippleLibSharp.Commands.Accounts;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PaymentPreviewSubmitWidget : Gtk.Bin
	{
		public PaymentPreviewSubmitWidget ()
		{
			this.Build ();

			if (paymentstree1 == null) {
				paymentstree1 = new PaymentsTree ();
				paymentstree1.Show ();

				vbox2.Add (paymentstree1);
			}

			if (walletswitchwidget1 == null) {
				walletswitchwidget1 = new WalletSwitchWidget ();
				walletswitchwidget1.Show ();

				hbox1.Add (walletswitchwidget1);
			}

			paymentstree1.SetParent (this);

			infoBarlabel.Text = "";

			walletswitchwidget1.WalletChangedEvent += (source, eventArgs) => {

				RippleWallet rippleWallet = eventArgs.GetRippleWallet ();
				string acc = rippleWallet?.GetStoredReceiveAddress ();
				if (acc == null) {
					this.SetInfoBox ("Invalid wallet account\n");
					return;
				}

				if (_default_payments != null) {
					foreach (RipplePaymentTransaction payment in _default_payments) {
						payment.Account = acc;
					}
				}

				if (paymentstree1._payments_tuple != null) {
					foreach (RipplePaymentTransaction payment in paymentstree1._payments_tuple.Item1) {
						payment.Account = acc;
					}
				}
			};

			this.submitButton.Clicked += delegate {
				
				Task.Run ((Action)SubmitAll);
			};

			this.selectButton.Clicked += paymentstree1.Selectbutton_Clicked;

			this.removeButton.Clicked += paymentstree1.Removebutton_Clicked;

			this.resetButton.Clicked += ResetButton_Clicked;

			this.stopButton.Clicked += paymentstree1.Stopbutton_Clicked;
		}

		void ResetButton_Clicked (object sender, EventArgs e)
		{
			
			this.paymentstree1.SetPayments (_default_payments);
		}





		internal void SetLicenseType (LicenseType licenseType)
		{
			this._licenseType = licenseType;
		}


		private CancellationTokenSource tokenSource = null;
		public void SubmitAll ()
		{




#if DEBUG
			string method_sig = clsstr + nameof (SubmitAll) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PaymentPreviewSubmitWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif



	    		// Cancel any existing threads
			tokenSource?.Cancel ();

			// Create the tokens to cancel this thread
			tokenSource = new CancellationTokenSource ();
			var token = tokenSource.Token;




			TextHighlighter greenHighlighter = new TextHighlighter {
				Highlightcolor = ProgramVariables.darkmode ?
					TextHighlighter.CHARTREUSE :
					TextHighlighter.GREEN
			};

			TextHighlighter redHighlighter = new TextHighlighter {
				Highlightcolor = ProgramVariables.darkmode ?
					TextHighlighter.LIGHT_RED :
		    			TextHighlighter.RED
			};


			// U.I
			SetInfoBox (greenHighlighter.Highlight ("Submiting all payments"));







			#region wallet
			RippleWallet rw = walletswitchwidget1.GetRippleWallet();
			if (rw == null) {
#if DEBUG
				if (DebugIhildaWallet.OrderPreviewSubmitWidget) {
					Logging.WriteLog (method_sig + "w == null, returning\n");
				}
#endif

				this.SetInfoBox (redHighlighter.Highlight("No wallet selected"));


				return;
			}
			#endregion







			#region network
			NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();
			if (ni == null) {
				// TODO network interface

				this.SetInfoBox (redHighlighter.Highlight("No network"));

				return;
			}

			#endregion







			#region license
			SetInfoBox (greenHighlighter.Highlight("Verifying License"));

			bool ShouldContinue = LeIceSense.LastDitchAttempt (rw, _licenseType);
			if (!ShouldContinue) {
				return;
			}
			#endregion






			#region sequence
			uint? s = AccountInfo.GetSequence (
	    			rw.GetStoredReceiveAddress (), 
				ni, 
		    		token);


			if (s==null) {
				SetInfoBox (redHighlighter.Highlight("Unable to retrieve sequence from network"));
				return;
			}


			uint se = Convert.ToUInt32 (s);
			#endregion





			#region password


			SetInfoBox (greenHighlighter.Highlight("Requesting password"));

			PasswordAttempt passwordAttempt = new PasswordAttempt ();

			passwordAttempt.InvalidPassEvent += (object sender, EventArgs e) =>
			{
				bool shou = AreYouSure.AskQuestionNonGuiThread (
				"Invalid password",
				"Unable to decrypt seed. Invalid password.\nWould you like to try again?"
				);
			};

			passwordAttempt.MaxPassEvent += (object sender, EventArgs e) =>
			{
				string mess = "Max password attempts";

				MessageDialog.ShowMessage (mess);

				SetInfoBox (redHighlighter.Highlight(mess));
				//WriteToOurputScreen ("\n" + mess + "\n");
			};


			DecryptResponse response = passwordAttempt.DoRequest (rw, token);



			RippleIdentifier rsa = response?.Seed;

			if (rsa?.GetHumanReadableIdentifier () == null) {

				return;
			}

			#endregion

			SoundSettings settings = SoundSettings.LoadSoundSettings ();

			for (int index = 0; index < this.paymentstree1._payments_tuple.Item1.Length; index++) {


				if (token.IsCancellationRequested) {
					//this.paymentstree1.stop = false;
					return;
				}

				// determine if selected
				bool selected = this.paymentstree1._payments_tuple.Item2 [index];

				// skip if selected
				if (!selected) {
					continue;
				}


				bool succeeded = this.paymentstree1.SubmitPaymentAtIndex (
					index, 
					se++, 
		    			ni, 
		    			token, 
					rsa);





				if (!succeeded) {

					if (settings != null && settings.HasOnTxFail && settings.OnTxFail != null) {

						Task.Run (delegate {

							SoundPlayer player = new SoundPlayer (settings.OnTxFail);
							player.Load ();
							player.Play ();
						});

					}

					return;
				}



				if (settings != null && settings.HasOnTxSubmit && settings.OnTxSubmit != null) {

					Task.Run (delegate {
						SoundPlayer player = new SoundPlayer (settings.OnTxSubmit);
						player.Load ();
						player.Play ();
					});

				}


			}





		}

		public void SetInfoBox (string markup)
		{
			Gtk.Application.Invoke (
				delegate {
					infoBarlabel.Markup = markup;
					infoBarlabel.Visible = true;
				}	
			);
		}


		public void SetPayments ( IEnumerable <RipplePaymentTransaction> payments )
		{
			SetPayments (payments, false);
		}
		public void SetPayments (IEnumerable <RipplePaymentTransaction> payments, bool isSelectDefault)
		{
			this.paymentstree1.SetPayments (payments.ToArray(), isSelectDefault);

			this.SetDefaultPayments (payments.ToArray());
		}

		private RipplePaymentTransaction [] _default_payments = null;

		public void SetDefaultPayments (RipplePaymentTransaction [] payments)
		{
			_default_payments = payments;
		}

		public void SetRippleWallet (RippleWallet rippleWallet)
		{
			
			this.walletswitchwidget1.SetRippleWallet (rippleWallet);
		}

		//private RippleWallet _rippleWallet = null;

		private LicenseType _licenseType {
			get;
			set;
		}

#if DEBUG
		public string clsstr = nameof (PaymentPreviewSubmitWidget) + DebugRippleLibSharp.colon;
#endif
	}
}

