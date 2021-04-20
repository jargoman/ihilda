using System;
using System.Text;
using Org.BouncyCastle.Math;
using RippleLibSharp.Keys;
using RippleLibSharp.Util;

namespace IhildaWallet
{
	public partial class RandomSeedGenerator : Gtk.Dialog
	{
		public RandomSeedGenerator ()
		{
			this.Build ();

			this.Modal = true;

			StartupSeed ();

			Gdk.Window gwin = this.drawingarea1.GdkWindow;
			Gdk.Color background = new Gdk.Color (150, 150, 150);

			this.drawingarea1.ModifyBg (Gtk.StateType.Normal, background);


			this.drawingarea1.AddEvents ((int)Gdk.EventMask.PointerMotionMask);

			this.drawingarea1.MotionNotifyEvent += (sender, args) => {
				Gdk.EventMotion Event = args.Event;

				// SSLLOOOOWWWW !!!!
				//if (Debug.RandomSeedGenerator) {
				//	Logging.write("RandomSeedGenerator.MotionNotifyEvent. x = " + Event.X.ToString() + ", y = " + Event.Y.ToString() + ", time = " + Event.Time.ToString());
				//}

				//bigInteger = bigInteger.Add( BigInteger.ValueOf( (long)Event.X * (long)Event.Y * (long)Event.Time));

				//bigInteger = bigInteger.Add( BigInteger.ValueOf( (long)Event.X * (long)Event.Y ));
				//bigInteger = bigInteger.Add( BigInteger.ValueOf( (long) Event.Time));
				BigInteger big = BigInteger.ValueOf ((long)(Event.XRoot * Event.YRoot)).Multiply (BigInteger.ValueOf (Event.Time)).Add (BigInteger.ValueOf ((long)Event.X).Add (BigInteger.ValueOf ((long)Event.Y)));

				Random ra = new Random (big.IntValue);

				ra.NextBytes (bytesBuff);

				bigInteger = bigInteger.Xor (new BigInteger (1, bytesBuff));

				Gdk.GC gc = new Gdk.GC (gwin);

				gwin.DrawPoint (gc, (int)Event.X, (int)Event.Y);

				//int cirsize = 5;
				//gwin.DrawArc(gc, true, (int)Event.X - (cirsize / 2), (int)Event.Y - (cirsize / 2), cirsize, cirsize, 0, 23040);
				//gwin.DrawArc(
				Update ();
			};

			Update ();
		}


#if DEBUG
		public string clsstr = nameof (RandomSeedGenerator) + DebugRippleLibSharp.colon;
#endif

		private BigInteger InitBigint (int timeseed)
		{
			String hostname = System.Environment.MachineName;

			timeseed *= GetIntFromString(hostname);

			Random rand = new Random(timeseed); 


			rand.NextBytes(bytesBuff);

			return new BigInteger(1, bytesBuff);
		}

		public BigInteger GetBigInt ()
		{

			return bigInteger;
		}

		public void Update ()
		{
			Gtk.Application.Invoke( delegate {
				this.bigintlabel.Text = bigInteger.ToString();
			});
		}

		//byte[] seedbuff = new byte[16];

		// returns a different number each time
		public RippleSeedAddress GetGeneratedSeed ()
		{
			int timeseed = TimeSeed();

			/*
			if (bigInteger == null) { // won't happen

			}
			*/

			String userstring = this.entry1.Text;

			if (userstring!=null && !userstring.Equals("")) {
				timeseed*= GetIntFromString(userstring);
			}

			Random rand = new Random(timeseed);



			byte[] seedbuff = new byte[16];

			int count = 0;
			while (count < 7) { // seven should be more than enough, the purpose of the loop is the unlikely event that the xor leads to a number with less than 16 bytes

				rand.NextBytes(bytesBuff);

				bigInteger = bigInteger.Xor(new BigInteger(1, bytesBuff));

				byte[] buffer = bigInteger.ToByteArray();

				if (buffer.Length < 16) { // 
					continue;
				}

				System.Array.Copy( buffer, buffer.Length - 16, seedbuff, 0, 16);

				return new RippleSeedAddress(seedbuff);  // used to be seedbuff that was copied. trying to optimise. 
			}

			throw new Exception("The generated seedbytes aren't 16 bytes long");
		}


		private static int TimeSeed ()
		{
			return DateTime.UtcNow.Hour * DateTime.UtcNow.Month * DateTime.UtcNow.Day * DateTime.UtcNow.Minute * DateTime.UtcNow.Second * DateTime.UtcNow.Millisecond;

		}

		private static int GetIntFromString(  String str ) {

			int result = 0;
			if (str!=null) {
				char[] chacha = str.ToCharArray();
				foreach (char move in chacha) {
					result *= move;
				}
			}

			if (result == 0) {
				result++;
			}

			return result;
		}


		public void StartupSeed ()
		{
			bigInteger = InitBigint(TimeSeed());
		}


		//public static RandomSeedGenerator currentInstance


#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private BigInteger bigInteger = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant
		private byte[] bytesBuff = new byte[16];
	}
}

