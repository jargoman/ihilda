using System;
using System.Linq;
using System.Text;
using RippleLibSharp.Transactions;

using Gtk;
using RippleLibSharp.Util;
using IhildaWallet.Util;
using RippleLibSharp.Binary;
using System.Collections.Generic;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CascadedBuyWidget : Gtk.Bin
	{
		public CascadedBuyWidget ()
		{
			this.Build ();


			//this.previewbutton.Clicked += onPreview;

			this.buybutton.Clicked += OnBuyClicked;

			this.numberentry.Changed += Entry_Changed;
			this.priceentry.Changed += Entry_Changed;
			this.pricemodentry.Changed += Entry_Changed;
			this.amountentry.Changed += Entry_Changed;
			this.amountmodentry.Changed += Entry_Changed;

			Label l = (Label)this.buybutton.Child;
			l.UseMarkup = true;


		}

		void Entry_Changed (object sender, EventArgs e)
		{
			CascadedOrderLogic cbo = GetCascadedOrderLogic (false);
			if (cbo == null) {
				ClearTotals ();
				return;
			}

			if (this.TradePairInstance == null) {
				return;
			}

			// Determine the orders so we can tally up the totals
			AutomatedOrder [] orders = DetermineOrders (cbo, this.TradePairInstance);

			if (orders == null) {
				return;
			}


			Decimal buyTotal = 0, sellTotal = 0, costTotal = 0, priceTotal = 0;


			for (int i = 0; i < orders.Length; i++) {
				if (orders [i] != null) {
					buyTotal += orders [i].TakerPays.amount;
					sellTotal += orders [i].TakerGets.amount;
				}


			}

			if (orders [0] != null) {
				if (orders [0].TakerPays.IsNative) {
					if (buyTotal != 0) {
						buyTotal /= 1000000;
					}
				}

				if (orders [0].TakerGets.IsNative) {
					if (sellTotal != 0) {
						sellTotal /= 1000000;
					}

				}
			}

			/*
			if (sellTotal != 0) {
				costTotal = buyTotal / sellTotal;
			}
			*/
			costTotal = (sellTotal == 0) ? 0 : buyTotal / sellTotal;
			priceTotal = (buyTotal == 0) ? 0 : sellTotal / buyTotal;

			StringBuilder sb = new StringBuilder ();

			sb.Append (buyTotal.ToString ());
			sb.Append (" ");
			sb.Append (orders [0].TakerPays.currency ?? "");
			sb.Append (" ");
			sb.Append (orders [0].TakerPays.issuer ?? "");

			this.buytotallabel.Text = sb.ToString ();

			sb.Clear ();

			sb.Append (sellTotal.ToString ());
			sb.Append (" ");
			sb.Append (orders [0].TakerGets.currency ?? "");
			sb.Append (" ");
			sb.Append (orders [0].TakerGets.issuer ?? "");

			this.selltotallabel.Text = sb.ToString ();
			sb.Clear ();

			this.costtotallabel.Text = costTotal.ToString ();
			this.pricetotallabel.Text = priceTotal.ToString ();
		}

		public void ClearTotals ()
		{
			this.buytotallabel.Text = "";
			this.selltotallabel.Text = "";
			this.pricetotallabel.Text = "";
			this.costtotallabel.Text = "";
		}

		#region determineOrders


		public AutomatedOrder [] DetermineOrders (CascadedOrderLogic caskbo, TradePair tradePair)
		{

			AutomatedOrder [] boa = new AutomatedOrder [caskbo.numberOfOrders];

			RippleWallet rw = _rippleWallet;
			if (rw == null) {
				return null;
			}

			uint number = caskbo.numberOfOrders;
			Decimal amount = caskbo.amount;
			Decimal price = caskbo.startingPrice;
			Decimal pricemod = caskbo.priceMod;
			Decimal amountmod = caskbo.amountMod;

			string mark = caskbo.markAs;
			string markmod = caskbo.markAsMod;

			MarkingIndexer markingIndexer = new MarkingIndexer ();


			if (mark.StartsWith ("[")) {
				if (!mark.EndsWith ("]")) {

					// todo warn
					return null;
				}

				mark = mark.Substring (1, mark.Length - 2);

				if (mark.Contains ("-")) {
					var spl = mark.Split ('-');
					if (spl.Length != 2) {
						return null;
					}
					int? min = RippleCurrency.ParseInt32 (spl [0]);
					int? max = RippleCurrency.ParseInt32 (spl [1]);
					if (min == null || max == null) {
						// todo
						return null;
					}
					if (max <= min) {
						return null;
					}


					for (int i = (int)min; i <= max; i++) {
						markingIndexer.marks.Add (i.ToString ());
					}
				}




			} else {


				var ml = mark.Split (',');
				markingIndexer.marks.AddRange (ml);

			}


			if (markmod.StartsWith ("[")) {
				if (!markmod.EndsWith ("]")) {
					
					// todo warn
					return null;
				}

				markmod = markmod.Substring (1, markmod.Length - 2);

				if (markmod.Contains ("-")) {
					var spl = markmod.Split ('-');
					if (spl.Length != 2) {
						return null;
					}

					int? min = RippleCurrency.ParseInt32 (spl [0]);
					int? max = RippleCurrency.ParseInt32 (spl [1]);
					if (min == null || max == null) {
						return null;
					}
					if (max <= min) {
						return null;
					}


					for (int i = (int)min; i <= max; i++) {


						markingIndexer.markmods.Add (i.ToString ());

					}

				}


				markmod = markmod.Substring (1, markmod.Length - 2);



			} else {
				var ml = markmod.Split (',');
				markingIndexer.markmods.AddRange (ml);
			}


			var botmarkings = markingIndexer.GetMarks ((int)number);

			if (pricemod > 1) {
				pricemod = 1 / pricemod;
			}

			for (uint i = 0; i < number; i++) {

				Decimal gets = amount * price;
				Decimal pays = amount;

				AutomatedOrder order = new AutomatedOrder {
					Account = rw.GetStoredReceiveAddress (),
					taker_pays = tradePair.Currency_Base.DeepCopy (),
					taker_gets = tradePair.Currency_Counter.DeepCopy (),
					BotMarking = botmarkings.ElementAt ((int)i)
				};


				if (order.TakerPays.IsNative) {
					pays = pays * 1000000m;
				}
				if (order.TakerGets.IsNative) {
					gets = gets * 1000000m;
				}

				order.taker_pays.amount = pays;
				order.taker_gets.amount = gets;

				boa [i] = order;

				price = price * pricemod;
				amount = amount * amountmod;




			}

			return boa;
		}
		#endregion




		public void SetOffer (Offer off)
		{
#if DEBUG
			string method_sig = clsstr + nameof (SetOffer) + DebugRippleLibSharp.right_parentheses + nameof (Offer) + DebugRippleLibSharp.space_char + nameof (off) + DebugRippleLibSharp.equals + DebugIhildaWallet.ToAssertString (off) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.CascadedBuyWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			if (off == null) {
#if DEBUG
				if (DebugIhildaWallet.CascadedBuyWidget) {
					Logging.WriteLog (method_sig + "off == null\n");
				}
#endif

				Application.Invoke (
					delegate {
						this.amountentry.Entry.Text = "";
						this.priceentry.Entry.Text = "";
					}

				);
				//this.maxcomboboxentry.Entry.Text = "";
				return;
			}

			if (off.taker_pays != null) {
#if DEBUG
				if (DebugIhildaWallet.CascadedBuyWidget) {
					Logging.WriteLog (method_sig + "off.TakerGets != null\n");
				}
#endif

				Decimal amount = off.taker_pays.amount;
				if (off.taker_pays.IsNative) {
					amount /= 1000000;
				}

				Application.Invoke (
					delegate {
						this.amountentry.Entry.Text = amount.ToString ();
						this.amountentry.Entry.ModifyBase (Gtk.StateType.Normal, new Gdk.Color (255, 55, 55));
					}
				);

				//this.amountentry.ModifyBg(Gtk.StateType.Normal, new Gdk.Color(255, 0, 0));
			}

			if (off.taker_gets != null) {
#if DEBUG
				if (DebugIhildaWallet.CascadedBuyWidget) {
					Logging.WriteLog (method_sig + "off.TakerPays != null\n");
				}
#endif
				this.priceentry.Entry.Text = off.taker_pays.GetNativeAdjustedPriceAt (off.taker_gets).ToString ();

			}


		}


		public void OnBuyClicked (object sender, EventArgs e)
		{

			CascadedOrderLogic cbo = GetCascadedOrderLogic (true);
			if (cbo == null) {
				return;
			}

			if (cbo.priceMod == 0) {

			}

			if (cbo.priceMod > 1) {

				string title = "Invalid price mod";
				string message = "A price mod of " + cbo.priceMod.ToString () + " will be automatically converted to " + (1 / cbo.priceMod).ToString () + ". Do you wish to continue?";

				bool answer = AreYouSure.AskQuestion (title, message);

				if (!answer) return; // return if they aren't sure lol
			}

			//

			AutomatedOrder [] offs = DetermineOrders (cbo, TradePairInstance);


			if (offs == null) {
				// TODO debug
				return;
			}

			if (offs.Length == 0) {
				return;
			}
			//RebuyDialog.doDialog(offs);
			LicenseType licenseT = Util.LicenseType.CASCADING;

			if (LeIceSense.IsLicenseExempt (offs [0].taker_gets) || LeIceSense.IsLicenseExempt (offs [0].taker_pays)) {
				licenseT = LicenseType.NONE;
			}
			OrderSubmitWindow win = new OrderSubmitWindow (_rippleWallet, licenseT);

			win.SetOrders (offs);
		}

		private CascadedOrderLogic GetCascadedOrderLogic (bool warnuser)
		{


			CascadedOrderLogic cbo = new CascadedOrderLogic ();

			string num = numberentry.Entry.Text;
			string pstr = priceentry.Entry.Text;
			string pms = pricemodentry.Entry.Text;
			string amst = amountentry.Entry.Text;
			string ammt = amountmodentry.Entry.Text;

			string mark = comboboxentry3.Entry.Text;
			string markmod = comboboxentry4.Entry.Text;

			uint? numberOfOrders = RippleCurrency.ParseUInt32 (num);
			if (numberOfOrders == null) {
				if (warnuser) {
					MessageDialog.ShowMessage ("Number of orders is formatted incorrectly \n");
				}
				return null;
			}



			Decimal? startingPrice = RippleCurrency.ParseDecimal (pstr);
			if (startingPrice == null) {
				if (warnuser) {
					MessageDialog.ShowMessage ("Starting price is formatted incorrectly \n");
				}
				return null;
			}
			Decimal? priceMod = RippleCurrency.ParseDecimal (pms);
			if (priceMod == null) {
				if (warnuser) {
					MessageDialog.ShowMessage ("Pricemod is formatted incorrectly \n");
				}
				return null;
			}
			Decimal? amount = RippleCurrency.ParseDecimal (amst);
			if (amount == null) {
				if (warnuser) {
					MessageDialog.ShowMessage ("Starting amount is formatted incorrectly \n");
				}
				return null;
			}
			Decimal? amountMod = RippleCurrency.ParseDecimal (ammt);
			if (amountMod == null) {
				if (warnuser) {
					MessageDialog.ShowMessage ("Amountmod is formatted incorrectly \n");
				}
				return null;
			}


			cbo.numberOfOrders = (uint)numberOfOrders;

			cbo.startingPrice = (Decimal)startingPrice;

			cbo.priceMod = (Decimal)priceMod;

			cbo.amount = (Decimal)amount;

			cbo.amountMod = (Decimal)amountMod;

			cbo.markAs = mark;
			cbo.markAsMod = markmod;

			return cbo;
		}


		public TradePair TradePairInstance {
			get {
				return _tradepair;
			}
			set {
				_tradepair = value;

				string b = _tradepair.Currency_Base.currency;
				Gtk.Application.Invoke (
					delegate {

						label10.Markup = "<b><u>Buy " + b + "</u></b>";

					}
				);
			}
		}




#if DEBUG
		private static readonly string clsstr = nameof (CascadedBuyWidget) + DebugRippleLibSharp.colon;
#endif
		/*
		public void printPreview (BuyOffer[] offers) {
			textview.Buffer.Clear ();
			StringBuilder sb = new StringBuilder ();
			foreach (Offer o in offers) {
				sb.Clear ();
				sb.Append ("Buy : ");
				sb.Append (o.taker_pays.ToString ());
				sb.Append ("For : ");
				sb.Append (o.taker_gets.ToString ());
				sb.AppendLine ();

				textview.Buffer.Text += sb.ToString ();
			}
		

		}*/

		public void SetRippleWallet (RippleWallet rippleWallet)
		{
			this._rippleWallet = rippleWallet;
		}

		private RippleWallet _rippleWallet = null;

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private TradePair _tradepair = null;

	}

	public class MarkingIndexer
	{

		public List<string> marks = new List<string> ();
		public List<string> markmods = new List<string> ();

		public IEnumerable<string> GetMarks (int num)
		{


			List<string> returnMe = new List<string> ();
			int index = 0;

			if (marks != null && marks.Any ()) {
				foreach (string m in marks) {
					if (index >= num) {
						return returnMe;
					}

					index++;
					returnMe.Add (m);
				}
			}


			if (markmods != null && markmods.Any ()) {
				while (index < num) {
					foreach (string m in markmods) {
						if (index >= num) {
							return returnMe;
						}

						if (m == "++" || m == "--") {

							if (index >= num) {
								return returnMe;
							}

							string last = returnMe.Last ();
							int? lastint = RippleCurrency.ParseInt32 (last);
							if (lastint == null) {
								return null;
							}

							if (m == "++") {
								returnMe.Add ((lastint.Value + 1).ToString ());
							} else {
								returnMe.Add ((lastint.Value - 1).ToString ());
							}
							index++;
							continue;
						}

						index++;
						returnMe.Add (m);

					}




				}
			}

			while (true) {
				if (index >= num) {
					return returnMe;
				}

				index++;
				returnMe.Add ("");

			}

			//return null;

		}
	}


	public class CascadedOrderLogic
	{
		public uint numberOfOrders = 0;
		public Decimal startingPrice = decimal.Zero;
		public Decimal priceMod = decimal.Zero;
		public Decimal amount = decimal.Zero;
		public Decimal amountMod = decimal.Zero;
		public string markAs = null;
		public string markAsMod = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant
	}


}

