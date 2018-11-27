using System;
using System.Text;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ConnectedDisplayWidget : Gtk.Bin
	{
		public ConnectedDisplayWidget ()
		{
			this.Build ();

			this.connectStatusLabel.UseMarkup = true;

			if (!Program.network) {
				this.connectStatusLabel.Markup = "<span foreground=\"red\">Networking Disabled</span>";
			}
		}

		public void SetConnected (string serverUrl)
		{

			#if DEBUG
			string method_sig =  clsstr + nameof (SetConnected) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog( method_sig + DebugRippleLibSharp.beginn );
			}
#endif

			if (serverUrl == null) {
				SetDisConnected ();
				return;
			}

			Gtk.Application.Invoke ( delegate {
				#if DEBUG
				if (DebugIhildaWallet.ConnectedDisplayWidget) {
					Logging.WriteLog (clsstr + DebugIhildaWallet.gtkInvoke);
				}
#endif
				//this.connectStatusLabel.ma

				StringBuilder stringBuilder = new StringBuilder ();
				stringBuilder.Append ("<span foreground=\"");
				stringBuilder.Append (Program.darkmode ? "chartreuse" : "green");
				stringBuilder.Append ("\"><big><b>Connected</b></big></span>");
				this.connectStatusLabel.Markup = stringBuilder.ToString();
				if (Program.showPopUps) {
					this.TooltipMarkup = serverUrl;
				}
			}
			);
		}


		public void SetDisConnected ()	{
#if DEBUG
			string method_sig = clsstr + nameof (SetDisConnected) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.ConnectedDisplayWidget) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}
			#endif
			Gtk.Application.Invoke ( delegate {
				#if DEBUG
				if (DebugIhildaWallet.NetworkSettings) {
					Logging.WriteLog (method_sig + DebugIhildaWallet.gtkInvoke);
				}
#endif

				if (!Program.network) {
					this.connectStatusLabel.Markup = "<span foreground=\"red\">Networking Disabled</span>";
					return;
				}

				this.connectStatusLabel.Markup = "<span foreground=\"red\">Disconnected</span>";
				this.TooltipMarkup = "";

			});

		}

		#if DEBUG
		private const string clsstr = nameof (ConnectedDisplayWidget) + DebugRippleLibSharp.colon;
		#endif
	}
}

