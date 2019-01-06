using System;
using System.Threading;
using System.Threading.Tasks;
using Codeplex.Data;
using RippleLibSharp.Result;
using RippleLibSharp.Network;
using RippleLibSharp.Binary;

using IhildaWallet.Networking;
using RippleLibSharp.Commands.Server;
using RippleLibSharp.Util;
using RippleLibSharp.Commands.Subscriptions;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ServerInfoWidget : Gtk.Bin
	{
		public ServerInfoWidget ()
		{
			#if DEBUG
			if (DebugIhildaWallet.ServerInfoWidget) {
				Logging.WriteLog (clsstr + "constructor\n");
			}
			#endif

			this.Build ();

			#if DEBUG
			if (DebugIhildaWallet.ServerInfoWidget) {
				Logging.WriteLog (clsstr + "build complete\n");
			}
#endif

			Task.Run ((System.Action)DoServerStateLoop);

		}


		public void SetServer (NetworkInterface ni) {
			//this.networkInterface = ni;
			if (ni == null) {
				return;
			}

			this.ClearUI ();
			//ni.onOpen += new NetworkInterface.connectEventHandler (connectionOpened);
			ni.onOpen += ConnectionOpened;





		}

		private void ConnectionOpened (object sender, EventArgs e)
		{
			//firstconnect = true;

			//refresh();

			//Thread thr = new Thread( new ThreadStart( refresh ));  // start a new thread because we don't want to call wait on the network thread otherwise the network will be waiting on itself and never return
			//thr.Start();

			Task.Run ((System.Action)Refresh);



		}


		public void DoServerStateLoop ()
		{

			// TODO two cansellation tokens

			while (true) {

				if (LedgerTracker.ServerStateEvent == null) {
					Thread.Sleep (10000);

				} else {
					LedgerTracker.ServerStateEvent.WaitOne ();
				}

				var stateEvent = LedgerTracker.ServerStateEv;
				if (stateEvent == null) {
					continue;
				}

				var basefee = stateEvent.base_fee;
				var loadfactor = stateEvent.load_factor;

				var tupe = LedgerTracker.GetFeeAndLastLedger (new CancellationToken());
				if (tupe == null) {
					continue;
				}
				var fee = tupe.Item1;
				if (fee == null) {
#if DEBUG
					fee = "null";
#else
					continue;

#endif 
				}
				//var ledger = tupe.Item2;

				Gtk.Application.Invoke ( delegate {
					load_factor_label_var.Text = loadfactor.ToString ();
					base_fee_label_var.Text = basefee.ToString ();
					transaction_fee_label_var.Text = fee;

				});
			}

		}

		private CancellationTokenSource tokenSource = null;
		public void Refresh ()
		{
			CancellationTokenSource ts = new CancellationTokenSource ();

			tokenSource?.Cancel ();
			tokenSource = ts;
			CancellationToken token = ts.Token;

			NetworkInterface ni = NetworkController.CurrentInterface;
			if (ni == null) {
				return;
			}
			//ConnectAttemptInfo connectAttemptInfo = ni.GetConnectAttemptInfo ();

			Task<Response<ServerInfoResult>> tsk = ServerInfo.GetResult (ni, token);
			tsk.Wait (token);

			if (token.IsCancellationRequested) {
				return;
			}

			Response<ServerInfoResult> resp = tsk.Result;


			ServerInfoResult sir = resp.result;
			SetServerInfo (sir.info);

			Gtk.Application.Invoke (
				delegate {
					//connectAttemptInfo.ServerUrl;
					var connectInfo = ni.GetConnectAttemptInfo ();

					string server = connectInfo?.ServerUrl;

					this.label26.Markup = Program.darkmode
						? "<b>Server Info : <span fgcolor=\"chartreuse\">" + (string)(server ?? "null") + "</span></b>"
						: "<b>Server Info : <span fgcolor=\"green\">" + (string)(server ?? "null") + "</span></b>";

				}
			);


		}

		public void Refresh_blocking ()
		{
#if DEBUG
			if (DebugIhildaWallet.ServerInfo) {
				Logging.WriteLog("Server Info : refresh_blocking\n");
			}
#endif

			_waitHandle.Reset();

			Refresh();

			_waitHandle.WaitOne(); // race condition if network fires before method return?

#if DEBUG
			if (DebugIhildaWallet.ServerInfo) {

				Logging.WriteLog("Server Info : refresh_continue\n");

			}
#endif
		}

		public void ClearUI () {
			Gtk.Application.Invoke (delegate {
				label26.Markup = "<b>Server Info</b>";
				this.build_version_label_var.Text = "";
				this.host_id_label_var.Text = "";
				this.complete_ledgers_label_var.Text = "";
				this.load_factor_label_var.Text = "";
				this.base_fee_label_var.Text = "";
				this.transaction_fee_label_var.Text = "";

			});
		}

		public void SetServerInfo (Info serverInfo) {
			
#if DEBUG
			string method_sig = clsstr + "setServerInfo : ";
			//if (firstconnect) { // faster if we only set these values once per connect
			if (DebugIhildaWallet.ServerInfo) {
				Logging.WriteLog(method_sig + "firstconnect true");
			}
#endif

			if (serverInfo == null) {
#if DEBUG
				if (DebugIhildaWallet.ServerInfo) {
					Logging.WriteLog(method_sig + "serverInfo == null");
				}
#endif
				this.ClearUI ();
				return;
			}

#if DEBUG
			if (DebugIhildaWallet.ServerInfo) {
				Logging.WriteLog(method_sig + "Server Info not null");
			}
#endif


			if (serverInfo.build_version!=null) {
				Gtk.Application.Invoke (delegate {
					this.build_version_label_var.Text = serverInfo?.build_version ?? ""; 
				});

			}

			if (serverInfo.hostid!=null) {
				Gtk.Application.Invoke (delegate {
					this.host_id_label_var.Text = serverInfo?.hostid ?? "";
				});

			}


			if (serverInfo.complete_ledgers!=null) {
				Gtk.Application.Invoke (delegate {
				this.complete_ledgers_label_var.Text = serverInfo?.complete_ledgers ?? "";
				});
			}



			double native_base_fee;
			if (serverInfo.validated_ledger != null) {
				

				//native_base_fee = new decimal (serverInfo.validated_ledger.base_fee_native);
				native_base_fee = serverInfo.validated_ledger.base_fee_xrp;
			}
			else {
				return;
			}

			ulong base_fee_drops = (ulong)(native_base_fee * 1000000);
			ulong transaction_fee = (ulong)((native_base_fee * 1000000) * serverInfo.load_factor);

			Gtk.Application.Invoke (delegate {  // exceptions in this thread are not caught but placing it here ensures it only runs if server returns valid info

				this.load_factor_label_var.Text = Base58.TruncateTrailingZerosFromString(serverInfo?.load_factor.ToString()) ?? "";
				this.base_fee_label_var.Text = base_fee_drops.ToString();
				this.transaction_fee_label_var.Text = transaction_fee.ToString();
			});


				



				
				



		}







		static EventWaitHandle _waitHandle = new ManualResetEvent( true );


#if DEBUG
		private static readonly string clsstr = nameof (ServerInfoWidget) + DebugRippleLibSharp.colon;
#endif

	}
}

