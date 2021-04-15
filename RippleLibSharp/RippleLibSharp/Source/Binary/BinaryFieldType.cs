
using System;
using System.Linq;

namespace RippleLibSharp.Binary
{
	public sealed class BinaryFieldType : IComparable
	{
		

		public static readonly BinaryFieldType CloseResolution = 
			new BinaryFieldType (
				new BinaryType(BinaryType.UINT8), 
				0x01, 
				nameof (CloseResolution));

		public static readonly BinaryFieldType TemplateEntryType = 
			new BinaryFieldType(
				new BinaryType(BinaryType.UINT8), 
				0x02, 
				nameof(TemplateEntryType));

		public static readonly BinaryFieldType TransactionResult = new BinaryFieldType (new BinaryType(BinaryType.UINT8), 0x03, nameof(TransactionResult));

		public static readonly BinaryFieldType LedgerEntryType = new BinaryFieldType (new BinaryType(BinaryType.UINT16), 0x01, nameof (LedgerEntryType));
		public static readonly BinaryFieldType TransactionType = new BinaryFieldType (new BinaryType(BinaryType.UINT16), 0x02, nameof (TransactionType));
		public static readonly BinaryFieldType SignerWeight = new BinaryFieldType (new BinaryType(BinaryType.UINT16), 0x03, nameof (SignerWeight));

