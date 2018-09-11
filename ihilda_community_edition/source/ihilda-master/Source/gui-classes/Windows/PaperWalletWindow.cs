using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Gdk;
using QRCoder;
using RippleLibSharp.Keys;
using RippleLibSharp.Util;

using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using Gtk;

namespace IhildaWallet
{
	public partial class PaperWalletWindow : Gtk.Window
	{
		public PaperWalletWindow () :
				base (Gtk.WindowType.Toplevel)
		{
			this.Build ();

			this.checkbutton1.Clicked += (object sender, EventArgs e) => {
				secretlabel.Visible = checkbutton1.Active;
				image6.Visible = checkbutton1.Active;
			};

			this.button138.Clicked += Button138_Clicked;

			//eventbox1.ModifyBg (Gtk.StateType.Normal, new Gdk.Color (255, 255, 255);
		}

		void Button138_Clicked (object sender, EventArgs e)
		{

			PdfDocument document = new PdfDocument ();
			document.Info.Title = "Printable Document";
			document.Info.Author = Program.verboseName;

			PdfPage pdfPage = document.AddPage ();

			XGraphics gfx = XGraphics.FromPdfPage (pdfPage);

			//const string facename = "Times New Roman";


			string acc = addresslabel.Text;
			string sec = secretlabel.Text;
			string notes = textview3.Buffer.Text;
			Bitmap accTextBitMap = new Bitmap (300, 20);
			Bitmap secTextBitMap = new Bitmap (300, 20);

			Bitmap noteBitMap = new Bitmap (600, 600);

			Graphics accGraphic = Graphics.FromImage (accTextBitMap);
			Graphics secGraphic = Graphics.FromImage (secTextBitMap);
			Graphics noteGraphics = Graphics.FromImage (noteBitMap);


			FontFamily fontFamily = new FontFamily ("Arial");
			System.Drawing.Font font = new System.Drawing.Font (
			   fontFamily,
			   10,
			   FontStyle.Regular,
			   GraphicsUnit.Point);
			RectangleF rectF = new RectangleF (0, 0, 300, 20);
			SolidBrush solidBrush = new SolidBrush (System.Drawing.Color.Black);


			accGraphic.DrawString (acc, font, solidBrush, rectF);


			secGraphic.DrawString (sec, font, solidBrush, rectF);


			rectF = new RectangleF (0,0,600, 600);
			noteGraphics.DrawString (notes, font, solidBrush, rectF);

			XImage xImageAcc = XImage.FromGdiPlusImage (accBitmap);
			XImage xImageSec = XImage.FromGdiPlusImage (secBitmap);

			//gfx.DrawString (acc, xFont, XBrushes.Black, 0, 0);

			gfx.DrawImage (accTextBitMap, 25, 15, 275, 20);

			gfx.DrawImage (xImageAcc, 5, 30, 275, 275);

			//gfx.DrawString (sec, xFont, XBrushes.Black, 0, 200);
			gfx.DrawImage (secTextBitMap, 300, 15, 275, 20);

			gfx.DrawImage (xImageSec, 280, 30, 275, 275);

			gfx.DrawImage (noteBitMap, 25, 310, 600, 600);


			FileChooserDialog fileChooserDialog = new FileChooserDialog (
				"Save PDF", 
				this, 
				FileChooserAction.Save, 
				"Cancel", 
				ResponseType.Cancel,
				"Save", ResponseType.Accept);

			if (fileChooserDialog.Run () == (int)ResponseType.Accept) {
				document.Save (fileChooserDialog.Filename + ".pdf");
			}

			fileChooserDialog.Destroy ();
			//document.Save ("/home/karim/pdftextihilda.pdf");


		}



		public void SetSecret ( string secret )
		{

			RippleSeedAddress seedAddress = null;

			seedAddress = new RippleSeedAddress (secret);


			string add = seedAddress.GetPublicRippleAddress ().ToString ();
			string sec = seedAddress.GetHumanReadableIdentifier ();


			QRCodeGenerator qRCodeGenerator = new QRCodeGenerator ();

			QRCodeData addressqrCodeData = qRCodeGenerator.CreateQrCode (add, QRCodeGenerator.ECCLevel.Q);
			QRCodeData secretqrCodeData = qRCodeGenerator.CreateQrCode (sec, QRCodeGenerator.ECCLevel.Q);

			//string html = 
			//	"<html><h1>" +
			//	sec +
			//	"</h1>"




			TextHighlighter.Highlightcolor = TextHighlighter.GREEN;
			add = TextHighlighter.Highlight ("<big>" + add + "</big>");

			TextHighlighter.Highlightcolor = TextHighlighter.RED;
			sec = TextHighlighter.Highlight ("<big>" + sec + "</big>");
			this.addresslabel.Markup = add;
			this.secretlabel.Markup = sec;

			QRCode qrCodeAdd = new QRCode (addressqrCodeData);
			QRCode qRCodeSec = new QRCode (secretqrCodeData);

			Bitmap qrCodeImageAdd = qrCodeAdd.GetGraphic (8, System.Drawing.Color.Black, System.Drawing.Color.White, true);
			Bitmap qrCodeImageSec = qRCodeSec.GetGraphic (8, System.Drawing.Color.Black, System.Drawing.Color.White, true);


			secBitmap = qrCodeImageAdd;
			accBitmap = qrCodeImageSec;

			MemoryStream ms = new MemoryStream ();
			qrCodeImageAdd.Save (ms, ImageFormat.Png);
			ms.Position = 0;
       			Gdk.Pixbuf pb= new Gdk.Pixbuf (ms);

			this.image5.Pixbuf = pb;

			MemoryStream ms2 = new MemoryStream ();
			qrCodeImageSec.Save (ms2, ImageFormat.Png);
			ms2.Position = 0;
			pb = new Pixbuf (ms2);

			this.image6.Pixbuf = pb;


		}

		private Bitmap accBitmap = null;
		private Bitmap secBitmap = null;

	}
}
