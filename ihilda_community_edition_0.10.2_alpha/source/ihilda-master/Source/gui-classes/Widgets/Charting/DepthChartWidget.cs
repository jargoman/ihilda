using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using RippleLibSharp.Util;
using RippleLibSharp.Transactions;
using RippleLibSharp.Network;
using IhildaWallet.Networking;
using RippleLibSharp.Commands.Stipulate;
using RippleLibSharp.Result;

using Cairo;

using Gtk;
using Gdk;
using Pango;
using System.Text;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class DepthChartWidget : Gtk.Bin
	{
		public DepthChartWidget ()
		{
			this.Build ();

			if (this.drawingarea1 == null) {
				this.drawingarea1 = new DrawingArea ();
				this.drawingarea1.Show ();

				vbox2.Add (this.drawingarea1);
			}

			this.drawingarea1.AddEvents ((int) 
				(EventMask.ButtonPressMask    
					|EventMask.ButtonReleaseMask    
					|EventMask.KeyPressMask    
					|EventMask.PointerMotionMask
					|EventMask.LeaveNotifyMask

				));

			this.label1.UseMarkup = true;

			this.drawingarea1.ExposeEvent += Drawingarea1_ExposeEvent;
			this.drawingarea1.Show ();

			this.drawingarea1.MotionNotifyEvent += Drawingarea1_MotionNotifyEvent;

			this.drawingarea1.LeaveNotifyEvent += Drawingarea1_LeaveNotifyEvent;

			//this.drawingarea1.ConfigureEvent += Drawingarea1_ConfigureEvent;

			//this.drawingarea1.Realized += Drawingarea1_Realized;

			this.drawingarea1.ButtonPressEvent += Drawingarea1_ButtonPressEvent;
			this.hscale1.Digits = 0;
			this.hscale2.Digits = 0;

			this.hscale1.ValueChanged += (object sender, EventArgs e) => {
				max_bids = (int)hscale1.Value;
				this.Drawingarea1_ExposeEvent (null, null);

			};

			this.hscale2.ValueChanged += (object sender, EventArgs e) => {
				max_asks = (int)hscale2.Value;
				this.Drawingarea1_ExposeEvent (null, null);
			};

			Task.Factory.StartNew (async () => {

				while (_cont) {
					await Task.Delay (30000);
					UpdateBooks ();
				}
			}
			);

		}

		~DepthChartWidget ()
		{
			_cont = false;
		}

		private bool _cont = true;

		private TradeWindow ShowTradeWindow () {
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
				taker_gets = tradePair.Currency_Counter.DeepCopy(),
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

			Menu menu = new Menu();


#region buy_menus
			MenuItem buy = new MenuItem (
				"Prepare a <span fgcolor=\"green\">buy</span>  order at " 
				+ price.ToString() 
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

				TradeWindow tradeWindow = ShowTradeWindow();

				/*
				Offer offer = new Offer();

				offer.taker_gets = tradePair.currency_counter;
				offer.taker_pays = tradePair.currency_base;

				offer.taker_gets.amount = price * amount;

				offer.taker_pays.amount = amount;
				*/
				tradeWindow.SetBidOffer(buyOffer);



			};

			Gtk.MenuItem cassbuy = new MenuItem (
				                       "Cascade <span fgcolor=\"green\">buy</span> orders from "
				                       + price.ToString ()
				                       + " "
				                       + tradePair.Currency_Counter.currency
				                       + " per "
				                       + tradePair.Currency_Base.currency
			                       );
			cassbuy.Show();
			menu.Add(cassbuy);

			l = (Label)cassbuy.Child;
			l.UseMarkup = true;

			cassbuy.Activated += (object sender, EventArgs e) => {
				#if DEBUG
				if (DebugIhildaWallet.DepthChartWidget) {
					Logging.WriteLog("cassbuy selected");
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




				tradeWindow.InitiateCascade(buyOffer, OrderEnum.BID);
			};

			Gtk.MenuItem autobuy = new MenuItem ("Prepare an automated <span fgcolor=\"green\">buy</span> at " 
				+ price.ToString() 
				+ " "
				+ tradePair.Currency_Counter.currency
				+ " per "
				+ tradePair.Currency_Base.currency
				+ " ");
			autobuy.Show();
			menu.Add (autobuy);

			l = (Gtk.Label)autobuy.Child;
			l.UseMarkup = true;

			autobuy.Activated += (object sender, EventArgs e) => {
				#if DEBUG
				if (DebugIhildaWallet.DepthChartWidget) {
					Logging.WriteLog("autobuy selected");
				}			
				#endif

				TradeWindow tradeWindow = ShowTradeWindow ();

				tradeWindow.SetAutomatedBidOffer(buyOffer);

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
					Logging.WriteLog("sell selected");
				}			
				#endif

				TradeWindow tradeWindow = ShowTradeWindow();

				tradeWindow.SetAskOffer (sellOffer);

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
					Logging.WriteLog("casssell selected");
				}			
				#endif

				TradeWindow tradeWindow = ShowTradeWindow();
				tradeWindow.InitCascadedAskOffer(sellOffer);

			};

			Gtk.MenuItem autosell = new MenuItem (
				
				"Prepare an automated <span fgcolor=\"red\">sell</span> order at " 
				+ price.ToString() 
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
					Logging.WriteLog("autosell selected");
				}			
				#endif
				TradeWindow tradeWindow = ShowTradeWindow ();


				tradeWindow.SetAutomatedOffer(sellOffer, OrderEnum.ASK);





			};
