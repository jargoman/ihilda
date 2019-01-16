/*
 *	License : Le Ice Sense 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Codeplex.Data;
using RippleLibSharp.Network;
using IhildaWallet;
using IhildaWallet.Networking;
using RippleLibSharp.Util;


namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class NetworkSettings : Gtk.Bin
	{

		static NetworkSettings ()
		{
			settingsPath = FileHelper.GetSettingsPath (settingsFileName);
		}

		//public ConnectedDisplayWidget ConnectedDisplayWidget;
		public NetworkSettings ()
		{



#if DEBUG
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog (clsstr + nameof (NetworkSettings) + DebugRippleLibSharp.both_parentheses);
			}
#endif



			Build ();

			if (serverinfowidget1 == null) {
				serverinfowidget1 = new ServerInfoWidget ();
				serverinfowidget1.Show ();

				scrolledwindow1.AddWithViewport (serverinfowidget1);
			}

			if (connecteddisplaywidget1 == null) {
				connecteddisplaywidget1 = new ConnectedDisplayWidget ();
				connecteddisplaywidget1.Show ();

				hbox8.Add (connecteddisplaywidget1);
			}

#if DEBUG
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog (clsstr + "build complete\n");
			}
#endif




			this.button43.Clicked += OnDefaultSettingsButtonClicked;

			this.testnetbutton1.Clicked += Testnetbutton1_Clicked;

			this.LoadSettings ();


			this.disconnectbutton.Clicked += OnDisconnectbuttonClicked;



		}

		public ServerInfoWidget GetServerInfoWidget ()
		{
			return this.serverinfowidget1;
		}

		void Testnetbutton1_Clicked (object sender, EventArgs e)
		{
#if DEBUG
			string method_sig = clsstr + "Testnetbutton1_Clicked" + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			bool ya = AreYouSure.AskQuestion ("Revert to test settings", "Are you sure you want to revert to test settings?");

			if (!ya) {
				return;

			}

			ConnectionSettings coni = null;
			try {
				string jsn = FileHelper.LoadResourceText (nameof (IhildaWallet) + ".Files.testnetsettings.jsn");
				coni = DynamicJson.Parse (jsn);



			}

#pragma warning disable 0168
			catch (Exception exp) {
#pragma warning restore 0168

#if DEBUG
				if (DebugIhildaWallet.NetworkSettings) {
					Logging.ReportException (method_sig, exp); // exp NOT e
					//Logging.writeLog(method_sig + Debug.exceptionMessage);
					//Logging.writeLog(exp.StackTrace);

				}
#endif

				coni = new ConnectionSettings {
					ServerUrls = new string [] { TEST_URL },
					LocalUrl = TEST_LOCAL,
					UserAgent = TEST_USER_AGENT
				};
			}

			string [] sa = coni.ServerUrls;
			string l = coni.LocalUrl;
			string u = coni.UserAgent;

			Gtk.Application.Invoke (delegate {
				if (sa != null) {
					servertextview.Buffer.Clear ();
					foreach (string s in sa) {
						servertextview.Buffer.Text += s;
						servertextview.Buffer.Text += " \n";
					}

				}
				if (l != null) localentry.Text = l;
				if (u != null) agententry.Text = u;
			});
		}



		~NetworkSettings ()
		{

#if DEBUG
			//if (Debug.NetworkSettings) {
			Logging.WriteLog (clsstr + "~destructor begin");
			//}

#endif

			//NetworkInterface ni = NetworkController.currentInterface;

			//if (onOpenEvent != null && ni.onOpen != null) ni.onOpen -= onOpenEvent;
			//if (onCloseEvent != null && ni.onClose != null) ni.onClose -= onCloseEvent;
			//if (onErrorEvent != null && ni.onError != null) ni.onError -= onErrorEvent;
		}

		public void SetNetworkDevice (NetworkInterface ni)
		{
#if DEBUG
			string method_sig = clsstr + nameof (SetNetworkDevice) + DebugRippleLibSharp.left_parentheses + nameof (NetworkInterface) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}

#endif
			this.onOpenEvent += (object sender, EventArgs e) => {
#if DEBUG
				if (DebugIhildaWallet.NetworkSettings) {
					Logging.WriteLog (method_sig + DebugRippleLibSharp.colon + nameof (this.onOpenEvent) + DebugRippleLibSharp.colon + DebugRippleLibSharp.beginn);
				}
#endif


				this.connecteddisplaywidget1.SetConnected (ni.GetConnectAttemptInfo().ServerUrl);

				if (WalletManagerWidget.currentInstance != null) {

					WalletManagerWidget.currentInstance.SetConnected (ni.GetConnectAttemptInfo ().ServerUrl);
				}
				//this.saveSettings ();  // It was a good intention but introduced a serious bug that drove me up the wall where the network settings were being saved due to multithreading environment


			};

			ni.OnMessage += onMessageEvent;


			ni.OnOpen += onOpenEvent;


			ni.OnClose += (object sender, EventArgs e) => {
				// 
#if DEBUG
				if (DebugIhildaWallet.NetworkSettings) {
					Logging.WriteLog (clsstr + DebugRippleLibSharp.colon + nameof(onCloseEvent) + DebugRippleLibSharp.colon + DebugRippleLibSharp.beginn);
				}
#endif

				this.connecteddisplaywidget1.SetDisConnected ();

				this.serverinfowidget1.SetServerInfo (null);

				if (WalletManagerWidget.currentInstance != null) {
					WalletManagerWidget.currentInstance.SetConnected (null);
				}
			};

			//ni.onClose += this.onCloseEvent;



			this.onErrorEvent += (SuperSocket.ClientEngine.ErrorEventArgs e) => {
				if (
					e.Exception.Message.Equals ("RemoteCertificateChainErrors")
					|| e.Exception.Message.Equals ("RemoteCertificateNotAvailable")) {

					//if (e.Exception.Message.Equals ("RemoteCertificateNotAvailable")) {

					String message = "\nUnable to establish an ssl connection, you need to install the servers ssl security certificate\n" +
					    "Example # certmgr --ssl " +
						 ///* url + 
						"  ( using the command line of your local operating system terminal/cmd.exe ect )\n\n" +
					    "if that doesn't work try importing mozilla's certificate store" +
					    "Example# mozroots --import --sync \n";

					//MainWindow.currentInstance

#if DEBUG
					if (DebugRippleLibSharp.NetworkInterface) {
						Logging.WriteLog (message);
						//Logging.WriteLog ("should print" + e.Exception.Message);
						Logging.WriteLog ("should print" + e.Exception.Message);
					}
#endif


					MessageDialog.ShowMessage (message);
					//return;
				}



#if DEBUG
				if (DebugIhildaWallet.NetworkSettings) {
					Logging.WriteLog (method_sig + DebugRippleLibSharp.colon + nameof (this.onErrorEvent) + DebugRippleLibSharp.colon + DebugRippleLibSharp.beginn);
				}
#endif

				this.connecteddisplaywidget1.SetDisConnected ();
				this.serverinfowidget1.SetServerInfo (null);
			};


			this.serverinfowidget1.SetServer (ni);


		}



		public void SetUIConnectStatus (string serverUrl)
		{
			if (this.connecteddisplaywidget1 == null) {
				// TODO debug

				return;
			}

			if (serverUrl != null) {
				this.connecteddisplaywidget1.SetConnected (serverUrl);
			} else {
				this.connecteddisplaywidget1.SetDisConnected ();
			}

		}

		NetworkInterface.connectEventHandler onOpenEvent;

#pragma warning disable 0649
		private NetworkInterface.connectEventHandler onCloseEvent;
		private NetworkInterface.OnMessageEventHandler onMessageEvent;
#pragma warning restore 0649
		NetworkInterface.errorEventHandler onErrorEvent;



		/*
		public void websocket_Error (object sender, System.IO.ErrorEventArgs e) {
			// TODO
			//this.connecteddisplaywidget1.setDisConnected ();
		}
		*/

		public static ConnectionSettings LoadConnectionInfo ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (LoadConnectionInfo) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			if (!Program.network) {
				return null;
			}

			string jsn = ReadSettingsFile ();

