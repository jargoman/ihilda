using System;
using System.Linq;
using System.Collections.Generic;
using Gtk;
using RippleLibSharp.Transactions;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Network;
using IhildaWallet.Networking;
using RippleLibSharp.Trust;
using RippleLibSharp.Keys;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public partial class DenominatedIssuedPopup : Gtk.Dialog
	{
		public DenominatedIssuedPopup (String title, String message)
		{
			this.Build ();

			this.label1.Text = "<big><b><u>" + title + "</u></b></big>";
			this.label1.UseMarkup = true;
			this.textview1.Buffer.Text = message;
			PopulateIssuers();
		}

		public RippleCurrency GetSelected () {
			String s = this.comboboxentry1.ActiveText;

			RippleCurrency dic = RippleCurrency.FromIssuerCode(s);

			return dic;
		}

		public static RippleCurrency DoPopup ( String title, String message)
		{
			#if DEBUG
			String method_sig = clsstr + nameof (DoPopup) + DebugRippleLibSharp.left_parentheses + nameof (String) + DebugRippleLibSharp.space_char + nameof (title) + DebugRippleLibSharp.comma + nameof (String) + DebugRippleLibSharp.space_char + nameof (message) + DebugRippleLibSharp.right_parentheses; 
			#endif
			DenominatedIssuedPopup dom = new DenominatedIssuedPopup(title, message);
			RippleCurrency dino = null;

			while (true) {
				ResponseType resp = (ResponseType) dom.Run();
				dom.Hide();

				if (resp != ResponseType.Ok) {
					#if DEBUG
					if (DebugIhildaWallet.PairPopup) {
						Logging.WriteLog(method_sig + "resp != ResponseType.OK, breaking");
					}
					#endif
					break;
				}

				dino = dom.GetSelected();
				if (dino == null) {
					MessageDialog.ShowMessage ("The tradepair you entered was invalid");

					continue;
				}
				break;

			}

			dom.Destroy();
			#if DEBUG
			if (DebugIhildaWallet.PairPopup) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.returning + DebugRippleLibSharp.comma + dino?.ToString () ?? DebugRippleLibSharp.null_str);
			}
			#endif
			return dino;
		}



		private void PopulateIssuers ()
		{
			#if DEBUG
			string method_sig = clsstr + nameof (PopulateIssuers) + DebugRippleLibSharp.both_parentheses;
			#endif

			NetworkInterface ni = NetworkController.CurrentInterface;
			if (ni == null) {
				return;
			}

			// TODO explicitly set the address before hand ?
			RippleWallet rw = WalletManager.GetRippleWallet();
			if (rw == null) {
				return;
			}


			string address = rw.GetStoredReceiveAddress();
			TrustLine[] lines = AccountLines.GetTrustLines (address, ni);




			if (lines== null) {
				return;
			}

			List<RippleCurrency> ish = new List<RippleCurrency>();

			IEnumerable<TrustLine> lnq = from TrustLine t in lines
			          where (
						t != null && 
						t.currency != null && 
						!t.currency.Equals ("") && 
						t.account != null && 
						!t.account.Equals ("")
					)
				select t;





			foreach (TrustLine tl in lnq) {
				//if ( ) {
				if (tl.GetBalanceAsDecimal() >= 0) { // they shouldn't owe you, this could be changed
					RippleAddress issuer = null;
					try {
						//issuer = (RippleAddress)tl.account;
						issuer = tl.account;
					}

					#pragma warning disable 0168
					catch (Exception e) {
					#pragma warning disable 0168

						#if DEBUG 
						if (DebugIhildaWallet.DenominatedIssuedPopup) {
							Logging.ReportException (method_sig, e);
						}
						#endif
							continue;
						}
						if (issuer == null) {
							continue;
						}

						RippleCurrency dis = new RippleCurrency( Decimal.Zero, issuer, tl.currency );
						ish.Add(dis);
					}
				//}
			}


			//comboboxentry1.Model = new TreeModel ();
			//comboboxentry1 = new ComboBoxEntry (ish.ToArray ());
			//issuers = ish;

			comboboxentry1.Clear ();
			CellRendererText cell = new CellRendererText ();
			comboboxentry1.PackStart ( cell, true ); // TODO expand = false ?
			comboboxentry1.AddAttribute( cell, "text", 0 );
			ListStore store = new ListStore ( typeof (string) );
			comboboxentry1.Model = store;
			foreach (RippleCurrency rc in ish ) {
				string s = rc?.issuer;
				if (s == null) {
					continue;
				}

				comboboxentry1.AppendText (rc.issuer);
			}
		}

		//private static List<RippleCurrency> issuers = null;

		#if DEBUG
		private static readonly string clsstr = nameof (DenominatedIssuedPopup) + DebugRippleLibSharp.colon;
		#endif
	}
}

