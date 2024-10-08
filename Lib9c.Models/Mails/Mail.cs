using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.Mail"/>
/// </summary>
[BsonIgnoreExtraElements]
public record Mail : IBencodable
{
    public Guid Id { get; init; }
    public string TypeId { get; init; }
    public long BlockIndex { get; init; }
    public long RequiredBlockIndex { get; init; }

    [BsonIgnore, GraphQLIgnore]
    public virtual IValue Bencoded => Dictionary.Empty
        .Add("id", Id.Serialize())
        .Add("typeId", TypeId.Serialize())
        .Add("blockIndex", BlockIndex.Serialize())
        .Add("requiredBlockIndex", RequiredBlockIndex.Serialize());

    public Mail(IValue bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        Id = d["id"].ToGuid();
        TypeId = d["typeId"].ToDotnetString();
        BlockIndex = d["blockIndex"].ToLong();
        RequiredBlockIndex = d["requiredBlockIndex"].ToLong();
    }
}
