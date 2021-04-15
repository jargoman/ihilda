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

using RippleLibSharp.Commands.Subscriptions;

using RippleLibSharp.Keys;

using RippleLibSharp.Transactions;
using RippleLibSharp.Transactions.TxTypes;

using System.Linq;
using System.Threading;
using RippleLibSharp.Binary;
using System.IO;

using Ripple.Signing;
using Ripple.Signing.K256;
using Org.BouncyCastle.Math;
using Ripple.TxSigning;
using Newtonsoft.Json.Linq;
using System.Collections;
using RippleLibSharp.Source.Mnemonics;

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


			TestMnemonic ();

		
			//System.Console.WriteLine (RippleAddress.RIPPLE_ADDRESS_ICE_ISSUER);

			NetworkInterface ni = TestNetworking ();



			//RLSTest rlst = new RLSTest (ni);

			//RippleDeterministicKeyGenerator.TestVectors ();

			//rlst.testTrustLines ();
			//rlst.TestOfferSignatures();

			//rlst.TestKeyGen ();

			//rlst.SignConsistencyTest();
			//rlst.
			//rlst.AccountSubscribeTest ();

			//rlst.LedgerSubscribeTest ();

			//var key = Seed.FromBase58("ssBrKaAGmb6ZjQgB4BBSJcGqmLa9e").KeyPair();
			var key = Seed.FromBase58 ("sh5NgfPpGZcKWUzbfzXKVBEAUtXiS").KeyPair ();

			Console.WriteLine ("Key Id : = " + key.Id ());
			Console.WriteLine ("key pubHash = " + Base58.ByteArrayToHexString (key.PubKeyHash ()));


			var rseed = new RippleSeedAddress ("sh5NgfPpGZcKWUzbfzXKVBEAUtXiS");

			var privateKey0 = rseed.GetPrivateKey (0);

			string ppp = privateKey0.AsHex ();

			var privateKey = RipplePrivateKey.FromHex (ppp);

			var bi = Base58.DecodeToBigInteger (privateKey.ToString ());

			Console.WriteLine ("Account 0 private key = " + privateKey.ToString ());


			var publicKey = privateKey.GetPublicKey ();
			Console.WriteLine ("Public key = " + publicKey);


			var address = publicKey.GetAddress ();
			Console.WriteLine ("Account 0 address = " + address.ToString ());

			/*
			// The private generator (aka root private key, master private key)
			var privateGen = K256KeyGenerator.ComputePrivateGen(rseed.GetBytes());
			var root_key =
			    // The root keyPair
			    new K256KeyPair(privateGen);

			var secret = K256KeyGenerator.ComputeSecretKey(privateGen, (uint) 0);

			var testPrivate = new RipplePrivateKey(secret);
			var pair0 = new K256KeyPair(secret);
			*/
			var bits = privateKey.GetBytes ();


			BigInteger bigInteger =
			    new BigInteger (1, bits);


			//var b = new BigInteger(0, privateKey.GetBytes());

			K256KeyPair pair = new K256KeyPair (bigInteger);
			var signer = TxSigner.FromKeyPair (pair);


			var dest =
			    new RippleAddress ("rJ8UEAvqsMtQqaRv9VdasEyrf5DauwgZSg");


			var oneXRP = new RippleCurrency (1.0m);

			RipplePaymentTransaction tx =
			    new RipplePaymentTransaction {
				    Destination = dest,
				    Account = address,
				    Amount = oneXRP
			    };

			tx.AutoRequestFee (
			    ni,
			    new CancellationToken ());

			tx.AutoRequestSequence (
			    address,
			    ni,
			    new CancellationToken ());

			//tx.LastLedgerSequence = 

			string jsn = tx.GetJsonTxDotNet ();

			var o = JObject.Parse (jsn);

			SignedTx signedTx = signer.SignStObject (o);

			tx.SignedTransactionBlob = signedTx?.TxBlob;
			tx.hash = signedTx?.Hash;

			tx.signed = true;


			var response = tx.Submit (ni, new CancellationToken ());

			var stat = response.status;



		}

		public static void TestMnemonic ()
		{

			var wrds = new List<string> () {
				"piano",
				"sleep",
		 		"fall",
		 		"aerobic",
		  		"under",
		   		"glide",
		    		"today",
		     		"favorite",
		      		"response",
				"family",
			  	"basket",
			    	"leader",
			      	"inquiry",
				"unable",
				"useless"
	     		};


			MnemonicIsValid valid = MnemonicWordList.IsValid (wrds);

			if (!valid.IsValid) {
				Console.WriteLine ("Not valid : ");
				Console.WriteLine (valid.Message);
				return;
			} else {
				Console.WriteLine (valid.Message);
			}


			/*
			var wrds =
			    "piano sleep fall aerobic under glide today favorite response family basket leader inquiry unable useless"
			 ;*/

			var nm = new MnemonicWordList (wrds);
			nm.LoadKeysFromMnemonic ();

			uint [] accs = { 0, 1, 2, 3, 4 };
			var accounts = nm.GetAccounts (accs);


			foreach (var a in accounts) {
				Console.WriteLine (a.AsHex ());
			}

		}

		public static NetworkInterface TestNetworking ()
		{

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
			System.Console.WriteLine ("connected == " + res.ToString ());
			return ni;
		}

		public void TestTrustLines ()
		{
			CancellationToken token = new CancellationToken ();
			var task = AccountLines.GetResult (test_address.ToString (), NetworkInterfaceObj, token);
			System.Console.WriteLine ("testTrustLines : waiting....\n");
			task.Wait ();
			System.Console.WriteLine ("\n\n testTrustLines : done waiting\n");

			var taskres = task.Result;

			var accrsp = taskres.result;

			System.Console.WriteLine ("accrsp.account = " + accrsp.account);


			int x = 0;
			foreach (var l in accrsp.lines) {
				System.Console.WriteLine ("line " + x++.ToString () + " " + l.account);
			}
		}

		public void TestKeyGen ()
		{

			int count = 0;

			Random rnd = new Random ();
			Byte [] b = new Byte [16];
			int x = 0;
			int max = 100;
			while (x++ < max) {
				rnd.NextBytes (b);

				Console.WriteLine ("The Random bytes are: ");
				for (int i = 0; i <= b.GetUpperBound (0); i++)
					Console.WriteLine ("{0}: {1}", i, b [i]);


				RippleSeedAddress rsa = new RippleSeedAddress (b);

				Response<RpcWalletProposeResult> res = LocalRippledWalletPropose.GetResult (rsa.ToString ());
				string account2 = res.result.account_id;
				string account1 = rsa.GetPublicRippleAddress ();

				if (!account1.Equals (account2)) {
					string messg = "propsed wallet does not match generated seed\n";

					//throw new ArithmeticException (messg);

					Logging.WriteLog (messg);
				} else {

					string messg = "propsed wallet matches generated seed\n";
					Logging.WriteLog (messg);
					count++;
				}
			}

			Logging.WriteLog (x.ToString () + " wallets tested\n");
			Logging.WriteLog (count.ToString () + " wallets succeeded\n");


		}

		public void SignConsistencyTest ()
		{
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


				string signature = rpt.SignRippleLibSharp (rsa);
				string signature2 = rpt.SignLocalRippled (rsa);

			}
		}



		public void TestOfferSignatures ()
		{

			//int numberOfTests = 100;

			RippleSeedAddress seed = new RippleSeedAddress ("sh5HPveFez84GR64twXuwSSS6MNVj");

			string [] curs = {
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

			foreach (int takergets in Enumerable.Range (1, 100)) {
				foreach (int takerpays in Enumerable.Range (1, 100)) {
					foreach (uint sequence in Enumerable.Range (1, 100)) {
						foreach (uint lastLedgerSequence in Enumerable.Range (1, 100)) {
							foreach (int fee in Enumerable.Range (1, 100)) {
								foreach (string s in curs) {
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

									string dotnet = offtx.SignRippleDotNet (seed);
									string rippled = offtx.SignLocalRippled (seed);
									string libsharp = offtx.SignRippleLibSharp (seed);

									byte [] bytes = Base58.HexStringToByteArray (rippled);
									byte [] bytes2 = Base58.HexStringToByteArray (libsharp);

									BinarySerializer bs = new BinarySerializer ();
									MemoryStream memoryStream = new MemoryStream (bytes);
									MemoryStream memoryStream2 = new MemoryStream (bytes2);

									var v = bs.ReadBinaryObject (memoryStream);
									var v2 = bs.ReadBinaryObject (memoryStream2);

									byte [] sig = (byte [])v.GetField (BinaryFieldType.TxnSignature);
									byte [] sig2 = (byte [])v2.GetField (BinaryFieldType.TxnSignature);




									bool sigsame = ArraysEqual (sig, sig2);


									bool m = rippled.Equals (libsharp);

									if (m) {
										Logging.WriteLog ("Match : " + m.ToString ());

										//throw new DataMisalignedException ();
									} else {

										Logging.WriteLog ("Non match : " + m.ToString ());

									}


								}
							}

						}


					}
				}

			}




		}

		private bool ArraysEqual (byte [] one, byte [] two)
		{
			bool equal = true;
			if (one.Length != two.Length) {
				equal = false;
			}
			for (int i = 0; i < one.Length; i++) {
				Logging.WriteLog ("bytes : " + one [i] + " " + two [i]);
				if (one [i] != two [i]) {
					equal = false;
				}
			}
			return equal;
		}

		public void LedgerSubscribeTest ()
		{
			Thread thread = new Thread ((object obj) => {

				String [] accounts = new String [] {
					//test_address.ToString(),
					//test_address2.ToString(),
					//test_address3.ToString()
		    			"rBuDDpdVBt57JbyfXbs8gjWvp4ScKssHzx"
				 };

				SubscribeResponsesConsumer responsesConsumer = new SubscribeResponsesConsumer ();
				responsesConsumer.OnMessageReceived += (object sender, SubscribeEventArgs e) => {
					Logging.WriteLog ("Subscribe message recieved\n");

					Response<object> response = new Response<object> ();
					//response = response.SetFromJsonResp (e.Response);

				};



				Task<Response<SubscribeServerResult>> task = Subscribe.LedgerSubscribe (NetworkInterfaceObj, new CancellationToken (), responsesConsumer);

				task.Wait ();

				Response<SubscribeServerResult> res = task.Result;

			});

			thread.Start ();

			//NetworkInterfaceObj.SendToServer (new byte [] { });

			Task.Delay (1000 * 60 * 30).Wait ();

			// TODO close connection 
		}

		public void AccountSubscribeTest ()
		{

			Thread thread = new Thread ((object obj) => {

				String [] accounts = new String [] {
					//test_address.ToString(),
					//test_address2.ToString(),
					//test_address3.ToString()
		    			"rBuDDpdVBt57JbyfXbs8gjWvp4ScKssHzx"
				 };

				SubscribeResponsesConsumer responsesConsumer = new SubscribeResponsesConsumer ();
				responsesConsumer.OnMessageReceived += (object sender, SubscribeEventArgs e) => {
					Logging.WriteLog ("Subscribe message recieved\n");

					Response<AccountTxResult> response = new Response<AccountTxResult> ();
					//response = response.SetFromJsonResp (e.Response);

				};



				Task<Response<AccountTxResult>> task = Subscribe.AccountSubscribe (accounts, NetworkInterfaceObj, new CancellationToken (), responsesConsumer);

				task.Wait ();

				Response<AccountTxResult> res = task.Result;

			});

			thread.Start ();

			//NetworkInterfaceObj.SendToServer (new byte [] { });

			Task.Delay (1000 * 60 * 30).Wait ();
		}



		RippleAddress test_address = RippleAddress.RIPPLE_ADDRESS_JARGOMAN;
		RippleAddress test_address2 = RippleAddress.RIPPLE_ADDRESS_BITSTAMP;
		RippleAddress test_address3 = "rhub8VRN55s94qWKDv6jmDy1pUykJzF3wq";

		//RippleAddress test_address = "rBuDDpdVBt57JbyfXbs8gjWvp4ScKssHzx";
	}
}

