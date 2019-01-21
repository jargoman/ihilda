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
using RippleLibSharp.Commands.Subscriptions;
using System.Media;

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

			CurrentInterface.OnOpen += delegate {



				ConnectAttemptInfo conf =  CurrentInterface.GetConnectAttemptInfo();


				StringBuilder stringBuilder = new StringBuilder ();

				stringBuilder.Append ("Connected to successfully to server : ");
				stringBuilder.Append ( conf.ServerUrl ?? " " );
				stringBuilder.Append ("\n");

				Logging.WriteBoth (stringBuilder.ToString());


				Task.Run (delegate {
					LedgerTracker.TokenSource?.Cancel ();
					LedgerTracker.TokenSource = new CancellationTokenSource ();
					var token = LedgerTracker.TokenSource.Token;
					NetworkInterface networkInterface = NetworkController.CurrentInterface;
					Subscribe.LedgerSubscribe (networkInterface, token, null);
					Subscribe.ServerSubscribe (networkInterface, token, null);
				});

			};

			CurrentInterface.OnClose += delegate {
				
				ConnectAttemptInfo conf = CurrentInterface.GetConnectAttemptInfo ();

				StringBuilder stringBuilder = new StringBuilder ();

				stringBuilder.Append ("Disconnected from server : ");
				stringBuilder.Append (conf?.ServerUrl ?? "(null)");
				stringBuilder.Append ("\n");

				Logging.WriteBoth (stringBuilder.ToString ());
			};


			CurrentInterface.OnError += delegate {
				ConnectAttemptInfo conf = CurrentInterface.GetConnectAttemptInfo ();
				StringBuilder stringBuilder = new StringBuilder ();

				stringBuilder.Append ("Connection to server ");
				stringBuilder.Append (conf.ServerUrl ?? "(null)");
				stringBuilder.Append (" has encounted an error\n");

				Logging.WriteBoth(stringBuilder.ToString());

			};




			CurrentInterface.OnMessage += ( object sender, WebSocket4Net.MessageReceivedEventArgs e ) => {
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

		public static void PlayNoNetSound ()
		{

			Task.Run (delegate {

				SoundSettings settings = SoundSettings.LoadSoundSettings ();

				if (settings.HasOnNetWorkFail && settings.OnNetWorkFail != null) {


					SoundPlayer player = new SoundPlayer (settings.OnNetWorkFail);
					player.Load ();
					player.Play ();


				}


			});
		}


		public static NetworkInterface GetNetworkInterfaceGuiThread () {
			if (!Program.network) { return null; }
			if (CurrentInterface != null) {
				return CurrentInterface;
			}
			PlayNoNetSound ();
			DoNetworkingDialog ();

			return CurrentInterface;
		}

		public static NetworkInterface GetNetworkInterfaceNonGUIThread () {
			if (!Program.network) { return null; }
			if (CurrentInterface != null) {
				return CurrentInterface;
			}
			PlayNoNetSound ();
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


		public static Task<Response<RippleSubmitTxResult>> UiTxNetworkSubmit (RippleTransaction rt, NetworkInterface ni, CancellationToken token) => Task.Run (
				delegate {
					SpinWait sw;
					using (ManualResetEvent mre = new ManualResetEvent (true)) {
						mre.Reset ();
						sw = null;
						Gtk.Application.Invoke (
						    (object sender, EventArgs e) => {
							    sw = new SpinWait ();
							    sw.Show ();
							    //sw.ShowAll();
							    mre.Set ();
						    }

						);

						//mre.WaitOne ();
						WaitHandle.WaitAny (new [] { mre, token.WaitHandle });
					}

					Response<RippleSubmitTxResult> tsk = rt.Submit (ni, token);


					Gtk.Application.Invoke (
						(object sender, EventArgs e) => {
							if (sw != null) {
								sw.Hide ();
							}
						}
					);

					return tsk;
				}
			);

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
					return ni != null && ni.Connect();
				}
			);


		}

		#if DEBUG
		private const string clsstr = nameof (NetworkController) + DebugRippleLibSharp.colon;
		#endif
	}
}

