using System;
using System.Threading;
using System.Threading.Tasks;


using RippleLibSharp.Network;
using Gtk;
using RippleLibSharp.Keys;

using IhildaWallet.Splashes;
using IhildaWallet.Util;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WalletManagerWidget : Gtk.Bin
	{
		public WalletManagerWidget ()
		{
			#if DEBUG
			string method_sig = clsstr + nameof (WalletManagerWidget) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.WalletManagerWidget) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}
			#endif

			this.Build ();

			if (this.wallettree1 == null) {
				this.wallettree1 = new WalletTree ();
				this.wallettree1.Show ();
				vbox4.Add ( this.wallettree1 );
			}

			this.SetActions ();
			#if DEBUG

			if (DebugIhildaWallet.WalletManagerWidget) {
				Logging.WriteLog(method_sig + DebugIhildaWallet.buildComp);
			}
			#endif

			currentInstance = this;

			this.botbutton.Clicked += (sender, e) => Task.Run ((System.Action)BotButtonClicked);



			// works as expected
			this.useButton.Clicked += (sender, e) => {

#if DEBUG
				string event_sig = clsstr + nameof (useButton) + ".Clicked" + DebugRippleLibSharp.colon;

				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog (event_sig + DebugRippleLibSharp.begin);
				}
#endif


				Task.Run ((System.Action)Payment);

			};

			this.tradepairButton.Clicked += (sender, e) => {
#if DEBUG
				string event_sig = clsstr + nameof (tradepairButton) + ".Clicked event" + DebugRippleLibSharp.colon;
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog (event_sig + DebugRippleLibSharp.beginn);
				}
#endif

				Task.Run ((System.Action)TradePair);

			};


			this.editButton.Clicked += (sender, e) => {

#if DEBUG
				String event_sig = clsstr + nameof (editButton) + ".Clicked event" + DebugRippleLibSharp.colon;
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog (event_sig + DebugRippleLibSharp.beginn);
				}
#endif

				Edit ();
			};

			this.deleteButton.Clicked += (sender, e) => Delete ();


			this.importButton.Clicked += (sender, e) => {

				WalletManager.ImportWallet ();

				this.UpdateUI ();
			};

			this.exportButton.Clicked += (sender, e) => Export ();


			this.newbutton.Clicked += (sender, e) => {

#if DEBUG

				String event_sig = clsstr + "newbutton.Clicked () : ";
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog (event_sig);
				}

#endif

				New_Wallet_Wizard ();

			};






			this.quitButton.Clicked += Program.QuitRequest;


			// trust button
			this.trustbutton.Clicked += (sender, e) => {

#if DEBUG
				String event_sig = clsstr + nameof (trustbutton) + " clicked : ";
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog (event_sig);
				}
#endif



				Task.Run ((System.Action)Trust);

			};


			this.networkbutton1.Clicked += (sender, e) => NetworkSettingsDialog.ShowDialog ();


			this.txbutton.Clicked += (sender, e) => {

#if DEBUG
				String event_sig = clsstr + nameof (txbutton) + "Clicked : ";
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog (event_sig + DebugRippleLibSharp.beginn);
				}
