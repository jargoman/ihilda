using System;

using System.Collections;

using System.Collections.Generic;

using Codeplex.Data;

using RippleLibSharp.Keys;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public class RuleManager
	{
		public RuleManager (RippleAddress account)
		{
			settingsPath = FileHelper.GetSettingsPath (account.ToString () + settingsFileName);
			RulesList = new List<OrderFilledRule> ();
			Account = account;
		}

		public string Account {
			get;
		}

		public List<OrderFilledRule> RulesList {
			get;
			set;
		}


		// Add rule to the list
		public void AddRule (OrderFilledRule val)
		{
			RulesList.Add (val);
		}

		// Remove rule to the list
		public bool RemoveRule (OrderFilledRule val)
		{
			return RulesList.Remove (val);
		}


		public void LoadRules (string path)
		{
			string str = FileHelper.GetJsonConf (path);
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

			OrderFilledRule [] rls = jsconf.Rules;

			//this.LastKnownLedger = jsconf.LastKnownLedger;

			RulesList.Clear ();

			foreach (OrderFilledRule or in rls) {
				RulesList.Add (or);
			}

		}

		public void ClearRules ()
		{
			RulesList.Clear ();
			this.SaveRules ();
		}

		public void LoadRules ()
		{
			LoadRules (settingsPath);
		}

		public bool SaveRules (string path)
		{

#if DEBUG
			string method_sig = clsstr + nameof (SaveRules) + DebugRippleLibSharp.both_parentheses;
#endif

			try {
				ConfStruct rs = new ConfStruct (RulesList);

				string conf = DynamicJson.Serialize (rs);
				return !string.IsNullOrWhiteSpace (conf) && FileHelper.SaveConfig(path, conf);
			} catch ( Exception e ) {

#if DEBUG
				if (DebugIhildaWallet.RuleManager) {
					Logging.ReportException (method_sig, e);
				}
#endif

				return false;
			}

		}



		public bool SaveRules ()
		{

			return SaveRules (settingsPath);
		}

		private class ConfStruct
		{
			public ConfStruct (ICollection<OrderFilledRule> rules)
			{

				int c = rules.Count;

				var it = rules.GetEnumerator ();



				this.Rules = new OrderFilledRule [rules.Count];

				for (int i = 0; i < rules.Count; i++) {
					it.MoveNext ();
					Rules [i] = it.Current;

				}

			}

			public ConfStruct ()
			{

			}


			public OrderFilledRule [] Rules {
				get;
				set;
			}

			public int LastKnownLedger {
				get;
				set;
			}
		}

		public OrderFilledRule RetreiveFromValues (
			string bought, 
			string sold, 
			string mark, 
			string markas, 
			string payless, 
			string getmore,
			string expayless,
			string exgetmore, // she wishes :D
			string speculate
		)
		{
			foreach (OrderFilledRule rule in RulesList) {

				if (rule == null) {
					return null;
				}

				if (bought != rule.BoughtCurrency.ToIssuerString ()) {
					continue;
				}

				if (sold != rule.SoldCurrency.ToIssuerString ()) {
					continue;
				}

				if (string.IsNullOrWhiteSpace (rule.Mark)) {
					if (!string.IsNullOrWhiteSpace(mark)) {
						continue;
					}
				} else {
					if (mark != rule.Mark) {
						continue;
					}
				}

				if (string.IsNullOrWhiteSpace (rule.MarkAs)) {
					if (!string.IsNullOrWhiteSpace (markas)) {
						continue;
					}
				} else {
					if (markas != rule.MarkAs) {
						continue;
					}
				}

				if (payless != rule.RefillMod.Pay_Less.ToString ()) {
					continue;
				}

				if (getmore !=rule.RefillMod.Get_More.ToString ()) {
					continue;
				}

				if (expayless != rule.RefillMod.Exp_Pay_Less.ToString ()) {
					continue;
				}

				if (exgetmore != rule.RefillMod.Exp_Get_More.ToString ()) {
					continue;
				}

				if (speculate != rule.RefillMod.Speculate.ToString ()) {
					continue;
				}

				return rule;

			}

			return null;
		}

		/*
		public uint? LastKnownLedger {
			get;
			set;
		}*/


		public uint? BotLedger {
			get;
			set;
		}


		public static OrderFilledRule SelectedRule {
			get;
			set;
		}

		public const string settingsFileName = "RuleSettings.jsn";

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		static string settingsPath = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

#if DEBUG
		private const string clsstr = nameof (RuleManager) + DebugRippleLibSharp.colon;
#endif

	}
}

