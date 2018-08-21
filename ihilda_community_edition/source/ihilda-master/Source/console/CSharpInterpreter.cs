using System;
using System.Reflection;
using RippleLibSharp.Util;
//using Mono.CSharp;

namespace IhildaWallet
{
	public static class CSharpInterpreter
	{
		public static object InterpretCharp ( string code )
		{
			if (code == null) {
				return null;
			}

			if (firstrun) {
				InitInterpreter();
			}

			String str = null;
			//
			try {
				//Evaluator.Run(command);

				//Object o = Evaluator.Evaluate( code );
				//Object o = eval.Evaluate(code);

				//Logging.writeBoth(o.ToString());

				//str = o.ToString();

				//if () {
				//Logging.writeBoth( Debug.toAssertString(str));
				//}
			}

			catch (Exception ex) {
				
				Logging.WriteBoth(ex.Message);
				return null;
			}


			return str;
		}

		public static void InitInterpreter ()
		{
			#if DEBUG
			String method_sig = clsstr + nameof (InitInterpreter) + DebugRippleLibSharp.both_parentheses;
			if (DebugIhildaWallet.CSharpInterpreter) {
				Logging.WriteLog (method_sig + DebugRippleLibSharp.begin);
			}
			#endif

			//CompilerSettings settings = new CompilerSettings ();
			//ConsoleReportPrinter crp = new ConsoleReportPrinter ();

			//CompilerContext cmpc = new CompilerContext (settings,crp);
			//CSharpInterpreter.eval = new Evaluator (cmpc);

			int cnt = 0;
			while (cnt < 2) {
			foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
				if (assembly == null) {
					continue;
				}

				#if DEBUG
				if (DebugIhildaWallet.CSharpInterpreter) {
						Logging.WriteLog(method_sig + nameof (assembly) + DebugRippleLibSharp.equals + assembly.ToString());
				}
				#endif

				if (assembly.GetName().ToString().Contains("ystem")) { // ignore system files dirty hack 101 ;)
					continue;
				}

				try {
					//Mono.CSharp.Evaluator.ReferenceAssembly (assembly);
					//eval.ReferenceAssembly(assembly);
				}

					#pragma warning disable 0168
				catch (NullReferenceException e) {
					#pragma warning restore 0168

					#if DEBUG
					Logging.WriteLog(method_sig + "bad assembly" + e.Message);
					#endif
				}


			}
				//Mono.CSharp.Evaluator.Evaluate("1+2;");
				//eval.Evaluate ("1+2");  // why did I put this here? test? I think there was a reason besides test such as to init 
				cnt++;
			}

			firstrun = false;

		}

		//static Evaluator eval = null;

		private static bool firstrun = true;

		#if DEBUG
		private static readonly string clsstr = nameof(CSharpInterpreter) + DebugRippleLibSharp.colon;
		#endif
	}
}