#endif


				Task.Run ((System.Action)Txview);
			};
		
			this.optionsbutton1.Clicked += (sender, e) => {



				OptionsWindow opswin = new OptionsWindow ();
				opswin.Show ();

				/*
				using (OptionsWindow opswin = new OptionsWindow ()) {
					try {
					}
					catch (  Exception ee) {
						if (Debug.WalletManagerWidget) {
							Logging.write("");
						}
					}

					finally {
						opswin.Destroy();
					}

				}
				*/

			};

			this.consolebutton1.Clicked += (sender, e) => {

				ConsoleWindow consoleWin = ConsoleWindow.GetConsoleWindow ();
				consoleWin.Reshow ();

			};

			this.button67.Clicked += (sender, e) => Task.Run ((System.Action)EncryptWallet);

			this.button66.Clicked += (sender, e) => Task.Run ((System.Action)DecryptWallet);
			this.eventbox1.ModifyBg (StateType.Normal, new Gdk.Color (0, 0, 0));
			wallettree1.GrabFocus ();
		}

		public void SetActions () {
			this.NewAction.Activated += (sender, e) => New_Wallet_Wizard ();

			this.EditWalletAction.Activated += (sender, e) => Edit ();

			this.UpgradeAction.Activated += (sender, e) => {

			};

			this.EncryptAction.Activated += (sender, e) => EncryptWallet ();

			this.DecryptAction.Activated += (sender, e) => DecryptWallet ();

			this.ImportAction.Activated += (sender, e) => {
				WalletManager.ImportWallet ();

				this.UpdateUI ();
			};

			this.ExportAction.Activated += (sender, e) => Export ();

			this.TradingMarketAction.Activated += (sender, e) => TradePair ();

			this.MakeAPaymentAction.Activated += (sender, e) => Task.Run ((System.Action)Payment);

			this.MarketBotAction.Activated += (sender, e) => Task.Run ((System.Action)BotButtonClicked);

			this.ManageTrustAction.Activated += (sender, e) => Task.Run ((System.Action)Trust);

		}

		public void EncryptWallet () {


			RippleWallet rw = WalletManager.GetRippleWallet();
			if (rw == null) {
				return;
			}

		

			rw.EncryptWithSideEffects ();

			rw.Save ();

			this.UpdateUI ();

		}

		public void DecryptWallet () {
			RippleWallet rw = WalletManager.GetRippleWallet();
			if (rw == null) {
				return;
			}



			//IEncrypt ie = new Rijndaelio ();


			rw.DecryptWithSideEffects ();
			rw.Save ();

			this.UpdateUI ();
		}

		public void Txview () {
			
			#if DEBUG
			string method_sig = clsstr + nameof (Txview) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.WalletManagerWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
			#endif

			//RippleWallet rw = getRippleWalletNonGtkThread ();
			RippleWallet rw = WalletManager.GetRippleWallet();
			string addr = null;
			RippleAddress ra = null;
			if (rw != null) {
				addr= rw.GetStoredReceiveAddress ();
				ra = new RippleAddress (addr);
			}

			 
			 

			Gtk.Application.Invoke ((sender, e) => {
				TxWindow txv = new TxWindow (ra);
				//txv.ShowAll();
				txv.SetRippleAddress (ra);

				//txv.HideAll();
			});
				
				



		}

		public void Delete ()
		{
			//RippleWallet rw = wallettree2.getSelected();
			RippleWallet rw = WalletManager.GetRippleWallet();
			if (rw!=null) {
				rw.ForgetDialog();
			}
			else {
				NoWalletSelected();
				// todo debug
			}

			this.UpdateUI();

		}

		public void TradePair () {
			

			// let this be free and do token licensing in tradePair manager

			Application.Invoke ((sender, e) => {
				if (TradePairManagerWindow.CurrentInstance == null) {
					TradePairManagerWindow.CurrentInstance = new TradePairManagerWindow ();
				}
				TradePairManagerWindow.CurrentInstance?.Show ();
				TradePairManagerWindow.CurrentInstance.GrabFocus ();
			});

		}

		public void Export ()
		{
			//RippleWallet rw = wallettree2.getSelected();
			RippleWallet rw = WalletManager.GetRippleWallet();
			if (rw!=null) {
				rw.ExportWallet();
			}
		
			else {
				NoWalletSelected();
				// todo debug
			}
			// this.updateUI();  // TODO, figure out if updating the ui is worth the slowness.  // does nothing anyway

			// todo show popup alert??
		}

		public void Edit ()
		{
			#if DEBUG
			String method_sig = clsstr + nameof (Edit) + DebugRippleLibSharp.both_parentheses;
			if ( DebugIhildaWallet.WalletManagerWidget ) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
			#endif

			//RippleWallet rw = wallettree2.getSelected();
			RippleWallet rw = WalletManager.GetRippleWallet();
			if (rw==null) {
				NoWalletSelected();
				return;
			}
				
			#if DEBUG
			if (DebugIhildaWallet.WalletManagerWidget) {
				Logging.WriteLog(method_sig + "wallet is not null");

			}
#endif


			FromSecretDialog fsd = new FromSecretDialog ("Edit Wallet", rw) {
				Modal = true
			};


			ResponseType ret = (ResponseType) fsd.Run ();


			if (ret == ResponseType.Ok) {
				#if DEBUG
				if (DebugIhildaWallet.WalletManagerWidget) {
				
					Logging.WriteLog(method_sig + "Response is OK");
					
				}
				#endif
				// todo replace one wallet with another

				RippleWallet rw_new = fsd.GetWallet();
				if (rw_new == null) {
					#if DEBUG
					if (DebugIhildaWallet.WalletManagerWidget) {
						Logging.WriteLog(method_sig + "new wallet is null");
					}
					#endif
					return;
				}

				walletManager.Replace(rw, rw_new);

			}

			else {
				#if DEBUG
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog(method_sig + "Response is NOT ok");
				}
				#endif
			}

			fsd.Destroy();
			// TODO swtitch these two around?
			this.UpdateUI();
		}

		/*
		 *  No longer needed now that GUI is separated from logic
		 * 
		public RippleWallet getRippleWalletNonGtkThread () {

			String method_sig = clsstr + "getRippleWalletNonGtkThread () : ";

			if (Debug.WalletManagerWidget) {
				Logging.writeLog ( method_sig + Debug.begin);
			}

			EventWaitHandle waith = new ManualResetEvent( true );

			waith.Reset ();

			RippleWallet rw = null;

			Application.Invoke (delegate {
				if (Debug.WalletManagerWidget) {
					Logging.writeLog(method_sig + "gtk invoke");
				}
				rw = wallettree2.getSelected();
				waith.Set();
			});

			waith.WaitOne (5000);

			if (rw == null) {
				noWalletSelected ();
				return null;
			}

			string addr = rw.getStoredReceiveAddress ();
			bool valid = true;
			try {
				RippleAddress ra = new RippleAddress(addr);
				valid = (ra != null);
			}

			catch (FormatException fe) {
				if (Debug.WalletManagerWidget) {
					Logging.writeLog (fe.Message);
				}
				valid = false;
			}

			catch (Exception e) {
				if (Debug.WalletManagerWidget) {
					Logging.writeLog (e.Message);
				}
				valid = false;
			}

			if (!valid) {
				MessageDialog.showMessage ("invalid wallet");
				return null;
			}

			if (Debug.WalletManagerWidget) {
				Logging.writeLog ( method_sig + "returning " + (string)(rw == null ? "null" : rw.getStoredReceiveAddress()));
			}



			return rw;
		}
		*/


		public void Trust ()
		{

			#if DEBUG
			String method_sig = clsstr + nameof (Trust) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.WalletManagerWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
			#endif

			LoadingWindow loadingwin = null;



			//RippleWallet rw = getRippleWalletNonGtkThread ();
			RippleWallet rw = WalletManager.GetRippleWallet();
			if (rw == null) {
				#if DEBUG
				if ( DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog (method_sig + "No wallet selected");
				}
				#endif

				return;
			}

			bool shouldContinue = LeIceSense.DoTrialDialog (rw, LicenseType.TRUST);
			if (!shouldContinue) {
				return;
			}

			Task t1 = Task.Run (delegate {
				EventWaitHandle wh = new ManualResetEvent (true);
				wh.Reset();
				Gtk.Application.Invoke (
					delegate {
						loadingwin = new LoadingWindow ();
						loadingwin.Show ();
						wh.Set();

					}
				);
				wh.WaitOne();

			});

			/*
			Task t2 = Task.Run ( delegate {
				if (WalletManagerWindow.currentInstance!=null) {
					#if DEBUG
					if (DebugIhildaWallet.WalletManagerWidget) {
						Logging.WriteLog(method_sig + "hiding wallet manager");
					}
					#endif
					EventWaitHandle wh = new ManualResetEvent(true);
					wh.Reset();
					Gtk.Application.Invoke( 
						delegate {
							WalletManagerWindow.currentInstance.Hide();
							wh.Set();
						}
					);
					wh.WaitOne();


				}
			});
			*/


			Task[] tasks = { t1, /*t2*/ };

			Task.WaitAll (tasks);

			//Gtk.Application.Invoke( delegate {
			//	paymentVehicleInit(rw); 
			//});

			Task<TrustManagementWindow> t3 = TrustManagementWindow.InitGUI(rw);

				/*new Task (delegate {
				

			});*/




			Task t4 = Task.Run (
				delegate {
					// TODO uncomment
					//NetworkInterface.blockingInitNetworking ();

				}
			);

			tasks = new Task[] { t3, t4 };


			Task.WaitAll (tasks);

			var trustManagementWindow = t3.Result;

			if (trustManagementWindow == null) {
				// Todo redundant warning. 
				return;
			}




			Gtk.Application.Invoke (delegate {
				loadingwin.Hide();
				loadingwin.Destroy();
				loadingwin = null;

			});

			Gtk.Application.Invoke( delegate {
				#if DEBUG
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog (method_sig + "showing trust window");
				}
				#endif
				trustManagementWindow.Show();

				trustManagementWindow.Visible = true;
			});

			//

		}

		public void Payment (  )
		{
			#if DEBUG
			String method_sig = clsstr + nameof (Payment) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.WalletManagerWidget) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}
			#endif

			LoadingWindow loadingwin = null;
			Gtk.Application.Invoke (
				delegate {
				loadingwin = new LoadingWindow();
				loadingwin.Show();
			});

			try {

				//RippleWallet rw = getRippleWalletNonGtkThread ();
				RippleWallet rw = WalletManager.GetRippleWallet ();
				if (rw == null)  //
				{
					Gtk.Application.Invoke (delegate {
						loadingwin.Hide ();
						loadingwin.Destroy ();
						loadingwin = null;
						NoWalletSelected ();

					});
					return;

				}

				bool ShouldContinue = LeIceSense.DoTrialDialog (rw, LicenseType.PAYMENT);

				if (!ShouldContinue) {
					Gtk.Application.Invoke (delegate {
						loadingwin.Hide ();
						loadingwin.Destroy ();
						loadingwin = null;


					});
					return;
				}
				/*
				if (WalletManagerWindow.currentInstance!=null) {
					#if DEBUG
					if (Debug.WalletManagerWidget) {
						Logging.writeLog(method_sig + "hiding wallet manager");
					}
					#endif
					Gtk.Application.Invoke( delegate {
						WalletManagerWindow.currentInstance.Hide();
					});


				}
				*/



				/*
				if (WalletManagerWindow.currentInstance!=null) {
					#if DEBUG
					if (DebugIhildaWallet.WalletManagerWidget) {
						Logging.WriteLog(method_sig + "hiding " + nameof (DebugRippleLibSharp));
					}
					#endif
					Application.Invoke ((sender, e) => WalletManagerWindow.currentInstance.Hide ());
				}
				*/


				Task<PaymentWindow> task = PaymentWindow.InitGUI ();
				task.Wait ();



				PaymentWindow paymentWindow = task.Result;

				if (paymentWindow != null) {
#if DEBUG
					if (DebugIhildaWallet.WalletManagerWidget) {
						Logging.WriteLog (method_sig + "showing " + nameof (WalletManagerWindow));
					}
#endif
					paymentWindow.SetRippleWallet (rw);

					Application.Invoke ((sender, e) => paymentWindow.ShowAll ());

				} else {
					// DEBUG
				}


			} catch (Exception e) {
#if DEBUG
				Logging.ReportException (method_sig, e);
#endif
			}

			finally {
				Application.Invoke ((sender, e) => {
					if (loadingwin != null) {
						loadingwin?.Hide ();
						loadingwin?.Destroy ();
						loadingwin = null;
					}

				});
			}


		}


		public void UpdateUI ()
		{
			
			#if DEBUG
			String method_sig = clsstr + nameof (UpdateUI) + DebugRippleLibSharp.both_parentheses;

			if (DebugIhildaWallet.WalletManagerWidget) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
			#endif

			if (wallettree1 == null) {
			
				#if DEBUG
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog (method_sig + "wallettree == " + DebugRippleLibSharp.null_str);
				}
				#endif

				return;

			}

			//lock (WalletManager.walletLock) {
				if (walletManager.wallets == null || walletManager.wallets.Values == null) 
				{
					#if DEBUG
					if (DebugIhildaWallet.WalletManagerWidget) {
						Logging.WriteLog(method_sig + "null value in walletmanager setting values to zero");
					}
					#endif

					//Application.Invoke( delegate {
					wallettree1.ClearValues();
					//});

					return;
				}

				if (walletManager.wallets.Values.Count > 0) {
			
					#if DEBUG
					if (DebugIhildaWallet.WalletManagerWidget) {
						Logging.WriteLog(method_sig + "walletManager.wallets.Values.Count = " + walletManager.wallets.Values.Count.ToString());
	
						Logging.WriteLog(method_sig + "setting values");
					}
					#endif

				
					wallettree1.SetValues(walletManager.wallets.Values);

				

				}

				else {
					#if DEBUG
					if (DebugIhildaWallet.WalletManagerWidget) {
						Logging.WriteLog(method_sig + "value is not greater than zero, clearing values");
					}
					#endif

				
					wallettree1.ClearValues();
				

				}
			//}
				

		}

		public RippleWallet FromSecret ()
		{
			#if DEBUG
			String method_sig = clsstr + nameof (FromSecret) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.WalletManagerWidget) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
			#endif
			using (FromSecretDialog fsd = new FromSecretDialog()) {

				fsd.Modal = true;


				while (true) {
					#if DEBUG
					if (DebugIhildaWallet.WalletManagerWidget) {
						Logging.WriteLog(method_sig + "while (true) begining from secret dialog");
					}
					#endif
					ResponseType ret = (ResponseType)fsd.Run ();
					fsd.Hide();

					if (ret != ResponseType.Ok) {
						#if DEBUG
						if (DebugIhildaWallet.WalletManagerWidget) {
							Logging.WriteLog(method_sig + "User did not click ok");
						}
						#endif
						fsd.Destroy ();
						 
						return null;
					}

					#if DEBUG
						// todo 
					if (DebugIhildaWallet.WalletManagerWidget) {
						Logging.WriteLog(method_sig + "User selected OK");
					}
					#endif
						
					RippleWallet rw = fsd.GetWallet ();

					if (rw == null) {
						#if DEBUG
						if (DebugIhildaWallet.WalletManagerWidget) {
							Logging.WriteLog(method_sig + "rw == null");
						}
						#endif
						continue;
					}

					#if DEBUG
					if (DebugIhildaWallet.WalletManagerWidget) {
						Logging.WriteLog(method_sig + "rw != null");
					}
					#endif

					//walletManager.addWallet (rw);
					//initiateWalletAddThread(rw);

					fsd.Destroy ();
					return rw;
						


				}


				//fsd.Destroy ();
			}

			//return null;

		}

		public void New_Wallet_Wizard ()
		{
			#if DEBUG
			String method_sig = clsstr + "new_wallet_wizard : ";
			if (DebugIhildaWallet.WalletManagerWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);

			}
			#endif

			if (walletManager == null) {
				#if DEBUG
				if (DebugIhildaWallet.WalletManager) {
					Logging.WriteLog(method_sig + "walletManager == null, returning");
				}
				#endif
				return;
			}



			using ( NewButtonDialog nbd = new NewButtonDialog() ) {

				try {

					ResponseType ret = (ResponseType)nbd.Run ();
					nbd.Destroy();

					if (ret == ResponseType.Ok) {
						RippleWallet rw = null;
						IhildaWallet.NewButtonDialog.NewOption no = nbd.GetSelection();
						#if DEBUG
						string usel = "user selected ";
#endif
						switch (no) {
						case NewButtonDialog.NewOption.SECRET:

#if DEBUG
							if (DebugIhildaWallet.WalletManagerWidget) {
								Logging.WriteLog (method_sig + usel + "SECRET");
							}
#endif

							rw = FromSecret ();

							break;

						case NewButtonDialog.NewOption.SCRIPT:
#if DEBUG
							if (DebugIhildaWallet.WalletManagerWidget) {
								Logging.WriteLog (method_sig + usel + "user selected SCRIPT");
							}
#endif
							FromScript ();
							return; // we are going to return because 

						case NewButtonDialog.NewOption.RANDOM:
#if DEBUG
							if (DebugIhildaWallet.WalletManagerWidget) {
								Logging.WriteLog (method_sig + usel + "user selected RANDOM");
							}
#endif
							rw = FromRandom ();
							break;



						case NewButtonDialog.NewOption.FILE:
#if DEBUG
							if (DebugIhildaWallet.WalletManagerWidget) {
								Logging.WriteLog (method_sig + usel + "user selected ");
							}
#endif

							WalletManager.ImportWallet ();

							this.UpdateUI ();
							return;
						default:
							break;
						}  // ends switch

						InitiateWalletAddThread (rw); 
							return;
					}


					// TODO responsetype != ok
					// do nada :)

#if DEBUG
					if (DebugIhildaWallet.WalletManagerWidget) {
						Logging.WriteLog(method_sig + "user didn't select OK");
					}
#endif


				}


#pragma warning disable 0168
				catch (Exception jic) {
					#pragma warning restore 0168

					#if DEBUG
						if (DebugIhildaWallet.WalletManagerWidget) {
						Logging.WriteLog (method_sig + DebugRippleLibSharp.exceptionMessage + jic.Message );
						}
					#endif
				}

				//finally {
				//	nbd.Destroy(); // this IS needed desite the using. Dispose() does not Destroy()
				//walletManager.loadWallets();
				//updateUI();
				//}

			}

		}

		private static void InitiateWalletAddThread (RippleWallet rw)
		{
			#if DEBUG
			// todo double check no secret info being leaked in debug and add if allowinsucure debugging
			String method_sig = clsstr + nameof (InitiateWalletAddThread) + DebugRippleLibSharp.left_parentheses + DebugIhildaWallet.ToAssertString(rw) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.WalletManagerWidget) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
			#endif
			

			if (rw == null) { 
				#if DEBUG
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog(method_sig + "rw == null, " + DebugRippleLibSharp.right_parentheses);
				}
				#endif
				return;
			}


			#if DEBUG
			if (DebugIhildaWallet.WalletManagerWidget) {
				Logging.WriteLog(method_sig + "rw != null");
			}
			#endif
				//walletManager.addWallet(rw);
			ParameterizedThreadStart thstrart = new ParameterizedThreadStart(ThreadedWalletAdd );

			Thread th = new Thread ( thstrart );
			th.Start(rw);
		}

		/*
		public bool paymentVehicleInit (RippleWallet rw)
		{
			String method_sig = clsstr + "paymentVehicleInit () : ";
			if (Debug.WalletManagerWidget) {
				Logging.writeLog (method_sig + Debug.begin);
			}


			if (rw==null) {
				if (Debug.WalletManagerWidget) {
					Logging.writeLog(method_sig + "rw == null, returning");
				}
				return false;
			}

			if (Debug.WalletManagerWidget) {
				Logging.writeLog(method_sig + "wallet is not null");
			}

				

			if (MainWindow.currentInstance==null) { // it better not be null !!
				if (Debug.WalletManagerWidget) {
					Logging.writeLog(method_sig + "MainWindow == null, creating new MainWindow, this is a bug");
				}

				EventWaitHandle wHandle = new ManualResetEvent( true );
				wHandle.Reset ();

				Application.Invoke (delegate {
					new MainWindow (); // slow as @$#
					wHandle.Set();
				});

				wHandle.WaitOne (50000);
			}
				
			// connect if auto connect
			//NetworkInterface.initNetworking();


			
			WalletManager.selectedWallet = rw;

			// TODO uncomment? or set another way
			MainWindow.currentInstance.setRippleWallet(rw);


			return true;
		}
		*/

		public RippleWallet FromRandom ()
		{
			#if DEBUG
			String method_sig = clsstr + nameof (FromRandom) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.WalletManagerWidget) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}
			#endif

			RippleWallet rw = null;
			FromSecretDialog fsd = null;

			// much thought was put into avoiding goto. I know so many people dislike it. 
			// This is an acceptable use of goto as it results in the cleanest code 
			// it used to be a while loop, there was problems with proper flow of logic

			//while (loop) {
			#if DEBUG
			if (DebugIhildaWallet.WalletManagerWidget) {
				Logging.WriteLog(method_sig + "begin loop");
			}
			#endif

			using (RandomSeedGenerator rsg = new RandomSeedGenerator()) {
				#if DEBUG
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog(method_sig + "begin using RandomSeedGenerator");
				}
				#endif
				// goto
			RANDOM:

				ResponseType resp = (ResponseType)rsg.Run ();
				rsg.Hide();
				#if DEBUG
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog(method_sig + "user made selection");
				}
				#endif

				if (resp != ResponseType.Ok) {
					#if DEBUG
					if (DebugIhildaWallet.WalletManagerWidget) {
						Logging.WriteLog(method_sig + "user did not click ok, breaking");
					}
					#endif

					goto ENDING;
				}

				#if DEBUG
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog (method_sig + "user clicked ok, generating wallet....");
				}
				#endif

			NEWSEED:
				RippleSeedAddress seed = rsg.GetGeneratedSeed ();
	
				if ( seed == null ) {
					#if DEBUG
					if ( DebugIhildaWallet.WalletManagerWidget ) {
						Logging.WriteLog( method_sig + "seed == null");
					}
					#endif

					goto ENDING;
					//break;
				}

				#if DEBUG
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog( method_sig + "seed != null" );
				}
				#endif
						

				try {
					rw = new RippleWallet (seed);
					#pragma warning disable 0168
				} catch (Exception ex) {
					#pragma warning restore 0168


					#if DEBUG
					if (DebugIhildaWallet.WalletManagerWidget) {
						Logging.WriteLog (method_sig + DebugRippleLibSharp.exceptionMessage + ex.Message);
						//goto DA_END;
						//break;
						Logging.WriteLog (method_sig + "trying another seed ...");
					}
					#endif
					goto NEWSEED; // try another seed

				}

				if (rw == null) {
					#if DEBUG
					if (DebugIhildaWallet.WalletManagerWidget) {
						Logging.WriteLog(method_sig + "rw == null");
					}
					#endif
					//break;
					goto NEWSEED;
				}

				#if DEBUG
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog(method_sig + "rw != null");
					Logging.WriteLog(method_sig + "Starting confirmation dialog");
				}
				#endif

				fsd = new FromSecretDialog ("Confirm Wallet", rw);

				#if DEBUG
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog(method_sig + "Running dialog");
				}
				#endif

			CONFIRM:
				ResponseType ret = (ResponseType)fsd.Run ();
				fsd.Hide ();

				if (ret == ResponseType.Ok) {
					// todo replace one wallet with another
#if DEBUG
					if (DebugIhildaWallet.WalletManagerWidget) {
						Logging.WriteLog (method_sig + "user clicked Ok");
					}
#endif

					rw = fsd.GetWallet ();

					if (rw == null) {

						// TODO double check that in this case flow shouldn't jump to label RANDOM
						goto CONFIRM;
					}


					goto ENDING;  //  

				}
