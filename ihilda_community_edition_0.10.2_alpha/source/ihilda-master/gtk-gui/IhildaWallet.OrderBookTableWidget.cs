
// This file has been generated by the GUI designer. Do not modify.
namespace IhildaWallet
{
	public partial class OrderBookTableWidget
	{
		private global::Gtk.ScrolledWindow scrolledwindow1;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget IhildaWallet.OrderBookTableWidget
			global::Stetic.BinContainer.Attach(this);
			this.Name = "IhildaWallet.OrderBookTableWidget";
			// Container child IhildaWallet.OrderBookTableWidget.Gtk.Container+ContainerChild
			this.scrolledwindow1 = new global::Gtk.ScrolledWindow();
			this.scrolledwindow1.CanFocus = true;
			this.scrolledwindow1.Name = "scrolledwindow1";
			this.scrolledwindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			this.Add(this.scrolledwindow1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}