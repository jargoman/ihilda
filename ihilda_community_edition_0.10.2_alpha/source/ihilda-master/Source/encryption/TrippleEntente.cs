using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using RippleLibSharp.Util;
using RippleLibSharp.Keys;

namespace IhildaWallet
{
	
	public class TrippleEntente : IEncrypt
	{
		/*
		public TrippleEntente ()
		{
			
		}
		*/

		public byte[] Encrypt ( RippleSeedAddress seed,  byte[] salt ) {
			if (salt == null)  {
				throw new ArgumentNullException ();
			}

			//String password = password;

			string first = GetFirstPassword ();

			string second = GetSecondPassword ();

			if (Password == null) {
				// TODO throw error? warn user? 
				throw new ArgumentNullException ();
			}

			string third = Password;

			RippleAddress ra = seed.GetPublicRippleAddress ();

			byte[] address_salt = ra.GetBytes ();


			byte[] cypherone = DoEncryption (seed.GetBytes(), first, salt);
			byte[] cyphertwo = DoEncryption (cypherone, second, address_salt);
			byte[] cypherthree = DoEncryption (cyphertwo, third, salt);

			return cypherthree;
		}

		private byte[] DoEncryption (byte[] encode, String password, byte[] salt) {
			//RippleAddress ra = seed.getPublicRippleAddress ();

			//byte[] address_salt = ra.getBytes ();


			try {

				using (Rijndael myRijndael = Rijndael.Create())
				{


					using ( Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes (password, salt, 123456) ) 
					{
						myRijndael.Key = bytes.GetBytes(32);
						myRijndael.IV = bytes.GetBytes(16);

						using (MemoryStream memorystream = new MemoryStream ()) 
						{

							using (CryptoStream cryptostream = new CryptoStream (memorystream, myRijndael.CreateEncryptor(), CryptoStreamMode.Write))
							{
								


								//byte[] encode = Encoding.ASCII.GetBytes(seed.ToString());
								//byte[] encode = seed.getBytes();
								cryptostream.Write(encode,0,encode.Length);
								cryptostream.Close();

								cryptostream.Flush();
								memorystream.Flush();

								return memorystream.ToArray();
							}
						}


					}
				}


			}


			catch (Exception e) 
			{
				// TODO print to screen debug ect this.mainWindow.
				string m = "Encryption Error : " 
					#if DEBUG
					+ DebugRippleLibSharp.exceptionMessage
					#endif
					;
				Logging.WriteLog ( m + e.Message + "/n");
				return null;
			}


		}

		public byte[] DoDecryption ( byte [] cipher, String password, byte[] salt) {
			



			try {

				//

				using (Rijndael rijndael = Rijndael.Create())
				{
					using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes (password, salt, 123456))
					{
						rijndael.Key = bytes.GetBytes(32); // if the stupid editor won't let you type this without autocompleting into something else just write it as a comment then uncomment it
						rijndael.IV = bytes.GetBytes(16);

						using (MemoryStream memorystream = new MemoryStream()) 
						{
							using (CryptoStream cryptostream = new CryptoStream (memorystream,rijndael.CreateDecryptor(),CryptoStreamMode.Write))
							{
								
								cryptostream.Write(cipher, 0, cipher.Length);
								cryptostream.FlushFinalBlock();
								cryptostream.Close();


								byte[] array = memorystream.ToArray();
								//string value = ASCIIEncoding.ASCII.GetString(array);
								//return value;

								return array;
							}
						}

					}
				}
			}

			catch (Exception e) {
				// TODO print/debug
				string m = "Decryption Error : " 
					#if DEBUG
					+ DebugRippleLibSharp.exceptionMessage
					#endif
					;
				Logging.WriteLog (m + e.Message + "\n");
				return null;
			}
		}

		public byte[] Decrypt ( byte [] cipher, byte[] salt, RippleAddress ra ) {

			string first = GetFirstPassword ();

			string second = GetSecondPassword ();

			if (Password == null) {
				// TODO throw error? warn user? 
				throw new ArgumentNullException ();
			}

			string third = Password;



			byte[] address_salt = ra.GetBytes ();

			byte[] res1 = DoDecryption (cipher, third, salt);
			byte[] res2 = DoDecryption (res1, second, address_salt);
			byte[] res3 = DoDecryption (res2, first, salt);


			return res3;

		}

		public String Name { 
			get { return EncryptionType.TrippleEntente.ToString ("G"); }
			/*set { } */
		}

		public Planet Planet {
			get;
			set;
		}
		public ColorCrypts ColorCrypt {
			get;
			set;
		}

		public Animals Animal {
			get;
			set;
		}

		public Elements Element {
			get;
			set;
		}

		public Cards Card {
			get;
			set;
		}

		public Suits Suit {
			get;
			set;
		}

		public string Pincode {
			get;
			set;
		}

		public string Password {
			get;
			set;
		}





