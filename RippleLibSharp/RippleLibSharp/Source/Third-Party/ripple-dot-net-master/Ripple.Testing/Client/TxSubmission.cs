using Newtonsoft.Json.Linq;
using Ripple.Core.Types;
using Ripple.Testing.Utils;

namespace Ripple.Testing.Client
{
    public class TxSubmission
    {
        public readonly Hash256 Hash;
        public readonly EngineResult EngineResult;
        public readonly JObject TxJson;
        public readonly JObject ResultJson;

        // Testing Hacks
        public ResultExpectation ExpectedFinalResult;
        public bool ShouldClaimFee() => EngineResult.ShouldClaimFee();

        public TxSubmission(Hash256 hash,
            EngineResult engineResult,
            JObject txJson,
            JObject resultJson)
        {
            Hash = hash;
            EngineResult = engineResult;
            TxJson = txJson;
            ResultJson = resultJson;
            ExpectedFinalResult = null;
        }
    }
}
