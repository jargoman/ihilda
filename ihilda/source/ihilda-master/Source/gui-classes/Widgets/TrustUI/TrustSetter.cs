using System;
using System.Threading.Tasks;
using RippleLibSharp.Transactions.TxTypes;
using RippleLibSharp.Keys;
using RippleLibSharp.Transactions;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using IhildaWallet.Networking;
using RippleLibSharp.Util;
using IhildaWallet.Util;
using RippleLibSharp.Trust;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TrustSetter : Gtk.Bin
	{
		public TrustSetter ()
		{
			#if DEBUG
			if (DebugIhildaWallet.TrustSetter) {
				Logging.WriteLog(clsstr + "new");
			}
			#endif

			this.Build ();



			#if DEBUG
			if (DebugIhildaWallet.TrustSetter) {
				Logging.WriteLog(clsstr + "build complete");
			}
			#endif

			this.button2.Clicked += (object sender, EventArgs e) => {
				#if DEBUG
				if (DebugIhildaWallet.TrustSetter) {
					Logging.WriteLog("Set Trust Button clicked");
				}
#endif
				//Task.Run (
				//	(System.Action)SetTrust
				//);

				SetTrust ();
			};

			this.checkbutton5.Toggled += (object sender, EventArgs e) => {
				
				label1.Visible = checkbutton5.Active;
				label2.Visible = checkbutton5.Active;
				comboboxentry4.Visible = checkbutton5.Active;
				comboboxentry5.Visible = checkbutton5.Active;
				label7.Visible = checkbutton5.Active;
				label6.Visible = checkbutton5.Active;
			};


			this.bitstampUSDbutton.Clicked += (sender, e) => {

				this.SetTrustLine (
					new TrustLine () { 
						currency = "USD",
						account = "rvYAfWj5gh67oV6fW32ZzP3Aw4Eubs59B",
						
					}
				);
			};

			this.bitstampBTCbutton.Clicked += (sender, e) => {
				this.SetTrustLine (
					new TrustLine () {
						currency = "BTC",
						account = "rvYAfWj5gh67oV6fW32ZzP3Aw4Eubs59B",

					}
				);
			};

			this.gatehubUSDbutton.Clicked += (sender, e) => {
				this.SetTrustLine (
					new TrustLine () {
						currency = "USD",
						account = "rhub8VRN55s94qWKDv6jmDy1pUykJzF3wq",

					}
				);
			};

			this.gatehubBTCbutton.Clicked += (sender, e) => {
				this.SetTrustLine (
					new TrustLine () {
						currency = "BTC",
						account = "rchGBxcD1A1C2tdxF6papQYZ8kjRKMYcL",

					}
				);
			};

			this.gatehubEURbutton.Clicked += (sender, e) => {
				this.SetTrustLine (
					new TrustLine () {
						currency = "EUR",
						account = "rhub8VRN55s94qWKDv6jmDy1pUykJzF3wq",

					}
				);
			};

			this.gatehubETHbutton.Clicked += (sender, e) => {
				this.SetTrustLine (
					new TrustLine () {
						currency = "ETH",
						account = "rcA8X3TVMST1n3CJeAdGk1RdRCHii7N2h",

					}
				);
			};

			this.gatehubDASHbutton.Clicked += (sender, e) => {
				this.SetTrustLine (
					new TrustLine () {
						currency = "DASH",
						account = "rcXY84C4g14iFp6taFXjjQGVeHqSCh9RX",

					}
				);
			};

			this.gatehubREPbutton.Clicked += (sender, e) => {
				this.SetTrustLine (
					new TrustLine () {
						currency = "REP",
						account = "rckzVpTnKpP4TJ1puQe827bV3X4oYtdTP",

					}
				);
			};

		}
		#if DEBUG
		private static readonly String clsstr = nameof (TrustSetter) + DebugRippleLibSharp.colon;
		#endif

		public void HideRipplingWidgets () {
			
			label1.Visible = false;
			label2.Visible = false;
			comboboxentry4.Visible = false;
			comboboxentry5.Visible = false;
			label7.Visible = false;
			label6.Visible = false;

		}

		public void SetTrust ()
		{
			#if DEBUG
			String method_sig = clsstr + nameof (SetTrust) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.TrustSetter) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
			#endif
			RippleCurrency rippleCurrency = GetDenominated ();

			if (rippleCurrency == null) {
				#if DEBUG
				if (DebugIhildaWallet.TrustSetter) {
					Logging.WriteLog(method_sig + "Denominated Currency equals null");
				}
				#endif
				return;
			}

			bool allowRippling = checkbutton5.Active;
			UInt32? qin = null;
			UInt32? qout = null;
			if (allowRippling) {
				String i = comboboxentry4.ActiveText;
				qin = ProcessQuality (i, "in");

				String o = comboboxentry4.ActiveText;
				qout = ProcessQuality (o, "out");

			}

			if (qin == null) {
				qin = 0;

			}

			if (qout == null) {
				qout = 0;
			}

			//RippleWallet rw = MainWindow.currentInstance.getRippleWallet();
			RippleWallet rw = _rippleWallet;
			// TODO double check intended address.

			if (rw == null) {
				// todo debug
				return;
			}

			/*
			RippleSeedAddress seed = rw.GetDecryptedSeed ();

			if (seed == null) {
				#if DEBUG
				if (DebugIhildaWallet.TrustSetter) {
					Logging.WriteLog(method_sig + "seed == null");
				}
				#endif
				return;
			}
			*/

			NetworkInterface ni = NetworkController.GetNetworkInterfaceGuiThread();

			if (ni == null) {
				//NetworkController.doNetworkingDialogNonGUIThread ();
				return;
			}

			SignOptions opts = SignOptions.LoadSignOptions();
			uint lls = 0;
			if (opts != null) {
				lls = opts.LastLedgerOffset;
			}

			if (lls < 5) {
				lls = SignOptions.DEFAUL_LAST_LEDGER_SEQ;
			}

			LicenseType licenseT = Util.LicenseType.TRUST;
			if (LeIceSense.IsLicenseExempt (rippleCurrency)) {
				licenseT = LicenseType.NONE;
			}

			RippleTrustSetTransaction trustx = new RippleTrustSetTransaction( rw.GetStoredReceiveAddress(),rippleCurrency, qin, qout);

			TransactionSubmitWindow transactionSubmitWindow = new TransactionSubmitWindow (rw, licenseT);
			transactionSubmitWindow.SetTransactions (trustx);
			//trustx.autoRequestFee (ni);

			/*
			trustx.AutoRequestSequence (rw.GetStoredReceiveAddress(), ni);
			Tuple<UInt32,UInt32> f = FeeSettings.GetFeeAndLastLedgerFromSettings ( ni );
			if (f == null) {
				return;
			}
			trustx.fee = f.Item1.ToString ();
			trustx.LastLedgerSequence = f.Item2 + lls;

			//trustx.sign(seed);

			if (opts.UseLocalRippledRPC) {
				Logging.WriteLog("Signing using rpc");
				trustx.SignLocalRippled (seed);
				Logging.WriteLog ("Signed rpc");
			}

			else {
				Logging.WriteLog("Signing using RippleLibSharp");
				trustx.Sign(seed);
				Logging.WriteLog("Signed RippleLibSharp");

			}

			Task< Response <RippleSubmitTxResult>> task = NetworkController.UiTxNetworkSubmit ( trustx, ni );
			task.Wait ();
			// todo option to save and submit later
			//trustx.submit(ni);

			*/

		}

		private UInt32? ProcessQuality (String value, String input)
		{
			#if DEBUG
			String method_sig = clsstr + "processQuality( value=" + DebugIhildaWallet.ToAssertString(value) + ", inout="  + DebugIhildaWallet.ToAssertString(input) + ") : ";
			if (DebugIhildaWallet.TrustSetter) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin); 
			}
			#endif
			//String i = comboboxentry4.ActiveText;
			UInt32? q = RippleCurrency.ParseUInt32(value);
			if (q == null) {
				MessageDialog.ShowMessage ("Quality " + input + " is formatted incorrectly \n");

				#if DEBUG
				if (DebugIhildaWallet.TrustSetter) {
					Logging.WriteLog(method_sig + "q == null, returning null");
				}
				#endif
				return null;
			}

			if ( q > 1999999999) {
				// todo alert user
				#if DEBUG
				if (DebugIhildaWallet.TrustSetter) {
					Logging.WriteLog(method_sig + "q > 1999999999");
				}
				#endif
				return null;
			}

			#if DEBUG
			if (DebugIhildaWallet.TrustSetter) {
				Logging.WriteLog(method_sig + "Quality " + input + " =" + q.ToString());
			}
			#endif
			return q;


		}

		public RippleCurrency GetDenominated ()
		{
			#if DEBUG
			String method_sig = clsstr + nameof (GetDenominated) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.TrustSetter) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
			#endif

			RippleAddress issuerAddress;

			String currency = comboboxentry1.ActiveText;
			if (currency == null) {
				#if DEBUG
				// todo debug
				if (DebugIhildaWallet.TrustSetter) {
					Logging.WriteLog(method_sig + "currency == null");
				}
				#endif

				return null;
			}

			currency = currency.Trim();

			if (currency.Equals("")) {
				// todo debug
				#if DEBUG
				if (DebugIhildaWallet.TrustSetter) {
					Logging.WriteLog(method_sig + "currency is empty");
				}
				#endif
				return null;
			}

			String issuer = comboboxentry2.ActiveText;
			if (issuer == null) {
				 // todo debug
				#if DEBUG
				if (DebugIhildaWallet.TrustSetter) {
					Logging.WriteLog(method_sig + "issuer == null");
				}
				#endif
				return null;
			}

			issuer = issuer.Trim();

			try {
				issuerAddress = new RippleAddress(issuer);
			}

			catch (FormatException e) {
				#if DEBUG
				Logging.WriteLog(method_sig + e.Message);
				#endif
				MessageDialog.ShowMessage("Issuer address error : " + e.Message);
				return null;
			}


			String amount = comboboxentry3.ActiveText;
			if (amount == null) {
				#if DEBUG
				if (DebugIhildaWallet.TrustSetter) {
					Logging.WriteLog(method_sig + "amount == null");
				}
				#endif
				return null;
			}

			amount = amount.Trim();

			if (amount.Equals("")) {
				// todo debug
				#if DEBUG
				if (DebugIhildaWallet.TrustSetter) {
					Logging.WriteLog(method_sig + "amount is empty");
				}
				#endif
				return null;
			}

			Decimal? d = RippleCurrency.ParseDecimal(amount);
			if (d==null) {
				
				MessageDialog.ShowMessage ("Amount entered is not a valid decimal \n");

				#if DEBUG
				if (DebugIhildaWallet.TrustSetter) {
					Logging.WriteLog(method_sig + "Unable to parse amount");
				}
				#endif
				return null;
			}

			try {
				RippleCurrency denar = new RippleCurrency((Decimal)d,issuerAddress,currency);
				#if DEBUG
				if (DebugIhildaWallet.TrustSetter) {
					Logging.WriteLog("erase me");
				}
				#endif
				return denar;
			}

			catch (Exception e) {
				Logging.WriteLog(e.Message);
				return null;
			}

		}

		public void SetTrustLine (TrustLine trustLine)
		{
			RippleCurrency currency = new RippleCurrency {
				amount = trustLine.GetBalanceAsDecimal (),
				SelfLimit = trustLine.limit,
				currency = trustLine.currency,
				issuer = trustLine.account
			};

			comboboxentry1.Entry.Text = trustLine.currency;
			comboboxentry2.Entry.Text = trustLine.account;
			comboboxentry3.Entry.Text = trustLine.GetBalanceAsDecimal ().ToString();
			comboboxentry4.Entry.Text = trustLine.quality_in.ToString();
			comboboxentry5.Entry.Text = trustLine.quality_out.ToString ();

		}

		public void SetRippleWallet (RippleWallet rippleWallet)
		{
			this._rippleWallet = rippleWallet;
		}

		private RippleWallet _rippleWallet {
			get;
			set;
		}

	}
}

