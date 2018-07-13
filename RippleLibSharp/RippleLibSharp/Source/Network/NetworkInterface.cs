/*
 *	License : Le Ice Sense 
 */

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using SuperSocket.ClientEngine;
//using WebSocket4Net;

//using Codeplex.Data;  // try not to use this here to properly separate code 
using RippleLibSharp.Util;


namespace RippleLibSharp.Network
{
	public class NetworkInterface
	{
		#region constructors
		static NetworkInterface ()
		{
#if DEBUG
			string method_sig = clsstr + "static " + nameof (NetworkInterface) + DebugRippleLibSharp.both_parentheses;
			if (DebugRippleLibSharp.NetworkInterface) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

		}

		public NetworkInterface (ConnectionSettings connectionInfo)
		{
#if DEBUG
			string method_sig = clsstr
				+ nameof (NetworkInterface)
				+ DebugRippleLibSharp.left_parentheses
					+ nameof (connectionInfo)
									 //+ Debug.toAssertString(connection
									 + DebugRippleLibSharp.right_parentheses;

			if (DebugRippleLibSharp.NetworkInterface) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			SetConnnectInfo (connectionInfo);


		}

		#endregion

		public void InitNetworkTasking ()
		{

			NetworkRequestTask.InitNetworkTasking (this);
		}

		private void SetConnnectInfo (ConnectionSettings cone)
		{
			/*
			#region debug
			#if DEBUG
			String method_sig = clsstr + "setConnectionInfo ( cone = " +
				Debug.toAssertString(cone) +
					" ) : ";

			if (Debug.NetworkInterface) {
				Logging.writeLog(method_sig + Debug.begin);
			}
			#endif

				
			if (cone == connectionInfo) {
				#if DEBUG
				if (Debug.NetworkInterface) {
					Logging.writeLog(method_sig + "cone == this.connnectionInfo, nothing to do");
				}
				#endif
				return;
			}
			#endregion
			*/

			if (cone == null) {
				// todo // set to blank?
#if DEBUG
				if (DebugRippleLibSharp.NetworkInterface) {
					//	Logging.writeLog(method_sig + "cone == null, returning");
				}
#endif
				return;
			}

			if (cone.ServerUrls == null) {
#if DEBUG
				if (DebugRippleLibSharp.NetworkInterface) {
					//	Logging.writeLog(method_sig + "cone.servers == null");
				}
#endif

				cone.ServerUrls = new string [] { }; // wow I finally understand zero length arrays lol

			}

			if (cone.LocalUrl == null) {
#if DEBUG
				if (DebugRippleLibSharp.NetworkInterface) {
					//	Logging.writeLog(method_sig + "cone.local == null : setting to \"\" (blank)");
				}
#endif
				cone.LocalUrl = "";
			}

			if (cone.UserAgent == null) {
#if DEBUG
				if (DebugRippleLibSharp.NetworkInterface) {
					//	Logging.writeLog(method_sig + "cone.userAgent == null : setting to \"\" (blank)");
				}
#endif
				cone.UserAgent = "";
			}

			ConnectionInfo = cone;
#if DEBUG
			if (DebugRippleLibSharp.NetworkInterface) {
				//Logging.writeLog (method_sig + "NetworkInterface.connectionInfo = " + Debug.toAssertString(this.connectionInfo));
			}
#endif



		}

		private Tuple<WebSocket4Net.WebSocket, ConnectAttemptInfo> ConnectionTuple {
			get;
			set;
		}

		public static bool PRINT_RESPONSE = true;

		public ConnectionSettings ConnectionInfo {
			get;
			set;
		}

		public ConnectAttemptInfo GetConnectAttemptInfo ()
		{

			return this.ConnectionTuple?.Item2;

		}

		//public bool reconnect = false;

		#region variables
		//bool shouldconnect = false; // You don't want to connect automatically 

		//private int max_reconnect = 3;

		//private int connectattempts = 0;

		//private bool stopConnect = false;

		#endregion

