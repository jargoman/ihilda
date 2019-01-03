using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Gtk;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class WalletTree : Gtk.Bin
	{
		public WalletTree ()
		{

#if DEBUG
			string method_sig = clsstr + nameof (WalletTree) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.WalletTree) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			this.Build ();


#if DEBUG
			if (DebugIhildaWallet.WalletTree) {
				Logging.WriteLog (method_sig + DebugIhildaWallet.buildComp);
			}
#endif

			this.treeview2.HoverSelection = false;

			//this.treeview2.sel


			store = new ListStore (
				typeof (bool), // Select
				typeof (string), // Name
				typeof (string), // Type
				typeof (string) // Account 
				//typeof (string), // Enryption
				//typeof (string), // Balance
				//typeof (string) // notifications
			);


			CellRendererToggle tog = new CellRendererToggle {
				Radio = true
			};

			CellRendererText renderer = new CellRendererText ();



			treeview2.AppendColumn ("", tog, "active", 0);
			treeview2.AppendColumn ("Name", renderer, "markup", 1);
			treeview2.AppendColumn ("Type", renderer, "markup", 2);
			treeview2.AppendColumn ("Account", renderer, "markup", 3);
			/*treeview2.AppendColumn ("Encryption", renderer, "markup", 4);*/
			//treeview2.AppendColumn ("Balance", renderer, "markup", 4);
			//treeview2.AppendColumn ("Notifications", renderer, "markup", 4);

			currentInstance = this;


			treeview2.EnterNotifyEvent += (object o, EnterNotifyEventArgs args) => {
				treeview2.GrabFocus ();
			};

			treeview2.LeaveNotifyEvent += (object o, LeaveNotifyEventArgs args) => {

			};


			treeview2.KeyReleaseEvent += (object o, KeyReleaseEventArgs args) => {

				if (args.Event.Key == Gdk.Key.Return) {

					RippleWallet selectedRippleWallet = GetSelected ();

					if (selectedRippleWallet != null) {
						selectedRippleWallet.Notification = "";

					}

					WalletManagerWidget.currentInstance.SetQRandWalletAddress (selectedRippleWallet);

					//WalletManager.SetRippleWallet (selectedRippleWallet);

					//WalletManager.currentInstance?.UpdateUI ();
				}


			};

			treeview2.ButtonReleaseEvent += (object o, ButtonReleaseEventArgs args) => {

				Logging.WriteLog (nameof (ButtonReleaseEvent) + " at x=" + args.Event.X.ToString () + " y=" + args.Event.Y.ToString ());
				RippleWallet hoveredRippleWallet = GetFromPos (args.Event.X, args.Event.Y);

				RippleWallet selectedRippleWallet = GetSelected ();

				if (hoveredRippleWallet == null) {
					/*
					if (selectedRippleWallet != null) {
						return;
					}
					*/
					if (args.Event.Button == 3) {
						if (selectedRippleWallet != null) {
							RunWalletSelectedPopup ();
						} else {
							RunNoWalletPopup ();
						}
					}

					goto GUI;
					//return;
				}


				/*
				if (selectedRippleWallet == null) {
					if (hoveredRippleWallet != null) {
						return;
					}
					RunNoWalletPopup ();
					goto GUI;
					//return;
				}
				*/
				/*
				if (!hoveredRippleWallet.WalletName.Equals (selectedRippleWallet?.WalletName)) {
					goto GUI;
					//return;
				}
				*/

				if (selectedRippleWallet != null) {
					selectedRippleWallet.Notification = "";
					WalletManagerWidget.currentInstance.SetQRandWalletAddress (selectedRippleWallet);


					Logging.WriteConsole ("Selected Wallet " + (selectedRippleWallet?.GetStoredReceiveAddress () ?? "null") + " \n\n");
				} else {
					hoveredRippleWallet.Notification = "";
					WalletManagerWidget.currentInstance.SetQRandWalletAddress (hoveredRippleWallet);
				}

				//WalletManager.SetRippleWallet( selectedRippleWallet );
				

				if (args.Event.Button == 3) {

#if DEBUG
					if (DebugIhildaWallet.WalletTree) {
						Logging.WriteLog ("Right click \n");
					}
#endif
					RunWalletSelectedPopup ();

					//args.Event.
				}

				if (args.Event.Button == 1) {

#if DEBUG
					if (DebugIhildaWallet.WalletTree) {
						Logging.WriteLog ("args.Event.Button == 1\n");
					}
#endif
				}



			GUI:
				WalletManager.currentInstance?.UpdateUI ();
			};


			/*
			this.treeview2.SelectionReceived += delegate(object o, SelectionReceivedArgs args) {
				Logging.writeLog("selection received\n");
			};

			this.treeview2.SelectionGet += delegate(object o, SelectionGetArgs args) {
				Logging.writeLog("SelectionGet\n");
			};

			this.treeview2.SelectionNotifyEvent += delegate(object o, SelectionNotifyEventArgs args) {
				Logging.writeLog("SelectionNotifyEvent\n");
			};
			*/
		}



		public void RunWalletSelectedPopup ()
		{


			#region copymenu

			Gtk.Menu copyMenu = new Menu ();
			Gtk.MenuItem copyAdd = new MenuItem ("Copy Address");
			copyAdd.Show ();

			copyMenu.Add (copyAdd);

			copyAdd.ButtonPressEvent += (object sender, ButtonPressEventArgs e) => {

#if DEBUG
				if (DebugIhildaWallet.WalletTree) {
					Logging.WriteLog ("Copy Address selected");
				}
#endif

				RippleWallet rw = WalletManager.GetRippleWallet();
				if (rw == null) {
					return;
				}
				var clipboard = this.GetClipboard (Gdk.Selection.Clipboard);

				clipboard.Clear ();

				clipboard.Text = rw?.GetStoredReceiveAddress () ?? "";
			};

			Gtk.MenuItem copyName = new MenuItem ("Copy Walletname");
			copyName.Show ();

			copyMenu.Add (copyName);

			copyName.ButtonPressEvent += (object sender, ButtonPressEventArgs e) => {
#if DEBUG
				if (DebugIhildaWallet.WalletTree) {
					Logging.WriteLog ("Copy name selected");
				}
#endif
				RippleWallet rw = WalletManager.GetRippleWallet();
				if (rw == null) {
					return;
				}
				var clipboard = this.GetClipboard (Gdk.Selection.Clipboard);
				clipboard.Clear ();

				clipboard.Text = rw?.WalletName ?? "";
			};


			Gtk.MenuItem copyenc = new MenuItem ("Copy Encryption Type");
			copyenc.Show ();
			copyMenu.Add (copyenc);

			copyenc.ButtonPressEvent += (object sender, ButtonPressEventArgs e) => {
#if DEBUG
				if (DebugIhildaWallet.WalletTree) {
					Logging.WriteLog ("Copy Encryption Type selected");
				}
#endif
				RippleWallet rw = WalletManager.GetRippleWallet();
				if (rw == null) {
					return;
				}
				var clipboard = this.GetClipboard (Gdk.Selection.Clipboard);
				clipboard.Clear ();

				clipboard.Text = rw?.Encryption_Type ?? "";
			};


			Gtk.MenuItem copynoti = new MenuItem ("Copy Notification");
			copynoti.Show ();
			copyMenu.Add (copynoti);

			copynoti.ButtonPressEvent += (object sender, ButtonPressEventArgs e) => {
#if DEBUG
				if (DebugIhildaWallet.WalletTree) {
					Logging.WriteLog ("Copy Notification selected");
				}
#endif
				RippleWallet rw = WalletManager.GetRippleWallet();
				if (rw == null) {
					return;
				}
				var clipboard = this.GetClipboard (Gdk.Selection.Clipboard);
				clipboard.Clear ();

				clipboard.Text = rw?.Notification ?? "";
			};

			#endregion

			#region transact
			Gtk.Menu txMenu = new Gtk.Menu ();

			Gtk.MenuItem bot = new MenuItem ("Market Bot");
			bot.Show ();
			txMenu.Add (bot);

			bot.ButtonPressEvent += (object sender, ButtonPressEventArgs e) => {

#if DEBUG
				if (DebugIhildaWallet.WalletTree) {
					Logging.WriteLog ("Market bot selected");
				}
#endif

				if (WalletManagerWidget.currentInstance != null) {

					Task.Run ((System.Action)WalletManagerWidget.currentInstance.BotButtonClicked);
				}
			};

			Gtk.MenuItem TradeMarket = new MenuItem ("Trading Market");
			TradeMarket.Show ();
			txMenu.Add (TradeMarket);

			TradeMarket.ButtonPressEvent +=  (object sender, ButtonPressEventArgs e) => {
#if DEBUG
				if (DebugIhildaWallet.WalletTree) {
					Logging.WriteLog ("trade selected");
				}
#endif
				if (WalletManagerWidget.currentInstance != null) {

					Task.Run (
						(System.Action)WalletManagerWidget.currentInstance.TradePair
					);
				}
			};


			MenuItem pay = new MenuItem ("Make A Payment");

			pay.Show ();
			txMenu.Add (pay);

			pay.ButtonPressEvent += (object sender, ButtonPressEventArgs e) => {
#if DEBUG
				if (DebugIhildaWallet.WalletTree) {
					Logging.WriteLog ("make a payment selected");
				}
#endif


				if (WalletManagerWidget.currentInstance != null) {

					Task.Run (
						(System.Action)WalletManagerWidget.currentInstance.Payment
					);
				}
			};

			MenuItem trust = new MenuItem ("Manage Trust");
			trust.Show ();
			txMenu.Add (trust);

			trust.ButtonPressEvent += (object sender, ButtonPressEventArgs e) => {
#if DEBUG
				if (DebugIhildaWallet.WalletTree) {
					Logging.WriteLog ("Manage trust selected");
				}
#endif
				if (WalletManagerWidget.currentInstance != null) {

					Task.Run (
						(System.Action)WalletManagerWidget.currentInstance.Trust
					);
				}
			};

			#endregion



			#region wallet

			Gtk.Menu walletMenu = new Menu ();

			MenuItem edit = new MenuItem ("Edit Wallet");
			edit.Show ();
			walletMenu.Add (edit);

			edit.ButtonPressEvent += (object sender, ButtonPressEventArgs e) => {
#if DEBUG
				if (DebugIhildaWallet.WalletTree) {
					Logging.WriteLog ("edit selected");
				}
#endif

				if (WalletManagerWidget.currentInstance != null) {
					/*
					Task.Run (
						(System.Action)WalletManagerWidget.currentInstance.Edit
					);
					*/
					WalletManagerWidget.currentInstance.Edit ();
				}
			};

			MenuItem upgrade = new MenuItem ("Upgrade Wallet");
			upgrade.Show ();

			MenuItem export = new MenuItem ("Export Wallet");
			export.Show ();
			walletMenu.Add (export);

			export.ButtonPressEvent += (object sender, ButtonPressEventArgs e) => {
#if DEBUG
				if (DebugIhildaWallet.WalletTree) {
					Logging.WriteLog ("Export selected");
				}
#endif

				if (WalletManagerWidget.currentInstance != null) {

					Task.Run (
						(System.Action)WalletManagerWidget.currentInstance.Export
					);
				}
			};

			MenuItem dele = new MenuItem ("Delete Wallet");
			dele.Show ();
			walletMenu.Add (dele);


			dele.ButtonPressEvent +=  (object sender, ButtonPressEventArgs e) => {
#if DEBUG
				if (DebugIhildaWallet.WalletTree) {
					Logging.WriteLog ("delete selected");
				}
#endif

				if (WalletManagerWidget.currentInstance != null) {
					/*
					Task.Run (
						(System.Action)WalletManagerWidget.currentInstance.Delete
					);
					*/

					WalletManagerWidget.currentInstance?.Delete ();
				}
			};

			#endregion


			#region browse 
			Menu browseMenu = new Menu ();

			MenuItem xrpCharts = new MenuItem ("View on " + URLexplorer.xrpChartsUrl);
			xrpCharts.Show ();

			browseMenu.Add (xrpCharts);

			xrpCharts.ButtonPressEvent += (object o, ButtonPressEventArgs args) => {

				RippleWallet rw = WalletManager.GetRippleWallet();
				if (rw == null) {
					return;
				}

				StringBuilder stringBuiler = new StringBuilder ();

				stringBuiler.Append (URLexplorer.proto);
				stringBuiler.Append (URLexplorer.xrpChartsUrl);
				stringBuiler.Append ("/#/graph/");
				stringBuiler.Append (rw.GetStoredReceiveAddress());

				URLexplorer.OpenUrl (stringBuiler.ToString ());
			};
			#endregion

			#region rootMenu

			Gtk.Menu rootmenu = new Menu ();

			Gtk.MenuItem copysub = new Gtk.MenuItem ("Copy to clipboard");

			copysub.Show ();
			rootmenu.Add (copysub);

			copysub.Submenu = copyMenu;
					

			Gtk.MenuItem txsub = new Gtk.MenuItem ("Transact");

			txsub.Show ();
			rootmenu.Add (txsub);

			txsub.Submenu = txMenu;

			Gtk.MenuItem walletsub = new Gtk.MenuItem ("Wallet");

			walletsub.Show ();
			rootmenu.Add (walletsub);

			walletsub.Submenu = walletMenu;

			Gtk.MenuItem browseSub = new MenuItem ("Web Browser");
			browseSub.Show ();

			rootmenu.Add (browseSub);

			browseSub.Submenu = browseMenu;
#endregion





			rootmenu.Popup ();
		}

		public void RunNoWalletPopup ()
		{
#if DEBUG
			String methog_sig = clsstr + nameof (RunNoWalletPopup) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.WalletTree) {
				Logging.WriteLog (methog_sig + DebugRippleLibSharp.begin);
			}
