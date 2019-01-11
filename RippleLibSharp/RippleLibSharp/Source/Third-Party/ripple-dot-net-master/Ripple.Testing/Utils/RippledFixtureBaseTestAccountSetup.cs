using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Ripple.Core;
using Ripple.Core.ShaMapTree;
using Ripple.Core.Types;

namespace Ripple.Testing.Utils
{

    // RippledFixtureBaseTestAccountSetup
    public abstract partial class RippledFixtureBase
    {
        protected static TrustLine Line(decimal balance,
            Amount limit,
            TxConfigurator[] confs = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
            )
        {
            return new TrustLine(limit.NewValue(balance), limit)
            {
                Source =    
                    new CallerInfo(memberName, sourceFilePath, sourceLineNumber),
                TxConfigurators = confs
            };
        }

        protected static TrustLine Line(Amount limitAndBalance,
            TxConfigurator[] confs = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
            )
        {
            return new TrustLine(limitAndBalance, limitAndBalance)
            {
                Source =
                    new CallerInfo(memberName, sourceFilePath, sourceLineNumber),
                TxConfigurators = confs
            };
        }

        protected static Offer Order(Amount pays,
            Amount gets,
            TxConfigurator[] confs = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
            )
        {
            var configs = new TxConfigurator[] {SetFlag(TxFlag.Passive)};
            if (confs != null)
            {
                configs = configs.Concat(confs).ToArray();
            }
            return new Offer
            {
                Source =
                    new CallerInfo(memberName, sourceFilePath, sourceLineNumber),
                Owner = null, // set elsewhere
                TakerPays = pays,
                TakerGets = gets,
                TxConfigurators = configs
            };
        }

        protected List<TestAccount> GetTestAccounts()
        {
            return GetType()
                .GetFields()
                .Where(f => typeof(TestAccount).IsAssignableFrom(f.FieldType))
                .Select(f => f.GetValue(this))
                .Cast<TestAccount>()
                .ToList();
        }

        public async Task SetupWithDefaultRipple(
            params TestAccountState[] testAccount)
        {
            var accountLookup = GetTestAccounts().ToDictionary(a => a.Id);
            var states = GetAllTestAccountStates(testAccount);
            var stateLookup = states.ToDictionary(a => a.Id);
            var lines = states.SelectMany(a => a.Lines).ToList();
            var offers = states.SelectMany(a => a.Offers).ToList();
            
            // you can't trust an account not in the ledger
            var xrpPayments = states
                // .Concat(trusted)
                .Where(a => !Equals(a.Id, ROOT.Id))
                .Distinct();

            offers.ForEach(a => AddTx(stateLookup[a.Owner]));
            lines.ForEach(l =>
            {
                if (l.Balance.DecimalValue() > 0)
                {
                    AddTx(stateLookup[l.Balance.Issuer]);
                }
                AddTx(stateLookup[l.Owner]);
            });

            await SubmitAndClose(xrpPayments
                .Select(async a =>
                {
                    // DefaultRipple
                    a.TxsRequired++;
                    var balance = a.XrpBalance;
                    var source = balance.Source; // ?? new CallerInfo();
                    return balance.TxSubmission = await Expect(Pay(
                        ROOT,
                        accountLookup[a.Id],
                        a.XrpBalancePlusTxFees(DefaultFee)),
                        // ReSharper disable ExplicitCallerInfoArgument
                        source.MemberName,
                        source.SourceFilePath,
                        source.SourceLineNumber
                        // ReSharper restore ExplicitCallerInfoArgument
                        );
                }));


            await SubmitAndClose(accountLookup.Values.Select(async a =>
            {
                var accountSet = AccountSet(a, With(SetDefaultRipple));
                var sub = await Expect(accountSet);
                return sub;
            }));

            await SubmitAndClose(
                lines.Select(async line =>
                {
                    return line.TxSubmission = await Expect(Trust(
                        accountLookup[line.Owner],
                        line.Limit,
                        line.TxConfiguratorsArray),
                        line.Source);
                }));

            await SubmitAndClose(
                lines.Select(async line =>
                {
                    return line.TxSubmission = await Expect(Pay(
                        accountLookup[line.Balance.Issuer],
                        accountLookup[line.Owner],
                        line.Balance),
                        line.Source);
                }));

            await SubmitAndClose(
                offers.Select(async offer =>
                {
                    return offer.TxSubmission = await Expect(Offer(
                        accountLookup[offer.Owner],
                        offer.TakerPays,
                        offer.TakerGets,
                        // ReSharper disable once SuspiciousTypeConversion.Global
                        offer.TxConfiguratorsArray),
                        offer.Source);
                }));
        }

        protected TestAccountState[] GetAllTestAccountStates(
            TestAccountState[] some)
        {
            var states = some.ToDictionary(s => s.Id);
            return
                GetTestAccounts()
                    .Select(
                        a =>
                            states.ContainsKey(a.Id)
                                ? states[a.Id]
                                : a.NewState())
                    .ToArray();
        }

        private static void AddTx(TestAccountState account)
        {
            account.TxsRequired++;
        }

        protected Balance Balance(Amount native,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            return new Balance
            {
                XrpBalance = native,
                Source =
                    new CallerInfo(memberName, sourceFilePath, sourceLineNumber)
            };
        }

        protected async Task<Hash256> GetNormalisedAccountStateHash()
        {
            var accountState = (await GetValidatedLedger())["accountState"];
            return AccountState.FromJson(accountState, normalise:true).Hash();
        }
    }
}