using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gdk;
using Gtk;
using IhildaWallet.Networking;
using Pango;
using RippleLibSharp.Commands.Stipulate;
using RippleLibSharp.Network;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class DepthChartWidget : Gtk.Bin
	{
		public DepthChartWidget ()
		{
			this.Build ();



			this.darkmodecheckbox.Active = ProgramVariables.darkmode;


			if (this.drawingarea1 == null) {
				this.drawingarea1 = new DrawingArea ();
				this.drawingarea1.Show ();

				vbox2.Add (this.drawingarea1);
			}

			if (label6 == null) {
				label6 = new Label ("Order Cluster");
			}

			if (orderclusterwidget1 == null) {

				orderclusterwidget1 = new OrderClusterWidget ();
				//orderclusterwidget1
				notebook1.AppendPage (orderclusterwidget1, label6);
			}

			//this.DoubleBuffered = false;
			//this.drawingarea1.DoubleBuffered = false;

			this.DoubleBuffered = true;
			this.drawingarea1.DoubleBuffered = true;

			this.drawingarea1.AddEvents ((int)
				(EventMask.ButtonPressMask
					| EventMask.ButtonReleaseMask
					| EventMask.KeyPressMask
					| EventMask.PointerMotionMask
					| EventMask.LeaveNotifyMask

				));

			this.label1.UseMarkup = true;

			this.drawingarea1.ExposeEvent += Drawingarea1_ExposeEvent;
			this.drawingarea1.Show ();

			this.drawingarea1.MotionNotifyEvent += Drawingarea1_MotionNotifyEvent;

			this.drawingarea1.LeaveNotifyEvent += Drawingarea1_LeaveNotifyEvent;
			//this.drawingarea1.
			//this.drawingarea1.ConfigureEvent += Drawingarea1_ConfigureEvent;

			//this.drawingarea1.Realized += Drawingarea1_Realized;

			this.drawingarea1.SizeAllocated += Drawingarea1_SizeAllocated;

			this.drawingarea1.ButtonPressEvent += Drawingarea1_ButtonPressEvent;

			this.drawingarea1.DragMotion += (o, args) => {

				// TODO I actually think there's nothing to do here
				//CopyBuffer ();
				//drawingarea1.
			};

			orderclusterwidget1.OnClusterChanged += (object sender, ClusterChangedEventArgs e) => {


				Gdk.Window gwin = this.drawingarea1?.GdkWindow;

				if (gwin == null) {
					return;
				}

				var pointframe = GetPointFrame ();

				decimal price = pointframe.midprice;

				var cluster = e?.Cluster;
				if (cluster == null) {
					return;
				}

				var orders = cluster.GetOrders ((double)price, _tradePair);
				if (orders == null) {
					return;
				}


				this.ordersTuple = orders;



				this.Drawbackground (gwin, pointframe);

				this.DrawChartToPixMap (gwin, pointframe);
				//this.CopyBuffer ();
			};

			darkmodecheckbox.Clicked += (object sender, EventArgs e) => {

				Gdk.Window gwin = this.drawingarea1?.GdkWindow;

				if (gwin == null) {
					return;
				}

				var pointframe = GetPointFrame ();

				Drawbackground (gwin, pointframe);

				this.DrawChartToPixMap (gwin, pointframe);

				//this.CopyBuffer ();
			};


			/*
var pointframe = this.GetPointFrame ();
this.DrawChartToPixMap (pointframe);
this.CopyBuffer ();
};*/

			this.hscale1.Digits = 0;
			this.hscale2.Digits = 0;

			this.hscale1.ValueChanged += (object sender, EventArgs e) => {

				//VisibleOrdersChanged ();
				//CopyPreloadedPointFrame ();

				visible_bids = (int)hscale1.Value;


				ScaleChanged ();
			};

			this.hscale2.ValueChanged += (object sender, EventArgs e) => {


				visible_asks = (int)hscale2.Value;

				ScaleChanged ();
			};



			this.drawingarea1.EnterNotifyEvent += (object o, EnterNotifyEventArgs args) => {
				scaleChanged = true;
			};


			this.motionLayout = new Pango.Layout (this.PangoContext);


		}

		~DepthChartWidget ()
		{
			TokenSource?.Cancel (); // = false;
			TokenSource?.Dispose ();
			//TokenSource = null;
			this.Dispose ();
		}

		public CancellationTokenSource TokenSource;
		//private bool _cont = true;

		private TradeWindow ShowTradeWindow ()
		{
			TradeWindow tradeWindow = new TradeWindow (_rippleWallet, _tradePair);
			//tradeWindow.



			tradeWindow.Show ();

			/*
			Task<TradeWindow> task = TradeWindow.InitGUI (_rippleWallet, this._tradePair);
			task.Wait ();
			TradeWindow tradeWindow = task.Result;
			//tradeWindow.

			if (tradeWindow == null) {
				// Todo DEBUG
				return null;
			}

			tradeWindow.Show();
			*/
			return tradeWindow;

		}


		void ScaleChanged ()
		{

			Gdk.Window gwin = this.drawingarea1?.GdkWindow;

			if (gwin == null) {
				return;
			}


			var pointFrame = GetPointFrame ();

			Drawbackground (gwin, pointFrame);


			if (this.asks == null && this.bids == null) {


				return;
			}
			
			
			





			DrawChartToPixMap (gwin, pointFrame);



			//CopyBuffer ( /*pointFrame*/ );

		}

		void Drawingarea1_ButtonPressEvent (object o, ButtonPressEventArgs args)
		{

			TradePair tradePair = _tradePair;
			if (tradePair == null) {
				return;
			}

			var pointFrame = GetPointFrame ();

			decimal price = pointFrame.lowestxprice + ((int)args.Event.X * pointFrame.rawxPriceWidth / pointFrame.chartWidth);
			//decimal amount = (pointFrame.chartHeight - (int)args.Event.Y) * pointFrame.highestAmount / pointFrame.chartHeight;
			decimal amount = (pointFrame.chartHeight - (int)args.Event.Y) / (pointFrame.chartHeight / pointFrame.highestAmount);


			Offer buyOffer = new Offer {
				taker_gets = tradePair.Currency_Counter.DeepCopy (),
				taker_pays = tradePair.Currency_Base.DeepCopy ()
			};

			buyOffer.taker_pays.amount = amount;

			buyOffer.taker_gets.amount = amount * price;

			if (buyOffer.taker_gets.IsNative) {
				buyOffer.taker_gets.amount *= 1000000m;
			}


			if (buyOffer.taker_pays.IsNative) {
				buyOffer.taker_pays.amount *= 1000000m;
			}


			Offer sellOffer = new Offer {
				taker_pays = tradePair.Currency_Counter.DeepCopy (),
				taker_gets = tradePair.Currency_Base.DeepCopy ()
			};

			sellOffer.taker_pays.amount = price * amount;

			sellOffer.taker_gets.amount = amount;

			if (sellOffer.taker_gets.IsNative) {
				sellOffer.taker_gets.amount *= 1000000m;
			}

			if (sellOffer.taker_pays.IsNative) {
				sellOffer.taker_pays.amount *= 1000000m;
			}

			Menu menu = new Menu ();


			#region buy_menus
			MenuItem buy = new MenuItem (
				ProgramVariables.darkmode ? "Prepare a <span fgcolor=\"chartreuse\">buy</span>  order at" : "Prepare a <span fgcolor=\"green\">buy</span>  order at "
				+ price.ToString ()
				+ " "
				+ tradePair.Currency_Counter.currency
				+ " per "
				+ tradePair.Currency_Base.currency

			);


			Gtk.Label l = (Gtk.Label)buy.Child;
			l.UseMarkup = true;

			buy.Show ();
			menu.Add (buy);

			buy.Activated += (object sender, EventArgs e) => {


#if DEBUG
				if (DebugIhildaWallet.DepthChartWidget) {
					Logging.WriteLog ("buy selected");
				}
#endif

				TradeWindow tradeWindow = ShowTradeWindow ();

				/*
				Offer offer = new Offer();

				offer.taker_gets = tradePair.currency_counter;
				offer.taker_pays = tradePair.currency_base;

				offer.taker_gets.amount = price * amount;

				offer.taker_pays.amount = amount;
				*/
				tradeWindow.SetBuyOffer (buyOffer);



			};

			Gtk.MenuItem cassbuy = new MenuItem (
				ProgramVariables.darkmode ? 
					"Cascade <span fgcolor=\"chartreuse\">buy</span> orders from " : 
				       	"Cascade <span fgcolor=\"green\">buy</span> orders from "
					+ price.ToString ()
					+ " "
		    			+ tradePair.Currency_Counter.currency
					+ " per "
					+ tradePair.Currency_Base.currency
		    	);

			cassbuy.Show ();
			menu.Add (cassbuy);

			l = (Label)cassbuy.Child;
			l.UseMarkup = true;

			cassbuy.Activated += (object sender, EventArgs e) => {
#if DEBUG
				if (DebugIhildaWallet.DepthChartWidget) {
					Logging.WriteLog ("cassbuy selected");
				}
#endif

				/*
				Offer offer = new Offer();

				offer.taker_gets = tradePair.currency_counter;
				offer.taker_pays = tradePair.currency_base;

				offer.taker_gets.amount = price * amount;

				offer.taker_pays.amount = amount;
				*/
				//Task.Run ();
				TradeWindow tradeWindow = ShowTradeWindow ();




				tradeWindow.InitiateCascade (buyOffer, OrderEnum.BID);
			};

			Gtk.MenuItem autobuy = new MenuItem (
				ProgramVariables.darkmode ?
				"Prepare an automated <span fgcolor=\"chartreuse\">buy</span> at " :
				"Prepare an automated <span fgcolor=\"green\">buy</span> at "
				+ price.ToString ()
				+ " "
				+ tradePair.Currency_Counter.currency
				+ " per "
				+ tradePair.Currency_Base.currency
				+ " ");
			autobuy.Show ();
			menu.Add (autobuy);

			l = (Gtk.Label)autobuy.Child;
			l.UseMarkup = true;

			autobuy.Activated += (object sender, EventArgs e) => {
#if DEBUG
				if (DebugIhildaWallet.DepthChartWidget) {
					Logging.WriteLog ("autobuy selected");
				}
#endif

				TradeWindow tradeWindow = ShowTradeWindow ();

				tradeWindow.SetAutomatedBuyOffer (buyOffer);

			};
			#endregion

			#region sell_menus

			Gtk.MenuItem sell = new MenuItem (ProgramVariables.darkmode ? "Prepare a <span fgcolor=\"#FFAABB\">sell</span> order at this price" : "Prepare a <span fgcolor=\"red\">sell</span> order at this price");
			sell.Show ();
			menu.Add (sell);

			l = (Gtk.Label)sell.Child;
			l.UseMarkup = true;

			sell.Activated += (object sender, EventArgs e) => {
#if DEBUG
				if (DebugIhildaWallet.DepthChartWidget) {
					Logging.WriteLog ("sell selected");
				}
#endif

				TradeWindow tradeWindow = ShowTradeWindow ();

				tradeWindow.SetSellOffer (sellOffer);

			};

			Gtk.MenuItem casssell = new MenuItem (
				(string)(ProgramVariables.darkmode ? "Cascade <span fgcolor=\"#FFAABB\">sell</span> orders begining at " : "Cascade <span fgcolor=\"red\">sell</span> orders begining at ")
				+ price.ToString ()
				+ " "
				+ tradePair.Currency_Counter.currency
				+ " per "
				+ tradePair.Currency_Base.currency);

			casssell.Show ();
			menu.Add (casssell);
			l = (Gtk.Label)casssell.Child;
			l.UseMarkup = true;
			casssell.Activated += (object sender, EventArgs e) => {

#if DEBUG
				if (DebugIhildaWallet.DepthChartWidget) {
					Logging.WriteLog ("casssell selected");
				}
#endif

				TradeWindow tradeWindow = ShowTradeWindow ();
				tradeWindow.InitCascadedSellOffer (sellOffer);

			};

			Gtk.MenuItem autosell = new MenuItem (

				ProgramVariables.darkmode ? "Prepare an automated <span fgcolor=\"#FFAABB\">sell</span> order at " : "Prepare an automated <span fgcolor=\"red\">sell</span> order at "
				+ price.ToString ()
				+ " "
				+ tradePair.Currency_Counter.currency
				+ " per "
				+ tradePair.Currency_Base.currency
				);

			autosell.Show ();
			menu.Add (autosell);
			l = (Gtk.Label)autosell.Child;
			l.UseMarkup = true;
			autosell.Activated += (object sender, EventArgs e) => {

#if DEBUG
				if (DebugIhildaWallet.DepthChartWidget) {
					Logging.WriteLog ("autosell selected");
				}
#endif
				TradeWindow tradeWindow = ShowTradeWindow ();


				tradeWindow.SetAutomatedOffer (sellOffer, OrderEnum.ASK);





			};
			#endregion

			var orders = this.ordersTuple;
			if (orders != null) {


				Gtk.MenuItem orderCluster = new MenuItem ("Prepare order cluster as shown");
				orderCluster.Show ();
				menu.Add (orderCluster);
				l = (Gtk.Label)orderCluster.Child;
				l.UseMarkup = true;
				orderCluster.Activated += (object sender, EventArgs e) => {

#if DEBUG
					if (DebugIhildaWallet.DepthChartWidget) {
						Logging.WriteLog ("autosell selected \n");
					}
#endif

					//

					//OrderSubmitWindow orderSubmitWindow = new OrderSubmitWindow (_rippleWallet, Util.LicenseType.SEMIAUTOMATED);
					OrderSubmitWindow orderSubmitWindow = new OrderSubmitWindow (_rippleWallet);

					var buys = orders.Item1;
					var list = buys.Concat (orders.Item2);

					orderSubmitWindow.SetOrders (list);

				};


				Gtk.MenuItem moveCluster = new MenuItem ("Move OrderCluster to " + (price.ToString () ?? ""));
				moveCluster.Show ();
				menu.Add (moveCluster);
				l = (Gtk.Label)moveCluster.Child;
				l.UseMarkup = true;
				moveCluster.Activated += (object sender, EventArgs e) => {

#if DEBUG

					if (DebugIhildaWallet.DepthChartWidget) {
						Logging.WriteLog ("move cluster selected \n");
					}
#endif

					this.ordersTuple = this.orderclusterwidget1.cluster.GetOrders ((double)price, tradePair);

					var window = this.drawingarea1.GdkWindow;

					DrawChartToPixMap (window, pointFrame);


				};
			}



			menu.Popup ();
		}

		/*
		void Drawingarea1_Realized (object sender, EventArgs e)
		{
			calculateBidSums ();
			calculatePoints ();

		}
		*/

		public void CalculatePoints (PointFrame pointFrame)
		{

			if (pointFrame == null) {
				return;
			}
				

			pointFrame.bidPoints = new List<Gdk.Point> ();
			pointFrame.askPoints = new List<Gdk.Point> ();

			pointFrame.localBidPointsRed = new List<Gdk.Point> ();
			pointFrame.localAskPointsRed = new List<Gdk.Point> ();


			pointFrame.localBidPointsBlue = new List<Gdk.Point> ();
			pointFrame.localAskPointsBlue = new List<Gdk.Point> ();

			pointFrame.oderClusterPoints = new List<Gdk.Point> (); // cluster

			decimal rawPricex = Decimal.Zero;
			decimal rawAmounty = pointFrame.highestAmount;

			decimal localy = pointFrame.highestAmount;

			// We're calulating this backwards. Starting with totals bids
			decimal bidsGetsSpent = 0;

			decimal midcalc = 0;

			for (int i = 0; i < pointFrame.numBids; i++) {

				rawPricex = pointFrame.bidsArray [i].TakerPays.GetNativeAdjustedPriceAt (pointFrame.bidsArray [i].taker_gets);
				if (i == 0) {
					midcalc += rawPricex;
				}

				// TODO deal with divide by zero exception that can occur. Why? How to prevent? handle the exception ect
				double resx = (double)((rawPricex - pointFrame.lowestxprice) * (pointFrame.chartWidth / pointFrame.rawxPriceWidth));
				double resy = (double)(rawAmounty * (pointFrame.chartHeight / pointFrame.highestAmount));

				rawAmounty -= pointFrame.bidsArray [i].TakerPays.amount;



				Gdk.Point point = new Gdk.Point ((int)resx, (int)resy /*- RULER_WIDTH*/);

				pointFrame.bidPoints.Add (point);

				RippleWallet rw = _rippleWallet;
				if (rw?.GetStoredReceiveAddress () == null) {
					continue;
				}

				if (rw.GetStoredReceiveAddress ().Equals (pointFrame.bidsArray [i].Account)) {

					resy = (double)(localy * (pointFrame.chartHeight / pointFrame.highestAmount));
					localy -= pointFrame.bidsArray [i].TakerPays.amount;
					bidsGetsSpent += pointFrame.bidsArray [i].TakerGets.amount;


					if (_tradePair.Currency_Counter.amount > bidsGetsSpent) {
						pointFrame.localBidPointsBlue.Add (
							new Gdk.Point (
								(int)resx,

								(int)resy /*- RULER_WIDTH*/

							)
						);
					} else {
						pointFrame.localBidPointsRed.Add (
							new Gdk.Point (
								(int)resx,


								(int)resy /*- RULER_WIDTH*/

							)
						);
					}
				}

			}

			// uncommenting this adds a line from the last point to the bottom of the screen
			//bidPoints.Add ( new Gdk.Point ( (int) (double) ((rawx - lowestxpoint ) * (width / rawxWidth)) , (int) height)); // TODO is the cast necessary?




			rawAmounty = pointFrame.highestAmount;
			rawPricex = pointFrame.asksArray [0].TakerGets.GetNativeAdjustedPriceAt (pointFrame.asksArray [0].TakerPays);


			//double resultx = (double)((rawx - lowestxpoint) * (width / rawxWidth));
			//double resulty = (double)(rawy * height / highestypoint);

			//Gdk.Point point1 = new Gdk.Point ((int)resultx, (int)resulty);

			//askPoints.Add (point1);


			rawAmounty = pointFrame.highestAmount;
			localy = pointFrame.highestAmount;

			Decimal asksGetsSpent = Decimal.Zero;
			for (int i = 0; i < pointFrame.numAsks; i++) {

				rawPricex = pointFrame.asksArray [i].TakerGets.GetNativeAdjustedPriceAt (pointFrame.asksArray [i].TakerPays);

				if (i == 0) {
					midcalc += rawPricex;
				}

				rawAmounty -= pointFrame.asksArray [i].TakerGets.amount;


				double resx = (double)((rawPricex - pointFrame.lowestxprice) * (pointFrame.chartWidth / pointFrame.rawxPriceWidth));
				double resy = (double)(rawAmounty * pointFrame.chartHeight / pointFrame.highestAmount);

				Gdk.Point point = new Gdk.Point ((int)resx, (int)resy /*- RULER_WIDTH*/);

				pointFrame.askPoints.Add (point);

				RippleWallet rw = _rippleWallet;
				if (rw?.GetStoredReceiveAddress () == null) {
					continue;
				}

				if (rw.GetStoredReceiveAddress ().Equals (pointFrame.asksArray [i].Account)) {
					localy -= pointFrame.asksArray [i].TakerGets.amount;
					asksGetsSpent += pointFrame.asksArray [i].TakerGets.amount;
					resy = (double)(localy * pointFrame.chartHeight / pointFrame.highestAmount);

					if (_tradePair.Currency_Base.amount > asksGetsSpent) {
						pointFrame.localAskPointsBlue.Add (
							new Gdk.Point (
								(int)resx,
										  (int)resy /*- RULER_WIDTH*/

							)
							);
					} else {
						pointFrame.localAskPointsRed.Add (
							new Gdk.Point (
								(int)resx,
										  (int)resy /*- RULER_WIDTH*/

							)
							);
					}
				}

			}

			pointFrame.midprice = midcalc / 2;

			double px = (double)((pointFrame.midprice - pointFrame.lowestxprice) * (pointFrame.chartWidth / pointFrame.rawxPriceWidth));
			double py = (double)(0 * (pointFrame.chartHeight / pointFrame.highestAmount));


			pointFrame.midChartpoint = new Gdk.Point ((int)px, (int)py);



			//XScaleChanged (pointFrame);

			//YScalePointsChanged (pointFrame);


		}

		public PointFrame XScaleChanged (PointFrame pointFrame)
		{
			int bidc = pointFrame.numBids;
			int askc = pointFrame.numAsks;

			Decimal lowestPrice = pointFrame.bidsArray [bidc - 1].TakerGets.GetNativeAdjustedCostAt (pointFrame.bidsArray [bidc - 1].TakerPays);
			Decimal highestPrice = pointFrame.asksArray [askc - 1].TakerGets.GetNativeAdjustedPriceAt (pointFrame.asksArray [askc - 1].TakerPays);
			Decimal priceRange = highestPrice - lowestPrice;


			int target_num_points = pointFrame.drawingAreaWidth / 160;



			Decimal targetIncrement = priceRange / target_num_points;  // get as close to 5 points as possible

			//Decimal targetIncrement = pointFrame.chartWidth / PRICE_SCLALE_WIDTH;

			Decimal scaleGuess = 10000000000000000000000000000m; // guess a random amount to increment price by

			int incr = 0;
			while (scaleGuess > targetIncrement) {

				if (incr == 0) {
					scaleGuess /= 2; // make divisible by 5
					incr++;
				} else if (incr == 1) {
					scaleGuess /= 2; // make divisible by 2.5
					incr++;
				} else {
					scaleGuess /= 2.5m; // make divisible by 10
					incr = 0;
				}

			}

			//Decimal modulus = (lowestPrice % scaleGuess); 
			Decimal evenNum = lowestPrice / scaleGuess; // find the number of even point under the low price
			evenNum = Math.Round (evenNum); // make sure it's not a decimal 
			Decimal ex = evenNum * scaleGuess; // get the lowest point x




			while (ex < pointFrame.highestxprice) { // keep placing points until 


				double esRes = (double)((ex - pointFrame.lowestxprice) * (pointFrame.chartWidth / pointFrame.rawxPriceWidth)); // convert to chart resolution

				Tuple<Gdk.Point, Decimal> tuple = new Tuple<Gdk.Point, decimal> (new Gdk.Point ((int)esRes, pointFrame.chartHeight + 2), ex); // save the point and price

				pointFrame.scalePointsX.Add (tuple);
				ex += scaleGuess;
			}

			return pointFrame;
		}


		public PointFrame YScalePointsChanged (PointFrame pointFrame)
		{
			int bidc = pointFrame.numBids;
			int askc = pointFrame.numAsks;

			Decimal lowestAmount = pointFrame.lowestypointAmount;
			Decimal highestAmount = pointFrame.highestAmount;

			//Decimal priceRange = highestPrice - lowestPrice;


			Decimal targetIncrement = highestAmount / 5;  // aim for around 5 points

			Decimal scaleGuess = 1000000000000000m; // guess an asronomically high number

			int incr = 0;
			while (scaleGuess > targetIncrement) {

				if (incr == 0) {
					scaleGuess /= 2; // make divisible by 5
					incr++;
				} else if (incr == 1) {
					scaleGuess /= 2;  // make divisible by 2.5
					incr++;
				} else {
					scaleGuess /= 2.5m; // make divisible by 10
					incr = 0;
				}

			}

			//Decimal modulus = (lowestAmount % scaleGuess);
			//Decimal evenNum = lowestAmount / scaleGuess;
			//evenNum = Math.Round (evenNum);

			Decimal evenYamount = scaleGuess; // we know the lowest amount is divisible by zero

			while (evenYamount < pointFrame.highestAmount) {

				double resy = (pointFrame.chartHeight - (double)(evenYamount * (pointFrame.chartHeight / pointFrame.highestAmount))) ; // get y point



				//decimal amount = (pointFrame.chartHeight - mouseY) / (pointFrame.chartHeight / pointFrame.highestAmount);
				Decimal amount = _tradePair.Currency_Base.IsNative ?
					evenYamount / 1000000 : evenYamount;
				Tuple<Gdk.Point, Decimal> tuple = new Tuple<Gdk.Point, decimal> (new Gdk.Point (pointFrame.chartWidth + 2, (int)resy), amount);
				
				pointFrame.scalePointsY.Add (tuple);
				evenYamount += scaleGuess;
			}

			return pointFrame;
		}

		/*
		void Drawingarea1_ConfigureEvent (object o, ConfigureEventArgs args)
		{
			calculateBidSums ();
			calculatePoints ();
			//Gdk.Window gwin = this.drawingarea1.GdkWindow;
			//gwin.GetSize(out width, out height);


		}
		*/

		void Drawingarea1_LeaveNotifyEvent (object o, LeaveNotifyEventArgs args)
		{

			if (!this.drawingarea1.IsDrawable) {
				return;
			}


			Gdk.Window gwin = this.drawingarea1?.GdkWindow;

			if (gwin == null) {
				return;
			}


			PointFrame pointframe = GetPointFrame ();

			Drawbackground (gwin, pointframe);

			DrawChartToPixMap (gwin, pointframe);



		}

		void Drawingarea1_SizeAllocated (object o, SizeAllocatedArgs args)
		{
			Gdk.Window gwin = this.drawingarea1?.GdkWindow;

			if (gwin == null) {
				return;
			}


			var pointFrame = GetPointFrame ();

			Drawbackground (gwin, pointFrame);


			if (this.asks == null && this.bids == null) {



				return;
			}


			DrawChartToPixMap (gwin, pointFrame);

			
		}



		Gdk.Color color_orchid = new Gdk.Color (218, 112, 214);
		Gdk.Color color_black = new Gdk.Color (0, 0, 0);
		Gdk.Color color_red = new Gdk.Color (255, 0, 0);
		Gdk.Color color_green = new Gdk.Color (0, 255, 0);
		Gdk.Color Color_white = new Gdk.Color (255,255,255);

		Gdk.Color BackgroundColorDark = new Gdk.Color (39, 40, 33);
		Gdk.Color BackgroundColorLight = new Gdk.Color (255, 255, 255);

		Tuple<IEnumerable<AutomatedOrder>, IEnumerable<AutomatedOrder>> ordersTuple = null;

		private void SetOrdersTuple (decimal price)
		{
			OrderCluster cluster = this.orderclusterwidget1.cluster;
			Tuple<IEnumerable<AutomatedOrder>, IEnumerable<AutomatedOrder>> orderTuple = cluster.GetOrders ((double)price, _tradePair);
			ordersTuple = orderTuple;
		}

		void Drawingarea1_MotionNotifyEvent (object o, MotionNotifyEventArgs args)
		{

			// TODO get cached point frame


			Gdk.Window gwin = this.drawingarea1?.GdkWindow;

			if (gwin == null) {
				return;
			}

			
			var pointFrame = GetPointFrame ();



			int mouseX = (int)args.Event.X;
			int mouseY = (int)args.Event.Y;





			if (!this.drawingarea1.IsDrawable) {
				return;
			}


			Drawbackground (gwin, pointFrame);
			if (asks == null && bids == null) {

				return;
			}


			DrawChartToPixMap (gwin, pointFrame);

			DrawMouseMotionBuffer (gwin, mouseX, mouseY, pointFrame);


			
		}




		Pango.Layout motionLayout = null;
		StringBuilder motionStringBuilder = new StringBuilder ();




		public void DrawMouseMotionBuffer (Drawable drawable, int mouseX, int mouseY, PointFrame pointFrame)
		{


			//Pixmap motionBuffer = null; // ???? type drawable?




			// changed to chart width/height because only the chart area needs to be redrawn
			//var pixmap = new Pixmap (gwin, pointFrame.chartWidth, pointFrame.chartHeight);
			//var pixmap = new Gtk.Image ();


			if (pointFrame.chartWidth == 0 || pointFrame.chartHeight == 0) {
				return;
			}

			if (pointFrame.highestAmount == 0) {
				return;
			}

			using (Gdk.GC gc = new Gdk.GC (drawable)) {



				decimal amount = (pointFrame.chartHeight - mouseY) / (pointFrame.chartHeight / pointFrame.highestAmount);

				//Gdk.Image buff = chartBufferImage;
				decimal price = pointFrame.lowestxprice + (mouseX * pointFrame.rawxPriceWidth / pointFrame.chartWidth);


				if (amount > Decimal.Zero && price < pointFrame.highestxprice) {





					bool darkmode = darkmodecheckbox.Active;
					if (!darkmode) {
						gc.RgbFgColor = color_black;
						//gc.RgbFgColor = new Gdk.Color (39, 40, 33);
					} else {
						gc.RgbFgColor = Color_white;
						gc.RgbBgColor = new Gdk.Color (39, 40, 33);

					}

					// TODO set dotted lines 

					gc.SetLineAttributes (2, LineStyle.DoubleDash, CapStyle.Butt, JoinStyle.Round);

					drawable.DrawLine (gc, mouseX, mouseY, mouseX, pointFrame.chartHeight);
					drawable.DrawLine (gc, mouseX, mouseY, pointFrame.chartWidth, mouseY);

					gc.RgbFgColor = color_black;
					gc.RgbBgColor = color_orchid;

					//Pango.Layout layout = new Pango.Layout (this.PangoContext);



					//decimal amount = (pointFrame.height - y) * pointFrame.highestypoint / pointFrame.height;
					string amt = default (String);
					if (_tradePair.Currency_Base.IsNative) {
						amt = (amount / 1000000).ToString ();
					} else {
						amt = amount.ToString ();
					}


					motionStringBuilder.Clear ();
					motionStringBuilder.Append ("<span bgcolor=\"orchid\">Price: ");
					motionStringBuilder.Append (price.ToString ());
					motionStringBuilder.Append ("\nAmount: ");
					motionStringBuilder.Append (amt);
					motionStringBuilder.Append (" ");
					motionStringBuilder.Append (_tradePair.Currency_Base.currency);

#if DEBUG
					if (DebugIhildaWallet.DepthChartWidget) {

						motionStringBuilder.Append ("x,y=");
						motionStringBuilder.Append (mouseX.ToString ());
						motionStringBuilder.Append (",");
						motionStringBuilder.AppendLine (mouseY.ToString ());
					}
#endif


					motionStringBuilder.Append ("</span>");

					motionLayout.SetMarkup (motionStringBuilder.ToString ());

					int xoffset = 0;

					if (mouseX < pointFrame.chartWidth / 2) {
						xoffset += 10;
					} else {
						xoffset -= 150;
					}

					int yoffset = 0;
					if (mouseY < pointFrame.chartHeight / 2) {
						yoffset += 30;
					} else {
						yoffset -= 50;
					}
					
					drawable.DrawLayout (gc, mouseX + xoffset, mouseY + yoffset, motionLayout);



				}

				gc?.Dispose ();
			}

			

		}



		public void SetTradePair (TradePair tp)
		{
			this._tradePair = tp;

			this.label1.Markup = "<b>" + tp.Currency_Base.currency + ":" + tp.Currency_Counter.currency + "</b>";
		}

		public void InitBooksUpdate ()
		{

			Task.Factory.StartNew (async () => {

				TokenSource?.Cancel ();
				TokenSource = new CancellationTokenSource ();
				var token = TokenSource.Token;

				/*
				Application.Invoke ( delegate {
					

				});


				*/

				//System.GC.Collect ();
				//System.GC.WaitForPendingFinalizers ();


				do {
					UpdateBooksOnce (token);
					await Task.Delay (30000, token);

				} while (!token.IsCancellationRequested);
			}
			);



		}

		private void UpdateBooksOnce (CancellationToken token)
		{


#if DEBUG
			String method_sig = clsstr + nameof (UpdateBooksOnce) + DebugRippleLibSharp.both_parentheses;
#endif

			PumpUI (token);

			TradePair tradePair = _tradePair;

			if (tradePair == null) {
#if DEBUG
				if (DebugIhildaWallet.DepthChartWidget) {
					Logging.WriteLog (method_sig + "tp == null");
				}
#endif
				// todo the lines below
				//if (this.buyorderbooktablewidget != null) {
				//	this.buyorderbooktablewidget.clearTable();
				//}

				//if (this.sellorderbooktablewidget != null) {
				//	this.sellorderbooktablewidget.clearTable();
				//}
				ResetProgressBar ();
				return;
			}





			PumpUI (token);



			//NetworkInterface ni = NetworkController.GetNetworkInterfaceGuiThread ();
			NetworkInterface ni = NetworkController.GetNetworkInterfaceNonGUIThread ();
			if (ni == null) {

				ResetProgressBar ();
				return;
			}
			Task balTask = Task.Run ( delegate {
				this._tradePair.UpdateBalances (_rippleWallet.GetStoredReceiveAddress (), ni);
			}, token);



			Task<AutomatedOrder[]> bidsTask = 
				Task.Run (
					delegate { 
						return UpdateBids (ni, tradePair, token).ToArray(); 
						
					}
					, token
				);

			Task<AutomatedOrder[]> askTask = 
				Task.Run (
					delegate { 
						return UpdateAsks (ni, tradePair, token).ToArray(); 
					}, token
				);

			// TODO, shoud we wait on balances?
			Task [] tasks = { bidsTask, askTask, balTask };

			while (

				TaskHelper.TaskIsWaiting(bidsTask) &&
				TaskHelper.TaskIsWaiting(askTask) &&
				TaskHelper.TaskIsWaiting(balTask)
			)

				
			{
				Task.WaitAll (tasks, 250, token);
				PumpUI (token);
			}

#if DEBUG
			if (DebugIhildaWallet.DepthChartWidget) {
				Logging.WriteLog (method_sig + "stopped Waiting");
				//Logging.writeLog("id type =" + id.GetType().ToString());
				//Logging.writeLog("id = " + id.ToString());
			}
#endif
			

	    		asks = askTask?.Result?.ToArray ();


			bids = bidsTask?.Result?.ToArray ();



			this.scaleChanged = true;


			Application.Invoke ( delegate {
				if (drawingarea1 == null) {
					return;
				}

				if (!this.drawingarea1.IsDrawable) {
					return;
				}

				Gdk.Window gwin = this.drawingarea1?.GdkWindow;
				//gwin.GetSize (out int gwinwidth, out int gwinheight);




				var pointFrame = GetPointFrame();

				Drawbackground (gwin, pointFrame);

				DrawChartToPixMap (gwin, pointFrame);



			});


			ResetProgressBar ();


		}

		private void PumpUI (CancellationToken token)
		{
			Gtk.Application.Invoke ( delegate {
				if (token.IsCancellationRequested) {
					return;
				}
				progressbar1.Pulse ();
			}
			);
		}

		private void ResetProgressBar ()
		{
			Gtk.Application.Invoke (delegate {

				progressbar1.Fraction = 0;
			}
			);
		}



		public IEnumerable<AutomatedOrder> UpdateBids (NetworkInterface ni, TradePair tp, CancellationToken token)
		{


#if DEBUG
			string method_sig = nameof (UpdateBids) + DebugRippleLibSharp.colon;
			if (DebugIhildaWallet.DepthChartWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);

			}
#endif

			RippleCurrency cur_base = tp.Currency_Base;
			if (cur_base == null) {
#if DEBUG
				if (DebugIhildaWallet.DepthChartWidget) {
					Logging.WriteLog (method_sig + "cur_base == null, returning");
				}
#endif
				return null;
			}

			RippleCurrency counter_currency = tp.Currency_Counter;
			if (counter_currency == null) {
#if DEBUG
				if (DebugIhildaWallet.DepthChartWidget) {
					Logging.WriteLog (method_sig + "counter_currency == null, returning");
				}
#endif
				return null;
			}

			Task<Response<BookOfferResult>> buyTask = BookOffers.GetResult (counter_currency, cur_base, ni, token);
			buyTask.Wait (token);
#if DEBUG

			if (DebugIhildaWallet.DepthChartWidget) {
				Logging.WriteLog (method_sig + "done waiting");
				//Logging.writeLog(method_sig + "e.Message = " + Debug.toAssertString(e.Message));
			}
#endif

			Offer [] buys = buyTask?.Result?.result?.offers;

			IEnumerable<AutomatedOrder> buyoffers = AutomatedOrder.ConvertFromIEnumerableOrder ( /*account,*/ buys);

			return buyoffers;
		}

		public IEnumerable<AutomatedOrder> UpdateAsks (NetworkInterface ni, TradePair tp, CancellationToken token)
		{


#if DEBUG
			string method_sig = nameof (UpdateAsks) + DebugRippleLibSharp.colon;
			if (DebugIhildaWallet.DepthChartWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);

			}