#if DEBUG
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog(method_sig + " jsn = " + DebugRippleLibSharp.ToAssertString(jsn));
			}

#endif

			ConnectionSettings con = null;
			try {
#if DEBUG
				if (DebugIhildaWallet.NetworkSettings) {
					Logging.WriteLog(method_sig + " parsing json");
				}
#endif

				con = DynamicJson.Parse (jsn);

#if DEBUG
				if (DebugIhildaWallet.NetworkSettings) {
					Logging.WriteLog(method_sig + "con.ServerUrls", con?.ServerUrls );
				}
#endif
			}
#pragma warning disable 0168
			catch (Exception e) {
#pragma warning disable 0168

#if DEBUG
				if (DebugIhildaWallet.NetworkSettings) {
					string message = e?.Message ?? "error message is null \n";
					Logging.WriteLog(method_sig + DebugRippleLibSharp.exceptionMessage +  message);
				}
#endif
				return null;
			}

#if DEBUG
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog (method_sig + "first catch cleared");
			}
#endif

			return con;

		}


		public void LoadSettings ()
		{

#if DEBUG
			String method_sig = clsstr + nameof (LoadSettings) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			/*
#if DEBUG
			if (Debug.NetworkSettings) {
				Logging.writeLog(method_sig + "network interface is non null\n");
			}
#endif
			*/

			/*
			var con = NetworkInterface.connectionInfo;
			if (con == null) {
#if DEBUG
				if (Debug.NetworkSettings) {
					Logging.writeLog(method_sig + "connection info is null, returning\n");
				}
#endif
				return;
			}
			*/

			ConnectionSettings con = LoadConnectionInfo ();



			if (con == null) {
#if DEBUG
				if (DebugIhildaWallet.NetworkSettings) {
					Logging.WriteLog (method_sig + "con == null");
				}
#endif

				return;
			}

			if (con.ServerUrls == null) {
#if DEBUG
				if (DebugIhildaWallet.NetworkSettings) {
					Logging.WriteLog(method_sig + " con.ServerUrls == null");
				}
#endif
			}


			String [] serv = con.ServerUrls;


#if DEBUG
			if (serv == null) {
				Logging.WriteLog(method_sig + "serv == null");
			}
#endif


			Gtk.Application.Invoke (delegate {
#if DEBUG
				if (DebugIhildaWallet.NetworkSettings) {
					Logging.WriteLog(method_sig + DebugIhildaWallet.gtkInvoke);
				}



#endif

				//if (con.server != null && this.serverentry != null) this.serverentry.Text = con.server;
				//if (con.ServerUrl != null && this.servertextview != null && this.servertextview.Buffer != null) {
#if DEBUG
					if (DebugIhildaWallet.NetworkSettings) {
					Logging.WriteLog(method_sig + "found server information in connection info\n");
					}
#endif


				//String servs = con.server//

				String [] servers = con.ServerUrls;//servs.Split(" ", "\r", "\n", "\t");

				StringBuilder sb = new StringBuilder ();

				foreach (String s in servers) {
					sb.AppendLine (s);
				}

				this.servertextview.Buffer.Text = sb.ToString ();

				//}


				if (con.LocalUrl != null && this.localentry != null) this.localentry.Text = con.LocalUrl;
				if (con.UserAgent != null && this.agententry != null) this.agententry.Text = con.UserAgent;
				if (this.autoconnectbutton != null) this.autoconnectbutton.Active = con.Reconnect;
			});


		}




		/*///// !!!VARIABLES!!! ///////*/

		//static string DEFAULT_SERVER = "wss://s-west.ripple.com";

		//public static NetworkSettings currentInstance = null;
		private static String settingsFileName = "networkSettings.jsn";
		private static String settingsPath = "";

