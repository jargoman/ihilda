using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Gtk;
using IhildaWallet.Networking;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Commands.Server;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;
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


			WalletManagerWidget.currentInstance?.TestConnectivity ();

			try {

				// TODO error handling 
				NetworkInterface ni = NetworkController.CurrentInterface;
				if (ni == null || !ni.IsConnected ()) {
					NetworkController.AutoConnect ();
					return;
				}

				var wallets = WalletManager.currentInstance?.wallets;
				if (wallets == null || wallets.Count < 1) {
					return;
				}


				FeeAndLastLedgerResponse feeResp = ServerInfo.GetFeeAndLedgerSequence (ni, _token);

				if (feeResp == null) {
					return;
				}

				if (feeResp.HasError) {
					return;
				}

				if (feeResp.LastLedger == 0) {
					return;
				}




				IEnumerable<RippleWallet> wal = wallets.Values.AsEnumerable ();


				if (wallets.Count < 10 && !_token.IsCancellationRequested) {

					DoExtensiveNotification ((uint)feeResp.LastLedger, ni, _token, wal);

				} else if (wallets.Count < 100 && !_token.IsCancellationRequested) {
					DoBasicNotification ((uint)feeResp.LastLedger, ni, _token, wal);
				}


				// TODO what to do if more that 100 wallets. Turn off notifications?

				if (!_token.IsCancellationRequested) WalletManager.currentInstance?.UpdateUI ();
			} catch (Exception exc) {
#if DEBUG
				Logging.ReportException (method_sig, exc);
#endif
			}

		}


		private void DoExtensiveNotification (uint ledger, NetworkInterface ni, CancellationToken token, IEnumerable<RippleWallet> wallets)
		{

#if DEBUG
			string method_sig = clsstr + nameof (DoExtensiveNotification) + DebugRippleLibSharp.both_parentheses;

#endif

			List<Task<int>> tasks = new List<Task<int>> ();

			int numberOfMenu = 0;

			SoundSettings settings = SoundSettings.LoadSoundSettings ();

			foreach (RippleWallet rw in wallets) {

				string r = rw.GetStoredReceiveAddress ();




				//rw.Notification = "Updating balance...";
				//WalletManager.currentInstance?.UpdateUI ();

				try {

					RippleCurrency rippleCurrency = AccountInfo.GetNativeBalance (rw.GetStoredReceiveAddress (), ni, token);

					DateTime dateTime = DateTime.Now;


					if (rippleCurrency != null) {
						rw.LastKnownNativeBalance = rippleCurrency;
						rw.BalanceNote = "Balance updated as of " + dateTime.ToShortTimeString ();
						WalletManager.currentInstance?.UpdateUI ();
					} else {
						string mess = "<span fgcolor=\"salmon\">Could not update balance</span>";

						if (rw.CouldNotUpdateBalanceCount++ < 1) {
							rw.Notification = mess;
							WalletManager.currentInstance?.UpdateUI ();

						} else {
							rw.Notification = null;
							rw.BalanceNote = mess;
							WalletManager.currentInstance?.UpdateUI ();
						}

					}

		    			

					if (rw?.NotificationLedger == null || rw?.NotificationLedger == 0) {


						rw.Notification = "<span fgcolor=\"" + (string)(Program.darkmode ? "lightblue" : "blue") + "\">Newly added wallet</span>";
						rw.NotificationLedger = ledger;
						rw.SaveBotLedger (ledger, rw.NotificationLadgerPath);
						WalletManager.currentInstance?.UpdateUI ();
						continue;
						//return;
					}



					DoLogicResponse tuple = DoOfferLogic (rw, ni);

					if (tuple == null) {
						rw.Notification = Program.darkmode ? "<span fgcolor=\"#FFAABB\">Failed to retrieve newly filled orders.</span>" : "<span fgcolor=\"red\">Failed to retrieve newly filled orders.</span>";

						WalletManager.currentInstance?.UpdateUI ();
						continue;

						//return;

					}

					if (tuple.HasError) {
						rw.Notification = tuple.ErrorMessage;

						if (tuple.ErrorCode == 55) {
							rw.NotificationLedger = 0;
							rw.SaveBotLedger (0, rw.NotificationLadgerPath);
						}



						WalletManager.currentInstance?.UpdateUI ();
						continue;
					}

		    			

					IEnumerable<AutomatedOrder> totalFilled = tuple.FilledOrders;

					if (totalFilled == null) {
						rw.NotificationLedger = tuple.LastLedger;
						rw.SaveBotLedger (tuple.LastLedger, rw.NotificationLadgerPath);
						continue;
					}

					int c = totalFilled.Count ();

					if (c == 0) {
						rw.NotificationLedger = tuple.LastLedger;
						rw.SaveBotLedger (tuple.LastLedger, rw.NotificationLadgerPath);
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


					if (settings.HasOnOrderFilled && settings.OnOrderFilled != null) {

						Task.Run (delegate {

							SoundPlayer player =
							new SoundPlayer (settings.OnOrderFilled);
							player.Load ();
							player.Play ();
						});

					} else if (settings.FallBack) {
						this.PlayNotification ();
					}



					rw.NotificationLedger = tuple.LastLedger;
					rw.SaveBotLedger (tuple.LastLedger, rw.NotificationLadgerPath);

					continue;
					//return;

				} catch (Exception ex) {
#if DEBUG
					Logging.ReportException (method_sig, ex);
#endif

					rw.Notification = "<span>Exception thrown in notification thread</span>";
					continue;
				} finally {

					WalletManager.currentInstance?.UpdateUI ();


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

		private void DoBasicNotification (uint ledger, NetworkInterface ni, CancellationToken token, IEnumerable<RippleWallet> wallets)
		{

#if DEBUG
			string method_sig = clsstr + nameof (DoBasicNotification) + DebugRippleLibSharp.both_parentheses;
#endif

			//List<Task<int>> tasks = new List<Task<int>> ();
			//RippleWallet[] walls = wallets.ToArray ();

			int accnts = 0;
			int count = 0;

			foreach (RippleWallet rippleWallet in wallets) {

				RippleWallet rw = rippleWallet;
				//tasks.Add (Task.Run (

				//		delegate {

				//rw.Notification = "Updating balance...";
				//WalletManager.currentInstance?.UpdateUI ();

				try {

					RippleCurrency rippleCurrency = AccountInfo.GetNativeBalance (rw.GetStoredReceiveAddress (), ni, token);

					DateTime dateTime = DateTime.Now;


					if (rippleCurrency != null) {
						rw.LastKnownNativeBalance = rippleCurrency;
						rw.Notification = null;
						rw.BalanceNote = "balance updated as of " + dateTime.ToShortTimeString ();
					} else {

						if (rw.CouldNotUpdateBalanceCount++ < 1) {
							rw.Notification = "<span fgcolor=\"salmon\">Could not update balance</span>";
						} else {
							rw.Notification = null;
							rw.BalanceNote = "<span fgcolor=\"salmon\">Could not update balance</span>";
						}
						
					}

					WalletManager.currentInstance?.UpdateUI ();

					if (rw?.NotificationLedger == null || rw.NotificationLedger == 0) {

						rw.NotificationLedger = ledger;
						rw.SaveBotLedger (ledger, rw.NotificationLadgerPath);
						continue;
						//return;
					}


					DoLogicResponse tuple = DoOfferLogic (rw, ni);
					if (tuple == null) {
						continue;
						//return;
					}

					if (tuple.HasError) {
						if (tuple.ErrorCode == 55 ) {
							rw.NotificationLedger = 0;
							rw.SaveBotLedger (0, rw.NotificationLadgerPath);
						}

						
						continue;
					}

		    			

					IEnumerable<AutomatedOrder> totalFilled = tuple.FilledOrders;
					if (totalFilled == null) {
						rw.NotificationLedger = tuple.LastLedger;
						rw.SaveBotLedger (tuple.LastLedger, rw.NotificationLadgerPath);
						continue;
					}


					int c = totalFilled.Count ();

					if (c == 0) {
						rw.NotificationLedger = tuple.LastLedger;
						rw.SaveBotLedger (tuple.LastLedger, rw.NotificationLadgerPath);
						continue;
					}


					string message2 =
						c.ToString () +
						" new orders";

					rw.Notification = message2;


					rw.NotificationLedger = tuple.LastLedger;
					rw.SaveBotLedger (tuple.LastLedger, rw.NotificationLadgerPath);

					count += c;
					accnts += 1;
					continue;
					//return;

				}
#pragma warning disable 0168
						catch (Exception ex) {
#pragma warning restore 0168

#if DEBUG
					Logging.ReportException (method_sig, ex);
#endif

					continue;
				} finally {
					WalletManager.currentInstance?.UpdateUI ();
				}


				//}



				//));

			}

			//if (tasks != null && tasks.Count > 0) {
			//	Task.WaitAll (tasks.ToArray (), 15000);
			//Task.a
			//}


			//var va = from Task<int> t in tasks
			//select t.Result;

			int am = accnts; //va.Count ();
			int grandTotal = count;

			if (grandTotal == 0) {
				return;

			}

			string message = am.ToString () + " accounts have a total of \n" + grandTotal.ToString () + " filled orders";

			Application.Invoke (
				delegate {
					StatusTrayIcon.Blinking = true;
					// display even if tooltips disabled, it's functionality not help
					StatusTrayIcon.Tooltip = grandTotal.ToString () + " Orders have filled";
				}
			);

			ShowNotification (message, 0);


			SoundSettings settings = SoundSettings.LoadSoundSettings ();
			if (settings.HasOnOrderFilled && settings.OnOrderFilled != null) {

				Task.Run (delegate {

					SoundPlayer player =
					new SoundPlayer (settings.OnOrderFilled);
					player.Load ();
					player.Play ();
				});

			} else if (settings.FallBack) {
				this.PlayNotification ();
			}






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

		public DoLogicResponse DoOfferLogic (RippleWallet wallet, NetworkInterface networkInterface)
		{

#if DEBUG
			string method_sig = clsstr + nameof (DoOfferLogic) + DebugRippleLibSharp.left_parentheses + nameof (wallet) + DebugRippleLibSharp.comma + nameof (NetworkInterface) + DebugRippleLibSharp.right_parentheses;
#endif

			DoLogicResponse logicResponse = new DoLogicResponse ();
			 
			string ledgerMax = (-1).ToString ();
			string ledgerMin = wallet.NotificationLedger.ToString ();


			uint lastKnownLedger = 0;
			//int lim = limit ?? 200;

			Task<FullTxResponse> task = null;

			try {
				task =
					AccountTx.GetFullTxResult (
						wallet.Account,
						ledgerMin,
						ledgerMax,
						networkInterface,
						_token
					);
				if (task == null) {
					//return null;

					logicResponse.HasError = true;
					logicResponse.ErrorMessage += "Get account tx returned null\n";
					return logicResponse;
					//throw new NullReferenceException ();
				}


				task.Wait (15000, _token);


			} catch (Exception e) {

				StringBuilder stringBuilder = new StringBuilder ();
				stringBuilder.AppendLine ("Network exception");
				stringBuilder.AppendLine (e.Message);
				stringBuilder.AppendLine (e.StackTrace);
				while (e.InnerException != null) {

					stringBuilder.AppendLine ("Inner Exception");
					stringBuilder.AppendLine (e.InnerException.Message);
					stringBuilder.AppendLine (e.InnerException.StackTrace);

					e = e.InnerException;
				}
				Logging.WriteBoth (stringBuilder.ToString ());

				MessageDialog.ShowMessage (stringBuilder.ToString ());

				logicResponse.HasError = true;
				logicResponse.ErrorMessage += stringBuilder.ToString ();
				return logicResponse;


			}

			FullTxResponse fullTx = task.Result;
			if (fullTx == null) {

				logicResponse.HasError = true;
				logicResponse.ErrorMessage += "Fetch task returned null\n";
				return logicResponse;

			}

			if (fullTx.HasError) {
				logicResponse.HasError = true;
				logicResponse.ErrorMessage += fullTx.ErrorMessage;

				logicResponse.ErrorCode = fullTx.TroubleResponse.error_code;

				return logicResponse;
				
			}

			IEnumerable<Response<AccountTxResult>> res = fullTx.Responses;

			if (res == null) {
				logicResponse.HasError = true;
				logicResponse.ErrorMessage += "Account tx response is null\n";
				return logicResponse;
			}

			IEnumerable<AccountTxResult> results =
				from Response<AccountTxResult> r in res
				where r != null
				&& r.result != null
				select r.result;

			if (
				results == null
				//|| results.Count() == 0
				|| !results.Any ()
			) {


				return logicResponse;
			}

			//List<RippleTxStructure> list = new List<RippleTxStructure> ();

			/*
			foreach (AccountTxResult re in results) {


				IEnumerable<RippleTxStructure> txs = re.transactions;

				list.AddRange (txs);
			}
	    		*/

			uint max_ledger = results.Max (x => (uint)x.ledger_index_max);
			lastKnownLedger = max_ledger;
			IEnumerable<RippleTxStructure> txList = results.SelectMany (a => a.transactions);


			IEnumerable<RippleTxStructure> payments =
				from RippleTxStructure tx in txList
				where tx.tx.TransactionType == "Payment"
				&& tx.tx.Account != wallet.Account // Not payments made from this wallet. I'd hope the user already knows they sent a payment 
				&& tx.tx.Destination == wallet.Account
				select tx;

			NotifyPayments (payments, wallet);

			OrderManagementBot omb = new OrderManagementBot (wallet, networkInterface, _token);

			IEnumerable<AutomatedOrder> total = null;

			try {

				total = omb.UpdateTx (txList);
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

				logicResponse.HasError = true;
				logicResponse.ErrorMessage += "Excption thrown";
				logicResponse.ErrorMessage += e.Message;
				logicResponse.ErrorMessage += e.StackTrace;

				return logicResponse;
			}

			logicResponse.LastLedger = lastKnownLedger;
			logicResponse.FilledOrders = total;

			return logicResponse;

		}




		public void NotifyPayments (IEnumerable<RippleTxStructure> paymentsStructures, RippleWallet rippleWallet)
		{
			SoundSettings settings = SoundSettings.LoadSoundSettings ();
			int count = 0;
			StringBuilder builder = new StringBuilder ();
			foreach (RippleTxStructure txStructure in paymentsStructures) {

				builder.Clear ();

				RippleCurrency amount = txStructure.meta.delivered_amount;

				builder.Append ("Received a payment of ");

				if (amount.IsNative) {

					builder.Append (amount.amount / 1000000);
					builder.Append (" ");
					builder.Append (RippleCurrency.NativeCurrency);
				} else {
					builder.Append (amount.amount);
					builder.Append (" ");
					builder.Append (amount.currency);
				}

				rippleWallet.Notification = builder.ToString ();

				ShowNotification (builder.ToString(), count++);

				if (settings.HasOnPaymentReceived && settings.OnPaymentReceived != null) {

					Task.Run (delegate {

						SoundPlayer player =
						new SoundPlayer (settings.OnPaymentReceived);
						player.Load ();
						player.Play ();
					});

				} else if (settings.FallBack) {
					this.PlayNotification ();
				}

			}
		}

		public Task InitNotificationSystem () => Task.Run (
				delegate {
					using (ManualResetEvent mre = new ManualResetEvent (false)) {
						mre.Reset ();
						Application.Invoke (delegate {



							Gdk.Pixbuf icon = Gdk.PixbufLoader.LoadFromResource (nameof (IhildaWallet) + ".Images.ih_alpha.png").Pixbuf;

							StatusTrayIcon = new StatusIcon {
								Pixbuf = icon,
								Blinking = false,
								Visible = true

							};


							var cur = WalletManagerWindow.currentInstance;
							if (cur != null) {
								cur.ExposeEvent += delegate {
									StatusTrayIcon.Blinking = false;
								};

								cur.EnterNotifyEvent += delegate {
									StatusTrayIcon.Blinking = false;
								};
							}

							StatusTrayIcon.Activate += delegate {
								var cu = WalletManagerWindow.currentInstance;
								if (cu != null) {
									if (cu.Visible) {
										cu.Visible = false;
									} else {

										Program.splash?.Hide ();


										cu.Visible = true;

									}
								}


							};


							mre.Set ();

						});

						//mre.WaitOne ();
						WaitHandle.WaitAny (new [] { mre, _token.WaitHandle });
					}

					if (!Program.network) {
					// if networking explicitly prohibited don't do networking loop. 
						return;

					}
					try {
						//Thread.Sleep(1000);
					Task.Delay (1000).Wait (1000, _token);
					}
#pragma warning disable 0168
					catch (Exception ex) {
#pragma warning restore 0168
						MessageDialog.ShowMessage ("hmmmm");
					}

					//ThreadStart ts = doLoop;
					//Thread thread = new Thread(ts);
					//thread.Start();

					Task.Run ((System.Action)DoLoop, _token);
				}
			);

		public void DoLoop ()
		{

#if DEBUG
			string method_sig = clsstr + nameof (DoLoop) + DebugRippleLibSharp.colon;

#endif


			/*
			for (int i = 0; i < 6; i++) {
				T_Elapsed (null, null);
			}
			*/

			//Stopwatch watch = new stopwatch ()

			while (!_token.IsCancellationRequested) {
				try {
					T_Elapsed (null, null);
					int x = WalletManager.currentInstance.wallets.Count;

					//Task.Delay (15000);
					//Thread.Sleep (15000);
					
				} catch (Exception e) {
#if DEBUG
					if (DebugIhildaWallet.NotificationThread) {
						Logging.ReportException (method_sig, e);
					}
#endif
				} finally {
					_token.WaitHandle.WaitOne (15000);
				}
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

