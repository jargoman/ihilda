using System;
using System.Collections.Generic;
using Codeplex.Data;
using RippleLibSharp.Keys;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public class SentimentManager
	{
		public SentimentManager (RippleAddress account)
		{
			settingsPath = FileHelper.GetSettingsPath ( account.ToString () + settingsFileName );
			SentimentList = new List<Sentiment> ();
		}

		public List<Sentiment> SentimentList {
			get;
			set;
		}

		public Sentiment LookUpSentiment (string asset)
		{
			IEnumerable<Sentiment> sentiments = SentimentList;

			foreach (var v in sentiments) {
				if (v.Match == asset) {
					return v;
				}
			}

			return null;
		}

		public bool EditSentiment (Sentiment newSentiment)
		{
			if (newSentiment == null) {
				return false;
			}
			
			foreach (Sentiment sent in SentimentList) {
				if (newSentiment.Match == sent.Match) {
					sent.Rating = newSentiment.Rating;
					return true;
				}
			}

			return false;
		}

		public void AddSentiment (Sentiment val)
		{
			SentimentList.Add (val);
		}

		public bool RemoveSentiment (Sentiment val)
		{
			bool success = SentimentList.Remove (val);

			if (success) {
				SaveSentiments ();
			}

			return success;
		}

		public void LoadSentiments ()
		{
			string str = FileHelper.GetJsonConf (settingsPath);
			if (str == null) {
				return;
			}
			SentimentConfStruct jsconf = null;
			try {
				jsconf = DynamicJson.Parse (str);

			} catch (Exception e) {
				Logging.WriteLog (e.Message + e.StackTrace);
				return;
			}

			if (jsconf == null) {
				return;
			}

			Sentiment [] snts = jsconf.Sentiments;



			SentimentList.Clear ();

			foreach (Sentiment or in snts) {
				SentimentList.Add(or);
			}

		}

		public void SaveSentiments ()
		{

			SentimentConfStruct rs = new SentimentConfStruct (SentimentList);

			string conf = DynamicJson.Serialize (rs);

			FileHelper.SaveConfig (settingsPath, conf);

		}

		public void GetIndex (int index)
		{	
			
		}

		private class SentimentConfStruct
		{
			public SentimentConfStruct (ICollection<Sentiment> sentiments)
			{

				int c = sentiments.Count;

				var it = sentiments.GetEnumerator ();



				this.Sentiments = new Sentiment [sentiments.Count];

				for (int i = 0; i < sentiments.Count; i++) {
					it.MoveNext ();
					Sentiments [i] = it.Current;

				}

			}

			public SentimentConfStruct ()
			{

			}


			public Sentiment [] Sentiments {
				get;
				set;
			}


		}


		public const string settingsFileName = "SentimentSettings.jsn";

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		static string settingsPath = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

#if DEBUG
		private const string clsstr = nameof (SentimentManager) + DebugRippleLibSharp.colon;
#endif



	}
}