#if DEBUG
		const string clsstr = nameof (NetworkSettings) + DebugRippleLibSharp.colon;
#endif




		public static string ReadSettingsFile ()
		{
#if DEBUG
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog(clsstr + nameof (ReadSettingsFile) + DebugRippleLibSharp.colon + DebugRippleLibSharp.beginn);
			}
#endif

			String json = null;

			try {

				if (!File.Exists (settingsPath)) {
					Logging.WriteBoth ("No network settings found\n");
					return json;
				}

				String foundmessage = "Found network settings file at " + settingsPath + "\n";
				Logging.WriteBoth(foundmessage);

				if (!(new FileInfo(settingsPath).Length > 0)) {
					Logging.WriteBoth ("Warning : Network settings file at " + settingsPath + " is empty !!");
					return json;
				}

				json = File.ReadAllText(settingsPath);
				if (json==null) {
					Logging.WriteBoth ("Unknown error, reading network settings file\n" );
				}

				return json;


			}
			catch (Exception e) {
#if DEBUG
				if (DebugIhildaWallet.NetworkSettings) {
					Logging.WriteLog ( 
						"NetworkSettings : method loadSettings : Exception thrown reading " 
						+ settingsPath 
						+ "\n" 
						+ e.Message
					);
				}
#endif
				return null;

			}
		}

		public void SaveSettings ()
		{
			
#if DEBUG
			String method_sig = null;

			if (DebugIhildaWallet.NetworkSettings) {
				method_sig = clsstr + nameof (SaveSettings) + DebugRippleLibSharp.both_parentheses;
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif


			//EventWaitHandle ew = new ManualResetEvent (true);
			//ew.Reset ();
			String str = null;
			//Gtk.Application.Invoke (
			//	delegate {

					str = this.servertextview.Buffer.Text;
			//		ew.Set();
			//	});
			//ew.WaitOne ();

#if DEBUG
			if (DebugIhildaWallet.NetworkSettings) {
				//Logging.write (method_sig + "delegate invoke\n");
				Logging.WriteLog (method_sig + "serverEntry = " + str);

			}
#endif


			String[] servs = null;

			str = str.Trim ();

			if (str == null) {
#if DEBUG
				if (DebugIhildaWallet.NetworkSettings) {
					Logging.WriteLog (method_sig + "str == null : setting to \"\" (blank)");
				}
#endif
				str = "";
			}



			if (str.Equals ("")) {
				/*servs = new string[0];
				 * automatically saving a blank server list turned out to be a horrible bug in
				 * a multi threaded environent. The solution is to only save the network 
				 * settings if they are not empty
				 */
#if DEBUG
				if (DebugIhildaWallet.NetworkSettings) {
					Logging.WriteLog (method_sig + "server string is blank returning without saving\n");
				}
#endif

				return;
			}
			servs = str.Split (' ', '\r', '\n', '\t', ',', '|');
			if (servs == null || servs.Length < 1) {
#if DEBUG
				Logging.WriteLog (method_sig + "no valid servers, returning");
#endif
				return;
			}

			List<String> lis = new List<string> ();
#if DEBUG
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog (method_sig + "adding servers : " + DebugIhildaWallet.ToAssertString (servs) + "\n");
			}
#endif

			foreach (String s in servs) {
#if DEBUG
				if (DebugIhildaWallet.NetworkSettings) {
					Logging.WriteLog ("adding " + DebugIhildaWallet.ToAssertString (s) + "\n");
				}
#endif
				lis.Add (s);
			}

			servs = lis.ToArray ();


#if DEBUG
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog (method_sig + "servertextview text = " + str);
				Logging.WriteLog (method_sig + "servs = ", servs);
			}