		public static readonly BinaryFieldType Flags = new BinaryFieldType (new BinaryType(BinaryType.UINT32), 0x02, nameof (Flags));
		public static readonly BinaryFieldType SourceTag = new BinaryFieldType (new BinaryType(BinaryType.UINT32), 0x03, nameof (SourceTag));
		public static readonly BinaryFieldType Sequence = new BinaryFieldType ( new BinaryType(BinaryType.UINT32), 0x04, nameof(Sequence));
		public static readonly BinaryFieldType PreviousTxnLgrSeq = new BinaryFieldType ( new BinaryType(BinaryType.UINT32), 0x05, nameof(PreviousTxnLgrSeq));
		public static readonly BinaryFieldType LedgerSequence = new BinaryFieldType ( new BinaryType(BinaryType.UINT32), 0x06, nameof (LedgerSequence));
		public static readonly BinaryFieldType CloseTime = new BinaryFieldType (new BinaryType(BinaryType.UINT32), 0x07, nameof (CloseTime));
		public static readonly BinaryFieldType ParentCloseTime = new BinaryFieldType (new BinaryType(BinaryType.UINT32), 0x08, nameof (ParentCloseTime));
		public static readonly BinaryFieldType SigningTime = new BinaryFieldType (new BinaryType(BinaryType.UINT32), 0x09, nameof (SigningTime));
		public static readonly BinaryFieldType Expiration = new BinaryFieldType (new BinaryType(BinaryType.UINT32), 0x0a, nameof (Expiration));
		public static readonly BinaryFieldType TransferRate = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x0b, nameof (TransferRate));
		public static readonly BinaryFieldType WalletSize = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x0c, nameof (WalletSize));
		public static readonly BinaryFieldType OwnerCount = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x0d, nameof (OwnerCount));
		public static readonly BinaryFieldType DestinationTag = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x0e, nameof (DestinationTag));
		// missing value 0x0f ?? doesn't exist?
		public static readonly BinaryFieldType HighQualityIn = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x10, nameof (HighQualityIn));
		public static readonly BinaryFieldType HighQualityOut = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x11, nameof (HighQualityOut));
		public static readonly BinaryFieldType LowQualityIn = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x12, nameof (LowQualityIn));
		public static readonly BinaryFieldType LowQualityOut = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x13, nameof (LowQualityOut));
		public static readonly BinaryFieldType QualityIn = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x14, nameof (QualityIn));
		public static readonly BinaryFieldType QualityOut = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x15, nameof (QualityOut));
		public static readonly BinaryFieldType StampEscrow = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x16, nameof (StampEscrow));
		public static readonly BinaryFieldType BondAmount = new BinaryFieldType ( new BinaryType(BinaryType.UINT32),0x17, nameof (BondAmount));
		public static readonly BinaryFieldType LoadFee = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x18, nameof (LoadFee));
		public static readonly BinaryFieldType OfferSequence = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x19, nameof (OfferSequence));
		public static readonly BinaryFieldType FirstLedgerSequence = new BinaryFieldType (new BinaryType(BinaryType.UINT32), 0x1a, nameof (FirstLedgerSequence));
		public static readonly BinaryFieldType LastLedgerSequence = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x1b, nameof (LastLedgerSequence));
		public static readonly BinaryFieldType TransactionIndex = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x1c, nameof (TransactionIndex));
		public static readonly BinaryFieldType OperationLimit = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x1d, nameof (OperationLimit));
		public static readonly BinaryFieldType ReferenceFeeUnits = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x1e, nameof (ReferenceFeeUnits));
		public static readonly BinaryFieldType ReserveBase = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x1f, nameof (ReserveBase));
		public static readonly BinaryFieldType ReserveIncrement = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x20, nameof (ReserveIncrement));
		public static readonly BinaryFieldType SetFlag = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x21, nameof (SetFlag));
		public static readonly BinaryFieldType ClearFlag = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x22, nameof (ClearFlag));
		public static readonly BinaryFieldType SignerQuorum = new BinaryFieldType (new BinaryType (BinaryType.UINT32), 35, nameof (SignerQuorum));
		public static readonly BinaryFieldType CancelAfter = new BinaryFieldType (new BinaryType (BinaryType.UINT32), 36, nameof (CancelAfter));
		public static readonly BinaryFieldType FinishAfter = new BinaryFieldType (new BinaryType (BinaryType.UINT32), 37, nameof (FinishAfter));
		public static readonly BinaryFieldType SignerListID = new BinaryFieldType (new BinaryType (BinaryType.UINT32), 38, nameof (SignerListID));
		public static readonly BinaryFieldType SettleDelay = new BinaryFieldType (new BinaryType (BinaryType.UINT32), 39, nameof (SettleDelay));

		public static readonly BinaryFieldType IndexNext = new BinaryFieldType (new BinaryType(BinaryType.UINT64),0x01, nameof (IndexNext));
		public static readonly BinaryFieldType IndexPrevious = new BinaryFieldType (new BinaryType(BinaryType.UINT64),0x02, nameof (IndexPrevious));
		public static readonly BinaryFieldType BookNode = new BinaryFieldType (new BinaryType(BinaryType.UINT64),0x03, nameof (BookNode));
		public static readonly BinaryFieldType OwnerNode = new BinaryFieldType (new BinaryType(BinaryType.UINT64),0x04, nameof (OwnerNode));
		public static readonly BinaryFieldType BaseFee = new BinaryFieldType (new BinaryType(BinaryType.UINT64),0x05, nameof (BaseFee));
		public static readonly BinaryFieldType ExchangeRate = new BinaryFieldType (new BinaryType(BinaryType.UINT64),0x06, nameof (ExchangeRate));
		public static readonly BinaryFieldType LowNode = new BinaryFieldType (new BinaryType(BinaryType.UINT64),0x07, nameof (LowNode));
		public static readonly BinaryFieldType HighNode = new BinaryFieldType (new BinaryType(BinaryType.UINT64),0x08, nameof(HighNode));
		public static readonly BinaryFieldType DestinationNode = new BinaryFieldType (new BinaryType (BinaryType.UINT64), 9, nameof (DestinationNode));
		public static readonly BinaryFieldType Cookie = new BinaryFieldType (new BinaryType (BinaryType.UINT64), 10, nameof (Cookie));

		public static readonly BinaryFieldType EmailHash = new BinaryFieldType (new BinaryType(BinaryType.HASH128),0x01, nameof (EmailHash));

		public static readonly BinaryFieldType TakerPaysCurrency = new BinaryFieldType (new BinaryType (BinaryType.HASH160), 0x01, nameof (TakerPaysCurrency));
		public static readonly BinaryFieldType TakerPaysIssuer = new BinaryFieldType (new BinaryType (BinaryType.HASH160), 0x02, nameof (TakerPaysIssuer));
		public static readonly BinaryFieldType TakerGetsCurrency = new BinaryFieldType (new BinaryType (BinaryType.HASH160), 0x03, nameof (TakerGetsCurrency));
		public static readonly BinaryFieldType TakerGetsIssuer = new BinaryFieldType (new BinaryType (BinaryType.HASH160), 0x04, nameof (TakerGetsIssuer));


		public static readonly BinaryFieldType LedgerHash = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x01, nameof (LedgerHash));
		public static readonly BinaryFieldType ParentHash = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x02, nameof (ParentHash));
		public static readonly BinaryFieldType TransactionHash = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x03, nameof (TransactionHash));
		public static readonly BinaryFieldType AccountHash = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x04, nameof (AccountHash));
		public static readonly BinaryFieldType PreviousTxnID = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x05, nameof (PreviousTxnID));
		public static readonly BinaryFieldType LedgerIndex = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x06, nameof (LedgerIndex));
		public static readonly BinaryFieldType WalletLocator = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x07, nameof (WalletLocator));
		public static readonly BinaryFieldType RootIndex = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x08, nameof (RootIndex));



		public static readonly BinaryFieldType BookDirectory = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x10, nameof (BookDirectory));
		public static readonly BinaryFieldType InvoiceID = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x11, nameof (InvoiceID));
		public static readonly BinaryFieldType Nickname = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x12, nameof (Nickname));

		public static readonly BinaryFieldType Amendment = new BinaryFieldType (new BinaryType (BinaryType.HASH256), 19, nameof (Amendment));
		public static readonly BinaryFieldType TicketID = new BinaryFieldType (new BinaryType (BinaryType.HASH256), 20, nameof (TicketID)); 
		public static readonly BinaryFieldType Digest = new BinaryFieldType (new BinaryType (BinaryType.HASH256), 21, nameof (Digest));
		public static readonly BinaryFieldType Channel = new BinaryFieldType (new BinaryType (BinaryType.HASH256), 22, nameof (Channel));
		public static readonly BinaryFieldType ConsensusHash = new BinaryFieldType (new BinaryType (BinaryType.HASH256), 23, nameof (ConsensusHash));
		public static readonly BinaryFieldType CheckID = new BinaryFieldType (new BinaryType (BinaryType.HASH256), 24, nameof (CheckID));
		//public static readonly BinaryFieldType Feature = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x13);



		public static readonly BinaryFieldType Amount = new BinaryFieldType (new BinaryType(BinaryType.AMOUNT),0x01, nameof (Amount));
		public static readonly BinaryFieldType Balance = new BinaryFieldType (new BinaryType(BinaryType.AMOUNT),0x02, nameof (Balance));
		public static readonly BinaryFieldType LimitAmount = new BinaryFieldType (new BinaryType(BinaryType.AMOUNT),0x03, nameof (LimitAmount));
		public static readonly BinaryFieldType TakerPays = new BinaryFieldType (new BinaryType(BinaryType.AMOUNT),0x04, nameof (TakerPays));
		public static readonly BinaryFieldType TakerGets = new BinaryFieldType (new BinaryType(BinaryType.AMOUNT),0x05, nameof (TakerGets));
		public static readonly BinaryFieldType LowLimit = new BinaryFieldType (new BinaryType(BinaryType.AMOUNT),0x06, nameof (LowLimit));
		public static readonly BinaryFieldType HighLimit = new BinaryFieldType (new BinaryType(BinaryType.AMOUNT),0x07, nameof (HighLimit));
		public static readonly BinaryFieldType Fee = new BinaryFieldType (new BinaryType(BinaryType.AMOUNT),0x08, nameof (Fee));
		public static readonly BinaryFieldType SendMax = new BinaryFieldType (new BinaryType(BinaryType.AMOUNT),0x09, nameof (SendMax));
		public static readonly BinaryFieldType DeliverMin = new BinaryFieldType (new BinaryType (BinaryType.AMOUNT), 10, nameof (DeliverMin));


		public static readonly BinaryFieldType MinimumOffer = new BinaryFieldType (new BinaryType(BinaryType.AMOUNT),0x10, nameof (MinimumOffer));
		public static readonly BinaryFieldType RippleEscrow = new BinaryFieldType (new BinaryType(BinaryType.AMOUNT),0x11, nameof (RippleEscrow));
		public static readonly BinaryFieldType DeliveredAmount = new BinaryFieldType (new BinaryType (BinaryType.AMOUNT), 18, nameof (DeliveredAmount));

		public static readonly BinaryFieldType PublicKey = new BinaryFieldType (new BinaryType(BinaryType.VARIABLE_LENGTH),0x01, nameof (PublicKey));
		public static readonly BinaryFieldType SigningPubKey = new BinaryFieldType (new BinaryType (BinaryType.VARIABLE_LENGTH), 0x03, nameof (SigningPubKey));
		// TODO Signature
		public static readonly BinaryFieldType MessageKey = new BinaryFieldType (new BinaryType(BinaryType.VARIABLE_LENGTH),0x02, nameof (MessageKey));

		public static readonly BinaryFieldType TxnSignature = new BinaryFieldType (new BinaryType(BinaryType.VARIABLE_LENGTH),0x04, nameof (TxnSignature));
		public static readonly BinaryFieldType Generator = new BinaryFieldType (new BinaryType(BinaryType.VARIABLE_LENGTH),0x05, nameof (Generator));
		public static readonly BinaryFieldType Signature = new BinaryFieldType (new BinaryType(BinaryType.VARIABLE_LENGTH),0x06, nameof (Signature));
		public static readonly BinaryFieldType Domain = new BinaryFieldType (new BinaryType(BinaryType.VARIABLE_LENGTH),0x07, nameof (Domain));
		public static readonly BinaryFieldType FundCode = new BinaryFieldType (new BinaryType(BinaryType.VARIABLE_LENGTH),0x08, nameof (FundCode));
		public static readonly BinaryFieldType RemoveCode = new BinaryFieldType (new BinaryType(BinaryType.VARIABLE_LENGTH),0x09, nameof (RemoveCode));
		public static readonly BinaryFieldType ExpireCode = new BinaryFieldType (new BinaryType(BinaryType.VARIABLE_LENGTH),0x0a, nameof (ExpireCode));
		public static readonly BinaryFieldType CreateCode = new BinaryFieldType (new BinaryType(BinaryType.VARIABLE_LENGTH),0x0b, nameof (CreateCode));

		public static readonly BinaryFieldType MemoType = new BinaryFieldType ( new BinaryType (BinaryType.VARIABLE_LENGTH), 12, nameof (MemoType));
		public static readonly BinaryFieldType MemoData = new BinaryFieldType (new BinaryType (BinaryType.VARIABLE_LENGTH), 13, nameof (MemoData));
		public static readonly BinaryFieldType MemoFormat = new BinaryFieldType (new BinaryType (BinaryType.VARIABLE_LENGTH), 14, nameof (MemoFormat));

		public static readonly BinaryFieldType Fulfillment = new BinaryFieldType (new BinaryType (BinaryType.VARIABLE_LENGTH), 16, nameof (Fulfillment));
		public static readonly BinaryFieldType Condition = new BinaryFieldType (new BinaryType (BinaryType.VARIABLE_LENGTH), 17, nameof (Condition));
		//public static readonly BinaryFieldType MasterSignature = new BinaryFieldType (new BinaryType (BinaryType.VARIABLE_LENGTH), 18);

		public static readonly BinaryFieldType Account = new BinaryFieldType (new BinaryType(BinaryType.ACCOUNT),0x01, nameof (Account));
		public static readonly BinaryFieldType Owner = new BinaryFieldType (new BinaryType(BinaryType.ACCOUNT),0x02, nameof (Owner));
		public static readonly BinaryFieldType Destination = new BinaryFieldType (new BinaryType(BinaryType.ACCOUNT),0x03, nameof (Destination));
		public static readonly BinaryFieldType Issuer = new BinaryFieldType (new BinaryType(BinaryType.ACCOUNT),0x04, nameof (Issuer));
		public static readonly BinaryFieldType Target = new BinaryFieldType (new BinaryType(BinaryType.ACCOUNT),0x07, nameof (Target));
		public static readonly BinaryFieldType RegularKey = new BinaryFieldType (new BinaryType(BinaryType.ACCOUNT),0x08, nameof (RegularKey));

		public static readonly BinaryFieldType Paths = new BinaryFieldType (new BinaryType(BinaryType.PATHSET), 0x01, nameof (Paths));


		public static readonly BinaryFieldType Indexes = new BinaryFieldType (new BinaryType(BinaryType.VECTOR256),0x01, nameof (Indexes));
		public static readonly BinaryFieldType Hashes = new BinaryFieldType (new BinaryType(BinaryType.VECTOR256),0x02, nameof (Hashes));
		public static readonly BinaryFieldType Features = new BinaryFieldType (new BinaryType(BinaryType.VECTOR256),0x03, nameof (Features));


		public static readonly BinaryFieldType TransactionMetaData = new BinaryFieldType (new BinaryType(BinaryType.OBJECT), 0x02, nameof (TransactionMetaData));
		public static readonly BinaryFieldType CreatedNode = new BinaryFieldType (new BinaryType(BinaryType.OBJECT), 0x03, nameof (CreatedNode));
		public static readonly BinaryFieldType DeletedNode = new BinaryFieldType (new BinaryType(BinaryType.OBJECT), 0x04, nameof (DeletedNode));
		public static readonly BinaryFieldType ModifiedNode = new BinaryFieldType (new BinaryType(BinaryType.OBJECT), 0x05, nameof (ModifiedNode));
		public static readonly BinaryFieldType PreviousFields = new BinaryFieldType(new BinaryType(BinaryType.OBJECT), 0x06, nameof (PreviousFields));
		public static readonly BinaryFieldType FinalFields = new BinaryFieldType (new BinaryType(BinaryType.OBJECT), 0x07, nameof (FinalFields));
		public static readonly BinaryFieldType NewFields = new BinaryFieldType (new BinaryType(BinaryType.OBJECT), 0x08, nameof (NewFields));
		public static readonly BinaryFieldType TemplateEntry= new BinaryFieldType (new BinaryType(BinaryType.OBJECT), 0x09, nameof (TemplateEntry));
		public static readonly BinaryFieldType Memo = new BinaryFieldType (new BinaryType (BinaryType.OBJECT), 10, nameof (Memo));
		public static readonly BinaryFieldType SignerEntry = new BinaryFieldType (new BinaryType (BinaryType.OBJECT), 11, nameof (SignerEntry));

		public static readonly BinaryFieldType SigningAccounts= new BinaryFieldType (new BinaryType(BinaryType.ARRAY), 0x02, nameof (SigningAccounts));
		public static readonly BinaryFieldType TxnSignatures= new BinaryFieldType (new BinaryType(BinaryType.ARRAY), 0x03, nameof (TxnSignatures));
		public static readonly BinaryFieldType Signatures= new BinaryFieldType (new BinaryType(BinaryType.ARRAY), 0x04, nameof (Signatures));
		public static readonly BinaryFieldType Template= new BinaryFieldType (new BinaryType(BinaryType.ARRAY), 0x05, nameof (Template));
		public static readonly BinaryFieldType Necessary= new BinaryFieldType (new BinaryType(BinaryType.ARRAY), 0x06, nameof (Necessary));
		public static readonly BinaryFieldType Sufficient= new BinaryFieldType (new BinaryType(BinaryType.ARRAY), 0x07, nameof (Sufficient));
      		public static readonly BinaryFieldType AffectedNodes= new BinaryFieldType (new BinaryType(BinaryType.ARRAY), 0x08, nameof (AffectedNodes));
		public static readonly BinaryFieldType Memos = new BinaryFieldType (new BinaryType (BinaryType.ARRAY), 0x09, nameof (Memos));

		public static BinaryFieldType[] GetValues ()
		{

			if (valuesCache == null) {
				valuesCache = new BinaryFieldType[] {

				// note this is the Enumarable order
				CloseResolution,
				TemplateEntryType,
				TransactionResult,

				LedgerEntryType,
				TransactionType,

				Flags,
				SourceTag,
				Sequence,
				PreviousTxnLgrSeq,
				LedgerSequence,
				CloseTime,
				ParentCloseTime,
				SigningTime,
				Expiration,
				TransferRate,
				WalletSize,
				OwnerCount,
				DestinationTag,

				HighQualityIn,
				HighQualityOut,
				LowQualityIn,
				LowQualityOut,
				QualityIn,
				QualityOut,
				StampEscrow,
				BondAmount,
				LoadFee,
				OfferSequence,
				FirstLedgerSequence,
				LastLedgerSequence,
				TransactionIndex,
				OperationLimit,
				ReferenceFeeUnits,
				ReserveBase,
				ReserveIncrement,
				SetFlag,
				ClearFlag,

				IndexNext,
				IndexPrevious,
				BookNode,
				OwnerNode,
				BaseFee,
				ExchangeRate,
				LowNode,
				HighNode,
				EmailHash,
				LedgerHash,
				ParentHash,
				TransactionHash,
				AccountHash,
				PreviousTxnID,
				LedgerIndex,
				WalletLocator,
				RootIndex ,
				BookDirectory,
				InvoiceID ,
				Nickname,
				//Feature,


				TakerPaysCurrency,
				TakerPaysIssuer,
				TakerGetsCurrency,
				TakerGetsIssuer,

				Amount,
				Balance,
				LimitAmount,
				TakerPays,
				TakerGets,
				LowLimit,
				HighLimit,
				Fee,
				SendMax,
				MinimumOffer,
				RippleEscrow,

				PublicKey,
				MessageKey,
				SigningPubKey,
				TxnSignature,
				Generator,
				Signature,
				Domain,
				FundCode,
				RemoveCode,
				ExpireCode,
				CreateCode,

				Account,
				Owner,
				Destination,
				Issuer,
				Target,
				RegularKey,

				Paths,


				Indexes,
				Hashes,
				Features,


				TransactionMetaData,
				CreatedNode,
				DeletedNode,
				ModifiedNode,
				PreviousFields,
				FinalFields,
				NewFields,
				TemplateEntry,

				SigningAccounts,
				TxnSignatures,
				Signatures,
				Template,
				Necessary,
				Sufficient,
      				AffectedNodes,
		    		Memos
			};
			}

			return valuesCache;

		}

		public static String StringFromType (BinaryFieldType typ)
		{

			return typ.name;

			/*
			if (typ == CloseResolution) {
				return nameof(CloseResolution);
			}

			if (typ == TemplateEntryType) {
				return nameof(TemplateEntryType);
			}

			if (typ == TransactionResult) {
				return nameof (TransactionResult);
			}

			if (typ == LedgerEntryType) {
				return nameof(LedgerEntryType);
			}

			if (typ == TransactionType) {
				return nameof(TransactionType);
			}

			if (typ == Flags) {
				return nameof(Flags);
			}

			if (typ == SourceTag) {
				return nameof(SourceTag);
			}

			if (typ == Sequence) {
				return nameof(Sequence);
			}
				
			if (typ == PreviousTxnLgrSeq) {
				return nameof(PreviousTxnLgrSeq);
			}

			if (typ == LedgerSequence) {
				return nameof(LedgerSequence);
			}

			if (typ == CloseTime) {
				return nameof (CloseTime);
			}

			if (typ == ParentCloseTime) {
				return nameof(ParentCloseTime);
			}

			if (typ == SigningTime) {
				return nameof(SigningTime);
			}

			if (typ == Expiration) {
				return nameof(Expiration);
			}

			if (typ == TransferRate) {
				return nameof(TransferRate);
			}

			if (typ == WalletSize) {
				return nameof(WalletSize);
			}

			if (typ == OwnerCount) {
				return nameof(OwnerCount);
			}

			if (typ == DestinationTag) {
				return nameof(DestinationTag);
			}

			if (typ == HighQualityIn) {
				return nameof (HighQualityIn);
			}

			if (typ == HighQualityOut) {
				return nameof(HighQualityOut);
			}

			if (typ == LowQualityIn) {
				return nameof(LowQualityIn);
			}

			if (typ == LowQualityOut) {
				return nameof(LowQualityOut);
			}

			if (typ == QualityIn) {
				return nameof(QualityIn);
			}

			if (typ == QualityOut) {
				return nameof(QualityOut);
			}

			if (typ == StampEscrow) {
				return nameof(StampEscrow);
			}

			if (typ == BondAmount) {
				return nameof(BondAmount);
			}

			if (typ == LoadFee) {
				return nameof(LoadFee);
			}

			if (typ == OfferSequence) {
				return nameof(OfferSequence);
			}

			if (typ == FirstLedgerSequence) {
				return nameof(FirstLedgerSequence);
			}

			if (typ == LastLedgerSequence) {
				return nameof(LastLedgerSequence);
			}

			if (typ == TransactionIndex) {
				return nameof(TransactionIndex);
			}

			if (typ == OperationLimit) {
				return nameof(OperationLimit);
			}

			if (typ == ReferenceFeeUnits) {
				return nameof(ReferenceFeeUnits);
			}

			if (typ == ReserveBase) {
				return nameof(ReserveBase);
			}

			if (typ == ReserveIncrement) {
				return nameof(ReserveIncrement);
			}

			if (typ == SetFlag) {
				return nameof(SetFlag);
			}

			if (typ == ClearFlag) {
				return nameof(ClearFlag);
			}

			if (typ == IndexNext) {
				return nameof(IndexNext);
			}

			if (typ == IndexPrevious) {
				return nameof(IndexPrevious);
			}

			if (typ == BookNode) {
				return nameof(BookNode);
			}

			if (typ == OwnerNode) {
				return nameof(OwnerNode);
			}

			if (typ == BaseFee) {
				return nameof(BaseFee);
			}

			if (typ ==ExchangeRate) {
				return nameof(ExchangeRate);
			}

			if (typ == LowNode) {
				return nameof(LowNode);
			}

			if (typ == HighNode) {
				return nameof(HighNode);
			}

			if (typ == EmailHash) {
				return nameof(EmailHash);
			}

			if (typ == LedgerHash) {
				return nameof(LedgerHash);
			}

			if (typ == ParentHash) {
				return nameof(ParentHash);
			}

			if (typ == TransactionHash) {
				return nameof(TransactionHash);
			}

			if (typ == AccountHash) {
				return nameof(AccountHash);
			}

			if (typ == PreviousTxnID) {
				return nameof(PreviousTxnID);
			}

			if (typ == LedgerIndex) {
				return nameof(LedgerIndex);
			}

			if (typ == WalletLocator) {
				return nameof(WalletLocator);
			}

			if (typ == RootIndex) {
				return nameof(RootIndex);
			}

			if (typ == BookDirectory) {
				return nameof(BookDirectory);
			}

			if (typ == InvoiceID) {
				return nameof(InvoiceID);
			}

			if (typ == Nickname) {
				return nameof(Nickname);
			}

			///*
			///if (typ == Feature) {
				///return "Feature";
			///}
			///

			if (typ == TakerPaysCurrency) {
				return nameof(TakerPaysCurrency);
			}

			if (typ == TakerPaysIssuer) {
				return nameof(TakerPaysIssuer);
			}

			if (typ == TakerGetsCurrency) {
				return nameof(TakerGetsCurrency);
			}

			if (typ == TakerGetsIssuer) {
				return nameof(TakerGetsIssuer);
			}

			if (typ == Amount) {
				return nameof(Amount);
			}

			if (typ == Balance) {
				return nameof(Balance);
			}

			if (typ == LimitAmount) {
				return nameof(LimitAmount);
			}

			if (typ == TakerPays) {
				return nameof(TakerPays);
			}

			if (typ == TakerGets) {
				return nameof(TakerGets);
			}

			if (typ == LowLimit) {
				return nameof(LowLimit);
			}

			if (typ == HighLimit) {
				return nameof(HighLimit);
			}

			if (typ == Fee) {
				return nameof(Fee);
			}

			if (typ == SendMax) {
				return nameof(SendMax);
			}

			if (typ == MinimumOffer) {
				return nameof(MinimumOffer);
			}

			if (typ == RippleEscrow) {
				return nameof(RippleEscrow);
			}

			if (typ == PublicKey) {
				return nameof(PublicKey);
			}

			if (typ == MessageKey) {
				return nameof(MessageKey);
			}

			if (typ == SigningPubKey) {
				return nameof(SigningPubKey);
			}

			if (typ == TxnSignature) {
				return nameof(TxnSignature);
			}

			if (typ == Generator) {
				return nameof(Generator);
			}

			if (typ == Signature) {
				return nameof(Signature);
			}

			if (typ == Domain) {
				return nameof(Domain);
			}

			if (typ == FundCode) {
				return nameof(FundCode);
			}

			if (typ == RemoveCode) {
				return nameof(RemoveCode);
			}

			if (typ == ExpireCode) {
				return nameof(ExpireCode);
			}

			if (typ == CreateCode) {
				return nameof(CreateCode);
			}

			if (typ == Account) {
				return nameof(Account);
			}

			if (typ == Owner) {
				return nameof(Owner);
			}

			if (typ == Destination) {
				return nameof(Destination);
			}

			if (typ == Issuer) {
				return nameof(Issuer);
			}

			if (typ == Target) {
				return nameof(Target);
			}

			if (typ == RegularKey) {
				return nameof(RegularKey);
			}

			if (typ == Paths) {
				return nameof(Paths);
			}

			if (typ == Indexes) {
				return nameof(Indexes);
			}

			if (typ == Hashes) {
				return nameof(Hashes);
			}

			if (typ == Features) {
				return nameof(Features);
			}

			if (typ == TransactionMetaData) {
				return nameof(TransactionMetaData);
			}

			if (typ == CreatedNode) {
				return nameof(CreatedNode); 
			}

			if (typ == DeletedNode) {
				return nameof(DeletedNode);
			}

			if (typ == ModifiedNode) {
				return nameof(ModifiedNode);
			}

			if (typ == PreviousFields) {
				return nameof(PreviousFields);
			}

			if (typ == FinalFields) {
				return nameof(FinalFields);
			}

			if (typ == NewFields) {
				return nameof(NewFields);
			}

			if (typ == TemplateEntry) {
				return nameof(TemplateEntry);
			}

			if (typ == SigningAccounts) {
				return nameof(SigningAccounts);
			}

			if (typ == TxnSignatures) {
				return nameof(TxnSignatures);
			}

			if (typ == Signatures) {
				return nameof(Signatures);
			}

			if (typ == Template) {
				return nameof(Template);
			}

			if (typ == Necessary) {
				return nameof (Necessary);
			}

			if (typ == Sufficient) {
				return nameof (Sufficient);
			}

			if (typ == AffectedNodes) {
				return nameof(AffectedNodes);
			}

			if (typ == Memos) {
				return nameof (Memos);
			}

			if (typ == Memo) {
				return nameof (Memo);

			}




			throw new ArgumentException(
				"No String value found for BinaryFieldType of type : " + 
				typ.type.ToString() + 
				" and Value : " + 
				typ.value.ToString());

			*/
		}


