using System;
using System.Linq;
using System.Collections.Generic;


using NBitcoin;

using Org.BouncyCastle.Asn1.Sec;
using RippleLibSharp.Keys;

namespace RippleLibSharp.Source.Mnemonics
{
	public class MnemonicWordList
	{

		public MnemonicWordList () { }


		public MnemonicWordList (IEnumerable<string> words)
		{


			SetWordList (words);
		}

		public static MnemonicIsValid IsValid (IEnumerable<string> words)
		{
			MnemonicIsValid mnemonicIsValid = new MnemonicIsValid ();

			mnemonicIsValid.language = NBitcoin.Wordlist.AutoDetectLanguage (words.ToArray ());
			Console.WriteLine (mnemonicIsValid.language);

			if (mnemonicIsValid.language == Language.Unknown) {
				mnemonicIsValid.IsValid = false;
				mnemonicIsValid.Message = "Unknown language";
				return mnemonicIsValid;
			}

			Wordlist lanlist = GetLanguage (mnemonicIsValid.language);

			foreach (string word in words) {
				bool has = lanlist.WordExists (word, out int index);

				if (!has) {
					mnemonicIsValid.IsValid = false;
					mnemonicIsValid.Message = word + " not found in " + mnemonicIsValid.language.ToString () + " word list";
					return mnemonicIsValid;
				}
			}

			//12,15,18,21 or 24

			switch (words.Count ()) {
			case 12:
			case 15:
			case 18:
			case 21:
			case 24:
				try {
					RandomUtils.Random = new UnsecureRandom ();

					Mnemonic mnemonic = new Mnemonic (string.Join (" ", words));

					if (!mnemonic.IsValidChecksum) {
						mnemonicIsValid.IsValid = false;
						mnemonicIsValid.Message = "Invalid checksum";

						return mnemonicIsValid;
					}


				} catch (Exception e) {

					mnemonicIsValid.IsValid = false;
					mnemonicIsValid.Message = e.Message;
					return mnemonicIsValid;
				}


				mnemonicIsValid.IsValid = true;
				mnemonicIsValid.Message = "Valid word list";
				return mnemonicIsValid;

			}

			mnemonicIsValid.IsValid = false;
			mnemonicIsValid.Message = "List must contain 12, 15, 18, 21 or 24 words";
			return mnemonicIsValid;
		}



		public void SetWordList (IEnumerable<string> words)
		{


			mnemonicPhrase = string.Join (" ", words);

			try {
				Wordlist wl = Wordlist.AutoDetect (mnemonicPhrase);

				Console.WriteLine (wl.WordCount);



			} catch (Exception e) {
				Console.WriteLine (e.Message);
			}

			Console.WriteLine ("cont");
		}



		//public Language language;

		// default for xrp
		public string keyPath = "m/44'/144'/0'/0";

		private string mnemonicPhrase { get; set; }

		public string password { get; set; }


		public ExtKey BIP39_SEED { get; set; }

		public ExtKey Account_Extended_Private_key { get; set; }

		public static bool ValidateKeyPath (string keyPath)
		{
			return KeyPath.TryParse (keyPath, out KeyPath p);

		}

		public bool LoadKeysFromMnemonic ()
		{
			try {
				RandomUtils.Random = new UnsecureRandom ();

				Mnemonic mnemonic = new Mnemonic (this.mnemonicPhrase);

				KeyPath keyPathToDerive = KeyPath.Parse (keyPath);

				byte [] seed = mnemonic.DeriveSeed ();

				BIP39_SEED = new ExtKey (seed);

				Account_Extended_Private_key = BIP39_SEED.Derive (keyPathToDerive);


			} catch (Exception e) {
				Console.WriteLine (e.Message);
				return false;
			}

			return true;
		}

		public IEnumerable<RipplePrivateKey> GetAccounts (IEnumerable<uint> accounts)
		{

			IEnumerable<RipplePrivateKey> privateKeys = from uint i in accounts select GetAccount (i);
			return privateKeys;

		}



		public RipplePrivateKey GetAccount (uint account)
		{
			try {


				ExtKey keyNew = Account_Extended_Private_key.Derive (account);

				byte [] pkeyBytes = keyNew.PrivateKey.PubKey.ToBytes ();
				var ecParams = SecNamedCurves.GetByName ("secp256k1");
				var point = ecParams.Curve.DecodePoint (pkeyBytes);
				var xCoord = point.XCoord.GetEncoded ();
				var yCoord = point.YCoord.GetEncoded ();
				var uncompressedBytes = new byte [64];
				// copy X coordinate
				Array.Copy (xCoord, uncompressedBytes, xCoord.Length);
				// copy Y coordinate
				for (int i = 0; i < 32 && i < yCoord.Length; i++) {
					uncompressedBytes [uncompressedBytes.Length - 1 - i] = yCoord [yCoord.Length - 1 - i];
				}
				var PrivateKey = keyNew.PrivateKey.ToBytes ();

				var privk = new RipplePrivateKey (PrivateKey);

				// System.Console.WriteLine("privk = " + privk.GetHumanReadableIdentifier());
				//Console.WriteLine("as hex = " + privk.AsHex());


				return privk;


				//var PublicKey = pkeyBytes;
				// ethereum address is last 20 bytes of keccak(uncompressed public key)
				// use any c# keccak implementation.
				//var PublicKeyString = CryptoUtils.BytesToHexString(Utils.Solidity.Keccak(uncompressedBytes)).Substring(24);
			} catch (Exception e) {


			}

			return null;
		}



		// private static Dictionary<Language, Wordlist> dic = new Dictionary<Language, Wordlist>();
		public static Wordlist GetLanguage (Language language)
		{

			var task = Wordlist.LoadWordList (language);

			task.Wait ();

			Wordlist list = task.Result;

			return list;
		}
	}

	public class MnemonicIsValid
	{
		public MnemonicIsValid ()
		{

		}

		public bool IsValid { get; set; }
		public Language language { get; set; }
		public string Message { get; set; }
	}
}
