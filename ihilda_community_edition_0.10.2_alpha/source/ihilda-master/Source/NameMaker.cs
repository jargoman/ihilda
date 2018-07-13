using System;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public static class NameMaker
	{

		private static readonly char[] numbs = {'1','2','3','4','5','6','7','8','9','0'};

		public static String RequestName (String request, PluginType pluginType)
		{
			#if DEBUG
			String method_sig = clsstr + "requestName ( request = " + (String)(request ?? "null") + ", pluginType = " + pluginType.ToString () + " ) : ";
			if (DebugIhildaWallet.NameMaker) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
			#endif

			// I love this method !!!
			// takes a string. "hello" and if the string exists it numerates it. eg. "hello" "hello" "hello2" ect
			/*
			if (WalletManager.currentInstance==null) {
				// todo issue warning
				return request;
			}
			*/

			if (pluginType == PluginType.WALLET) {
				//lock (WalletManager.walletLock) {
					if (WalletManager.currentInstance?.wallets == null) {

						#if DEBUG
						if (DebugIhildaWallet.NameMaker) {
							Logging.WriteLog (method_sig + "wallets == null, returning " + DebugIhildaWallet.ToAssertString(request));
						}
						#endif

						// todo should this return null or request, you MUST return something otherwise null pointer exceptions below
						return request;
					} 

					#if DEBUG
					if (DebugIhildaWallet.NameMaker) {
						Logging.WriteLog (method_sig + "wallets != null");
					}
					#endif

					
				//}


			}

			/*
			else if (pluginType == PluginType.TAB) {
				if (PluginController.currentInstance == null) {
					#if DEBUG
					if (Debug.NameMaker) {
						Logging.writeLog (method_sig + "warning, plugin controller is null");
					}
					#endif

					// todo should this return null?
				}
			} else if (pluginType == PluginType.ENCRYPTION) {
				// todo, 
			}

			*/


			if (request == null) {
				#if DEBUG
				if (DebugIhildaWallet.NameMaker) {
					Logging.WriteLog (method_sig + "warning, request is null");
				}	
				#endif
			}

			String suggest = request;

			//bool contains = 

			if (request == null || request.Equals ("")) {

				switch (pluginType) {
				case PluginType.WALLET:
					suggest = default_wallet_name;
					break
					;

				case PluginType.TAB:
					suggest = default_plugin_name;
					break
					;

				case PluginType.ENCRYPTION:
					suggest = default_encryption_name;
					break
					;

				}

			}



			#if DEBUG
			if (DebugIhildaWallet.NameMaker) {
				Logging.WriteLog (method_sig + "loop iter");

				// the brackets are VERY important. weirdest bug ever
				Logging.WriteLog (method_sig + "suggest =" + (String)(suggest ?? "null"));
				//Logging.write(method_sig + "pluginType"); 
			}
			#endif

			START:
			if (NameMaker.TestNameAvailable (suggest, PluginType.WALLET)) {
				#if DEBUG
				if (DebugIhildaWallet.NameMaker) {
					Logging.WriteLog (method_sig + "suggested name is available. returning " + (String)(suggest ?? "null" ));
				}
				#endif
				return suggest;
			}


			//if (num != null && !num.Equals (""))
			//	num = num.Trim (); // trim space of front and back

			#if DEBUG
			if (DebugIhildaWallet.NameMaker) {
				Logging.WriteLog ( method_sig + "suggest.length = " +  (string)( suggest?.Length.ToString() ?? "null" ) );
			}
			#endif
			String num = "";
			for (int i = suggest.Length - 1; i >= 0; i--) {
				#if DEBUG
				if (DebugIhildaWallet.NameMaker) {
					Logging.WriteLog (method_sig + "i = " + i.ToString ());
				}
				#endif
				char c = suggest [i];
				#if DEBUG
				if (DebugIhildaWallet.NameMaker) {
					Logging.WriteLog (method_sig + " char c == " + c.ToString ());
				}
#endif


				if (IsNumber (c)) {
#if DEBUG
					if (DebugIhildaWallet.NameMaker) {
						Logging.WriteLog (method_sig + "char c is a number..");
					}
#endif
					num = c.ToString () + num;


#if DEBUG
					if (DebugIhildaWallet.NameMaker) {
						Logging.WriteLog (method_sig + "num is now equal " + num);
					}
#endif

					continue;
				}                   // if the first char (last character in suggest) wasn't a number then start at beginning. eg. hello2
#if DEBUG
				if (DebugIhildaWallet.NameMaker) {
					Logging.WriteLog (method_sig + "num is not a number, it's value = " + num);
				}
#endif

				if (num.Equals ("")) {
#if DEBUG
					if (DebugIhildaWallet.NameMaker) {
						Logging.WriteLog (method_sig + "num equals \"\"");
					}
#endif
					num = start.ToString ();
				}


				suggest = suggest.TrimEnd (numbs); // strip numbers off the end
				try {
#if DEBUG
					if (DebugIhildaWallet.NameMaker) {
						Logging.WriteLog (method_sig + "trying to parse number " + DebugIhildaWallet.ToAssertString (num));
					}
#endif
					int x = Int32.Parse (num);  // parse string to an int
#if DEBUG
					if (DebugIhildaWallet.NameMaker) {
						Logging.WriteLog (method_sig + "successfully parsed int x = " + x.ToString ());
					}
#endif
					suggest = suggest + (++x).ToString ();  // add one and tack it onto the back of the suggested string

#if DEBUG
					if (DebugIhildaWallet.NameMaker) {
						Logging.WriteLog (method_sig + "added one, suggest is now = " + DebugIhildaWallet.ToAssertString (suggest));
					}
#endif

					goto START;

#pragma warning disable 0168
				} catch (Exception e) {
#pragma warning restore 0168

					// todo definite bug !! won't ever happen... hopefully :P

					// Maybe recover by using random letter/number sequence. 

#if DEBUG
					if (DebugIhildaWallet.NameMaker) {
						Logging.WriteLog (method_sig + "exception thrown : " + e.ToString ());
					}
#endif

				}
			}

			//if (!request.Equals(suggest)) {
				// debug. This will never happen. I like robust code though
			//}
			// todo ??? max attempt exceeded
			return null;

		}




		private static bool TestNameAvailable(String name, PluginType pluginType)
		{
			#if DEBUG
			String method_sig = clsstr + nameof (TestNameAvailable) + DebugRippleLibSharp.both_parentheses;
			if ( DebugIhildaWallet.NameMaker ) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.beginn);
				Logging.WriteLog(method_sig + "name = " + (string)(name ?? "null"));
				Logging.WriteLog(method_sig + "pluginType = " + pluginType.ToString());
			}
