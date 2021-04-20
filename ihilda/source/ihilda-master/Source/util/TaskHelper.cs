using System;
using System.Threading;
using System.Threading.Tasks;

namespace IhildaWallet
{
	public static class TaskHelper
	{
		public static void GuiInvokeSyncronous (Action action)
		{
			GuiInvokeSyncronous (action, UImain.TokenSource.Token);
		}



		public static void GuiInvokeSyncronous (Action action, CancellationToken token)
		{
			using (var waitHandle = new ManualResetEventSlim ()) {
				Gtk.Application.Invoke ((s, a) => {
					try {
						action ();
					} 

					/*catch (Exception e) {
						e.ToString ();	
					} 
		    			*/

					finally {
						waitHandle.Set ();
					}
				});
				waitHandle.Wait ();
			};
		}

		public static bool TaskIsWaiting (Task task)
		{
			
			return (!task.IsCanceled && !task.IsCompleted && !task.IsFaulted);
		}



	}
}
