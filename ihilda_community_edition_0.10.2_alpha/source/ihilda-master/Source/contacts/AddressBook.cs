using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Codeplex.Data;
using System.Collections;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public class AddressBook
	{
		static AddressBook ()
		{
			bookSettingsPath = FileHelper.GetSettingsPath (bookSettingsFileName);
		}

		~AddressBook ()
		{
			addresses.Clear();
			addresses = null;
		}

		/*
		public AddressBook ()
		{
			 
		}
		*/


		public bool LoadAddressBook ()
		{
			#if DEBUG
			string method_sig = nameof(LoadAddressBook) + DebugRippleLibSharp.both_parentheses;
			#endif

			try {

				addresses.Clear();
				string jsn = FileHelper.GetJsonConf(bookSettingsPath);

				dynamic book = DynamicJson.Parse(jsn);

				foreach (Object o in book) {
					if (!(o is AddressBookEntry page)) {
						// todo debug invalid addressbook. 
						continue;
					}

					//String name = page.

					addresses.Add (page);//addresses.TryAdd(page);
											 // todo provide feedback for debug

				}


			}

			#pragma warning disable 0168
			catch (Exception e) {
			#pragma warning restore 0168

				// TODO debug
				#if DEBUG
				Logging.ReportException (method_sig, e);
				#endif

				return false;
			}

			return true;
		}

		public bool SaveAddressBook () {
			#if DEBUG
			string method_sig = clsstr + nameof(SaveAddressBook) + DebugRippleLibSharp.both_parentheses;
			#endif

			try {

				if (addresses == null) {
					// todo debug
					return false;
				}

				//ICollection<AddressBookEntry> vals = addresses.ToArray();

				//if (vals == null) {
					// todo debug
				//	return false;
				//}

				//AddressBookEntry[] entries = new AddressBookEntry[vals.Count];
				//vals.CopyTo(entries, 0);

				AddressBookEntry[] entries = addresses.ToArray();
				String json = DynamicJson.Serialize(entries);

				FileHelper.SaveConfig(bookSettingsPath, json);

				return true;

			}

			#pragma warning disable 0168
			 catch ( Exception exc) {
			#pragma warning restore 0168

				#if DEBUG
				if (DebugIhildaWallet.AddressBook) {
					Logging.ReportException (method_sig, exc);
				}
				#endif

				return false;

			}


		}





		//public void updateGivename (String address, String )
		//{

		//}

		/*
		public static string toJson ( AddressBook book )
		{

		}

		public static AddressBook fromAddressBook () {

		} */

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		List<AddressBookEntry> addresses = null;


		static String bookSettingsFileName = "addressbook.jsn";

		static String bookSettingsPath = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant



		#if DEBUG
		private static String clsstr = nameof(AddressBook) + DebugRippleLibSharp.colon;
		#endif

	}
}