		public void Disconnect ()
		{

#if DEBUG
			if (DebugRippleLibSharp.NetworkInterface) {
				Logging.WriteLog ("NetworkInterface : method disconnect() : begin\n");
			}
#endif

			WebSocket4Net.WebSocket websocket = this.ConnectionTuple.Item1;

			//shouldconnect = false;

			if (websocket == null) {
#if DEBUG
				if (DebugRippleLibSharp.NetworkInterface) {
					Logging.WriteLog ("NetworkInterface : method disconnect() : websocket == null\n");
				}
#endif
				return;
			}

#if DEBUG
			if (DebugRippleLibSharp.NetworkInterface) {
				Logging.WriteLog ("NetworkInterface : Websocket.State = " + websocket.State.ToString () + "\n");
			}
#endif

			switch (websocket.State) {
			case WebSocket4Net.WebSocketState.Closed: {
					Logging.WriteLog ("Connection is already closed\n");
					break;
				}

			case WebSocket4Net.WebSocketState.Closing: {
					Logging.WriteLog ("Connection is already currently closing\n");
					break;
				}

			case WebSocket4Net.WebSocketState.Open: {
					Logging.WriteLog ("Closing connection\n");
					try {
						websocket.Close ();
					} catch (Exception e) {
						Logging.WriteLog (e.Message + "\n");
					}
					break;
				}

			case WebSocket4Net.WebSocketState.Connecting: {
					Logging.WriteLog ("Can't disconnect. Currently connecting.\n");
					try {
						//this.websocket.Close ();
					} catch (Exception e) {
						Logging.WriteLog ("Exception thrown\n" + e.Message + "\n");
					}
					break;
				}

			case WebSocket4Net.WebSocketState.None: {
					Logging.WriteLog ("WebsocketState == null");
					break;
				}



			default: {
					Logging.WriteLog ("Socket in an unknown state\n");
					break;
				}
			}
		}

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public bool connecting = false;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		public Task<bool> ConnectTask ()
		{

			return Task.Run (
				delegate {
#if DEBUG
					string method_sig = clsstr + "Task<bool> connectTask () : ";
					if (DebugRippleLibSharp.NetworkInterface) {
						Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
					}
#endif


					return ConnectThread ();

					//return this.isConnected();
				}

			);
		}

		public bool Connect ()
		{
			#region debug
#if DEBUG
			string method_sig = clsstr + nameof (Connect) + DebugRippleLibSharp.both_parentheses;


			if (DebugRippleLibSharp.NetworkInterface) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif
			#endregion

			// launch in new thread 
			//Thread thr = new Thread (new ThreadStart (connectthread));
			//thr.Start();

			return this.ConnectThread ();

		}

		private bool ConnectThread ()
		{

			#region debug
#if DEBUG
			string method_sig = clsstr + nameof (ConnectThread) + DebugRippleLibSharp.both_parentheses;
			if (DebugRippleLibSharp.NetworkInterface) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			if (connecting) {
#if DEBUG
				if (DebugRippleLibSharp.NetworkInterface) {
					// todo debug
					Logging.WriteLog (method_sig + "Connection already in progress, returning\n");
				}
#endif
				return false;
			}
			#endregion


			try {
				connecting = true;
				#region debug

#if DEBUG
				if (DebugRippleLibSharp.NetworkInterface) {
					Logging.WriteLog (method_sig + "try-catch " + DebugRippleLibSharp.beginn);
				}
#endif


#if DEBUG
				if (DebugRippleLibSharp.NetworkInterface) {
					Logging.WriteLog (method_sig + "begin while loop\n");
				}
#endif
				#endregion

				Task<bool> bl = TryConnect (ConnectionInfo);

				bl.Wait ();
				bool returned = bl.Result;

				#region debug
#if DEBUG
				if (DebugRippleLibSharp.NetworkInterface) {
					Logging.WriteLog (method_sig + "returned=" + returned + "sleep\n");
				}
#endif
				//Thread.Sleep (SLEEP_TIME);

#if DEBUG
				if (DebugRippleLibSharp.NetworkInterface) {
					Logging.WriteLog (method_sig + "wake\n");
				}
#endif
				#endregion

				return returned;

#if DEBUG
				//if (returned == false && Debug.NetworkInterface && (this.max_reconnect <= this.connectattempts)) {
				//	Logging.writeLog (method_sig + "exceeded max connect attempts\n");
				//}
#endif
#pragma warning disable 0168
			} catch (Exception e) {
#pragma warning restore 0168
#if DEBUG
				if (DebugRippleLibSharp.NetworkInterface) {
					Logging.WriteLog (method_sig + DebugRippleLibSharp.exceptionMessage + e.ToString ());
				}
#endif
			} finally {
#if DEBUG
				if (DebugRippleLibSharp.NetworkInterface) {
					Logging.WriteLog (method_sig + "finally\n");
				}
#endif
				connecting = false;

			}
			return false;

		}


