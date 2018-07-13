using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;
using Codeplex.Data;
using RippleLibSharp.Result;
using RippleLibSharp.Util;

namespace RippleLibSharp.Network
{
	public static class NetworkRequestTask
	{


		public static Task<Response<T>> RequestResponse<T> (int ticket, string request, NetworkInterface networkInterface)
		{
			return Task.Run (
				delegate {
#if DEBUG
					string method_sig =
						clsstr
						+ nameof (RequestResponse)
						+ " < "
						+ typeof (T).ToString ()
						+ " > "
								   + DebugRippleLibSharp.left_parentheses
								   + nameof (ticket)
								   + DebugRippleLibSharp.equals
								   + ticket.ToString ()
								   + "..."
								   + DebugRippleLibSharp.right_parentheses;
#endif
					if (networkInterface == null) {
						// TODO connect?, alert user? ect

						return null;
					}

					TicketStub stub = new TicketStub {
						Handle = new ManualResetEvent (true)
					};



					Response<T> resp = null;
					for (int i = 1; i < 3; i++) {

						stub.Handle.Reset ();
						ticketCache [ticket] = stub;
						Task.Run (delegate {
							networkInterface.SendToServer (request);
						}

						);
						stub.Handle.WaitOne (MAX_WAIT);

						//Response<Json_Response> d = stub.response;
#if DEBUG

						if (DebugRippleLibSharp.NetworkRequestTask) {
							Logging.WriteLog (method_sig + "Type = " + typeof (T).FullName);
						}
#endif



						bool successfull = ticketCache.TryRemove (ticket, out TicketStub tempStub);

						/*
						if (!successfull) {
							if (tempStub == null) {
								tempStub = stub;
								if (tempStub == null) {
									goto RETRY;

								}


							}
						}
						*/
						resp = stub?.GetResponse<T> ();
						if (resp == null) {
							continue;
						}


						if (resp.error_code == 666666) {
							continue;
						}

						return resp;
					}
					return resp;
				}
			);
		}

		public static int ObtainTicket ()
		{
			return _ticket++;
		}
#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private static int _ticket = 0;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		private class StrParam
		{
			public StrParam (string s)
			{
				this.str = s;
			}
			public string str;

		}

