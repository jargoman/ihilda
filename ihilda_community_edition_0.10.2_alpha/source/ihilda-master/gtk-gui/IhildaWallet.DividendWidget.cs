
// This file has been generated by the GUI designer. Do not modify.
namespace IhildaWallet
{
	public partial class DividendWidget
	{
		private global::Gtk.VBox vbox2;

		private global::Gtk.Label label2;

		private global::Gtk.Table table1;

		private global::Gtk.ComboBoxEntry amountcomboboxentry;

		private global::Gtk.ComboBoxEntry divcurcomboboxentry;

		private global::Gtk.ComboBoxEntry divissuercomboboxentry;

		private global::Gtk.CheckButton excludecheckbox;

		private global::Gtk.FileChooserButton filechooserbutton1;

		private global::Gtk.FileChooserButton filechooserbutton2;

		private global::Gtk.CheckButton includecheckbox;

		private global::Gtk.Label label3;

		private global::Gtk.Label label4;

		private global::Gtk.Label label5;

		private global::Gtk.Label label6;

		private global::Gtk.Label label7;

		private global::Gtk.Label label8;

		private global::Gtk.CheckButton maxcheckbox;

		private global::Gtk.ComboBoxEntry maxcomboboxentry;

		private global::Gtk.CheckButton mincheckbox;

		private global::Gtk.ComboBoxEntry mincomboboxentry;

		private global::Gtk.ComboBoxEntry shareissuercomboboxentry;

		private global::Gtk.ComboBoxEntry sharetickercomboboxentry;