		private Task<bool> ConnectAttempt (ConnectionSettings coninfo, string url)
		{

			//string u = url;
			return Task.Run (
				delegate {

					#region debug
#if DEBUG
					string method_sig = nameof (ConnectAttempt) + DebugRippleLibSharp.left_parentheses + DebugRippleLibSharp.ToAssertString (coninfo) + DebugRippleLibSharp.right_parentheses;


					if (DebugRippleLibSharp.NetworkInterface) {
						Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
					}
#endif
					#endregion

					//String[] urls = coninfo.ServerUrls;
					String userAgent = coninfo.UserAgent;
					String local = coninfo.LocalUrl;

					ConnectAttemptInfo inf = new ConnectAttemptInfo {
						ServerUrl = url,
						UserAgent = userAgent,
						LocalUrl = local
					};

					WebSocket4Net.WebSocket websocket = this.CreateWebSocket (inf);

					this.ConnectionTuple = new Tuple<WebSocket4Net.WebSocket, ConnectAttemptInfo> (websocket, inf);

					this.SetHandlers (websocket, url, coninfo);
					this.OpenConnection ();

					//String srv = (String)((connectionInfo != null && connectionInfo.server != null) ? connectionInfo.server : "null");
					if (websocket.State == WebSocket4Net.WebSocketState.Open) {
#if DEBUG
						if (DebugRippleLibSharp.NetworkInterface) {
							Logging.WriteLog ("NetworkInterface : method tryconnect() : websocket.State = Open, url = " + url + "/n");
						}
#endif



						return true;
					}

#if DEBUG
					if (DebugRippleLibSharp.NetworkInterface) {
						Logging.WriteLog ("NetworkInterface : method tryconnect() : websocket.State = " + websocket.State.ToString () + ", url = " + url + "/n");
					}
#endif


					return false;





				}

			);

		}

		private Task<bool> TryConnect (ConnectionSettings conenctionInfo)
		{
			return Task.Run (
				delegate {

					#region debug

#if DEBUG
					string method_sig = nameof (TryConnect)  /*+ Debug.toAssertString( coninfo )*/ + DebugRippleLibSharp.right_parentheses;


					if (DebugRippleLibSharp.NetworkInterface) {
						Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
					}
#endif

					if (conenctionInfo == null) {
#if DEBUG
						Logging.WriteLog (method_sig + nameof (conenctionInfo) + " is null, returning\n");
#endif
						return false;
					}

					#endregion


					String [] urls = conenctionInfo.ServerUrls;
					String userAgent = conenctionInfo.UserAgent;
					String local = conenctionInfo.LocalUrl;

					//if (Debug.NetworkInterface) {
					//	Logging.writeLog (clsstr + "method tryconnect() : begin\n");
					//}




					for (int i = 0; i < urls.Length; i++) {
#if DEBUG
						if (DebugRippleLibSharp.NetworkInterface) {
							Logging.WriteLog (method_sig + "begin for loop\n");
						}
#endif


						String url = urls [i];
#if DEBUG
						if (DebugRippleLibSharp.NetworkInterface) {
							Logging.WriteLog (method_sig + nameof (url) + DebugRippleLibSharp.equals + url + "\n");
						}
#endif

						if (url == null || url.Equals ("")) {
#if DEBUG
							if (DebugRippleLibSharp.NetworkInterface) {
								Logging.WriteLog (method_sig + "url is empty, continuing with for\n");
							}
#endif
							continue;
						}

#if DEBUG
						if (DebugRippleLibSharp.NetworkInterface) {
							Logging.WriteLog ("NetworkInterface : connectionattempt = " + url + "\n");
						}
#endif

						//this.webSocketDisposal ();
						Task<bool> conectTask = this.ConnectAttempt (conenctionInfo, url);

						conectTask.Wait ();
						bool res = conectTask.Result;

						if (res) {
#if DEBUG
							if (DebugRippleLibSharp.NetworkInterface) {
								Logging.WriteLog (method_sig + "websocket created\n");
							}
#endif
							return true;
						}

					}

					return false;
				});

		}