#endif

			RippleCurrency cur_base = tp.Currency_Base;
			if (cur_base == null) {
#if DEBUG
				if (DebugIhildaWallet.DepthChartWidget) {
					Logging.WriteLog (method_sig + "cur_base == null, returning");
				}
#endif
				return null;
			}

			RippleCurrency counter_currency = tp.Currency_Counter;
			if (counter_currency == null) {
#if DEBUG
				if (DebugIhildaWallet.DepthChartWidget) {
					Logging.WriteLog (method_sig + "counter_currency == null, returning");
				}
#endif
				return null;
			}

			Task<Response<BookOfferResult>> sellTask = BookOffers.GetResult (cur_base, counter_currency, ni, token);
			sellTask.Wait (token);
#if DEBUG

			if (DebugIhildaWallet.DepthChartWidget) {
				Logging.WriteLog (method_sig + "done waiting");
				//Logging.writeLog(method_sig + "e.Message = " + Debug.toAssertString(e.Message));
			}
#endif
			Offer [] sells = sellTask?.Result?.result?.offers;

			IEnumerable<AutomatedOrder> selloffers = AutomatedOrder.ConvertFromIEnumerableOrder (/*account,*/ sells);

			return selloffers;

		}



		// NOTE HIGHEST VALUE not HIGHEST POSITION this got confusing quick

