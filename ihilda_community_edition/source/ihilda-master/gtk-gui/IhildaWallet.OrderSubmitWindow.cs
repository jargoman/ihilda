
// This file has been generated by the GUI designer. Do not modify.
namespace IhildaWallet
{
	public partial class OrderSubmitWindow
	{
		private global::IhildaWallet.OrderPreviewSubmitWidget orderpreviewsubmitwidget1;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget IhildaWallet.OrderSubmitWindow
			this.Name = "IhildaWallet.OrderSubmitWindow";
			this.Title = global::Mono.Unix.Catalog.GetString("OrderSubmitWindow");
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			// Container child IhildaWallet.OrderSubmitWindow.Gtk.Container+ContainerChild
			this.orderpreviewsubmitwidget1 = new global::IhildaWallet.OrderPreviewSubmitWidget();
			this.orderpreviewsubmitwidget1.Events = ((global::Gdk.EventMask)(256));
			this.orderpreviewsubmitwidget1.Name = "orderpreviewsubmitwidget1";
			this.orderpreviewsubmitwidget1.AllSubmitted = false;
			this.Add(this.orderpreviewsubmitwidget1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.DefaultWidth = 1051;
			this.DefaultHeight = 386;
			this.Show();
		}
	}
}
