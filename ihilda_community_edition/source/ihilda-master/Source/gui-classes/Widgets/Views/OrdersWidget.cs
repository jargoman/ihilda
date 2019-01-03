using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using RippleLibSharp.Transactions;
using RippleLibSharp.Keys;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Result;
using RippleLibSharp.Util;
using RippleLibSharp.Network;
using IhildaWallet.Networking;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class OrdersWidget : Gtk.Bin
	{
		public OrdersWidget ()
		{
			#if DEBUG
			string method_sig = clsstr + nameof (OrdersWidget) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.OrdersWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
			#endif
			this.Build ();

			if (orderwidget1 == null) {
				orderwidget1 = new OrderWidget ();
				orderwidget1.Show ();
				vbox2.Add (orderwidget1);
			}

			if (orderwidget2 == null) {
				orderwidget2 = new OrderWidget ();
				orderwidget2.Show ();
				vbox2.Add (orderwidget2);
			}

			if (orderwidget3 == null) {
				orderwidget3 = new OrderWidget ();
				orderwidget3.Show ();
				vbox2.Add (orderwidget3);
			}

			if (orderwidget4 == null) {
				orderwidget4 = new OrderWidget ();
				orderwidget4.Show ();
				vbox2.Add (orderwidget4);
			}

			if (orderwidget5 == null) {
				orderwidget5 = new OrderWidget ();
				orderwidget5.Show ();
				vbox2.Add (orderwidget5);
			}

			if (orderwidget6 == null) {
				orderwidget6 = new OrderWidget ();
				orderwidget6.Show ();
				vbox2.Add (orderwidget6);
			}

			if (orderwidget7 == null) {
				orderwidget7 = new OrderWidget ();
				orderwidget7.Show ();
				vbox2.Add (orderwidget7);
			}

			if (orderwidget8 == null) {
				orderwidget8 = new OrderWidget ();
				orderwidget8.Show ();
				vbox2.Add (orderwidget8);
			}

			if (orderwidget9 == null) {
				orderwidget9 = new OrderWidget ();
				orderwidget9.Show ();
				vbox2.Add (orderwidget9);
			}

			if (orderwidget10 == null) {
				orderwidget10 = new OrderWidget ();
				orderwidget10.Show ();
				vbox2.Add (orderwidget10);
			}

			if (pagerwidget1 == null) {
				pagerwidget1 = new PagerWidget ();
				pagerwidget1.Show ();
				hbox11.Add (pagerwidget1);
				
			}

			this.infoBarLabel.UseMarkup = true;
			#if DEBUG
			if (DebugIhildaWallet.OrdersWidget) {
				Logging.WriteLog (method_sig + DebugIhildaWallet.buildComp);
			}
			#endif

			orderscash = new PageCache<Offer> ( nameof (OrdersWidget) );



			pagerwidget1.first.Clicked += (object sender, EventArgs e) => {
				#if DEBUG
				string event_sig = method_sig + "first button clicked : ";
				if (DebugIhildaWallet.OrdersWidget) {
					Logging.WriteLog(event_sig + DebugRippleLibSharp.begin);
				}
				#endif


				Task.Run((System.Action)FirstClicked);

			};

			pagerwidget1.previous.Clicked += (object sender, EventArgs e) => {
				#if DEBUG
				string event_sig = method_sig + "previous button clicked : ";
				if (DebugIhildaWallet.OrdersWidget) {
					Logging.WriteLog(event_sig + DebugRippleLibSharp.begin);
				}
				#endif


				Task.Run((System.Action)PreviousClicked);


			};

			pagerwidget1.next.Clicked += (object sender, EventArgs e) => {
				#if DEBUG
				string event_sig = method_sig + "next button clicked : ";
				if (DebugIhildaWallet.OrdersWidget) {
					Logging.WriteLog(event_sig + DebugRippleLibSharp.begin);
				}
				#endif


				Task.Run((System.Action)NextClicked);

			};

			pagerwidget1.last.Clicked += (object sender, EventArgs e) => {
				#if DEBUG
				string event_sig = method_sig + "last button clicked : ";
				if (DebugIhildaWallet.OrdersWidget) {
					Logging.WriteLog(event_sig + DebugRippleLibSharp.begin);
				}
				#endif


				Task.Run((System.Action)LastClicked);
			};

				


			this.syncbutton.Clicked += (object sender, EventArgs e) => {
				#if DEBUG
				string event_sig = clsstr + "syncbutton.Clicked : ";
				if (DebugIhildaWallet.OrdersWidget) {
					Logging.WriteLog (event_sig + DebugRippleLibSharp.beginn);
				}
				#endif

				// todo verify user entered address for correctness 
				string addr = this.comboboxentry1.ActiveText;
				/*
				try {
					RippleAddress ra = new RippleAddress (addr);
					if (ra == null) {
						throw new NullReferenceException("User likely entered an invalid address");
					}
				}
				catch ( Exception ex ) {
					#if DEBUG
					Logging.reportException(ex);
					#endif

					return;

				}*/
				OAParam oap = new OAParam(addr);


				Thread th = new Thread( 
					new ParameterizedThreadStart(SyncClicked)
				);

				th.Start(oap);


			};

			this.selectAllButton.Clicked += (object sender, EventArgs e) => {
				
				#if DEBUG
				string event_sig = clsstr + "selectAllButton.Clicked : ";
				if (DebugIhildaWallet.OrdersWidget) {
					Logging.WriteLog (event_sig + DebugRippleLibSharp.beginn);
				}
				#endif

				#region already
				if (
					this.orderwidget1.Selected == true &&

					this.orderwidget2.Selected == true &&

					this.orderwidget3.Selected == true &&

					this.orderwidget4.Selected == true &&

					this.orderwidget5.Selected == true &&

					this.orderwidget6.Selected == true &&

					this.orderwidget7.Selected == true &&

					this.orderwidget8.Selected == true &&

					this.orderwidget9.Selected == true &&

					this.orderwidget10.Selected == true
				) {
					this.orderwidget1.Selected = false;

					this.orderwidget2.Selected = false;

					this.orderwidget3.Selected = false;

					this.orderwidget4.Selected = false;

					this.orderwidget5.Selected = false;

					this.orderwidget6.Selected = false;

					this.orderwidget7.Selected = false;

					this.orderwidget8.Selected = false;

					this.orderwidget9.Selected = false;

					this.orderwidget10.Selected = false;
				}


				#endregion


				#region allToTrue
				this.orderwidget1.Selected = true;
						
				this.orderwidget2.Selected = true;
					
				this.orderwidget3.Selected = true;
					
				this.orderwidget4.Selected = true;
						
				this.orderwidget5.Selected = true;
					
				this.orderwidget6.Selected = true;
						
				this.orderwidget7.Selected = true;
						
				this.orderwidget8.Selected = true;
						
				this.orderwidget9.Selected = true;
						
				this.orderwidget10.Selected = true;
				#endregion
						




			};

			this.cancelSelectedButton.Clicked += (object sender, EventArgs e) => {


				List<Offer> list = new List<Offer>();
				for (int i = 0; i < _offers.Length; i++) {
					switch (i++) {
					case 0:
						if (!this.orderwidget1.Selected) {
							continue;
						}
						break;
					case 1:
						if (!this.orderwidget2.Selected) {
							continue;
						}
						break;
					case 2:
						if (!this.orderwidget3.Selected) {
							continue;
						}
						break;

					case 3:
						if (!this.orderwidget4.Selected) {
							continue;
						}
						break;
					case 4:
						if (!this.orderwidget5.Selected) {
							continue;
						}
						break;

					case 5:
						if (!this.orderwidget6.Selected) {
							continue;
						}
						break;
					case 6:
						if (!this.orderwidget7.Selected) {
							continue;
						}
						break;

					case 7:
						if (!this.orderwidget8.Selected) {
							continue;
						}
						break;
					case 8:
						if (!this.orderwidget9.Selected) {
							continue;
						}
						break;

					case 9:
						if (!this.orderwidget10.Selected) {
							continue;
						}

						break;

					}

					list.Add(_offers[i]);
				}


			};
		}



		public void FirstClicked () {
			#if DEBUG
			string method_sig = clsstr + nameof (FirstClicked) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.OrdersWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
			#endif
			SetOrdersGUI ( orderscash.GetfirstCache() );

			pagerwidget1.SetCurrentPage( orderscash.GetFirst() );
			orderscash.SetFirst();

			orderscash.Preload();
		}



		public void LastClicked () {
			#if DEBUG
			string method_sig = nameof (LastClicked) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.OrdersWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
			#endif
			SetOrdersGUI( orderscash.GetLastCache() );
			pagerwidget1.SetCurrentPage( orderscash.GetLast() );
			orderscash.SetLast();
			orderscash.Preload();
		}

		public void PreviousClicked () {
			#if DEBUG
			string method_sig = nameof (PreviousClicked) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.OrdersWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
			#endif
			if (orderscash == null) {
				return;
			}

			Offer[] offs = orderscash.GetPreviousCache();
			if (offs != null) {
				SetOrdersGUI(offs);

				pagerwidget1.SetCurrentPage(orderscash.GetPrevious());
				orderscash.SetPrevious();
				orderscash.Preload();
			}
		}

		public void NextClicked () {
			#if DEBUG
			string method_sig = clsstr + nameof(NextClicked) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.OrdersWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
			#endif
			if (orderscash == null) {
				// todo debug
				return;
			}

			Offer[] trlar = orderscash.GetNextCache();
			if (trlar != null) {
				SetOrdersGUI(trlar);

				pagerwidget1.SetCurrentPage(orderscash.GetNext());
				orderscash.SetNext();
				orderscash.Preload();
			}
		

		}


		/*public void setOffers ( Offer[] offerarray ) {*/
		public void SetOffers ( IEnumerable<Offer> offers) {
			#if DEBUG
			string method_sig = clsstr + nameof (SetOffers) + DebugRippleLibSharp.left_parentheses + nameof (offers) + DebugRippleLibSharp.array_brackets + nameof (offers) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.OrderWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
			#endif
			//String account = null;


			orderscash.Set(offers.ToArray());
			int num = orderscash.GetNumPages;
			this.SetNumPages(num);
			this.FirstClicked();

		}

		public void SetNumPages (int num) {
			#if DEBUG
			string method_sig = clsstr + nameof (SetNumPages) + DebugRippleLibSharp.left_parentheses + nameof (Int32) + DebugRippleLibSharp.space_char + nameof (num) + DebugRippleLibSharp.equals + num.ToString() + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.OrdersWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
			#endif
			this.pagerwidget1.SetNumberOfPages(num);
		}

		public void SetRippleWallet (RippleWallet rippleWallet) {

			this._rippleWallet = rippleWallet;
			this.comboboxentry1.Entry.Text = rippleWallet?.GetStoredReceiveAddress()?.ToString() ?? "";
			this.ClearGui ();

		}


		private CancellationTokenSource tokenSource = null;
		public void SyncClicked ( object address ) {
			#if DEBUG
			string method_sig = clsstr + nameof (SyncClicked) + DebugRippleLibSharp.left_parentheses + nameof (address) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.OrdersWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}

#endif

			var ts = new CancellationTokenSource ();
			CancellationToken token = ts.Token;
			tokenSource?.Cancel ();

			tokenSource = ts;


			if (!(address is OAParam oap)) {
#if DEBUG
				if (DebugIhildaWallet.AccountLinesWidget) {
					Logging.WriteLog (method_sig + "addr == null, returning\n");
				}
#endif
				return;
			}

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
				if (DebugIhildaWallet.OrderWidget) {
					Logging.WriteLog(fe.Message);
				}
				#endif
				return;
			}

			#pragma warning disable 0168
			catch (Exception ex) {
			#pragma warning restore 0168

				MessageDialog.ShowMessage("Error processing ripple address\n");
				#if DEBUG
				if (DebugIhildaWallet.OrderWidget) {
					Logging.WriteLog(ex.Message);
				}
				#endif
				return;
			}

			finally {
				#if DEBUG
				if (DebugIhildaWallet.OrdersWidget) {
					Logging.WriteLog (method_sig + "ra=" + DebugIhildaWallet.ToAssertString(ra));
				}
				#endif
			}

			if (ra == null) {
				#if DEBUG
				if (DebugIhildaWallet.OrdersWidget) {
					Logging.WriteLog (method_sig + "ra == null, " + DebugRippleLibSharp.returning);
				}
				#endif
				return;
			}

			if (token.IsCancellationRequested) {
				return;
			}

			NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread (); //getNetworkInterfaceNonGuiThread();

			Task <IEnumerable<Response<AccountOffersResult>> > task = AccountOffers.GetFullOfferList (addr, ni, token);
			task.Wait (token);


			if (token.IsCancellationRequested) {
				return;
			}

			IEnumerable<Response<AccountOffersResult>> responses = task.Result;

			Response<AccountOffersResult> first = responses.First ();
			if (first.HasError()) {
				Gtk.Application.Invoke (
					delegate {
						if (token.IsCancellationRequested) {
							return;
						}
						string DEFAULT_ERROR = "Error";
						this.infoBarLabel.Text = 
							"<span fgcolor=\"red\">" 
							+ first?.error_message ?? DEFAULT_ERROR
							+ "</span>";
						this.infoBarLabel.Show();
					}
				);
				return;
			}

			var firstOffs = first?.result?.offers;
			if (firstOffs == null) {
				// TODO good ol debug every possibility for production code. 
				return;
			}

			if (!firstOffs.Any()) {

				Gtk.Application.Invoke (
					delegate {
						string message = "<span fgcolor=\"red\">Server returned no orders for account "
							+ (ra?.ToString() ?? "")
							+ "</span>" ;

						this.infoBarLabel.Markup = message;
						this.infoBarLabel.Show();
					}
				);
				return;
			}

			var offerslist = from Response<AccountOffersResult> res in responses
			        where res?.result?.offers != null
			        select res.result?.offers;


			IEnumerable<Offer> offerss = offerslist.SelectMany ((IEnumerable<Offer> arg) => arg);

			/*
			List<Offer> offers = new List<Offer> ();

			foreach (var offs in offerslist) {
				offers.AddRange (offs);
			}

			if (offers == null) {
				return;
			}

			*/

			//AccountOffersResult aor = task?.Result?.result;

			#if DEBUG
			if (DebugIhildaWallet.OrdersWidget) {
				Logging.WriteLog (method_sig + "blockingSyncreturned");
			}
			#endif



			//this.setOffers( aor.offers);
			this.SetOffers (offerss);

		}



		public void ClearGui () {

			Gtk.Application.Invoke ( delegate {
				this.infoBarLabel.Hide();
				this.orderwidget1.Order = null;
				this.orderwidget2.Order = null;
				this.orderwidget3.Order = null;
				this.orderwidget4.Order = null;
				this.orderwidget5.Order = null;
				this.orderwidget6.Order = null;
				this.orderwidget7.Order = null;
				this.orderwidget8.Order = null;
				this.orderwidget9.Order = null;
				this.orderwidget10.Order = null;
			});


		}