#if DEBUG
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog (method_sig + "user didn't click ok, continueing");
				}
#endif

				goto RANDOM;

			ENDING:
				// one statement is needed after the label for the goto to index properly
				#if DEBUG
				if (DebugIhildaWallet.WalletManagerWidget) { 
					Logging.WriteLog(method_sig + "ENDING");
				}
				#endif

				rsg.Destroy();
				if (fsd != null) {
					fsd.Destroy();
				}
			} // end using


			

			return rw;

		}

		public void FromScript ()
		{
			#if DEBUG
			String method_sig = clsstr + nameof (FromScript) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.WalletManagerWidget) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
			#endif

			FromScriptDialog.DoDialog ();


		}

		public static void OnScriptSuccess (RippleSeedAddress seed)
		{
			#if DEBUG
			String method_sig = clsstr + nameof (OnScriptSuccess) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.WalletManagerWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
			#endif

			if (seed == null) {
				#if DEBUG
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog (method_sig + "seed == null, returning null");
				}
				#endif
				return;
			} 
			#if DEBUG
			if (DebugIhildaWallet.WalletManagerWidget ) {
				Logging.WriteLog(method_sig + "seed = " + DebugIhildaWallet.AssertAllowInsecure(seed));
			}
			#endif


			RippleWallet rw = new RippleWallet (seed);
			#if DEBUG
			if (DebugIhildaWallet.WalletManagerWidget) {
				Logging.WriteLog(method_sig + nameof (RippleWallet) + " rw =" + DebugIhildaWallet.ToAssertString(rw));
			}
			#endif

			using (FromSecretDialog fsd = new FromSecretDialog ("Save new wallet", rw)) {
				while (true) {
					#if DEBUG
					if (DebugIhildaWallet.WalletManagerWidget) {
						Logging.WriteLog(method_sig + "while (true)");
					}
					#endif

					ResponseType resp = (ResponseType)fsd.Run ();

					if (resp == ResponseType.Ok) {
						#if DEBUG
						if (DebugIhildaWallet.WalletManagerWidget) {
							Logging.WriteLog(method_sig + "User selected Ok");
						}
						#endif
						//threadedWalletAdd(rw);
						InitiateWalletAddThread(rw);
						return;
					}

					if (resp == ResponseType.Cancel) {
						#if DEBUG
						if (DebugIhildaWallet.WalletManagerWidget) {
							Logging.WriteLog(method_sig + "User selected Cancel");
						}
						#endif
						return;
					}

					fsd.Dispose(); // needed !!
				}
			}
		}

		public void SetWalletManager (WalletManager wm) {
			this.walletManager = wm;

			//this.walletviewport1.setWallets(walletManager.wallets.Values);
			this.UpdateUI();
		}


		public static void NoWalletSelected ()
		{
			MessageDialog.ShowMessage("You must first select a wallet to perform this action");
		}

		public WalletManager walletManager = null;


		public static WalletManagerWidget currentInstance = null;

