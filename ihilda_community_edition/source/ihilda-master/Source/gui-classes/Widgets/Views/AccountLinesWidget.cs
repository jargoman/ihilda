/*
 *	License : Le Ice Sense 
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using Gtk;
using IhildaWallet;

using RippleLibSharp.Keys;
using RippleLibSharp.Result;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Trust;

using RippleLibSharp.Network;
using IhildaWallet.Networking;

using RippleLibSharp.Transactions;
using RippleLibSharp.Util;
using RippleLibSharp.Transactions.TxTypes;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AccountLinesWidget : Gtk.Bin
	{

		public AccountLinesWidget ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (AccountLinesWidget) + DebugRippleLibSharp.both_parentheses;
#endif

			//currentInstance = this;
			this.Hide ();
			//this.NoShowAll = true;
			this.Build ();

			if (this.pagerwidget1 == null) {
				this.pagerwidget1 = new PagerWidget ();
				this.pagerwidget1.Show ();

				vbox1.PackEnd (pagerwidget1, false, false, 1);
				//vbox1.Add (this.pagerwidget1);
			}

			this.infoBarLabel.Hide ();
			//while(Gtk.Application.EventsPending())
			//	Gtk.Application.RunIteration();

#if DEBUG
			if (DebugIhildaWallet.TxViewWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);

			}
#endif

			linescash = new PageCache<TrustLine> (nameof (AccountLines));
			InitTable ();




			this.syncbutton.Clicked += (object sender, EventArgs e) => {
				// todo verify user entered address for correctness 
				string addr = this.comboboxentry1.ActiveText;

				try {
					// creating of rippleAddress used for testing validity of address
#pragma warning disable RECS0026 // Possible unassigned object created by 'new'
					new RippleAddress (addr);
#pragma warning restore RECS0026 // Possible unassigned object created by 'new'
				}

#pragma warning disable 0168
				catch (FormatException fe) {
#pragma warning restore 0168

					string mssg = addr + " is not a properlay formatted " + nameof (RippleAddress) + "\n";
					MessageDialog.ShowMessage (mssg);
#if DEBUG
					if (DebugIhildaWallet.AccountLinesWidget) {
						Logging.WriteLog(mssg + fe.Message);
					}
#endif
					return;
				}

#pragma warning disable 0168
				catch (Exception ex) {
#pragma warning restore 0168

					string mssg = "Error processing " + nameof (RippleAddress) + "\n";
					MessageDialog.ShowMessage (mssg);
#if DEBUG
					if (DebugIhildaWallet.AccountLinesWidget) {
						Logging.WriteLog(mssg + ex.Message);
					}
#endif
					return;
				}

				//_view_account = addr;

				Thread th = new Thread (
					new ParameterizedThreadStart (SyncClicked));

				th.Start (addr);



			};

			SetPagerWidget ();
			//this.NoShowAll = false;
		}

		public void SetPagerWidget ()
		{
#if DEBUG
			string method_sig = clsstr + nameof(SetPagerWidget) + DebugRippleLibSharp.both_parentheses;
			if ( DebugIhildaWallet.AccountLinesWidget ) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			if (pagerwidget1 == null) {
				return;
			}

			pagerwidget1.first.Clicked += (object sender, EventArgs e) => {
#if DEBUG
				string event_sig = method_sig + "first button clicked : ";
				if (DebugIhildaWallet.AccountLinesWidget) {
					Logging.WriteLog(event_sig + DebugRippleLibSharp.begin);
				}
#endif

				Task.Run ((System.Action)FirstClicked);

			};

			pagerwidget1.previous.Clicked += (object sender, EventArgs e) => {
#if DEBUG
				string event_sig = method_sig + "previous button clicked : ";
				if (DebugIhildaWallet.AccountLinesWidget) {
					Logging.WriteLog(event_sig + DebugRippleLibSharp.begin);
				}
#endif

				Task.Run ((System.Action)PreviousClicked);
			};

			pagerwidget1.next.Clicked += (object sender, EventArgs e) => {
#if DEBUG
				string event_sig = method_sig + "next button clicked : ";
				if (DebugIhildaWallet.AccountLinesWidget) {
					Logging.WriteLog(event_sig + DebugRippleLibSharp.begin);
				}
#endif


				Task.Run ((System.Action)NextClicked);

			};

			pagerwidget1.last.Clicked += (object sender, EventArgs e) => {
#if DEBUG
				string event_sig = method_sig + "last button clicked : ";
				if (DebugIhildaWallet.AccountLinesWidget) {
					Logging.WriteLog(event_sig + DebugRippleLibSharp.begin);
				}
#endif

				Task.Run ((System.Action)LastClicked);


			};
		}

		public void AutoTrustLicense ()
		{
			//AreYouSure ays = new AreYouSure();
		}

		// variables !!

		private void InitTable ()
		{
#if DEBUG
			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append (clsstr);
			stringBuilder.Append (nameof(InitTable));
			stringBuilder.Append (DebugRippleLibSharp.both_parentheses);


			String method_sig = stringBuilder.ToString();


			if (DebugIhildaWallet.AccountLinesWidget) {
				stringBuilder.Append ("creating a table with ");
				stringBuilder.Append (TrustLineTableRow.numColumns.ToString ());
				stringBuilder.Append (" columns and ");
				stringBuilder.Append ((rowsPerPage + 1).ToString ());
				stringBuilder.Append (" row");
#pragma warning disable RECS0065 // Expression is always 'true' or always 'false'
#pragma warning disable RECS0110 // Condition is always 'true' or always 'false'
				stringBuilder.Append (rowsPerPage == 0 ? "" : "s");
#pragma warning restore RECS0110 // Condition is always 'true' or always 'false'
#pragma warning restore RECS0065 // Expression is always 'true' or always 'false'
				Logging.WriteLog( stringBuilder.ToString() );
			}

			stringBuilder.Clear ();
#endif

			this.lineLabels = new Label [(int)TrustLineTableRow.numColumns, rowsPerPage];
			this.buttons = new Button [rowsPerPage];
			String [] titles = TrustLineTableRow.titles; // just an array of strings for the title


			//Gtk.Application.Invoke( delegate {
			/*
			#if DEBUG
			if (DebugIhildaWallet.AccountLinesWidget) {
				Logging.WriteLog (method_sig + DebugIhildaWallet.gtkInvoke);
			}
			#endif
			*/
			if (tabl != null) {
				//this.scrolledwindow1.Remove (tabl);
				//this.scrolledwindow1
#if DEBUG
				if (DebugIhildaWallet.AccountLinesWidget) {
					Logging.WriteLog(method_sig + "Destroying old table");
				}
#endif

				tabl.Destroy ();
			}

			uint rows = rowsPerPage + 1;
