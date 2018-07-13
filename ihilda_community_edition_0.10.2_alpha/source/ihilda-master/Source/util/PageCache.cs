using System;
using System.Text;
using System.IO;
using Codeplex.Data;
using IhildaWallet;
using System.Collections.Generic;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public class PageCache<t>
	{
		public PageCache (string filenameroot)
		{

#if DEBUG
			string method_sig = clsstr + nameof (PageCache<t>) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			this.directory = filenameroot;
			this.filenameroot = filenameroot + "-page";
			string pth = System.IO.Path.Combine (FileHelper.CACHE_FOLDER_PATH, directory);
			FileHelper.AssureDirectory (pth);


		}



		// changing these to non-static feilds. One cache per page
#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private t [] currentCache = null;
		private t [] firstCache = null;
		private t [] lastCache = null;
		private t [] previousCache = null;
		private t [] nextCache = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		public t [] GetCurrentCache ()
		{

#if DEBUG
			string method_sig = clsstr + nameof (GetCurrentCache ) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			if (currentCache == null) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "currentCache == null");
				}
#endif
				currentCache = GetPage (currentPage);
			}
#if DEBUG
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.returning, currentCache);
			}
#endif
			return currentCache;
		}

		public t [] GetfirstCache ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (GetfirstCache) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			if (firstCache == null) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "firstCache == null");
				}
#endif
				firstCache = GetPage (1);
			}

#if DEBUG
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + "returning ", firstCache);
			}
#endif
			return firstCache;
		}

		public t [] GetNextCache ()
		{
#if DEBUG
			string methog_sig = clsstr + nameof (GetNextCache) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (methog_sig + DebugRippleLibSharp.begin);
			}
#endif
			if (nextCache == null) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (methog_sig + "nextCache == null");
				}
#endif
				if (currentPage == pages - 1) {  // 
#if DEBUG
					if (DebugIhildaWallet.PageCache) {
						Logging.WriteLog (methog_sig + "currentPage == pages - 1");
					}
#endif
					nextCache = GetLastCache ();
				} else {
#if DEBUG
					if (DebugIhildaWallet.PageCache) {
						Logging.WriteLog (methog_sig + "getting page");
					}
#endif
					nextCache = GetPage (currentPage + 1);
				}
			}
#if DEBUG
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (methog_sig + DebugRippleLibSharp.returning, nextCache);
			}
#endif

			return nextCache;
		}

		public t [] GetPreviousCache ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (GetPreviousCache) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.beginn);
			}
#endif
			if (previousCache == null) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "previousCache == null");
				}
#endif
				if (currentPage < 2) {
#if DEBUG
					if (DebugIhildaWallet.PageCache) {
						Logging.WriteLog (method_sig + "currentPage < 2");
					}
#endif
					previousCache = GetfirstCache ();
				} else {
#if DEBUG
					if (DebugIhildaWallet.PageCache) {
						Logging.WriteLog (method_sig + "currentPage !< 2");
					}
#endif
					previousCache = GetPage (currentPage - 1);
				}
			}
#if DEBUG
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.returning, previousCache);
			}
#endif

			return previousCache;
		}

		public t [] GetLastCache ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (GetLastCache) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			if (lastCache == null) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "lastCache == null");
				}
#endif
				lastCache = GetPage (pages);
			}

#if DEBUG
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + "returning ", lastCache);
			}
#endif

			return lastCache;
		}

		public void Preload ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (Preload ) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			//getCurrentCache();  // should be already set?
			GetPreviousCache ();
			GetNextCache ();

		}


		// why not have file writing functions as static, member functions writing to hard drive seems rather odd to me
		/*
		private static void write (String path, String json) {
			string method_sig = clsstr + "write (String path = " + Debug.toAssertString(path) + ", String json = " + Debug.toAssertString(json) + ") : ";
			if (Debug.PageCache) {
				Logging.writeLog (method_sig + Debug.begin);
			}


			//string path = FileHelper.getCachePath(filename);
			try {

				File.WriteAllText(path, json);
			}

			catch (Exception e) {
				
				Logging.writeLog(e.Message);
			}

		}
		*/


		public void SetTotal (int total)
		{
#if DEBUG
			String method_sig = clsstr + "setTotal (total = " + total.ToString () + ") : ";
			if (DebugIhildaWallet.PagerWidget) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			if (this.total == total) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "this.total == total, nothing to do, returning");
				}
