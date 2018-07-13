using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using Gtk;

using RippleLibSharp.Transactions;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public partial class PairPopup : Gtk.Dialog
	{
		public PairPopup ()
		{
			#if DEBUG
			String method_sig = clsstr + nameof (PairPopup) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PairPopup) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
			#endif

			this.Build ();

			PopulatePairsList();

			if (pairslist != null) {
				this.SetSuggestedPairs(pairslist);

			}
		}






		private static void PopulatePairsList ()
		{
			#if DEBUG
			string method_sig = clsstr + nameof (PopulatePairsList) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PairPopup) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
			#endif

			if (pairslist != null) { // it's static so another popup may have initialized it already
				#if DEBUG
				if (DebugIhildaWallet.PairPopup) {
					Logging.WriteLog (method_sig + "pairslist != null, returning\n");
				}
				#endif
				return;
			}

			List<TradePair> pairs = new List<TradePair>();

			foreach (String s in defaultSuggestions) {
				TradePair t = TradePair.FromString(s);
				pairs.Add(t);
			}

			//var pairss = from defaultSuggestions


			String fave = BalanceTabOptionsWidget.actual_values[0];
			if (fave == null || fave.Equals ("")) {
				pairslist = pairs;
				return;
			}

			var vals = (from String s in BalanceTabOptionsWidget.actual_values
			            where s != null && !fave.Equals (s) && !s.Equals ("")
			            select s + "//" + fave)
				.Except (defaultSuggestions);
				//.select (TradePair.fromString(s));
			



			foreach (String strin in vals) {
				/*
				if ( fave.Equals(s) || s == null || s.Equals("")) {
				
					continue;

				}
				*/


				//String strin = s + "/" + fave;

				/*
				foreach ( String defa in defaultSuggestions) {
				
					if (defa.Equals(strin)) {
					
						continue; // continue BOTH foreach !! 


	
					}
				
				}*/
				

				TradePair tp = TradePair.FromString(strin);

				if (tp == null) {
					#if DEBUG
					if (DebugIhildaWallet.PairPopup) {
						Logging.WriteLog (method_sig + "tp == null\n");
					}
					#endif
				
					return;

				}

				pairs.Add(tp);
				continue;

			}
			pairslist = pairs;
		}

		public void SetSuggestedPairs (List<TradePair> pairs)
		{

			ListStore store = new ListStore(typeof(string));

			foreach (TradePair tp in pairs) {
				store.AppendValues(tp.ToHumanString()); 
			}

			store.GetIterFirst (out TreeIter iter);
			this.comboboxentry1.Model = store;

			this.comboboxentry1.SetActiveIter(iter);



		}

		public TradePair GetSelected ()
		{
			#if DEBUG
			String method_sig = clsstr + nameof (GetSelected) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PairPopup) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
			#endif

			String s = this.comboboxentry1.ActiveText;
			TradePair tp = TradePair.FromString(s);

			#if DEBUG
			if (DebugIhildaWallet.PairPopup) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.returning + DebugIhildaWallet.ToAssertString(tp));
			}
			#endif
			return tp;

		}

		public static TradePair DoPopup ()
		{
			
			#if DEBUG
			String method_sig = clsstr + nameof (DoPopup) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PairPopup) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
			#endif
			PairPopup pp = new PairPopup();
			TradePair tp = null;

			while (true) {
				ResponseType resp = (ResponseType) pp.Run();
				pp.Hide();

				if (resp != ResponseType.Ok) {
					#if DEBUG
					if (DebugIhildaWallet.PairPopup) {
						Logging.WriteLog(method_sig + "resp != ResponseType.OK, breaking");
					}
					#endif
					break;
				}

				tp = pp.GetSelected();
				if (tp == null) {
					MessageDialog.ShowMessage ("The tradepair you entered was invalid");

					continue;
				}
				break;

			}

			pp.Destroy();

			#if DEBUG
			if (DebugIhildaWallet.PairPopup) {
				Logging.WriteLog(method_sig + "returning, " + DebugIhildaWallet.ToAssertString(tp) );
			}
			#endif
			return tp;

		}

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public static List<TradePair> pairslist = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		public static readonly String[] defaultSuggestions = { RippleCurrency.NativeCurrency + "/USD", RippleCurrency.NativeCurrency + "/CAD", RippleCurrency.NativeCurrency + "/ICE" };

		#if DEBUG
		private static readonly string clsstr = nameof (PairPopup) + DebugRippleLibSharp.colon;
		#endif
	}
}

