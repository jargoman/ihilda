using System;
using Codeplex.Data;
using System.Text;
using RippleLibSharp.Util;

namespace RippleLibSharp.Network
{
	public class ConnectionSettings
	{
		public ConnectionSettings ()
		{
#if DEBUG
			String method_sig = clsstr + nameof (ConnectionSettings) + DebugRippleLibSharp.both_parentheses;
			if (DebugRippleLibSharp.ConnectionSettings) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
#endif
			Reconnect = true; // setting the default


		}

		public String [] ServerUrls { get; set; }
		public String LocalUrl { get; set; }
		public String UserAgent { get; set; }
		public bool Reconnect { get; set; }

		public override bool Equals (Object obj)
		{
#if DEBUG
			String method_sig = clsstr + nameof (Equals) + DebugRippleLibSharp.left_parentheses + nameof (Object) + DebugRippleLibSharp.space_char + nameof(obj) + DebugRippleLibSharp.equals +  DebugRippleLibSharp.ToAssertString(obj) + DebugRippleLibSharp.right_parentheses;
			if (DebugRippleLibSharp.ConnectionSettings) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
#endif
			if (obj == null) {
#if DEBUG
				if (DebugRippleLibSharp.ConnectionSettings) {
					Logging.WriteLog(method_sig + "o == null, returning false");
				}
#endif
				return false;
			}

			ConnectionSettings ci = obj as ConnectionSettings;

			if (ci == null) {
#if DEBUG
				if (DebugRippleLibSharp.ConnectionSettings) {
					Logging.WriteLog(method_sig + nameof (obj) + " is not an instance of ConnectionInfo, returning false");
				}
#endif
				return false;
			}

			return Equals (ci);
		}

		public bool Equals (ConnectionSettings conInf)
		{
#if DEBUG
			String method_sig = clsstr + nameof (Equals) + DebugRippleLibSharp.left_parentheses + nameof (ConnectionSettings) + DebugRippleLibSharp.space_char + nameof (conInf) + DebugRippleLibSharp.equals + DebugRippleLibSharp.ToAssertString(conInf) + DebugRippleLibSharp.right_parentheses;
			if (DebugRippleLibSharp.ConnectionSettings) {
				Logging.WriteLog(method_sig + DebugRippleLibSharp.begin);
			}
#endif
			if (conInf == null) {
#if DEBUG
				if (DebugRippleLibSharp.ConnectionSettings) {
					Logging.WriteLog(method_sig + "conInf == null, returning false");
				}
#endif
				return false;
			}

			if ((conInf.ServerUrls != null && this.ServerUrls == null) || (conInf.ServerUrls == null && this.ServerUrls != null)) {
#if DEBUG
				if (DebugRippleLibSharp.ConnectionSettings) {
					Logging.WriteLog(method_sig + "server lists differ, returning false");
				}
#endif
				return false;
			}


			if (conInf.ServerUrls != null) {
#if DEBUG
				if (DebugRippleLibSharp.ConnectionSettings) {
					Logging.WriteLog(method_sig + "conInf.servers != null");
				}
#endif

				if (conInf.ServerUrls.Length != this.ServerUrls.Length) {
#if DEBUG
					if (DebugRippleLibSharp.ConnectionSettings) {
						Logging.WriteLog(method_sig + "conInf.servers.Length != this.servers.Length, servers differ, returning false");
					}
#endif
					return false;
				}

				for (int i = 0; i < conInf.ServerUrls.Length; i++) {
#if DEBUG
					if (DebugRippleLibSharp.ConnectionSettings) {
						Logging.WriteLog(method_sig + "for loop, i = " + i.ToString());
					}
#endif

					if ((conInf.ServerUrls [i] == null && this.ServerUrls [i] != null) || (conInf.ServerUrls [i] != null && this.ServerUrls [i] == null)) {
#if DEBUG
						if (DebugRippleLibSharp.ConnectionSettings) {
							Logging.WriteLog(method_sig + "servers " + i.ToString() + "differ, returning false" );
						}
#endif
						return false;
					}

					if (conInf.ServerUrls [i] == null) {
						// if one's null they both are
#if DEBUG
						if (DebugRippleLibSharp.ConnectionSettings) {
							Logging.WriteLog(method_sig + "conInf.servers[i] == null, continuing");
						}
#endif
						continue;
					}

					if (!conInf.ServerUrls [i].Equals (this.ServerUrls [i])) {
#if DEBUG
						if (DebugRippleLibSharp.ConnectionSettings) {
							Logging.WriteLog(method_sig + "servers " + i.ToString() + " aren't equal returning null");
						}
#endif
						return false;
					}


				}
			} // no need for else

			if (conInf.LocalUrl == null && this.LocalUrl != null || this.LocalUrl == null && conInf.LocalUrl != null) {
#if DEBUG
				if ( DebugRippleLibSharp.ConnectionSettings ){
					Logging.WriteLog(method_sig + "local urls differ, returning false");
				}
#endif
				return false;
			}

			if (conInf.LocalUrl != null) {
#if DEBUG
				if (DebugRippleLibSharp.ConnectionSettings) {
					Logging.WriteLog(method_sig + "conInf.local != null");
				}
#endif

				if (!(conInf.LocalUrl.Equals (this.LocalUrl))) {
#if DEBUG
					if (DebugRippleLibSharp.ConnectionSettings) {
						Logging.WriteLog(method_sig + "!(conInf.local.Equals(this.local)), returning false");
					}
#endif
					return false;
				}
			}



			if (conInf.UserAgent == null && this.UserAgent != null || this.UserAgent == null && conInf.UserAgent != null) {
#if DEBUG
				if (DebugRippleLibSharp.ConnectionSettings) {
					Logging.WriteLog(method_sig + "conInf.userAgent == null && this.userAgent != null || this.userAgent == null && conInf.userAgent != null, returning false"); // I know, getting lazy, yawn... 3:36 am -_-

				}
#endif
				return false;
			}

			if (conInf.UserAgent != null) {
#if DEBUG
				if (DebugRippleLibSharp.ConnectionSettings) {
					Logging.WriteLog( method_sig + "conInf.userAgent != null");
				}
#endif

				if (!conInf.UserAgent.Equals (this.UserAgent)) {
#if DEBUG
					if (DebugRippleLibSharp.ConnectionSettings) {
						Logging.WriteLog(method_sig + "conInf.userAgent != this.userAgent, returning null");
					}
#endif
					return false;
				}

			}


			if (conInf.Reconnect != this.Reconnect) {
#if DEBUG
				if (DebugRippleLibSharp.ConnectionSettings) {
					Logging.WriteLog ( method_sig + "conInf.reconnect == this.reconnect" );
				}
#endif
				return false;
			}

			return true;
		}