#endregion


			menu.Popup ();
		}

		/*
		void Drawingarea1_Realized (object sender, EventArgs e)
		{
			calculateBidSums ();
			calculatePoints ();

		}
		*/

		public void CalculatePoints (PointFrame pointFrame) {

			 

			pointFrame.bidPoints = new List<Gdk.Point> ();
			pointFrame.askPoints = new List<Gdk.Point> ();

			pointFrame.localBidPointsRed = new List<Gdk.Point> ();
			pointFrame.localAskPointsRed = new List<Gdk.Point> ();


			pointFrame.localBidPointsBlue = new List<Gdk.Point> ();
			pointFrame.localAskPointsBlue = new List<Gdk.Point> ();


			decimal rawx = Decimal.Zero;
			decimal rawy = pointFrame.highestypoint;

			decimal localy = pointFrame.highestypoint;

			// We're calulating this backwards. Starting with totals bids
			decimal bidsGetsSpent = 0;



			for ( int i = 0; i < pointFrame.numBids; i++) {

				rawx = pointFrame.bids[i].TakerPays.GetNativeAdjustedPriceAt ( pointFrame.bids[i].taker_gets );

				// TODO deal with divide by zero exception that can occur. Why? How to prevent? handle the exception ect
				double resx = (double) ((rawx - pointFrame.lowestxpoint ) * (pointFrame.width / pointFrame.rawxWidth)); 
				double resy = (double) (rawy *  (pointFrame.height / pointFrame.highestypoint));

				rawy -= pointFrame.bids[i].TakerPays.amount;



				Gdk.Point point = new Gdk.Point ((int)resx, (int)resy /*- RULER_WIDTH*/);

				pointFrame.bidPoints.Add (point);

				RippleWallet rw = _rippleWallet;
				if (rw?.GetStoredReceiveAddress() == null) {
					continue;
				}

				if (rw.GetStoredReceiveAddress().Equals(pointFrame.bids[i].Account)) {

					resy = (double) (localy *  (pointFrame.height / pointFrame.highestypoint));
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
			rawx = pointFrame.asks[0].TakerGets.GetNativeAdjustedPriceAt (pointFrame.asks[0].TakerPays);


			//double resultx = (double)((rawx - lowestxpoint) * (width / rawxWidth));
			//double resulty = (double)(rawy * height / highestypoint);

			//Gdk.Point point1 = new Gdk.Point ((int)resultx, (int)resulty);

			//askPoints.Add (point1);


			rawy = pointFrame.highestypoint;
			localy = pointFrame.highestypoint;

			Decimal asksGetsSpent = Decimal.Zero;
			for (int i = 0; i < pointFrame.numAsks; i++) {

				rawx = pointFrame.asks[i].TakerGets.GetNativeAdjustedPriceAt (pointFrame.asks[i].TakerPays);
				rawy -= pointFrame.asks[i].TakerGets.amount;


				double resx = (double) ((rawx - pointFrame.lowestxpoint) * (pointFrame.width / pointFrame.rawxWidth));
				double resy = (double) (rawy * pointFrame.height / pointFrame.highestypoint);

				Gdk.Point point = new Gdk.Point ((int)resx, (int)resy /*- RULER_WIDTH*/);

				pointFrame.askPoints.Add (point);

				RippleWallet rw = _rippleWallet;
				if (rw?.GetStoredReceiveAddress() == null) {
					continue;
				}

				if (rw.GetStoredReceiveAddress().Equals(pointFrame.asks[i].Account)) {
					localy -= pointFrame.asks[i].TakerGets.amount;
					asksGetsSpent += pointFrame.asks [i].TakerGets.amount;
					resy = (double) (localy * pointFrame.height / pointFrame.highestypoint);

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

			int bidc = pointFrame.numBids;
			int askc = pointFrame.numAsks;

			Decimal lowestPrice = pointFrame.bids [bidc - 1].TakerGets.GetNativeAdjustedCostAt (pointFrame.bids [bidc - 1].TakerPays);
			Decimal highestPrice = pointFrame.asks [askc - 1].TakerGets.GetNativeAdjustedPriceAt (pointFrame.asks [askc -1].TakerPays);
			Decimal priceRange =  highestPrice - lowestPrice;


			Decimal targetIncrement = priceRange / 5;

			Decimal scaleGuess = 1000000000;

			int incr = 0;
			while ( scaleGuess > targetIncrement ) {

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




			while ( ex < pointFrame.highestxpoint ) {
				double esRes = (double)((ex - pointFrame.lowestxpoint) * (pointFrame.width / pointFrame.rawxWidth));

				Tuple<Gdk.Point, Decimal> tuple = new Tuple<Gdk.Point, decimal> (new Gdk.Point ((int)esRes, pointFrame.height + 2), ex);

				pointFrame.scalePoints.Add ( tuple );
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

			this.Drawingarea1_ExposeEvent (null,null);

		}

		void Drawingarea1_MotionNotifyEvent (object o, MotionNotifyEventArgs args)
		{


			PointFrame pointFrame = _pointFrame;

			if (!this.drawingarea1.IsDrawable) {
				return;
			}

			//r.Width;
			//r.Height;


			Gdk.Window gwin = this.drawingarea1.GdkWindow;
			//gwin.GetSize(out int width, out int height);

			gwin.Clear ();

			this.Drawingarea1_ExposeEvent (null,null);

			int x = (int)args.Event.X;
			int y = (int)args.Event.Y;

			decimal amount = (pointFrame.height - y) * pointFrame.highestypoint / pointFrame.height;

			if (amount <= Decimal.Zero) {
				return;
			}
			Gdk.GC gc = new Gdk.GC (gwin) {
				RgbFgColor = new Gdk.Color (0, 0, 0),
				RgbBgColor = new Gdk.Color (218, 112, 214)
			};
			Pango.Layout layout = new Pango.Layout (this.PangoContext);



			decimal price = pointFrame.lowestxpoint + (x * pointFrame.rawxWidth / pointFrame.width);

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

			layout.SetMarkup ( stringBuilder.ToString () );

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


			gwin.DrawLine (gc, x, y, x, pointFrame.height);




		}

		public void SetTradePair ( TradePair tp )
		{
			this._tradePair = tp;

			this.label1.Markup = "<b>" + tp.Currency_Base.currency + ":" + tp.Currency_Counter.currency + "</b>";
		}



		public void UpdateBooks (  ) {
			#if DEBUG
			String method_sig = clsstr + nameof (UpdateBooks) + DebugRippleLibSharp.both_parentheses;
#endif

			TradePair tradePair = _tradePair;

			if (tradePair == null) {
				#if DEBUG
				if (DebugIhildaWallet.DepthChartWidget) {
					Logging.WriteLog(method_sig + "tp == null");
				}
				#endif
				// todo the lines below
				//if (this.buyorderbooktablewidget != null) {
				//	this.buyorderbooktablewidget.clearTable();
				//}

				//if (this.sellorderbooktablewidget != null) {
				//	this.sellorderbooktablewidget.clearTable();
				//}
				return;
			}






				


			NetworkInterface ni = NetworkController.GetNetworkInterfaceGuiThread ();
			if (ni == null) {
				return;
			}

			this._tradePair.UpdateBalances (_rippleWallet.GetStoredReceiveAddress (), ni);

			Task<IEnumerable<AutomatedOrder>> bidsTask = Task.Run( delegate { return UpdateBids (ni, tradePair); } );
			Task<IEnumerable<AutomatedOrder>> askTask = Task.Run( delegate { return UpdateAsks (ni, tradePair);  }  );

			Task[] tasks = { bidsTask, askTask};
			Task.WaitAll (tasks);

			#if DEBUG
			if (DebugIhildaWallet.DepthChartWidget) {
				Logging.WriteLog(method_sig + "stopped Waiting");
				//Logging.writeLog("id type =" + id.GetType().ToString());
				//Logging.writeLog("id = " + id.ToString());
			}
#endif



			asks = askTask?.Result?.ToArray ();
			bids = bidsTask?.Result?.ToArray ();






		}

		public PointFrame GetPointFrame ()
		{

			// TODO is linq faster? is this too much copying?
			PointFrame pointFrame = new PointFrame ();



			pointFrame.numAsks = asks.Length < max_asks ? asks.Length : max_asks;
			pointFrame.numBids = bids.Length < max_bids ? bids.Length : max_bids;

			pointFrame.asks = asks;
			pointFrame.bids = bids;

			CalculateBidSums (pointFrame);
			CalculatePoints (pointFrame);

			return pointFrame;
		}

		public IEnumerable <AutomatedOrder> UpdateBids (NetworkInterface ni, TradePair tp) {
			

			#if DEBUG
			string method_sig = nameof(UpdateBids) + DebugRippleLibSharp.colon;
			if (DebugIhildaWallet.DepthChartWidget) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);

			}
			#endif

			RippleCurrency cur_base = tp.Currency_Base;
			if (cur_base == null) {
				#if DEBUG
				if (DebugIhildaWallet.DepthChartWidget) {
					Logging.WriteLog(method_sig + "cur_base == null, returning");
				}
				#endif
				return null;
			}

			RippleCurrency counter_currency = tp.Currency_Counter;
			if (counter_currency == null) {
				#if DEBUG
				if (DebugIhildaWallet.DepthChartWidget) {
					Logging.WriteLog(method_sig + "counter_currency == null, returning");
				}
				#endif
				return null;
			}

			Task< Response<BookOfferResult>> buyTask = BookOffers.GetResult (counter_currency, cur_base, ni);
			buyTask.Wait ();
			#if DEBUG

			if (DebugIhildaWallet.DepthChartWidget) {
				Logging.WriteLog(method_sig + "done waiting");
				//Logging.writeLog(method_sig + "e.Message = " + Debug.toAssertString(e.Message));
			}
			#endif

			Offer[] buys = buyTask?.Result?.result?.offers;

			IEnumerable<AutomatedOrder> buyoffers = AutomatedOrder.ConvertFromIEnumerableOrder ( /*account,*/ buys);

			return buyoffers;
		}

		public IEnumerable <AutomatedOrder> UpdateAsks (NetworkInterface ni, TradePair tp) {
			

			#if DEBUG
			string method_sig = nameof(UpdateAsks) + DebugRippleLibSharp.colon;
			if (DebugIhildaWallet.DepthChartWidget) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);

			}
			#endif

			RippleCurrency cur_base = tp.Currency_Base;
			if (cur_base == null) {
				#if DEBUG
				if (DebugIhildaWallet.DepthChartWidget) {
					Logging.WriteLog(method_sig + "cur_base == null, returning");
				}
				#endif
				return null;
			}

			RippleCurrency counter_currency = tp.Currency_Counter;
			if (counter_currency == null) {
				#if DEBUG
				if (DebugIhildaWallet.DepthChartWidget) {
					Logging.WriteLog(method_sig + "counter_currency == null, returning");
				}
				#endif
				return null;
			}

			Task< Response<BookOfferResult>> sellTask = BookOffers.GetResult (cur_base, counter_currency, ni);
			sellTask.Wait ();
			#if DEBUG

			if (DebugIhildaWallet.DepthChartWidget) {
				Logging.WriteLog(method_sig + "done waiting");
				//Logging.writeLog(method_sig + "e.Message = " + Debug.toAssertString(e.Message));
			}
			#endif
			Offer[] sells = sellTask?.Result?.result?.offers;

			IEnumerable<AutomatedOrder> selloffers = AutomatedOrder.ConvertFromIEnumerableOrder (/*account,*/ sells);

			return selloffers;

		}



		// NOTE HIGHEST VALUE not HIGHEST POSITION this got confusing quick

#pragma warning disable RECS0122 // Initializing field with default value is redundant





	
		TradePair _tradePair = null;

		// TODO adjust later
		int RULER_WIDTH = 35;
		public void CalculateBidSums (PointFrame pointFrame) {



			if (pointFrame.bids == null && pointFrame.asks == null) {
				return;
			}

			if (this.drawingarea1 == null) {
				return;
			}

			this.drawingarea1.GetSizeRequest (out pointFrame.width, out pointFrame.height);

			Gdk.Window gwin = this.drawingarea1.GdkWindow;
			//	return;


			gwin.GetSize( out pointFrame.width, out pointFrame.height);


			pointFrame.height -= RULER_WIDTH;

			if (pointFrame.bids != null) {
				for (int i = 0; i < pointFrame.numBids; i++) {
				//foreach ( AutomatedOrder ao in pointFrame.bids ) {

					//bidsum += ao.TakerPays.amount;
					pointFrame.bidsum += pointFrame.bids[i].TakerPays.amount;

					RippleWallet rw = _rippleWallet;
					if (rw?.GetStoredReceiveAddress() == null) {
						continue;
					}

					if (rw.GetStoredReceiveAddress().Equals(pointFrame.bids[i].Account)) {


						pointFrame.localbidsum += pointFrame.bids[i].TakerPays.amount;
					}
				}
			}

			if (pointFrame.asks != null) {
				for (int i = 0; i < pointFrame.numAsks; i++) {
				//foreach ( AutomatedOrder ao in pointFrame.asks ) {
					//asksum += ao.TakerPays.amount;
					pointFrame.asksum += pointFrame.asks[i].TakerGets.amount;

					RippleWallet rw = _rippleWallet;
					if (rw?.GetStoredReceiveAddress() == null) {
						continue;
					}

					if (rw.GetStoredReceiveAddress().Equals(pointFrame.asks[i].Account)) {


						pointFrame.localasksum += pointFrame.asks[i].TakerGets.amount;
					}
				}

			}

			AutomatedOrder lowestBid = pointFrame.bids [pointFrame.numBids - 1];
			AutomatedOrder highestAsk = pointFrame.asks[pointFrame.numAsks - 1];



			pointFrame.highestypoint = (pointFrame.bidsum > pointFrame.asksum) ? pointFrame.bidsum : pointFrame.asksum; // summ
			pointFrame.lowestypoint = 0;

			if ( highestAsk != null ) {
				pointFrame.highestxpoint = highestAsk.taker_gets.GetNativeAdjustedPriceAt ( highestAsk.taker_pays );
				if (lowestBid == null) {
					AutomatedOrder lowestAsk = pointFrame.bids.First ();

					pointFrame.lowestxpoint = lowestAsk.taker_gets.GetNativeAdjustedPriceAt ( lowestAsk.taker_pays );
				}
			}

			if ( lowestBid != null ) {
				pointFrame.lowestxpoint = lowestBid.taker_pays.GetNativeAdjustedPriceAt( lowestBid.taker_gets );
				if (highestAsk == null) {
					AutomatedOrder highestBid = pointFrame.asks.First ();
					pointFrame.highestxpoint = highestBid.taker_gets.GetNativeAdjustedPriceAt ( highestBid.taker_pays );
				}
			}

			pointFrame.rawxWidth = pointFrame.highestxpoint - pointFrame.lowestxpoint; // full spread
		}



		public void Drawingarea1_ExposeEvent (object sender, ExposeEventArgs args)
		{

			//DrawingArea da = (DrawingArea)sender;

			PointFrame pointFrame = _pointFrame;

			#if DEBUG
			if (DebugIhildaWallet.DepthChartWidget) {
				//Logging.writeLog ();
			}
			#endif



			if (!this.drawingarea1.IsDrawable) {
				return;
			}





			//decimal lowest_price = low

			//Context cr = Gdk.CairoHelper.Create ( da.GdkWindow );

			//Gtk.Requisition r = this.drawingarea1.SizeRequest ();

			//Gtk.Requisition r = this.SizeRequest ();



			Gdk.Window gwin = this.drawingarea1.GdkWindow;
			gwin.Clear ();
			gwin.GetSize(out pointFrame.width, out pointFrame.height);
			Gdk.GC gc = new Gdk.GC(gwin);

			gc.SetLineAttributes (3, LineStyle.Solid, CapStyle.Butt, JoinStyle.Miter);
			//CalculatePoints (pointFrame);

			//gwin.DrawRectangle (gc, true, new Gdk.Rectangle(1,1,100,10));

	

			gc.RgbFgColor = new Gdk.Color (0, 250, 0);
			gwin.DrawLines(gc, pointFrame.bidPoints.ToArray());

			gc.RgbFgColor = new Gdk.Color (250, 0, 0);
			gwin.DrawLines (gc,pointFrame.askPoints.ToArray());


			gc.RgbFgColor = new Gdk.Color (0,0,250);

			gc.SetLineAttributes (1, LineStyle.Solid, CapStyle.Round, JoinStyle.Round);
			foreach (Gdk.Point p in pointFrame.localBidPointsBlue) {
				//gwin.DrawLine(gc, p.X, p.Y, p.X, 0);

				int cirsize = 5;
				gwin.DrawArc(gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);

			}

			foreach (Gdk.Point p in pointFrame.localAskPointsBlue) {
				//gwin.DrawLine (gc, p.X, p.Y, p.X, 0);

				int cirsize = 5;
				gwin.DrawArc(gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);

			}

			gc.RgbFgColor = new Gdk.Color (250, 0, 0);

			foreach (Gdk.Point p in pointFrame.localBidPointsRed) {
				//gwin.DrawLine(gc, p.X, p.Y, p.X, 0);

				int cirsize = 5;
				gwin.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);

			}

			foreach (Gdk.Point p in pointFrame.localAskPointsRed) {
				//gwin.DrawLine (gc, p.X, p.Y, p.X, 0);

				int cirsize = 5;
				gwin.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);

			}


			gc.RgbFgColor = new Gdk.Color (0, 0, 0);
			gc.SetLineAttributes (2, LineStyle.Solid, CapStyle.Butt, JoinStyle.Miter);
			gwin.DrawLine (gc, 0, pointFrame.height - RULER_WIDTH, pointFrame.width, pointFrame.height - RULER_WIDTH);
			gc.SetLineAttributes (1, LineStyle.Solid,CapStyle.Butt,JoinStyle.Miter);
			foreach ( var tuple in pointFrame.scalePoints) {
				//gwin.DrawLine(gc, p.X, p.Y, p.X, 0);
				Gdk.Point p = tuple.Item1;
				Decimal inc = tuple.Item2;
				int cirsize = 5;
				gwin.DrawArc (gc, true, p.X - (cirsize / 2), p.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);

				Gdk.PangoRenderer renderer = Gdk.PangoRenderer.GetDefault (gwin.Screen);
				renderer.Drawable = this.drawingarea1.GdkWindow;
				renderer.Gc = this.drawingarea1.Style.BlackGC;

				Pango.Context context = this.CreatePangoContext ();

				Pango.Layout layout = new Pango.Layout (context);

				layout.Width = Pango.Units.FromPixels (pointFrame.width);
				layout.SetText (inc.ToString ());

				FontDescription desc = FontDescription.FromString ("Serif Bold 9");

				layout.FontDescription = desc;

				renderer.SetOverrideColor (RenderPart.Foreground, new Gdk.Color (0, 0, 0));
				layout.Alignment = Pango.Alignment.Left;

				gwin.DrawLayout (gc, p.X + 1, p.Y + 5, layout);
				//renderer.DrawLayout (layout, p.X, p.Y);

				renderer.SetOverrideColor (RenderPart.Foreground, Gdk.Color.Zero);
				renderer.Drawable = null;
				renderer.Gc = null;

			}

		}

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


	public class PointFrame {
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

		public int width = 0; //r.Width;
		public int height = 0; //r.Height;

		public List<Gdk.Point> bidPoints = new List<Gdk.Point> ();
		public List<Gdk.Point> askPoints = new List<Gdk.Point> ();

		public List<Gdk.Point> localBidPointsBlue = new List<Gdk.Point> ();
		public List<Gdk.Point> localAskPointsBlue = new List<Gdk.Point> ();

		public List<Gdk.Point> localBidPointsRed = new List<Gdk.Point> ();
		public List<Gdk.Point> localAskPointsRed = new List<Gdk.Point> ();

		public List< Tuple <Gdk.Point, Decimal>> scalePoints = new List< Tuple <Gdk.Point, Decimal>> ();

		public AutomatedOrder [] bids = null;

		public AutomatedOrder [] asks = null;

	}

}

