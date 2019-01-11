namespace Ripple.Testing.Utils
{
    public class TxFlag
    {
        public const uint
            FullyCanonicalSig = 0x80000000,
            Universal = FullyCanonicalSig,
            UniversalMask = ~Universal,

            // AccountSet flags:
            RequireDestTag = 0x00010000,
            OptionalDestTag = 0x00020000,
            RequireAuth = 0x00040000,
            OptionalAuth = 0x00080000,
            DisallowXrp = 0x00100000,
            AllowXrp = 0x00200000,
            AccountSetMask = ~(Universal | RequireDestTag | OptionalDestTag
                               | RequireAuth | OptionalAuth
                               | DisallowXrp | AllowXrp),

            // AccountSet SetFlag/ClearFlag values
            AsfRequireDest = 1,
            AsfRequireAuth = 2,
            AsfDisallowXrp = 3,
            AsfDisableMaster = 4,
            AsfAccountTxnId = 5,
            AsfNoFreeze = 6,
            AsfGlobalFreeze = 7,
            AsfDefaultRipple = 8,

            // OfferCreate flags:
            Passive = 0x00010000,
            ImmediateOrCancel = 0x00020000,
            FillOrKill = 0x00040000,
            Sell = 0x00080000,
            OfferCreateMask = ~(Universal | Passive | ImmediateOrCancel | FillOrKill | Sell),

            // Payment flags:
            NoRippleDirect = 0x00010000,
            PartialPayment = 0x00020000,
            LimitQuality = 0x00040000,
            PaymentMask = ~(Universal | PartialPayment | LimitQuality | NoRippleDirect),

            // TrustSet flags:
            SetAuth = 0x00010000,
            SetNoRipple = 0x00020000,
            ClearNoRipple = 0x00040000,
            SetFreeze = 0x00100000,
            ClearFreeze = 0x00200000,
            TrustSetMask = ~(Universal | SetAuth | SetNoRipple | ClearNoRipple | SetFreeze | ClearFreeze);
    }
}
