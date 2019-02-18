/*
 *	License : Le Ice Sense 
 */


using Gtk;
using System;
using System.Threading;
using System.Threading.Tasks;
using IhildaWallet;
using IhildaWallet.Util;
using RippleLibSharp.Keys;
using RippleLibSharp.Result;

using RippleLibSharp.Commands.Accounts;

using RippleLibSharp.Network;
using IhildaWallet.Networking;

using RippleLibSharp.Transactions;
using RippleLibSharp.Transactions.TxTypes;
using RippleLibSharp.Util;
using static IhildaWallet.MemoCreateDialog;
using System.Collections.Generic;
using System.Linq;
using RippleLibSharp.Commands.Subscriptions;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SendIce : Gtk.Bin
	{
		public SendIce ()
		{

			this.Build ();

			this.issuerLabel.Text = LeIceSense.LICENSE_ISSUER;
			this.label1.Text = "<u>" + LeIceSense.LICENSE_CURRENCY + "</u>";

			this.label1.UseMarkup = true;

			this.sendICEButton.Clicked += OnSendICButtonClicked;

			this.addmemobutton.Clicked += (object sender, EventArgs e) => {

				SelectableMemoIndice createdMemo = null;
				using (MemoCreateDialog memoCreateDialog = new MemoCreateDialog ()) {
					try {
						ResponseType resp = (ResponseType)memoCreateDialog.Run ();


						if (resp != ResponseType.Ok) {

							return;
						}
						createdMemo = memoCreateDialog.GetMemoIndice ();
						this.AddMemo (createdMemo);
					} catch (Exception ee) {
						throw ee;
					} finally {
						memoCreateDialog?.Destroy ();
					}
				}




			};

			clearmemobutton.Clicked += (object sender, EventArgs e) => {
				ListStore.Clear ();

				Memos = null;

			};

			Task.Factory.StartNew ( () => {
				var token = TokenSource.Token;
				while (!token.IsCancellationRequested) {
					if (TokenSource == null) {
						return;
					}

					for (int i = 0; i < 5; i++) {
						WaitHandle.WaitAny (
							new WaitHandle [] {
								LedgerTracker.LedgerResetEvent,
								token.WaitHandle
							},
							6000
			    			);
					}

					//await Task.Delay (30000, token);
					UpdateBalance ();
				}
			}
			);

			CellRendererToggle rendererToggle = new CellRendererToggle () {
				Activatable = true
			};

			CellRendererText cellRendererText = new CellRendererText ();

			treeview1.AppendColumn ("Enabled", rendererToggle, "active", 0);
			treeview1.AppendColumn ("MemoType", cellRendererText, "text", 1);
			treeview1.AppendColumn ("MemoFormat", cellRendererText, "text", 2);
			treeview1.AppendColumn ("MemoData", cellRendererText, "text", 3);

			ListStore = new ListStore (
					typeof (bool),
					typeof (string),
		    			typeof (string),
					typeof (string)
				);


			var memo = Program.GetClientMemo ();
			this.AddMemo (memo);
		}

		~SendIce ()
		{
			TokenSource.Cancel ();
			TokenSource.Dispose ();
		}

		private CancellationTokenSource TokenSource = new CancellationTokenSource ();




		private IEnumerable<SelectableMemoIndice> Memos {
			get;
			set;
		}

		public void AddMemo (SelectableMemoIndice indice)
		{
			List<SelectableMemoIndice> memoIndices = Memos?.ToList () ?? new List<SelectableMemoIndice> ();
			indice.IsSelected = true;
			memoIndices.Add (indice);

			SetMemos (memoIndices);

		}

		public void SetMemos (IEnumerable<SelectableMemoIndice> Memos)
		{
			Gtk.Application.Invoke (
				delegate {
					ListStore.Clear ();

					foreach (SelectableMemoIndice memoIndice in Memos) {
						ListStore.AppendValues (
							memoIndice.IsSelected,
							memoIndice?.GetMemoTypeAscii (),
							memoIndice?.GetMemoFormatAscii (),
							memoIndice?.GetMemoDataAscii ()
						);
					}

					this.Memos = Memos;
					this.treeview1.Model = ListStore;

				}
			);

		}

		Gtk.ListStore ListStore {
			get;
			set;
		}


		protected void OnSendICButtonClicked (object sender, EventArgs e)
		{
			#if DEBUG
			string method_sig = clsstr + "OnSendICButtonClicked : ";
			if (DebugIhildaWallet.SendIce) {

			}
			#endif

			RippleWallet rw = _rippleWallet;
			if (rw == null) {
				// TODO
				return;
			}

			string sendingAccount = rw.GetStoredReceiveAddress ();

			string amount = amountentry?.Text;
			string destination = destinationentry?.Text;

			if (amount == null || amount.Trim().Equals("")) {
				MessageDialog.ShowMessage ("You must specify an amount to send");
				return;
			}


			if (destination == null || destination.Trim().Equals("")) {
				MessageDialog.ShowMessage ("You must specify a destination address");
				return;
			}


			string destinationTag = destinationTagcomboboxentry?.ActiveText;

			uint? DestTag = null;
			UInt32 res = 0;

			if (!string.IsNullOrWhiteSpace (destinationTag)) {
				destinationTag = destinationTag.Trim ();

				bool hasDest = UInt32.TryParse (destinationTag, out res);
				if (!hasDest) {
					return;
				}

				if (res == 0) {
					string msg = "<span fgcolor>You've specified a destination tag of zero</span>. A destination tag is used by the recipiant to distinguisg payments from one another. Ask your recepient what destination tag if any to use\nWould you like to continue with a destination tag of zero?";
					bool b = AreYouSure.AskQuestion ("destination tag is zero", msg);

					if (!b) {
						return;
					}
				}
				DestTag = new uint? (res);

			}




			Decimal? dee = RippleCurrency.ParseDecimal (amount);  





			if (dee == null) {
				MessageDialog.ShowMessage ( "Send Amount is not a valid Decimal" );
				return;
			} 


			if (dee < 0) {
				MessageDialog.ShowMessage("Sending negative amounts is not supported. Please enter a valid amount");
				return;
			}



			if (string.IsNullOrWhiteSpace(destination)) {
				MessageDialog.ShowMessage ("You must specify a destination address : rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
				return;
			}

			destination = destination?.Trim ();

			try {
				#pragma warning disable 0219
				RippleAddress destcheck = new RippleAddress( destination );
				#pragma warning restore 0219

			}

#pragma warning disable 0168
			catch (Exception exce) {
#pragma warning restore 0168
#if DEBUG
				if (DebugIhildaWallet.SendIce) {
					Logging.ReportException(method_sig, exce);
				}
#endif

				// it'll never be null but an exception was thrown with destination in the try block
				MessageDialog.ShowMessage ("Invalid destination address : " + destination ?? "null" ); 
					 
				return;
			}

			RippleAddress payee = new RippleAddress(destination);
			RippleCurrency sendMax = null;

			RippleCurrency amnt = null;
			try {
				amnt = new RippleCurrency((Decimal)dee, destination, LeIceSense.LICENSE_CURRENCY);
				sendMax = new RippleCurrency ((Decimal)dee, sendingAccount, LeIceSense.LICENSE_CURRENCY);
			}

			#pragma warning disable 0168
			catch (Exception ex) {
				#pragma warning restore 0168

				#if DEBUG
				if (DebugIhildaWallet.SendIce) {
					Logging.ReportException(method_sig, ex);
				}
				#endif
			}



			RipplePaymentTransaction tx =
				new RipplePaymentTransaction (
					rw.GetStoredReceiveAddress (),
					payee,
					amnt,
					sendMax
				) {
					DestinationTag = DestTag
				};

			tx.Memos = Memos?.Where ((SelectableMemoIndice arg) => arg.IsSelected).ToArray();

			RipplePaymentTransaction [] arr = { tx } ;



			PaymentSubmitWindow paymentWindow = new PaymentSubmitWindow (rw, LicenseType.NONE);
			paymentWindow.SetPayments( arr);
			paymentWindow.Show ();



		}


		public void UpdateBalance () {

			Task.Run (() => {

				string ra = _rippleWallet?.GetStoredReceiveAddress ();
				if (ra == null) {
					return;
				}

				NetworkInterface ni = NetworkController.CurrentInterface;
				if (ni == null) {
					return;
				}

			Task<Response<AccountLinesResult>> task = AccountLines.GetResult (
				ra,
				ni,
				new CancellationToken ()
				);

				if (task == null) {
					return;
				}

				task.Wait ();

				Response<AccountLinesResult> resp = task?.Result;

				AccountLinesResult result = resp?.result;

				if (result == null) {
					return;
				}

				RippleCurrency rc = result.GetBalanceAsCurrency (
					LeIceSense.LICENSE_CURRENCY,
					LeIceSense.LICENSE_ISSUER
				);

				if (rc == null) {
					return;
				}
				Application.Invoke (
					delegate {
						this.balancelabel.Text = rc.amount.ToString ();
						//this.issuerLabel.Text = LeIceSense.LICENSE_ISSUER;
					}

				);


			});




		}

		public void SetRippleWallet (RippleWallet rippleWallet)
		{
			this._rippleWallet = rippleWallet;
			UpdateBalance ();
		}

		private RippleWallet _rippleWallet {
			get;
			set;
		}


		#if DEBUG
		private const string clsstr = nameof (SendIce) + DebugRippleLibSharp.colon;
		#endif
	}
}

