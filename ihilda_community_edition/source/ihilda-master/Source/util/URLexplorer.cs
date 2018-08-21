using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace IhildaWallet
{
	public static class URLexplorer
	{
		/*
		public URLexplorer ()
		{
		}
		*/

		public static void OpenUrl(string url)
		{


			try
			{
				Process.Start(url);
			}
			catch
			{

				// The commented code below doesn't compile due to missing dependencies. would have to upgrade nugget to 3.???? 
				// missing RuntimeInformation.dll 

				/*
				// hack because of this: https://github.com/dotnet/corefx/issues/10361
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					url = url.Replace("&", "^&");
					Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					Process.Start("xdg-open", url);
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					Process.Start("open", url);
				}
				else
				{
					throw;
				}
				*/
			}
		}


		public static readonly string proto = "https://";
		public static readonly string xrpChartsUrl = "xrpcharts.ripple.com";


	}
}