		private void Websocket_Closed (object sender, EventArgs e)
		{
			Logging.WriteLog ("Connection Closed\n");
			if (onClose != null) {
				onClose (sender, e);
			} else {
				Logging.WriteLog ("NetworkInterface : Error : onClose == null\n");
			}

			//TODO autoreconnectt implementation 
			/*
			if ((connectionInfo != null) && connectionInfo.reconnect && this.shouldconnect && !this.connecting) {
				#if DEBUG
				if (Debug.NetworkInterface) {
					Logging.writeLog("NetworkInterface : method websocket_Closed() : auto reconnecting\n");
				}
				#endif
				this.connect ();
			}
			*/
		}

		private void Websocket_Opened (object sender, EventArgs e)
		{
			//this.stopConnect = true;
			//this.connectattempts = 0;
			Logging.WriteLog ("Connection Opened\n");
			if (onOpen != null) {
				onOpen (sender, e);
			} else {
				Logging.WriteLog ("NetworkInterface : Error: onOpen == null\n");
			}


			Thread.Sleep (5);
			onOpenWaitHandler.Set ();
		}

		public void SendToServer (String message)
		{
			var tupe = this.ConnectionTuple;

			WebSocket4Net.WebSocket websocket = tupe.Item1;


			if (websocket == null || websocket.State != WebSocket4Net.WebSocketState.Open) {

				if ((ConnectionInfo != null) && ConnectionInfo.Reconnect /*&& this.shouldconnect*/) {
					this.TryConnect (ConnectionInfo);

					// let the server breathe
					System.Threading.Thread.Sleep (100);

					if (websocket == null || websocket.State != WebSocket4Net.WebSocketState.Open) {
						Logging.WriteLog ("You need to be connected to a server to send a message.\n");
						return;
					}


				} else {

					Logging.WriteLog ("You need to be connected to a server to send a message.\n");
					return;
				}
			} else {
				Logging.WriteLog ("Sending message :\n" + message + "\n");
				try {

					websocket.Send (message);

				} catch (Exception e) {

					Logging.WriteLog ("Error sending message : An exception was thrown.\n" + e.Message);
					return;
				}


			}

			if (websocket != null) {

				// TODO I don't think this web socket library has any error flags to check??? (websockets4net), it's event based, see on error event, also catching exceptions above


			} else {
				Logging.WriteLog ("Error, websocket address is now null. Is another thread calling websocket_dispose?\n");
			}

		}

		public void SendToServer (byte [] message)
		{
			SendToServer (message, 0, message.Length);
		}

		public void SendToServer (byte [] message, int byteoffset, int bytelength)
		{
			var tupe = this.ConnectionTuple;

			WebSocket4Net.WebSocket websocket = tupe.Item1;


			if (websocket == null || websocket.State != WebSocket4Net.WebSocketState.Open) {

				if (ConnectionInfo != null && ConnectionInfo.Reconnect /*&& this.shouldconnect*/) {
					this.TryConnect (ConnectionInfo);

					// let the server breathe
					System.Threading.Thread.Sleep (10);

					if (websocket == null || websocket.State != WebSocket4Net.WebSocketState.Open) {
						Logging.WriteLog ("You need to be connected to a server to send a message.\n");
						return;
					}


				} else {

					Logging.WriteLog ("You need to be connected to a server to send a message.\n");
					return;
				}
			} else {
				Logging.WriteLog ("Sending binary message\n"/*:\n" + message + "\n"*/ );
				try {

					websocket.Send (message, byteoffset, bytelength);


				} catch (Exception e) {

					Logging.WriteLog ("Error sending message : An exception was thrown.\n" + e.Message);
					return;
				}


			}

			if (websocket != null) {

				// TODO I don't think this web socket library has any error flags to check??? (websockets4net), it's event based, see on error event, also catching exceptions above


			} else {
				Logging.WriteLog ("Error, websocket address is now null. Is another thread calling websocket_dispose?\n");
			}

		}

