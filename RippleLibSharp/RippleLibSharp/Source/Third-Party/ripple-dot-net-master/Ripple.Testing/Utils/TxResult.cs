using Newtonsoft.Json.Linq;
using Ripple.Core.Enums;
using Ripple.Core.Types;

namespace Ripple.Testing.Utils
{
    public class TxResult
    {
        public readonly StObject Tx;
        public readonly StObject Meta;
        public readonly EngineResult Result;
        public readonly TransactionType Type;
        public readonly Hash256 Hash;
        public readonly Uint32 TxIndex;
        public readonly Uint32 LedgerIndex;

        public TxResult(StObject tx, StObject meta, Uint32 ledgerIndex)
        {
            Tx = tx;
            Meta = meta;
            LedgerIndex = ledgerIndex;
            Hash = tx[Field.hash];
            Result = meta[Field.TransactionResult];
            TxIndex = meta[Field.TransactionIndex];
            Type = tx[Field.TransactionType];
        }

        public JObject ToJObject()
        {
            return new JObject
            {
                ["hash"] = Hash.ToJson(),
                ["ledger_index"] = LedgerIndex.ToJson(),
                ["meta"] = Meta.ToJson(),
                ["tx_json"] = Tx.ToJson(),
            };
        }


        public override string ToString()
        {
            return ToJObject().ToString();
        }

        private string SummaryString()
        {
            return $"{Result}({Type},{LedgerIndex},{TxIndex}, {Hash})";
        }
    }
}