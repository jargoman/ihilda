using System;
using System.Collections.Generic;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class OrderClusterWidget : Gtk.Bin
	{
		public OrderClusterWidget ()
		{
			this.Build ();


			ordersentry.Changed += ParseUI;

			spreadentry.Changed += ParseUI;

			pricemodentry.Changed += ParseUI;

			amountentry.Changed += ParseUI;

			amountmodentry.Changed += ParseUI;

			markentry.Changed += ParseUI;
		}

		public void ParseUI (object sender, EventArgs e) {

			Gdk.Color orchid = new Gdk.Color (218, 112, 214);

			TextHighlighter.Highlightcolor = ProgramVariables.darkmode ? "\"#FFAABB\"" : "\"red\"";
			string orders = ordersentry.ActiveText;
			if (string.IsNullOrWhiteSpace(orders)) {
				label8.Markup = TextHighlighter.Highlight ("Please specify a number orders per side");

				ordersentry.Entry.ModifyBase (Gtk.StateType.Normal, orchid);
				cluster = null;

				return;
			}

			bool b = int.TryParse (orders, out int ordernum);
			if (!b) {
				label8.Markup = TextHighlighter.Highlight ("Number of orders must be a valid integer.");
				ordersentry.Entry.ModifyBase (Gtk.StateType.Normal, orchid);
				cluster = null;
				return;
			}

			ordersentry.Entry.ModifyBase (Gtk.StateType.Normal);

			string spread = spreadentry.ActiveText;
			if (string.IsNullOrWhiteSpace (spread)) {
				label8.Markup = TextHighlighter.Highlight ("Please specify a spread. use 1.01 for one percent (1%)");

				spreadentry.Entry.ModifyBase (Gtk.StateType.Normal, orchid);
				cluster = null;
				return;

			}


			b = double.TryParse (spread, out double spreadnum);

			if (!b) {
				label8.Markup = TextHighlighter.Highlight ("Spread is not a valid number");
				spreadentry.Entry.ModifyBase (Gtk.StateType.Normal, orchid);
				cluster = null;
				return;

			}
			spreadentry.Entry.ModifyBase (Gtk.StateType.Normal);

			string priceMod = pricemodentry.ActiveText;
			if (string.IsNullOrWhiteSpace (priceMod)) {
				label8.Markup = TextHighlighter.Highlight ("Please specify a price mod.");

				pricemodentry.Entry.ModifyBase (Gtk.StateType.Normal, orchid);
				cluster = null;
				return;
			}

			b = double.TryParse (priceMod, out double priceModNum);
			if (!b) {
				label8.Markup = TextHighlighter.Highlight ("Price mod is not a valid number");

				pricemodentry.Entry.ModifyBase (Gtk.StateType.Normal, orchid);
				cluster = null;
				return;
			}

			pricemodentry.Entry.ModifyBase (Gtk.StateType.Normal);

			string amount = amountentry.ActiveText;
			if (string.IsNullOrWhiteSpace (amount)) {
				label8.Markup = TextHighlighter.Highlight ("Please specify an amount.");

				amountentry.Entry.ModifyBase (Gtk.StateType.Normal, orchid);
				cluster = null;
				return;
			}

			b = double.TryParse (amount, out double amountNum);
			if (!b) {
				label8.Markup = TextHighlighter.Highlight ("Amount is not a valid number.");
				amountentry.Entry.ModifyBase (Gtk.StateType.Normal, orchid);
				cluster = null;
				return;
			}
			amountentry.Entry.ModifyBase (Gtk.StateType.Normal);

			string amountMod = amountmodentry.ActiveText;
			if (string.IsNullOrWhiteSpace (amountMod)) {

				label8.Markup = TextHighlighter.Highlight ("Please specify an amount mod.");

				amountmodentry.Entry.ModifyBase (Gtk.StateType.Normal, orchid);
				cluster = null;
				return;
			}

			b = double.TryParse (amountMod, out double amountModNum);
			if (!b) {
				label8.Markup = TextHighlighter.Highlight ("Amount mod is not a valid number.");
				amountmodentry.Entry.ModifyBase (Gtk.StateType.Normal, orchid);
				cluster = null;
				return;
			}
			amountmodentry.Entry.ModifyBase (Gtk.StateType.Normal);


			string mark = markentry.ActiveText;
			if (string.IsNullOrWhiteSpace (mark)) {
				
				label8.Markup = TextHighlighter.Highlight ("please specify a mark.");

				markentry.Entry.ModifyBase (Gtk.StateType.Normal, orchid);
				cluster = null;
				return;
			}

			markentry.Entry.ModifyBase (Gtk.StateType.Normal);

			cluster = new OrderCluster {
				Orders = ordernum,
				Spread = spreadnum,
				Pricemod = priceModNum,
				Amount = amountNum,
				Amountmod = amountModNum,
				Mark = mark
			};

			SetDetails ( cluster );

			if (OnClusterChanged != null) {
				OnClusterChanged.Invoke (this, new ClusterChangedEventArgs () { Cluster = cluster });
			}
		}

		public void SetDetails (OrderCluster cluster)
		{
			//cluster.GetOrders (
		}

		public OrderCluster cluster = null;
		public event EventHandler<ClusterChangedEventArgs> OnClusterChanged;
	}



	public class ClusterChangedEventArgs : EventArgs
	{

		public OrderCluster Cluster {
			get;
			set;
		}


	}


	public class OrderCluster
	{
		public int Orders { get; set; }

		public double Spread { get; set; }

		public double Pricemod { get; set; }

		public double Amount { get; set;  }

		public double Amountmod { get; set; }

		public string Mark { get; set; }

		public Tuple < IEnumerable<AutomatedOrder>, IEnumerable<AutomatedOrder> > GetOrders (double midPrice, TradePair tradePair)
		{
			

			int number = Orders;

			double buyPrice = midPrice / ((Spread + 1) / 2);
			double sellPrice = midPrice * ((Spread + 1) / 2 );


			double amount = Amount;


			List<AutomatedOrder> buylist = new List<AutomatedOrder> ();
			List<AutomatedOrder> selllist = new List<AutomatedOrder> ();

			for (uint i = 0; i < number; i++) {

				Decimal buygets = (decimal)(amount * buyPrice); 


				Decimal buypays = (decimal)(amount );

				Decimal sellgets = (decimal)(amount);
				Decimal sellpays = (decimal)(amount * sellPrice);

				AutomatedOrder buyorder = new AutomatedOrder {
					//Account = rw.GetStoredReceiveAddress (),
					taker_pays = tradePair.Currency_Base.DeepCopy (),
					taker_gets = tradePair.Currency_Counter.DeepCopy (),
					BotMarking = Mark//botmarkings.ElementAt ((int)i)
				};

				AutomatedOrder sellorder = new AutomatedOrder {
					//Account = rw.GetStoredReceiveAddress (),
					taker_gets = tradePair.Currency_Base.DeepCopy (),
					taker_pays = tradePair.Currency_Counter.DeepCopy (),
					BotMarking = Mark
				};


				if (buyorder.TakerPays.IsNative) {
					buypays = buypays * 1000000m;
				}
				if (buyorder.TakerGets.IsNative) {
					buygets = buygets * 1000000m;

				}

				if (sellorder.TakerPays.IsNative) {
					sellpays = sellpays * 1000000m;
				}
				if (sellorder.TakerGets.IsNative) {
					sellgets = sellgets * 1000000m;
				}

				buyorder.taker_pays.amount = buypays;
				buyorder.taker_gets.amount = buygets;

				sellorder.taker_pays.amount = sellpays;
				sellorder.taker_gets.amount = sellgets;

				buylist.Add (buyorder);
				selllist.Add (sellorder);

				buyPrice = buyPrice / Pricemod;
				sellPrice = sellPrice * Pricemod;

				amount = amount * Amountmod;




			}



			return new Tuple< IEnumerable<AutomatedOrder>, IEnumerable<AutomatedOrder> > ( buylist, selllist );

		}


	}
}
