using System;
using System.Threading.Tasks;
using RippleLibSharp.Commands.Stipulate;
using RippleLibSharp.Result;
using RippleLibSharp.Transactions;
using RippleLibSharp.Network;
using IhildaWallet.Networking;
using Gtk;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SpreadWidget : Gtk.Bin
	{
		public SpreadWidget ()
		{
			this.Build ();

			Task.Factory.StartNew (async () => {

				while (_cont) {
					await Task.Delay (30000);
					Update ();
				}
			}
			);

		}

		~SpreadWidget()
		{
			_cont = false;
		}

		private bool _cont = true;

		private TradePair _tradePair = null;

		public void Set (TradePair tp)
		{
			_tradePair = tp;

			Application.Invoke (
				(object sender, EventArgs e) => {
					bidlabel.Text = "";
					asklabel.Text = "";
					spreadlabel.Text = "";

					label5.Visible = false;
					spreadlabel.Visible = false;
				}

			);

			Update ();
		}

		public void Update () {

			TradePair tp = _tradePair;
			if (tp == null) {
				return;
			}

			NetworkInterface ni = NetworkController.CurrentInterface;
			if (ni == null) {
				return;
			}

			Task<Response<BookOfferResult>> buyTask = 
				BookOffers.GetResult (
					tp.Currency_Counter,
					tp.Currency_Base,
					2,
					ni

			);
			Task<Response<BookOfferResult>> sellTask = 
			BookOffers.GetResult (
					tp.Currency_Base,
					tp.Currency_Counter,
					2,
					ni
			);

			Task.WaitAll ( new Task[] { buyTask, sellTask } );

			Offer[] buyoffers = buyTask?.Result?.result?.offers;
			Offer[] selloffers = sellTask?.Result?.result?.offers;

			Offer highestBid = null;
			Offer lowestAsk = null;

			Decimal bidPrice = 0;
			Decimal askPrice = 0;

			Decimal spread = 0;

			string bidLabelText = null;
			string askLabelText = null;

			string spreadLabelText = null;

			bool canSpread = true;
			if (buyoffers == null || buyoffers.Length < 1) {
				bidLabelText = "No Bids";
				canSpread = false;
			} else {
				highestBid = buyoffers[0];
				bidPrice = highestBid.TakerPays.GetNativeAdjustedPriceAt ( highestBid.TakerGets );
				bidLabelText = bidPrice.ToString ();

			}


			if (selloffers == null || selloffers.Length < 1) {
				askLabelText = "No asks";
				canSpread = false;
			} else {
				
				lowestAsk = selloffers[0];
				askPrice = lowestAsk.TakerPays.GetNativeAdjustedCostAt ( lowestAsk.taker_gets );
				askLabelText = askPrice.ToString ();
			}



			if (canSpread) {

				spread = ((askPrice - bidPrice) / askPrice) * 100;

				spread = Math.Round (spread, 2);

				spreadLabelText = spread.ToString () + "%";
			} else {
				spreadLabelText = "";
			}








			Application.Invoke (
				(object sender, EventArgs e) => {
					bidlabel.Text = bidLabelText;
					asklabel.Text = askLabelText;
					spreadlabel.Text = spreadLabelText;

					label5.Visible = canSpread;
					spreadlabel.Visible = canSpread;
				}

			);

		}
	}
}

