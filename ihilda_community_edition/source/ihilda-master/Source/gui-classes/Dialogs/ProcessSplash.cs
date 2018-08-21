using System;
using System.Text;
using System.Threading;
using Gtk;
using System.ComponentModel;
using Org.BouncyCastle.Math;
using RippleLibSharp.Keys;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public partial class ProcessSplash : Gtk.Window
	{

		// spaghetti code class :( proving bug prone and hard to maintain
		public ProcessSplash ( BigInteger big, int threads ) : 
				base(Gtk.WindowType.Toplevel)
		{
#if DEBUG
			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append (clsstr);
			stringBuilder.Append (nameof (ProcessSplash));
			stringBuilder.Append (DebugRippleLibSharp.both_parentheses);
			String method_sig =  stringBuilder.ToString() ;

			if (DebugIhildaWallet.ProcessSplash) {
				stringBuilder.Append (DebugRippleLibSharp.begin);
				Logging.WriteLog(stringBuilder.ToString());
				stringBuilder.Clear ();
				stringBuilder.Append (method_sig);
				stringBuilder.Append (nameof (BigInteger));
				stringBuilder.Append (DebugRippleLibSharp.space_char);
				stringBuilder.Append (nameof (big));
				stringBuilder.Append (DebugRippleLibSharp.equals);
				stringBuilder.Append (DebugIhildaWallet.AssertAllowInsecure (big));
				Logging.WriteLog( stringBuilder.ToString());

			}

			stringBuilder.Clear ();
			#endif

			this.bigInteger = big;
			this.Build ();

			this.cancelButton.Clicked += (object sender, EventArgs e) => {
				#if DEBUG
				string event_sig = method_sig + "cancelButton.Clicked : ";
				if (DebugIhildaWallet.ProcessSplash) {
					Logging.WriteLog( event_sig + DebugRippleLibSharp.beginn);
				}
				#endif
				WalletManagerWindow.ShowCurrent();


				this.CancelAll ();



				this.Destroy();
			};




			#if DEBUG
			if (DebugIhildaWallet.ProcessSplash) {
				Logging.WriteLog(method_sig + "creating " + threads.ToString() + " new threads");
			}
			#endif
			workers = new ThreadedBackgroundWorker[threads];

			for (int i = 0; i < threads; i++) {
				workers [i] = new ThreadedBackgroundWorker {
					ThreadNumber = i
				};
				SetBackGroundWorker (workers[i]);
			}

		}

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		ThreadedBackgroundWorker [] workers = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant
		//public RippleSeedAddress result = null;


		public void SetBackGroundWorker (ThreadedBackgroundWorker bgw)
		{


			#if DEBUG
			//String method_sig = clsstr + "setBackGroundWorker ( threadNumber = " + threadNumber + ") : ";
			if (DebugIhildaWallet.ProcessSplash) {
				Logging.WriteLog("setBeckGroundWorker : thread " + bgw.ThreadNumber.ToString() + DebugRippleLibSharp.begin);
			}
			#endif

			bgw.WorkerSupportsCancellation = true;
			bgw.WorkerReportsProgress = true;

			bgw.DoWork += delegate(object sender, DoWorkEventArgs e) {
				#if DEBUG
				String method_sig = clsstr + "thread number " + bgw.ThreadNumber.ToString() + " : DoWork : ";

				if (DebugIhildaWallet.ProcessSplash) {
					Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
				}
				#endif


				this.DoWork (sender, e);

				/*
				if (Debug.ProcessSplash) {
					Logging.writeLog(method_sig + "doWork returned" );
					Logging.writeLog( method_sig + "result = " + (string)(result == null ? "null" : result.ToString() ));
				}
				*/

				//Logging.writeLog(method_sig + "saving the result");

				/*if (result!=null) {
					Logging.writeLog(method_sig + "result=" + result.ToString());
					this.result = res;
				}

				else {
					Logging.writeLog(method_sig + "result=null");
				}*/


				//Thread.Sleep(1000);

			};

		


			bgw.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs e) => {
				


				//Thread.Sleep(1000);

				#if DEBUG
				String method_sig = clsstr + "thread number " + bgw.ThreadNumber.ToString() + DebugRippleLibSharp.space_char + nameof (bgw.RunWorkerCompleted) +  DebugRippleLibSharp.both_parentheses;
				if (DebugIhildaWallet.ProcessSplash) {
					Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
				}
				#endif

				//this.Hide();



				if (WalletManager.currentInstance==null) {
						// todo debug
					#if DEBUG
					if (DebugIhildaWallet.ProcessSplash) {
						Logging.WriteLog(method_sig + "WalletManager.currentInstance==null");
					}
					#endif
					return;
				}

				//if (bgw.result == null) {
				//	if (Debug.ProcessSplash) {
				//		Logging.writeLog(method_sig + "result equals null");
				//	}

				//	return;
				//}
					
				#if DEBUG
				if (DebugIhildaWallet.ProcessSplash) {
					Logging.WriteLog(method_sig + "result != null");
				}
				#endif



			};

			bgw.ProgressChanged += (object sender, ProgressChangedEventArgs e) => {
				#if DEBUG
				String method_sig = clsstr + nameof(bgw.ProgressChanged) + DebugRippleLibSharp.both_parentheses;
				if ( DebugIhildaWallet.ProcessSplash ) {
					Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
				}


				#endif

				if (!(sender is BackgroundWorker backwrk)) {
					
#if DEBUG
					if (DebugIhildaWallet.FromScriptDialog) {
						Logging.WriteLog (method_sig + "not a BackgroundWorker, returning");
					}
#endif
					return;
				}



				if (!backwrk.IsBusy) {
					#if DEBUG
					if (DebugIhildaWallet.ProcessSplash) {
						Logging.WriteLog(method_sig + "is not busy");
					}
					#endif
					return;
				}

				if (backwrk.CancellationPending) {
					#if DEBUG
					if (DebugIhildaWallet.ProcessSplash) {
						Logging.WriteLog(method_sig + "backwrk.CancellationPending");
					}
					#endif
					return;
				}


				Gtk.Application.Invoke( delegate {
					#if DEBUG
					if (DebugIhildaWallet.ProcessSplash) {
						Logging.WriteLog(method_sig + "gtk invoke progressbar pulse begin");
					}
#endif

					if (this.progressbar1 == null) {
						return;
					}

					if (!this.progressbar1.IsRealized) {
						#if DEBUG
						if (DebugIhildaWallet.ProcessSplash) {
							Logging.WriteLog(method_sig + "progressbar is not realized");
						}
						#endif
						return;
					}

					if (!this.progressbar1.IsDrawable) {
						#if DEBUG
						if (DebugIhildaWallet.ProcessSplash) {
							Logging.WriteLog(method_sig + "progressbar is not ");
						}
						#endif
						return;
					}


					#if DEBUG
					if (DebugIhildaWallet.ProcessSplash) {
						Logging.WriteLog(method_sig + "progressbar is realised, pulsing");
					}
					#endif


					this.progressbar1.Pulse();  // wahooo!!
					return;
				});

			};






		}


		public void SetParams (String[] pattern, bool ignoreCase, bool start, bool end, bool contains, int tries) {
			if (ignoreCase) {
				String[] lower = new string[pattern.Length];

				for (int i = 0; i < pattern.Length; i++) {
					lower[i] = pattern[i].ToLower ();
				}

				pattern = lower;

			}

			this.pattern = pattern;
			this.ignoreCase = ignoreCase;
			this.start = start;
			this.end = end;
			this.contains = contains;
			this.tries = tries;
		}

		public String[] pattern; 
		public bool ignoreCase; 
		public bool start; 
		public bool end; 
		public bool contains; 
		public int tries;

		private BigInteger bigInteger;


		// returns a different number each time
		public RippleSeedAddress GetNextSeed ()
		{






			#if DEBUG
			String method_sig = clsstr + nameof (GetNextSeed) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.ProcessSplash) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
			#endif

			byte[] seedbuff = new byte[16];

			byte[] buffer = null;

			lock (bigInteger) {
			bigInteger = bigInteger.Add(BigInteger.One);
				buffer = bigInteger.ToByteArray();
			}

			if (buffer.Length < 16) { // 
				Exception esc = new Exception("The generated seedbytes aren't 16 bytes long");
				Logging.WriteLog(esc.Message + " length is " + buffer.Length.ToString());
				throw esc;
			}



			System.Array.Copy( buffer, buffer.Length - 16, seedbuff, 0, 16);

			return new RippleSeedAddress(seedbuff);  // used to be seedbuff that was copied. trying to optimise. 
		}



		private void DoWork (object sender, DoWorkEventArgs e)
		{ 
			ThreadedBackgroundWorker bgw = sender as ThreadedBackgroundWorker;

			if (sender == null) {
				// todo debug
				#if DEBUG
				if (DebugIhildaWallet.ProcessSplash) {
					Logging.WriteLog( "Error, doWork needs to to run in the background" );
				}
				#endif
				return;
			}

			RippleSeedAddress seed = null;

			StringBuilder bild = new StringBuilder();

			foreach (String pat in pattern) {
				if (pat != null) {
					bild.Append(pat ?? "null" + " ");
				}

			}

			#if DEBUG
			String method_sig = clsstr + nameof (DoWork) + DebugRippleLibSharp.colon + "thread " + bgw.ThreadNumber.ToString() + "( pattern=" + bild.ToString() + ", ignoreCase=" + ignoreCase.ToString() + ", start=" + start.ToString() + ", end=" + contains.ToString() + ", tries=" + tries.ToString() + " : ";
			if (DebugIhildaWallet.ProcessSplash) {
				Logging.WriteLog( method_sig + DebugRippleLibSharp.beginn );
			}
			#endif

			if (pattern == null) {
				// todo debug
				#if DEBUG
				if (DebugIhildaWallet.ProcessSplash) {
					Logging.WriteLog(method_sig + "pattern == null");
				}
				#endif
				return;
			}

			if (!(start || end || contains)) {

				// todo there must be a way to exit the loop. 
				#if DEBUG
				if (DebugIhildaWallet.ProcessSplash) {
					Logging.WriteLog(method_sig + "must be able to exit loop");
				}
				#endif

				return;
			}

			int x = 0; //

			while (!bgw.CancellationPending) {

				#if DEBUG
				if (DebugIhildaWallet.ProcessSplash) {
					Logging.WriteLog( method_sig + "while loop" );
				}
				#endif

				seed = GetNextSeed();

				if (seed == null) {
					#if DEBUG
					if (DebugIhildaWallet.ProcessSplash) {
						Logging.WriteLog( method_sig + "seed == null" );
					}
					#endif
				}

				#if DEBUG
				if (DebugIhildaWallet.ProcessSplash) {
					Logging.WriteLog( method_sig + "seed = " + seed.ToString ());
				}
				#endif

				RippleAddress address = seed.GetPublicRippleAddress();

				#if DEBUG
				if ( DebugIhildaWallet.ProcessSplash ) {
					Logging.WriteLog ( method_sig + "ad = " + address.ToString());
				}
				#endif

				/*
				if (ad== null || "".Equals(ad.ToString())) {
					// todo debug
					return null;
				}
				*/

				String s = this.ignoreCase ? address.ToString().ToLower() : address.ToString();

				/*
				Gtk.Application.Invoke ( delegate {
					if (Debug.ProcessSplash) {
						Logging.write(method_sig + "Gui Pulse");
					}
					this.progressbar1.Pulse();

				});
*/

				Application.Invoke( delegate {
					int xx = x;

					try {
						if (bgw.IsBusy && !bgw.CancellationPending ) {
							#if DEBUG
							if (DebugIhildaWallet.ProcessSplash) {
								Logging.WriteLog(method_sig + "reporting progress");
							}
							#endif
							bgw.ReportProgress(xx);
						}

						else {
							#if DEBUG
							if (DebugIhildaWallet.ProcessSplash) {
								Logging.WriteLog(method_sig + "background worker has ended");
							}
							#endif
						}
					}

					#pragma warning disable 0168
					catch (Exception ee) {
						#pragma warning disable 0168

						#if DEBUG
						Logging.WriteLog(method_sig + DebugRippleLibSharp.exceptionMessage + ee.Message);
						#endif
					}
				}
				);


				foreach (String pat in pattern) {

					//  this is put here to check often if thread has been cancelled.
					if (bgw.CancellationPending) {
						#if DEBUG
						if (DebugIhildaWallet.ProcessSplash) {
							Logging.WriteLog(method_sig + "CancellationPending");
						}
						#endif

						e.Cancel = true;

						return;
					}


					if (contains) {
						if (s.Contains(pat)) {
							#if DEBUG
							if (DebugIhildaWallet.ProcessSplash) {
								Logging.WriteLog(method_sig + "Match (contains): " + pat + " returning seed = " + seed.ToString());
							}
							#endif
							//bgw.result = seed;
							FoundSeed (seed);
							this.CancelAll();
							this.Hide ();
							return;
						}

						//continue; // used to continue but realised continue was returning flow up two loops to the while loop. 
					}

					else {
						// if ( start && s.StartsWith(pat) ) {
						if ( start && s.StartsWith(pat, StringComparison.CurrentCulture)) {
							#if DEBUG
							if ( DebugIhildaWallet.ProcessSplash ) {
								Logging.WriteLog(method_sig + "Match (starts with) : " + pat + " returning seed = " + seed.ToString());
							}
							#endif

							//bgw.result = seed;
							this.CancelAll();
							FoundSeed (seed);
							this.Hide ();
							return;
						}
						//if ( end && s.EndsWith(pat) ) {
						if ( end && s.EndsWith(pat, StringComparison.CurrentCulture)) {
							#if DEBUG
							if (DebugIhildaWallet.ProcessSplash) {
								Logging.WriteLog(method_sig + "Match ( ends with ): " + pat + " returning seed = " + seed.ToString());
							}
							#endif

							//bgw.result = seed;
							this.CancelAll();
							FoundSeed(seed);
							this.Hide ();
							return;
						}


					}

				}


				if (tries == 0) {
#if DEBUG
					if (DebugIhildaWallet.ProcessSplash) {
						Logging.WriteLog (method_sig + "tries == 0, continuing");
					}
#endif
					continue;
				}
				if (tries == ++x) {
#if DEBUG
					if (DebugIhildaWallet.RandomSeedGenerator) {
						Logging.WriteLog (method_sig + "exhausted " + x.ToString () + " number of tries");
					}
#endif
					return;
				}


			}


			//if ( Debug.ProcessSplash ) {
			//	Logging.writeLog (method_sig + "returning null ");
			//}
			//return null;
		}

		public void CancelAll ()
		{
			#if DEBUG
			string method_sig = clsstr + nameof (CancelAll) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.ProcessSplash) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn );
			}
			#endif
			foreach (BackgroundWorker bgw in workers) {
				//if (bgw.IsBusy) { // todo figure out if this is necessary
					
				if (bgw.WorkerSupportsCancellation) {
					#if DEBUG
					if (DebugIhildaWallet.ProcessSplash) {
						Logging.WriteLog(method_sig + "bgw.WorkerSupportsCancellation");
					}
					#endif
					bgw.CancelAsync();
				}
				//}
			}

		}

		public void RunScript ()
		{
			#if DEBUG
			String method_sig = clsstr + nameof (RunScript) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.ProcessSplash) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
			}
			#endif

			foreach (BackgroundWorker bgw in workers) {

				if (!bgw.IsBusy) {
					#if DEBUG
					if (DebugIhildaWallet.ProcessSplash) {
						Logging.WriteLog(method_sig + "running backgroundworker async");
					}
					#endif

					bgw.RunWorkerAsync ();  //
				}

				else {
					#if DEBUG
					if (DebugIhildaWallet.ProcessSplash) {
						Logging.WriteLog(method_sig + "woker busy");
					}
					#endif
				}
			}

		}


		public static void FoundSeed ( RippleSeedAddress rsa ) {
			#if DEBUG
			String method_sig = clsstr + "foundSeed (  ) : ";
			#endif


			Application.Invoke( delegate {
				#if DEBUG
				if (DebugIhildaWallet.ProcessSplash) {
					Logging.WriteLog(method_sig + "Gtk invove found seed begin");
				}
				#endif

				FromSecretDialog fsd = new FromSecretDialog("From script ", new RippleWallet ( rsa, RippleWalletTypeEnum.Master) );

				while (true) {
					#if DEBUG
					if (DebugIhildaWallet.ProcessSplash) {
						Logging.WriteLog(method_sig + "while loop");
					}
					#endif

					ResponseType resp = (ResponseType) fsd.Run();
					fsd.Hide();

					if (resp != ResponseType.Ok) {
						#if DEBUG
						if (DebugIhildaWallet.ProcessSplash) {
							Logging.WriteLog(method_sig + "resp != ResponseType.Ok");
						}
						#endif
						break;
					}

					RippleWallet rw = fsd.GetWallet();
					if (rw != null) {
						#if DEBUG
						if (DebugIhildaWallet.ProcessSplash) {
							Logging.WriteLog(method_sig + "rw != null");
						}
						#endif

						WalletManagerWidget.ThreadedWalletAdd(rw);
						return;

					}



				}

				if (fsd!=null) {
					//fsd.Hide();
					fsd.Destroy();
					fsd = null;
				}






				if (WalletManagerWindow.currentInstance!=null) {
					WalletManagerWindow.ShowCurrent();
				}

				else {
					// todo debug debug debug 

					MessageDialog.ShowMessage("");
					Application.Quit();
				}


				//this.Destroy();

			});
		}


		
#if DEBUG
		public static string clsstr = nameof (ProcessSplash) + DebugRippleLibSharp.colon;
#endif


	}
}

