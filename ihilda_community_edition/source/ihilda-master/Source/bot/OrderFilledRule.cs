using System;
using RippleLibSharp.Transactions;

namespace IhildaWallet
{
	public class OrderFilledRule
	{

		/*
		public OrderFilledRule ()
		{


		}
		*/

		public string Account {
			get;
			set;
		}

		public bool IsActive {
			get;
			set;
		}

		public RippleCurrency BoughtCurrency {
			get;
			set;
		}

		public RippleCurrency SoldCurrency {
			get;
			set;
		}

		public string Mark {
			get;
			set;
		}


		public string MarkAs {
			get;
			set;
		}


		public ProfitStrategy RefillMod {
			get;
			set;
		}

		public string GetNewMark (string old)
		{
			string markas = MarkAs;
			if (string.IsNullOrWhiteSpace(markas)) {
				return "";
			}

			bool valid;
			if (MarkAsCommand.ADDONE == markas) {
				valid = int.TryParse (old, out int aint);
				if (!valid) {
					throw new InvalidCastException ("Invalid cast " + old ?? "" + " can not apply command " + MarkAsCommand.ADDONE );
				}

				return (++aint).ToString();
			}

			if (MarkAsCommand.SUBTRACTONE == markas) {
				valid = int.TryParse (old, out int aint);
				if (!valid) {
					throw new InvalidCastException ("Invalid cast " + old ?? "" + " can not apply command " + MarkAsCommand.SUBTRACT);
				}

				return (--aint).ToString ();
			}

			if ( markas.StartsWith( MarkAsCommand.ADD ) ) {
				markas = markas.Substring (2);
				valid = int.TryParse (markas, out int matchint);
				if (!valid) {
					throw new InvalidCastException ("Invalid cast " + markas ?? "" + " can not apply command " + MarkAsCommand.ADD);
				}

				valid = int.TryParse (old, out int oldint);
				if (!valid) {
					throw new InvalidCastException ("Invalid cast " + old ?? "" + " can not apply command " + MarkAsCommand.ADD);
				}

				return (matchint + oldint).ToString();
			}

			if (markas.StartsWith (MarkAsCommand.SUBTRACT)) {
				markas = markas.Substring (2);
				valid = int.TryParse (markas, out int matchint);
				if (!valid) {
					throw new InvalidCastException ("Invalid cast " + markas ?? "" + " can not apply command " + MarkAsCommand.SUBTRACT);
				}

				valid = int.TryParse (old, out int oldint);
				if (!valid) {
					throw new InvalidCastException ("Invalid cast " + old ?? "" + " can not apply command " + MarkAsCommand.SUBTRACT);
				}

				return (oldint - matchint).ToString();
			}

			if (markas.StartsWith (MarkAsCommand.MULTIPLY)) {
				markas = markas.Substring (2);
				valid = int.TryParse (markas, out int matchint);
				if (!valid) {
					throw new InvalidCastException ("Invalid cast " + markas ?? "" + " can not apply command " + MarkAsCommand.MULTIPLY);
				}

				valid = int.TryParse (old, out int oldint);
				if (!valid) {
					throw new InvalidCastException ("Invalid cast " + old ?? "" + " can not apply command " + MarkAsCommand.MULTIPLY);
				}

				return (oldint * matchint).ToString ();
			}

			if (markas.StartsWith (MarkAsCommand.DIVIDE)) {
				markas = markas.Substring (2);
				valid = int.TryParse (markas, out int matchint);
				if (!valid) {
					throw new InvalidCastException ("Invalid cast " + markas ?? "" + " can not apply command " + MarkAsCommand.DIVIDE);
				}

				valid = int.TryParse (old, out int oldint);
				if (!valid) {
					throw new InvalidCastException ("Invalid cast " + old ?? "" + " can not apply command " + MarkAsCommand.DIVIDE);
				}

				if (oldint == 0) {
					return 0.ToString();
				}

				return (oldint / matchint).ToString ();
			}

			return markas;


		}


		private bool GetMarkMatch (string marking)
		{
			string matchRule = Mark;
			if (string.IsNullOrWhiteSpace (matchRule)) {
				return string.IsNullOrWhiteSpace (marking);
			}

			if (matchRule.StartsWith (MatchMarkModifer.NOT)) {
				matchRule = matchRule.TrimStart (MatchMarkModifer.NOT.ToCharArray ());
				return !_GetMarkMatch (marking, matchRule);
			} else {
				return _GetMarkMatch (marking, matchRule);
			}
		}