		protected void WebSocketDisposal ()
		{
#if DEBUG
			if (DebugRippleLibSharp.NetworkInterface) {
				Logging.WriteLog ("NetworkingInterface : method webSocketDisposal() : begin \n");
			}
#endif

			var tupe = this.ConnectionTuple;

			WebSocket4Net.WebSocket websocket = tupe.Item1;

			if (websocket != null) {
#if DEBUG
				if (DebugRippleLibSharp.NetworkInterface) {
					Logging.WriteLog ("NetworkingInterface : method webSocketDisposal() : this.websocket!=null\n");
				}
#endif
				this.Disconnect ();
				websocket = null;
			} else {
#if DEBUG
				if (DebugRippleLibSharp.NetworkInterface) {
					Logging.WriteLog ("NetworkingInterface : method webSocketDisposal() : websocket==null\n");
				}
#endif
			}
		}

		private void ProcessIncomingJson (WebSocket4Net.MessageReceivedEventArgs e)
		{
			//dynamic incoming = JsonConvert.DeserializeObject<dynamic>(jsonString);

#if DEBUG
			if (DebugRippleLibSharp.NetworkInterface) {
				Logging.WriteLog ("NetworkingInterface : method processIncomingJson (MessageReceivedEventArgs e) : begin ");
			}
#endif

			if (onMessage != null) {
				// todo should this print
				onMessage (this, e);  // All networking depends on this being called. 

			} else {
				Logging.WriteLog ("NetworkInterface : method processIncomingJson : Critical Error : onMessage == null\n");
			}

		}

		public WebSocket4Net.WebSocket CreateWebSocket (ConnectAttemptInfo attemptInfo)
		{
#if DEBUG
			string method_sig = clsstr + nameof (CreateWebSocket) + DebugRippleLibSharp.both_parentheses;
			if (DebugRippleLibSharp.NetworkInterface) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}


#endif
			if (attemptInfo == null) {
				return null; // TODO
			}

			WebSocket4Net.WebSocket websocket = null;




			// prints in debug mode or not
			Logging.WriteLog ("Initiating connection to server ");

			try {

				websocket = new WebSocket4Net.WebSocket (attemptInfo.ServerUrl);

				/*
				this.websocket = new WebSocket4Net.WebSocket (
					url,
					"",
					null,
					null,
					userAgent,
					local,
					(WebSocket4Net.WebSocketVersion)(13),
					null
				);*/

#pragma warning disable 0168
			} catch (ArgumentException e) {
#pragma warning restore 0168

				string url_err = "Invalid URL : " +
					(attemptInfo?.ServerUrl ?? "null") + "\n" +
					"Syntax is [protocol]://[domain]:[port]\n" +
					"Example : ws://s-west.ripple.com:80 or" +
					" (ssl) wss://s-west.ripple.com:443\n";

				Logging.WriteLog ("Exception thrown, " + url_err);


#if DEBUG
				if (DebugRippleLibSharp.NetworkInterface) {
					Logging.WriteLog (e.Message);
				}
#endif


				return null;

#pragma warning disable 0168
			} catch (Exception e) {
#pragma warning restore 0168
#if DEBUG
				Logging.WriteLog (method_sig + "Exception thrown : \n" + e.Message + "\n");
#endif


				return null;
			}


			if (websocket == null) {
#if DEBUG
				Logging.WriteLog (method_sig + "Unable to create websocket, did you use a valid url?\n");
#endif
				return null;
			}




			return websocket;


		}

		private void SetHandlers (WebSocket4Net.WebSocket websocket, string url, ConnectionSettings conInfo)
		{

#if DEBUG
			string method_sig = clsstr + nameof (SetHandlers) + DebugRippleLibSharp.both_parentheses;
			if (DebugRippleLibSharp.NetworkInterface) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif


			websocket.Opened += Websocket_Opened;



			websocket.Closed += Websocket_Closed;

			websocket.MessageReceived += (object sender, WebSocket4Net.MessageReceivedEventArgs e) =>
				this.ProcessIncomingJson (e);

			/*
			websocket.MessageReceived += delegate(object sender, MessageReceivedEventArgs e) {
				#if DEBUG
				if (Debug.NetworkInterface) {
					Logging.writeLog ("NetworkInterface : event websocket.MessageReceived : begin");
				}


				if (PRINT_RESPONSE || Debug.NetworkInterface) {
					Logging.writeLog ("Response from server " + url + ":\n" + e.Message + "\n\n");
				}
				#endif

				this.processIncomingJson (e);
			};*/



			websocket.Error += ErrorFunction;




			//};
			NetworkRequestTask.InitNetworkTasking (this);


		}




