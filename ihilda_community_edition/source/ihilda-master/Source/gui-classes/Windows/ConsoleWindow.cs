using System;
using System.Threading;
using System.Threading.Tasks;
using RippleLibSharp.Util;
using Gtk;

namespace IhildaWallet
{
	public partial class ConsoleWindow : Window
	{
		public ConsoleWindow () : 
				base(WindowType.Toplevel)
		{

			Build ();

			if (console1 == null) {
				console1 = new Console ();
				console1.Show ();
				this.vbox2.Add (console1);
			}





			currentInstance = this;

			DeleteEvent += (sender, args) => currentInstance = null;
		}

		public void Reshow ()
		{
			Widget[] children = this.Children;

			foreach (Widget child in children) {
				//Logging.writeLog ( child.GetType().ToString() );
				child.Show();
				//child.ShowAll();
			}


			this.Show();
			//this.ShowAll();
			this.ShowNow();
		}

		public static ConsoleWindow GetConsoleWindow () {
			if ( ConsoleWindow.currentInstance == null ) {
				// Should never be called. 
				ConsoleWindow.currentInstance = new ConsoleWindow();
			}

			return currentInstance;
		}

		public static Task< ConsoleWindow >InitGUI () {
			return Task.Run (delegate {
				#if DEBUG 
				string method_sig = clsstr + nameof (InitGUI) + DebugRippleLibSharp.both_parentheses;
				#endif
				ConsoleWindow csw = null;
				ManualResetEvent ewh = new ManualResetEvent(true);
				ewh.Reset ();

				Application.Invoke ( delegate {
					try {
						#if DEBUG
						if (DebugIhildaWallet.ConsoleWindow) {
							Logging.WriteLog(method_sig + "Invoking ConsoleWindow creation thread : Thread priority = " + Thread.CurrentThread.Priority);
						}
						#endif

						csw = new ConsoleWindow();
						//csw.ShowAll();
						ewh.Set();

					}

					#pragma warning disable 0168
					catch ( Exception e ) {
					#pragma warning restore 0168

						#if DEBUG
						if (DebugIhildaWallet.ConsoleWindow) {
							Logging.ReportException(method_sig, e);
						}
						#endif
					}

					finally {
						ewh.Set();
					}

				});

				ewh.WaitOne();
				return csw;
			});

		}

		public static ConsoleWindow currentInstance = null;
		#if DEBUG
		private const string clsstr = nameof (ConsoleWindow) + DebugRippleLibSharp.colon;
		#endif
	}
}