#if DEBUG
		private const string clsstr = nameof (WalletManagerWidget) + DebugRippleLibSharp.colon;
#endif
		public void BotButtonClicked () {
			
			RippleWallet rw = WalletManager.GetRippleWallet();

			if (rw == null) {
				Application.Invoke ((sender, e) => NoWalletSelected ());
			}

			bool shouldContinue = LeIceSense.DoTrialDialog ( rw, LicenseType.MARKETBOT );
			if (!shouldContinue) {
				return;
			}
			Application.Invoke(
				(sender, e) => {
					FilledRuleManagementWindow rulewin = new FilledRuleManagementWindow (rw);
					rulewin.Show ();
				}

			);
		}

		public static void ThreadedWalletAdd (object obj)
		{
			#if DEBUG
			String method_sig = clsstr + nameof (ThreadedWalletAdd) + DebugRippleLibSharp.left_parentheses + obj.ToString () + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.WalletManagerWidget) {
				Logging.WriteLog( method_sig + DebugRippleLibSharp.beginn );
			}
			#endif


			// this is running in NON gtk thread.


			#if DEBUG
			if (DebugIhildaWallet.WalletManagerWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
			#endif
			if (obj == null) {
				#if DEBUG
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog (method_sig + "object o == null, " + DebugRippleLibSharp.returning);
				}
				#endif
				return;
			}

			if (!(obj is RippleWallet rw)) {
#if DEBUG
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog (method_sig + "object is not an instance of RippleWallet, " + DebugRippleLibSharp.returning);
				}
