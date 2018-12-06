using System;
using System.Threading;
using System.Threading.Tasks;
using RippleLibSharp.Binary;
using RippleLibSharp.Transactions;
using RippleLibSharp.Keys;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Result;
using IhildaWallet.Networking;
using RippleLibSharp.Network;
using RippleLibSharp.Util;


using RippleLibSharp.Trust;
using Gtk;
using System.Collections.Generic;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class BalanceTab : Gtk.Bin
	{
		public BalanceTab ()
		{
			
			#if DEBUG
			string method_sig = clsstr + nameof(BalanceTab) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.BalanceTab) {
				
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}
			#endif

			BalanceTab.CurrentInstance = this;
			this.Build ();


			#if DEBUG
			if (DebugIhildaWallet.BalanceTab) {
				Logging.WriteLog(method_sig + DebugIhildaWallet.buildComp);
			}
			#endif

			ListStoreObj = new ListStore (  typeof(string), typeof(string), typeof(string)  );

			Gtk.CellRendererText cell = new Gtk.CellRendererText {
				Editable = true

			};


			//cell.Mode = CellRendererMode.

			this.treeview1.AppendColumn ("Currency", cell, "markup", 0);
			this.treeview1.AppendColumn ("Issuer", cell, "markup", 1);
			this.treeview1.AppendColumn ("Balance", cell, "markup", 2);



		}

		public void SetInteractivty ()
		{
			this.treeview1.ButtonReleaseEvent += Treeview1_ButtonReleaseEvent;
		}

		void Treeview1_ButtonReleaseEvent (object o, ButtonReleaseEventArgs args)
		{
			//args.Event.Button;
		}




		private void Clear ()
		{

			Application.Invoke (
				delegate {
				
					ListStoreObj?.Clear ();
					treeview1.Model = ListStoreObj;

				}
			);

		}



		private string _rippleAddress = null;
		public void SetAddress ( RippleAddress ra)
		{

#if DEBUG
			string method_sig = clsstr + nameof (SetAddress) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.BalanceTab) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			_rippleAddress = ra;

			if (ra == null) {
				
				Clear ();
				return;
			}


			if (!ra.Equals(_rippleAddress)) {
				Clear ();
			}




			this.UpdateBalance ();

		}


		private CancellationTokenSource balanceTokenSource = null;
		public void UpdateBalance ()
		{
			balanceTokenSource?.Cancel ();
			balanceTokenSource = new CancellationTokenSource ();

			Task.Run (
				delegate {
					this.Clear ();
					NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();
					if (ni == null) {
						bool test = NetworkController.DoNetworkingDialogNonGUIThread ();
						if (!test) {

							return;
						}

					}





					RippleAddress ra = _rippleAddress;

					if (ra == null) {
						return;
					}

					Task<Response<AccountLinesResult>> task = AccountLines.GetResult (ra.ToString (), ni, balanceTokenSource.Token);

					task.Wait (balanceTokenSource.Token);

					Response<AccountLinesResult> response = task.Result;

					AccountLinesResult res = response?.result;

					if (res == null) {
						return;
					}

					TrustLine [] lines = res?.lines;
					if (lines == null) {
						return;
					}

					RippleCurrency [] cur = res?.GetBalancesAsRippleCurrencies ();

					if (cur == null) {
						return;
					}

					this.SetCurrencies (cur);
				}



			);
		}

		public void SetCurrencies (RippleCurrency [] currencyArray)
		{
			

			#if DEBUG
			String method_sig = clsstr + nameof(SetCurrencies) + DebugRippleLibSharp.left_parentheses + currencyArray.ToString() + DebugRippleLibSharp.right_parentheses; 
			if (DebugIhildaWallet.BalanceTab) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}

			#endif

			// this sets the balance as well. 
			Gtk.Application.Invoke( delegate {
				#if DEBUG
				if (DebugIhildaWallet.BalanceTab) {
					Logging.WriteLog(method_sig + DebugIhildaWallet.gtkInvoke);
				}
				#endif


				ListStoreObj?.Clear ();

				List<Tuple<string, string, string>> values = new List<Tuple<string, string, string>> ();
				List<Tuple<string, string, string>> zerovalues = new List<Tuple<string, string, string>> ();

				for (int i = 0; i < currencyArray.Length; i++) {

					RippleCurrency c = currencyArray [i];
					if (c == null) {
						continue;
					}
					string cu = c.currency;
					string iss = c.IsNative ? "native currency" : c.issuer;
					string ba = c.amount.ToString ();

					if (c.amount == decimal.Zero) {
						TextHighlighter.Highlightcolor = "\"grey\"";
						cu = TextHighlighter.Highlight (cu);
						iss = TextHighlighter.Highlight (iss);
						ba = TextHighlighter.Highlight (ba);

						zerovalues.Add (new Tuple<string, string, string> (cu, iss, ba));
					} else if (c.amount < decimal.Zero) {
						TextHighlighter.Highlightcolor = "\"red\"";
						//cu = TextHighlighter.Highlight (cu);
						//iss = TextHighlighter.Highlight (iss);
						ba = TextHighlighter.Highlight (ba);

						values.Add (new Tuple<string, string, string> (cu, iss, ba));
					} else {

						TextHighlighter.Highlightcolor = Program.darkmode ? "\"chartreuse\"" :"\"green\"";
						//cu = TextHighlighter.Highlight (cu);
						//iss = TextHighlighter.Highlight (iss);
						ba = TextHighlighter.Highlight (ba);

						values.Add (new Tuple<string, string, string> (cu, iss, ba));
					}

						



				}

				values.AddRange (zerovalues);

				foreach (var v in values) {
					ListStoreObj.AppendValues (v.Item1, v.Item2, v.Item3);
				}



				treeview1.Model = ListStoreObj;

			

			}); // end delegate


		} // end public set() (str[] str) 


		private Gtk.ListStore ListStoreObj {
			get;
			set;
		}

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public static BalanceTab CurrentInstance = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant


#if DEBUG
		private static string clsstr = nameof (BalanceTab) + DebugRippleLibSharp.colon;
		#endif

	}
}