#endif

			if (servs == null || !(servs.Length > 0)) {
#if DEBUG
				if (DebugIhildaWallet.NetworkSettings) {
					Logging.WriteLog (method_sig + "no servers found in list\n");
				}
#endif
				return;
			}
			EventWaitHandle ewh = new ManualResetEvent (true);
			ewh.Reset ();

			Object ob = null;
			//Gtk.Application.Invoke (delegate {



			ob = new {
				ServerUrls = servs,   //this.serverentry.Text,
				LocalUrl = this.localentry.Text,

				UserAgent = this.agententry.Text,

				reconnect = this.autoconnectbutton.Active,

			};

			//ewh.Set();
			//});
			//ewh.WaitOne ();




			String json = DynamicJson.Serialize (ob);

#if DEBUG
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog (method_sig + "json = " + json + "\n");
			}
#endif

			//File.WriteAllText(settingsPath, json);
			bool res  = FileHelper.SaveConfig(settingsPath, json);

			if (!res) {
				// todo debug // 
#if DEBUG
				if (DebugIhildaWallet.NetworkSettings) {
					Logging.WriteLog (method_sig + "failed to save ");
				}
#endif
			}
		//	}
		//);

		}

		protected void Connect () {


			
#if DEBUG
			string method_sig = clsstr + nameof (Connect) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			//bool autoconnect = autoconnectbutton.Active;
			//if (autoconnectbutton == null) {
				// TODO debug
			//	Logging.write ("NetworkSettings : method connect : Critical Error : autoconnectbutton = null");

			//	return;
			//}
			ConnectionSettings conny = GetConnectionInfoFromGUI();
			NetworkInterface ni = NetworkController.InitNetworking (conny);
			this.SetNetworkDevice (ni);

			ni.Connect ();

			
		}

		public ConnectionSettings GetConnectionInfoFromGUI () {
#if DEBUG
			string method_sig = clsstr + ": getConnectionInfoFromGUI () : ";
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif


			ManualResetEvent ev = new ManualResetEvent(false);
			ConnectionSettings conny = new ConnectionSettings();



			// if we call this from the gtk thread things are much easier
			Gtk.Application.Invoke( delegate {  

#if DEBUG
				if (DebugIhildaWallet.NetworkSettings) {
					Logging.WriteLog (method_sig + "gtk invoke : " + DebugRippleLibSharp.beginn);
				}
#endif


				String str = servertextview.Buffer.Text;
				String[] srv = str.Split(' ', '\r', '\n', '\t', ',');

				if ( srv == null || srv.Length == 0 ) {
#if DEBUG
					if (DebugIhildaWallet.NetworkSettings) {
						Logging.WriteLog (method_sig + "no servers specified : returning \n");
					}
#endif
					//.showMessage ("");
					return;
				}

				conny.ServerUrls = srv;
				conny.LocalUrl = localentry.Text;
				conny.UserAgent = agententry.Text;
				if (this.autoconnectbutton != null) conny.Reconnect = this.autoconnectbutton.Active;
				ev.Set();

			});


#if DEBUG
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog(method_sig + "Connection info =" + DebugIhildaWallet.ToAssertString(conny) + "\n");
			}