#pragma warning disable RECS0122 // Initializing field with default value is redundant






		TradePair _tradePair = null;

		// TODO adjust later

		// 
		public const int RULER_HEIGHT = 35;

		public const int PRICE_SCLALE_WIDTH = 100;

		//private PointFrame screenSizedPointFrame;


		public PointFrame GetPointFrame ()
		{
			PointFrame pFrame = CreatePointFrame ();

			VisibleOrdersChanged (pFrame);

			VisibleOrderbookSumsChanged (pFrame);

			CalculatePoints (pFrame);

			XScaleChanged (pFrame);


			YScalePointsChanged (pFrame);

			return pFrame;
		}


		public PointFrame CreatePointFrame ()
		{

			Gdk.Window gwin = this.drawingarea1.GdkWindow;

			if (gwin == null) {
				return null;
			}
			

			var pointFrame = new PointFrame ();

			gwin.GetSize (out pointFrame.drawingAreaWidth, out pointFrame.drawingAreaHeight);
	    		
			

			// remove bottom scale area from drawing area height
			pointFrame.chartHeight = pointFrame.drawingAreaHeight- DepthChartWidget.RULER_HEIGHT;
			pointFrame.chartWidth = pointFrame.drawingAreaWidth - DepthChartWidget.PRICE_SCLALE_WIDTH;

			return pointFrame;

		}

		public PointFrame VisibleOrderbookSumsChanged (PointFrame pointFrame)
		{

			if (pointFrame == null) {
				// TODO
				return null;
			}

			if (pointFrame.bidsArray == null && pointFrame.asksArray == null) {
				return null;
			}

			if (this.drawingarea1 == null) {
				return null;
			}

			//this.drawingarea1.GetSizeRequest (out pointFrame.width, out pointFrame.height);



			if (pointFrame.bidsArray != null) {
				for (int i = 0; i < pointFrame.numBids; i++) {

					pointFrame.bidAmountSum += pointFrame.bidsArray [i].TakerPays.amount;

					RippleWallet rw = _rippleWallet;
					if (rw?.GetStoredReceiveAddress () == null) {
						continue;
					}

					if (rw.GetStoredReceiveAddress ().Equals (pointFrame.bidsArray [i].Account)) {


						pointFrame.userBidAmountSum += pointFrame.bidsArray [i].TakerPays.amount;
					}
				}
			}

			if (pointFrame.asksArray != null) {
				for (int i = 0; i < pointFrame.numAsks; i++) {

					pointFrame.askAmountSum += pointFrame.asksArray [i].TakerGets.amount;

					RippleWallet rw = _rippleWallet;
					if (rw?.GetStoredReceiveAddress () == null) {
						continue;
					}

					if (rw.GetStoredReceiveAddress ().Equals (pointFrame.asksArray [i].Account)) {


						pointFrame.userAskAmountSum += pointFrame.asksArray [i].TakerGets.amount;
					}
				}

			}

			AutomatedOrder lowestBid = pointFrame.bidsArray [pointFrame.numBids - 1];
			AutomatedOrder highestAsk = pointFrame.asksArray [pointFrame.numAsks - 1];



			pointFrame.highestAmount = (pointFrame.bidAmountSum > pointFrame.askAmountSum) ? pointFrame.bidAmountSum : pointFrame.askAmountSum; // summ
			pointFrame.lowestypointAmount = 0;

			if (highestAsk != null) {
				pointFrame.highestxprice = highestAsk.taker_gets.GetNativeAdjustedPriceAt (highestAsk.taker_pays);
				if (lowestBid == null) {
					AutomatedOrder lowestAsk = pointFrame.bidsArray.First ();

					pointFrame.lowestxprice = lowestAsk.taker_gets.GetNativeAdjustedPriceAt (lowestAsk.taker_pays);
				}
			}

			if (lowestBid != null) {
				pointFrame.lowestxprice = lowestBid.taker_pays.GetNativeAdjustedPriceAt (lowestBid.taker_gets);
				if (highestAsk == null) {
					AutomatedOrder highestBid = pointFrame.asksArray.First ();
					pointFrame.highestxprice = highestBid.taker_gets.GetNativeAdjustedPriceAt (highestBid.taker_pays);
				}
			}

			pointFrame.rawxPriceWidth = pointFrame.highestxprice - pointFrame.lowestxprice; // full spread


			return pointFrame;

		}


		// just the bids and asks
		// called when orderBook changed
		public PointFrame VisibleOrdersChanged (PointFrame pointFrame) {
			int numAsk = 0;
			int numBid = 0;

			if (asks != null) {

				numAsk = asks.Length < visible_asks ? asks.Length : visible_asks;
			}

			if (bids != null) {
				numBid = bids.Length < visible_bids ? bids.Length : visible_bids;
			}

			if (numAsk == 0 && numBid == 0) {
				// TODO
				return null;
			}


			// TODO is linq faster? is this too much copying?
			
			pointFrame.numAsks = numAsk;
			pointFrame.numBids = numBid;
			pointFrame.asksArray = asks;
			pointFrame.bidsArray = bids;

			return pointFrame;


		}

		/*
		public PointFrame VisibleOrderFrame {
			get;
			set;

		}*/

		public void Drawingarea1_ExposeEvent (object sender, ExposeEventArgs args)
		{
			if (drawingarea1 == null) {
				return;
			}

			if (!this.drawingarea1.IsDrawable) {
				return;
			}

			Gdk.Window gwin = this.drawingarea1?.GdkWindow;

			if (gwin == null) {
				return;
			}


			PointFrame pointFrame = GetPointFrame ();

			Drawbackground (gwin, pointFrame);




			if (this.asks == null && this.bids == null) {

				//gwin.Clear ();
				//gwin.GetSize (out int gwinwidth, out int gwinheight);

				//PointFrame initialPointFrame = new PointFrame ();





				return;
			}


		




			DrawChartToPixMap (gwin, pointFrame);


		}






		public void Drawbackground (Drawable drawable, PointFrame pointFrame)
		{


			//backgroundPixmap = new Pixmap (drawingarea1.GdkWindow, pointFrame.drawingAreaWidth, pointFrame.drawingAreaHeight);

			using (Gdk.GC gc = new Gdk.GC (drawable)) {
				bool darkmode = darkmodecheckbox.Active;


				#region bagroundfill

				if (!darkmode) {
					gc.RgbFgColor = BackgroundColorLight;
				} else {
					gc.RgbFgColor = BackgroundColorDark;
				}

				drawable.DrawRectangle (gc, true, 0, 0, pointFrame.drawingAreaWidth, pointFrame.drawingAreaHeight);
				#endregion


				#region ihildalogo

				Pango.Context context = CreatePangoContext ();

				using (Pango.Layout layout = new Pango.Layout (context) {
					Width = Pango.Units.FromPixels (pointFrame.chartWidth)
				}) {


					FontDescription desc = FontDescription.FromString ("Serif Bold 100");

					layout.FontDescription = desc;

					//renderer.SetOverrideColor (RenderPart.Foreground, new Gdk.Color (0, 0, 0));
					layout.Alignment = Pango.Alignment.Center;

					//gwin.DrawImage (gc, gdkImage, 0, 0, 0, 0, pointFrame.width, pointFrame.height);
					layout.SetText ("Ihilda");



					if (!darkmode) {
						gc.RgbFgColor = new Gdk.Color (250, 235, 249);
					} else {
						gc.RgbFgColor = new Gdk.Color (81, 21, 78);
					}


					drawable.DrawLayout (gc, 25, 25, layout);

					layout.Dispose ();

				}

				#endregion
			}
		}






		public void DrawChartLines (Drawable drawable, PointFrame pointFrame)
		{

			using (Gdk.GC gc = new Gdk.GC (drawable)) {

				bool darkmode = darkmodecheckbox.Active;

				#region outline

				if (!darkmode) {
					gc.RgbFgColor = color_black;
				} else {
					gc.RgbFgColor = Color_white;
				}

				//gc.RgbFgColor = new Gdk.Color (255,255,255);
				gc.SetLineAttributes (2, LineStyle.Solid, CapStyle.Butt, JoinStyle.Miter);

				//gwin.DrawLine (gc, 0, pointFrame.height - RULER_WIDTH, pointFrame.width, pointFrame.height - RULER_WIDTH);

				// either windowwidth - Ruler_width OR pointframeheight

				// draw horizontal line
				drawable.DrawLine (gc, 0, pointFrame.chartHeight, pointFrame.chartWidth, pointFrame.chartHeight);

				// draw verticle line
				drawable.DrawLine (gc, pointFrame.chartWidth, 0, pointFrame.chartWidth, pointFrame.chartHeight);
				#endregion

				gc.Dispose ();
			}

		}
		// Only called when window size changed : 1
		


		//private Gdk.Image chartBufferImage = null;
		public void DrawChartToPixMap (Drawable drawable, PointFrame pointFrame)
		{
			if (pointFrame == null) {
				return;
			}



			int cirsize = 5;
			//

#if DEBUG
			if (DebugIhildaWallet.DepthChartWidget) {
				//Logging.writeLog ();
			}
#endif


		


			DrawChartLines(drawable, pointFrame);


			//var pixmap = new Pixmap (drawingarea1.GdkWindow, gwinwidth, gwinheight);

			using (Gdk.GC gc = new Gdk.GC (drawable)) {



				bool darkmode = darkmodecheckbox.Active;



				if (!darkmode) {
					gc.RgbFgColor = color_black;
				} else {
					gc.RgbFgColor = Color_white;
				}


				Pango.Context context = CreatePangoContext ();
				Pango.Layout layout = new Pango.Layout (context) {
					Width = Pango.Units.FromPixels (pointFrame.chartWidth)
				};

				gc.SetLineAttributes (1, LineStyle.Solid, CapStyle.Butt, JoinStyle.Miter);

				layout.Alignment = Pango.Alignment.Left;
				layout.FontDescription = FontDescription.FromString ("Serif Bold 9");

				foreach (var tuple in pointFrame.scalePointsX) {
					//gwin.DrawLine(gc, p.X, p.Y, p.X, 0);
					Gdk.Point p = tuple.Item1;
					Decimal inc = tuple.Item2;

					//gwin.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);
					drawable.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);
					//Gdk.PangoRenderer renderer = Gdk.PangoRenderer.GetDefault (gwin.Screen);
					//renderer.Drawable = this.drawingarea1.GdkWindow;
					//renderer.Gc = this.drawingarea1.Style.BlackGC;




					layout.SetText (inc.ToString ());
					//gwin.DrawLayout (gc, p.X + 1, p.Y + 5, layout);
					drawable.DrawLayout (gc, p.X + 1, p.Y + 5, layout);
					//renderer.DrawLayout (layout, p.X, p.Y);

					//renderer.SetOverrideColor (RenderPart.Foreground, Gdk.Color.Zero);
					//renderer.Drawable = null;
					//renderer.Gc = null;

				}

				foreach (var tuple in pointFrame.scalePointsY) {
					Gdk.Point p = tuple.Item1;
					Decimal inc = tuple.Item2;

					drawable.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);

					layout.SetText (inc.ToString ());

					drawable.DrawLayout (gc, p.X + 5, p.Y + 1, layout);

				}


				gc.SetLineAttributes (3, LineStyle.Solid, CapStyle.Butt, JoinStyle.Miter);

				gc.RgbFgColor = new Gdk.Color (0, 250, 0);

				drawable.DrawLines (gc, pointFrame.bidPoints.ToArray ());
				//gwin.DrawLines (gc, pointFrame.bidPoints.ToArray ());

				gc.RgbFgColor = new Gdk.Color (250, 0, 0);
				drawable.DrawLines (gc, pointFrame.askPoints.ToArray ());
				//gwin.DrawLines (gc, pointFrame.askPoints.ToArray ());

				if (!darkmode) {
					gc.RgbFgColor = new Gdk.Color (0, 0, 250);
				} else {
					gc.RgbFgColor = new Gdk.Color (0, 90, 255);
				}

				gc.SetLineAttributes (1, LineStyle.Solid, CapStyle.Round, JoinStyle.Round);
				foreach (Gdk.Point p in pointFrame.localBidPointsBlue) {
					//gwin.DrawLine(gc, p.X, p.Y, p.X, 0);

					//int cirsize = 5;
					//gwin.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);
					drawable.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);
				}

				foreach (Gdk.Point p in pointFrame.localAskPointsBlue) {
					//gwin.DrawLine (gc, p.X, p.Y, p.X, 0);

					//int cirsize = 5;
					//gwin.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);
					drawable.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);
				}

				gc.RgbFgColor = new Gdk.Color (250, 0, 0);

				foreach (Gdk.Point p in pointFrame.localBidPointsRed) {
					//gwin.DrawLine(gc, p.X, p.Y, p.X, 0);


					//gwin.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);
					drawable.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);

				}

				foreach (Gdk.Point p in pointFrame.localAskPointsRed) {
					//gwin.DrawLine (gc, p.X, p.Y, p.X, 0);


					//gwin.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);
					drawable.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);
				}




				//Gdk.GC


				if (this.ordersTuple == null) {

					//gwin.DrawLine (gc, x, y, x, pointFrame.height);
					//return;
				} else {


					var orderTuple = this.ordersTuple;
					gc.Foreground = color_green;
					gc.RgbFgColor = color_green;
					foreach (AutomatedOrder order in orderTuple.Item1) {
						//decimal am = order.taker_pays.amount;


						//decimal yyy = pointFrame.height - (am * pointFrame.height) / pointFrame.highestypoint;


						decimal pric = order.taker_pays.GetNativeAdjustedPriceAt (order.taker_gets);


						decimal xxx = ((pric - pointFrame.lowestxprice) * pointFrame.chartWidth) / pointFrame.rawxPriceWidth;

						//gwin.DrawArc (gc, true, (int)xxx, (int)yyy, 5, 5, 0, 360 * 64);

						drawable.DrawLine (gc, (int)xxx, 0, (int)xxx, pointFrame.chartHeight /*+ RULER_WIDTH */);
					}

					gc.Foreground = color_red;
					gc.RgbFgColor = color_red;
					foreach (AutomatedOrder order in orderTuple.Item2) {

						//decimal am = order.taker_gets.amount;


						//decimal yyy = pointFrame.height - (am * pointFrame.height) / pointFrame.highestypoint;

						decimal pric = order.taker_pays.GetNativeAdjustedCostAt (order.taker_gets);


						decimal xxx = ((pric - pointFrame.lowestxprice) * pointFrame.chartWidth) / pointFrame.rawxPriceWidth;

						//gwin.DrawArc (gc, true, (int)xxx, (int)yyy, 5, 5, 0, 360 * 64);

						drawable.DrawLine (gc, (int)xxx, (int)0, (int)xxx, pointFrame.chartHeight /*+ RULER_WIDTH*/);
					}
				}


				//this.chartBufferImage = null;
				



				gc.Dispose ();
			}
			


		}

		//Gdk.Image image = null;
		//private int lastWidth = 0;
		//private int lastHeight = 0;

