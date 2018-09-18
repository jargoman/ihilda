using System;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using IhildaWallet.Util;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public partial class WalletManagerWindow : Gtk.Window
	{
		public WalletManagerWindow () : 
				base(Gtk.WindowType.Toplevel)
		{
			
			#if DEBUG
			string method_sig = clsstr + nameof (WalletManagerWidget) + DebugRippleLibSharp.both_parentheses; 
			if (DebugIhildaWallet.WalletManagerWindow) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}
			#endif
			this.Hide();

			this.Build ();

			if (this.walletmanagerwidget1 == null) {
				this.walletmanagerwidget1 = new WalletManagerWidget ();
				this.walletmanagerwidget1.Show ();
				vbox2.Add ( this.walletmanagerwidget1 );
			}


			this.Hide ();

			this.Visible = false;
			#if DEBUG
			if (DebugIhildaWallet.WalletManagerWindow) {
				Logging.WriteLog(method_sig + DebugIhildaWallet.buildComp);
			}
			#endif

			//Gdk.Color col = new Gdk.Color(); 
			//Gdk.Color.Parse("red", ref col);

			//this.image15.ModifyBg(StateType.Normal, col);
			//this.vbox2.ModifyBg(StateType.Normal, col);
			//this.OnDeleteEvent += new EventHandler(OnDeleteEvent);

			//Gdk.Color col = new Gdk.Color(3, 3, 56);
			//this.eventbox1.ModifyBg(StateType.Normal, col);
			//(3, 3, 56);
			currentInstance = this;



		}

		public static WalletManagerWindow currentInstance;

		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
		

			//AreYouSure aus = new AreYouSure ("Are you sure you want to close Wallet Manager?");


		//if (aus.Run () == (int)ResponseType.Ok) {
		//	aus.Destroy ();
		//	Application.Quit ();
		//	a.RetVal = true;
		//} else {
		//	aus.Destroy ();
		//	a.RetVal = false;
		//	Application.Run ();
		//}


			// instead we'll do...
			Program.QuitRequest(sender,a);

		}



		public static WalletManagerWindow ShowCurrent () {

			if (WalletManagerWindow.currentInstance != null) {
				Gtk.Application.Invoke ((sender, e) => WalletManagerWindow.currentInstance.Show ());

			}

			return currentInstance;
		}



		public static WalletManagerWindow HideCurrent () {
			#if DEBUG
			string method_sig = clsstr + nameof (HideCurrent) + DebugRippleLibSharp.both_parentheses;
			#endif 


			if (WalletManagerWindow.currentInstance!=null) {
				#if DEBUG
				if (DebugIhildaWallet.WalletManagerWindow) {
					Logging.WriteLog(method_sig + "hiding wallet manager widget");
				}
				#endif
				WalletManagerWindow.currentInstance.Hide();
			}
			return currentInstance;
		}



		public static Task< WalletManagerWindow > InitGUI () {
			return Task.Run ( delegate {
				#if DEBUG
				string method_sig = clsstr + nameof (InitGUI) + DebugRippleLibSharp.both_parentheses;
				#endif
				WalletManagerWindow wmw = null;
				EventWaitHandle wh = new ManualResetEvent(false);
				wh.Reset();
				Gtk.Application.Invoke ( delegate {
					try {
						#if DEBUG
						if (DebugIhildaWallet.WalletManagerWindow) {
							Logging.WriteLog ( method_sig + "Invoking "+ nameof (WalletManagerWindow) + " creation thread : Thread priority = " + Thread.CurrentThread.Priority);
						}
						#endif
						wmw = new WalletManagerWindow ();
						//wmw.ShowAll();
						#if DEBUG
						if (DebugIhildaWallet.WalletManagerWindow) {
							Logging.WriteLog(method_sig + "t9 complete");
						}
						#endif
						wh.Set();	
					}

					#pragma warning disable 0168
					catch (Exception e) {
					#pragma warning restore 0168

						#if DEBUG
						if (DebugIhildaWallet.WalletManagerWindow) {
							Logging.ReportException(method_sig, e);
						}
						#endif
					}

					finally  {
						wh.Set();
					}

				});
				wh.WaitOne();
				return wmw;

			});
		}

		#if DEBUG
		private const string clsstr = nameof (WalletManagerWindow) + DebugRippleLibSharp.colon;
		#endif
	}
}

