using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Net.Configuration;
using Ripple.Core.Enums;
using Ripple.Core.Types;
using Ripple.Signing;
using Ripple.Testing.Client;
using Ripple.TxSigning;

namespace Ripple.Testing.Utils
{
    public class TestAccountState
    {
        public List<TrustLine> Lines = new List<TrustLine>();
        public List<Offer> Offers = new List<Offer>();
        public Balance XrpBalance = new Balance();
        public int TxsRequired = 0;
        public readonly AccountId Id;

        public TestAccountState(AccountId id)
        {
            Id = id;
        }

        public static TestAccountState operator <(TestAccountState ac, TrustLine b)
        {
            return ac.AddLine(b);
        }

        public static TestAccountState operator <(TestAccountState ac, Balance b)
        {
            ac.XrpBalance = b;
            return ac;
        }

        public static TestAccountState operator <(TestAccountState ac, Offer b)
        {
            b.Owner = ac.Id;
            ac.Offers.Add(b);
            return ac;
        }

        public TestAccountState AddLine(TrustLine line)
        {
            line.Owner = Id;
            Lines.Add(line);
            return this;
        }

        public static TestAccountState operator >(TestAccountState ac, Balance b)
        {
            throw new NotImplementedException();
        }
        public static TestAccountState operator >(TestAccountState ac, TrustLine b)
        {
            throw new NotImplementedException();
        }
        public static TestAccountState operator >(TestAccountState ac, Offer b)
        {
            throw new NotImplementedException();
        }

        public Amount XrpBalancePlusTxFees(Amount fee)
        {
            try
            {
                var xrp = XrpBalance.XrpBalance;
                var allFees = fee.DecimalValue() * TxsRequired;
                return xrp.NewValue(xrp.DecimalValue() + allFees);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // 
                throw;
            }
        }
    }

    public class TestAccount
    {
        public readonly string Alias;
        public readonly string Secret;
        public readonly AccountId Id;

        public string EncodedId => Id.ToString();
        public IKeyPair KeyPair;
        public TxSigner Signer;
        public uint NextSequence;

        // Starting Xrp Balance/Lines/Offers etc

        private TestAccount(string alias, AccountId id, IKeyPair keyPair, string secret)
        {
            Id = id;
            Secret = secret;
            SetSigningKey(keyPair);
            NextSequence = 1;
            Alias = alias;
        }

        private void SetSigningKey(IKeyPair keyPair)
        {
            KeyPair = keyPair;
            Signer = TxSigner.FromKeyPair(keyPair);
        }

        public Amount Issue(decimal value, Currency currency)
        {
            return new Amount(value, currency, Id);
        }

        public Amount Issue(string value, Currency currency)
        {
            return new Amount(value, currency, Id);
        }

        public static TestAccount FromAlias(string alias)
        {
            var phrase = alias.ToUpper() == "ROOT" ? "masterpassphrase" : alias;
            var seed = Seed.FromPassPhrase(phrase);
            var keyPair = seed.KeyPair();
            var id = new AccountId(keyPair.PubKeyHash());
            return new TestAccount(alias, id, keyPair, seed.ToString());
        }

        public TestAccountState NewState()
        {
            return new TestAccountState(this.Id);
        }

        public static TestAccountState operator <(TestAccount ac, TrustLine b)
        {
            return ac.NewState() < b;
        }

        public static TestAccountState operator <(TestAccount ac, Balance b)
        {
            return ac.NewState() < b;
        }

        public static TestAccountState operator <(TestAccount ac, Offer b)
        {
            return ac.NewState() < b;
        }

        public static TestAccountState operator >(TestAccount ac, Balance b)
        {
            throw new NotImplementedException();
        }
        public static TestAccountState operator >(TestAccount ac, TrustLine b)
        {
            throw new NotImplementedException();
        }
        public static TestAccountState operator >(TestAccount ac, Offer b)
        {
            throw new NotImplementedException();
        }

        public static implicit operator AccountId(TestAccount ac) => ac.Id;
    }

    public class TestAccountStateItem
    {
        public CallerInfo Source = new CallerInfo();
        public AccountId Owner;
        public TxSubmission TxSubmission;
        public TxConfigurator[] TxConfigurators;

        public TxConfigurator[] TxConfiguratorsArray {
            get {
                var l = TxConfigurators?.Length ?? 0;
                var arr = new TxConfigurator[l];
                for (var i = 0; i < l; i++)
                {
                    arr[i] = TxConfigurators?[i];
                }
                return arr;
            }
        }
    }

    public class Offer : TestAccountStateItem
    {
        public Amount TakerPays;
        public Amount TakerGets;
    }

    public class Balance : TestAccountStateItem
    {
        public Amount XrpBalance = 1000 / Currency.Xrp;
    }

    public class TrustLine : TestAccountStateItem
    {
        public Amount Balance;
        public Amount Limit;

        public TrustLine(Amount balance, Amount limit)
        {
            Balance = balance;
            Limit = limit;
        }

        public static implicit operator TrustLine(Amount both)
        {
            return new TrustLine(both, both);
        }
    }
}