		private string GetFirstPassword () {
			StringBuilder sb = new StringBuilder ();

			sb.Append (Planet.ToString("G"));
			sb.Append (ColorCrypt.ToString("G"));
			sb.Append (Suit.ToString("G"));
			sb.Append (Card.ToString("G"));
			sb.Append (Element.ToString("G"));
			sb.Append (Animal.ToString("G"));

			string str = sb.ToString();

			sb.Clear ();

			foreach (char c in str) {
				sb.Append (GetFromRomanTable(c));
			}

			return sb.ToString ();
}									

		private string GetSecondPassword () {
			StringBuilder sb = new StringBuilder ();

			foreach ( var v in GetIntSelection()) {
				sb.Append (v.ToString());
			}


			sb.Append (Pincode);


			return sb.ToString ();
		}







		private int[] GetIntSelection () {
			int[] ints = new int[6];

			ints[0] = (int)Planet;

			ints[1] =(int)ColorCrypt;

			ints[2] =(int)Suit;
			ints[3] =(int)Card;

			ints[4] =(int)Element;

			ints[5] = (int)Animal;

			return ints;
		}

		public char GetFromRomanTable (char c) {
			foreach (Tuple<char,char> row in romanTable) {
				if (row.Item1 == c) {
					return row.Item2;
				}

			}

			return c;
		}


