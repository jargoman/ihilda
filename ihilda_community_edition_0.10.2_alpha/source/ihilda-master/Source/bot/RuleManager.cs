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
			RulesList = new LinkedList<OrderFilledRule> ();
		}



		public LinkedList<OrderFilledRule> RulesList {
			get;
			set;
		}

		public void AddRule (OrderFilledRule val)
		{
			RulesList.AddLast (val);
		}

		public bool RemoveRule (OrderFilledRule val)
		{
			return RulesList.Remove (val);
		}


		public void LoadRules ()
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

			OrderFilledRule [] rls = jsconf.Rules;

			this.LastKnownLedger = jsconf.LastKnownLedger;

			RulesList.Clear ();

			foreach (OrderFilledRule or in rls) {
				RulesList.AddLast (or);
			}

		}

		public void SaveRules ()
		{

			ConfStruct rs = new ConfStruct (RulesList) {
				LastKnownLedger = this.LastKnownLedger
			};

			string conf = DynamicJson.Serialize (rs);

			FileHelper.SaveConfig (settingsPath, conf);

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

		public OrderFilledRule RetreiveFromValues (string b, string s, string pl, string gm)
		{
			foreach (OrderFilledRule rule in RulesList) {
				if (!b.Equals (rule.BoughtCurrency.ToIssuerString ())) {
					continue;
				}

				if (!s.Equals (rule.SoldCurrency.ToIssuerString ())) {
					continue;
				}

				if (!pl.Equals (rule.RefillMod.Pay_Less.ToString ())) {
					continue;
				}

				if (!gm.Equals (rule.RefillMod.Get_More.ToString ())) {
					continue;
				}

				return rule;

			}

			return null;
		}

		public int LastKnownLedger {
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