#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private static BinaryFieldType [] valuesCache = null;
		static BinaryFieldType[,] typeFieldLookup = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		static BinaryFieldType ()
		{
			/*
			foreach (BinaryFieldType f in GetValues()) {
				MAXBYTEVALUE = Math.Max(MAXBYTEVALUE, f.value);
			}
			*/    

	    		MAXBYTEVALUE = GetValues ().Max((arg) => arg.value);

			MAXBYTEVALUE++;

			BinaryFieldType.typeFieldLookup = new BinaryFieldType[BinaryType.MAXBYTEVALUE,BinaryFieldType.MAXBYTEVALUE]; // note the two MAXBYTEVALUES are different 

			foreach (BinaryFieldType f in BinaryFieldType.GetValues()) {
				typeFieldLookup[f.type.typeCode,f.value] = f;
			}
		}

		private BinaryFieldType (BinaryType type, byte value, string name)
		{
			this.type = type;
			this.value = value;
			this.name = name;
		}

		public readonly BinaryType type;
		public readonly byte value;
		public readonly string name;


		//int fieldID;

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		static private byte MAXBYTEVALUE=0;
#pragma warning restore RECS0122 // Initializing field with default value is redundant



		public static BinaryFieldType Lookup (int type, int fieldType)
		{
			BinaryFieldType returnMe = null;
			if (type < 0 || type >= MAXBYTEVALUE) {
				returnMe = null;
			} else if (fieldType < 0 || fieldType >= MAXBYTEVALUE) {
				returnMe = null;
			}
			else {
				returnMe = typeFieldLookup[type,fieldType];
			}


			if (returnMe == null) {
				throw new MissingFieldException("Could not find type "+type+", field "+fieldType);
			}

			return returnMe;
		}

		public override bool Equals (Object obj)
		{
			if (obj == null) {
				return false;
			}

			BinaryFieldType bft = obj as BinaryFieldType;
			if (bft == null) {
				return false;
			}

			return this.Equals((BinaryFieldType)bft);
		}

		public bool Equals (BinaryFieldType bft)
		{
			if (bft == null) {
				return false;
			}

			return (
				(bft.type.Equals(this.type)) && 
				(bft.value.Equals(this.value)));
		}

		public static bool operator ==(BinaryFieldType left, BinaryFieldType right)
		{
			if (left is null)
				return right is null;

			return left.Equals(right);
		}
 
		public static bool operator !=(BinaryFieldType left, BinaryFieldType right)
		{
			if (left is null)
				return !(right is null);

			return !left.Equals(right);
		}

		public override int GetHashCode () {
			
			int v = this.value;
			int t = this.type.typeCode;

			return (t * 256) + v;
		}

		public int CompareTo (object obj)
		{


			if (obj == null) {
				throw new ArgumentNullException ();
			}

			BinaryFieldType bft = obj as BinaryFieldType;

			if (bft != null) {
				throw new ArgumentException (
					"Can not compare BinaryFieldType to unknown type "
					+ (obj?.GetType ()?.ToString () ?? "null") );
			}


			if (
				this.type == bft.type && 
				this.value == bft.value
			) {
				return 0;
			}

			foreach (BinaryFieldType b in GetValues ()) {
				if (
					b.type == this.type && 
					b.value == this.value
		    		) {
					return -1;
				}

				if (
					bft.type == b.type && 
					bft.value == this.value
		    		) {
					return 1;
				}
			}



			throw new ArgumentException("Unknown error comparing BinaryFieldType. Report this as a bug");
		}

		public override string ToString ()
		{
			return StringFromType(this);
		}
	}
}

