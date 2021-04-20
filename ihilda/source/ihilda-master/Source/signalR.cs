using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace IhildaWallet
{
	public class signalR
	{
		public signalR (string HubName)
		{
			hubConnection = new HubConnection (ProgramVariables.webUrl);
			hubProxy = hubConnection.CreateHubProxy (HubName);
			ServicePointManager.DefaultConnectionLimit = 10;

		}

		public signalR (string HubName, Cookie authCookie) : this (HubName)
		{
			hubConnection.CookieContainer = new CookieContainer ();
			hubConnection.CookieContainer.Add (authCookie);
		}

		public HubConnection hubConnection { get; internal set; }

		public IHubProxy hubProxy { get; set; }

		public void Start () {
			var task = hubConnection.Start ();
			task.Wait ();
		}


	}


}