#endif

			switch (pluginType) {
			case PluginType.WALLET:
				if (WalletManager.currentInstance == null || WalletManager.currentInstance.wallets == null) {
					// todo what to return here?
					return false;
				}

				if (WalletManager.currentInstance.wallets.ContainsKey (name)) {
					return false;
				}

				// todo check for wallet file in dir

				break;

			case PluginType.TAB:
				// todo, determine if plugin name exists

				// !FileHelper.testPluginPathAvailability(suggest)) 
				break;

			case PluginType.ENCRYPTION:
				// todo, determine if encryption name exists
				break;
			}


			/*
			if (pluginType == PluginType.WALLET) {
				if (WalletManager.currentInstance == null || WalletManager.currentInstance.wallets == null) {
					// todo what to return here?
					return false;
				}



				if ( WalletManager.currentInstance.wallets.ContainsKey (name)) {
					return false;
				}


			}

			else if (pluginType == PluginType.TAB) {
				
			}

			else if (pluginType == PluginType.ENCRYPTION) {
				
			}
			*/

			return true;
		}

		private static bool IsNumber (Char c)
		{
			foreach (char cha in numbs) {
				if (c.Equals(cha)) {
					return true;
				}
			}

			return false;

		}

		#if DEBUG
		private static readonly string clsstr = nameof (NameMaker) + DebugRippleLibSharp.colon;
#endif

		//private static int max_attempts = 100000;

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private static readonly int start = 0;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		public static String default_wallet_name = "wallet1";
		public static String default_plugin_name = "plugin1";
		public static String default_encryption_name = "encryption1";
	}
}