#endif
				return;
			}

			if (total < 0) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "total < 0, this is a bug");
				}
#endif
				this.total = 0;

			}

			this.total = total;

			SetNumPages ();
			SanityCheck ();

			this.SetFirst ();


		}

		public void Set (t [] pageArray)
		{


#if DEBUG
			string method_sig = clsstr + "set( pageArray) : ";
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			if (pageArray == null) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "pageArray == null, returning");
				}
#endif
				return;
			}

#if DEBUG
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + "pageArray = ", pageArray);
			}
#endif
			// todo too slow?? overwrite? Clobber over and not care and delete at shutdown?
			DeleteCacheFiles ();

			this.currentCache = null;
			this.nextCache = null;
			this.previousCache = null;
			this.lastCache = null;
			this.firstCache = null;

			SetTotal (pageArray.Length);

			//int pages = total / AccountLines.rowsPerPage;
			//int modu = total % itemsPerPage;

			//if (modu != 0) { // 
			//	pages++;
			//}

#if DEBUG
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + "total = " + total.ToString () + ", pages = " + pageArray.ToString () + ", modu = " + modu.ToString ());
			}
#endif

			// todo add filname + (0).ToString() for meta data. account, number of lines ect

			int count = 0;
			t [] tmp = null;
			int rows = itemsPerPage;  // default 10
			this.SanityCheck ();

			for (int i = 1; i < (pages + 1); i++) { // for each page number

				if (((i == pages) && (modu != 0))) { // if this is the last page and there was a remainder. trim the row size to the amount remaining. 
#if DEBUG
					if (DebugIhildaWallet.PageCache) {
						Logging.WriteLog (method_sig + "trimming last row");
					}
#endif
					rows = modu;
				}

				String path = GetPagePath (i);

				tmp = new t [rows];

				for (int j = 0; j < rows; j++) {

					tmp [j] = pageArray [count++];

				}

				if (currentPage == i) {
#if DEBUG
					if (DebugIhildaWallet.PageCache) {
						Logging.WriteLog (method_sig + "currentPage == i, i = " + i.ToString ());
					}
#endif
					this.currentCache = tmp;

				}

				try { // introducing scope to trigger garbage collection

					String json = DynamicJson.Serialize (tmp);
					FileHelper.SaveConfig (path, json);
					//write(path, json);

				} catch (Exception e) {

					Logging.WriteLog (e.Message);

				}

				/*
				finally {
					if (Debug.PageCache) {
						Logging.writeLog (method_sig + "finally, forcing garbage collection");
					}
					GC.Collect();;
				}
				*/



			}




		}




		private string GetPagePath (int page)
		{
#if DEBUG
			String method_sig = clsstr + "getPagePath ( int page = " + page.ToString () + " ) : ";

			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			if (this.filenameroot == null) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "filename == null, thowing format exception");
				}
#endif
				throw new FormatException ();
			}

			String nm = filenameroot + page.ToString ();  // page 1, page 2 ect. We are cachig the pages, makes the most sense
			String path = FileHelper.GetCachePath (directory, nm);
#if DEBUG
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + "returning " + DebugIhildaWallet.ToAssertString (path));
			}
#endif

			return path;

		}

		public t [] GetPage (int page)
		{
#if DEBUG
			String method_sig = clsstr + "getPage( page=" + page + ") : ";
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			if ((page < 0)) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "page is negative, returning null\n");
				}
#endif
				return null;
			}

			if (page > this.GetNumPages) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "page > this.getNumPages\n");
				}
#endif
				return null;
			}

			String p = GetPagePath (page);


			string path = FileHelper.GetCachePath (directory, p);
#if DEBUG
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + "checking for cache file at " + DebugIhildaWallet.ToAssertString (path));
			}
#endif

			if (!File.Exists (path)) {
				// todo debug
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "path " + path + " does not exist,  page is not cached");
				}