#if DEBUG
			if (DebugIhildaWallet.AccountLinesWidget) {
				Logging.WriteLog(method_sig + "Creating new table with " + rows.ToString() + " rows and " + TrustLineTableRow.numColumns.ToString() + "columns");
			}	
#endif

			tabl = new Table (
				rows,  // add one for the title row
				TrustLineTableRow.numColumns,
				false
			);

			//tabl.Homogeneous = true;



			// for each column
			for (uint x = 0; x < TrustLineTableRow.numColumns; x++) {


				String text = " <big><b><u>" + titles [x] + "</u></b></big> ";
#if DEBUG
				if (DebugIhildaWallet.AccountLinesWidget) {
					Logging.WriteLog (method_sig + "Setting title " + x.ToString() + " to " + text);
				}
#endif


				Label title = new Label (text) {
					Justify = Justification.Left
				};

				if (x == 0) {
					title.WidthRequest = 310;
				}


				title.UseMarkup = true;

				tabl.Attach (
					title,
					x,
					x + 1,
					0,
					1,
					AttachOptions.Expand,
					AttachOptions.Shrink,
					5,
					4

				);

				//tabl.

				//title.Show ();



				for (int y = 0; y < rowsPerPage; y++) {

					String tex = "";
#if DEBUG
					if (DebugIhildaWallet.AccountLinesWidget) {
					tex = "unset";
					}
#endif
					if (x == TrustLineTableRow.numColumns - 1) {
						Button b = new Button {
							Label = "remove trust",
							Visible = true
						};

						tabl.Attach (
							b,
							x,
							x + 1,
							(uint)y + 1,
							(uint)y + 2,
							AttachOptions.Fill,
							AttachOptions.Shrink,
							5u,
							2u
						);

						int index = y;
						b.Clicked += (object sender, EventArgs e) => {
							
							RippleWallet rippleWallet = _rippleWallet;
							TrustLine [] lines = trustLines;
							TrustLine trust = lines [index];
							string account = rippleWallet.GetStoredReceiveAddress ();
							RippleCurrency limitAmount = new RippleCurrency (0, trust.account, trust.currency);
							RippleTrustSetTransaction rippleTrustSetTransaction = new RippleTrustSetTransaction (account, limitAmount);

							TransactionSubmitWindow transactionSubmitWindow = new TransactionSubmitWindow (rippleWallet, Util.LicenseType.NONE);
							transactionSubmitWindow.SetTransactions (rippleTrustSetTransaction);
							transactionSubmitWindow.Show ();
						};
						buttons [y] = b;
						continue;

					}
					Label lab = new Label (tex);
					this.lineLabels [x, y] = lab;
					lab.Selectable = true;
					lab.Visible = true;
					lab.CanFocus = false;
					lab.Sensitive = true;

					lab.Justify = Justification.Left;

					tabl.Attach (
						lab,
						(uint)x,
						(uint)x + 1,
						(uint)y + 1,
						(uint)y + 2,
						AttachOptions.Fill,
						AttachOptions.Shrink,
						(uint)5,
						(uint)2
					);

					lab.Yalign = 0.5f;
					lab.Xalign = 0.0f;
					//lab.Ypad = 0;
					//lab.Xpad = 0;

					//lab.Show ();
#if DEBUG
					if (DebugIhildaWallet.AccountLinesWidget) {
						Logging.WriteLog (method_sig + " Table : x = " + x.ToString() + " y = " + y.ToString() + " label " + tex );
					}
#endif
				}


			}

