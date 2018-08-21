using System;
using Gtk;
using System.IO;
using System.Collections.Generic;



using Mono.CSharp;


//using CSScriptEvaluatorApi;
//using CSScriptEvaluatorApi;


using Codeplex.Data;


namespace IhildaWallet
{	/*
	public class PluginController
	{
		public PluginController ()
		{
			#if DEBUG
			if (Debug.PluginController) {
				Logging.writeLog("new PluginController()");
			}
			#endif

			currentInstance = this;

			//pluginList = new Dictionary<string, Plugin>();
			//encryptors = new Dictionary<string, IEncrypt>();

			loadPlugins();
			loadEncryptors();

		}

		public static PluginController currentInstance = null;
		private static readonly String configName = "plugins.jsn";
		private static Boolean use_filename = true;  // sets the default

		private static readonly string clsstr = nameof (PluginController) Debug.colon;

		public Dictionary<String, Ty> loadPluginType <Ty>( String folder, String[] extentions )
		{
			String method_sig = clsstr + "loadPlugins <" +  typeof(Ty).ToString()  + ">(" + (String)((folder == null) ? "null" : folder) + ")";

			#if DEBUG
			if (Debug.PluginController) {
				Logging.writeLog(method_sig + Debug.begin);
				Logging.writeLog(method_sig + "extentions = ", extentions);
			}
			#endif



			Dictionary<String, Ty> list = (typeof(Ty) == typeof(PluginController)) ? null : new Dictionary<String, Ty>();

			string[] names = FileHelper.getDirFileNames( folder ); // FileHelper.DATA_FOLDER_PATH

			foreach (String path in names) {
				#if DEBUG
				if (Debug.PluginController) {
					Logging.writeLog( method_sig + "dirname = " + Debug.toAssertString(path) + " ) : ");
				}
				#endif

				if (path == null) {
					#if DEBUG
					if (Debug.PluginController) {
						Logging.writeLog(method_sig + "path == null, returning");
					}
					#endif
					continue;
				}

				foreach (String ext in extentions) {
					#if DEBUG
					if (Debug.PluginController) {
						Logging.writeLog(method_sig + "testing " + ext);
					}
					#endif

					if (path.EndsWith(ext)) {


						try {
							//String path = name;//FileHelper.getPluginPath(name);

							String className = Path.GetFileNameWithoutExtension(path);
							if (className == null) {
								// Todo debug
								#if DEBUG
								if (Debug.PluginController) {
									Logging.writeLog(method_sig + "failed to get filename without extention ");
								}
								#endif
								continue;
							}




							Assembly a = CSScript.LoadCodeFrom(path); //

							if (typeof(Ty) == typeof(PluginController)) { // note type PluginController is a place holder for no type. i.e classes, methods, libraries ect. 
								#if DEBUG
								if (Debug.PluginController) {
									Logging.writeLog(method_sig + "library loaded, nothing else to do, continuing");
								}
								#endif
								continue;
							}

							if (a==null) {
								// Todo debuug
								continue;
							}

							AsmHelper scriptAsm = new AsmHelper(a);


							var pl = scriptAsm.TryCreateObject((String)(use_filename ? className : "*"));



	
							if (pl != null) {
								list.Add(className, (Ty)pl);
							
								//Plugin ipl = scriptAsm.TryCreateObject((String)(use_filename ? className : "*")) as Plugin; //scriptAsm.CreateObject(className) as Plugin;

								if (typeof(Ty) == typeof(Plugin)) {
									Plugin ipl = (Plugin)pl;


									if (ipl.jsonConf==null) {
										
										string n = className + ".jsn";
										#if DEBUG
										if (Debug.PluginController) {
											Logging.writeLog(method_sig + "ipl.jsonConf==null, setting to " + n);
										}
										#endif


										ipl.jsonConf = n;
									}

									if (ipl.title==null) {
										#if DEBUG
										if (Debug.PluginController) {
											Logging.writeLog(method_sig + "ipl.title == null, setting to " + className);
										}
										#endif
										ipl.title = className;
									}
	
									if (ipl.tab_label==null) {
										#if DEBUG
										if (Debug.PluginController) {
											Logging.writeLog(method_sig + "ipl.tab_label == null, setting to " + className);
										}
										#endif
										ipl.tab_label = className;
									}

									if (ipl.name == null) {
										#if DEBUG
										if (Debug.PluginController) {
											Logging.writeLog(method_sig + "ipl.name == null, setting to " + className);
										}
										#endif
										ipl.name = className;
									}

	
									continue;
								}

								else if (typeof(Ty) == typeof(IEncrypt)) {
									#if DEBUG
									if (Debug.PluginController) {
										Logging.writeLog(method_sig + "is of type IEncrypt, loading encryption module");
									}
									#endif
									IEncrypt ie = (IEncrypt)pl;

									ie.Name = className;

									encryptors.Add(ie.Name,ie);

									continue;
								}
							}
						}

						catch (Exception e) {
							Logging.writeLog("Exception while loading plugin " + e.Message);
						}
           
					}
				}

			}

			return list;

		}

		public void loadPlugins ()
		{

			pluginList = loadPluginType<Plugin>(FileHelper.PLUGIN_FOLDER_PATH, plug_ext);
		}

		public IEncrypt lookupEncryption (String s)
		{	
			IEncrypt lue = null;



			if (encryptors.TryGetValue (s, out lue)) {
				return lue;
			} else {

				if (Rijndaelio.isDescribedByString(s)) {
					lue = new Rijndaelio();
					encryptors.Add(s,lue);
					return lue;
				}
				return null;
			}
		}

		public void loadEncryptors ()
		{
			encryptors = loadPluginType<IEncrypt> (FileHelper.ENCRYPTION_FOLDER_PATH, crypt_ext);

			// don't forget to add the default encryptor
			Rijndaelio riji = new Rijndaelio ();

			if (Rijndaelio.default_name != null) {
				encryptors.Add(Rijndaelio.default_name,riji);
			} else {
				// todo debug. this value should never be null;
			}
		
		}

		public void loadClasses ()
		{

		}

		public void preStartUp ()
		{

			if (pluginList == null) {
				// Todo, either dbug or this may be acceptable (i.e the user disabled plugings. 
				return;
			}

			foreach (IPlugin plug in pluginList.Values) {
				if (plug!=null) {
					plug.preStartup();
				}
			}

		}

		public void postStartUp ()
		{
			if (pluginList == null) {
				// Todo, either dbug or this may be acceptable (i.e the user disabled plugings. 
				return;
			}

			foreach (IPlugin plug in pluginList.Values) {
				if (plug!=null) {
					plug.postStartup();
				}
			}
		}


		public List<Widget> getOptionsWidgets ()
		{
			if (pluginList == null) {
				// Todo, either dbug or this may be acceptable (i.e the user disabled plugings. 
				return null;
			}

			List<Widget> widgets = new List<Widget>();


			foreach (Plugin plug in pluginList.Values) {
				if (plug!=null) {
					Widget w = plug.getOptionTab();
					if (w!=null) {
						widgets.Add(w);
					}
				}
			}

			return widgets;
		}

		public List<Widget> getMainPluginDisplays(  )
		{
			if (pluginList == null) {
				// Todo, either dbug or this may be acceptable (i.e the user disabled plugings. 
				return null;
			}

			List<Widget> widgets = new List<Widget>();

			foreach (IPlugin plug in pluginList.Values) {
				if (plug!=null) {
					Widget w = plug.getMainTab();
				}
			}

			return widgets;

		}

		public static String[] plug_ext = new string[] {".plg", ".plug"};
		public static String[] crypt_ext = new string[] {".enc", ".cryp"};
		public static String[] class_ext = new string[] {".cs", ".css"};

		public static Dictionary<String, Plugin> pluginList = null;
		//public static List<IEncrypt> encryptors = null;

		public static Dictionary<String,IEncrypt> encryptors = null;

		public static bool allow_plugins = true;

		public static void setAllow (string param)
		{	try {
				allow_plugins = Convert.ToBoolean(param);
			}

			catch (Exception e) {
				Logging.writeLog("Invalid Paramater plugins=" + param + " value must be 'true' or 'false'");
				Application.Quit();
			}
		}
		public static void initPlugins ()
		{
			string method_sig = clsstr + "initPlugins () : ";

			String path = FileHelper.getSettingsPath (configName);

			if (path == null || !File.Exists (path)) {
				// it's a security risk to allow plugins automatically :/
				// even though it doesn't take much for an attacker
				allow_plugins = false;  

				return;
			}

			String json = FileHelper.getJsonConf(path);

			if (json == null) {
				allow_plugins = false; // 
				return;
			}

			dynamic d; 
			try {
				d = DynamicJson.Parse(json);
				if (d.IsDefined("allow_plugins")) {
					allow_plugins = d.allow_plugins;
				}

				else {
					allow_plugins = false;
				}

				if (d.IsDefined("use_filename")) {
					use_filename = d.use_filename;
				}

			}

			catch (Exception e) {
				// TODO invalid plugin config
				Logging.reportException(method_sig, e);
				allow_plugins = false;
			}

		
			if (allow_plugins) {
				new PluginController();
			}
		}

		public static void createExamplePlugin(String name)
		{
			//string u = @"using IhildaWallet;";

String s = 
@"
using System;
using Gtk;
using IhildaWallet;
using CSScriptLibrary;
public class " + name + @" : IhildaWallet.Plugin
{
		public " + name + @" ()
		{
			
		}

		// this is a good place to put early heavy loading while the splash screen is displayed. 
		// 
		public void preStartup () {}


		String getTitle ( return ""Example Plugin""; );
		// 
		public void postStartup() {}   // 

		public Gtk.Widget getOptionTab() { return null;}

		public Gtk.Widget getMainTab() { return null;}

}";


		
			String path = FileHelper.getSettingsPath( name + ".plg" );

			if (!File.Exists(path)) {
				File.WriteAllText(path,s);
			}
		}


	}

*/
}

