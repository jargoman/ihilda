using System;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using RippleLibSharp;
using RippleLibSharp.Transactions.TxTypes;
using RippleLibSharp.Network;
using RippleLibSharp.Transactions;

using RippleLibSharp.Result;
using RippleLibSharp.Util;

namespace IhildaWallet.Networking
{
	public static class NetworkController
	{
		public static NetworkInterface CurrentInterface {
			get {
				if (!Program.network) {
					return null;
				}
				return _CurrentInterface; 
			}
			set { _CurrentInterface = value; }
		}

		private static NetworkInterface _CurrentInterface = null;

		public static NetworkInterface InitNetworking (ConnectionSettings coninf)
		{
			if (!Program.network) {
				return null;
			}
			#region debug
			#if DEBUG
			String method_sig = clsstr + nameof (InitNetworking) + DebugRippleLibSharp.left_parentheses + nameof (coninf) + DebugRippleLibSharp.equals + DebugRippleLibSharp.ToAssertString (coninf) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.NetworkController) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
			#endif

			if (coninf == null) {
				#if DEBUG
				if (DebugIhildaWallet.NetworkController) {
					Logging.WriteLog (method_sig + "coning == null \n");
				}
				#endif
					
				return null;
			}

#if DEBUG
			if (DebugIhildaWallet.NetworkController) {
				Logging.WriteLog (method_sig + "coninf != null\n");
			}
			#endif
			#endregion
		
			CurrentInterface = new NetworkInterface (coninf);

			if (coninf.Reconnect) {
				//currentInterface.connect();
			}


			//WebSocket4Net.WebSocket ws = currentInterface.websocket;
			//WebSocket4Net.WebSocket ws = null;

			//stringBuilder.Append (coninf.);

			CurrentInterface.onOpen += delegate {



				ConnectAttemptInfo conf =  CurrentInterface.GetConnectAttemptInfo();


				StringBuilder stringBuilder = new StringBuilder ();

				stringBuilder.Append ("Connected to successfully to server : ");
				stringBuilder.Append ( conf.ServerUrl ?? " " );
				stringBuilder.Append ("\n");

				Logging.WriteBoth (stringBuilder.ToString());

			};

			CurrentInterface.onClose += delegate {
				
				ConnectAttemptInfo conf = CurrentInterface.GetConnectAttemptInfo ();

				StringBuilder stringBuilder = new StringBuilder ();

				stringBuilder.Append ("Disconnected from server : ");
				stringBuilder.Append (conf?.ServerUrl ?? "(null)");
				stringBuilder.Append ("\n");

				Logging.WriteBoth (stringBuilder.ToString ());
			};

			CurrentInterface.onError += delegate {
				ConnectAttemptInfo conf = CurrentInterface.GetConnectAttemptInfo ();
				StringBuilder stringBuilder = new StringBuilder ();

				stringBuilder.Append ("Connection to server ");
				stringBuilder.Append (conf.ServerUrl ?? "(null)");
				stringBuilder.Append (" has encounted an error\n");

				Logging.WriteBoth(stringBuilder.ToString());

			};




			CurrentInterface.onMessage += ( object sender, WebSocket4Net.MessageReceivedEventArgs e ) => {
				ConnectAttemptInfo conf = CurrentInterface?.GetConnectAttemptInfo();
				StringBuilder stringBuilder = new StringBuilder();

				stringBuilder.Append ("Message recieved from server : ");
				stringBuilder.Append (conf?.ServerUrl ?? "(null)");
				stringBuilder.Append ("\n");	

				stringBuilder.Append (e.Message);
				stringBuilder.Append ("\n");
				Logging.WriteBoth (stringBuilder.ToString());
			};

			/*
			#if DEBUG
			if (Debug.NetworkInterface) {
				Logging.writeLog(method_sig + "currentInstance != null, setting connection info");
			}
			#endif
			*/
			return CurrentInterface;
		}


		public static NetworkInterface GetNetworkInterfaceGuiThread () {
			if (!Program.network) { return null; }
			if (CurrentInterface != null) {
				return CurrentInterface;
			}

			DoNetworkingDialog ();

			return CurrentInterface;
		}

		public static NetworkInterface GetNetworkInterfaceNonGUIThread () {
			if (!Program.network) { return null; }
			if (CurrentInterface != null) {
				return CurrentInterface;
			}

			DoNetworkingDialogNonGUIThread ();

			return CurrentInterface;
		}

		public static bool DoNetworkingDialog () {
			if (!Program.network) {
				return false;
			}

			string title = "Network Error";
			string message = "You are not connected to the internet would you like to view network settings?";

			bool networking = AreYouSure.AskQuestion (title, message);

			if (networking) {
				return NetworkSettingsDialog.ShowDialog ();
			}

			return false;

		}

		public static bool DoNetworkingDialogNonGUIThread () {
			if (!Program.network) {
				return false;
			}

			string title = "Network Error";
			string message = "You are not connected to the internet would you like to view network settings?";

			bool networking = AreYouSure.AskQuestionNonGuiThread (title, message);

			if (networking) {
				//return NetworkSettingsDialog.showDialog ();
				return NetworkSettingsDialog.ShowDialogNonGuiThread ();
			}

			return false;
		}


		public static Task<Response< RippleSubmitTxResult>> UiTxNetworkSubmit (RippleTransaction rt, NetworkInterface ni) {
			return Task.Run (
				delegate {
					
					ManualResetEvent mre = new ManualResetEvent (true);
					mre.Reset ();

					SpinWait sw = null;

					Gtk.Application.Invoke (
						(object sender, EventArgs e) => {
							sw = new SpinWait ();
							sw.Show();
							//sw.ShowAll();
							mre.Set();
						}

					);

					mre.WaitOne ();
					Response<RippleSubmitTxResult> tsk = rt.Submit(ni);


					Gtk.Application.Invoke (
						(object sender, EventArgs e) => {
							if (sw != null) {
								sw.Hide();
							}
						}
					);

					return tsk;
				}
			);


		}

		public static Task<bool> AutoConnect () {


			return Task.Run (
				delegate {
					if (!Program.network) {
						return false;
					}
					
					ConnectionSettings conny = NetworkSettings.LoadConnectionInfo();

					if (conny == null || !conny.Reconnect) {
						return false;
					}
					NetworkInterface ni = NetworkController.InitNetworking (conny);
					if (ni == null) {
						return false;
					}
					return ni.Connect ();
				}
			);


		}

		#if DEBUG
		private const string clsstr = nameof (NetworkController) + DebugRippleLibSharp.colon;
		#endif
	}
}

