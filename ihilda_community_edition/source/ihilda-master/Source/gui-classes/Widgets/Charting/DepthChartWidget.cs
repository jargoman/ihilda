using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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

			TokenSource = new CancellationTokenSource ();
			var token = TokenSource.Token;
			this.darkmodecheckbox.Active = Program.darkmode;


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
				CopyBuffer ();
				//drawingarea1.
			};

			orderclusterwidget1.OnClusterChanged += (object sender, ClusterChangedEventArgs e) => {
				var pointframe = this.GetPointFrame ();

				decimal price = pointframe.midprice;

				var cluster = e.Cluster;

				var orders = cluster.GetOrders ((double)price, _tradePair);

				this.ordersTuple = orders;
				this.DrawChartToPixMap (pointframe);
				this.CopyBuffer ();
			};

			darkmodecheckbox.Clicked += (object sender, EventArgs e) => {
				var pointframe = this.GetPointFrame ();
				this.DrawChartToPixMap (pointframe);
				this.CopyBuffer ();
			};


			/*
var pointframe = this.GetPointFrame ();
this.DrawChartToPixMap (pointframe);
this.CopyBuffer ();
};*/

			this.hscale1.Digits = 0;
			this.hscale2.Digits = 0;

			this.hscale1.ValueChanged += (object sender, EventArgs e) => {
				max_bids = (int)hscale1.Value;
				scaleChanged = true;
				PointFrame pointFrame = _pointFrame;
				if (pointFrame == null) {
					return;
				}
				this.chartBufferImage = DrawChartToPixMap (pointFrame);
				CopyBuffer ( /*pointFrame*/ );
			};

			this.hscale2.ValueChanged += (object sender, EventArgs e) => {
				max_asks = (int)hscale2.Value;
				scaleChanged = true;
				//DrawChart (_pointFrame);
				PointFrame pointFrame = _pointFrame;
				if (pointFrame == null) {
					return;
				}

				this.chartBufferImage = DrawChartToPixMap (pointFrame);

				CopyBuffer ( /*pointFrame*/ );
			};

			this.drawingarea1.EnterNotifyEvent += (object o, EnterNotifyEventArgs args) => {
				scaleChanged = true;
			};
			Task.Factory.StartNew (async () => {

				while (!token.IsCancellationRequested) {
					await Task.Delay (30000, token);
					UpdateBooks (token);
					//System.GC.Collect ();
					//System.GC.WaitForPendingFinalizers ();
				}
			}
			);

		}

		~DepthChartWidget ()
		{
			TokenSource?.Cancel (); // = false;
			TokenSource?.Dispose ();
			//TokenSource = null;
			this.Dispose ();
		}

		private CancellationTokenSource TokenSource;
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

		void Drawingarea1_ButtonPressEvent (object o, ButtonPressEventArgs args)
		{

			TradePair tradePair = _tradePair;
			if (tradePair == null) {
				return;
			}

			PointFrame pointFrame = this._pointFrame;


			//Gdk.Window gwin = this.drawingarea1.GdkWindow;
			//gwin.GetSize (out int width, out int height);


			//decimal price = pointFrame.lowestxpoint + (x * pointFrame.rawxWidth / pointFrame.width);
			//decimal price2 = pointFrame.lowestxpoint + (x * pointFrame.width / pointFrame.rawxWidth);
			//decimal amount = (height - y) * pointFrame.highestypoint / pointFrame.height;

			decimal price = pointFrame.lowestxpoint + ((int)args.Event.X * pointFrame.rawxWidth / pointFrame.width);
			decimal amount = (pointFrame.height - (int)args.Event.Y) * pointFrame.highestypoint / pointFrame.height;

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
				Program.darkmode ? "Prepare a <span fgcolor=\"chartreuse\">buy</span>  order at" : "Prepare a <span fgcolor=\"green\">buy</span>  order at "
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
				Program.darkmode ? 
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
				Program.darkmode ?
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

			Gtk.MenuItem sell = new MenuItem ("Prepare a <span fgcolor=\"red\">sell</span> order at this price");
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
				"Cascade <span fgcolor=\"red\">sell</span> orders begining at "
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

				"Prepare an automated <span fgcolor=\"red\">sell</span> order at "
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

					OrderSubmitWindow orderSubmitWindow = new OrderSubmitWindow (_rippleWallet, Util.LicenseType.SEMIAUTOMATED);

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

					DrawChartToPixMap (pointFrame);
					CopyBuffer ();

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

			pointFrame.clusterPoints = new List<Gdk.Point> (); // cluster

			decimal rawx = Decimal.Zero;
			decimal rawy = pointFrame.highestypoint;

			decimal localy = pointFrame.highestypoint;

			// We're calulating this backwards. Starting with totals bids
			decimal bidsGetsSpent = 0;

			decimal midcalc = 0;

			for (int i = 0; i < pointFrame.numBids; i++) {

				rawx = pointFrame.bids [i].TakerPays.GetNativeAdjustedPriceAt (pointFrame.bids [i].taker_gets);
				if (i == 0) {
					midcalc += rawx;
				}

				// TODO deal with divide by zero exception that can occur. Why? How to prevent? handle the exception ect
				double resx = (double)((rawx - pointFrame.lowestxpoint) * (pointFrame.width / pointFrame.rawxWidth));
				double resy = (double)(rawy * (pointFrame.height / pointFrame.highestypoint));

				rawy -= pointFrame.bids [i].TakerPays.amount;



				Gdk.Point point = new Gdk.Point ((int)resx, (int)resy /*- RULER_WIDTH*/);

				pointFrame.bidPoints.Add (point);

				RippleWallet rw = _rippleWallet;
				if (rw?.GetStoredReceiveAddress () == null) {
					continue;
				}

				if (rw.GetStoredReceiveAddress ().Equals (pointFrame.bids [i].Account)) {

					resy = (double)(localy * (pointFrame.height / pointFrame.highestypoint));
					localy -= pointFrame.bids [i].TakerPays.amount;
					bidsGetsSpent += pointFrame.bids [i].TakerGets.amount;


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




			rawy = pointFrame.highestypoint;
			rawx = pointFrame.asks [0].TakerGets.GetNativeAdjustedPriceAt (pointFrame.asks [0].TakerPays);


			//double resultx = (double)((rawx - lowestxpoint) * (width / rawxWidth));
			//double resulty = (double)(rawy * height / highestypoint);

			//Gdk.Point point1 = new Gdk.Point ((int)resultx, (int)resulty);

			//askPoints.Add (point1);


			rawy = pointFrame.highestypoint;
			localy = pointFrame.highestypoint;

			Decimal asksGetsSpent = Decimal.Zero;
			for (int i = 0; i < pointFrame.numAsks; i++) {

				rawx = pointFrame.asks [i].TakerGets.GetNativeAdjustedPriceAt (pointFrame.asks [i].TakerPays);

				if (i == 0) {
					midcalc += rawx;
				}

				rawy -= pointFrame.asks [i].TakerGets.amount;


				double resx = (double)((rawx - pointFrame.lowestxpoint) * (pointFrame.width / pointFrame.rawxWidth));
				double resy = (double)(rawy * pointFrame.height / pointFrame.highestypoint);

				Gdk.Point point = new Gdk.Point ((int)resx, (int)resy /*- RULER_WIDTH*/);

				pointFrame.askPoints.Add (point);

				RippleWallet rw = _rippleWallet;
				if (rw?.GetStoredReceiveAddress () == null) {
					continue;
				}

				if (rw.GetStoredReceiveAddress ().Equals (pointFrame.asks [i].Account)) {
					localy -= pointFrame.asks [i].TakerGets.amount;
					asksGetsSpent += pointFrame.asks [i].TakerGets.amount;
					resy = (double)(localy * pointFrame.height / pointFrame.highestypoint);

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

			double px = (double)((pointFrame.midprice - pointFrame.lowestxpoint) * (pointFrame.width / pointFrame.rawxWidth));
			double py = (double)(0 * (pointFrame.height / pointFrame.highestypoint));


			pointFrame.midpoint = new Gdk.Point ((int)px, (int)py);

			int bidc = pointFrame.numBids;
			int askc = pointFrame.numAsks;

			Decimal lowestPrice = pointFrame.bids [bidc - 1].TakerGets.GetNativeAdjustedCostAt (pointFrame.bids [bidc - 1].TakerPays);
			Decimal highestPrice = pointFrame.asks [askc - 1].TakerGets.GetNativeAdjustedPriceAt (pointFrame.asks [askc - 1].TakerPays);
			Decimal priceRange = highestPrice - lowestPrice;


			Decimal targetIncrement = priceRange / 5;

			Decimal scaleGuess = 1000000000;

			int incr = 0;
			while (scaleGuess > targetIncrement) {

				if (incr == 0) {
					scaleGuess /= 2;
					incr++;
				} else if (incr == 1) {
					scaleGuess /= 2;
					incr++;
				} else {
					scaleGuess /= 2.5m;
					incr = 0;
				}

			}

			Decimal modulus = (lowestPrice % scaleGuess);
			Decimal evenNum = lowestPrice / scaleGuess;
			evenNum = Math.Round (evenNum);
			Decimal ex = evenNum * scaleGuess;




			while (ex < pointFrame.highestxpoint) {
				double esRes = (double)((ex - pointFrame.lowestxpoint) * (pointFrame.width / pointFrame.rawxWidth));

				Tuple<Gdk.Point, Decimal> tuple = new Tuple<Gdk.Point, decimal> (new Gdk.Point ((int)esRes, pointFrame.height + 2), ex);

				pointFrame.scalePoints.Add (tuple);
				ex += scaleGuess;
			}





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




			//Gdk.Window gwin = this.drawingarea1.GdkWindow;
			//gwin.GetSize(out int width, out int height);
			//gwin.Clear ();
			PointFrame pointFrame = _pointFrame;
			//DrawChart (_pointFrame);
			this.chartBufferImage = DrawChartToPixMap (pointFrame);
			CopyBuffer ( /*pointFrame*/ );
		}

		void Drawingarea1_SizeAllocated (object o, SizeAllocatedArgs args)
		{
			PointFrame pointFrame = _pointFrame;
			//DrawChart (_pointFrame);
			this.chartBufferImage = DrawChartToPixMap (pointFrame);
			CopyBuffer ( /*pointFrame*/ );
		}


		Gdk.Color color_orchid = new Gdk.Color (218, 112, 214);
		Gdk.Color color_black = new Gdk.Color (0, 0, 0);
		Gdk.Color color_red = new Gdk.Color (255, 0, 0);
		Gdk.Color color_green = new Gdk.Color (0, 255, 0);
		Gdk.Color Color_white = new Gdk.Color (255,255,255);
		Tuple<IEnumerable<AutomatedOrder>, IEnumerable<AutomatedOrder>> ordersTuple = null;

		private void SetOrdersTuple (decimal price)
		{
			var cluster = this.orderclusterwidget1.cluster;
			var orderTuple = cluster.GetOrders ((double)price, _tradePair);
			ordersTuple = orderTuple;
		}

		void Drawingarea1_MotionNotifyEvent (object o, MotionNotifyEventArgs args)
		{

			bool darkmode = darkmodecheckbox.Active;
			PointFrame pointFrame = _pointFrame;
			if (pointFrame == null) {
				return;
			}

			if (!this.drawingarea1.IsDrawable) {
				return;
			}

			//r.Width;
			//r.Height;


			Gdk.Window gwin = this.drawingarea1.GdkWindow;
			gwin.GetSize(out int gwinwidth, out int gwinheight);



			//this.Drawingarea1_ExposeEvent (null,null);

			int x = (int)args.Event.X;
			int y = (int)args.Event.Y;

			decimal amount = (pointFrame.height - y) * pointFrame.highestypoint / pointFrame.height;

			//Gdk.Image buff = chartBufferImage;

			if (chartBufferImage == null) {
				chartBufferImage = DrawChartToPixMap (pointFrame);
			}



			//Gdk.Image img = buff.GetImage (0, 0, pointFrame.width, pointFrame.height);
			//gwin.DrawImage (gc, img, 0, 0, 0, 0, pointFrame.width, pointFrame.height);

			CopyBuffer ( /* pointFrame */);

			if (amount <= Decimal.Zero) {
				return;
			}
			//return;

			Gdk.GC gc = new Gdk.GC (gwin);


			decimal price = pointFrame.lowestxpoint + (x * pointFrame.rawxWidth / pointFrame.width);

			if (!darkmode) {
				gc.RgbFgColor = color_black;
			} else {
				gc.RgbFgColor = Color_white;

			}
			gwin.DrawLine (gc, x, y, x, pointFrame.height);


			gc.RgbFgColor = color_black;
			gc.RgbBgColor = color_orchid;

			Pango.Layout layout = new Pango.Layout (this.PangoContext);



			//decimal amount = (pointFrame.height - y) * pointFrame.highestypoint / pointFrame.height;


			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append ("<span bgcolor=\"orchid\">Price: ");
			stringBuilder.Append (price.ToString ());
			stringBuilder.Append ("\nAmount: ");
			stringBuilder.AppendLine (amount.ToString ());

#if DEBUG
			if (DebugIhildaWallet.DepthChartWidget) {

				stringBuilder.Append ("x,y=");
				stringBuilder.Append (x.ToString ());
				stringBuilder.Append (",");
				stringBuilder.AppendLine (y.ToString ());
			}
#endif


			stringBuilder.Append ("</span>");

			layout.SetMarkup (stringBuilder.ToString ());

			int xoffset = 0;

			if (x < pointFrame.width / 2) {
				xoffset += 10;
			} else {
				xoffset -= 150;
			}

			int yoffset = 0;
			if (y < pointFrame.height / 2) {
				yoffset += 30;
			} else {
				yoffset -= 50;
			}
			//gwin.DrawRectangle (gc, true, new Gdk.Rectangle((int)args.Event.X,(int)args.Event.Y,100,10));
			gwin.DrawLayout (gc, x + xoffset, y + yoffset, layout);


			//gwin.DrawLine (gc, x


			gc?.Dispose ();

		}



		public void SetTradePair (TradePair tp)
		{
			this._tradePair = tp;

			this.label1.Markup = "<b>" + tp.Currency_Base.currency + ":" + tp.Currency_Counter.currency + "</b>";
		}



		public void UpdateBooks (CancellationToken token)
		{


#if DEBUG
			String method_sig = clsstr + nameof (UpdateBooks) + DebugRippleLibSharp.both_parentheses;
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



			NetworkInterface ni = NetworkController.GetNetworkInterfaceGuiThread ();
			if (ni == null) {

				ResetProgressBar ();
				return;
			}
			Task balTask = Task.Run ( delegate {
				this._tradePair.UpdateBalances (_rippleWallet.GetStoredReceiveAddress (), ni);
			}, token);



			Task<IEnumerable<AutomatedOrder>> bidsTask = 
				Task.Run (
					delegate { 
						return UpdateBids (ni, tradePair, token); 
					}
					, token
				);

			Task<IEnumerable<AutomatedOrder>> askTask = 
				Task.Run (
					delegate { 
						return UpdateAsks (ni, tradePair, token); 
					}, token
				);

			Task [] tasks = { bidsTask, askTask, balTask };

			while (

				(!bidsTask.IsCompleted && 
				!bidsTask.IsCanceled && 
				!bidsTask.IsFaulted) || 
				!(askTask.IsCompleted && 
				!askTask.IsCanceled && 
				!askTask.IsFaulted)) 
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


	    		var task1 = Task.Run ( delegate {
				asks = askTask?.Result?.ToArray ();
			});

			var task2 = Task.Run ( delegate {
				bids = bidsTask?.Result?.ToArray ();

			});

			while (
				((!task1.IsCanceled && !task1.IsCompleted && !task1.IsFaulted) || (!task2.IsFaulted && !task2.IsCanceled && !task2.IsCompleted)) && !token.IsCancellationRequested
			) {
				Task.WaitAll (new Task [] { task1, task2 }, 500, token);
				PumpUI (token);
			}

			this.scaleChanged = true;

			ResetProgressBar ();



			Application.Invoke ( delegate {
				if (drawingarea1 == null) {
					return;
				}

				if (!this.drawingarea1.IsDrawable) {
					return;
				}



				var pointFrame = _pointFrame;



				if (chartBufferImage == null) {
					chartBufferImage = DrawChartToPixMap (pointFrame);
				}

				CopyBuffer ();


			});

			



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

		public PointFrame GetPointFrame ()
		{


			int numAsk = 0;
			int numBid = 0;

			if (asks != null) {
				
				numAsk = asks.Length < max_asks ? asks.Length : max_asks;
			}

			if (bids != null) {
				numBid = bids.Length < max_bids ? bids.Length : max_bids;
			}

			if (numAsk == 0 && numBid == 0) {
				return null;
			}


			// TODO is linq faster? is this too much copying?
			PointFrame pointFrame = new PointFrame {
				numAsks = numAsk,
				numBids = numBid,

				asks = asks,
				bids = bids
			};



			CalculateBidSums (pointFrame);
			CalculatePoints (pointFrame);

			return pointFrame;
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
		int RULER_WIDTH = 35;
		public void CalculateBidSums (PointFrame pointFrame)
		{

			if (pointFrame == null) {
				return;
			}

			if (pointFrame.bids == null && pointFrame.asks == null) {
				return;
			}

			if (this.drawingarea1 == null) {
				return;
			}

			//this.drawingarea1.GetSizeRequest (out pointFrame.width, out pointFrame.height);

			Gdk.Window gwin = this.drawingarea1.GdkWindow;

			if (gwin == null) {
				return;
			}

			gwin.GetSize (out pointFrame.width, out pointFrame.height);


			pointFrame.height -= RULER_WIDTH;

			if (pointFrame.bids != null) {
				for (int i = 0; i < pointFrame.numBids; i++) {
					//foreach ( AutomatedOrder ao in pointFrame.bids ) {

					//bidsum += ao.TakerPays.amount;
					pointFrame.bidsum += pointFrame.bids [i].TakerPays.amount;

					RippleWallet rw = _rippleWallet;
					if (rw?.GetStoredReceiveAddress () == null) {
						continue;
					}

					if (rw.GetStoredReceiveAddress ().Equals (pointFrame.bids [i].Account)) {


						pointFrame.localbidsum += pointFrame.bids [i].TakerPays.amount;
					}
				}
			}

			if (pointFrame.asks != null) {
				for (int i = 0; i < pointFrame.numAsks; i++) {
					//foreach ( AutomatedOrder ao in pointFrame.asks ) {
					//asksum += ao.TakerPays.amount;
					pointFrame.asksum += pointFrame.asks [i].TakerGets.amount;

					RippleWallet rw = _rippleWallet;
					if (rw?.GetStoredReceiveAddress () == null) {
						continue;
					}

					if (rw.GetStoredReceiveAddress ().Equals (pointFrame.asks [i].Account)) {


						pointFrame.localasksum += pointFrame.asks [i].TakerGets.amount;
					}
				}

			}

			AutomatedOrder lowestBid = pointFrame.bids [pointFrame.numBids - 1];
			AutomatedOrder highestAsk = pointFrame.asks [pointFrame.numAsks - 1];



			pointFrame.highestypoint = (pointFrame.bidsum > pointFrame.asksum) ? pointFrame.bidsum : pointFrame.asksum; // summ
			pointFrame.lowestypoint = 0;

			if (highestAsk != null) {
				pointFrame.highestxpoint = highestAsk.taker_gets.GetNativeAdjustedPriceAt (highestAsk.taker_pays);
				if (lowestBid == null) {
					AutomatedOrder lowestAsk = pointFrame.bids.First ();

					pointFrame.lowestxpoint = lowestAsk.taker_gets.GetNativeAdjustedPriceAt (lowestAsk.taker_pays);
				}
			}

			if (lowestBid != null) {
				pointFrame.lowestxpoint = lowestBid.taker_pays.GetNativeAdjustedPriceAt (lowestBid.taker_gets);
				if (highestAsk == null) {
					AutomatedOrder highestBid = pointFrame.asks.First ();
					pointFrame.highestxpoint = highestBid.taker_gets.GetNativeAdjustedPriceAt (highestBid.taker_pays);
				}
			}

			pointFrame.rawxWidth = pointFrame.highestxpoint - pointFrame.lowestxpoint; // full spread
		}



		public void Drawingarea1_ExposeEvent (object sender, ExposeEventArgs args)
		{
			if (drawingarea1 == null) {
				return;
			}

			if (!this.drawingarea1.IsDrawable) {
				return;
			}



			PointFrame pointFrame = _pointFrame;



			if (chartBufferImage == null) {
				chartBufferImage = DrawChartToPixMap (pointFrame);
			}

			CopyBuffer (/*pointFrame*/);

		}

		public void CopyBuffer (/*PointFrame pointFrame*/)
		{



			Gdk.Window gwin = this.drawingarea1?.GdkWindow;
			if (gwin == null) {
				return;
			}

			if (chartBufferImage == null) {
				return;
			}

			Gdk.GC gc = new Gdk.GC (gwin);
			//chartBufferImage.GetSize (out int wid, out int hei);

			//Gdk.Image img = chartBufferImage.GetImage (0, 0, wid, hei);
			gwin.DrawImage (gc, chartBufferImage, 0, 0, 0, 0, chartBufferImage.Width, chartBufferImage.Height);

			//gc.Dispose ();

			//img.Dispose ();
			gc.Dispose ();

			//System.GC.Collect ();
			//System.GC.WaitForPendingFinalizers ();
		}



		private Gdk.Image chartBufferImage = null;
		private Pixmap lastpixmap = null;
		public Gdk.Image DrawChartToPixMap (PointFrame pointFrame)
		{
			if (pointFrame == null) {
				return null;
			}



			int cirsize = 5;
			//

#if DEBUG
			if (DebugIhildaWallet.DepthChartWidget) {
				//Logging.writeLog ();
			}
#endif



			Gdk.Window gwin = this.drawingarea1?.GdkWindow;

			if (gwin == null) {
				return null;
			}

			//gwin.Clear ();
			gwin.GetSize (out int gwinwidth, out int gwinheight);


			if (lastpixmap == null) {
				lastpixmap = new Pixmap (drawingarea1.GdkWindow, gwinwidth, gwinheight);


			}
			this.lastpixmap.GetSize (out int pixwidth, out int pixheight);

			if (gwinwidth != pixwidth || gwinheight != pixheight ) {
				
				this.lastpixmap?.Dispose ();
				lastpixmap = new Pixmap (drawingarea1.GdkWindow, gwinwidth, gwinheight);

				//this.chartBufferImage = new Pixmap (drawingarea1.GdkWindow, gwinwidth, gwinheight);
				//System.GC.Collect ();
				//System.GC.WaitForPendingFinalizers ();
			} else {
				
			}

			/*
			var gra = System.Drawing.Graphics.FromImage ((System.Drawing.Image)gdkImage.g);




			System.Drawing.FontFamily fontFamily = new System.Drawing.FontFamily ("Arial");
			System.Drawing.Font font = new System.Drawing.Font (
			   fontFamily,
			   10,
			   FontStyle.Regular,
			   GraphicsUnit.Point);

			RectangleF rectF = new RectangleF (0, 0, 300, 20);
			SolidBrush solidBrush = new SolidBrush (System.Drawing.Color.Black);

			*/





			//gra.DrawString ("test", new System.Drawing.Font());

			//gra.DrawString ("Ihilda", font, solidBrush, rectF);

			//Gdk.GC gc = new Gdk.GC (gwin);

			var pixmap = this.lastpixmap;
			Gdk.GC gc = new Gdk.GC (pixmap);

			bool darkmode = darkmodecheckbox.Active;

			Pango.Context context = this.CreatePangoContext ();

			Pango.Layout layout = new Pango.Layout (context) {
				Width = Pango.Units.FromPixels (pointFrame.width)
			};


			FontDescription desc = FontDescription.FromString ("Serif Bold 100");

			layout.FontDescription = desc;

			//renderer.SetOverrideColor (RenderPart.Foreground, new Gdk.Color (0, 0, 0));
			layout.Alignment = Pango.Alignment.Center;

			//gwin.DrawImage (gc, gdkImage, 0, 0, 0, 0, pointFrame.width, pointFrame.height);
			layout.SetText ("Ihilda");

			if (!darkmode) {
				gc.RgbFgColor = new Gdk.Color (255, 255, 255);
			} else {
				gc.RgbFgColor = new Gdk.Color (39, 40, 33);
			}

			pixmap.DrawRectangle (gc, true, 0, 0, gwinwidth, gwinheight);

			if (!darkmode) {
				gc.RgbFgColor = new Gdk.Color (250, 235, 249);
			} else {
				gc.RgbFgColor = new Gdk.Color (81, 21, 78);
			}


			pixmap.DrawLayout (gc, 25, 25, layout);



			if (!darkmode) {
				gc.RgbFgColor = color_black;
			} else {
				gc.RgbFgColor = Color_white;
			}
			//gc.RgbFgColor = new Gdk.Color (255,255,255);
			gc.SetLineAttributes (2, LineStyle.Solid, CapStyle.Butt, JoinStyle.Miter);

			//gwin.DrawLine (gc, 0, pointFrame.height - RULER_WIDTH, pointFrame.width, pointFrame.height - RULER_WIDTH);

			// either windowwidth - Ruler_width OR pointframeheight
			pixmap.DrawLine (gc, 0, pointFrame.height, pointFrame.width, pointFrame.height);

			gc.SetLineAttributes (1, LineStyle.Solid, CapStyle.Butt, JoinStyle.Miter);

			layout.Alignment = Pango.Alignment.Left;
			layout.FontDescription = FontDescription.FromString ("Serif Bold 9");

			foreach (var tuple in pointFrame.scalePoints) {
				//gwin.DrawLine(gc, p.X, p.Y, p.X, 0);
				Gdk.Point p = tuple.Item1;
				Decimal inc = tuple.Item2;

				//gwin.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);
				pixmap.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);
				//Gdk.PangoRenderer renderer = Gdk.PangoRenderer.GetDefault (gwin.Screen);
				//renderer.Drawable = this.drawingarea1.GdkWindow;
				//renderer.Gc = this.drawingarea1.Style.BlackGC;




				layout.SetText (inc.ToString ());
				//gwin.DrawLayout (gc, p.X + 1, p.Y + 5, layout);
				pixmap.DrawLayout (gc, p.X + 1, p.Y + 5, layout);
				//renderer.DrawLayout (layout, p.X, p.Y);

				//renderer.SetOverrideColor (RenderPart.Foreground, Gdk.Color.Zero);
				//renderer.Drawable = null;
				//renderer.Gc = null;

			}


			gc.SetLineAttributes (3, LineStyle.Solid, CapStyle.Butt, JoinStyle.Miter);

			gc.RgbFgColor = new Gdk.Color (0, 250, 0);

			pixmap.DrawLines (gc, pointFrame.bidPoints.ToArray ());
			//gwin.DrawLines (gc, pointFrame.bidPoints.ToArray ());

			gc.RgbFgColor = new Gdk.Color (250, 0, 0);
			pixmap.DrawLines (gc, pointFrame.askPoints.ToArray ());
			//gwin.DrawLines (gc, pointFrame.askPoints.ToArray ());

			if (!darkmode) {
				gc.RgbFgColor = new Gdk.Color (0, 0, 250);
			} else {
				gc.RgbFgColor = new Gdk.Color (0,90,255);
			}

			gc.SetLineAttributes (1, LineStyle.Solid, CapStyle.Round, JoinStyle.Round);
			foreach (Gdk.Point p in pointFrame.localBidPointsBlue) {
				//gwin.DrawLine(gc, p.X, p.Y, p.X, 0);

				//int cirsize = 5;
				//gwin.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);
				pixmap.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);
			}

			foreach (Gdk.Point p in pointFrame.localAskPointsBlue) {
				//gwin.DrawLine (gc, p.X, p.Y, p.X, 0);

				//int cirsize = 5;
				//gwin.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);
				pixmap.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);
			}

			gc.RgbFgColor = new Gdk.Color (250, 0, 0);

			foreach (Gdk.Point p in pointFrame.localBidPointsRed) {
				//gwin.DrawLine(gc, p.X, p.Y, p.X, 0);


				//gwin.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);
				pixmap.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);

			}

			foreach (Gdk.Point p in pointFrame.localAskPointsRed) {
				//gwin.DrawLine (gc, p.X, p.Y, p.X, 0);


				//gwin.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);
				pixmap.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);
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


					decimal xxx = ((pric - pointFrame.lowestxpoint) * pointFrame.width) / pointFrame.rawxWidth;

					//gwin.DrawArc (gc, true, (int)xxx, (int)yyy, 5, 5, 0, 360 * 64);

					pixmap.DrawLine (gc, (int)xxx, 0, (int)xxx, pointFrame.height /*+ RULER_WIDTH */);
				}

				gc.Foreground = color_red;
				gc.RgbFgColor = color_red;
				foreach (AutomatedOrder order in orderTuple.Item2) {

					//decimal am = order.taker_gets.amount;


					//decimal yyy = pointFrame.height - (am * pointFrame.height) / pointFrame.highestypoint;

					decimal pric = order.taker_pays.GetNativeAdjustedCostAt (order.taker_gets);


					decimal xxx = ((pric - pointFrame.lowestxpoint) * pointFrame.width) / pointFrame.rawxWidth;

					//gwin.DrawArc (gc, true, (int)xxx, (int)yyy, 5, 5, 0, 360 * 64);

					pixmap.DrawLine (gc, (int)xxx, (int)0, (int)xxx, pointFrame.height /*+ RULER_WIDTH*/);
				}
			}

			this.chartBufferImage?.Dispose ();
			this.chartBufferImage = null;
			this.chartBufferImage = pixmap.GetImage (0, 0, gwinwidth, gwinheight);



			gc.Dispose ();

			return chartBufferImage;


		}

		//Gdk.Image image = null;
		//private int lastWidth = 0;
		//private int lastHeight = 0;

