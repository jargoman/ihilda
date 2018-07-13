using System;

namespace IhildaWallet
{
	public class ProfitStrategy
	{
		public ProfitStrategy ( Decimal pay_less, Decimal get_more)
		{
			this.Pay_Less = pay_less;
			this.Get_More = get_more;
		}

		public ProfitStrategy () {

		}

		public Decimal Pay_Less {
			get;
			set;
		}

		public Decimal Get_More {
			get;
			set;
		}
		public static readonly ProfitStrategy JargoONE = new ProfitStrategy(1.007m, 1.007m);  // 
		public static readonly ProfitStrategy JargoTwo = new ProfitStrategy(1.013m, 1.013m);
		public static readonly ProfitStrategy JargoThree = new ProfitStrategy(1.017m, 1.017m);
		public static readonly ProfitStrategy JargoFour = new ProfitStrategy(1.023m, 1.023m);
		public static readonly ProfitStrategy JargoFive = new ProfitStrategy(1.027m, 1.027m);

	}
}

