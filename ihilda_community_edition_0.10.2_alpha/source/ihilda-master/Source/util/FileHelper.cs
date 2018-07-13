using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public static class FileHelper
	{
		public static void SetFolderPath (String pat)
		{

#if DEBUG
			string method_sig = clsstr + nameof (SetFolderPath) + DebugRippleLibSharp.colon;
			//String method_sig = clsstr + ("setFolderPath ( pat = " + ((string)Debug.toAssertString((string)pat)) + ") : ");
			//String method_sig = clsstr + "setFolderPath ( pat = " + (string)Debug.toAssertString((pat==null ? "null":pat)) + ") : ";
			if (DebugIhildaWallet.FileHelper) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}

#endif


			if (pat != null) {
				if (Directory.Exists (pat)) {

#if DEBUG
					if (DebugIhildaWallet.FileHelper) {
						Logging.WriteLog("Setting configuration directory to " + pat);
					}
#endif

					DATA_FOLDER_PATH = pat;
				} else {
					// Todo debug 

					Logging.WriteLog ("Configuration directory " + pat + " does not exist!");
					Gtk.Application.Quit ();
					return;
				}
			} else {
				/*
				#if DEBUG
				if (Debug.FileHelper) {
					Logging.writeLog(method_sig);
				}
				#endif
				*/
				DATA_FOLDER_PATH = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
			}

			if (DATA_FOLDER_PATH == null) {
				// Todo debug
				Gtk.Application.Quit ();
			}

#if DEBUG
			if (DebugIhildaWallet.FileHelper) {
				Logging.WriteLog("DATA_FOLDER_PATH = " + DATA_FOLDER_PATH);
			}
#endif
			AssureDirectory (DATA_FOLDER_PATH);

			DATA_FOLDER = System.IO.Path.Combine (DATA_FOLDER_PATH, DATA_FOLDER);
			AssureDirectory (DATA_FOLDER);


			PLUGIN_FOLDER_PATH = Path.Combine (DATA_FOLDER_PATH, DATA_FOLDER, PLUGIN_FOLDER);
			AssureDirectory (PLUGIN_FOLDER_PATH);

			ENCRYPTION_FOLDER_PATH = Path.Combine (DATA_FOLDER_PATH, DATA_FOLDER, ENCRYPTION_FOLDER);
			AssureDirectory (ENCRYPTION_FOLDER_PATH);

			WALLET_FOLDER_PATH = Path.Combine (DATA_FOLDER_PATH, DATA_FOLDER, WALLET_FOLDER);
			AssureDirectory (WALLET_FOLDER_PATH);

			CLASS_FOLDER_PATH = Path.Combine (DATA_FOLDER_PATH, DATA_FOLDER, CLASS_FOLDER);
			AssureDirectory (CLASS_FOLDER_PATH);

			CACHE_FOLDER_PATH = Path.Combine (DATA_FOLDER_PATH, DATA_FOLDER, CACHE_FOLDER);
			AssureDirectory (CACHE_FOLDER_PATH);

			CONFIG_FOLDER_PATH = Path.Combine (DATA_FOLDER_PATH, DATA_FOLDER, CONFIG_FOLDER);
			AssureDirectory (CONFIG_FOLDER_PATH);
		}

		public static String GetSettingsPath (String filename)
		{


			if (filename == null) {
				return null;
			}

			if (filename.Equals ("")) {
				return null;
			}

			//if (!(System.IO.Directory.Exists(DATA_FOLDER))) {
			//	System.IO.Directory.CreateDirectory (DATA_FOLDER);
			//}

			if (CONFIG_FOLDER_PATH == null || CONFIG_FOLDER == null) {
				Logging.WriteLog ("Critical error. invalid config folder");
				// TODO
				Gtk.Application.Quit ();
				return null;
			}

			String result = Path.Combine (CONFIG_FOLDER_PATH, filename);
			return result;
		}

		public static String [] GetDirFileNames (String path, String pattern)
		{
#if DEBUG
			string method_sig = clsstr + nameof (GetDirFileNames) + DebugRippleLibSharp.left_parentheses + nameof (path) + DebugRippleLibSharp.equals + (path ?? "") + DebugRippleLibSharp.comma + nameof (pattern) + DebugRippleLibSharp.equals  + (pattern ?? "") + DebugRippleLibSharp.right_parentheses;
#endif

			try {
				return Directory.GetFiles (path, pattern);    //(PLUGIN_FOLDER_PATH, "*");
			}

#pragma warning disable 0168
			catch (Exception e) {
#pragma warning restore 0168

#if DEBUG
				if (DebugIhildaWallet.FileHelper) {
					Logging.ReportException (method_sig, e);
				}
#endif
				return null;
			}
			//string[] me = new string[filePaths.Length];

			//for (int i = 0; i < filePaths.Length; i++) {
			//	me[i] = filePaths[i].
			//}

		}

		public static String [] GetDirFileNames (String path)
		{
			return GetDirFileNames (path, "*");
		}

		public static String GetPluginPath (String name)
		{
			return Path.Combine (PLUGIN_FOLDER_PATH, name);
		}

		public static String GetWalletPath (String name)
		{
			name = name + ".ice";

			return Path.Combine (WALLET_FOLDER_PATH, name);
		}

		public static bool TestPluginPathAvailability (String name)
		{
#if DEBUG
			String method_sig = clsstr + nameof (TestPluginPathAvailability) + DebugRippleLibSharp.left_parentheses + nameof (String) + DebugRippleLibSharp.space_char + nameof (name) + DebugRippleLibSharp.equals + (name ?? "null") + DebugRippleLibSharp.right_parentheses;
			if (name == null ) {
				if (DebugIhildaWallet.FileHelper) {
					Logging.WriteLog(method_sig + "name == null");
				}
				return false;
			}
#endif

			if (name.Equals ("")) {
				// todo what to do in this case
#if DEBUG
				if (DebugIhildaWallet.FileHelper) {
					Logging.WriteLog(method_sig + "name == \"\", returning false");
				}
#endif
				return false;
			}

			try {

				String path = GetPluginPath (name);

				//if (name == null || name.Equals("")) {  // would never happen. 
				//	return false;
				//}

				if (File.Exists (path)) {
					return false;
				}

				return true;

			}

#pragma warning disable 0168
			catch (Exception e) {
#pragma warning restore 0168

#if DEBUG
				Logging.ReportException(method_sig, e);
#endif

				return false;
			}



		}



		public static void AssureDirectory (String dir)
		{
#if DEBUG
			if (DebugIhildaWallet.FileHelper) {
				Logging.WriteLog("FileHelper : Assuring directory " + dir );
			}
#endif

			if (!(Directory.Exists (dir))) {
				try {
					Directory.CreateDirectory (dir);
				} catch (Exception e) {
					Logging.WriteLog (e.Message);

					Gtk.Application.Quit ();
					return;
				}
			}
		}


		public static bool SaveConfig (String path, String config)
		{
#if DEBUG
			string method_sig = clsstr + nameof (SaveConfig) + DebugRippleLibSharp.left_parentheses + nameof (path) + DebugRippleLibSharp.comma + nameof (config) + DebugRippleLibSharp.right_parentheses;

#endif

			try {
				File.WriteAllText (path, config);

				return true;
			} catch (Exception e) {
#if DEBUG
				Logging.ReportException (method_sig, e);
#endif
				Logging.WriteLog (e.ToString ());

				return false;
			}

			//return true;
		}

		public static bool RobustlySaveConfig (String path, String config)
		{
#if DEBUG
			string method_sig = clsstr + nameof (RobustlySaveConfig) + DebugRippleLibSharp.left_parentheses + nameof (path) + DebugRippleLibSharp.comma + nameof (config) + DebugRippleLibSharp.right_parentheses;
#endif
			try {

				string tempPath = path + FileHelper.TEMP_EXTENTION;

				return true;

			}

#pragma warning disable 0168
			catch (Exception e) {
#pragma warning restore 0168


#if DEBUG
				Logging.ReportException ( method_sig, e );
#endif
				return false;
			}

		}


		public static string GetJsonConf (String path)
		{
#if DEBUG
			String method_sig = clsstr + nameof (GetJsonConf) + DebugRippleLibSharp.left_parentheses + nameof (path) + DebugRippleLibSharp.equals + DebugRippleLibSharp.ToAssertString(path) + DebugRippleLibSharp.right_parentheses;
			if (DebugIhildaWallet.FileHelper) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
#endif
			String str = null;
			if (path == null) {
				return null;
			}

			/*// TODO. I really don't think I need a config cache
			if (configCache == null) {
				configCache = new Dictionary<string, string> ();
			}

			if (configCache.TryGetValue (path, out str)) {
				return str;
			}
			*/


			try {
				if (File.Exists(path)) {

					str = File.ReadAllText(path);
					//if (str!=null) {
					//	configCache.Add(path,str);
					//}
					return str;
				}
			}

#pragma warning disable 0168
			catch (Exception e) {
#pragma warning restore 0168

#if DEBUG
				Logging.WriteLog(method_sig + e.Message);
#endif
				return null;
			}

			return str;

		}

		public static string GetCachePath (String dir, String name)
		{
			String result = Path.Combine (CACHE_FOLDER_PATH, dir, name);
			return result;
		}

		// USE WITH CAUTION !!!! DELETES RECURSIVELY
		public static void ClearFolderContents (String path) {

			DirectoryInfo dir = new DirectoryInfo(path);

			foreach ( FileInfo fi in dir.GetFiles()) {
				fi.IsReadOnly = false;
				fi.Delete();
			}

			foreach ( DirectoryInfo di in dir.GetDirectories()) {
				ClearFolderContents(di.FullName);
				di.Delete();
			}

		}

		public static string LoadResourceText (string id) {
			Assembly assembly = Assembly.GetExecutingAssembly ();
			string ret = null;
			using (Stream stream = assembly.GetManifestResourceStream(id))
			using (StreamReader reader = new StreamReader (stream)) {
				ret = reader.ReadToEnd ();
			}

			return ret;
		}

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		public static string userSetPath = null;


		public static string DATA_FOLDER = Program.appname; // folder that contains all settings
		public static string DATA_FOLDER_PATH = null;  //

		public static string PLUGIN_FOLDER = "plugins";  // folder to place plugins
		public static string PLUGIN_FOLDER_PATH = null;

		public static string ENCRYPTION_FOLDER = "encryption";  
		public static string ENCRYPTION_FOLDER_PATH = null;

		public static string WALLET_FOLDER = "wallets";
		public static string WALLET_FOLDER_PATH = null;

		public static string CLASS_FOLDER = "classes";
		public static string CLASS_FOLDER_PATH = null;

		public static string CACHE_FOLDER = "cache";
		public static string CACHE_FOLDER_PATH = null;

		public static string CONFIG_FOLDER = "config";
		public static string CONFIG_FOLDER_PATH = null;

		public static string examplename = "exampleplugin";

		public static string TEMP_EXTENTION = ".tmp";
		public static string BACKUP_EXT = ".backup";

#pragma warning restore RECS0122 // Initializing field with default value is redundant
		//public static Dictionary<String, String> configCache = new Dictionary<String,String>(); // cache of config paths
		//public static Dictionary<String, String> cachecache = new Dictionary<string, string>(); // It's a cache of cache file



		#if DEBUG
		private const string clsstr = nameof (FileHelper) + DebugRippleLibSharp.colon;
		#endif
	}
}