		private bool _GetMarkMatch (string marking, string matchRule) {


			if (string.IsNullOrWhiteSpace (matchRule)) {
				return string.IsNullOrWhiteSpace (marking);
			}

			if ( matchRule.StartsWith("[") ) {
				if (!matchRule.EndsWith("]")) {
					throw new ArgumentException ("Invalid range. Missing closing \"]\"", nameof (Mark));
				}


				// It's a range. 

				matchRule = matchRule.Substring (1, matchRule.Length - 2);

				if (matchRule.Contains("-")) {
					string [] banana = matchRule.Split ('-');
					if (banana.Length != 2) {
						throw new ArgumentException ("Invalid range. Must be of format [LowInt-HighInt]", nameof (Mark));
					}

					bool valid = int.TryParse (banana [0], out int low);
					if (!valid) {
						throw new ArgumentException ("Invalid range. Left value is not valid integer. Range must be of format [LowInt-HighInt]", nameof (Mark));
					}

					valid = int.TryParse (banana [1], out int high);
					if (!valid) {
						throw new ArgumentException ("Invalid range. Right value is not valid integer. Range must be of format [LowInt-HighInt]", nameof (Mark));
					}

					valid = int.TryParse (marking, out int markInt);
					if (!valid) {

						// if it doesn't match don't throw an exception return false. Not matching the rule is not a bug. 
						return false;
						//throw new InvalidCastException ("Can not convert " + marking ?? "null" + " to integer for numeric comparison");
					}

					if (!(low < high)) {
						throw new ArgumentException ("Invalid range. Left value must be less than right value. format [LowInt-HighInt]");
					}

					return markInt >= low && markInt <= high;

				}

				if (matchRule.Contains (",")) {
					string [] banana = matchRule.Split (',');
					if (banana.Length < 1) {
						throw new ArgumentException ("Invalid collection. Must be of format [item,item]");
					}

					foreach (string str in banana) {
						if (str == marking) {
							return true;
						}
					}

					return false;
				}
			}


			if (matchRule.StartsWith(MatchMarkModifer.GREATER)) {
				matchRule = matchRule.TrimStart (MatchMarkModifer.GREATER.ToCharArray());
				bool valid = int.TryParse (matchRule, out int mint);
				if (!valid) {
					throw new InvalidCastException ("Invadic cast string matchRule " + matchRule ?? "null");
				}

				valid = int.TryParse (marking, out int markingInt);
				if (!valid) {
					// instead of throwing an exeception we don't trigger the rule
					return false;
				}

				return markingInt > mint;

			}

			if (matchRule.StartsWith (MatchMarkModifer.SMALLER)) {
				matchRule = matchRule.TrimStart (MatchMarkModifer.SMALLER.ToCharArray ());
				bool valid = int.TryParse (matchRule, out int mint);
				if (!valid) {
					throw new InvalidCastException ("Invadic cast string matchRule " + matchRule ?? "null");
				}

				valid = int.TryParse (marking, out int markingInt);
				if (!valid) {
					// instead of throwing an exeception we don't trigger the rule
					return false;
				}


				return markingInt < mint;
			}

			if (matchRule.StartsWith (MatchMarkModifer.EQUALS)) {
				matchRule = matchRule.TrimStart (MatchMarkModifer.EQUALS.ToCharArray ());

				return matchRule == marking;
			}



			return matchRule == marking;
		}


		public bool DetermineMatch (AutomatedOrder o) {
			
			if (o == null) {
				throw new NullReferenceException ("OrderFilledRule can not match with offer o = null");
			}

			if (string.IsNullOrWhiteSpace (Mark)) {
				if (!string.IsNullOrWhiteSpace (o.BotMarking)) {
					return false;
				}
			} else {
				bool markMatches = GetMarkMatch (o.BotMarking);

				if (!markMatches) {
					return false;
				}

			}



			if (BoughtCurrency != null) {

				if ( BoughtCurrency.currency != null && !BoughtCurrency.currency.Trim().Equals("")) {
					if (!BoughtCurrency.currency.Equals(o.TakerPays.currency)) {
						return false;
					}
				}

				if ( BoughtCurrency.issuer != null && !BoughtCurrency.issuer.Trim().Equals("")) {
					if (!BoughtCurrency.issuer.Equals(o.TakerPays.issuer)) {
						return false;
					}

				}

			}

			if (SoldCurrency != null) {
				if (SoldCurrency.currency != null && !SoldCurrency.currency.Trim().Equals("")) {
					if (!SoldCurrency.currency.Equals(o.TakerGets.currency)) {
						return false;
					}
				}

				if (SoldCurrency.issuer != null && !SoldCurrency.issuer.Trim().Equals("")) {
					if (!SoldCurrency.issuer.Equals(o.TakerGets.issuer)) {
						return false;
					}
				}
			}


			return true;

		}

	}
}

