using System;
using System.Text;

using System.Collections;

using System.Collections.Generic;

using Codeplex.Data;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public class TradePairManager
	{
		public TradePairManager ()
		//base ("IhildaWallet.TradePairManager")
		{
			//this.Build ();



			pairs = new Dictionary<string, TradePair> ();

			currentInstance = this;

			this.LoadTradePairs ();


		}

		static TradePairManager ()
		{

			settingsPath = FileHelper.GetSettingsPath (settingsFileName);
		}



		public void AddTradePair (TradePair tp)
		{

			string key = tp.DetermineKey ();

			if (pairs.ContainsKey(key)) {
				return;
			}
	    		
			pairs.Add (key, tp);
		}

		public void RemoveTradePair (TradePair tp)
		{
			string key = tp.DetermineKey ();

			if (!pairs.ContainsKey(key)) {
				return;
			}

			pairs.Remove (key);


		}

		public void LoadTradePairs ()
		{

			string str = FileHelper.GetJsonConf (settingsPath);
			if (str == null) {
				return;
			}
			ConfStruct jsconf = null;
			try {
				jsconf = DynamicJson.Parse (str);

			} catch (Exception e) {
				Logging.WriteLog (e.Message + e.StackTrace);
				return;
			}

			if (jsconf == null) {
				return;
			}

			TradePair [] tps = jsconf.TradePairs;



			pairs.Clear ();
			foreach (TradePair tp in tps) {
				string key = tp.DetermineKey ();

				if (key != null) {
					pairs.Add (key, tp);
				}
			}



		}


		public void SaveTradePairs ()
		{
			//FileHelper.saveConfig

			ConfStruct pears = new ConfStruct ( pairs.Values );

			string conf = DynamicJson.Serialize ( pears );

			FileHelper.SaveConfig ( settingsPath, conf );


		}


		private class ConfStruct
		{
			public ConfStruct (ICollection<TradePair> tp)
			{

				int c = tp.Count;

				var it = tp.GetEnumerator ();



				this.TradePairs = new TradePair [tp.Count];

				for (int i = 0; i < tp.Count; i++) {
					it.MoveNext ();
					TradePairs [i] = it.Current;

				}

			}

			public ConfStruct ()
			{

			}


			public TradePair [] TradePairs {
				get;
				set;
			}

		}

		public TradePair LookUpTradePair (String s)
		{

#if DEBUG

#endif

			if (!pairs.TryGetValue (s, out TradePair tp)) {
				// todo debug
			}
			return tp;
		}




		public static TradePair SelectedTradePair {
			get;
			set;
		}
		/*
		public void updateUI () {
			#if DEBUG
			if (Debug.TradePairManager) {
				Logging.writeLog(clsstr + "updateUI");
			}
			#endif
			if ( TradePairManagerWindow.currentInstance!=null) {
				TradePairManagerWindow.currentInstance.updateUI();
			}
		}
		*/

		public const string settingsFileName = "tradePairSettings.jsn";

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		static string settingsPath = null;


		public static TradePairManager currentInstance = null;


		public Dictionary<string, TradePair> pairs = null;

		//private object lockObject = new object();
#pragma warning restore RECS0122 // Initializing field with default value is redundant



#if DEBUG
		const string clsstr = nameof (TradePairManager) + DebugRippleLibSharp.colon;
#endif
	}
}