#endif

				return null;
			} 
#if DEBUG
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + "path " + path + " exists,  page is cached");
			}
#endif

			try {

				String json = File.ReadAllText (path);
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "loaded cached json page\n" + (json ?? "json is null"));
				}
#endif

				t [] array = DynamicJson.Parse (json);
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "cached page array = ", array);
					Logging.WriteLog (method_sig + "array length = " + array.Length.ToString ());
				}
#endif

				//TrustLine[] array = dyno.

				t [] ret = new t [array.Length];

				Array.Copy (array, 0L, ret, 0L, array.Length); // hopefully the copy throws the slower dynamic out of scope

				return ret;

			} catch (Exception e) {
				Logging.WriteLog (e.Message + "returning null");
				return null;
			}

			//return null;

		}


		public int GetPage ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (GetPage) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			return this.currentPage;
		}

		public bool HasPages ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (HasPages) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			bool r = pages > 0;
#if DEBUG
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + "returning " + r.ToString ());
			}
#endif
			return r;
		}

		public bool SetNext ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (SetNext) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			if (pages == currentPage) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "pages == currentPage, returning false");
				}
#endif
				return false;
			}

			if (currentPage == pages - 1) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "currentPage == pages - 1, returning setLast");
				}
#endif
				return SetLast ();
			}

			t [] next = null;
			t [] previous = GetCurrentCache ();
			t [] current = GetNextCache ();

			if (currentPage == pages - 2) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "currentPage == pages - 2");
				}
#endif
				next = GetLastCache ();
			}

			nextCache = next;  // only next can be null
			previousCache = previous;
			currentCache = current;


			currentPage++;
#if DEBUG
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + "returning true");
			}
#endif
			return true;
		}

		public bool SetFirst ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (SetFirst) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			if (currentPage == 1) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "currentPage == 1, returning false");
				}
#endif
				return false;
			}

			t [] next = null;
			t [] previous = GetfirstCache ();
			t [] current = GetfirstCache ();

			if (currentPage == 2) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "currentPage == 2");
				}
#endif
				next = GetCurrentCache ();
			} else if (currentPage == 3) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "currentPage == 3");
				}
#endif
				next = GetPreviousCache ();
			} else if (pages == 2) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "pages == 2");
				}
#endif
				next = GetLastCache ();
			}



			nextCache = next;  // only next can be null
			previousCache = previous;
			currentCache = current;

			currentPage = 1;

#if DEBUG
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + "returning true");
			}
#endif
			return true;
		}

		public bool SetPrevious ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (SetPrevious) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			if (currentPage < 2) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "currentPage < 2, returning false");
				}
#endif
				return false;
			}

			if (currentPage == 2) {

				bool r = SetFirst ();
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog ("currentPage == 2, returning " + r.ToString ());
				}
#endif
				return r;
			}

			t [] next = GetCurrentCache ();
			t [] previous = null;
			t [] current = GetPreviousCache ();


			if (currentPage == 3) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "currentPage == 3");
				}
#endif
				previous = GetfirstCache ();
			}

			nextCache = next;
			previousCache = previous; // only previous can be null
			currentCache = current;


			currentPage--;
#if DEBUG
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + "returning true");
			}
#endif
			return true;
		}

		public bool SetLast ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (SetLast) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			if (pages == currentPage) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "pages == currentPage, returning false");
				}
#endif
				return false;
			}

			t [] next = GetLastCache ();
			t [] previous = null;
			t [] current = GetLastCache ();

			if (currentPage == pages - 1) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "currentPage == pages - 1");
				}
#endif
				previous = GetCurrentCache ();
			}

			if (currentPage == pages - 2) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "currentPage == pages - 2");
				}
#endif
				previous = GetNextCache ();
			}

			if (pages == 2) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "pages == 2");
				}
#endif
				previous = GetfirstCache ();
			}

			nextCache = next;
			previousCache = previous; // only previous can be null
			currentCache = current;


			currentPage = pages;
#if DEBUG
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + "returning true");
			}
