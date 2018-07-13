
using System;
using System.Collections;
using RippleLibSharp.Util;

namespace RippleLibSharp.Binary
{
	public sealed class BinaryFieldType : IComparable
	{
		

		public static readonly BinaryFieldType CloseResolution = new BinaryFieldType (new BinaryType(BinaryType.UINT8), 0x01);
		public static readonly BinaryFieldType TemplateEntryType = new BinaryFieldType(new BinaryType(BinaryType.UINT8), 0x02);
		public static readonly BinaryFieldType TransactionResult = new BinaryFieldType (new BinaryType(BinaryType.UINT8), 0x03);

		public static readonly BinaryFieldType LedgerEntryType = new BinaryFieldType (new BinaryType(BinaryType.UINT16), 0x01);
		public static readonly BinaryFieldType TransactionType = new BinaryFieldType (new BinaryType(BinaryType.UINT16), 0x02);

		public static readonly BinaryFieldType Flags = new BinaryFieldType (new BinaryType(BinaryType.UINT32), 0x02);
		public static readonly BinaryFieldType SourceTag = new BinaryFieldType (new BinaryType(BinaryType.UINT32), 0x03);
		public static readonly BinaryFieldType Sequence = new BinaryFieldType ( new BinaryType(BinaryType.UINT32), 0x04);
		public static readonly BinaryFieldType PreviousTxnLgrSeq = new BinaryFieldType ( new BinaryType(BinaryType.UINT32), 0x05);
		public static readonly BinaryFieldType LedgerSequence = new BinaryFieldType ( new BinaryType(BinaryType.UINT32), 0x06);
		public static readonly BinaryFieldType CloseTime = new BinaryFieldType (new BinaryType(BinaryType.UINT32), 0x07);
		public static readonly BinaryFieldType ParentCloseTime = new BinaryFieldType (new BinaryType(BinaryType.UINT32), 0x08);
		public static readonly BinaryFieldType SigningTime = new BinaryFieldType (new BinaryType(BinaryType.UINT32), 0x09);
		public static readonly BinaryFieldType Expiration = new BinaryFieldType (new BinaryType(BinaryType.UINT32), 0x0a);
		public static readonly BinaryFieldType TransferRate = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x0b);
		public static readonly BinaryFieldType WalletSize = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x0c);
		public static readonly BinaryFieldType OwnerCount = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x0d);
		public static readonly BinaryFieldType DestinationTag = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x0e);
		// missing value 0x0f ?? doesn't exist?
		public static readonly BinaryFieldType HighQualityIn = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x10);
		public static readonly BinaryFieldType HighQualityOut = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x11);
		public static readonly BinaryFieldType LowQualityIn = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x12);
		public static readonly BinaryFieldType LowQualityOut = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x13);
		public static readonly BinaryFieldType QualityIn = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x14);
		public static readonly BinaryFieldType QualityOut = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x15);
		public static readonly BinaryFieldType StampEscrow = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x16);
		public static readonly BinaryFieldType BondAmount = new BinaryFieldType ( new BinaryType(BinaryType.UINT32),0x17);
		public static readonly BinaryFieldType LoadFee = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x18);
		public static readonly BinaryFieldType OfferSequence = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x19);
		public static readonly BinaryFieldType FirstLedgerSequence = new BinaryFieldType (new BinaryType(BinaryType.UINT32), 0x1a);
		public static readonly BinaryFieldType LastLedgerSequence = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x1b);
		public static readonly BinaryFieldType TransactionIndex = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x1c);
		public static readonly BinaryFieldType OperationLimit = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x1d);
		public static readonly BinaryFieldType ReferenceFeeUnits = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x1e);
		public static readonly BinaryFieldType ReserveBase = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x1f);
		public static readonly BinaryFieldType ReserveIncrement = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x20);
		public static readonly BinaryFieldType SetFlag = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x21);
		public static readonly BinaryFieldType ClearFlag = new BinaryFieldType (new BinaryType(BinaryType.UINT32),0x22);

		public static readonly BinaryFieldType IndexNext = new BinaryFieldType (new BinaryType(BinaryType.UINT64),0x01);
		public static readonly BinaryFieldType IndexPrevious = new BinaryFieldType (new BinaryType(BinaryType.UINT64),0x02);
		public static readonly BinaryFieldType BookNode = new BinaryFieldType (new BinaryType(BinaryType.UINT64),0x03);
		public static readonly BinaryFieldType OwnerNode = new BinaryFieldType (new BinaryType(BinaryType.UINT64),0x04);
		public static readonly BinaryFieldType BaseFee = new BinaryFieldType (new BinaryType(BinaryType.UINT64),0x05);
		public static readonly BinaryFieldType ExchangeRate = new BinaryFieldType (new BinaryType(BinaryType.UINT64),0x06);
		public static readonly BinaryFieldType LowNode = new BinaryFieldType (new BinaryType(BinaryType.UINT64),0x07);
		public static readonly BinaryFieldType HighNode = new BinaryFieldType (new BinaryType(BinaryType.UINT64),0x08);


		public static readonly BinaryFieldType EmailHash = new BinaryFieldType (new BinaryType(BinaryType.HASH128),0x01);


		public static readonly BinaryFieldType LedgerHash = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x01);
		public static readonly BinaryFieldType ParentHash = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x02);
		public static readonly BinaryFieldType TransactionHash = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x03);
		public static readonly BinaryFieldType AccountHash = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x04);
		public static readonly BinaryFieldType PreviousTxnID = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x05);
		public static readonly BinaryFieldType LedgerIndex = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x06);
		public static readonly BinaryFieldType WalletLocator = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x07);
		public static readonly BinaryFieldType RootIndex = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x08);
		public static readonly BinaryFieldType BookDirectory = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x10);
		public static readonly BinaryFieldType InvoiceID = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x11);
		public static readonly BinaryFieldType Nickname = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x12);
		public static readonly BinaryFieldType Feature = new BinaryFieldType (new BinaryType(BinaryType.HASH256),0x13);


		public static readonly BinaryFieldType TakerPaysCurrency = new BinaryFieldType (new BinaryType(BinaryType.HASH160),0x01);
		public static readonly BinaryFieldType TakerPaysIssuer = new BinaryFieldType (new BinaryType(BinaryType.HASH160),0x02);
		public static readonly BinaryFieldType TakerGetsCurrency = new BinaryFieldType (new BinaryType(BinaryType.HASH160),0x03);
		public static readonly BinaryFieldType TakerGetsIssuer = new BinaryFieldType (new BinaryType(BinaryType.HASH160),0x04);

		public static readonly BinaryFieldType Amount = new BinaryFieldType (new BinaryType(BinaryType.AMOUNT),0x01);
		public static readonly BinaryFieldType Balance = new BinaryFieldType (new BinaryType(BinaryType.AMOUNT),0x02);
		public static readonly BinaryFieldType LimitAmount = new BinaryFieldType (new BinaryType(BinaryType.AMOUNT),0x03);
		public static readonly BinaryFieldType TakerPays = new BinaryFieldType (new BinaryType(BinaryType.AMOUNT),0x04);
		public static readonly BinaryFieldType TakerGets = new BinaryFieldType (new BinaryType(BinaryType.AMOUNT),0x05);
		public static readonly BinaryFieldType LowLimit = new BinaryFieldType (new BinaryType(BinaryType.AMOUNT),0x06);
		public static readonly BinaryFieldType HighLimit = new BinaryFieldType (new BinaryType(BinaryType.AMOUNT),0x07);
		public static readonly BinaryFieldType Fee = new BinaryFieldType (new BinaryType(BinaryType.AMOUNT),0x08);
		public static readonly BinaryFieldType SendMax = new BinaryFieldType (new BinaryType(BinaryType.AMOUNT),0x09);
		public static readonly BinaryFieldType MinimumOffer = new BinaryFieldType (new BinaryType(BinaryType.AMOUNT),0x10);
		public static readonly BinaryFieldType RippleEscrow = new BinaryFieldType (new BinaryType(BinaryType.AMOUNT),0x11);

		public static readonly BinaryFieldType PublicKey = new BinaryFieldType (new BinaryType(BinaryType.VARIABLE_LENGTH),0x01);
		public static readonly BinaryFieldType MessageKey = new BinaryFieldType (new BinaryType(BinaryType.VARIABLE_LENGTH),0x02);
		public static readonly BinaryFieldType SigningPubKey = new BinaryFieldType (new BinaryType(BinaryType.VARIABLE_LENGTH),0x03);
		public static readonly BinaryFieldType TxnSignature = new BinaryFieldType (new BinaryType(BinaryType.VARIABLE_LENGTH),0x04);
		public static readonly BinaryFieldType Generator = new BinaryFieldType (new BinaryType(BinaryType.VARIABLE_LENGTH),0x05);
		public static readonly BinaryFieldType Signature = new BinaryFieldType (new BinaryType(BinaryType.VARIABLE_LENGTH),0x06);
		public static readonly BinaryFieldType Domain = new BinaryFieldType (new BinaryType(BinaryType.VARIABLE_LENGTH),0x07);
		public static readonly BinaryFieldType FundCode = new BinaryFieldType (new BinaryType(BinaryType.VARIABLE_LENGTH),0x08);
		public static readonly BinaryFieldType RemoveCode = new BinaryFieldType (new BinaryType(BinaryType.VARIABLE_LENGTH),0x09);
		public static readonly BinaryFieldType ExpireCode = new BinaryFieldType (new BinaryType(BinaryType.VARIABLE_LENGTH),0x0a);
		public static readonly BinaryFieldType CreateCode = new BinaryFieldType (new BinaryType(BinaryType.VARIABLE_LENGTH),0x0b);

		public static readonly BinaryFieldType Account = new BinaryFieldType (new BinaryType(BinaryType.ACCOUNT),0x01);
		public static readonly BinaryFieldType Owner = new BinaryFieldType (new BinaryType(BinaryType.ACCOUNT),0x02);
		public static readonly BinaryFieldType Destination = new BinaryFieldType (new BinaryType(BinaryType.ACCOUNT),0x03);
		public static readonly BinaryFieldType Issuer = new BinaryFieldType (new BinaryType(BinaryType.ACCOUNT),0x04);
		public static readonly BinaryFieldType Target = new BinaryFieldType (new BinaryType(BinaryType.ACCOUNT),0x07);
		public static readonly BinaryFieldType RegularKey = new BinaryFieldType (new BinaryType(BinaryType.ACCOUNT),0x08);

		public static readonly BinaryFieldType Paths = new BinaryFieldType (new BinaryType(BinaryType.PATHSET), 0x01);


		public static readonly BinaryFieldType Indexes = new BinaryFieldType (new BinaryType(BinaryType.VECTOR256),0x01);
		public static readonly BinaryFieldType Hashes = new BinaryFieldType (new BinaryType(BinaryType.VECTOR256),0x02);
		public static readonly BinaryFieldType Features = new BinaryFieldType (new BinaryType(BinaryType.VECTOR256),0x03);


		public static readonly BinaryFieldType TransactionMetaData = new BinaryFieldType (new BinaryType(BinaryType.OBJECT), 0x02);
		public static readonly BinaryFieldType CreatedNode = new BinaryFieldType (new BinaryType(BinaryType.OBJECT), 0x03);
		public static readonly BinaryFieldType DeletedNode = new BinaryFieldType (new BinaryType(BinaryType.OBJECT), 0x04);
		public static readonly BinaryFieldType ModifiedNode = new BinaryFieldType (new BinaryType(BinaryType.OBJECT), 0x05);
		public static readonly BinaryFieldType PreviousFields = new BinaryFieldType(new BinaryType(BinaryType.OBJECT), 0x06);
		public static readonly BinaryFieldType FinalFields = new BinaryFieldType (new BinaryType(BinaryType.OBJECT), 0x07);
		public static readonly BinaryFieldType NewFields = new BinaryFieldType (new BinaryType(BinaryType.OBJECT), 0x08);
		public static readonly BinaryFieldType TemplateEntry= new BinaryFieldType (new BinaryType(BinaryType.OBJECT), 0x09);

		public static readonly BinaryFieldType SigningAccounts= new BinaryFieldType (new BinaryType(BinaryType.ARRAY), 0x02);
		public static readonly BinaryFieldType TxnSignatures= new BinaryFieldType (new BinaryType(BinaryType.ARRAY), 0x03);
		public static readonly BinaryFieldType Signatures= new BinaryFieldType (new BinaryType(BinaryType.ARRAY), 0x04);
		public static readonly BinaryFieldType Template= new BinaryFieldType (new BinaryType(BinaryType.ARRAY), 0x05);
		public static readonly BinaryFieldType Necessary= new BinaryFieldType (new BinaryType(BinaryType.ARRAY), 0x06);
		public static readonly BinaryFieldType Sufficient= new BinaryFieldType (new BinaryType(BinaryType.ARRAY), 0x07);
        public static readonly BinaryFieldType AffectedNodes= new BinaryFieldType (new BinaryType(BinaryType.ARRAY), 0x08);


		public static BinaryFieldType[] GetValues ()
		{

			if (valuesCache == null) {
				valuesCache = new BinaryFieldType[] {

				// poor mans ENUM object // note this is the Enumarable order
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
				Feature,


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
      			AffectedNodes
			};
			}

			return valuesCache;

		}

		public static String StringFromType (BinaryFieldType typ)
		{

			if (typ == CloseResolution) {
				return "CloseResolution";
			}

			if (typ == TemplateEntryType) {
				return "TemplateEntryType";
			}

			if (typ == TransactionResult) {
				return "TransactionResult";
			}

			if (typ == LedgerEntryType) {
				return "LedgerEntryType";
			}

			if (typ == TransactionType) {
				return "TransactionType";
			}

			if (typ == Flags) {
				return "Flags";
			}

			if (typ == SourceTag) {
				return "SourceTag";
			}

			if (typ == Sequence) {
				return "Sequence";
			}
				
			if (typ == PreviousTxnLgrSeq) {
				return "PreviousTxnLgrSeq";
			}

			if (typ == LedgerSequence) {
				return "LedgerSequence";
			}

			if (typ == CloseTime) {
				return "CloseTime";
			}

			if (typ == ParentCloseTime) {
				return "ParentCloseTime";
			}

			if (typ == SigningTime) {
				return "SigningTime";
			}

			if (typ == Expiration) {
				return "Expiration";
			}

			if (typ == TransferRate) {
				return "TransferRate";
			}

			if (typ == WalletSize) {
				return "WalletSize";
			}

			if (typ == OwnerCount) {
				return "OwnerCount";
			}

			if (typ == DestinationTag) {
				return "DestinationTag";
			}

			if (typ == HighQualityIn) {
				return "HighQualityIn";
			}

			if (typ == HighQualityOut) {
				return "HighQualityOut";
			}

			if (typ == LowQualityIn) {
				return "LowQualityIn";
			}

			if (typ == LowQualityOut) {
				return "LowQualityOut";
			}

			if (typ == QualityIn) {
				return "QualityIn";
			}

			if (typ == QualityOut) {
				return "QualityOut";
			}

			if (typ == StampEscrow) {
				return "StampEscrow";
			}

			if (typ == BondAmount) {
				return "BondAmount";
			}

			if (typ == LoadFee) {
				return "LoadFee";
			}

			if (typ == OfferSequence) {
				return "OfferSequence";
			}

			if (typ == FirstLedgerSequence) {
				return "FirstLedgerSequence";
			}

			if (typ == LastLedgerSequence) {
				return "LastLedgerSequence";
			}

			if (typ == TransactionIndex) {
				return "TransactionIndex";
			}

			if (typ == OperationLimit) {
				return "OperationLimit";
			}

			if (typ == ReferenceFeeUnits) {
				return "ReferenceFeeUnits";
			}

			if (typ == ReserveBase) {
				return "ReserveBase";
			}

			if (typ == ReserveIncrement) {
				return "ReserveIncrement";
			}

			if (typ == SetFlag) {
				return "SetFlag";
			}

			if (typ == ClearFlag) {
				return "ClearFlag";
			}

			if (typ == IndexNext) {
				return "IndexNex";
			}

			if (typ == IndexPrevious) {
				return "IndexPrevious";
			}

			if (typ == BookNode) {
				return "BookNode";
			}

			if (typ == OwnerNode) {
				return "OwnerNode";
			}

			if (typ == BaseFee) {
				return "BaseFee";
			}

			if (typ ==ExchangeRate) {
				return "ExchangeRate";
			}

			if (typ == LowNode) {
				return "LowNode";
			}

			if (typ == HighNode) {
				return "HighNode";
			}

			if (typ == EmailHash) {
				return "EmailHash";
			}

			if (typ == LedgerHash) {
				return "LedgerHash";
			}

			if (typ == ParentHash) {
				return "ParentHash";
			}

			if (typ == TransactionHash) {
				return "TransactionHash";
			}

			if (typ == AccountHash) {
				return "AccountHash";
			}

			if (typ == PreviousTxnID) {
				return "PreviousTxnID";
			}

			if (typ == LedgerIndex) {
				return "LedgerIndex";
			}

			if (typ == WalletLocator) {
				return "WalletLocator";
			}

			if (typ == RootIndex) {
				return "RootIndex";
			}

			if (typ == BookDirectory) {
				return "BookDirectory";
			}

			if (typ == InvoiceID) {
				return "InvoiceID";
			}

			if (typ == Nickname) {
				return "Nickname";
			}

			if (typ == Feature) {
				return "Feature";
			}

			if (typ == TakerPaysCurrency) {
				return "TakerPaysCurrency";
			}

			if (typ == TakerPaysIssuer) {
				return "TakerPaysIssuer";
			}

			if (typ == TakerGetsCurrency) {
				return "TakerGetsCurrency";
			}

			if (typ == TakerGetsIssuer) {
				return "TakerGetsIssuer";
			}

			if (typ == Amount) {
				return "Amount";
			}

			if (typ == Balance) {
				return "Balance";
			}

			if (typ == LimitAmount) {
				return "LimitAmount";
			}

			if (typ == TakerPays) {
				return "TakerPays";
			}

			if (typ == TakerGets) {
				return "TakerGets";
			}

			if (typ == LowLimit) {
				return "LowLimit";
			}

			if (typ == HighLimit) {
				return "HighLimit";
			}

			if (typ == Fee) {
				return "Fee";
			}

			if (typ == SendMax) {
				return "SendMax";
			}

			if (typ == MinimumOffer) {
				return "MinimumOffer";
			}

			if (typ == RippleEscrow) {
				return "RippleEscrow";
			}

			if (typ == PublicKey) {
				return "PublicKey";
			}

			if (typ == MessageKey) {
				return "MessageKey";
			}

			if (typ == SigningPubKey) {
				return "SigningPubKey";
			}

			if (typ == TxnSignature) {
				return "TxnSignature";
			}

			if (typ == Generator) {
				return "Generator";
			}

			if (typ == Signature) {
				return "Signature";
			}

			if (typ == Domain) {
				return "Domain";
			}

			if (typ == FundCode) {
				return "FundCode";
			}

			if (typ == RemoveCode) {
				return "RemoveCode";
			}

			if (typ == ExpireCode) {
				return "ExpireCode";
			}

			if (typ == CreateCode) {
				return "CreateCode";
			}

			if (typ == Account) {
				return "Account";
			}

			if (typ == Owner) {
				return "Owner";
			}

			if (typ == Destination) {
				return "Destination";
			}

			if (typ == Issuer) {
				return "Issuer";
			}

			if (typ == Target) {
				return "Target";
			}

			if (typ == RegularKey) {
				return "RegularKey";
			}

			if (typ == Paths) {
				return "Paths";
			}

			if (typ == Indexes) {
				return "Indexes";
			}

			if (typ == Hashes) {
				return "Hashes";
			}

			if (typ == Features) {
				return "Features";
			}

			if (typ == TransactionMetaData) {
				return "TransactionMetaData";
			}

			if (typ == CreatedNode) {
				return "CreatedNode"; 
			}

			if (typ == DeletedNode) {
				return "DeletedNode";
			}

			if (typ == ModifiedNode) {
				return "ModifiedNode";
			}

			if (typ == PreviousFields) {
				return "PreviousFields";
			}

			if (typ == FinalFields) {
				return "FinalFields";
			}

			if (typ == NewFields) {
				return "NewFields";
			}

			if (typ == TemplateEntry) {
				return "TemplateEntry";
			}

			if (typ == SigningAccounts) {
				return "SigningAccounts";
			}

			if (typ == TxnSignatures) {
				return "TxnSignatures";
			}

			if (typ == Signatures) {
				return "Signatures";
			}

			if (typ == Template) {
				return "Template";
			}

			if (typ == Necessary) {
				return "Necessary";
			}

			if (typ == Sufficient) {
				return "Sufficient";
			}

			if (typ == AffectedNodes) {
				return "AffectedNodes";
			}

			throw new ArgumentException("No String value found for BinaryFieldType of type : " + typ.type.ToString() + " and Value : " + typ.value.ToString());
		}


