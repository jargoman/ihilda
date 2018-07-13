﻿using System;
using System.IO;
using System.Media;
using System.Reflection;
using System.Threading;
using System.Timers;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using IhildaWallet.Networking;
using RippleLibSharp.Commands.Server;
using RippleLibSharp.Network;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Result;
using RippleLibSharp.Nodes;
using RippleLibSharp.Transactions;
using Gtk;



using IhildaWallet.Util;
using System.Diagnostics.Contracts;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public class NotificationThread
	{

		public NotificationThread (CancellationToken cancellationToken)
		{
			_token = cancellationToken;
		}

		CancellationToken _token = default (CancellationToken);

#pragma warning disable RECS0154 // Parameter is never used
		void T_Elapsed (object sender, ElapsedEventArgs e)
#pragma warning restore RECS0154 // Parameter is never used
		{



#if DEBUG
			string method_sig = clsstr + nameof (T_Elapsed) + DebugRippleLibSharp.both_parentheses;

#endif

			if (_token.IsCancellationRequested) {
				return;
			}

			try {
				NetworkInterface ni = NetworkController.CurrentInterface;
				if (ni == null) {
					return;
				}

				var wallets = WalletManager.currentInstance?.wallets;
				if (wallets == null || wallets.Count < 1) {
					return;
				}


				var info = ServerInfo.GetFeeAndLedgerSequence (ni);

				if (info == null || info.Item2 == 0) {
					return;
				}

				//lock(WalletManager.walletLock) {

				//}


				IEnumerable<RippleWallet> wal = wallets.Values.AsEnumerable ();


				if (wallets.Count < 10 && !_token.IsCancellationRequested) {

					DoExtensiveNotification (info.Item2, ni, wal);

				} else if (wallets.Count < 100 && !_token.IsCancellationRequested) {
					DoBasicNotification (info.Item2, ni, wal);
				}


				// TODO what to do if more that 100 wallets. Turn off notifications?

				if (!_token.IsCancellationRequested) WalletManager.currentInstance?.UpdateUI ();
			} catch (Exception exc) {
#if DEBUG
				Logging.ReportException (method_sig, exc);
#endif
			}

		}


		private void DoExtensiveNotification (uint ledger, NetworkInterface ni, IEnumerable<RippleWallet> wallets)
		{

#if DEBUG
			string method_sig = clsstr + nameof (DoExtensiveNotification) + DebugRippleLibSharp.both_parentheses;

#endif

			List<Task<int>> tasks = new List<Task<int>> ();

			int numberOfMenu = 0;

			foreach (RippleWallet rw in wallets) {

				string r = rw.GetStoredReceiveAddress ();






				try {
					if (rw?.LastKnownLedger == null || rw.LastKnownLedger == 0) {

						rw.LastKnownLedger = ledger;
						rw.Save ();
						continue;
						//return;
					}


					RippleCurrency rippleCurrency = AccountInfo.GetNativeBalance (rw.GetStoredReceiveAddress (), ni);
					rw.LastKnownNativeBalance = rippleCurrency;
					Tuple<uint, IEnumerable<AutomatedOrder>> tuple = DoOfferLogic (rw, ni);

					if (tuple == null) {

						continue;

						//return;

					}

					IEnumerable<AutomatedOrder> totalFilled = tuple.Item2;



					int c = totalFilled.Count ();

					if (c == 0) {
						continue;
					}

					string message =
						r +
						" has " +
						c.ToString () +
						" filled orders";

					string message2 =
						c.ToString () +
						" new orders";

					rw.Notification = message2;
					this.ShowNotification (message, numberOfMenu++);
					this.PlayNotification ();
					rw.LastKnownLedger = tuple.Item1;
					rw.Save ();

					continue;
					//return;

				} catch (Exception ex) {
#if DEBUG
					Logging.ReportException (method_sig, ex);
#endif

					continue;
				}

				//return 0;





			}


			/*
			var va = from Task<int> t in tasks
					 select t.Result;

			int grandTotal = va.Sum ();

			if (grandTotal == 0) {
				return;

			}


			Application.Invoke (
				delegate {
					StatusTrayIcon.Blinking = true;
					StatusTrayIcon.Tooltip = grandTotal.ToString () + " Orders have filled";
				}
			);
			*/

		}

		private void DoBasicNotification (uint ledger, NetworkInterface ni, IEnumerable<RippleWallet> wallets)
		{

#if DEBUG
			string method_sig = clsstr + nameof (DoBasicNotification) + DebugRippleLibSharp.both_parentheses;
#endif

			List<Task<int>> tasks = new List<Task<int>> ();
			foreach (RippleWallet rw in wallets) {

				tasks.Add (Task.Run (

						delegate {

							try {

								if (rw?.LastKnownLedger == null || rw.LastKnownLedger == 0) {

									rw.LastKnownLedger = ledger;
									rw.Save ();
									return 0;
									//return;
								}


								Tuple<uint, IEnumerable<AutomatedOrder>> tuple = DoOfferLogic (rw, ni);
								if (tuple == null) {
									return 0;
									//return;
								}

								IEnumerable<AutomatedOrder> totalFilled = tuple.Item2;



								int c = totalFilled.Count ();

								if (c == 0) {
									return 0;
								}


								string message2 =
									c.ToString () +
									" new orders";

								rw.Notification = message2;


								rw.LastKnownLedger = tuple.Item1;
								rw.Save ();

								return c;
								//return;

							}
#pragma warning disable 0168
						catch (Exception ex) {
#pragma warning restore 0168

#if DEBUG
								Logging.ReportException (method_sig, ex);
#endif

								return 0;
							}

						}


				));

			}

			if (tasks != null && tasks.Count > 0) {
				Task.WaitAll (tasks.ToArray (), 15000);
				//Task.a
			}


			var va = from Task<int> t in tasks
					 select t.Result;

			int am = va.Count ();
			int grandTotal = va.Sum ();

			if (grandTotal == 0) {
				return;

			}

			string message = am.ToString () + " accounts have a total of \n" + grandTotal.ToString () + " filled orders";

			Application.Invoke (
				delegate {
					StatusTrayIcon.Blinking = true;
					StatusTrayIcon.Tooltip = grandTotal.ToString () + " Orders have filled";
				}
			);

			ShowNotification (message, 0);
			PlayNotification ();



		}


		//private static object numLock = new object ();

		public void ShowNotification (String message, int menu_num)
		{

#if DEBUG
			string method_sig = clsstr + nameof (ShowNotification) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.NotificationThread) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			//GNotification gn = new GNotification ();
			//GLib.GNotification lib = new GLib.GNotification ();
			//SoundPlayer player = null;





			Gtk.Application.Invoke (
				delegate {

					Gtk.Menu menu = null;
					CustomPopupWindow cpw = null;
					try {

						string marked = "<span bgcolor=\"blue\" fgcolor=\"green\">" + message + "</span>";



						Gtk.MenuItem item = new MenuItem (message);

						//Label l = (Label)item.Child;
						//l.Markup = marked;



#pragma warning disable IDE0018 // Inline variable declaration
						int x = 0;
						int y = 0;

						bool push_in = false;
#pragma warning restore IDE0018 // Inline variable declaration


						//item.Show();

						menu = new Menu ();
						menu.Hide ();
						menu.Visible = false;

						menu.Add (item);

						/*
						menu.Popup(
							null,
							null,
							delegate(
								Menu men, 
								out int x, 
								out int y, 
								out bool push_in
							) {*/


						//lock(numLock) { 

						Gtk.StatusIcon.PositionMenu (
							menu,
							out x,
							out y,
							out push_in,
							StatusTrayIcon.Handle
						);


						y = y - (22 * menu_num);
						//}

						/*
							},
							0,
							0
						);
						*/


						cpw = new CustomPopupWindow ();

						cpw.SetMessage (marked);
						if (push_in) {
							cpw.Move (x, y);
						} else {
							// TODO why width*1000 is needed rather than expected width
							// why the 1:1000 scale. on different systems the screen scaling may differ. At the very least test on different devices. 
							cpw.Move (x + menu.Allocation.Width * 1000, y);
						}

						cpw.Show ();

					}

#pragma warning disable 0168
					catch (Exception ex) {
#pragma warning restore 0168
#if DEBUG
						Logging.ReportException (method_sig, ex);
#endif
					}


					//finally {

					Task.Run (
						delegate {
							try {
								//Task.Delay (2000).Wait ();
								_token.WaitHandle.WaitOne (2000);
							}

#pragma warning disable 0168
								catch (OperationCanceledException oce) {
#pragma warning restore 0168

#if DEBUG
								Logging.ReportException (method_sig, oce);
#endif
							}

#pragma warning disable 0168
								catch (Exception e) {
#pragma warning restore 0168

#if DEBUG
								Logging.ReportException (method_sig, e);
#endif
							} finally {

								Application.Invoke (
									delegate {
										menu?.Destroy ();
										menu = null;
										cpw?.Destroy ();
										cpw = null;
									}
								);
							}
						}
					);



				}
			);

		}


		public void PlayNotification ()
		{

#if DEBUG
			string method_sig = clsstr + nameof (PlayNotification) + DebugRippleLibSharp.both_parentheses;
#endif

			try {
				SystemSounds.Asterisk.Play ();
			} catch (Exception e) {

#if DEBUG
				Logging.ReportException (method_sig, e);
#endif

				MessageDialog.ShowMessage ("Remember to delete me. I want to see if it fires." + e.Message);

			}




		}

		public Tuple<uint, IEnumerable<AutomatedOrder>> DoOfferLogic (RippleWallet wallet, NetworkInterface networkInterface)
		{

#if DEBUG
			string method_sig = clsstr + nameof (DoOfferLogic) + DebugRippleLibSharp.left_parentheses + nameof (wallet) + DebugRippleLibSharp.comma + nameof (NetworkInterface) + DebugRippleLibSharp.right_parentheses;
#endif


			string ledgerMax = (-1).ToString ();
			string ledgerMin = wallet.LastKnownLedger.ToString ();


			uint lastKnownLedger = 0;
			//int lim = limit ?? 200;

			Task<IEnumerable<Response<AccountTxResult>>> task = null;

			try {
				task =
					AccountTx.GetFullTxResult (
						wallet.Account,
						ledgerMin,
						ledgerMax,
						networkInterface
					);
				if (task == null) {
					//return null;
					throw new NullReferenceException ();
				}


				task.Wait (15000, _token);


			} catch (Exception e) {
				Logging.WriteBoth (e.Message);
				MessageDialog.ShowMessage ("Network exception");

				return null;
			}

			IEnumerable<Response<AccountTxResult>> res = task.Result;

			if (res == null) {
				return null;
			}

			var results =
				from Response<AccountTxResult> r in res
				where r != null
				where r.result != null
				select r.result;

			if (
				results == null
				//|| results.Count() == 0
				|| !results.Any ()
			) {
				return null;
			}

			List<RippleTxStructure> list = new List<RippleTxStructure> ();

			foreach (AccountTxResult re in results) {

				lastKnownLedger = (uint)re.ledger_index_max;
				IEnumerable<RippleTxStructure> txs = re.transactions;

				list.AddRange (txs);
			}



			OrderManagementBot omb = new OrderManagementBot (wallet, networkInterface);

			IEnumerable<AutomatedOrder> total = null;

			try {

				total = omb.UpdateTx (list);
			}

#pragma warning disable 0168
			catch (Exception e) {
#pragma warning restore 0168

#if DEBUG
				if (DebugIhildaWallet.Robotics) {
					Logging.ReportException (method_sig, e);
				}
#endif
				MessageDialog.ShowMessage ("Exception processing tx's\n" + e.ToString () + e.StackTrace);
				return null;
			}


			if (total == null) {
				return null;
			}

			return new Tuple<uint, IEnumerable<AutomatedOrder>> (lastKnownLedger, total);


		}

		public Task InitNotificationSystem () => Task.Run (
				delegate {

					ManualResetEvent mre = new ManualResetEvent (false);
					mre.Reset ();
					Application.Invoke (delegate {



						Gdk.Pixbuf icon = Gdk.PixbufLoader.LoadFromResource (nameof (IhildaWallet) + ".Images.ih_alpha.png").Pixbuf;

						StatusTrayIcon = new StatusIcon {
							Pixbuf = icon,
							Blinking = false,
							Visible = true

						};
						WalletManagerWindow.currentInstance.ExposeEvent += delegate {
							StatusTrayIcon.Blinking = false;
						};

						WalletManagerWindow.currentInstance.EnterNotifyEvent += delegate {
							StatusTrayIcon.Blinking = false;
						};

						//StatusTrayIcon.Visible = true;

						StatusTrayIcon.Activate += delegate {

							if (WalletManagerWindow.currentInstance.Visible) {
								WalletManagerWindow.currentInstance.Visible = false;
							} else {

								Program.splash?.Hide ();
								

								WalletManagerWindow.currentInstance.Visible = true;

							}
						};

						/*
							StatusTrayIcon.Activate += delegate {

							};
						*/





						mre.Set ();

					});
					
					mre.WaitOne ();

					try {
						//Thread.Sleep(1000);
						Task.Delay (1000).Wait (1000);
					}
#pragma warning disable 0168
					catch (Exception ex) {
#pragma warning restore 0168
						MessageDialog.ShowMessage ("hmmmm");
					}

					//ThreadStart ts = doLoop;
					//Thread thread = new Thread(ts);
					//thread.Start();

					Task.Run ((System.Action)DoLoop);
				}
			);

		public void DoLoop ()
		{


			/*
			for (int i = 0; i < 6; i++) {
				T_Elapsed (null, null);
			}
			*/

			//Stopwatch watch = new stopwatch ()

			while (!_token.IsCancellationRequested) {

				T_Elapsed (null, null);
				int x = WalletManager.currentInstance.wallets.Count;

				//Task.Delay (15000);
				//Thread.Sleep (15000);
				_token.WaitHandle.WaitOne (15000);
			}


		}




		public StatusIcon StatusTrayIcon {
			get;
			set;
		}

		//public static Gtk.Menu menu = null;

#if DEBUG
		private const string clsstr = nameof (NotificationThread) + DebugRippleLibSharp.colon;
#endif
	}
}
