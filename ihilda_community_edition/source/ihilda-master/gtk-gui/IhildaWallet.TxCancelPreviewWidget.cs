
// This file has been generated by the GUI designer. Do not modify.
namespace IhildaWallet
{
	public partial class TxCancelPreviewWidget
	{
		private global::Gtk.VBox vbox2;

		private global::IhildaWallet.OpenOrdersTree openorderstree1;

		private global::Gtk.HBox hbox1;

		private global::Gtk.Button button1;

		private global::Gtk.Button button2;

		private global::Gtk.Button button75;

		private global::Gtk.Button button76;

		private global::Gtk.Button button77;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget IhildaWallet.TxCancelPreviewWidget
			global::Stetic.BinContainer.Attach(this);
			this.Name = "IhildaWallet.TxCancelPreviewWidget";
			// Container child IhildaWallet.TxCancelPreviewWidget.Gtk.Container+ContainerChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.openorderstree1 = new global::IhildaWallet.OpenOrdersTree();
			this.openorderstree1.Events = ((global::Gdk.EventMask)(256));
			this.openorderstree1.Name = "openorderstree1";
			this.vbox2.Add(this.openorderstree1);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.openorderstree1]));
			w1.Position = 0;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.button1 = new global::Gtk.Button();
			this.button1.CanFocus = true;
			this.button1.Name = "button1";
			this.button1.UseUnderline = true;
			this.button1.Label = global::Mono.Unix.Catalog.GetString("Submit");
			this.hbox1.Add(this.button1);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.button1]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.button2 = new global::Gtk.Button();
			this.button2.CanFocus = true;
			this.button2.Name = "button2";
			this.button2.UseUnderline = true;
			this.button2.Label = global::Mono.Unix.Catalog.GetString("Stop");
			this.hbox1.Add(this.button2);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.button2]));
			w3.Position = 1;
			w3.Expand = false;
			w3.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.button75 = new global::Gtk.Button();
			this.button75.CanFocus = true;
			this.button75.Name = "button75";
			this.button75.UseUnderline = true;
			this.button75.Label = global::Mono.Unix.Catalog.GetString("Remove");
			this.hbox1.Add(this.button75);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.button75]));
			w4.Position = 2;
			w4.Expand = false;
			w4.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.button76 = new global::Gtk.Button();
			this.button76.CanFocus = true;
			this.button76.Name = "button76";
			this.button76.UseUnderline = true;
			this.button76.Label = global::Mono.Unix.Catalog.GetString("Select All or None");
			this.hbox1.Add(this.button76);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.button76]));
			w5.Position = 3;
			w5.Expand = false;
			w5.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.button77 = new global::Gtk.Button();
			this.button77.CanFocus = true;
			this.button77.Name = "button77";
			this.button77.UseUnderline = true;
			this.button77.Label = global::Mono.Unix.Catalog.GetString("Reset To Default");
			this.hbox1.Add(this.button77);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.button77]));
			w6.Position = 4;
			w6.Expand = false;
			w6.Fill = false;
			this.vbox2.Add(this.hbox1);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox1]));
			w7.Position = 1;
			w7.Expand = false;
			w7.Fill = false;
			this.Add(this.vbox2);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
