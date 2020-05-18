using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Commands.Subscriptions;
using RippleLibSharp.Result;
using RippleLibSharp.Util;
using WebSocket4Net;
using static RippleLibSharp.Commands.Subscriptions.LedgerTracker;

namespace RippleLibSharp.Network
{
	public static class NetworkRequestTask
	{


		public static Task<Response<T>> RequestResponse<T> (
			IdentifierTag identifierTag, // IDENTIFIER TAG VALUES ARE PRINTED TO BLOCKCHAIN IN PLAIN TEXT !!! They are used to match requests with responses. 
			string request, 
			NetworkInterface networkInterface, 
			CancellationToken token,
	    		SubscribeResponsesConsumer responsesConsumer = null
		)

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
						+ nameof (identifierTag)
						+ DebugRippleLibSharp.equals
						+ identifierTag?.ToString () ?? "null"
						+ "..."
						+ DebugRippleLibSharp.right_parentheses;
#endif

					if (token.IsCancellationRequested) {
						return null;
					}

					if (networkInterface == null) {
						// TODO connect?, alert user? ect

						return null;
					}

					int ticket = identifierTag.IdentificationNumber;

					TicketStub stub = new TicketStub {
						Handle = new ManualResetEvent (true)
					};

					

					Response<T> resp = null;

					ticketCache [ticket] = stub;

					stub.SetConsumer (responsesConsumer);

