using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json.Linq;
using Ripple.Core;
using Ripple.Core.Enums;
using Ripple.Core.Types;
using Ripple.Testing.Client;
using Ripple.Testing.Server;
using Ripple.TxSigning;

namespace Ripple.Testing.Utils
{
    public abstract partial class RippledFixtureBase
    {
        public static readonly ILog Log = Logging.ForContext();

        public MultiThreadedFailures MultiThreadedFailures = new MultiThreadedFailures();
        public Thread TestThread;
        public Rippled Rippled;
        public Connection Ws;

        public abstract ITestFrameworkAbstraction TestHelper();

        public abstract string RippledBasePath { get; }
        public abstract string RippledBinaryPath { get; }

        public abstract void PerformOneTimeSetUp();
        public abstract void PerformSetUp();
        public abstract void PerformOneTimeTearDown();
        public abstract void PerformTearDown();

        // ReSharper disable InconsistentNaming
        [AutoFilled]
        public Currency 
              USD
            , EUR
            , KHR
            , XRP
            ;

        [AutoFilled]
        public TestAccount 
              ROOT
            , MARY
            , GW1
            , GW2
            , BOB
            , ALICE
            , JOE
            ;
        // ReSharper restore InconsistentNaming

        static RippledFixtureBase()
        {
            if (Environment.GetEnvironmentVariable("CI") != null)
            {
                Logging.ConfigureConsoleLogging();
            }
        }

        private void SetupNamedMembers()
        {
            SetUpAccounts();
            AutoFilled.Set(this, Currency.FromString);
        }

        protected Func<object, string> Alias; 

        private void SetUpAccounts()
        {
            var testAccounts = AutoFilled.Set(this, TestAccount.FromAlias);
            var lookup = testAccounts.ToDictionary(a => a.Id.ToString());
            var pattern = string.Join("|", testAccounts.Select(a => a.Id));
            Alias = o => Regex.Replace(o.ToString(), pattern, match =>
                                       lookup[match.Groups[0].Value].Alias);
        }

        public void SetupRippled()
        {
            TestThread = Thread.CurrentThread;
            FixtureScopedRippledBase.Log.DebugFormat("{1}() tid={0}", TestThread.ManagedThreadId, nameof(SetupRippled));
            SetupNamedMembers();
            var conf = RippledConfig.PrepareWithDefault(RippledBasePath);
            var execConf = conf.ExecutionConfig(RippledBinaryPath)
                               .WithVerbosity(false);
            Rippled = new Rippled(execConf).Start();
            Ws = new Connection(conf.AdminWebSocketUrl());
            TestHelper().RunAsyncAction(Ws.ConnectAndSubscribe);
            if (Rippled.Proc.HasExited)
            {
                throw new InvalidOperationException("probably connected to a rippled that " +
                                                       "wasn't torn down properly");
            }
        }

        public void TearDownRippled()
        {
            FixtureScopedRippledBase.Log.DebugFormat(nameof(TearDownRippled));
            Ws.Disconnect();
            Rippled.Proc.Kill();
            Rippled.Proc.WaitForExit();
        }

        public async Task<JObject> GetValidatedLedger()
        {
            var args = new {ledger_index = "validated", full = true};
            var ledger = await Ws.RequestAsync("ledger", args);
            return (JObject) ledger["ledger"];
        }

        public async Task<TxSubmission> Pay(
            TestAccount src,
            TestAccount dest,
            Amount amt,
            TxConfigurator[] configure = null)
        {
            var payment = new StObject
            {
                [Field.TransactionType] = TransactionType.Payment,
                [Field.Amount] = amt,
                [Field.Destination] = dest.Id
            };

            var signed = SignAndConfigure(src, payment, configure);
            return await Ws.Submit(signed.TxBlob);
        }

        public async Task<TxSubmission> Trust(
            TestAccount src,
            Amount amt,
            TxConfigurator[] configure = null)
        {
            var trustSet = new StObject
            {
                [Field.TransactionType] = TransactionType.TrustSet,
                [Field.LimitAmount] = amt
            };

            var signed = SignAndConfigure(src, trustSet, configure);
            return await Ws.Submit(signed.TxBlob);
        }

