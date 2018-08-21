﻿using System;
using RippleLibSharp.Keys;
using System.Collections.Generic;
using RippleLibSharp.Transactions;

namespace IhildaWallet
{
	public static class Profiteer
	{
		/*
		
		public Profiteer (RippleAddress account)
		{
			
		}
		*/



		public static AutomatedOrder[] GetBuyBacks (OrderChange[] ords) {


			List<AutomatedOrder> lst = new List<AutomatedOrder> ();
			foreach ( OrderChange o in ords) {

				AutomatedOrder off = new AutomatedOrder {

					//Decimal price = off.TakerGets.getPriceAt (off.TakerPays);

					Account = o.Account,

					TakerGets = o.TakerPays / 1.007m,
					TakerPays = o.TakerGets * 1.007m
				};

				lst.Add (off);
			}

			return lst.ToArray ();
		}

		public static AutomatedOrder[] GetBuyBacks ( IEnumerable<AutomatedOrder> ords) {
			List<AutomatedOrder> lst = new List<AutomatedOrder> ();
			foreach ( AutomatedOrder o in ords) {

				AutomatedOrder off = new AutomatedOrder {

					//Decimal price = off.TakerGets.getPriceAt (off.TakerPays);

					Account = o.Account,

					TakerGets = o.TakerPays / 1.007m,
					TakerPays = o.TakerGets * 1.007m
				};

				lst.Add (off);
			}

			return lst.ToArray ();
		}

		public static AutomatedOrder GetBuyBack (AutomatedOrder off, ProfitStrategy strategy) {

			if (off == null) {
				// TODO
			}

			Decimal getsSpec = Decimal.Zero;
			Decimal paysSpec = Decimal.Zero;
			if (strategy.Speculate > Decimal.One) {
				SentimentManager sentimentManager = new SentimentManager (off.Account);
				sentimentManager.LoadSentiments ();
				string getsCur = off.taker_pays.currency; // opposite is intended
				string paysCur = off.taker_gets.currency;

				Sentiment getsSent = sentimentManager.LookUpSentiment (getsCur);
				Sentiment paysSent = sentimentManager.LookUpSentiment (paysCur);


				int lessShare = 0;
				int moreshare = 0;
				if (getsSent != null) {
					SentimentRatingEnum en = (SentimentRatingEnum)Enum.Parse (typeof (SentimentRatingEnum), getsSent.Rating);
					lessShare = (int) en;
				} else {
					lessShare = (int)SentimentRatingEnum.Neutral;
				}
				if (paysSent != null) {
					SentimentRatingEnum en = (SentimentRatingEnum)Enum.Parse (typeof (SentimentRatingEnum), paysSent.Rating);
					moreshare = (int)en;
				} else {
					moreshare = (int)SentimentRatingEnum.Neutral;
				}



				int totalShare = lessShare + moreshare;
				Decimal profitPerShare = (strategy.Speculate - 1) / totalShare;
				getsSpec = profitPerShare * lessShare;
				paysSpec = profitPerShare * moreshare;
			}

			string mark = off.BotMarking;
			bool isnum = Decimal.TryParse (mark, out decimal number );

			Decimal expl = Decimal.Zero;
			Decimal exgm = Decimal.Zero;

			if (strategy.Exp_Pay_Less != Decimal.Zero ) {
				if (!isnum) {
					throw new FormatException ("Mark " + mark + " is not a valid exponent");
				}

				expl = (Decimal)(Math.Pow ((double)strategy.Exp_Pay_Less, (double)number));
			}

			if ( strategy.Exp_Get_More != Decimal.Zero ) {
				if (!isnum) {
					throw new FormatException ("Mark " + mark + " is not a valid exponent");
				}

				exgm = (Decimal)(Math.Pow ((double)strategy.Exp_Get_More, (double)number));
			}






			AutomatedOrder ao = new AutomatedOrder {
				Account = off.Account,
				TakerGets = off.TakerPays / (strategy.Pay_Less + getsSpec + expl),
				TakerPays = off.TakerGets * (strategy.Get_More + paysSpec + exgm)
			};



			return ao;

		}

	}
}

