using System;
using System.IO;
using Codeplex.Data;
using Gtk;
using IhildaWallet.Splashes;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public partial class SplashWindow : Gtk.Window
	{
		public SplashWindow () :
				base (Gtk.WindowType.Toplevel)
		{

			if (path == null) {
				this.Build ();

				Gdk.Color col = new Gdk.Color (0, 0, 0);


				this.eventbox2.ModifyBg (StateType.Normal, col);
				this.eventbox3.ModifyBg (StateType.Normal, col);
				this.eventbox4.ModifyBg (StateType.Normal, col);

				image46.Animation = SpinWait.pa;
			} else { // override stetic gui designer

				//this.image87 = new Gtk.Image();

				//global::Stetic.Gui.Initialize (this);

				// Widget IhildaWallet.SplashWindow
				this.Name = nameof (IhildaWallet) + ".SplashWindow";
				this.Title = global::Mono.Unix.Catalog.GetString ("SplashWindow");
				this.TypeHint = ((global::Gdk.WindowTypeHint)(4));
				this.WindowPosition = ((global::Gtk.WindowPosition)(3));
				this.Resizable = false;
				this.AllowGrow = false;
				this.AcceptFocus = false;
				// Container child IhildaWallet.SplashWindow.Gtk.Container+ContainerChild
				this.vbox2 = new global::Gtk.VBox {
					Name = "vbox2",
					Spacing = 6
				};
				// Container child vbox2.Gtk.Box+BoxChild
				this.image87 = new global::Gtk.Image {
					Name = "image87"
				};

				if (pix != null) {
					this.image87.Pixbuf = pix;
				} else {
					this.image87.Pixbuf = global::Gdk.Pixbuf.LoadFromResource (nameof (IhildaWallet) + ".Images.xrp_crunched.png");
				}

				this.vbox2.Add (this.image87);
				global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.image87]));
				w1.Position = 0;
				this.Add (this.vbox2);
				if ((this.Child != null)) {
					this.Child.ShowAll ();
				}

				this.DefaultWidth = image87.Pixbuf.Width;
				this.DefaultHeight = image87.Pixbuf.Height;



				if (width.HasValue) {
					this.image87.WidthRequest = width.Value;
					this.WidthRequest = width.Value;
					this.DefaultWidth = width.Value;
				}

				if (height.HasValue) {
					image87.HeightRequest = height.Value;
					this.HeightRequest = height.Value;
					this.DefaultHeight = height.Value;
				}


			}

			this.Show ();


			loaded = true;
		}

		public static readonly String configName = "splash.jsn";


#pragma warning disable RECS0122 // Initializing field with default value is redundant

		public static Gdk.Pixbuf pix = null;


		public static bool isSplash = true;
		public static int? width = null;
		public static int? height = null;
		public static int? delay = null;
		public static string path = null;

		public static bool loaded = false;

		public static readonly int default_delay = 50;


		public static String jsonConfig = null;
		public static String configPath = null;

#pragma warning restore RECS0122 // Initializing field with default value is redundant



		public static void LoadSplash ()
		{

#if DEBUG
			String method_sig = clsstr + nameof (LoadSplash) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.SplashWindow) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif

			configPath = FileHelper.GetSettingsPath (configName);
			jsonConfig = FileHelper.GetJsonConf (configPath);

#if DEBUG
			if (DebugIhildaWallet.SplashWindow) {
				Logging.WriteLog (method_sig + "configPath = " + configPath ?? "null");
				Logging.WriteLog (method_sig + "jsonConfig = " + jsonConfig ?? "null");
			}
#endif

			if (jsonConfig != null) {
#if DEBUG
				if (DebugIhildaWallet.SplashWindow) {
					Logging.WriteLog (method_sig + "jsonConfig != null");
				}
#endif

				try {
					dynamic d = DynamicJson.Parse (jsonConfig);
					if (d.isDefined ("enable_splash")) {
						isSplash = d.enable_splash;
#if DEBUG
						if (DebugIhildaWallet.SplashWindow) {
							Logging.WriteLog (method_sig + "enable_splash defined, equals " + isSplash.ToString ());
						}
#endif
						// true or false decided by config file
					} else {
#if DEBUG
						if (DebugIhildaWallet.SplashWindow) {
							Logging.WriteLog (method_sig + "enable_splash not defined, setting to false");
						}
#endif

						isSplash = false; // ommitting "enable_splash"=true defaults to false
						return;
					}

					if (d.isDefined ("splash_path")) {
#if DEBUG
						if (DebugIhildaWallet.SplashWindow) {
							Logging.WriteLog (method_sig + "splash_path");
						}
#endif

						path = d.splash_path;


					} else {
						String splash = FileHelper.GetSettingsPath ("splash.png");

						if (File.Exists (splash)) {
							path = splash;
							isSplash = true;
						} else {
							path = null;
						}
						isSplash = false;
					}


					if (d.isDefined ("splash_width")) {
						width = d.splash_width as Int32?;
					} else {
						width = null;
					}

					if (d.isDefined ("splash_height")) {
						height = d.splash_height as Int32?;
					} else {
						height = null;
					}

					if (d.isDefined ("splash_delay")) {
						delay = d.splash_delay as Int32?;
					}

				} catch (Exception e) {

#if DEBUG
					Logging.ReportException (method_sig, e);
#endif
					isSplash = false;  // don't show invalid splash config
				}


			} else {
				isSplash = true; // show splash on lack of config
			}

			if (isSplash == true) {
				if (SplashWindow.path != null) {
					pix = new global::Gdk.Pixbuf (SplashWindow.path);
				} else {
					pix = global::Gdk.Pixbuf.LoadFromResource (nameof(IhildaWallet) + ".Images.ice_splash.png");
				}
			}

		}



#if DEBUG
		private const string clsstr = nameof (SplashWindow) + DebugRippleLibSharp.colon;
#endif

	}
}