        public async Task<TxSubmission> Offer(
            TestAccount src,
            Amount pays,
            Amount gets,
            TxConfigurator[] configure = null)
        {
            var offer = new StObject
            {
                [Field.TransactionType] = TransactionType.OfferCreate,
                [Field.TakerPays] = pays,
                [Field.TakerGets] = gets
            };
            var signed = SignAndConfigure(src, offer, configure);
            return await Ws.Submit(signed.TxBlob);
        }

        public async Task<TxSubmission> OfferCancel(
            TestAccount src,
            Uint32 cancel,
            TxConfigurator[] configure = null)
        {
            var offer = new StObject
            {
                [Field.TransactionType] = TransactionType.OfferCreate,
                [Field.OfferSequence] = cancel
            };

            var signed = SignAndConfigure(src, offer, configure);
            return await Ws.Submit(signed.TxBlob);
        }

        public async Task<TxSubmission> AccountSet(
            TestAccount src,
            TxConfigurator[] configure = null)
        {
            var offer = new StObject
            {
                [Field.TransactionType] = TransactionType.AccountSet
            };

            var signed = SignAndConfigure(src, offer, configure);
            return await Ws.Submit(signed.TxBlob);
        }

        public SignedTx SignAndConfigure(TestAccount src,
                                         StObject tx, 
                                         TxConfigurator[] configure = null)
        {
            AutoPopulateFields(src, tx);
            configure = configure ?? new TxConfigurator[0];
            foreach (var conf in configure)
            {
                conf.PreSubmit?.Invoke(tx);
            }
            var signed = GetSignedTx(tx, src);
            foreach (var conf in configure)
            {
                conf.AfterSigning?.Invoke(signed);
            }
            return signed;
        }

        private static SignedTx GetSignedTx(StObject tx, TestAccount src)
        {
            if (tx.Has(Field.Signers) || tx.Has(Field.TxnSignature))
            {
                return TxSigner.ValidateAndEncode(tx);
            }
            return src.Signer.SignStObject(tx);
        }

        public void AutoPopulateFields(TestAccount src, StObject tx)
        {
            if (!tx.Has(Field.Sequence))
            {
                tx[Field.Sequence] = src.NextSequence++;
            }
            if (!tx.Has(Field.Fee))
            {
                tx[Field.Fee] = DefaultFee;
            }
            if (!tx.Has(Field.Account))
            {
                tx[Field.Account] = src.Id;
            }
        }

        public async Task<int> LedgerAccept()
        {
            return await Ws.LedgerAccept();
        }


        public static Action<StObject> SetFlag(uint flag) => o => o.SetFlag(flag);
        public static Action<StObject> SetIndexedFlag(uint flag) => o => o[Field.SetFlag] = flag;
        public static Action<StObject> ClearIndexedFlag(uint flag) => o => o[Field.ClearFlag] = flag;

        public static Action<StObject> 
            PartialPaymentFlag = SetFlag(TxFlag.PartialPayment)
          , SellFlag = SetFlag(TxFlag.Sell)
          , FillOrKillFlag = SetFlag(TxFlag.FillOrKill)
          , DisallowXrpFlag = SetFlag(TxFlag.DisallowXrp)
          , NoRippleFlag = SetFlag(TxFlag.SetNoRipple)
          , ClearDefaultRipple = ClearIndexedFlag(TxFlag.AsfDefaultRipple)
          , SetDefaultRipple = SetIndexedFlag(TxFlag.AsfDefaultRipple)
          ;

        protected Amount DefaultFee = "1000";

        public static Action<StObject> Replaces(Uint32 offerSequence) =>
                o => o[Field.OfferSequence] = offerSequence;

        public Action<StObject> Debug =>
            tx => Console.WriteLine(Alias(tx.ToJson()));

        public Action<SignedTx> DebugSigned
            => tx => Console.WriteLine(Alias(tx.TxJson));

        /// <summary>
        /// Paths DSL Hop creation helper
        /// </summary>
        /// <param name="trickResharper">First positional is ignored and only
        ///                              here to make other keyword args 
        ///                              non redundant.
        /// </param>
        /// <param name="c">currency</param>
        /// <param name="a">account</param>
        /// <param name="i">issuer</param>
        /// <returns></returns>
        protected static PathHop Hop(bool? trickResharper=null, 
                                     Currency c = null, 
                                     AccountId a = null, 
                                     AccountId i = null)
        {
            return new PathHop(a, i, c);
        }