#pragma warning disable 0414
		private bool scaleChanged = false;
#pragma warning restore 0414




		private int visible_bids = 100;
		private int visible_asks = 100;

		public void SetRippleWallet (RippleWallet rippleWallet)
		{
			this._rippleWallet = rippleWallet;
		}

		private RippleWallet _rippleWallet {
			get;
			set;
		}

		//private PointFrame pointFrame = null;

		public AutomatedOrder [] bids = null;

		public AutomatedOrder [] asks = null;


#pragma warning restore RECS0122 // Initializing field with default value is redundant
#if DEBUG
		private const string clsstr = nameof (DepthChartWidget) + DebugRippleLibSharp.colon;
#endif
	}

	/*
	public class PointFrameHolder {
		public PointFrameHolder () { 
		
		}
		public PointFrame A_initialPointFrame { get; set; }
		public PointFrame B_ordersPointFrame { get; set; }
		public PointFrame C_sumsPointFrane { get; set; }
		public PointFrame D_chartPointFrame { get; set; }

		public PointFrame E_scalexPointFrame { get; set; }


		public PointFrame F_scaletPointFrame { get; set; } 

		public PointFrame G_PointFrame { get; set; }





	}*/

	public class PointFrame
	{
		// This is a frame of points that the draw func can use to draw a graph. 

		public int numAsks = 0;
		public int numBids = 0;

		public decimal bidAmountSum = 0;
		public decimal askAmountSum = 0;

		public decimal userBidAmountSum = 0;
		public decimal userAskAmountSum = 0;

		public decimal highestAmount = 0; // summ
		public decimal lowestypointAmount = 0;
		public decimal highestxprice = 0;
		public decimal lowestxprice = 0;

		public decimal rawxPriceWidth = 0; // full spread

		public decimal midprice = 0;
		public Gdk.Point midChartpoint = default(Gdk.Point);

		public int drawingAreaWidth = 0;
		public int drawingAreaHeight = 0;
		// chart width and height in pixels
		public int chartWidth = 0; //r.Width;
		public int chartHeight = 0; //r.Height;

		public List<Gdk.Point> bidPoints = new List<Gdk.Point> ();
		public List<Gdk.Point> askPoints = new List<Gdk.Point> ();

		public List<Gdk.Point> localBidPointsBlue = new List<Gdk.Point> ();
		public List<Gdk.Point> localAskPointsBlue = new List<Gdk.Point> ();

		public List<Gdk.Point> localBidPointsRed = new List<Gdk.Point> ();
		public List<Gdk.Point> localAskPointsRed = new List<Gdk.Point> ();

		public List<Gdk.Point> oderClusterPoints = new List<Gdk.Point> ();

		public List<Tuple<Gdk.Point, Decimal>> scalePointsX = new List<Tuple<Gdk.Point, Decimal>> ();
		public List<Tuple<Gdk.Point, Decimal>> scalePointsY = new List<Tuple<Gdk.Point, decimal>> ();
		public AutomatedOrder [] bidsArray = null;

		public AutomatedOrder [] asksArray = null;




		public PointFrame Copy ()
		{



			PointFrame frame = new PointFrame {
				bidsArray = this.bidsArray,
				asksArray = this.asksArray,
				numAsks = this.numAsks,
				numBids = this.numBids,
				chartHeight = this.chartHeight,
				chartWidth = this.chartWidth,
				highestxprice = this.highestxprice,
				highestAmount = this.highestAmount,
				bidAmountSum = this.bidAmountSum,
				askAmountSum = this.askAmountSum,
				lowestxprice = this.lowestxprice,
				lowestypointAmount = this.lowestypointAmount,
				userBidAmountSum = this.userBidAmountSum,
				userAskAmountSum = this.userAskAmountSum,
				rawxPriceWidth = this.rawxPriceWidth,

			};


			return frame;
		}

	}

	/*
	public static class MyExtensions
	{
		public static System.Drawing.Bitmap ToBitmap (this Pixbuf pix)
		{
			TypeConverter tc = TypeDescriptor.GetConverter (typeof (Bitmap));

			//// Possible file formats are: "jpeg", "png", "ico" and "bmp"
			return (Bitmap)tc.ConvertFrom (pix.SaveToBuffer ("jpeg"));
		}
	}*/

}