#endif
			Menu menu = new Menu ();

			MenuItem newWallet = new MenuItem ("New Wallet");
			newWallet.Show ();
			menu.Add (newWallet);

			newWallet.ButtonPressEvent +=  (object sender, ButtonPressEventArgs e) => {
#if DEBUG
				if (DebugIhildaWallet.WalletTree) {
					Logging.WriteLog (methog_sig + "new activated");
				}
#endif

				if (WalletManagerWidget.currentInstance != null) {
					WalletManagerWidget.currentInstance.New_Wallet_Wizard ();
				}
			};

			MenuItem importWallet = new MenuItem ("Import Wallet");
			importWallet.Show ();
			menu.Add (importWallet);

			importWallet.ButtonPressEvent +=  (object sender, ButtonPressEventArgs e) => {
#if DEBUG
				if (DebugIhildaWallet.WalletTree) {
					Logging.WriteLog (methog_sig + "import activated");
				}
#endif

				WalletManager.ImportWallet ();

				if (WalletManagerWidget.currentInstance != null) {
					WalletManagerWidget.currentInstance.UpdateUI ();
				}
			};

			menu.Popup ();

		}

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public static WalletTree currentInstance = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		public void SetValues (IEnumerable<RippleWallet> wallets)
		{
#if DEBUG
			String method_sig = clsstr + nameof (SetValues) + DebugRippleLibSharp.left_parentheses + nameof (wallets) + DebugRippleLibSharp.right_parentheses;

			if (DebugIhildaWallet.WalletTree) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			if (wallets == null) {
#if DEBUG
				if (DebugIhildaWallet.WalletTree) {
					Logging.WriteLog (method_sig + "wallets == null");
				}
#endif

				return;
				//Application.Quit(); // too much?
			}


#if DEBUG
			if (DebugIhildaWallet.WalletTree) {
				Logging.WriteLog (method_sig + "wallets = ", wallets);
			}
#endif


			Application.Invoke (delegate {

				store.Clear ();

				StringBuilder sb = new StringBuilder ();

				RippleWallet rippleWallet = WalletManager.GetRippleWallet ();


				foreach (RippleWallet rw in wallets) {
					//TODO there might be a cleaner way to do this. by index for example/ the name is used for the filename and is therefore unique
					bool b = false;

					if (rippleWallet?.WalletName != null) {
						b |= rippleWallet.WalletName.Equals (rw?.WalletName);
					}

					sb.Clear ();

					string balance = rw?.LastKnownNativeBalance?.ToString ();
					string notification = rw?.Notification;
					if (rw?.AccountType == RippleWalletTypeEnum.Master || rw?.AccountType == RippleWalletTypeEnum.MasterPrivateKey) {
						sb.Append ("<span ");

						/*
						if (b) {
							if (!Program.darkmode) {
								sb.Append ("bgcolor=\"lavender\"");
							} else {
								sb.Append ("bgcolor=\"black\"");
							}
						} */

						if (!Program.darkmode) {
							sb.Append ("fgcolor=\"green\"><big>");
						} else {
							sb.Append ("fgcolor=\"chartreuse\"><big>");
						}

						if (b) {
							sb.Append ("<b><u>");
						}
						sb.Append (rw?.GetStoredReceiveAddress () ?? " ");

						if (b) {
							sb.Append ("</u></b>");
						}



						sb.Append ("</big></span>");

						if (Program.darkmode) {
							sb.Append ("<span fgcolor=\"deepskyblue\" size=\"x-large\">");
						} else {
							sb.Append ("<span fgcolor=\"darkblue\" size=\"x-large\">");
						}

						if (balance != null) {
							sb.AppendLine ();
							sb.Append (balance);
						}


						sb.Append ("</span>");
						if (notification != null) {
							sb.AppendLine ();
							sb.Append (notification);
						}
					}

					if (rw?.AccountType == RippleWalletTypeEnum.Regular) {
						sb.Append ("<span ");
						if (b) {

							/*
							if (!Program.darkmode) {
								sb.Append ("bgcolor=\"lavender\"");
							} else {
								sb.Append ("bgcolor=\"black\"");
							}
							*/		    
						}

						if (Program.darkmode) {
							sb.Append ("fgcolor=\"chartreuse\">");
						} else {
							sb.Append ("fgcolor=\"green\">");
						}

						sb.Append (rw?.GetStoredReceiveAddress () ?? " ");
						sb.Append ("</span>");




						sb.AppendLine ();
						sb.Append ("<span ");
						if (b) {

							/*
							if (!Program.darkmode) {
								sb.Append ("bgcolor=\"antiquewhite\"");
							} else {
								sb.Append ("bgcolor=\"black\"");
							}
							*/		    
						}

						sb.Append ("fgcolor=\"grey\">");
						sb.Append (rw?.Regular_Key_Account ?? " ");
						sb.Append ("</span>");

						if (Program.darkmode) {
							sb.Append ("<span fgcolor=\"deepskyblue\" size=\"x-large\">");
						} else {
							sb.Append ("<span fgcolor=\"darkblue\" size=\"x-large\">");
						}
						if (balance != null) {
							sb.AppendLine ();
							sb.Append (balance);
						}

						sb.Append ("</span>");

						if (notification != null) {
							sb.AppendLine ();
							sb.Append (notification);
						}

					}
					//string accType = );

					StringBuilder stringBuilder = new StringBuilder ();
					stringBuilder.AppendLine (rw?.AccountType.ToString ());
					stringBuilder.Append (rw?.GetStoredEncryptionType () ?? "");

					string name = rw?.WalletName ?? "";
					if (b) {
						if (Program.darkmode) {
							name = "<span fgcolor=\"chartreuse\" ><b><u>" + name + "</u></b></span>";
						} else {
							name = "<span fgcolor=\"green\" ><b><u>" + name + "</u></b></span>";
						}
					}
					store.AppendValues (
						b,
						name,
						stringBuilder.ToString(),
						sb?.ToString () ?? ""




					);


				}


#if DEBUG
				if (DebugIhildaWallet.WalletTree) {
					Logging.WriteLog (method_sig + "setting store");
				}
#endif

				treeview2.Model = store;

			});
		}

		public void ClearValues ()
		{
			Application.Invoke ((sender, e) => store?.Clear ()
			);
			//treeview2.Model = store;
			//setValues(new RippleWallet[0]);  // an array of length zero
		}

		public RippleWallet GetFromPos (double xx, double yy)
		{
			int x = Convert.ToInt32 (xx);
			int y = Convert.ToInt32 (yy);
			if (!treeview2.GetPathAtPos (x, y, out TreePath path)) {
				return null;
			}

			if (!store.GetIter (out TreeIter iter, path)) {
				return null;
			}

			object o = store.GetValue (iter, 1);

			return ParseObject (o);

		}

		private RippleWallet GetSelected ()
		{
#if DEBUG
			String method_sig = clsstr + nameof (GetSelected) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.WalletTree) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			//treeview2.

			TreeSelection ts = treeview2.Selection;

			if (ts == null) {
#if DEBUG
				if (DebugIhildaWallet.WalletTree) {
					Logging.WriteLog (method_sig + "Selected item is null");

				}
#endif
				return null;
			}






			if (ts.GetSelected (out TreeModel tm, out TreeIter ti)) {
#if DEBUG
				if (DebugIhildaWallet.WalletTree) {
					Logging.WriteLog (method_sig + "retrieved value");
				}
#endif

				object o = tm.GetValue (ti, 1);

				return ParseObject (o);
			}