#endif
			ev.WaitOne();

			return conny;
			// TODO validate url, yuck :/

		}
			
		protected void OnConnectbuttonClicked (object sender, EventArgs e)
		{
			
#if DEBUG
			string method_sig = clsstr + nameof (OnConnectbuttonClicked) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			if (!Program.network) {
				MessageDialog.ShowMessage ("Network Disabled", "Networking has been disabled at the command line with parameter network=false\nTo enable networking restart " + Program.appname + " with parameter option network=true");
				return;
			}

			string servers = servertextview.Buffer.Text;
			if (servers == null || servers.Trim().Equals("")) {
				MessageDialog.ShowMessage ("You must specify a server to connect to\n");
				return;
			}


			Task.Run ((Action)Connect);

		}

		protected void OnDisconnectbuttonClicked (object sender, EventArgs e)
		{
#if DEBUG
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog ("NetworkSettings : method OnDisconnectbuttonClicked : begin\n");
			}
#endif
			NetworkInterface ni = NetworkController.CurrentInterface;
			if (ni != null) {
				
				ni.Disconnect ();
			} else {
				MessageDialog.ShowMessage ("You haven't even connected to a server yet.");
			}
		}

		protected void OnAutoconnectbuttonToggled (object sender, EventArgs e)
		{
#if DEBUG
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog ("NetworkSettings : method OnAutoconnectbuttonToggled : begin\n");
			}