		public static bool operator == (ConnectionSettings left, ConnectionSettings right)
		{
			if (System.Object.ReferenceEquals (left, right)) {
				return true;
			}

			if ((object)left == null || (object)right == null) {
				return false;
			}

			return left.Equals (right);
		}

		public static bool operator != (ConnectionSettings left, ConnectionSettings right)
		{
			return !(left == right);
		}


		override
		public string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("servers[");
#if DEBUG
			sb.Append (DebugRippleLibSharp.ToAssertString(this.ServerUrls));
#endif
			sb.Append ("] local=");
			sb.Append (this.LocalUrl ?? "null");
			sb.Append (" useragent=");
			sb.Append (this.UserAgent ?? "null");

			return sb.ToString ();

		}

		public static ConnectionSettings ParseSettingsJson (String json)
		{
#if DEBUG
			string method_sig = clsstr + nameof (ParseSettingsJson) + DebugRippleLibSharp.left_parentheses + nameof (json) + DebugRippleLibSharp.equals + DebugRippleLibSharp.ToAssertString(json) + DebugRippleLibSharp.right_parentheses;
			if (DebugRippleLibSharp.NetworkInterface) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
#endif

			if (json == null) {
#if DEBUG
				if (DebugRippleLibSharp.NetworkInterface) {
					Logging.WriteLog(method_sig + nameof (json) + " == null, returning");
				}
#endif
				return null;
			}

			ConnectionSettings conny = null;
#if DEBUG
			if (DebugRippleLibSharp.NetworkInterface) {
				Logging.WriteLog(method_sig + "parsing json\n");
			}
#endif

			try {

				dynamic dyn = DynamicJson.Parse (json);
				conny = dyn;

			} catch (Exception e) {

#if DEBUG
				Logging.WriteLog (method_sig + "Error parsing settings file : Excpetion thown\n" + e.Message);

#endif

				return null;

			}

			return conny;

		}

		public ConnectionSettings DeepCopy (ConnectionSettings connectInfo) {
			ConnectionSettings ret = new ConnectionSettings {
				LocalUrl = connectInfo?.LocalUrl,
				UserAgent = connectInfo?.UserAgent
			};

			if (connectInfo?.ServerUrls != null) {
				string[] srvs = new string[connectInfo.ServerUrls.Length];
				int count = 0;
				foreach (string s in connectInfo.ServerUrls) {
					if (s != null) {
						srvs[count] = s;
						count++;
					}
				}

				ret.ServerUrls = srvs;
			}

			return ret;
		}

		public override int GetHashCode ()
		{
			// TODO implement hash code algorythm
			return base.GetHashCode ();
		}

#if DEBUG
		private static readonly string clsstr = nameof (ConnectionSettings) + DebugRippleLibSharp.colon;
#endif

	}

	public class ConnectAttemptInfo {

		public String ServerUrl { get; set;}
		public String LocalUrl { get; set; }
		public String UserAgent { get; set; }
		//public bool reconnect { get; set; }
	}
}

