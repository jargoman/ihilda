﻿using System;
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
		}

		public void SetConnected ()
		{

			#if DEBUG
			string method_sig =  clsstr + nameof (SetConnected) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.NetworkSettings) {
				Logging.WriteLog( method_sig + DebugRippleLibSharp.beginn );
			}
			#endif

			Gtk.Application.Invoke ( delegate {
				#if DEBUG
				if (DebugIhildaWallet.ConnectedDisplayWidget) {
					Logging.WriteLog (clsstr + DebugIhildaWallet.gtkInvoke);
				}	
				#endif
				//this.connectStatusLabel.ma
				this.connectStatusLabel.Markup = "<span foreground=\"green\">Connected</span>";	

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

				this.connectStatusLabel.Markup = "<span foreground=\"red\">Disconnected</span>";	


			});

		}

		#if DEBUG
		private const string clsstr = nameof (ConnectedDisplayWidget) + DebugRippleLibSharp.colon;
		#endif
	}
}