#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private Offer [] _offers = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		public void SetOrdersGUI (Offer[] offers) {
			#if DEBUG
			string method_sig = clsstr + "setOrdersGUI (Offer[] offers) : ";
			if (DebugIhildaWallet.OrdersWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
			#endif

			this.ClearGui ();

			this._offers = offers;

			if (offers == null) {
				#if DEBUG
				if (DebugIhildaWallet.OrdersWidget) {
					Logging.WriteLog (method_sig + "offers == null, returning");

				}
				#endif

				return;
			}



			Gtk.Application.Invoke ( delegate {
				int x = 0;

				foreach (Offer of in offers) {
					switch (x++) {
					case 0:
						this.orderwidget1.Order = of;
						continue;
					case 1:
						this.orderwidget2.Order = of;
						continue;
					case 2:
						this.orderwidget3.Order = of;
						continue;
					case 3:
						this.orderwidget4.Order = of;
						continue;
					case 4:
						this.orderwidget5.Order = of;
						continue;
					case 5:
						this.orderwidget6.Order = of;
						continue;
					case 6:
						this.orderwidget7.Order = of;
						continue;
					case 7:
						this.orderwidget8.Order = of;
						continue;
					case 8:
						this.orderwidget9.Order = of;
						continue;
					case 9:
						this.orderwidget10.Order = of;
						continue;
				}
			}
			});

			#if DEBUG
			if (DebugIhildaWallet.OrdersWidget) {
				Logging.WriteLog (method_sig + "end loop\n");
			}
			#endif
		}

		private RippleWallet _rippleWallet = null;


#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public static PageCache<Offer> orderscash = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

#if DEBUG

		private static string clsstr = nameof (OrdersWidget) + DebugRippleLibSharp.colon;
		#endif

	}

	public class OAParam {
		public OAParam (string address) {
			this.Address = address;
		}

		public string Address {
			get;
			set;
		}

	}
}