#pragma warning disable 0414
		private bool scaleChanged = false;
#pragma warning restore 0414




		private int max_bids = 100;
		private int max_asks = 100;

		public void SetRippleWallet (RippleWallet rippleWallet)
		{
			this._rippleWallet = rippleWallet;
		}

		private RippleWallet _rippleWallet {
			get;
			set;
		}

		private PointFrame _pointFrame {
			get { return GetPointFrame (); }

		}

		public AutomatedOrder [] bids = null;

		public AutomatedOrder [] asks = null;


#pragma warning restore RECS0122 // Initializing field with default value is redundant
#if DEBUG
		private const string clsstr = nameof (DepthChartWidget) + DebugRippleLibSharp.colon;
#endif
	}


	public class PointFrame
	{
		// This is a frame of points that the draw func can use to draw a graph. 

		public int numAsks = 0;
		public int numBids = 0;

		public decimal bidsum = 0;

		public decimal asksum = 0;
		public decimal localbidsum = 0;
		public decimal localasksum = 0;

		public decimal highestypoint = 0; // summ
		public decimal lowestypoint = 0;
		public decimal highestxpoint = 0;
		public decimal lowestxpoint = 0;

		public decimal rawxWidth = 0; // full spread

		public decimal midprice = 0;
		public Gdk.Point midpoint = default(Gdk.Point);

		public int width = 0; //r.Width;
		public int height = 0; //r.Height;

		public List<Gdk.Point> bidPoints = new List<Gdk.Point> ();
		public List<Gdk.Point> askPoints = new List<Gdk.Point> ();

		public List<Gdk.Point> localBidPointsBlue = new List<Gdk.Point> ();
		public List<Gdk.Point> localAskPointsBlue = new List<Gdk.Point> ();

		public List<Gdk.Point> localBidPointsRed = new List<Gdk.Point> ();
		public List<Gdk.Point> localAskPointsRed = new List<Gdk.Point> ();

		public List<Gdk.Point> clusterPoints = new List<Gdk.Point> ();

		public List<Tuple<Gdk.Point, Decimal>> scalePoints = new List<Tuple<Gdk.Point, Decimal>> ();

		public AutomatedOrder [] bids = null;

		public AutomatedOrder [] asks = null;

	}


	public static class MyExtensions
	{
		public static System.Drawing.Bitmap ToBitmap (this Pixbuf pix)
		{
			TypeConverter tc = TypeDescriptor.GetConverter (typeof (Bitmap));

			//// Possible file formats are: "jpeg", "png", "ico" and "bmp"
			return (Bitmap)tc.ConvertFrom (pix.SaveToBuffer ("jpeg"));
		}
	}

}

