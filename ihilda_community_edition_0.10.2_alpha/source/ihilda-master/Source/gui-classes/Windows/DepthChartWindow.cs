using System;

namespace IhildaWallet
{
	public partial class DepthChartWindow : Gtk.Window
	{
		public DepthChartWindow (RippleWallet rippleWallet, TradePair tradePair) :
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();

			if (this.depthchartwidget1 == null) {
				
				this.depthchartwidget1 = new DepthChartWidget ();
				this.depthchartwidget1.Show ();
				vbox2.Add(depthchartwidget1);
			}

			this.depthchartwidget1.SetRippleWallet (rippleWallet);
			this.depthchartwidget1.SetTradePair (tradePair);
		}

		public DepthChartWidget GetWidget() {
			return this.depthchartwidget1;
		}
	}
}

