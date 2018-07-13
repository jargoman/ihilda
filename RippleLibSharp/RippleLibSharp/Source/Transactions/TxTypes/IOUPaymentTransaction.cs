using System;
using RippleLibSharp.Keys;

namespace RippleLibSharp.Transactions.TxTypes
{
	public class IOUPaymentTransaction
	{
		public IOUPaymentTransaction ()
		{
			sequenceNumber = 0;
		}


#pragma warning disable IDE1006 // Naming Styles
		RippleAddress payer {

			get;
			set;
		}
		RippleAddress payee {
			get;
			set;
		}
		RippleCurrency amount {
			get;
			set;
		}
		RippleCurrency fee {
			get;
			set;
		}
		UInt32 sequenceNumber {
			get;
			set;
		}

		RippleCurrency sendmax {
			get;
			set;
		}

#pragma warning restore IDE1006 // Naming Styles

		public IOUPaymentTransaction (RippleAddress payer, RippleAddress payee, RippleCurrency amount, RippleCurrency fee,UInt32 sequencenumber, RippleCurrency sendmax)
		{
			this.payer = payer;
			this.payee = payee;
			this.amount = amount;
			this.sequenceNumber = sequencenumber;
			this.fee = fee;

			this.sendmax = sendmax;
		}
	}
}

