using HotChocolate.Types;
using Lib9c.Models.Block;

namespace Lib9c.GraphQL.Types;

public class TransactionType : ObjectType<Transaction>
{
    protected override void Configure(IObjectTypeDescriptor<Transaction> descriptor)
    {
        descriptor
            .Field(f => f.Id)
            .Type<StringType>();

        descriptor
            .Field(f => f.Nonce)
            .Type<LongType>();

        descriptor
            .Field(f => f.PublicKey)
            .Type<StringType>();

        descriptor
            .Field(f => f.Signature)
            .Type<StringType>();

        descriptor
            .Field(f => f.Signer)
            .Type<AddressType>();

        descriptor
            .Field(f => f.Timestamp)
            .Type<StringType>();

        descriptor
            .Field(f => f.BlockTimestamp)
            .Type<StringType>();

        descriptor
            .Field(f => f.TxStatus)
            .Type<EnumType<TxStatus>>();

        descriptor
            .Field(f => f.UpdatedAddresses)
            .Type<ListType<AddressType>>();

        descriptor
            .Field(f => f.Actions)
            .Type<ListType<ActionType>>();
    }
} 