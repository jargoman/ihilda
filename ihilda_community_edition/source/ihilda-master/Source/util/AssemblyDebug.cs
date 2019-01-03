using System;

namespace IhildaWallet
{
	public class AssemblyDebug
	{
		public void DebugAssembly ()
		{
			
			var v = this.GetType ().Assembly.GetManifestResourceNames ();

			foreach (var s in v) {

				Logging.WriteLog ("Assembly loaded : " + s + "\n");
			}
		}




	}
}

