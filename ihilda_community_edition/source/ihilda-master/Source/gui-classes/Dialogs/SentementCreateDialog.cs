using System;
using Gtk;
using RippleLibSharp.Transactions;

namespace IhildaWallet
{
	public partial class SentementCreateDialog : Gtk.Dialog
	{
		public SentementCreateDialog ()
		{
			this.Build ();

			ListStore comboModel = new ListStore (typeof (string));

			SentimentRatingEnum [] sentimentEnums = (SentimentRatingEnum [])Enum.GetValues (typeof (SentimentRatingEnum));

			foreach (SentimentRatingEnum sent in sentimentEnums) {
				comboModel.AppendValues (sent.ToString ());
			}
			this.comboboxentry3.Model = comboModel;
		}

		public Sentiment GetSentiment ()
		{
			string asset = this.comboboxentry2.ActiveText;
			string sentiment = this.comboboxentry3.ActiveText;

			SentimentRatingEnum sen = (SentimentRatingEnum)Enum.Parse (typeof (SentimentRatingEnum), sentiment);

			Sentiment sentimen = new Sentiment ();

			sentimen.Match = asset;



			sentimen.Rating = sen.ToString ();

			return sentimen;
		}

		public static Sentiment DoDialog ()
		{
			
			SentementCreateDialog rcd = new SentementCreateDialog ();
			Sentiment ret = null;

			do {
				ResponseType res = (ResponseType)rcd.Run ();

				/* 
				 * One should prefer ResponseType != ok rather than ResponseType == cancel
				 * !ok would also include the window being closed prematurely 
				 */
				if (res != ResponseType.Ok) {
					//return null;
					ret = null;
					break;
				}

				ret = rcd.GetSentiment ();


			} while (ret == null);

			rcd.Destroy ();

			return ret;
		}
	}
}
