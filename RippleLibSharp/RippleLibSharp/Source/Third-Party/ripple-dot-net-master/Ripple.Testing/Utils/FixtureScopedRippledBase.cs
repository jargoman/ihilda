using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using log4net;
using Ripple.Core.Types;

namespace Ripple.Testing.Utils
{
    public abstract class FixtureScopedRippledBase : RippledFixtureBase
    {
        protected int Test = 0;
        protected bool Failed;
        protected PartitionedTests PartitionedTests;

        public void SetUpTest()
        {
            MultiThreadedFailures.SetUp();
            PartitionedTests.PreTest(TestHelper().CurrentTestName(), anyPriors =>
            {
                anyPriors.ForEach(p =>
                {
                    var task = (p.Invoke(this, null) as Task);
                    if (task != null)
                    {
                        // This gives us much nicer exception tracebacks
                        // then just using Wait()
                        TestHelper().RunAsyncAction(async delegate { await task; });
                    }
                });

            });
        }

        public void TearDownTest()
        {
            PartitionedTests.PostTest(TestHelper().DidTestFail());
            MultiThreadedFailures.TearDown();
        }

        public void SetUpFixture()
        {
            MultiThreadedFailures.EnableForBlock(() =>
            {
                SetupRippled();
                PartitionedTests = PartitionedTests.Create(GetType(), 
                                        TestHelper().TestAttributeType());
            });
        }

        public void TearDownFixture()
        {
            MultiThreadedFailures.EnableForBlock(TearDownRippled);
        }
    }
}