        protected static Path Path(params PathHop[] hops)
        {
            return new Path(hops);
        }

        protected static Action<StObject> SendMax(Amount s)
        {
            return t => t[Field.SendMax] = s;
        }

        protected static Action<StObject> Paths(params Path[] paths)
        {
            return t => t[Field.Paths] = new PathSet(paths);
        }

        protected static Action<StObject> Paths(params PathHop[] hops)
        {
            return t => t[Field.Paths] = new PathSet(new[] {new Path(hops)});
        }

        protected async Task<TxResult> RequestTxResult(Hash256 hash)
        {
            return await Ws.RequestTxResult(hash);
        }

        // ReSharper disable once InconsistentNaming
        protected async Task<TxSubmission> Expect(
            Task<TxSubmission> future, CallerInfo caller)
        {
            return await Expect(future,
                // ReSharper disable ExplicitCallerInfoArgument
                caller.MemberName,
                caller.SourceFilePath,
                caller.SourceLineNumber);
                // ReSharper restore ExplicitCallerInfoArgument
        }

        // ReSharper disable once InconsistentNaming
        protected async Task<TxSubmission> Expect(
            Task<TxSubmission> future,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
            )
        {
            return await Expect(EngineResult.tesSUCCESS, future, 
                // ReSharper disable ExplicitCallerInfoArgument
                        memberName, 
                        sourceFilePath, 
                        sourceLineNumber);
                // ReSharper restore ExplicitCallerInfoArgument
        }

        protected async Task<TxSubmission> Expect(
            EngineResult ter, 
            Task<TxSubmission> future,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {

            TxSubmission result;
            try
            {
                result = await future;
            }
            catch (InvalidOperationException e)
            {
                throw new UnexpectedEngineResultException(
                    $"In {memberName} @ {sourceFilePath}:{sourceLineNumber} you " +
                    $"expected {ter} submit response, got:\n" +
                    $"{Alias(e.Message)}");
            }

            if (ter != result.EngineResult)
            {
                throw new UnexpectedEngineResultException(
                    $"In {memberName} @ {sourceFilePath}:{sourceLineNumber} you " + 
                    $"expected {ter} submit response, got {result.EngineResult}:\n" +
                    $"{Alias(result.ResultJson.ToString())}");
            }
            // For checking the final result
            result.ExpectedFinalResult = new ResultExpectation
            {
                EngineResult = ter,
                MemberName = memberName,
                SourceFilePath = sourceFilePath,
                SourceLineNumber = sourceLineNumber
            };
            return result;
        }

        protected async Task<TxResult[]> SubmitAndClose(
            IEnumerable<Task<TxSubmission>> fut)
        {
            return await SubmitAndClose(fut.ToArray());
        }

        protected async Task<TxResult[]> SubmitAndClose(params Task<TxSubmission>[] fut)
        {
            var submissions = await Task.WhenAll(fut);
            await LedgerAccept();
            return (await Task.WhenAll(
                submissions.Select(async s =>
                {
                    try
                    {
                        return await RequestTxResult(s.Hash);
                    }
                    catch (TxNotFound e)
                    {
                        if (s.ExpectedFinalResult.EngineResult.ShouldClaimFee())
                        {
                            throw new UnexpectedEngineResultException(
                                "Transaction was expected to be in closed ledger, " +
                                "but wasn't found", e);
                        }
                        return null;
                    }
                }
            ))).Where(tx => true).ToArray();
        }

        protected TxConfigurator[] With(params TxConfigurator[] conf)
        {
            return conf;
        }

        protected static async Task Future(params Task[] tasks)
        {
            await Task.WhenAll(tasks);
        }

        protected async Task<JObject[]> GetAccountData(AccountId testAccount)
        {
            // ReSharper disable once InconsistentNaming
            const string ledger_index = "validated";
            var account = testAccount.ToString();
            var parameters = new {ledger_index, account};
            var lines = Ws.RequestAsync("account_lines", parameters);
            var info = Ws.RequestAsync("account_info", parameters);
            var offers = Ws.RequestAsync("account_offers", parameters);
            return await Task.WhenAll(info, lines, offers);
        }

    }
}
