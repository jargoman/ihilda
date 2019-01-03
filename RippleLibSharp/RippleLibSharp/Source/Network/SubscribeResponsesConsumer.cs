using System;
using RippleLibSharp.Result;
using RippleLibSharp.Util;

namespace RippleLibSharp.Network
{
	public class SubscribeResponsesConsumer
	
	{
		public SubscribeResponsesConsumer ()
		{
		}

		public void OnMessage (Object sender, SubscribeEventArgs args)
		{
			OnMessageReceived.Invoke (this, args);
		}

		public event EventHandler<SubscribeEventArgs> OnMessageReceived;


	}

	public class SubscribeEventArgs : EventArgs
	{
		/*
		public bool Success {
			get;
			set;
		}
		*/

		public Response<Json_Response> Response {
			get;
			set;
		}


#if DEBUG
		const string clsstr = nameof (SubscribeResponsesConsumer) + DebugRippleLibSharp.colon;
#endif

	}


}



