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

			addresslabel.Selectable = true;
			secretlabel.Selectable = true;
		}

		void Button138_Clicked (object sender, EventArgs e)
		{

			PdfDocument pdfDocument = new PdfDocument ();

			pdfDocument.Info.Title = "Printable Document";
			pdfDocument.Info.Author = ProgramVariables.verboseName;

			PdfPage pdfPage = pdfDocument.AddPage ();

			XGraphics gfx = XGraphics.FromPdfPage (pdfPage);

			//const string facename = "Times New Roman";


			// get the values from UI
			string acc = addresslabel.Text;
			string sec = secretlabel.Text;
			string notes = textview3.Buffer.Text;


			Bitmap accTextBitMap = new Bitmap (300, 20);
			Bitmap secTextBitMap = new Bitmap (this.IsPrivateKey ? 500 : 300, 20);

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


			FontFamily fontFamilyBig = new FontFamily ("Arial");
			System.Drawing.Font fontBig = new System.Drawing.Font (
			   fontFamilyBig,
			   14,
			   FontStyle.Bold,
			   GraphicsUnit.Point);
			RectangleF rectFBig = new RectangleF (0, 0, 302, 20);
			SolidBrush solidBrushBig = new SolidBrush (System.Drawing.Color.Black);


			/*
			System.Drawing.Font fontPk = new System.Drawing.Font (
			   fontFamilyBig,
			   14,
			   FontStyle.Bold,
			   GraphicsUnit.Point); */

			RectangleF rectFBigPk = new RectangleF (0, 0, 502, 20);
			//SolidBrush solidBrushBigPk = new SolidBrush (System.Drawing.Color.Black);


			accGraphic.DrawString (acc, font, solidBrush, rectF);


			secGraphic.DrawString (sec, font, solidBrush, this.IsPrivateKey ? rectFBigPk : rectF);


			rectF = new RectangleF (0,0,600, 600);
			noteGraphics.DrawString (notes, font, solidBrushBig, rectF);

			

			XImage xImageAcc = XImage.FromGdiPlusImage (accBitmap);
			XImage xImageSec = XImage.FromGdiPlusImage (secBitmap);

			//gfx.DrawString (acc, xFont, XBrushes.Black, 0, 0);

			gfx.DrawImage (accTextBitMap, 27, 65, 275, 20);

			gfx.DrawImage (xImageAcc, 5, 100, 275, 275);

			//gfx.DrawString (sec, xFont, XBrushes.Black, 0, 200);

			if (IsPrivateKey) {

				int y_increase = 425;

				gfx.DrawImage (secTextBitMap, 27, 65 + y_increase, 375, 20);

				gfx.DrawImage (xImageSec, 5, 100 + y_increase, 275, 275);

				gfx.DrawImage (noteBitMap, 27, 380, 600 + y_increase, 600);

			} else {

				gfx.DrawImage (secTextBitMap, 302, 65, 275, 20);

				gfx.DrawImage (xImageSec, 280, 100, 275, 275);

				gfx.DrawImage (noteBitMap, 27, 380, 600, 600);

			}

			FileChooserDialog fileChooserDialog = new FileChooserDialog (
				"Save PDF", 
				this, 
				FileChooserAction.Save, 
				"Cancel", 
				ResponseType.Cancel,
				"Save", ResponseType.Accept);

			if (fileChooserDialog.Run () == (int)ResponseType.Accept) {
				pdfDocument.Save (fileChooserDialog.Filename + ".pdf");
			}

			fileChooserDialog.Destroy ();
			//document.Save ("/home/karim/pdftextihilda.pdf");


		}




		public void SetSecret ( string secret )
		{

			IsPrivateKey = false;

			TextHighlighter highlighter = new TextHighlighter ();

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




			highlighter.Highlightcolor = ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN;
			add = highlighter.Highlight ("<big>" + add + "</big>");

			highlighter.Highlightcolor = TextHighlighter.RED;
			sec = highlighter.Highlight ("<big>" + sec + "</big>");
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

		public void SetPrivateKey (string secret)
		{

			IsPrivateKey = true;

			TextHighlighter highlighter = new TextHighlighter ();

			RipplePrivateKey privateKey = null;

			privateKey = new RipplePrivateKey (secret);


			string add = privateKey.GetPublicKey().GetAddress().ToString ();
			string sec = privateKey.GetHumanReadableIdentifier ();


			QRCodeGenerator qRCodeGenerator = new QRCodeGenerator ();

			QRCodeData addressqrCodeData = qRCodeGenerator.CreateQrCode (add, QRCodeGenerator.ECCLevel.Q);
			QRCodeData secretqrCodeData = qRCodeGenerator.CreateQrCode (sec, QRCodeGenerator.ECCLevel.Q);

			//string html = 
			//	"<html><h1>" +
			//	sec +
			//	"</h1>"




			highlighter.Highlightcolor = ProgramVariables.darkmode ? TextHighlighter.CHARTREUSE : TextHighlighter.GREEN;
			add = highlighter.Highlight ("<big>" + add + "</big>");

			highlighter.Highlightcolor = TextHighlighter.RED;
			sec = highlighter.Highlight ("<big>" + sec + "</big>");
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
			Gdk.Pixbuf pb = new Gdk.Pixbuf (ms);

			this.image5.Pixbuf = pb;

			MemoryStream ms2 = new MemoryStream ();
			qrCodeImageSec.Save (ms2, ImageFormat.Png);
			ms2.Position = 0;
			pb = new Pixbuf (ms2);

			this.image6.Pixbuf = pb;


		}

		private bool IsPrivateKey { get; set; }

		private Bitmap accBitmap = null;
		private Bitmap secBitmap = null;

	}
}