#pragma warning disable RECS0122 // Initializing field with default value is redundant
		private static BinaryFieldType [] valuesCache = null;
		static BinaryFieldType[,] typeFieldLookup = null;
#pragma warning restore RECS0122 // Initializing field with default value is redundant

		static BinaryFieldType ()
		{
			foreach (BinaryFieldType f in GetValues()) {
				MAXBYTEVALUE = Math.Max(MAXBYTEVALUE, f.value);
			}

			MAXBYTEVALUE++;

			BinaryFieldType.typeFieldLookup = new BinaryFieldType[BinaryType.MAXBYTEVALUE,BinaryFieldType.MAXBYTEVALUE]; // note the two MAXBYTEVALUES are different 

			foreach (BinaryFieldType f in BinaryFieldType.GetValues()) {
				typeFieldLookup[f.type.typeCode,f.value] = f;
			}
		}

		private BinaryFieldType (BinaryType type, byte value)
		{
			this.type = type;
			this.value = value;
		}

		public readonly BinaryType type;
		public readonly byte value;


		//int fieldID;

#pragma warning disable RECS0122 // Initializing field with default value is redundant
		static byte MAXBYTEVALUE=0;
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

			return ((bft.type.Equals(this.type)) && (bft.value == this.value));
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
				return right is null;

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
				if (this == bft) {
					return 0;
				}

				foreach (BinaryFieldType b in GetValues()) {
					if (b == this) {
						return -1;
					}

					if (bft == b) {
						return 1;
					}
				}

			} else {
				throw new ArgumentException(
					"Can not compare BinaryFieldType to unknown type " 
					+ obj?.GetType()?.ToString() ?? "null");
			}

			throw new ArgumentException("Unknown error comparing BinaryFieldType. Report this as a bug");
		}

		public override string ToString ()
		{
			return StringFromType(this);
		}
	}
}