#if DEBUG
			if (DebugIhildaWallet.AccountLinesWidget) {
				Logging.WriteLog(method_sig + "labels attached to table");
			}	
#endif
			//this.scrolledwindow1.Add (tabl);
			this.scrolledwindow1.AddWithViewport (tabl);
			//tabl.Show ();

			//MainWindow.currentInstance.ShowAll ();
			//});

		}

		public void ClearTable ()
		{



			if (tabl != null && this.lineLabels != null) {
				for (uint x = 0; x < TrustLineTableRow.numColumns; x++) {
					for (int y = 0; y < rowsPerPage; y++) {
						uint xx = x;
						int yy = y;
						Gtk.Application.Invoke (delegate {
							Label label = this.lineLabels? [xx, yy];

							if (label != null) label.Text = "";
						});
					}
				}

			}

			Application.Invoke (
				delegate {
					this.tabl.Hide ();
					this.infoBarLabel.Markup = "";
					this.infoBarLabel.Hide ();
				}
			);


		}

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public Table tabl = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant


		public void SetViewAccount (String account)
		{
			//this._view_account = account;

			this.ClearTable ();

			Application.Invoke (
				delegate {
					comboboxentry1.Entry.Text = account ?? "";
				}
			);
		}

		//public string _view_account = null;


		public void SyncClicked (object address)
		{

#if DEBUG
			string method_sig = clsstr + nameof(SyncClicked) + DebugRippleLibSharp.left_parentheses + nameof(address) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.AccountLinesWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}


#endif


			if (!(address is string addr)) {
#if DEBUG
				if (DebugIhildaWallet.AccountLinesWidget) {
					Logging.WriteLog (method_sig + "addr == null, returning\n");
				}
#endif
				return;
			}

#if DEBUG
			if (DebugIhildaWallet.AccountLinesWidget) {
				Logging.WriteLog (method_sig + "addr = "  + addr + "\n");
			}
