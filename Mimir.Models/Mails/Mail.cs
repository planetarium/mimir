using Bencodex;
using Bencodex.Types;
using Mimir.Models.Exceptions;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Mimir.Models.Mails;

public record Mail : IBencodable
{
    public Guid Id { get; init; }
    public string TypeId { get; init; }
    public long BlockIndex { get; init; }
    public long RequiredBlockIndex { get; init; }

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
                [ValueKind.Dictionary],
                bencoded.Kind);
        }

        Id = d["id"].ToGuid();
        TypeId = d["typeId"].ToDotnetString();
        BlockIndex = d["blockIndex"].ToLong();
        RequiredBlockIndex = d["requiredBlockIndex"].ToLong();
    }
}