#if DEBUG
			if (DebugIhildaWallet.WalletTree) {
				Logging.WriteLog (method_sig + "failed to retreive string from UI, returning null");
			}
#endif
			return null;
		}

		private RippleWallet ParseObject (object o)
		{
#if DEBUG
			String method_sig = clsstr + nameof (ParseObject) + DebugRippleLibSharp.left_parentheses + DebugIhildaWallet.ToAssertString (o) + DebugRippleLibSharp.right_parentheses;
#endif

			if (o == null) {
#if DEBUG
				if (DebugIhildaWallet.WalletTree) {
					Logging.WriteLog (method_sig + "o == null");
				}
#endif
				return null;
			}

			if (!(o is String val)) {

				#if DEBUG
			if (DebugIhildaWallet.WalletTree) {
					Logging.WriteLog (method_sig + "val is not string, " + DebugRippleLibSharp.returning + DebugRippleLibSharp.null_str);
				}
#endif


				return null;
			}

			int c = val.Length;

			string sp1 = "<span fgcolor=\"green\" ><b><u>";
			string sp2 = "</u></b></span>";

			string sp3 ="<span fgcolor=\"chartreuse\" ><b><u>";

			if (val.Contains(sp1)) {
				val = val.Remove (c - sp2.Length);
				val = val.Remove (0, sp1.Length);

			}

			if (val.Contains(sp3)) {
				val = val.Remove (c - sp2.Length);
				val = val.Remove (0, sp3.Length);
			}

			RippleWallet rw = WalletManager.currentInstance.LookUp (val);
#if DEBUG
			if (DebugIhildaWallet.WalletTree) {
				Logging.WriteLog (method_sig + "Selected Ripple Wallet is " + DebugIhildaWallet.ToAssertString (rw));
			}
#endif
			return rw;




		}

		/*
		private object ParseObject ()
		{
			throw new NotImplementedException ();
		}
		*/

		ListStore store;
#if DEBUG
		private const string clsstr = nameof (WalletTree) + DebugRippleLibSharp.colon;
#endif
	}
}

