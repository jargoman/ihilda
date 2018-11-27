using System;
using RippleLibSharp.Util;
using RippleLibSharp.Transactions;
using IhildaWallet.Util;
using Gtk;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class TxWidget : Gtk.Bin
	{
		public TxWidget ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (TxWidget) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.TxWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);

			}
#endif
			this.Build ();


			if (!Program.darkmode) {
				Gdk.Color col = new Gdk.Color (255, 234, 254);
				this.eventbox1.ModifyBg (StateType.Normal, col);
			} else {
				Gdk.Color col = new Gdk.Color (5,5,5);
				this.eventbox1.ModifyBg (StateType.Normal, col);
			}





			this.label1.Text = Tx_Type;
			this.metalabel.UseMarkup = true;
			this.metalabel.Selectable = true;
			this.txlabel.UseMarkup = true;
			this.txlabel.Selectable = true;
			this.txidlabel.Selectable = true;
			optionsbutton.Clicked += (sender, e) => DoOptionsClicked ();



		}

		public string Tx_Type {
			get;
			set;
		}

		public string Tx_Text {
			get {
#if DEBUG
				string method_sig = clsstr + nameof (Tx_Text) + ".get" + DebugRippleLibSharp.colon;
				if (DebugIhildaWallet.TxWidget) {
					Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
				}
#endif
				return txlabel.LabelProp;
			}
			set {
				//this.textview2.Buffer
				//this.textview2
				//layout.Attributes.



				//layout = this.label1.CreatePangoLayout( "<markup>" + value + "</markup>");  //new Pango.Layout(this.drawingarea1.PangoContext);
				//layout.Width = Pango.Units.FromPixels(width);
				//layout.Wrap = Pango.WrapMode.Word;
				//layout.Alignment = Pango.Alignment.Left;
				//layout.FontDescription = Pango.FontDescription.FromString("Ahafoni CLM Bold 100");
				//layout.SetMarkup ("<markup>" + value + "</markup>");
				//this.drawingarea1.la
#if DEBUG
				string method_sig = clsstr + "TxWidget.Set : ";
				if (DebugIhildaWallet.TxWidget) {
					Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
				}
#endif
				this.txlabel.LabelProp = MarkupTag (value);
				//this.txlabel.Selectable = true;

				/*
				this.label1.ExposeEvent += delegate(object o, Gtk.ExposeEventArgs args) {
					string event_sig = "ExposeEvent : ";
					if (Debug.TxWidget) {
						Logging.writeLog(clsstr + event_sig + Debug.beginn);
					}
					this.label1.GdkWindow.DrawLayout (label1.Style.TextGC (Gtk.StateType.Normal), 5, 5, layout);
				};

				this.label1.GdkWindow.DrawLayout (label1.Style.TextGC (Gtk.StateType.Normal), 5, 5, layout);
				*/

			}
		}

		public string Meta_Text {
			get { return metalabel.LabelProp; }
			set {
				this.metalabel.LabelProp = MarkupTag (value);
				//this.metalabel.Selectable = true;
			}
		}

		private string MarkupTag (string s)
		{
			return "<markup>" + s + "</markup>";
		}

		public TxSummary Summary {
			get { return __Summary; }
			set {


				//this.checkbutton3.Label = value?.tx_type ?? "";
				this.label1.Text = value?.Tx_Type ?? "";
				this.Meta_Text = value?.Meta_Summary_Buy ?? "";
				this.Tx_Text = value?.Tx_Summary_Buy ?? "";
				this.txidlabel.Text = value?.Tx_id ?? "";

				DateTime dt = TimeHeler.ConvertRippleTimeToUnix (value.Time);

				this.dateLabel.Text = dt.ToLongDateString () + " " + dt.ToLongTimeString ();
				__Summary = value;
			}
		}

		private void SetBuyDesc ()
		{
			this.Meta_Text = __Summary.Meta_Summary_Buy;
			this.Tx_Text = __Summary.Tx_Summary_Buy;
		}

		private void SetSellDesc ()
		{
			this.Meta_Text = __Summary.Meta_Summary_Sell;
			this.Tx_Text = __Summary.Tx_Summary_Sell;
		}
#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private TxSummary __Summary = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		public void DoOptionsClicked ()
		{


			Gtk.Menu menu = new Gtk.Menu ();

			Gtk.MenuItem raw = new Gtk.MenuItem ("raw");
			raw.Show ();
			menu.Add (raw);

			raw.Activated += (object sender, EventArgs e) => {
				MessageDialog.ShowMessage ("Raw transaction", __Summary.RawJSON);
			};

			Gtk.MenuItem flip = new Gtk.MenuItem ("flip");
			flip.Show ();
			menu.Add (flip);

			flip.Activated += (object sender, EventArgs e) => {
				if (flipped) SetBuyDesc ();
				else SetSellDesc ();
				flipped = !flipped;
			};



			var ch = this.__Summary.Changes;
			AutomatedOrder [] offs = Profiteer.GetBuyBacks (ch);

			if (offs != null && offs.Length != 0) {

				string r = flipped ? "Repurchase" : "Resell";
				Gtk.MenuItem repurchase = new Gtk.MenuItem (r);
				repurchase.Show ();
				menu.Add (repurchase);

				repurchase.Activated += (object sender, EventArgs e) => {

					LicenseType licenseT = Util.LicenseType.SEMIAUTOMATED;
					if (LeIceSense.IsLicenseExempt (offs [0].taker_gets) || LeIceSense.IsLicenseExempt (offs [0].taker_pays)) {
						licenseT = LicenseType.NONE;
					}
					OrderSubmitWindow win = new OrderSubmitWindow (_rippleWallet, licenseT);

					win.SetOrders (offs);

				};
			}
			menu.Popup ();
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
		private bool flipped = false;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		//int width = 400;
		//int height = 300;

		//Pango.Layout layout;

#if DEBUG
		private const string clsstr = nameof (TxWidget) + DebugRippleLibSharp.colon;

#endif
	}
}

