using System;
using RippleLibSharp.Transactions;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class OrderWidget : Gtk.Bin
	{
		public OrderWidget ()
		{
			this.Build ();

			this.buylabel.UseMarkup = true;
			this.soldlabel.UseMarkup = true;


			this.SetToBlank ();

		}

		public bool Selected {
			get { 
				return _Sel; 
			}
			set { 
				checkbutton1.Active = value;
				_Sel = value; 
			}
		}

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private bool _Sel = false;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		private void SetToBlank () {



			this.buylabel.Markup = "";
			this.soldlabel.Markup = "";
			this.pricelabel.Text = "";
			this.checkbutton1.Label = "";
			this.Hide ();
		

			//this.button793.Visible = false;

			//_offer = null;  // TODO should this be here in gui code?


		}

		private void SetOrder (Offer offer) {
			#if DEBUG
			string method_sig = clsstr + nameof (SetOrder) + DebugRippleLibSharp.left_parentheses + nameof (Offer) + DebugRippleLibSharp.space_char + nameof (offer) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.OrderWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
			#endif
			SetToBlank ();
			if (offer == null) {
				#if DEBUG
				if (DebugIhildaWallet.OrderWidget) {
					Logging.WriteLog (method_sig + "offer == null, setting gui to blank/hidden\n");
				}
				#endif



				return;
			}

			if (offer?.taker_pays == null || offer?.taker_gets == null) {
				#if DEBUG
				if (DebugIhildaWallet.OrderWidget) {
					Logging.WriteLog (method_sig + "offer.taker_pays == null || offer.taker_gets == null, returning\n");
					return;
				}
				#endif
			}

			if (offer?.quality == null) {
				#if DEBUG
				if (DebugIhildaWallet.OrderWidget) {
					Logging.WriteLog (method_sig + "offer.quality == null, returning\n");
				}
				#endif
			}

			this.Show ();
			this.hbox1.Show ();
			//this.hbox1.ShowAll ();

			this.buylabel.Markup = "<markup><span foreground=" 
				+ "'green'" + ">" 
				+ (offer?.taker_pays?.ToString() ??
					#if DEBUG
					"null"
					#else
					""
					#endif
				) 
				+ "</span></markup>";
			//this.buylabel.Text += (string)offer.taker_pays.value;

			this.soldlabel.Markup = "<markup><span foreground=" 
				+ "'red'" + ">" 
				+ (offer?.taker_gets?.ToString() ?? 
					#if DEBUG
					"null"
					#else
					""
					#endif
				) 
				+ "</span></markup>";
			//this.soldlabel.LabelProp = ((Decimal)offer.taker_gets.value).ToString();

			this.pricelabel.Text = 
				offer?.quality ??
				#if DEBUG
				"null";
				#else
				"";
				#endif

			this.checkbutton1.Label = "     " + offer.Sequence.ToString();

			_Offer = offer;
		}

		public void CancelOffer () {



		}

		public Offer Order {
			get { 
				return _Offer; 
			}
			set { 
				_Offer = value;
				SetOrder (value);			
			}
		}

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private Offer _Offer = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

#if DEBUG
		private static string clsstr = nameof (OrderWidget) + DebugRippleLibSharp.colon;
		#endif

	}
}

