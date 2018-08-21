using System;
using System.Threading.Tasks;
using Gtk;
//using Mono.CSharp;
using System.IO;
using System.Text;
using System.Collections.Generic;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class Console : Gtk.Bin
	{
		public Console ()
		{

#if DEBUG
			string method_sig = clsstr + nameof (Console) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.Console) {

				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			Build ();

#if DEBUG
			if (DebugIhildaWallet.Console) {
				Logging.WriteLog (method_sig + DebugIhildaWallet.buildComp);
			}
#endif

			SetConsoleCombo ();


			this.DeleteEvent += OnDeleteEvent;

			this.consoleentry.Activated += this.OnConsoleentryActivated;
			this.sendbutton.Clicked += this.OnSendbuttonClicked;

			Logging.textview = this.consoleView;

			this.clearbutton.Clicked += delegate {
				this.consoleView.Buffer.Clear ();
			};

			this.hidebutton.Clicked += delegate {
				if (ConsoleWindow.currentInstance != null) {
					ConsoleWindow.currentInstance.Hide ();
				} else if (this.Parent != null) {
					this.Parent.Hide ();
				}
				//this.Hide();
			};

			this.scriptbutton.Clicked += delegate {

			};


			if (tokens != null) {
				history = new List<string> (tokens);
			} else {
				history = new List<string> ();
			}

			currentInstance = this;


		}



		static Console ()
		{
			settingsPath = FileHelper.GetSettingsPath (historyFileName);
		}

		public static Console currentInstance = null;

		public static readonly int max_lines_default = 50;

		public int max_lines = max_lines_default;

		public static String historyFileName = "consoleHistory.txt";
		public static String settingsPath = null;

		public static String [] tokens = null;

		List<String> history = new List<string> ();

		public void SetConsoleCombo ()
		{
			ListStore treemodel = ConsoleInterpreter.GetInterpretersListStore ();

			// TODO make sure the combobox isn't empty
			//OptionsWidget.com

			treemodel.GetIterFirst (out TreeIter treeIter);
			do {
				object o = treemodel.GetValue (treeIter, 0);
				if (!(o is String s)) {
					continue;
				}
			}

			while (treemodel.IterNext (ref treeIter));

			this.interpretercombobox.Model = treemodel;
			//this.interpretercombobox.
			//this.inter
		}

		private void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			//Thread thr = new Thread( new ThreadStart( goback ));
			//thr.Start();
			Logging.textview.Destroy ();
			Logging.textview = null;
			Goback ();
		}

		private void Goback ()
		{
			this.Hide ();
			if (WalletManagerWindow.currentInstance != null) {
				Task<WalletManagerWindow> task = WalletManagerWindow.InitGUI ();


			} 
				// todo debug
			WalletManagerWindow.currentInstance.Show ();

		}

		private void Send ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (Send) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.Console) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			// simple... send the raw imput to server

			String mess = this.consoleentry.Text;

			if (mess == null || mess.Equals ("")) {
#if DEBUG
				if (DebugIhildaWallet.Console) {
					Logging.WriteLog (method_sig + "entry text is empty\n");
				}
#endif
				return;
			}

			this.UpdateHistory (mess);

			ConsoleInterpreter.Interpret (mess);


		}

		public void OnSendbuttonClicked (object sender, EventArgs e)
		{
#if DEBUG
			if (DebugIhildaWallet.Console) {
				Logging.WriteLog ("Console : Event OnSendbuttonClicked fired\n");
			}
#endif

			Send ();

		}

		public void OnConsoleentryActivated (object sender, EventArgs e)
		{
#if DEBUG
			if (DebugIhildaWallet.Console) {
				Logging.WriteLog ("Console : Event OnConsoleentryActivated fired\n");
			}
#endif

			Send ();
		}

		private void UpdateHistory (String str)
		{
			this.history.Add (str);

			while (history.Count > max_lines) {
				history.RemoveAt (history.Count - 1);
			}

		}

		private void HistoryUp (object sender, EventArgs e)
		{
			historyIndex++;

			if (historyIndex >= history.Count) {
				historyIndex = history.Count - 1;
			}

			HistorySeek ();
		}

		private void HistoryDown (object sender, EventArgs e)
		{
			historyIndex--;

			if (historyIndex < 0) {
				historyIndex = 0;
			}

			HistorySeek ();
		}

		private void HistorySeek ()
		{
			if (history.Count <= 0) {
				return;
			}

			if (history.Count < historyIndex) {
				return;
			}

			String str = history [history.Count - historyIndex - 1];

			if (str == null) {
				return;
			}

			this.consoleentry.Text = str;
		}

		public static void LoadHistory ()
		{

			if (settingsPath == null) {
				// Todo debug
			}

			String his = FileHelper.GetJsonConf (settingsPath);

			if (his != null) {
				tokens = his.Split (new string [] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

			}



		}

		public void SaveHistory ()
		{
			StringBuilder str = new StringBuilder ();

			foreach (String s in history) {
				str.Append (s + "\n");
			}

			String st = str.ToString ();

			File.WriteAllText (settingsPath, st);
		}

		int historyIndex = 0;


		public static int max_screen_lines = 25;

		//public static get

#if DEBUG
		const string clsstr = nameof (Console) + DebugRippleLibSharp.colon;
#endif
	}
}

