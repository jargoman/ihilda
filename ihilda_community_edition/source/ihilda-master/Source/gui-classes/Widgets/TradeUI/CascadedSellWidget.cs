using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using IhildaWallet.Networking;
using IhildaWallet.Util;
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CascadedSellWidget : Gtk.Bin
	{
		public CascadedSellWidget ()
		{
			this.Build ();

			this.sellbutton.Clicked += OnPreview;

			this.numberentry.Changed += Numberentry_Changed;
			this.priceentry.Changed += Numberentry_Changed;
			this.pricemodentry.Changed += Numberentry_Changed;
			this.amountentry.Changed += Numberentry_Changed;
			this.amountmodentry.Changed += Numberentry_Changed;

			Label l = (Label)this.sellbutton.Child;
			l.UseMarkup = true;

			button114.Clicked += PercentageClicked;
			button115.Clicked += PercentageClicked;
			button111.Clicked += PercentageClicked;
			button112.Clicked += PercentageClicked;

			button123.Clicked += PercentageClicked;
			button113.Clicked += PercentageClicked;
			button119.Clicked += PercentageClicked;

			button116.Clicked += PercentageClicked;

			button120.Clicked += PercentageClicked;

			button117.Clicked += PercentageClicked;

			button118.Clicked += PercentageClicked;

			button121.Clicked += PercentageClicked;

			button122.Clicked += PercentageClicked;

			hscale2.ValueChanged += Hscale2_ValueChanged;

		}

		public void SetAmount (Decimal amount)
		{
			amountentry.Entry.Text = amount.ToString ();
		}


		public void SetPrice (Decimal price)
		{
			priceentry.Entry.Text = price.ToString ();
		}

		public void SetAmountMax ()
		{
			this.hscale2.Value = 100.0;
		}


		void Hscale2_ValueChanged (object sender, EventArgs e)
		{
			string acc = _rippleWallet.Account;

			NetworkInterface ni = NetworkController.GetNetworkInterfaceGuiThread ();

			CancellationTokenSource tokenSource = new CancellationTokenSource ();
			CancellationToken token = tokenSource.Token;

			double val = hscale2.Value;
			if (!_tradepair.Currency_Base.IsNative) {
				var cur = AccountLines.GetBalanceForIssuer
				(
					_tradepair.Currency_Base.currency,
					_tradepair.Currency_Base.issuer,
					acc,
					ni,
					token
				);

				double bal = (double)cur.amount;

				double res = bal * val / 100;

				amountentry.Entry.Text = res.ToString ();
			} else {

				Task<Response<AccountInfoResult>> task =
					AccountInfo.GetResult (acc, ni, token);

				task.Wait (token);

				Response<AccountInfoResult> resp = task.Result;
				AccountInfoResult res = resp.result;

				var reserve = res.GetReserveRequirements (ni, token);

				RippleCurrency rippleCurrency = new RippleCurrency (res.account_data.Balance);

				double bal = (double)(rippleCurrency.amount - reserve.amount) / 1000000 * val / 100;

				amountentry.Entry.Text = bal.ToString ();
			}
		}

		void PercentageClicked (object sender, EventArgs e)
		{
			if (sender is Button b) {
				string s = b?.Label.TrimEnd ('%');
				double d = Convert.ToDouble (s);

				hscale2.Value = d;
			}
		}


		public void SetOffer (Offer off) {
			#if DEBUG
			string method_sig = clsstr + nameof (SetOffer) + DebugRippleLibSharp.left_parentheses + nameof (Offer) + DebugRippleLibSharp.space_char  + nameof (off) + DebugRippleLibSharp.equals + DebugIhildaWallet.ToAssertString(off) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.CascadedSellWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
			#endif

			if (off == null) {
				#if DEBUG
				if (DebugIhildaWallet.CascadedSellWidget) {
					Logging.WriteLog (method_sig + nameof (off) + " == null\n");
				}
				#endif

				Application.Invoke (
					delegate {
						this.amountentry.Entry.Text = "";
						this.priceentry.Entry.Text = "";
					}
				);

				//this.maxcoentry.Entry.Text = "";
				return;
			}

			if (off.TakerGets != null) {
				#if DEBUG
				if (DebugIhildaWallet.CascadedSellWidget) {
					Logging.WriteLog (method_sig + nameof (off.TakerGets) + " != null\n");
				}
				#endif

				Decimal amount = off.taker_gets.amount;
				if (off.taker_gets.IsNative) {
					amount /= 1000000;
				}

				Application.Invoke (
					delegate {
						this.amountentry.Entry.Text = amount.ToString ();
					}
				);

			}

			if (off.taker_pays != null) {
				#if DEBUG
				if (DebugIhildaWallet.CascadedSellWidget) {

				}
				#endif

				string tex = off.taker_gets.GetNativeAdjustedPriceAt (off.TakerPays).ToString();

				Application.Invoke (
					delegate {
						this.priceentry.Entry.Text = tex;
					}
				);

			}
		}

		void Numberentry_Changed (object sender, EventArgs e)
		{
			
			CascadedOrderLogic cbo = GetCascadedOrderLogic (false);
			if (cbo == null) {
				ClearTotals ();
				return;
			}

			if (this.TradePairInstance == null) {
				return;
			}

			AutomatedOrder[] orders = DetermineOrders (cbo, this.TradePairInstance);
			if (orders == null) {
				return;
			}

			if (orders.Length == 0) {
				return;
			}

			if (orders[0]?.TakerGets == null | orders[0].TakerPays == null) {
				return;
			}

			Decimal buyTotal = 0, sellTotal = 0, costTotal = 0, priceTotal = 0;

			for ( int i = 0; i < orders.Length; i++ ) {
				buyTotal += orders [i].TakerPays.amount;
				sellTotal += orders [i].taker_gets.amount;
			}



			if (orders[0].TakerPays.IsNative) {
				if (buyTotal != 0) {
					buyTotal /= 1000000;
				}
			}

			if (orders[0].TakerGets.IsNative) {
				if (sellTotal != 0) {
					sellTotal /= 1000000;
				}
			}

			costTotal = sellTotal == 0 ? 0 : buyTotal / sellTotal;
			priceTotal = buyTotal == 0 ? 0 : sellTotal / buyTotal;

			StringBuilder sb = new StringBuilder ();

			sb.Append (buyTotal.ToString());
			sb.Append (" ");
			sb.Append (orders [0].TakerPays.currency ?? "");
			sb.Append (" ");
			sb.Append (orders [0].TakerPays.issuer ?? "");

			this.buytotallabel.Text = sb.ToString ();

			sb.Clear ();

			sb.Append (sellTotal.ToString());
			sb.Append (" ");
			sb.Append (orders [0].TakerGets.currency ?? "");
			sb.Append (" ");
			sb.Append (orders [0].TakerGets.issuer ?? "");

			this.selltotallabel.Text = sb.ToString ();

			sb.Clear ();


			this.costtotallabel.Text = costTotal.ToString ();
			this.pricetotallabel.Text = priceTotal.ToString ();
		}

		public void ClearTotals () {
			
			this.buytotallabel.Text = "";
			this.selltotallabel.Text = "";
			this.pricetotallabel.Text = "";
			this.costtotallabel.Text = "";

		}


		public void OnPreview (object sender, EventArgs e) {


			CascadedOrderLogic col = GetCascadedOrderLogic (true);
			if (col == null) {
				return;
			}
			AutomatedOrder[] offers = DetermineOrders ( col, TradePairInstance);

			//RebuyDialog.doDialog(v);
			if (offers == null) {
				// TODO debug
				return;
			}

			if (offers.Length == 0) {
				return;
			}
			LicenseType licenseT = Util.LicenseType.CASCADING;
			if (LeIceSense.IsLicenseExempt (offers [0].taker_gets) || LeIceSense.IsLicenseExempt (offers [0].taker_pays)) {
				licenseT = LicenseType.NONE;
			}

			OrderSubmitWindow win = new OrderSubmitWindow (_rippleWallet, licenseT);
			win.SetOrders (offers);

		}






		public AutomatedOrder[] DetermineOrders( CascadedOrderLogic col, TradePair tradePair ) {

			uint number = col.numberOfOrders;
			Decimal amount = col.amount;
			Decimal price = col.startingPrice;
			Decimal pricemod = col.priceMod;
			Decimal amountmod = col.amountMod;


			string mark = col.markAs;
			string markmod = col.markAsMod;

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


			if (pricemod < 1) {
				pricemod = 1 / pricemod;
			}

			AutomatedOrder[] boa = new AutomatedOrder[number];

			RippleWallet rw = _rippleWallet;
			if (rw == null) {
				return null;
			}

			for (uint i = 0; i < number; i++) {

				AutomatedOrder b = new AutomatedOrder {
					Account = rw.GetStoredReceiveAddress (),
					taker_gets = tradePair.Currency_Base.DeepCopy (),
					taker_pays = tradePair.Currency_Counter.DeepCopy (),
					BotMarking = botmarkings.ElementAt ((int)i)
				};

				Decimal p = amount * price;
				Decimal g = amount;





				if (b.TakerPays.IsNative) {
					p = p * 1000000m;
				}
				if (b.TakerGets.IsNative) {
					g = g * 1000000m;
				}

				b.taker_pays.amount = p;
				b.taker_gets.amount = g;

				boa [i] = b;

				price = price * pricemod;
				amount = amount * amountmod;

			}

			return boa;
		}


		private CascadedOrderLogic GetCascadedOrderLogic ( bool warnuser ) {


			CascadedOrderLogic cbo = new CascadedOrderLogic ();

			string num = numberentry.Entry.Text;
			string pstr = priceentry.Entry.Text;
			string pms = pricemodentry.Entry.Text;
			string amst = amountentry.Entry.Text;
			string ammt = amountmodentry.Entry.Text;

			string mark = comboboxentry1.Entry.Text;
			string markmod = comboboxentry2.Entry.Text;

			uint? numberOfOrders = RippleCurrency.ParseUInt32 ( num );
			if (numberOfOrders == null) {
				if ( warnuser ) {
					var message = "Numbers of orders is formatted incorrectly \n";

					TextHighlighter.Highlightcolor = TextHighlighter.RED;
					label17.Markup = TextHighlighter.Highlight (message);
				}
				return null;
			}
		
			Decimal? startingPrice = RippleCurrency.ParseDecimal (pstr);
			if (startingPrice == null) {
				if (warnuser) {
					var message = "Starting price is formatted incorrectly \n";

					TextHighlighter.Highlightcolor = TextHighlighter.RED;
					label17.Markup = TextHighlighter.Highlight (message);
				}
				return null;
			}

			Decimal? priceMod = RippleCurrency.ParseDecimal ( pms );
			if (priceMod == null) {
				if (warnuser) {
					var message = "Price mod is formatted incorrectly \n";

					TextHighlighter.Highlightcolor = TextHighlighter.RED;
					label17.Markup = TextHighlighter.Highlight (message);

				}
				return null;
			}

			Decimal? amount = RippleCurrency.ParseDecimal (amst);
			if (amount == null) {
				if (warnuser) {
					var message = "Amount is formatted incorrectly \n";
					TextHighlighter.Highlightcolor = TextHighlighter.RED;
					label17.Markup = TextHighlighter.Highlight (message);
					// TODO color amount red
				}
				return null;
			}

			Decimal? amountMod = RippleCurrency.ParseDecimal (ammt);
			if (amountMod == null) {
				if (warnuser) {
					var message = "Amount mod is formatted incorrectly";

					TextHighlighter.Highlightcolor = TextHighlighter.RED;
					label17.Markup = TextHighlighter.Highlight (message);
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
						label10.Markup = "<b><u>Sell " + b + "</u></b>";
					}
				);
			}
		}

		public void SetRippleWallet (RippleWallet rippleWallet)
		{
			this._rippleWallet = rippleWallet;
		}

		private RippleWallet _rippleWallet {
			get;
			set;
		}

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private TradePair _tradepair = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

#if DEBUG
		private static readonly string clsstr = nameof(CascadedSellWidget) + DebugRippleLibSharp.colon;
#endif
	}
}