		private void ErrorFunction (object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
		{
			//websocket.Error += delegate(object sender, ErrorEventArgs e) {

			//stopConnect = true;

			if (e.Exception.Message.Equals ("RemoteCertificateNotAvailable")) {

				String message = "Unable to establish an ssl connection, you need to install the servers ssl security certificate\n" +
					"Example # certmgr --ssl " +
					///* url + 
					"  ( using the command line of your local operating system terminal/cmd.exe ect )\n\n" +
					"if that doesn't work try importing mozilla's certificate store" +
					"Example# mozroots --import --sync \n";

				//MainWindow.currentInstance

#if DEBUG
				if (DebugRippleLibSharp.NetworkInterface) {
					Logging.WriteLog (message);
					Logging.WriteLog ("should print" + e.Exception.Message);
				}
#endif







				return;
			}


#if DEBUG
			if (DebugRippleLibSharp.NetworkInterface) {
				Logging.WriteLog ("Error Occured\n" + e.Exception.Message + "\n");
			}
#endif
			//stopConnect = true;
			return;
		}





		private bool OpenConnection ( /*string url*/)
		{
#if DEBUG
			string method_sig = clsstr + nameof (OpenConnection) + DebugRippleLibSharp.both_parentheses;
			if (DebugRippleLibSharp.NetworkInterface) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			var tupe = this.ConnectionTuple;
			WebSocket4Net.WebSocket websocket = tupe.Item1;
			ConnectAttemptInfo inf = tupe.Item2;

			try {
#if DEBUG
				if (DebugRippleLibSharp.NetworkInterface) {
					Logging.WriteLog (method_sig + "opening websocket\n"); // TODO print to console non debug mode?
				}
#endif

				websocket.Open ();


			} catch (Exception e) {
				Logging.WriteLog (e.ToString () + "\nError opening connection to " + inf.ServerUrl ?? "(null)" + " : \n" + e.Message + "\n");
				return false;
			}


#if DEBUG
			if (DebugRippleLibSharp.NetworkInterface) {
				Logging.WriteLog (method_sig + "waiting on onOpenWaitHandler...\n"); // TODO print to console non debug mode?
			}
#endif
			onOpenWaitHandler.Reset ();
			onOpenWaitHandler.WaitOne ();

#if DEBUG
			if (DebugRippleLibSharp.NetworkInterface) {
				Logging.WriteLog (method_sig + "finished waiting on onOpenWaitHandler...\n"); // TODO print to console non debug mode?
			}
#endif

			bool b = IsConnected ();
#if DEBUG
			if (DebugRippleLibSharp.NetworkInterface) {
				Logging.WriteLog (method_sig + "returning isConnected () == " + b.ToString ());
			}

#endif

			return b;
		}

		public bool IsConnected ()
		{

#if DEBUG
			string method_sig = clsstr + nameof (IsConnected) + DebugRippleLibSharp.both_parentheses;
			if (DebugRippleLibSharp.NetworkInterface) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif



			var tupe = this.ConnectionTuple;


			WebSocket4Net.WebSocket websocket = tupe?.Item1;

			if (websocket == null) {
#if DEBUG

				if (DebugRippleLibSharp.NetworkInterface) {
					Logging.WriteLog (method_sig + "this.websocket == null, returning false");
				}
#endif
				return false;
			}


			return (websocket.State == WebSocket4Net.WebSocketState.Open);
		}


		#region handlers
		public delegate void OnMessageEventHandler (object sender, WebSocket4Net.MessageReceivedEventArgs e);

		public OnMessageEventHandler onMessage;




		public delegate void connectEventHandler (object sender, EventArgs e);

		public connectEventHandler onOpen;

		public connectEventHandler onClose;

		//public delegate void errorEventHandler (object sender, SuperSocket.ClientEngine.ErrorEventArgs e);
		public delegate void errorEventHandler (object sender, EventArgs e);
		public errorEventHandler onError;



		private EventWaitHandle onOpenWaitHandler = new ManualResetEvent (true);
		#endregion

#if DEBUG
		private const string clsstr = nameof (NetworkInterface) + DebugRippleLibSharp.colon;
#endif

	}





}

