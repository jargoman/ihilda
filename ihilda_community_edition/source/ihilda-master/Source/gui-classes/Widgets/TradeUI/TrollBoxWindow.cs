using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace IhildaWallet
{
	public partial class TrollBoxWindow : Gtk.Window
	{
		public TrollBoxWindow (string cbase, string ccounter) :
				base (Gtk.WindowType.Toplevel)
		{
			this.Build ();

			this.basecur = cbase;
			this.countercur = ccounter;

			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append ("TrollBox for ");
			stringBuilder.Append (cbase);
			stringBuilder.Append ("/");
			stringBuilder.Append (ccounter);
			this.label1.Text = stringBuilder.ToString ();


			var ar = new string[] {basecur, countercur };

			var sorted = ar.OrderBy (x=>x);
			stringBuilder.Clear ();
			foreach (var i in sorted) {
				stringBuilder.Append (i);
			}

			Connect ();

			group = stringBuilder.ToString ();
			sendbutton.Clicked += (sender, e) => {

				string text = entry1.Text;
				if (string.IsNullOrEmpty(text)) {
					//TODO
				}

				ChatMessage chat = new ChatMessage () {
					Message = text,
		    			Base = this.basecur,
					Counter = this.countercur
				};

				sigRConnect.hubProxy.Invoke ("OnMessage", chat, group);
			};
	    		

		}

		public void Connect ()
		{



			Task t = Task.Run ( delegate {


				var cookie = ProgramVariables.winter.authCookie;



				sigRConnect = new signalR (hubname, cookie);

				//sigRConnect = new signalR (hubname);

				//sigRConnect.hubConnection.Closed += () => {
				//	textview1.Buffer.Text += "Connection closed \n";
				//};
				

				sigRConnect.hubProxy.On<ChatMessage, string> (
					"OnMessage",
					(chatmessage, group) => {
						onChatMessageHandler?.Invoke (chatmessage);
					});


				onChatMessageHandler += (ChatMessage chatter) => {
					Logging.WriteBoth (chatter.Message);

					StringBuilder sb = new StringBuilder ();
					sb.Append (chatter?.UserName != null ? chatter.UserName : "null username");
					sb.Append (":");
					sb.Append (chatter?.Message != null ? chatter.Message : "null message");
					sb.AppendLine ();

					Gtk.Application.Invoke (delegate {

						textview1.Buffer.Text += sb.ToString ();

					});

				};

				try {
					sigRConnect.Start ();

					sigRConnect.hubProxy.Invoke ("JoinGroup", group);

				} catch (Exception e) {
					// TODO
					Logging.WriteLog ("s");
				}

				Gtk.Application.Invoke ( delegate {

					textview1.Buffer.Text += "Connected\n";
				});

			});

		}

		public delegate void OnChatMessage (ChatMessage chat);
		public static OnChatMessage onChatMessageHandler;

		private signalR sigRConnect { get; set; }

		private string basecur { get; set; }
		private string countercur { get; set; }
		private string group { get; set; }
		private string hubname = "MessageHub";
	}

	public class ChatMessage
	{
		public int Id { get; set; }
		public string Base { get; set; }
		public string Counter { get; set; }
		public string UserName { get; set; }
		public string Message { get; set; }
		public DateTime Time { get; set; }
	}

}
