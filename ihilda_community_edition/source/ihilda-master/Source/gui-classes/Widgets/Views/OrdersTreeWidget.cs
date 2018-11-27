using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using RippleLibSharp.Network;
using IhildaWallet.Networking;
using RippleLibSharp.Keys;
using RippleLibSharp.Transactions;

using RippleLibSharp.Transactions.TxTypes;

using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Result;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class OrdersTreeWidget : Gtk.Bin
	{
		public OrdersTreeWidget ()
		{
			this.Build ();

			if (this.openorderstree1 == null) {
				this.openorderstree1 = new OpenOrdersTree ();
				this.openorderstree1.Show ();
				vbox1.Add (openorderstree1);
			}

			this.infoBarLabel.UseMarkup = true;
			this.infoBarLabel.Hide ();

			this.button74.Clicked += (object sender, EventArgs e) => {
				tokenSource.Cancel ();
				tokenSource.Dispose ();
				tokenSource = null;

				button74.Visible = false;

				cancelbutton.Visible = false;
				selectButon.Visible = false;

			};



			this.syncbutton.Clicked += (object sender, EventArgs e) => {
				#if DEBUG
				string event_sig = clsstr + "syncbutton.Clicked : ";
				if (DebugIhildaWallet.OrdersTreeWidget) {
					Logging.WriteLog (event_sig + DebugRippleLibSharp.beginn);
				}
				#endif

				// todo verify user entered address for correctness 
				string addr = this.comboboxentry1.ActiveText;

				if (string.IsNullOrWhiteSpace(addr)) {
					infoBarLabel.Markup = "<span fgcolor=\"red\">No address specified</span>";
					infoBarLabel.Visible = true;
					return;
				}

				infoBarLabel.Markup = "Retrieving orders for " + addr;
				infoBarLabel.Visible = true;
				selectButon.Visible = false;
				cancelbutton.Visible = false;
				button74.Visible = true;

				OAParam oap = new OAParam(addr);


				Thread th = new Thread( 
					new ParameterizedThreadStart( SyncClicked )
				);

				th.Start( oap );

			};

			this.selectButon.Clicked += (object sender, EventArgs e) => {
				#if DEBUG
				string event_sig = clsstr + "selectButton.Clicked : ";
				if (DebugIhildaWallet.OrdersTreeWidget) {
					Logging.WriteLog(event_sig + DebugRippleLibSharp.begin);
				}
				#endif

				var offs = this.openorderstree1?._offers;
				if ( offs == null || offs.Length == 0) {
					//MessageDialog.showMessage("");
					return;
				}

				bool allselected = true;

				foreach (AutomatedOrder ao in offs) {
					if (ao == null) {
						continue;
					}
					if (!ao.Selected) {
						allselected = false;
						break;

					}
				}

				foreach (AutomatedOrder ao in offs) {
					if (ao == null) {
						continue;
					}
					ao.Selected = !allselected;
				}

				this.openorderstree1.SetOffers (offs);

			};


			this.cancelbutton.Clicked += delegate {
				//ThreadStart ts = new ThreadStart( cancelSelected );
				//Thread tt = new Thread(ts);
				//tt.Start();
				infoBarLabel.Markup = "<span>Cancelling selected orders</span>";
				infoBarLabel.Visible = true;

				button74.Visible = true;
				cancelbutton.Visible = false;
				selectButon.Visible = false;

				Task.Run((System.Action)CancelSelected);
			};


		}

		public void SetRippleWallet (RippleWallet rippleWallet) {

			this._rippleWallet = rippleWallet;

			this.comboboxentry1.Entry.Text = rippleWallet.GetStoredReceiveAddress ()?.ToString() ?? "";
			button74.Visible = false;
			selectButon.Visible = false;
			cancelbutton.Visible = false;

			this.ClearGui ();

		}

		public void CancelSelected () {
			#if DEBUG
			string method_sig = clsstr + nameof (CancelSelected) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.OrdersTreeWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			this.tokenSource?.Cancel ();
			this.tokenSource = new CancellationTokenSource ();
			CancellationToken token = tokenSource.Token;

			if (this.openorderstree1?._offers == null ) {
				return;
			}

			// TODO 

			RippleWallet rw = _rippleWallet;
			if (rw == null) {

#if DEBUG
				if (DebugIhildaWallet.OrdersTreeWidget) {
					Logging.WriteLog (method_sig + "w == null, returning\n");
				}
#endif

				return;
			}

			NetworkInterface ni = NetworkController.CurrentInterface;

			if (ni == null) {
				// TODO network interface
			}

			uint se = Convert.ToUInt32( AccountInfo.GetSequence ( rw.GetStoredReceiveAddress(), ni, token ));

			RippleIdentifier rsa = rw.GetDecryptedSeed();
			for ( int index = 0; index < this.openorderstree1._offers.Length;  index++) {

				/*
				if (stop) {
					stop = false;
					return;
				}
				*/


				AutomatedOrder off = this.openorderstree1._offers [index];
				off.Account = rw.GetStoredReceiveAddress ();
				if (!off.Selected) {
					continue;
				}


				bool suceeded = this.openorderstree1.CancelOrderAtIndex ( rw.GetStoredReceiveAddress(), index, se++, ni, rsa );
				if (!suceeded) {
					return;
				}
			}

			Gtk.Application.Invoke (
					delegate {

						progressbar1.Fraction = 1;
						infoBarLabel.Markup = "";
						infoBarLabel.Visible = false;

						cancelbutton.Visible = true;
						selectButon.Visible = true;
						button74.Visible = false;
					}
				);

		}






		public void ClearGui () {
			this.openorderstree1.ClearOffers ();

			Gtk.Application.Invoke (
				delegate { 
					this.infoBarLabel.Hide(); 
					this.infoBarLabel.Text = "";
				}
			);
		}


		private CancellationTokenSource tokenSource = null;
		public void SyncClicked ( object address ) {


#if DEBUG

			string method_sig = 
				clsstr + 
				nameof (SyncClicked) + 
				DebugRippleLibSharp.left_parentheses + nameof(address) + DebugRippleLibSharp.right_parentheses;

			if (DebugIhildaWallet.OrdersTreeWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}


#endif


			if (!(address is OAParam oap)) {
#if DEBUG
				if (DebugIhildaWallet.OrdersTreeWidget) {
					Logging.WriteLog (method_sig + "addr == null, returning\n");
				}
#endif
				return;
			}

			tokenSource?.Cancel ();
			tokenSource = new CancellationTokenSource ();
			CancellationToken token = tokenSource.Token;



			string addr = oap.Address;

			RippleAddress ra = null;
			try {

				ra = new RippleAddress(addr);
			}

			#pragma warning disable 0168
			catch (FormatException fe) {
				#pragma warning restore 0168

				#if DEBUG
				MessageDialog.ShowMessage(addr + " is not a properlay formatted ripple address\n" );
				if (DebugIhildaWallet.OrdersTreeWidget) {
					Logging.WriteLog(fe.Message);
				}
				#endif
				return;
			}

			#pragma warning disable 0168
			catch (Exception ex) {
#pragma warning restore 0168

				string mess = "Error processing ripple address";

				Gtk.Application.Invoke (
					delegate {

						infoBarLabel.Markup = "<span fgcolor=\"red\">" + mess + "</span>";
						infoBarLabel.Visible = true;

						progressbar1.Fraction = 0;
						

						cancelbutton.Visible = true;
						selectButon.Visible = true;
						button74.Visible = false;
					}
				);


#if DEBUG
				MessageDialog.ShowMessage (mess);
				if (DebugIhildaWallet.OrdersTreeWidget) {
					Logging.WriteLog(ex.Message);
				}
				#endif
				return;
			}

			finally {
				#if DEBUG
				if (DebugIhildaWallet.OrdersTreeWidget) {
					Logging.WriteLog (method_sig + nameof (ra) + DebugRippleLibSharp.equals + DebugIhildaWallet.ToAssertString(ra));
				}
				#endif
			}

			if (ra == null) {
				#if DEBUG
				if (DebugIhildaWallet.OrdersTreeWidget) {
					Logging.WriteLog (method_sig + nameof (ra) + DebugRippleLibSharp.equals + DebugRippleLibSharp.null_str + DebugRippleLibSharp.comma + DebugRippleLibSharp.returning);
				}
				#endif
				return;
			}

			NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread (); //getNetworkInterfaceNonGuiThread();
			if (ni == null) {
				return;
			}


			Task <IEnumerable< Response <AccountOffersResult> > > task = AccountOffers.GetFullOfferList (addr, ni, token);
			while (!task.IsCompleted && !token.IsCancellationRequested) {
				Gtk.Application.Invoke (
					delegate {

						progressbar1.Pulse ();
					}
				);

				try {
					task.Wait (500, token);
				} catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException) {

				}
			}

			Gtk.Application.Invoke (
				delegate {
					infoBarLabel.Markup = "Recieved result, processing\n";
					infoBarLabel.Visible = true;
					progressbar1.Pulse ();

				}
			);

			if (task == null) {
				return;
			}

			IEnumerable <Response <AccountOffersResult>> responses = task.Result;
			if (responses == null) {
				return;
			}
			Response<AccountOffersResult> first = responses.First ();
			if (first.HasError()) {
				Gtk.Application.Invoke (
					delegate {
						string DEFAULT_ERROR = "Error";
						this.infoBarLabel.Markup = "" +
							"<span fgcolor=\"red\">" + (first?.error_message ?? DEFAULT_ERROR) + "</span>";
						this.infoBarLabel.Show();

						progressbar1.Fraction = 0;

						cancelbutton.Visible = false;
						selectButon.Visible = false;
						button74.Visible = false;
					}
				);
				return;
			}

			Gtk.Application.Invoke (
					delegate {

						progressbar1.Pulse ();
					}
				);

			IEnumerable <Offer> firstOffs = first?.result?.offers;
			if (firstOffs == null) {
				
				return;
			}

			if (!firstOffs.Any()) {
				Gtk.Application.Invoke (
					delegate {
						this.infoBarLabel.Markup = 
							"<span fgcolor=\"red\">Server returned no orders for account " 
							+ (ra?.ToString() ?? "")
							+ "</span>";
						this.infoBarLabel.Show ();

						progressbar1.Fraction = 0;



						cancelbutton.Visible = false;
						selectButon.Visible = false;
						button74.Visible = false;

					}
				);
				return;
			}

			Gtk.Application.Invoke (
					delegate {

						progressbar1.Pulse ();
					}
				);

			//List<Offer> offers = new List<Offer> ();


			//var offs_list = (from Response< AccountOffersResult > res in responses
			                //where res?.result?.offers != null
			                 //select res.result.offers).SelectMany(c => c);

			var offs_list = responses.Where (
				res => res?.result?.offers != null
			).SelectMany ( res => res.result.offers);

			/*
			foreach (var offs in offs_list) {
				Gtk.Application.Invoke (
					delegate {

						progressbar1.Pulse ();
					}
				);
				offers.AddRange (offs);
			}
			

			if (offers == null) {
				return;
			}



			//AccountOffersResult aor = task?.Result?.result;

			#if DEBUG
			if (DebugIhildaWallet.OrdersTreeWidget) {
				Logging.WriteLog ( method_sig + "blockingSyncreturned" );
			}
#endif


			Gtk.Application.Invoke (
					delegate {

						progressbar1.Pulse ();
					}
				);



			var v = from Offer off in offers
			        select new AutomatedOrder (off);
			*/


			this.openorderstree1.SetOffers ( offs_list );

			Gtk.Application.Invoke (
					delegate {

						progressbar1.Fraction = 1;
						infoBarLabel.Markup = "";
						infoBarLabel.Visible = false;

						cancelbutton.Visible = true;
						selectButon.Visible = true;
						button74.Visible = false;
					}
				);

		}

		private RippleWallet _rippleWallet;


#pragma warning disable RECS0122 // Initializing field with default value is redundant
		//private bool stop = false;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

#if DEBUG
		private static string clsstr = nameof (OrdersTreeWidget) + DebugRippleLibSharp.colon;
		#endif
	}
}