#endif

			//AccountLines.sync(addr);

			NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();
			if (ni == null) {
#if DEBUG
				if (DebugIhildaWallet.AccountLinesWidget) {
					Logging.WriteLog (method_sig + "ni == null, returning\n");
				}
#endif
				return;
			}

			Logging.WriteLog ("erase this else it's for testing");

			IEnumerable<Response<AccountLinesResult>> results = AccountLines.GetResultFull (addr, ni);



			Response<AccountLinesResult> res = results.FirstOrDefault ();
			if (res == null) {

				this.ClearTable ();
				Application.Invoke (
					delegate {

						// TODO
						this.infoBarLabel.Markup = "<span fgcolor=\"red\">Unkown Error</span>";
						this.infoBarLabel.Show ();
					}
				);

				return;
			}



			if (res.HasError ()) {
				if (res?.error_code == 19) {
					MessageDialog.ShowMessage ("The specified account "
						+ addr
						+ " is not found. Either it does not exist or has not been funded with "
											   + RippleCurrency.NativeCurrency
											  );
				}
				ClearTable ();

				Application.Invoke (
					delegate {

						System.Text.StringBuilder sb = new StringBuilder ();
						sb.Append ("<span fgcolor=\"red\">");
						//sb.Append(res.error_code);
						sb.Append (res?.error_message);
						sb.Append ("</span>");

						infoBarLabel.Markup = sb.ToString ();
					}
				);
			}

			IEnumerable<TrustLine> all = results.Where (x => x?.result?.lines != null).SelectMany (x => x.result.lines);

			//TrustLine[] tla = AccountLines.getResultFull (addr, ni).ToArray();
			if (!all.Any ()) {
				this.ClearTable ();
				Application.Invoke (
					delegate {
						this.infoBarLabel.Markup = "<span fgcolor=\"red\">This account has no trustlines</span>";
						//this.infoBarLabel.Markup = "This account has no trustlines";
						this.infoBarLabel.Show ();
					}
				);

				return;
			}
			this.SetTrustLines (all);
			this.SetTableGUI (all);

		}

		public void FirstClicked ()
		{

#if DEBUG
			string method_sig = nameof(FirstClicked) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.AccountLinesWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			TrustLine [] tls = linescash.GetfirstCache ();

			SetTableGUI (tls);

			pagerwidget1.SetCurrentPage (linescash.GetFirst ());

			linescash.SetFirst ();

			linescash.Preload ();
		}



		public void LastClicked ()
		{
#if DEBUG
			string method_sig = nameof(LastClicked) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.AccountLinesWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			SetTableGUI (linescash.GetLastCache ());
			pagerwidget1.SetCurrentPage (linescash.GetLast ());
			linescash.SetLast ();
			linescash.Preload ();
		}

		public void PreviousClicked ()
		{
#if DEBUG
			string method_sig = nameof(PreviousClicked) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.AccountLinesWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			if (linescash == null) {
				return;
			}

			TrustLine [] trlat = linescash.GetPreviousCache ();
			if (trlat != null) {
				SetTableGUI (trlat);

				pagerwidget1.SetCurrentPage (linescash.GetPrevious ());
				linescash.SetPrevious ();
				linescash.Preload ();
			}
		}

		public void NextClicked ()
		{
#if DEBUG
			string method_sig = nameof(NextClicked) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.AccountLinesWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			if (linescash == null) {
#if DEBUG
				if (DebugIhildaWallet.AccountLinesWidget) {
					Logging.WriteLog ("linescash == null, " + DebugRippleLibSharp.returning);
				}
#endif
				return;
			}

			TrustLine [] trlar = linescash.GetNextCache ();
			if (trlar == null) {
#if DEBUG
				if (DebugIhildaWallet.AccountLinesWidget) {
					Logging.WriteLog("trlar == null, returning");

				}
#endif
				return;
			}

			SetTableGUI (trlar);

			pagerwidget1.SetCurrentPage (linescash.GetNext ());
			linescash.SetNext ();
			linescash.Preload ();

		}

		public void SetTrustLines (IEnumerable<TrustLine> trustarray)
		{
#if DEBUG
			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append (clsstr);
			stringBuilder.Append (nameof (SetTrustLines));
			stringBuilder.Append (DebugRippleLibSharp.left_parentheses);
			stringBuilder.Append (nameof (TrustLine));
			stringBuilder.Append ("[] ");
			stringBuilder.Append (nameof (trustarray));
			stringBuilder.Append (DebugRippleLibSharp.right_parentheses);
			string method_sig =  stringBuilder.ToString();
			stringBuilder.Clear ();
#endif

			//String account = null;

			/*
			if (PaymentWindow.currentInstance == null) {

#if DEBUG
				if (DebugIhildaWallet.AccountLinesWidget) {
					stringBuilder.Append (method_sig);
					stringBuilder.Append (nameof (PaymentWindow.currentInstance));
					stringBuilder.Append (" == null, returning\n");
					Logging.WriteLog( stringBuilder.ToString());
					stringBuilder.Clear ();
				}
#endif
				return;
			}
			*/


			linescash.Set (trustarray.ToArray ());
			int num = linescash.GetNumPages;
			this.SetNumPages (num);
			this.FirstClicked ();

		}


		protected void SetTableGUI (IEnumerable<TrustLine> lines)
		{

#if DEBUG
			String method_sig = clsstr + nameof(SetTableGUI) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.AccountLinesWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
				Logging.WriteLog (method_sig + nameof (lines) + DebugRippleLibSharp.equals , lines);
			}