#endif
			NetworkInterface ni = NetworkController.CurrentInterface;

			if (ni !=null && ni.ConnectionInfo != null) {

				ni.ConnectionInfo.Reconnect = this.autoconnectbutton.Active;
			}


		}

		protected void OnServerentryActivated (object sender, EventArgs e)
		{
#if DEBUG
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog ("NetworkSettings : method OnServerentryActivated : begin\n");
			}
#endif

			if (localentry == null) {

				Logging.WriteLog ("NetworkSettings : method OnServerentryActivated : Critical Error : localentry = null\n");
				return;
			}

			this.localentry.GrabFocus ();
		}

		protected void OnLocalentryActivated (object sender, EventArgs e)
		{
#if DEBUG
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog ("NetworkSettings : method OnLocalentryActivated : begin\n");
			}
#endif
			if (agententry == null) {
#if DEBUG
				if (DebugIhildaWallet.NetworkSettings) {
					Logging.WriteLog ("NetworkSettings : method OnLocalentryActivated : Critical Error : agententry = null\n");
				}
#endif
				return;

			}

			this.agententry.GrabFocus ();
		}

		protected void OnAgententryActivated (object sender, EventArgs e)
		{
#if DEBUG
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog ("NetworkSettings : method OnAgententryActivated : begin\n");
			}
#endif

			if (connectbutton==null) {
				// TODO debhug
#if DEBUG
				if (DebugIhildaWallet.NetworkSettings) {
					Logging.WriteLog ("NetworkSettings : method OnAgententryActivated : Critical Error : connectbutton = null\n");
				}
#endif
				return;
			}

			this.connectbutton.GrabFocus ();
		}

		protected void OnDefaultSettingsButtonClicked (object sender, EventArgs e) {
			
#if DEBUG
			string method_sig = clsstr + nameof (OnDefaultSettingsButtonClicked) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);

			}
#endif

			bool ya = AreYouSure.AskQuestion("Revert to default", "Are you sure you want to revert to default settings?");


			ConnectionSettings coni = null;
			if (ya) {
				
				try {
					string jsn = FileHelper.LoadResourceText(nameof (IhildaWallet) + ".Files.networkSettingsDefault.jsn");
					coni = DynamicJson.Parse(jsn);



				}

				catch (Exception exp ) {
#if DEBUG
					if (DebugIhildaWallet.NetworkSettings) {
						Logging.WriteLog(method_sig + DebugRippleLibSharp.exceptionMessage);
						Logging.WriteLog(exp.StackTrace);
					}
#endif
					coni = new ConnectionSettings {
						ServerUrls = new string [] { DEFAULT_URL },
						LocalUrl = DEFAULT_LOCAL,
						UserAgent = DEFAULT_USER_AGENT
					};
				}

				string[] sa = coni.ServerUrls;
				string l = coni.LocalUrl;
				string u = coni.UserAgent;

				Gtk.Application.Invoke ( delegate {
					if (sa != null) {
						servertextview.Buffer.Clear();
						foreach (string s in sa) {
							servertextview.Buffer.Text += s;
							servertextview.Buffer.Text += " \n";
						}

					}
					if (l != null) localentry.Text = l;
					if (u != null) agententry.Text = u;
				});
			}

		}





		const string DEFAULT_URL = "wss://s1.ripple.com";
		const string DEFAULT_LOCAL = "localhost";
		const string DEFAULT_USER_AGENT = "ice captain";

		const string TEST_URL = "wss://s.altnet.rippletest.net:51233";
		const string TEST_LOCAL = DEFAULT_LOCAL;
		const string TEST_USER_AGENT = DEFAULT_USER_AGENT;

	}
}