#endif
			return true;
		}



		private void SetNumPages ()
		{
#if DEBUG
			string method_sig = clsstr + nameof(SetNumPages) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif
			if (this.total < 0) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "this.total < 0");
				}
#endif
				this.total = 0;
			}

			if (this.total == 0) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "this.total == 0");
				}
#endif
				pages = 0;
				return;
			}

			pages = total / itemsPerPage;
			modu = total % itemsPerPage;
#if DEBUG
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + pages.ToString ());
			}
#endif
			if (modu != 0) { // add an extra page if there is a remainder
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "total % itemsPerPage) != 0");
				}
#endif
				pages = pages + 1;
			}

#if DEBUG
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + "setting pages to " + pages.ToString ());
			}
#endif
			//AccountLines.currentInstance.setNumPages (pages);
		}


		public bool SanityCheck ()
		{
#if DEBUG
			String method_sig = clsstr + nameof (SanityCheck) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
				Logging.WriteLog (method_sig + "this.total = " + total.ToString ());
			}
#endif

			if (total < 0) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "total < 0");
				}
#endif
				this.total = 0;
				this.SetNumPages ();
				// todo debug
			}

			if (pages < 1) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "pages < 1");
				}
#endif
				SetNumPages ();
			}

			if (currentPage > pages) {
#if DEBUG
				if (DebugIhildaWallet.PagerWidget) {
					Logging.WriteLog (method_sig + "Adjusting currentPage from " + currentPage.ToString () + " to 1");
				}
#endif
				currentPage = pages;
			}

			if (currentPage < 1) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "currentPage < 1");
				}
#endif
				currentPage = 1;
			}

#if DEBUG
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + "returning true");
			}
#endif

			return true;
		}



		public int GetFirst ()
		{
#if DEBUG
			string method_sig = clsstr + "getFirst () : ";
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + "returning 1");
			}
#endif
			return 1;
		}

		public int GetLast ()
		{
#if DEBUG
			string method_sig = clsstr + "getLast () : ";
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + "returning pages = " + pages.ToString ());
			}
#endif
			return pages;  // todo pages - 1 ??
		}

		public int GetPrevious ()
		{
#if DEBUG
			string methog_sig = clsstr + "getPrevious () : ";
#endif
			if (currentPage < 2) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (methog_sig + "currentPage < 2, returning currentPage = " + currentPage.ToString ());
				}
#endif
				return currentPage;
			}

			int r = currentPage - 1;
#if DEBUG
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (methog_sig + "returning " + r.ToString ());
			}
#endif

			return r;
		}

		public int GetNext ()
		{
#if DEBUG
			string method_sig = clsstr + "getNext () : ";
#endif

			if (pages <= currentPage) {
#if DEBUG
				if (DebugIhildaWallet.PageCache) {
					Logging.WriteLog (method_sig + "pages <= currentPage, returning currentPage = " + currentPage.ToString ());
				}
#endif
				return currentPage;
			}
			int r = currentPage + 1;
#if DEBUG
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + "returning " + r.ToString ());
			}
#endif
			return r;
		}

		public void DeleteCacheFiles ()
		{
#if DEBUG
			string method_sig = clsstr + nameof (DeleteCacheFiles) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.PageCache) {
				Logging.WriteLog (method_sig + "begin, deleting all cache files");
			}
#endif
			String path = System.IO.Path.Combine (FileHelper.CACHE_FOLDER_PATH, directory);


			FileHelper.ClearFolderContents (path);


		}

		public int GetNumPages {
			get { return pages; }
		}





#pragma warning disable RECS0108 // Warns about static fields in generic types
		public static readonly int DEFAULT_ITEMS_PER_PAGE = 10;

		//private static int lastPage = 0;
#if DEBUG
		private static readonly String clsstr = nameof (PageCache<t>) + DebugRippleLibSharp.colon;
#endif

#pragma warning restore RECS0108 // Warns about static fields in generic types

		private int itemsPerPage = DEFAULT_ITEMS_PER_PAGE;

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private int currentPage = 0; // zero would represent no pages


		private int pages = 0;
		private int modu = 0;
		private int total = 0;



		private String filenameroot = null;
		private String directory = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant
	}
}