					try {
						int interval = 250;
						int time = 0;
						int maxtime = 0;
						int ret = 0;
						int retryIn = 60000;
						for (int i = 1; i < 3; i++) {

							stub.Handle.Reset ();

							Task.Run (delegate {
								if (token.IsCancellationRequested) {
									return;
								}
								networkInterface.SendToServer (request);
							}, token);

							//stub.Handle.WaitOne (MAX_WAIT);
						
							time = 0;
							do {

								ret = WaitHandle.WaitAny (new [] { token.WaitHandle, stub.Handle }, interval);
								time += interval;
								maxtime += interval;


							} while (
								ret == WaitHandle.WaitTimeout &&
								!token.IsCancellationRequested &&
								//!ticketCache.ContainsKey (ticket)
								!stub.HasResponse &&
								time <= retryIn
							);
							//Response<Json_Response> d = stub.response;
#if DEBUG

							if (DebugRippleLibSharp.NetworkRequestTask) {
								Logging.WriteLog (method_sig + "Type = " + typeof (T).FullName);
							}
#endif



							

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
							if (resp != null) {

								if (resp.error_code == 666666) {
									continue;
								}

								break;
							}


							

							
						}


						while (
								ret == WaitHandle.WaitTimeout &&
								!token.IsCancellationRequested &&
								//!ticketCache.ContainsKey (ticket)
								!stub.HasResponse &&
								    maxtime <= MAX_WAIT
						) {

							ret = WaitHandle.WaitAny ( new [] { token.WaitHandle, stub.Handle }, interval );
							time += interval;
						}

						
						//return resp;


					} catch (Exception e) {
						throw e;
					} finally {

						if (ticketCache == null) {
							throw new ArgumentNullException ("NetworkRequestTask contains a null ticketCache");
						}
						if (responsesConsumer == null) {
							bool successfull = ticketCache.TryRemove (ticket, out TicketStub tempStub);
							try {
								tempStub?.Handle?.Set ();
							} catch (Exception ex) {

							}
							tempStub?.Handle?.Close ();

						}

						//if (resp != null) {
						//	responsesConsumer.
						//}
					}
						
						
					return resp;
				}
				, token);
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
			TicketStub ticketstub = null;
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

			Response<Json_Response> response = null;
			try {

				dynamic dynamite = DynamicJson.Parse (sp.str, System.Text.Encoding.Default);

				string type = dynamite.type;

				if ("ledgerClosed".Equals(type)) {
					try {
						LedgerClosed ledger = dynamite;
						LedgerTracker.SetLedger (ledger);

					} catch ( Exception e ) {
#if DEBUG
						if (DebugRippleLibSharp.NetworkRequestTask) {
							Logging.ReportException (method_sig, e);
						}
#endif

					}
					return;
				}

				if ("serverStatus".Equals(type)) {
					try {
						ServerStateEventArgs serverState = dynamite;
						LedgerTracker.SetServerState (serverState);
					} catch (Exception e) {
						//TODO
#if DEBUG
						if (DebugRippleLibSharp.NetworkRequestTask) {
							Logging.ReportException (method_sig, e);
						}
#endif
					}
					return;
				}
				response = dynamite;

				if (response == null) {
					throw new NullReferenceException ("Unable to parse json response from network");
				}
			} catch (Exception e) {
#if DEBUG
				if (DebugRippleLibSharp.NetworkRequestTask) {
					Logging.WriteLog (e.Message);
					Logging.WriteLog (e.StackTrace);
				}
#endif

				// Exctract the id from the json to try and recover from the error
				//string idPattern = "{\"id\":";
				string idPattern = "{\"id\":{ \"IdentificationNumber\":";
				//if (sp.str.StartsWith(idPattern)) {
				if (sp.str.StartsWith (idPattern, StringComparison.CurrentCulture)) {
					// might seem overkill parsing though strings but it's worth handling this gracefully 
					string sub = sp.str.Substring (idPattern.Length);
					string number = new string (sub.TakeWhile (char.IsDigit).ToArray ());
					try {
						int i = int.Parse (number);
						ticketCache.TryGetValue (i, out ticketstub);
						if (ticketstub == null) return;

#if DEBUG
						if (DebugRippleLibSharp.NetworkRequestTask) {
							Logging.WriteLog (method_sig + "extracted ticket stub from broken json");
						}
#endif

						ticketstub.JsonResponseObj = new Response<Json_Response> {
							error_code = 666666,
							error_message = "error parsing json response",
							error = "PARSE_ERROR",
							status = "error"
						};

						ticketstub.Handle?.Set ();
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

				IdentifierTag identifier = response?.id;

				int? tick = identifier?.IdentificationNumber;
				if (tick == null) {


					throw new ArgumentNullException (nameof (identifier.IdentificationNumber), "Unable to parse identification number from network response");

					
				}


#if DEBUG

				if (DebugRippleLibSharp.NetworkRequestTask) {
					Logging.WriteLog (method_sig + tick.ToString ());
				}
#endif
				if (!ticketCache.ContainsKey ((int)tick)) {
					return;
				}



				ticketCache.TryGetValue ((int)tick, out ticketstub);
				if (ticketstub == null) return;
#if DEBUG

				if (DebugRippleLibSharp.NetworkRequestTask) {
					Logging.WriteLog (method_sig + "retrieved ticket stub from json !!!");
				}
#endif

				
				ticketstub.JsonResponseObj = response;

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
					Logging.WriteLog (method_sig + "Exception thrown in " + nameof (NetworkRequestTask));

					Logging.WriteLog (e.Message);

					Logging.WriteLog (e.StackTrace);





					while (e.InnerException != null) {
						Logging.WriteLog (nameof( e.InnerException) );

						Logging.WriteLog (e.InnerException.Message);

						Logging.WriteLog (e.InnerException.StackTrace);

						e = e.InnerException;
					}

					Logging.WriteLog (sp.str);

		    			/* used for testing only
					string folder = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
					string path = System.IO.Path.Combine (folder, "RippleLibSharp.crash.crash");
					System.IO.File.WriteAllText (path, sp.str);
		    			*/

				}
#endif

			} finally {
				if (ticketstub != null) {
					ticketstub.HasResponse = true;
					try {

						ticketstub.Handle?.Set ();
					} catch ( Exception e) {

#if DEBUG
						if (DebugRippleLibSharp.NetworkRequestTask) {
							Logging.ReportException (method_sig, e);
						}
#endif
					}
					ticketstub.FireConsumer (response);
				}
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
			networkInterface.OnMessage += delegate (object sender, MessageReceivedEventArgs e) {
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


		public static int MAX_WAIT = 60000 * 7; // 7 minutes !!
#if DEBUG
		private const string clsstr = nameof (NetworkRequestTask) + DebugRippleLibSharp.colon;
#endif

		public static ConcurrentDictionary<int, TicketStub> ticketCache = new ConcurrentDictionary<int, TicketStub> ();
	}

	public class TicketStub
	{
		public EventWaitHandle Handle { get; set; }


		public Response<Json_Response> JsonResponseObj { get; set; }

		public bool HasResponse {
			get;
			set;
		}

		public Response<T> GetResponse<T> ()
		{

			Response<T> res = new Response<T> ();



			res = res.SetFromJsonResp ( this.JsonResponseObj );

			return res;

		}

		public void FireConsumer (Response<Json_Response> response) {


			if (consumer != null) {

				//Task.Run ( delegate {

				Thread thread = new Thread ((object obj) => {
					SubscribeEventArgs subscribeEvent = new SubscribeEventArgs {
						Response = response

					};
					consumer.OnMessage (this, subscribeEvent);
				});

				thread.Start ();



				//});


			}
		}

		public void SetConsumer (SubscribeResponsesConsumer consumer)
		{
			this.consumer = consumer;
		}

		private SubscribeResponsesConsumer consumer = null;
		public int Ticket {
			get;

			set;
		}

	}





}

