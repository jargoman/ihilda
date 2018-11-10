using System;
using System.Threading;
using Gtk;

using IhildaWallet.Networking;
using RippleLibSharp.Network;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public partial class NetworkSettingsDialog : Gtk.Dialog
	{
		static NetworkSettingsDialog ()
		{
			Instance = null;
		}

		public NetworkSettingsDialog ()
		{

#if DEBUG
			string method_sig = clsstr + nameof (NetworkSettingsDialog) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.NetworkSettingsDialog) {
				Logging.WriteLog ( method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			// don't miss the fact that below calls NetworkSettings Constructor 
			this.Build ();

			if (networksettings1 == null) {
				networksettings1 = new NetworkSettings ();
				networksettings1.Show ();
				vbox2.Add (networksettings1);
			}

			this.Modal = true;


		}



		public void Save ()
		{

#if DEBUG
			string method_sig = clsstr + nameof (Save) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.NetworkSettingsDialog) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif


			this.networksettings1.SaveSettings ();
		}

		public void UpdateNetUI ()
		{
			string isConnected = DetermineConnectStatus ();

			this.networksettings1.SetUIConnectStatus (isConnected);

			if (isConnected != null) {

			}

		}

		private string DetermineConnectStatus ()
		{
#if DEBUG
			string method_sig = clsstr + "determineConnectStatus () : ";
#endif

			if (NetworkController.CurrentInterface == null) {
				return null;
			}

			try {
				if (NetworkController.CurrentInterface.IsConnected ()) {
					return NetworkController.CurrentInterface.GetConnectAttemptInfo().ServerUrl;
				}
			}


#pragma warning disable 0168
			catch (Exception e) {
#pragma warning restore 0168

#if DEBUG
				if (DebugIhildaWallet.NetworkSettingsDialog) {
					Logging.ReportException (method_sig, e);
				}
#endif
				return null;
			}

			return null;
		}

		private static NetworkSettingsDialog Instance {
			get;
			set;
		}

		private static NetworkSettingsDialog RetrieveDialog ()
		{

#if DEBUG
			string method_sig = clsstr + nameof (RetrieveDialog) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.NetworkSettingsDialog) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			//if (instnce == null) {
			Instance = new NetworkSettingsDialog ();
			//}
			Instance.UpdateNetUI ();

			string connected = Instance.DetermineConnectStatus ();
			if (connected != null) {
				ServerInfoWidget siw = Instance.networksettings1.GetServerInfoWidget ();

				siw.Refresh ();
			}
			return Instance;

		}

		public static bool ShowDialog ()
		{
#if DEBUG
			String method_sig = clsstr + nameof (ShowDialog) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.NetworkSettingsDialog) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			{
				NetworkSettingsDialog nsd = RetrieveDialog ();
#if DEBUG
				if (DebugIhildaWallet.NetworkSettingsDialog) {
					Logging.WriteLog (method_sig + "using NetworkSettingsDialog\n");
				}
#endif

				ResponseType resp = (ResponseType)nsd.Run ();

#if DEBUG
				if (DebugIhildaWallet.NetworkSettingsDialog) {
					Logging.WriteLog (method_sig + "responded\n");
				}
#endif


				if (resp == ResponseType.Ok) {
#if DEBUG
					if (DebugIhildaWallet.NetworkSettingsDialog) {
						Logging.WriteLog (method_sig + "ok clicked\n");
					}
#endif
					nsd.Save ();
				}

				nsd.Destroy ();
			}

#if DEBUG
			if (DebugIhildaWallet.NetworkSettingsDialog) {
				Logging.WriteLog (method_sig + "closed using\n");
			}
#endif

			return false;
		}



		public static bool ShowDialogNonGuiThread ()
		{
#if DEBUG
			String method_sig = clsstr + nameof (ShowDialogNonGuiThread) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.NetworkSettingsDialog) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			ManualResetEvent ev = new ManualResetEvent (false);

			Application.Invoke (delegate {
				NetworkSettingsDialog nsd = RetrieveDialog ();
#if DEBUG
				if (DebugIhildaWallet.NetworkSettingsDialog) {
					Logging.WriteLog (method_sig + "using NetworkSettingsDialog\n");
				}
#endif

				ResponseType resp = (ResponseType)nsd.Run ();

#if DEBUG
				if (DebugIhildaWallet.NetworkSettingsDialog) {
					Logging.WriteLog (method_sig + "responded\n");
				}
#endif


				if (resp == ResponseType.Ok) {
#if DEBUG
					if (DebugIhildaWallet.NetworkSettingsDialog) {
						Logging.WriteLog (method_sig + "ok clicked\n");
					}
#endif
					nsd.Save ();
				}

				nsd.Destroy ();


#if DEBUG
				if (DebugIhildaWallet.NetworkSettingsDialog) {
					Logging.WriteLog (method_sig + "closed using\n");
				}
#endif

				ev.Set ();

			}); // end gtk invoke

			ev.WaitOne ();

			// TODO what to return? isconnected netinterface ect...
			return false;
		}

#if DEBUG
		private const string clsstr = nameof (NetworkSettingsDialog) + DebugRippleLibSharp.colon;
#endif
	}
}

