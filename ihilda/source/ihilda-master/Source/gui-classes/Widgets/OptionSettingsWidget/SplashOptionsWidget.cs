using System;
using System.IO;
using System.Threading.Tasks;
using Codeplex.Data;
using Gtk;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SplashOptionsWidget : Gtk.Bin
	{
		public SplashOptionsWidget ()
		{
			this.Build ();



			pathbutton.Clicked += (sender, e) => {



				FileChooserDialog fileChooser = new FileChooserDialog (
						"Choose image",
						null, //this.ParentWindow,
						FileChooserAction.Open,
						"Cancel", ResponseType.Cancel,
						"Select Image", ResponseType.Accept
					);

				FileFilter fileFilter = new FileFilter ();
				fileFilter.AddPattern ("*.jpg");
				fileFilter.AddPattern ("*.png");
				fileFilter.AddPattern ("*.gif");
				fileChooser.AddFilter (fileFilter);

				Gtk.ResponseType resp = (ResponseType)fileChooser.Run ();

				if (resp == ResponseType.Accept) {
					pathlabel.Text = fileChooser.Filename;

				}

				fileChooser?.Destroy ();
			};

			Task.Run ( (System.Action)InitUI );

		}

		public void InitUI ()
		{
			SplashOptions opts = LoadSettings ();

			Gtk.Application.Invoke ( delegate {

				showsplashcheckbutton.Active = opts.Showsplash;

				splashdelayentry.Text = opts.Splash_delay.ToString();

				splashwidthentry.Text = opts.Splash_width.ToString ();

				splashheightentry.Text = opts.Splash_height.ToString ();

				pathlabel.Text = opts.Splash_path;

			});
		}



		public void ProcessSplashSettings ()
		{
			#if DEBUG
			String method_sig = clsstr + nameof (ProcessSplashSettings) + DebugRippleLibSharp.both_parentheses;
			if ( DebugIhildaWallet.SplashOptionsWidget ) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
			#endif
			//Gtk.Application.Invoke( delegate {

				#if DEBUG
				if (DebugIhildaWallet.SplashOptionsWidget) {
					Logging.WriteLog(method_sig + DebugIhildaWallet.gtkInvoke + DebugRippleLibSharp.beginn);
				}
				#endif

				try {
					// following must be run by gtk thread
					SplashOptions opts = new SplashOptions {
						Showsplash = this.showsplashcheckbutton.Active
					};
#if DEBUG
					if (DebugIhildaWallet.SplashOptionsWidget) {
						Logging.WriteLog(method_sig + "showsplash = " + opts.Showsplash.ToString());
					}
					#endif

					if (opts.Showsplash == false) {
						return;
					}
				

					String millstr = this.splashdelayentry.Text;
					if (millstr!=null) {
						millstr = millstr.Trim();
					}

					#if DEBUG
					if (DebugIhildaWallet.SplashOptionsWidget) {
						Logging.WriteLog(method_sig + "millstr = " + DebugIhildaWallet.ToAssertString(millstr));
					}
					#endif
					
					

					string path = pathlabel.Text;
					if (path != null) {
						path = path.Trim();
					}

					#if DEBUG
					if (DebugIhildaWallet.SplashOptionsWidget) {
						Logging.WriteLog(method_sig + "path = " + DebugIhildaWallet.ToAssertString(path));
					}
					#endif
					
					if (IsEdited(path)) {
						if (File.Exists(path)) {
							// todo try catch
							Gtk.Image image = null;
							try {
								image = new Gtk.Image(path);
								image.Show();
							}

							catch (Exception e) {
								MessageDialog.ShowMessage("Path is not a valid image file" + e);
								return;
							}
							opts.Splash_path = path;
						}
						else {
							Warn ("splash_path");
							return;
						}
					}

					//Int32? splshwidth = null;
					//Int32? splshheight = null;
					
					String wd = splashwidthentry.Text;
					if (wd!=null) {
						wd = wd.Trim();
					}


					String ht = splashheightentry.Text;
					if (ht!=null) {
						ht = ht.Trim();
					}


					opts.Splash_delay = ParseInt(millstr);
					if (opts.Splash_delay == null) {
						Warn("splash_delay");
					return;
					}

					opts.Splash_width = ParseInt(wd);
					if (opts.Splash_width == null)
					{
						Warn("splash_width");
					return;
					}

					opts.Splash_height = ParseInt(ht);
					if (opts.Splash_height == null)
					{
						Warn("splash_height");
					return;
					}


					Task.Run (delegate {
						SaveSettings (opts);
					});

				}

				#pragma warning disable 0168
				catch (Exception e) {
				#pragma warning restore 0168

					// TODO debug
					#if DEBUG
					if (DebugIhildaWallet.SplashOptionsWidget) {
						Logging.WriteLog(method_sig + "exception thrown : " + e.Message);
					}
					#endif
				}




			//	}

					
			//);
		}


		public static SplashOptions LoadSettings ()
		{
			string path = FileHelper.GetSettingsPath (SplashWindow.configName);
			string str = FileHelper.GetJsonConf (path);
			if (str == null) {
				return null;
			}
			SplashOptions opts = null;
			try {
				opts = DynamicJson.Parse (str);

			} catch (Exception e) {
				Logging.WriteLog (e.Message + e.StackTrace);
				return null;
			}

			return opts;


		}


		private void SaveSettings ( SplashOptions opts ) {
			
			String json = DynamicJson.Serialize(opts);

			#if DEBUG
			string method_sig = clsstr + "saveSettings : ";
			if (DebugIhildaWallet.SplashOptionsWidget) {
				Logging.WriteLog(method_sig + "json = " + DebugIhildaWallet.ToAssertString( json));
			}
			#endif

			if (json == null) {
				#if DEBUG
				if (DebugIhildaWallet.SplashOptionsWidget) {
					Logging.WriteLog(method_sig + "json == null, returning");
				}
				#endif
				return;
			}

			String path = FileHelper.GetSettingsPath( SplashWindow.configName );
			if (path == null) {
				#if DEBUG
				if (DebugIhildaWallet.SplashOptionsWidget) {
					Logging.WriteLog(method_sig + "splashConfig == null, returning");
				}
				#endif
				return;
			}

			bool success = FileHelper.SaveConfig(path, json);

			String m = (success ? "Success" : "Error") + " saving splash config file"; 
			#if DEBUG
			if (DebugIhildaWallet.SplashOptionsWidget) {
				Logging.WriteLog(method_sig + m);
			}
			#endif

			if (!success) {
				MessageDialog.ShowMessage(m);
			}
		}

		private static bool IsEdited (String s)
		{
			if (s == null) {
				return false;
			}

			s = s.Trim();

			return !s.Equals ("");

		}

		public static int? ParseInt (String s) // dynamic dyna
		{
			#if DEBUG
			String method_sig = clsstr + "parseInt ( String s = " + DebugIhildaWallet.ToAssertString(s) + " ) : ";
			#endif


			if (!IsEdited (s)) {
				#if DEBUG
				if (DebugIhildaWallet.SplashOptionsWidget) {
					Logging.WriteLog(method_sig + "not edited, returning false");
				}
				#endif
				return null;
			}

			Int32? num = null;

			try {
				if (s != null) {
					num = Convert.ToInt32 (s);
				} 


				#pragma warning disable 0168
			} catch (Exception e) {
				#pragma warning restore 0168
				// TODO debug

				#if DEBUG
				if (DebugIhildaWallet.SplashOptionsWidget) {
					
					Logging.ReportException(method_sig, e);
				}
				#endif

				return null;
			}

			return num;

		}

		private void Warn (String message) {
			Gtk.Application.Invoke(
				delegate {
				


					MessageDialog.ShowMessage (message + " is configured incorrectly");

				}
			);
		}

		#if DEBUG
		private static readonly string clsstr = nameof (SplashOptionsWidget) + DebugRippleLibSharp.colon;
		#endif
	}



	public class SplashOptions {

		public bool Showsplash {
			get;
			set;
		}

		public string Splash_path {
			get;
			set;
		}

		public int? Splash_delay {
			get;
			set;
		}

		public int? Splash_width {
			get;
			set;
		}

		public int? Splash_height {
			get;
			set;
		}



	}
}