		public Tuple<char, char>[] romanTable = {
			new Tuple<char, char>('a','h'),
			new Tuple<char, char>('b','a'),
			new Tuple<char, char>('c','w'),
			new Tuple<char, char>('d','g'),
			new Tuple<char, char>('e','s'),
			new Tuple<char, char>('f','r'),
			new Tuple<char, char>('g','d'),
			new Tuple<char, char>('h','q'),
			new Tuple<char, char>('i','p'),
			new Tuple<char, char>('j','o'),
			new Tuple<char, char>('k','y'),
			new Tuple<char, char>('l','n'),
			new Tuple<char, char>('m','b'),
			new Tuple<char, char>('n','i'),
			new Tuple<char, char>('o','v'),
			new Tuple<char, char>('p','c'),
			new Tuple<char, char>('q','x'),
			new Tuple<char, char>('r','m'),
			new Tuple<char, char>('s','l'),
			new Tuple<char, char>('t','t'),
			new Tuple<char, char>('u','k'),
			new Tuple<char, char>('v','u'),
			new Tuple<char, char>('w','e'),
			new Tuple<char, char>('x','z'),
			new Tuple<char, char>('y','f'),
			new Tuple<char, char>('z','j')


		};


//planet = 24 = 2 * 2 * 2 * 3
//color = 256 = 2 * 2 * 2 * 2 * 2 * 2 * 2 * 2; 
//animal = 580 = 2 * 2 * 5 * 29;
//element = 118 = 2 * 59;
// cards = 52 = 13 * 2 * 2

	}



	public enum Cards {
		
		TWO = 2,
		THREE = 3,
		FOUR = 4,
		FIVE = 5,
		SIX = 6,
		SEVEN = 7,
		EIGHT = 8,
		NINE = 9,
		TEN = 10,
		JACK = 11,
		QUEEN = 12,
		KING = 13,
		ACE = 14
	}

	public enum Suits {
		Hearts = '♥',
		Spades = '♠',
		Diamons = '♦',
		Clubs = '♣'
	}
	public enum Planet {
		
		Mercury = '☿',
		Venus = '♀',
		Earth = '⊕',
		Mars = '♂',
		Jupiter = '♃',
		Saturn = '♄',
		Uranus = '♅',
		Neptune = '♆',
		Pluto = '♇',
		Moon = '☾',
		Vesta = '⚶',
		Juno = '⚵',
		Ceres = '⚳',
		Pallas = '⚴',
		AsteroidBelt = '@',
		NorthernStar = '*',
		Sun = '☉',
		Heaven = '✙',
		Hell = '☠',
		Niburu = '☓',
		Europa = 'Σ',
		Io = '!',
		Callisto = '©',
		Ganymede = '∂',



	}


	public enum ColorCrypts {
		// 256 values
		Black = 0x000000,
		Night = 0x0C090A,	
		Gunmetal = 0x2C3539,	
		Midnight = 0x2B1B17,
		Charcoal = 0x34282C,
		DarkSlateGrey = 0x25383C,	
		Oil = 0x3B3131,
		BlackCat = 0x413839,	
		Iridium = 0x3D3C3A,	
		BlackEel = 0x463E3F,
		BlackCow = 0x4C4646,
		GrayWolf = 0x504A4B,	
		VampireGray = 0x565051,	
		GrayDolphin = 0x5C5858,	
		CarbonGray = 0x625D5D,	
		AshGray = 0x666362,	
		CloudyGray = 0x6D6968,	
		SmokeyGray = 0x726E6D,	
		Gray = 0x736F6E,
		Granite = 0x837E7C,
		BattleshipGray = 0x848482,	
		GrayCloud = 0xB6B6B4,	
		GrayGoose = 0xD1D0CE,	
		Platinum = 0xE5E4E2,	
		MetallicSilver = 0xBCC6CC,	
		BlueGray = 0x98AFC7,	
		LightSlateGray = 0x6D7B8D,	
		SlateGray = 0x657383,	
		JetGray = 0x616D7E,	
		MistBlue = 0x646D7E,	
		MarbleBlue = 0x566D7E,
		SlateBlue = 0x737CA1,	
		SteelBlue = 0x4863A0,	
		BlueJay = 0x2B547E,	
		DarkSlateBlue = 0x2B3856,	
		MidnightBlue = 0x151B54,	
		NavyBlue = 0x000080,	
		BlueWhale = 0x342D7E,
		LapisBlue = 0x15317E,
		DenimDarkBlue = 0x151B8D,	
		EarthBlue = 0x0000A0,	
		CobaltBlue = 0x0020C2,
		BlueberryBlue = 0x0041C2,	
		SapphireBlue = 0x2554C7,	
		BlueEyes = 0x1569C7,
		RoyalBlue = 0x2B60DE,	
		BlueOrchid = 0x1F45FC,
		BlueLotus = 0x6960EC,	
		LightSlateBlue = 0x736AFF,	
		WindowsBlue = 0x357EC7,
		GlacialBlueIce = 0x368BC1,	
		SilkBlue = 0x488AC7, 
		BlueIvy = 0x3090C7, 	
		BlueKoi = 0x659EC7,	
		ColumbiaBlue = 0x87AFC7,
		BabyBlue = 0x95B9C7,	
		LightSteelBlue = 0x728FCE,
		OceanBlue = 0x2B65EC, 
		BlueRibbon = 0x306EFF,
		BlueDress = 0x157DEC,	
		DodgerBlue = 0x1589FF,	
		CornflowerBlue = 0x6495ED,	
		SkyBlue = 0x6698FF,	
		ButterflyBlue = 0x38ACEC,	
		Iceberg = 0x56A5EC,	
		CrystalBlue = 0x5CB3FF,	
		DeepSkyBlue = 0x3BB9FF,
		DenimBlue = 0x79BAEC,	
		LightSkyBlue = 0x82CAFA,
		DaySkyBlue = 0x82CAFF,	
		JeansBlue = 0xA0CFEC,
		BlueAngel = 0xB7CEEC,	
		PastelBlue = 0xB4CFEC,		
		SeaBlue = 0xC2DFFF,	
		PowderBlue = 0xC6DEFF,	
		CoralBlue =	0xAFDCEC,	
		LightBlue	=0xADDFFF	,
		RobinEggBlue=	0xBDEDFF,	
		PaleBlueLily=	0xCFECEC,	
		LightCyan	=0xE0FFFF,	
		Water	=0xEBF4FA	,
		AliceBlue=	0xF0F8FF,	
		Azure=	0xF0FFFF,	
		LightSlate=	0xCCFFFF	,
		LightAquamarine=	0x93FFE8,	
		ElectricBlue	=0x9AFEFF	,
		Aquamarine	=0x7FFFD4	,
		CyanorAqua	=0x00FFFF	,
		TronBlue	=0x7DFDFE	,
		BlueZircon	=0x57FEFF	,
		BlueLagoon	=0x8EEBEC	,
		Celeste=	0x50EBEC,	
		BlueDiamond=	0x4EE2EC,	
		TiffanyBlue=	0x81D8D0,	
		CyanOpaque	=0x92C7C7,		
		BlueHosta=	0x77BFC7,	
		NorthernLightsBlue=	0x78C7C7,	
		MediumTurquoise=	0x48CCCD,
		Turquoise	=0x43C6DB,	
		Jellyfish	=0x46C7C7,	
		Bluegreen=	0x7BCCB5,	
		MacawBlueGreen	=0x43BFC7,	
		LightSeaGreen	=0x3EA99F,	
		DarkTurquoise	=0x3B9C9C,	
		SeaTurtleGreen	=0x438D80,	
		MediumAquamarine	=0x348781,	
		GreenishBlue	=0x307D7E	,
		GrayishTurquoise	=0x5E7D7E,	
		BeetleGreen	=0x4C787E,	
		Teal	=0x008080,
		SeaGreen	=0x4E8975,	
		CamouflageGreen	=0x78866B,	
		SageGreen	=0x848b79,	
		HazelGreen	=0x617C58,	
		VenomGreen	=0x728C00,	
		FernGreen	=0x667C26,	
		DarkForestGreen	=0x254117,	
		MediumSeaGreen	=0x306754,	
		MediumForestGreen=	0x347235,	
		SeaweedGreen	=0x437C17	,
		PineGreen	=0x387C44,	
		JungleGreen	=0x347C2C,	
		ShamrockGreen	=0x347C17,	
		MediumSpringGreen	=0x348017,	
		ForestGreen	=0x4E9258,	
		GreenOnion	=0x6AA121,	
		SpringGreen	=0x4AA02C,	
		LimeGreen	=0x41A317,	
		CloverGreen	=0x3EA055,	
		GreenSnake	=0x6CBB3C,	
		AlienGreen	=0x6CC417,	
		GreenApple	=0x4CC417,	
		YellowGreen	=0x52D017,	
		KellyGreen	=0x4CC552,	
		ZombieGreen	=0x54C571,	
		FrogGreen	=0x99C68E,	
		GreenPeas	=0x89C35C,	
		DollarBillGreen	=0x85BB65,	
		DarkSeaGreen	=0x8BB381,	
		IguanaGreen	=0x9CB071,	
		AvocadoGreen	=0xB2C248,	
		PistachioGreen	=0x9DC209,	
		SaladGreen	=0xA1C935,	
		HummingbirdGreen=	0x7FE817,	
		NebulaGreen	=0x59E817,	
		StoplightGoGreen=	0x57E964,	
		AlgaeGreen	=0x64E986,	
		JadeGreen	=0x5EFB6E,	
		Green	=0x00FF00,	
		EmeraldGreen=	0x5FFB17,	
		LawnGreen=	0x87F717,	
		Chartreuse=	0x8AFB17,	
		DragonGreen=	0x6AFB92,	
		Mintgreen	=0x98FF98,	
		GreenThumb	=0xB5EAAA,	
		LightJade	=0xC3FDB8,	
		TeaGreen	=0xCCFB5D,	
		GreenYellow	=0xB1FB17,	
		SlimeGreen	=0xBCE954,	
		Goldenrod	=0xEDDA74,	
		HarvestGold	=0xEDE275,	
		SunYellow	=0xFFE87C,	
		Yellow	=0xFFFF00,	
		CornYellow	=0xFFF380,	
		Parchment	=0xFFFFC2,	
		Cream	=0xFFFFCC,	
		LemonChiffon=	0xFFF8C6,	
		Cornsilk=	0xFFF8DC,	
		Beige	=0xF5F5DC,	
		Blonde	=0xFBF6D9,	
		AntiqueWhite=	0xFAEBD7,	
		Champagne=	0xF7E7CE,	
		BlanchedAlmond=	0xFFEBCD,	
		Vanilla	=0xF3E5AB,	
		TanBrown=	0xECE5B6,	
		Peach	=0xFFE5B4,	
		Mustard	=0xFFDB58,	
		RubberDuckyYellow=	0xFFD801,	
		BrightGold	=0xFDD017,	
		Goldenbrown	=	0xEAC117,		
		MacaroniandCheese=	0xF2BB66,	
		Saffron	=0xFBB917,	
		Beer = 0xFBB117,
		Cantaloupe = 0xFFA62F,
		BeeYellow = 0xE9AB17,
		BrownSugar = 0xE2A76F,
		BurlyWood = 0xDEB887,
		DeepPeach = 0xFFCBA4,
		GingerBrown = 0xC9BE62,
		SchoolBusYellow	= 0xE8A317,
		SandyBrown = 0xEE9A4D,
		FallLeafBrown = 0xC8B560,
		OrangeGold = 0xD4A017,
		Sand = 0xC2B280,
		CookieBrown = 0xC7A317,
		Caramel	= 0xC68E17,
		Brass = 0xB5A642,
		Khaki = 0xADA96E,
		Camelbrown = 0xC19A6B,
		Bronze = 0xCD7F32,
		TigerOrange = 0xC88141,
		Cinnamon = 0xC58917,
		BulletShell = 0xAF9B60,
		DarkGoldenrod = 0xAF7817,
		Copper = 0xB87333,
		Wood = 0x966F33,
		OakBrown = 0x806517,
		Moccasin = 0x827839,
		ArmyBrown = 0x827B60,
		Sandstone = 0x786D5F,
		Mocha = 0x493D26,
		Taupe = 0x483C32,
		Coffee	= 0x6F4E37,	
		BrownBear	=0x835C3B	,
		RedDirt	=0x7F5217	,
		Sepia	=0x7F462C	,
		OrangeSalmon=	0xC47451	,
		Rust	=0xC36241	,
		RedFox=	0xC35817	,
		Chocolate	=0xC85A17	,
		Sedona	=0xCC6600	,
		PapayaOrange	=0xE56717	,
		HalloweenOrange	=0xE66C2C	,
		PumpkinOrange	=0xF87217	,
		ConstructionConeOrange	=0xF87431	,
		SunriseOrange	=0xE67451	,
		MangoOrange	=0xFF8040	,
		DarkOrange	=0xF88017	,
		Coral	=0xFF7F50	,
		BasketBallOrange	=0xF88158	,
		LightSalmon	=0xF9966B	,
		Tangerine	=0xE78A61	,
		DarkSalmon	=0xE18B6B	,
		LightCoral	=0xE77471	,
		BeanRed=	0xF75D59	,
		ValentineRed	=0xE55451	,
		ShockingOrange	=0xE55B3C	,
		Red	=0xFF0000	,
		Scarlet	=0xFF2400	,
		RubyRed	=0xF62217	,
		FerrariRed	=0xF70D1A	,
		FireEngineRed	=0xF62817	,
		LavaRed	=0xE42217	,
		LoveRed	=0xE41B17	,
		Grapefruit	=0xDC381F	,
		ChestnutRed	=0xC34A2C	,
		CherryRed	=0xC24641	,
		Mahogany	=0xC04000	,
		ChilliPepper	=0xC11B17	,
		Cranberry	=0x9F000F	,
		RedWine	=0x990012	,
		Burgundy	=0x8C001A	,
		Chestnut	=0x954535	,
		BloodRed	=0x7E3517	,
		Sienna	=0x8A4117	,
		Sangria	=0x7E3817	,
		Firebrick	=0x800517	,
		Maroon	=0x810541	,
		PlumPie	=0x7D0541	,
		VelvetMaroon	=0x7E354D	,
		PlumVelvet	=0x7D0552	,
		RosyFinch	=0x7F4E52	,
		Puce	=0x7F5A58	,
		DullPurple	=0x7F525D	,
		RosyBrown	=0xB38481	,
		KhakiRose	=0xC5908E	,
		PinkBow	=0xC48189	,
		LipstickPink	=0xC48793	,
		Rose	=0xE8ADAA	,
		RoseGold	=0xECC5C0	,
		DesertSand	=0xEDC9AF	,
		PigPink = 0xFDD7E4,
		CottonCandy = 0xFCDFFF,
		PinkBubblegum = 0xFFDFDD,
		MistyRose = 0xFBBBB9,
		Pink = 0xFAAFBE,
		LightPink = 0xFAAFBA,
		FlamingoPink = 0xF9A7B0,
		PinkRose = 0xE7A1B0,
		PinkDaisy = 0xE799A3,
		CadillacPink = 0xE38AAE,
		Carnati0onPink = 0xF778A1,
		BlushRed = 0xE56E94,
		HotPink	= 0xF660AB,
		WatermelonPink = 0xFC6C85,
		VioletRed	=0xF6358A	,
		DeepPink	=0xF52887	,
		PinkCupcake	=0xE45E9D	,
		PinkLemonade	=0xE4287C	,
		NeonPink	=0xF535AA	,
		Magenta	=0xFF00FF	,
		DimorphothecaMagenta=	0xE3319D	,
		BrightNeonPink	=0xF433FF	,
		PaleVioletRed	=0xD16587	,
		TulipPink	=0xC25A7C	,
		MediumVioletRed	=0xCA226B	,
		RoguePink	=0xC12869	,
		BurntPink	=0xC12267	,
		BashfulPink	=0xC25283	,
		DarkCarnationPink	=0xC12283	,
		Plum	=0xB93B8F	,
		ViolaPurple=	0x7E587E	,
		PurpleIris=	0x571B7E	,
		PlumPurple	=0x583759	,
		Indigo	=0x4B0082	,
		PurpleMonster=	0x461B7E	,
		PurpleHaze=	0x4E387E	,
		Eggplant=	0x614051	,
		Grape	=0x5E5A80	,
		PurpleJam	=0x6A287E	,
		DarkOrchid	=0x7D1B7E	,
		PurpleFlower	=0xA74AC7	,
		MediumOrchid	=0xB048B5	,
		PurpleAmethyst	=0x6C2DC7	,
		DarkViolet	=0x842DCE	,
		Violet	=0x8D38C9	,
		PurpleSageBush	=0x7A5DC7	,
		LovelyPurple	=0x7F38EC	,
		Purple=	0x8E35EF	,
		AztechPurple=	0x893BFF	,
		MediumPurple=	0x8467D7	,
		JasminePurple=	0xA23BEC	,
		PurpleDaffodil=	0xB041FF	,
		TyrianPurple=	0xC45AEC	,
		CrocusPurple=	0x9172EC	,
		PurpleMimosa=	0x9E7BFF	,
		HeliotropePurple=0xD462FF	,
		Crimson	= 0xE238EC,
		PurpleDragon	=0xC38EC7	,
		Lilac	= 0xC8A2C8,
		BlushPink	=0xE6A9EC	,
		Mauve	=0xE0B0FF	,
		WisteriaPurple	=0xC6AEC7	,
		BlossomPink	=0xF9B7FF	,
		Thistle	=0xD2B9D3	,
		Periwinkle	=0xE9CFEC	,
		LavenderPinocchio=	0xEBDDE2	,
		Lavenderblue	=0xE3E4FA	,
		Pearl	=0xFDEEF4	,
		SeaShell	=0xFFF5EE	,
		MilkWhite	=0xFEFCFF	,
		White	=0xFFFFFF

	}


	public enum Animals {
		// 580 values ??

		A,
		Abyssinian,
		AdeliePenguin,
		Affenpinscher,
		AfghanHound,
		AfricanBushElephant,
		AfricanCivet,
		AfricanClawedFrog,
		AfricanForestElephant,
		AfricanPalmCivet,
		AfricanPenguin,
		AfricanTreeToad,
		AfricanWildDog,
		AinuDog,
		AiredaleTerrier,
		Akbash,
		Akita,
		AlaskanMalamute,
		Albatross,
		AldabraGiantTortoise,
		Alligator,
		AlpineDachsbracke,
		AmericanBulldog,
		AmericanCockerSpaniel,
		AmericanCoonhound,
		AmericanEskimoDog,
		AmericanFoxhound,
		AmericanPitBullTerrier,
		AmericanStaffordshireTerrier,
		AmericanWaterSpaniel,
		AnatolianShepherdDog,
		Angelfish,
		Ant,
		Anteater,
		Antelope,
		AppenzellerDog,
		ArcticFox,
		ArcticHare,
		ArcticWolf,
		Armadillo,
		AsianElephant,
		AsianGiantHornet,
		AsianPalmCivet,
		AsiaticBlackBear,
		AustralianCattleDog,
		AustralianKelpieDog,
		AustralianMist,
		AustralianShepherd,
		AustralianTerrier,
		Avocet,
		Axolotl,
		AyeAye,
		B,
		Baboon,
		BactrianCamel,
		Badger,
		Balinese,
		BandedPalmCivet,
		Bandicoot,
		Barb,
		BarnOwl,
		Barnacle,
		Barracuda,
		BasenjiDog,
		BaskingShark,
		BassetHound,
		Bat,
		BavarianMountainHound,
		Beagle,
		Bear,
		BeardedCollie,
		BeardedDragon,
		Beaver,
		BedlingtonTerrier,
		Beetle,
		BengalTiger,
		BerneseMountainDog,
		BichonFrise,
		Binturong,
		Bird,
		BirdsOfParadise,
		Birman,
		Bison,
		BlackBear,
		BlackRhinoceros,
		BlackRussianTerrier,
		BlackWidowSpider,
		Bloodhound,
		BlueLacyDog,
		BlueWhale,
		BluetickCoonhound,
		Bobcat,
		BologneseDog,
		Bombay,
		Bongo,
		Bonobo,
		Booby,
		BorderCollie,
		BorderTerrier,
		BorneanOrangutan,
		BorneoElephant,
		BostonTerrier,
		BottleNosedDolphin,
		BoxerDog,
		BoykinSpaniel,
		BrazilianTerrier,
		BrownBear,
		Budgerigar,
		Buffalo,
		BullMastiff,
		BullShark,
		BullTerrier,
		Bulldog,
		Bullfrog,
		BumbleBee,
		Burmese,
		BurrowingFrog,
		Butterfly,
		ButterflyFish,
		C,
		Caiman,
		CaimanLizard,
		CairnTerrier,
		Camel,
		CanaanDog,
		Capybara,
		Caracal,
		CarolinaDog,
		Cassowary,
		Cat,
		Caterpillar,
		Catfish,
		CavalierKingCharlesSpaniel,
		Centipede,
		CeskyFousek,
		Chameleon,
		Chamois,
		Cheetah,
		ChesapeakeBayRetriever,
		Chicken,
		Chihuahua,
		Chimpanzee,
		Chinchilla,
		ChineseCrestedDog,
		Chinook,
		ChinstrapPenguin,
		Chipmunk,
		ChowChow,
		Cichlid,
		CloudedLeopard,
		ClownFish,
		ClumberSpaniel,
		Coati,
		Cockroach,
		CollaredPeccary,
		Collie,
		CommonBuzzard,
		CommonFrog,
		CommonLoon,
		CommonToad,
		Coral,
		CottontopTamarin,
		Cougar,
		Cow,
		Coyote,
		Crab,
		CrabEatingMacaque,
		Crane,
		CrestedPenguin,
		Crocodile,
		CrossRiverGorilla,
		CurlyCoatedRetriever,
		Cuscus,
		Cuttlefish,
		D,
		Dachshund,
		Dalmatian,
		DarwinsFrog,
		Deer,
		DesertTortoise,
		DeutscheBracke,
		Dhole,
		Dingo,
		Discus,
		DobermanPinscher,
		Dodo,
		Dog,
		DogoArgentino,
		DogueDeBordeaux,
		Dolphin,
		Donkey,
		Dormouse,
		Dragonfly,
		Drever,
		Duck,
		Dugong,
		Dunker,
		DuskyDolphin,
		DwarfCrocodile,
		E,
		Eagle,
		Earwig,
		EasternGorilla,
		EasternLowlandGorilla,
		Echidna,
		EdibleFrog,
		EgyptianMau,
		ElectricEel,
		Elephant,
		ElephantSeal,
		ElephantShrew,
		EmperorPenguin,
		EmperorTamarin,
		Emu,
		EnglishCockerSpaniel,
		EnglishShepherd,
		EnglishSpringerSpaniel,
		EntlebucherMountainDog,
		EpagneulPontAudemer,
		EskimoDog,
		EstrelaMountainDog,
		F,
		Falcon,
		FennecFox,
		Ferret,
		FieldSpaniel,
		FinWhale,
		FinnishSpitz,
		FireBelliedToad,
		Fish,
		FishingCat,
		Flamingo,
		FlatCoatRetriever,
		Flounder,
		Fly,
		FlyingSquirrel,
		Fossa,
		Fox,
		FoxTerrier,
		FrenchBulldog,
		Frigatebird,
		FrilledLizard,
		Frog,
		FurSeal,
		G,
		GalapagosPenguin,
		GalapagosTortoise,
		Gar,
		Gecko,
		GentooPenguin,
		GeoffroysTamarin,
		Gerbil,
		GermanPinscher,
		GermanShepherd,
		Gharial,
		GiantAfricanLandSnail,
		GiantClam,
		GiantPandaBear,
		GiantSchnauzer,
		Gibbon,
		GilaMonster,
		Giraffe,
		GlassLizard,
		GlowWorm,
		Goat,
		GoldenLionTamarin,
		GoldenOriole,
		GoldenRetriever,
		Goose,
		Gopher,
		Gorilla,
		Grasshopper,
		GreatDane,
		GreatWhiteShark,
		GreaterSwissMountainDog,
		GreenBeeEater,
		GreenlandDog,
		GreyMouseLemur,
		GreyReefShark,
		GreySeal,
		Greyhound,
		GrizzlyBear,
		Grouse,
		GuineaFowl,
		GuineaPig,
		Guppy,
		H,
		HammerheadShark,
		Hamster,
		Hare,
		Harrier,
		Havanese,
		Hedgehog,
		HerculesBeetle,
		HermitCrab,
		Heron,
		HighlandCattle,
		Himalayan,
		Hippopotamus,
		HoneyBee,
		HornShark,
		HornedFrog,
		Horse,
		HorseshoeCrab,
		HowlerMonkey,
		Human,
		HumboldtPenguin,
		Hummingbird,
		HumpbackWhale,
		Hyena,
		I,
		Ibis,
		IbizanHound,
		Iguana,
		Impala,
		IndianElephant,
		IndianPalmSquirrel,
		IndianRhinoceros,
		IndianStarTortoise,
		IndochineseTiger,
		Indri,
		Insect,
		IrishSetter,
		IrishWolfHound,
		J,
		JackRussel,
		Jackal,
		Jaguar,
		JapaneseChin,
		JapaneseMacaque,
		JavanRhinoceros,
		Javanese,
		Jellyfish,
		K,
		Kakapo,
		Kangaroo,
		KeelBilledToucan,
		KillerWhale,
		KingCrab,
		KingPenguin,
		Kingfisher,
		Kiwi,
		Koala,
		KomodoDragon,
		Kudu,
		L,
		Labradoodle,
		LabradorRetriever,
		Ladybird,
		LeafTailedGecko,
		Lemming,
		Lemur,
		Leopard,
		LeopardCat,
		LeopardSeal,
		LeopardTortoise,
		Liger,
		Lion,
		Lionfish,
		LittlePenguin,
		Lizard,
		Llama,
		Lobster,
		LongEaredOwl,
		Lynx,
		M,
		MacaroniPenguin,
		Macaw,
		MagellanicPenguin,
		Magpie,
		MaineCoon,
		MalayanCivet,
		MalayanTiger,
		Maltese,
		Manatee,
		Mandrill,
		MantaRay,
		MarineToad,
		Markhor,
		MarshFrog,
		MaskedPalmCivet,
		Mastiff,
		Mayfly,
		Meerkat,
		Millipede,
		MinkeWhale,
		Mole,
		Molly,
		Mongoose,
		Mongrel,
		MonitorLizard,
		Monkey,
		MonteIberiaEleuth,
		Moorhen,
		Moose,
		MorayEel,
		Moth,
		MountainGorilla,
		MountainLion,
		Mouse,
		Mule,
		N,
		Neanderthal,
		NeapolitanMastiff,
		Newfoundland,
		Newt,
		Nightingale,
		NorfolkTerrier,
		NorwegianForest,
		Numbat,
		NurseShark,
		O,
		Ocelot,
		Octopus,
		Okapi,
		OldEnglishSheepdog,
		Olm,
		Opossum,
		Orangutan,
		Ostrich,
		Otter,
		Oyster,
		Q,
		Quail,
		Quetzal,
		Quokka,
		Quoll,
		R,
		Rabbit,
		Raccoon,
		RaccoonDog,
		RadiatedTortoise,
		Ragdoll,
		Rat,
		Rattlesnake,
		RedKneeTarantula,
		RedPanda,
		RedWolf,
		RedhandedTamarin,
		Reindeer,
		Rhinoceros,
		RiverDolphin,
		RiverTurtle,
		Robin,
		RockHyrax,
		RockhopperPenguin,
		RoseateSpoonbill,
		Rottweiler,
		RoyalPenguin,
		RussianBlue,
		S,
		SabreToothedTiger,
		SaintBernard,
		Salamander,
		SandLizard,
		Saola,
		Scorpion,
		ScorpionFish,
		SeaDragon,
		SeaLion,
		SeaOtter,
		SeaSlug,
		SeaSquirt,
		SeaTurtle,
		SeaUrchin,
		Seahorse,
		Seal,
		Serval,
		Sheep,
		ShihTzu,
		Shrimp,
		Siamese,
		SiameseFightingFish,
		Siberian,
		SiberianHusky,
		SiberianTiger,
		SilverDollar,
		Skunk,
		Sloth,
		SlowWorm,
		Snail,
		Snake,
		SnappingTurtle,
		Snowshoe,
		SnowyOwl,
		Somali,
		SouthChinaTiger,
		SpadefootToad,
		Sparrow,
		SpectacledBear,
		SpermWhale,
		SpiderMonkey,
		SpinyDogfish,
		Sponge,
		Squid,
		Squirrel,
		SquirrelMonkey,
		SriLankanElephant,
		StaffordshireBullTerrier,
		StagBeetle,
		Starfish,
		StellersSeaCow,
		StickInsect,
		Stingray,
		Stoat,
		StripedRocketFrog,
		SumatranElephant,
		SumatranOrangutan,
		SumatranRhinoceros,
		SumatranTiger,
		SunBear,
		Swan,
		T,
		Tang,
		Tapir,
		Tarsier,
		TasmanianDevil,
		TawnyOwl,
		Termite,
		Tetra,
		ThornyDevil,
		TibetanMastiff,
		Tiffany,
		Tiger,
		TigerSalamander,
		TigerShark,
		Tortoise,
		Toucan,
		TreeFrog,
		Tropicbird,
		Tuatara,
		Turkey,
		TurkishAngora,
		U,
		Uakari,
		Uguisu,
		Umbrellabird,
		V,
		VampireBat,
		VervetMonkey,
		Vulture,
		W,
		Wallaby,
		Walrus,
		Warthog,
		Wasp,
		WaterBuffalo,
		WaterDragon,
		WaterVole,
		Weasel,
		WelshCorgi,
		WestHighlandTerrier,
		WesternGorilla,
		WesternLowlandGorilla,
		WhaleShark,
		Whippet,
		WhiteFacedCapuchin,
		WhiteRhinoceros,
		WhiteTiger,
		WildBoar,
		Wildebeest,
		Wolf,
		Wolverine,
		Wombat,
		Woodlouse,
		Woodpecker,
		WoollyMammoth,
		WoollyMonkey,
		Wrasse,
		X,
		XRayTetra,
		Y,
		Yak,
		YellowEyedPenguin,
		YorkshireTerrier,
		Z,
		Zebra,
		ZebraShark,
		Zebu,
		Zonkey,
		Zorse,

	}


	public enum Elements {
		Hydrogen = 1,
		Helium = 2,
		Lithium = 3,
		Beryllium = 4,
		Boron = 5,
		Carbon = 6,
		Nitrogen = 7,
		Oxygen = 8,
		Fluorine = 9,
		Neon = 10,
		Sodium = 11,
		Magnesium = 12,
		Aluminium = 13,
		Silicon = 14,
		Phosphorus = 15,
		Sulfur = 16,
		Chlorine = 17,
		Argon = 18,
		Potassium = 19,
		Calcium = 20,
		Scandium = 21,
		Titanium = 22,
		Vanadium = 23,
		Chromium = 24,
		Manganese = 25,
		Iron = 26,
		Cobalt = 27,
		Nickel = 28,
		Copper = 29,
		Zinc = 30,
		Gallium = 31,
		Germanium = 32,
		Arsenic = 33,
		Selenium = 34,
		Bromine = 35,
		Krypton = 36,
		Rubidium = 37,
		Strontium = 38,
		Yttrium = 39,
		Zirconium = 40,
		Niobium = 41,
		Molybdenum = 42,
		Technetium = 43,
		Ruthenium = 44,
		Rhodium = 45,
		Palladium = 46,
		Silver = 47,
		Cadmium = 48,
		Indium = 49,
		Tin = 50,
		Antimony = 51,
		Tellurium = 52,
		Iodine = 53,
		Xenon = 54,
		Cesium = 55,
		Barium = 56,
		Lanthanum = 57,
		Cerium = 58,
		Praseodymium = 59,
		Neodymium = 60,
		Promethium = 61,
		Samarium = 62,
		Europium = 63,
		Gadolinium = 64,
		Terbium = 65,
		Dysprosium = 66,
		Holmium = 67,
		Erbium = 68,
		Thulium = 69,
		Ytterbium = 70,
		Lutetium = 71,
		Hafnium = 72,
		Tantalum = 73,
		Tungsten = 74,
		Rhenium = 75,
		Osmium = 76,
		Iridium = 77,
		Platinum = 78, 
		Gold = 79,
		Mercury = 80,
		Thallium = 81,
		Lead = 82,
		Bismuth = 83,
		Polonium = 84,
		Astatine = 85,
		Radon = 86,
		Francium = 87,
		Radium = 88,
		Actinium = 89,
		Thorium = 90,
		Protactinium = 91,
		Uranium = 92,
		Neptunium = 93,
		Plutonium = 94,
		Americium = 95,
		Curium = 96,
		Berkelium = 97,
		Californium = 98,
		Einsteinium = 99,
		Fermium = 100,
		Mendelevium = 101,
		Nobelium = 102,
		Lawrencium = 103,
		Rutherfordium = 104,
		Dubnium = 105,
		Seaborgium = 106,
		Bohrium = 107,
		Hassium = 108,
		Meitnerium = 109,
		Darmstadtium = 110,
		Roentgenium = 111,
		Copernicium = 112,
		Ununtrium = 113,
		Flerovium = 114,
		Ununpentium = 115,
		Livermorium = 116,
		Ununseptium = 117,
		Ununoctium = 118,

	}
}