#endif
				return;
			}

			if (WalletManagerWidget.currentInstance == null) {
				#if DEBUG
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog(method_sig + "WalletManagerWidget.currentInstance == null");
				}
				#endif
				return;
			}

			if (WalletManager.currentInstance == null) {
				#if DEBUG
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog(method_sig + "WalletManager.currentInstance == null");
				}
				#endif
				return;
			}


			if (WalletManager.currentInstance.AddWallet (rw)) {
				#if DEBUG
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog(method_sig + "wallet " + rw.WalletName == null ? "null" : rw.WalletName + " added successfully");
				}
				#endif

				if (currentInstance != null) {
					#if DEBUG
					if (DebugIhildaWallet.WalletManagerWidget) {
						Logging.WriteLog (method_sig + "currentInstance != null");
					}
					#endif

					Application.Invoke ((sender, e) => currentInstance.UpdateUI ());
				}

				else {
					#if DEBUG
					if (DebugIhildaWallet.WalletManagerWidget) {
						Logging.WriteLog(method_sig + "currentInstance == null"); // would be a bug
					}
					#endif
				}

			} else {

				#if DEBUG
				// todo error
				if (DebugIhildaWallet.WalletManagerWidget) {
					Logging.WriteLog(method_sig + "error adding wallet, continuing");
				}
				#endif

				// todo alert user to error

				// could be break but I like continue. if they want to brea they must do so from the random generator widget. 
				//goto RANDOM;
			}
		}


	}
}