		private global::Gtk.Button button217;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget IhildaWallet.DividendWidget
			global::Stetic.BinContainer.Attach(this);
			this.Name = "IhildaWallet.DividendWidget";
			// Container child IhildaWallet.DividendWidget.Gtk.Container+ContainerChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("<span size=\"x-large\"><b>Dividend</b></span>");
			this.label2.UseMarkup = true;
			this.vbox2.Add(this.label2);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.label2]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table(((uint)(9)), ((uint)(3)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.amountcomboboxentry = global::Gtk.ComboBoxEntry.NewText();
			this.amountcomboboxentry.Name = "amountcomboboxentry";
			this.table1.Add(this.amountcomboboxentry);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1[this.amountcomboboxentry]));
			w2.TopAttach = ((uint)(6));
			w2.BottomAttach = ((uint)(7));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(3));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.divcurcomboboxentry = global::Gtk.ComboBoxEntry.NewText();
			this.divcurcomboboxentry.Name = "divcurcomboboxentry";
			this.table1.Add(this.divcurcomboboxentry);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1[this.divcurcomboboxentry]));
			w3.TopAttach = ((uint)(8));
			w3.BottomAttach = ((uint)(9));
			w3.LeftAttach = ((uint)(1));
			w3.RightAttach = ((uint)(3));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.divissuercomboboxentry = global::Gtk.ComboBoxEntry.NewText();
			this.divissuercomboboxentry.Name = "divissuercomboboxentry";
			this.table1.Add(this.divissuercomboboxentry);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1[this.divissuercomboboxentry]));
			w4.TopAttach = ((uint)(7));
			w4.BottomAttach = ((uint)(8));
			w4.LeftAttach = ((uint)(1));
			w4.RightAttach = ((uint)(3));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.excludecheckbox = new global::Gtk.CheckButton();
			this.excludecheckbox.CanFocus = true;
			this.excludecheckbox.Name = "excludecheckbox";
			this.excludecheckbox.Label = global::Mono.Unix.Catalog.GetString("Exclude Accounts");
			this.excludecheckbox.DrawIndicator = true;
			this.excludecheckbox.UseUnderline = true;
			this.table1.Add(this.excludecheckbox);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1[this.excludecheckbox]));
			w5.TopAttach = ((uint)(5));
			w5.BottomAttach = ((uint)(6));
			w5.LeftAttach = ((uint)(1));
			w5.RightAttach = ((uint)(2));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.filechooserbutton1 = new global::Gtk.FileChooserButton(global::Mono.Unix.Catalog.GetString("Select a File"), ((global::Gtk.FileChooserAction)(0)));
			this.filechooserbutton1.Sensitive = false;
			this.filechooserbutton1.Name = "filechooserbutton1";
			this.table1.Add(this.filechooserbutton1);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.filechooserbutton1]));
			w6.TopAttach = ((uint)(4));
			w6.BottomAttach = ((uint)(5));
			w6.LeftAttach = ((uint)(2));
			w6.RightAttach = ((uint)(3));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.filechooserbutton2 = new global::Gtk.FileChooserButton(global::Mono.Unix.Catalog.GetString("Select a File"), ((global::Gtk.FileChooserAction)(0)));
			this.filechooserbutton2.Sensitive = false;
			this.filechooserbutton2.Name = "filechooserbutton2";
			this.table1.Add(this.filechooserbutton2);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1[this.filechooserbutton2]));
			w7.TopAttach = ((uint)(5));
			w7.BottomAttach = ((uint)(6));
			w7.LeftAttach = ((uint)(2));
			w7.RightAttach = ((uint)(3));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.includecheckbox = new global::Gtk.CheckButton();
			this.includecheckbox.CanFocus = true;
			this.includecheckbox.Name = "includecheckbox";
			this.includecheckbox.Label = global::Mono.Unix.Catalog.GetString("Include Accounts");
			this.includecheckbox.DrawIndicator = true;
			this.includecheckbox.UseUnderline = true;
			this.table1.Add(this.includecheckbox);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table1[this.includecheckbox]));
			w8.TopAttach = ((uint)(4));
			w8.BottomAttach = ((uint)(5));
			w8.LeftAttach = ((uint)(1));
			w8.RightAttach = ((uint)(2));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label3 = new global::Gtk.Label();
			this.label3.Name = "label3";
			this.label3.Xalign = 0F;
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Share Ticker</b>");
			this.label3.UseMarkup = true;
			this.table1.Add(this.label3);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table1[this.label3]));
			w9.TopAttach = ((uint)(1));
			w9.BottomAttach = ((uint)(2));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label4 = new global::Gtk.Label();
			this.label4.Name = "label4";
			this.label4.Xalign = 0F;
			this.label4.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Share Issuer</b>");
			this.label4.UseMarkup = true;
			this.table1.Add(this.label4);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1[this.label4]));
			w10.XOptions = ((global::Gtk.AttachOptions)(6));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label5 = new global::Gtk.Label();
			this.label5.Name = "label5";
			this.label5.Xalign = 0F;
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString("Participating Accounts");
			this.table1.Add(this.label5);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1[this.label5]));
			w11.TopAttach = ((uint)(4));
			w11.BottomAttach = ((uint)(6));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label6 = new global::Gtk.Label();
			this.label6.Name = "label6";
			this.label6.Xalign = 0F;
			this.label6.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Total Amount</b>");
			this.label6.UseMarkup = true;
			this.table1.Add(this.label6);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table1[this.label6]));
			w12.TopAttach = ((uint)(6));
			w12.BottomAttach = ((uint)(7));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label7 = new global::Gtk.Label();
			this.label7.Name = "label7";
			this.label7.Xalign = 0F;
			this.label7.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Dividend Currency</b>");
			this.label7.UseMarkup = true;
			this.table1.Add(this.label7);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table1[this.label7]));
			w13.TopAttach = ((uint)(8));
			w13.BottomAttach = ((uint)(9));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label8 = new global::Gtk.Label();
			this.label8.Name = "label8";
			this.label8.Xalign = 0F;
			this.label8.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Dividend Issuer</b>");
			this.label8.UseMarkup = true;
			this.table1.Add(this.label8);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table1[this.label8]));
			w14.TopAttach = ((uint)(7));
			w14.BottomAttach = ((uint)(8));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.maxcheckbox = new global::Gtk.CheckButton();
			this.maxcheckbox.CanFocus = true;
			this.maxcheckbox.Name = "maxcheckbox";
			this.maxcheckbox.Label = global::Mono.Unix.Catalog.GetString("Maximum Amount");
			this.maxcheckbox.DrawIndicator = true;
			this.maxcheckbox.UseUnderline = true;
			this.table1.Add(this.maxcheckbox);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.table1[this.maxcheckbox]));
			w15.TopAttach = ((uint)(3));
			w15.BottomAttach = ((uint)(4));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.maxcomboboxentry = global::Gtk.ComboBoxEntry.NewText();
			this.maxcomboboxentry.Sensitive = false;
			this.maxcomboboxentry.Name = "maxcomboboxentry";
			this.table1.Add(this.maxcomboboxentry);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.table1[this.maxcomboboxentry]));
			w16.TopAttach = ((uint)(3));
			w16.BottomAttach = ((uint)(4));
			w16.LeftAttach = ((uint)(1));
			w16.RightAttach = ((uint)(3));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.mincheckbox = new global::Gtk.CheckButton();
			this.mincheckbox.CanFocus = true;
			this.mincheckbox.Name = "mincheckbox";
			this.mincheckbox.Label = global::Mono.Unix.Catalog.GetString("Minimum Amount");
			this.mincheckbox.DrawIndicator = true;
			this.mincheckbox.UseUnderline = true;
			this.table1.Add(this.mincheckbox);
			global::Gtk.Table.TableChild w17 = ((global::Gtk.Table.TableChild)(this.table1[this.mincheckbox]));
			w17.TopAttach = ((uint)(2));
			w17.BottomAttach = ((uint)(3));
			w17.XOptions = ((global::Gtk.AttachOptions)(4));
			w17.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.mincomboboxentry = global::Gtk.ComboBoxEntry.NewText();
			this.mincomboboxentry.Sensitive = false;
			this.mincomboboxentry.Name = "mincomboboxentry";
			this.table1.Add(this.mincomboboxentry);
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.table1[this.mincomboboxentry]));
			w18.TopAttach = ((uint)(2));
			w18.BottomAttach = ((uint)(3));
			w18.LeftAttach = ((uint)(1));
			w18.RightAttach = ((uint)(3));
			w18.XOptions = ((global::Gtk.AttachOptions)(4));
			w18.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.shareissuercomboboxentry = global::Gtk.ComboBoxEntry.NewText();
			this.shareissuercomboboxentry.Name = "shareissuercomboboxentry";
			this.table1.Add(this.shareissuercomboboxentry);
			global::Gtk.Table.TableChild w19 = ((global::Gtk.Table.TableChild)(this.table1[this.shareissuercomboboxentry]));
			w19.LeftAttach = ((uint)(1));
			w19.RightAttach = ((uint)(3));
			w19.YOptions = ((global::Gtk.AttachOptions)(6));
			// Container child table1.Gtk.Table+TableChild
			this.sharetickercomboboxentry = global::Gtk.ComboBoxEntry.NewText();
			this.sharetickercomboboxentry.Name = "sharetickercomboboxentry";
			this.table1.Add(this.sharetickercomboboxentry);
			global::Gtk.Table.TableChild w20 = ((global::Gtk.Table.TableChild)(this.table1[this.sharetickercomboboxentry]));
			w20.TopAttach = ((uint)(1));
			w20.BottomAttach = ((uint)(2));
			w20.LeftAttach = ((uint)(1));
			w20.RightAttach = ((uint)(3));
			w20.XOptions = ((global::Gtk.AttachOptions)(4));
			w20.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox2.Add(this.table1);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.table1]));
			w21.Position = 1;
			// Container child vbox2.Gtk.Box+BoxChild
			this.button217 = new global::Gtk.Button();
			this.button217.CanFocus = true;
			this.button217.Name = "button217";
			this.button217.UseUnderline = true;
			this.button217.Label = global::Mono.Unix.Catalog.GetString("Proceed to Payment Manager  ( Send Dividends )");
			this.vbox2.Add(this.button217);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.button217]));
			w22.Position = 2;
			w22.Expand = false;
			w22.Fill = false;
			this.Add(this.vbox2);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}