#endif

			if (lines == null) {
#if DEBUG
				if (DebugIhildaWallet.AccountLinesWidget) {
					Logging.WriteLog (method_sig + DebugRippleLibSharp.left_parentheses + nameof(lines) + " == null" + DebugRippleLibSharp.right_parentheses);
				}
#endif

				this.ClearTable ();

				return;
			}

#if DEBUG
			if (DebugIhildaWallet.AccountLinesWidget) {
				Logging.WriteLog (method_sig + nameof(lines) + ".length" + DebugRippleLibSharp.equals + lines?.Count().ToString() ?? DebugRippleLibSharp.null_str );
			}
#endif

			Application.Invoke (
				delegate {
#if DEBUG
					string event_sig = method_sig + DebugIhildaWallet.gtkInvoke;
#endif

					trustLines = lines.ToArray ();

					this.tabl.Show ();
					for (int y = 0; y < rowsPerPage; y++) {

						for (int x = 0; x < TrustLineTableRow.numColumns - 1; x++) {

							if (y < lines.Count ()) {
								TrustLine line = lines.ElementAt (y);
								object o = TrustLineTableRow.GetTableXIndex (line, x);
								string tex = o?.ToString () ?? "null";

#if DEBUG
								if (DebugIhildaWallet.AccountLinesWidget) {
									Logging.WriteLog( method_sig + "setting lineLabels[ " + x.ToString() + ", " + y.ToString() + "].Text to " + DebugIhildaWallet.ToAssertString(tex ));
								}
#endif


#if DEBUG

								if (DebugIhildaWallet.AccountLinesWidget) {

									Logging.WriteLog(event_sig + "setting lineLabels[ " + x.ToString() + ", " + y.ToString() + "].Text to " + DebugIhildaWallet.ToAssertString(tex ));
									if (lineLabels == null) {
										Logging.WriteLog(event_sig + "lineLabels == " + DebugRippleLibSharp.null_str );
									}
								}
#endif


								lineLabels [x, y].Text = tex;
								lineLabels [x, y].Show ();

							} else {
								lineLabels [x, y].Text = "";
								lineLabels [x, y].Hide ();
							}

						}



						if (y < lines.Count ()) {
							buttons [y].Show ();
							//lineLabels[x,y].Show();
						} else {
							buttons [y].Hide ();
							//lineLabels[x,y].Hide();
						}



					}





				}
			);

			// foreach column



		}

		public void SetNumPages (int num)
		{
#if DEBUG
			string method_sig = clsstr + nameof(SetNumPages) + DebugRippleLibSharp.left_parentheses + nameof (Int32) + DebugRippleLibSharp.space_char + nameof(num) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.AccountLinesWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			this.pagerwidget1.SetNumberOfPages (num);
		}

		private Button [] buttons;
		private Label [,] lineLabels;

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public static PageCache<TrustLine> linescash = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		//public static bool hasIce = false;

		private TrustLine[] trustLines = null; 
		/*   constants  */
		public const int rowsPerPage = 10;


#if DEBUG
		public const string clsstr = nameof (AccountLinesWidget) + DebugRippleLibSharp.colon;
#endif



		public void SetRippleWallet (RippleWallet rippleWallet)
		{
			this._rippleWallet = rippleWallet;
		}

		private RippleWallet _rippleWallet {
			get;
			set;
		}


		//public static AccountLinesWidget currentInstance = null;


	}
}