		private static void OnMessageThread (object param)
		{
#if DEBUG
			string method_sig = clsstr + "onMessageThread";
			if (DebugRippleLibSharp.NetworkRequestTask) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif
			TicketStub ts = null;
			if (!(param is StrParam sp)) {
#if DEBUG
				if (DebugRippleLibSharp.NetworkRequestTask) {
					Logging.WriteLog (method_sig + "sp == null ");
				}
#endif
				return;
			}

			if (sp.str == null) {
#if DEBUG
				if (DebugRippleLibSharp.NetworkRequestTask) {
					Logging.WriteLog (method_sig + "sp.str == null");
				}
#endif
				return;
			}

			Logging.WriteLog (sp.str);

			Response<Json_Response> d = null;
			try {
				d = DynamicJson.Parse (sp.str, System.Text.Encoding.Default);

				if (d == null) {
					throw new NullReferenceException ();
				}
			} catch (Exception e) {
#if DEBUG
				if (DebugRippleLibSharp.NetworkRequestTask) {
					Logging.WriteLog (e.Message);
					Logging.WriteLog (e.StackTrace);
				}
#endif

				// Exctract the id from the json to try and recover from the error
				string idPattern = "{\"id\":";
				//if (sp.str.StartsWith(idPattern)) {
				if (sp.str.StartsWith (idPattern, StringComparison.CurrentCulture)) {
					// might seem overkill parsing though strings but it's worth handling this gracefully 
					string sub = sp.str.Substring (idPattern.Length);
					string number = new string (sub.TakeWhile (char.IsDigit).ToArray ());
					try {
						int i = int.Parse (number);
						ticketCache.TryGetValue (i, out ts);
						if (ts == null) return;

#if DEBUG
						if (DebugRippleLibSharp.NetworkRequestTask) {
							Logging.WriteLog (method_sig + "extracted ticket stub from broken json");
						}
#endif

						ts.JsonResponseObj = new Response<Json_Response> {
							error_code = 666666,
							error_message = "error parsing json response",
							error = "PARSE_ERROR",
							status = "error"
						};

						ts.Handle?.Set ();
					} catch (Exception ex) {
#if DEBUG
						Logging.ReportException (method_sig, ex);
#endif
					}
				}

				return;
			}


			try {

				//Response<Json_Response> d = DynamicJson.Parse(sp.str);

				//Ping p = d;
				int tick = d.id;
#if DEBUG

				if (DebugRippleLibSharp.NetworkRequestTask) {
					Logging.WriteLog (method_sig + tick.ToString ());
				}
#endif
				if (!ticketCache.ContainsKey (tick)) {
					return;
				}



				ticketCache.TryGetValue (tick, out ts);
				if (ts == null) return;
#if DEBUG

				if (DebugRippleLibSharp.NetworkRequestTask) {
					Logging.WriteLog (method_sig + "retrieved ticket stub from json !!!");
				}
#endif
				ts.JsonResponseObj = d;
#if DEBUG

				if (DebugRippleLibSharp.NetworkRequestTask) {
					Logging.WriteLog (method_sig + "dynamic response set");
				}
#endif

			}

#pragma warning disable 0168
			catch (Exception e) {
#pragma warning restore 0168

#if DEBUG

				if (DebugRippleLibSharp.NetworkRequestTask) {
					Logging.WriteLog (method_sig + "exception thrown");

					RippleLibSharp.Util.Logging.WriteLog (e.Message);

					RippleLibSharp.Util.Logging.WriteLog (e.StackTrace);

					string folder = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
					string path = System.IO.Path.Combine (folder, "RippleLibSharp.crash.crash");

					System.IO.File.WriteAllText (path, sp.str);

					if (e.InnerException != null) {
						RippleLibSharp.Util.Logging.WriteLog (e.InnerException.Message);

						RippleLibSharp.Util.Logging.WriteLog (e.InnerException.StackTrace);

					}


				}
#endif

			} finally {
				ts?.Handle?.Set ();
			}


		}

		public static void InitNetworkTasking (NetworkInterface networkInterface)
		{
#if DEBUG
			string method_sig = clsstr + nameof (InitNetworkTasking) + DebugRippleLibSharp.left_parentheses + nameof (networkInterface) + DebugRippleLibSharp.right_parentheses;
			if (DebugRippleLibSharp.NetworkRequestTask) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

#pragma warning disable RECS0154 // Parameter is never used
			networkInterface.onMessage += delegate (object sender, MessageReceivedEventArgs e) {
#pragma warning restore RECS0154 // Parameter is never used
				//dynamic DynamicJson.Parse(e.Message);
#if DEBUG
				string event_sig = method_sig + " event netwInterface.onMessage : ";
				if (DebugRippleLibSharp.NetworkRequestTask) {
					Logging.WriteLog (event_sig + DebugRippleLibSharp.beginn);
				}

#endif

				StrParam sp = new StrParam (e.Message);
				ParameterizedThreadStart pts = new ParameterizedThreadStart (OnMessageThread);
				Thread th = new Thread (pts);
				th.Start (sp);
			};

			//netwInterface.onMessage += n
		}


		public static int MAX_WAIT = 60000 * 20; // 20 minutes !!
#if DEBUG
		private const string clsstr = nameof (NetworkRequestTask) + DebugRippleLibSharp.colon;
#endif

		public static ConcurrentDictionary<int, TicketStub> ticketCache = new ConcurrentDictionary<int, TicketStub> ();
	}

	public class TicketStub
	{
		public EventWaitHandle Handle { get; set; }


		public RippleLibSharp.Result.Response<Json_Response> JsonResponseObj { get; set; }


		public Response<T> GetResponse<T> ()
		{

			Response<T> res = new Response<T> ();



			res = res.SetFromJsonResp (this.JsonResponseObj);

			return res;

		}

		public int Ticket {
			get;

			set;
		}

	}





}

