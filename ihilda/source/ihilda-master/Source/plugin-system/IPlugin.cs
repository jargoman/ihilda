using System;
using Gtk;

namespace IhildaWallet
{
	// not implemented
	public interface IPlugin
	{
		void PreStartup ();  // durring splash

		void PostStartup();   // after show



		Widget GetOptionTab ();  // place to put a widget

		Widget GetMainTab ();  // main widget


		String Name { get; set; }

		String Title { get; set; } 

		String JsonConf { get; set; }

		String Tab_Label { get; set; }


		//static string test = "test";

		// Todo void onNetwork (); onwallet ect

	}


}

