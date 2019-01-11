using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json.Linq;
using Ripple.Core;
using Ripple.Core.ShaMapTree;
using Ripple.Core.Types;
using Ripple.Testing.Utils;
using WebSocketSharp;

namespace Ripple.Testing.Client
{
    public class Connection : IEventEmitter
    {
        public static readonly ILog Log = Logging.ForContext();

        private readonly IDictionary<int, Request> _requests;
        private readonly WebSocket _ws;
        public bool Connected { get; private set; }
        private int _nextId = 1;

        public event Action<Connection> OnConnected;
        public event Action<Connection> OnDisconnected;

        // These are used for the one shot events
        public static readonly EventInfo OnConnectedEvent =
            typeof(Connection).GetEvent(nameof(OnConnected));

        public Connection(string url)
        {
            _ws = new WebSocket(url);
            _ws.OnOpen += OnOpen;
            _ws.OnClose += OnClose;
            _ws.OnMessage += OnMessage;
            _ws.OnError += OnError;
            // We want to be able to clean up these
            _requests = new ConcurrentDictionary<int, Request>();
        }

        private static void OnError(object sender, ErrorEventArgs e)
        {
            Log.Error("ws error: ", e.Exception);
            // We throw this in another thread, just to notify handlers
            // though there's probably a cleaner way to handle this.
            NonTestThreadException.ThrowInOtherThread(e.Exception);
        }

        public Connection Connect()
        {
            Log.Debug(nameof(Connect));
            _ws.ConnectAsync();
            return this;
        }

        internal Request Request(string command, JObject parameters = null)
        {
            return new Request(this)
            {
                Id = _nextId++,
                Command = command,
                Params = parameters ?? new JObject()
            };
        }

        public async Task<JObject> RequestAsync(string command, object parameters = null)
        {
            JObject args = null;
            if (parameters != null)
            {
                args = (JObject)
                        (parameters is JObject ? parameters :
                                                 JObject.FromObject(parameters));
            }
            return await Request(command, args).Send().AsyncResult();
        }
 
        public void SendWhenReady(JObject wsMessage)
        {
            OnceConnected(c =>
            {
                Log.DebugFormat("sending: {0}", wsMessage);
                _ws.SendAsync(wsMessage.ToString(), complete =>
                {
                    Log.DebugFormat("send complete: {0}", complete);
                });
            });
        }

        public Task<JObject> SubscribeAll()
        {
            return RequestAsync("subscribe", JObject.FromObject(new
            {
                streams = new[] {"ledger", "transactions", "server"}
            }));
        }

        public async Task<JObject> ConnectAndSubscribe()
        {
            return await Connect().SubscribeAll();
        }

        public void OnceConnected(Action<Connection> action)
        {
            if (Connected)
            {
                action(this);
                return;
            }

            this.ListenOnce(OnConnectedEvent, action);
        }

        private void OnClose(object sender, CloseEventArgs e)
        {
            Log.Debug(nameof(OnClose));
            Connected = false;
            OnDisconnected?.Invoke(this);
        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
            Log.DebugFormat("received: {0}", e.Data);
            var msg = JObject.Parse(e.Data);
            var type = msg["type"].Value<string>();

            if (type == "response")
            {
                var id = (int)msg["id"];
                if (_requests.ContainsKey(id))
                {
                    var request = _requests[id];
                    _requests.Remove(id);

                    var status = msg["status"].Value<string>();
                    if (status == "success")
                    {
                        request.NotifyResult(msg["result"] as JObject);
                    }
                    else
                    {
                        request.NotifyError(msg);
                    }
                }
                else
                {
                    Log.Warn("Request with no dict");
                }
            }
            else if (type == "transaction")
            {

            }
        }
        private void OnOpen(object sender, EventArgs e)
        {
            Log.Debug(nameof(OnOpen));
            Connected = true;
            OnConnected?.Invoke(this);
        }

        // ReSharper disable once InconsistentNaming
        public async Task<TxSubmission> Submit(string tx_blob)
        {
            var result = await RequestAsync("submit", new {tx_blob});
            var ngResult = EngineResult.Values.FromJson(result["engine_result"]);
            var txJson = (JObject)result["tx_json"];
            var hash = Hash256.FromJson(txJson["hash"]);
            return new TxSubmission(hash, ngResult, txJson, result);
        }

        internal void SendRequest(Request request)
        {
            var id = request.Id;
            var message = request.WsMessage();
            if (_requests.ContainsKey(id))
            {
                throw new InvalidOperationException(
                    $"Tried to send request twice: {message}");
            }
            _requests[id] = request;
            SendWhenReady(message);
        }

        public async Task<int> LedgerAccept()
        {
            var result = await RequestAsync("ledger_accept");
            return (int) result ["ledger_current_index"];
        }

        public void Disconnect()
        {
            _ws.Close();
        }

        public async Task<TxResult> RequestTxResult(Hash256 hash)
        {
            var req = await RequestAsync("tx",
                new
                {
                    transaction = hash.ToHex(),
                    ledger_index = "validated"
                });
            StObject tx = req;
            var metaJson = req["meta"];
            if (metaJson == null)
            {
                throw new TxNotFound("Tx found with no meta");
            }
            return new TxResult(tx, metaJson, (uint) req["ledger_index"]);
        }
    }
}