using System;

using Gtk;

namespace IhildaWallet
{
	public abstract class Plugin : IhildaWallet.IPlugin
	{
		/*
		public Plugin ()
		{

		}
		*/


		public abstract void PreStartup ();  // splash

		public abstract void PostStartup();  // 



		public abstract Widget GetOptionTab ();

		public abstract Widget GetMainTab ();

		// implemented 
		//public String getTitle () { if (title==null) {title = "";} return title; }

		public IPlugin GetPlugIn () { return this; }

		public String Title { get; set; } 

		public String JsonConf { get; set; }

		public String Tab_Label { get; set; }

		public String Name { get; set; }

	}
}