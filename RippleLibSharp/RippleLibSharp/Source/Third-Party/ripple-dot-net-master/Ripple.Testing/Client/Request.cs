using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Ripple.Testing.Client
{
    internal class Request
    {
        public string Command;
        public int Id;
        public JObject Params;
        private readonly Connection _connection;
        private readonly TaskCompletionSource<JObject> _tcs;

        public Request(Connection connection)
        {
            _connection = connection;
            _tcs = new TaskCompletionSource<JObject>();
            OnResult(_tcs.SetResult);
        }

        public delegate void ResultHandler(JObject result);

        protected event ResultHandler OnResultEvent;

        public void NotifyResult(JObject jObject)
        {
            InvokeOnResult(jObject);
        }

        public Request OnResult(ResultHandler handler)
        {
            if (handler != null)
            {
                OnResultEvent += handler;
            }
            return this;
        }

        public async Task<JObject> AsyncResult()
        {
            return await _tcs.Task;
        }

        public Request Send()
        {
            _connection.SendRequest(this);
            return this;
        }

        public JObject WsMessage()
        {
            var cloned = (JObject) Params.DeepClone();
            cloned["id"] = Id;
            cloned["command"] = Command;
            return cloned;
        }
        protected virtual void InvokeOnResult(JObject response)
        {
            OnResultEvent?.Invoke(response);
        }

        public void NotifyError(JObject response)
        {
            // var error = new InvalidOperationException(response.ToString());
            //throw error;
            _tcs.SetException(new RequestUnsuccessfulException(response.ToString()));
        }
    }

    internal class RequestUnsuccessfulException : Exception
    {
        public RequestUnsuccessfulException(string message) : base(message)
        {
        }
    }
}