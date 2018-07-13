using System;
using System.Threading.Tasks;
//using RippleLibSharp;

using System.Collections.Generic;

// networking test using statments. 
using RippleLibSharp.Network;
using RippleLibSharp.Util;

// test trustlines using
using RippleLibSharp.Commands.Accounts;
using RippleLibSharp.Result;

using RippleLibSharp.Keys;

using RippleLibSharp.Transactions;
using RippleLibSharp.Transactions.TxTypes;

using System.Linq;

namespace Source.UnitTesting
{
	public class RLSTest
	{
		public RLSTest (NetworkInterface ni)
		{
			this.NetworkInterfaceObj = ni;
		}
		NetworkInterface NetworkInterfaceObj {
			get;
			set;
		}


		//public static void Main ( string[] args ) {
		public static void Main (string [] args)
		{

			System.Console.WriteLine ("testing 1,2,3");

			System.Console.WriteLine (RippleAddress.RIPPLE_ADDRESS_ICE_ISSUER);

			NetworkInterface ni = TestNetworking ();



			RLSTest rlst = new RLSTest (ni);

			//rlst.testTrustLines ();
			//rlst.testOfferSignatures();

			//rlst.TestKeyGen ();

			rlst.SignConsistencyTest();
		}


		public static NetworkInterface TestNetworking () {

			ConnectionSettings connectInfo = new ConnectionSettings {

				//ServerUrls = new string[] { "wss://s1.ripple.com:443", "wss://s2.ripple.com:443" };
				ServerUrls = new string [] { "wss://s.altnet.rippletest.net:51233" },

				//ServerUrls = new string[] { "wss://127.0.0.1:6006" };
				LocalUrl = "localhost",
				UserAgent = "optional spoof browser user agent",
				Reconnect = true
			};


			NetworkInterface ni = new NetworkInterface (connectInfo);
			//ni.connect();

			//NetworkRequestTask.initNetworkTasking ();

			Task<bool> connectTask = ni.ConnectTask ();

			connectTask.Wait ();

			bool res = connectTask.Result;

			//Logging.writeLog ();
			System.Console.WriteLine ("connected == " + res.ToString());
			return ni;
		}

		public void TestTrustLines () {
			var task = AccountLines.GetResult (test_address.ToString(), NetworkInterfaceObj);
			System.Console.WriteLine ("testTrustLines : waiting....\n");
			task.Wait ();
			System.Console.WriteLine ("\n\n testTrustLines : done waiting\n");

			var taskres= task.Result;

			var accrsp = taskres.result;

			System.Console.WriteLine ("accrsp.account = " + accrsp.account);


			int x = 0;
			foreach (var l in accrsp.lines) {
				System.Console.WriteLine ("line " + x++.ToString () + " " + l.account);
			}
		}

		public void TestKeyGen () {
			
			Random rnd = new Random();
			Byte[] b = new Byte[16];
			int x = 0;
			int max = 25000;
			while (x++ < max) {
				rnd.NextBytes(b);

				Console.WriteLine("The Random bytes are: ");
				for (int i = 0; i <= b.GetUpperBound(0); i++) 
					Console.WriteLine("{0}: {1}", i, b[i]);  


				RippleSeedAddress rsa = new RippleSeedAddress (b);

				Response< RpcWalletProposeResult > res = LocalRippledWalletPropose.GetResult (rsa.ToString());
				string account2 = res.result.account_id;
				string account1 = rsa.GetPublicRippleAddress ();

				if (!account1.Equals(account2)) {
					throw new ArithmeticException ("propsed wallet does not match generated seed");
				}
			}

			Logging.WriteLog (x.ToString() + " wallets tested");


		}

		public void SignConsistencyTest () {
			//rMVZ3tvY463H2Z9wStCnUnCebgAcvNi7QQ
			//r3vrqkurd7vKVbf3BA7cVmVqqmYShRGUqd

			//rbo.putField(BinaryFieldType.Flags, this.flags);


			int x = 0;
			while (x++ < 100) {
				RippleSeedAddress rsa = new RippleSeedAddress ("ssBrKaAGmb6ZjQgB4BBSJcGqmLa9e");


				//if (this.sendmax!=null) {
				//	rbo.putField(BinaryFieldType.SendMax, this.sendmax);
				//}


				RipplePaymentTransaction rpt = new RipplePaymentTransaction (rsa.GetPublicRippleAddress (), "r3vrqkurd7vKVbf3BA7cVmVqqmYShRGUqd", "1", null) {
					Sequence = 1,

					Fee = "10"
				};


				string signature = rpt.Sign (rsa);
				string signature2 = rpt.SignLocalRippled (rsa);

			}
		}



		public void TestOfferSignatures () {

			//int numberOfTests = 100;

			RippleSeedAddress seed = new RippleSeedAddress("sh5HPveFez84GR64twXuwSSS6MNVj");

			string[] curs = {
				"ABC",
				"DEF",
				"GHI",
				"JKL",
				"MNO",
				"PQR",
				"STU",
				"VWX",
				"AYZ",
				"USD",
				"BTC",
				"ETH",
				"AUX",
				"LTC",
				"ETC",
				"NEM"
			};

			foreach ( int takergets in Enumerable.Range(1,100) ) {
				foreach ( int takerpays in Enumerable.Range(1,100) ) {
					foreach ( uint sequence in Enumerable.Range(1, 100)) {
						foreach (uint lastLedgerSequence in Enumerable.Range(1,100)) {
							foreach ( int fee in Enumerable.Range(1, 100)) {
								foreach ( string s in curs ) {
									Offer off = new Offer {

										//Account = seed.getPublicRippleAddress ();

										TakerPays = takerpays.ToString (),
										TakerGets = new RippleCurrency (takergets, RippleAddress.RIPPLE_ADDRESS_BITSTAMP, "BTC")
									};

									RippleOfferTransaction offtx = new RippleOfferTransaction (seed.GetPublicRippleAddress (), off) {
										LastLedgerSequence = lastLedgerSequence,

										fee = fee.ToString (),
										Sequence = sequence
									};


									string rippled = offtx.SignLocalRippled (seed);
									string libsharp = offtx.Sign (seed);

									bool m = rippled.Equals (libsharp);

									if (m) {
										Logging.WriteLog ("Match : " + m.ToString ());

										//throw new DataMisalignedException ();
									} else {
										
										Logging.WriteLog ( "Non match : " + m.ToString() );

									}


								}
							}

						}
					

					}
				}

			}




		}

		//RippleAddress test_address = RippleAddress.RIPPLE_ADDRESS_JARGOMAN;
		RippleAddress test_address = "rBuDDpdVBt57JbyfXbs8gjWvp4ScKssHzx";
	}
}

