using System.Numerics;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.AttachmentActionResults;

/// <summary>
/// <see cref="Nekoyume.Action.ItemEnhancement7.ResultModel"/>
/// </summary>
public record ItemEnhancement7Result : AttachmentActionResult
{
    public Guid Id { get; init; }
    public IEnumerable<Guid> MaterialItemIdList { get; init; }
    public BigInteger Gold { get; init; }
    public int ActionPoint { get; init; }

    [BsonIgnore, GraphQLIgnore]
    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add("id", Id.Serialize())
        .Add("materialItemIdList", MaterialItemIdList
            .OrderBy(i => i)
            .Select(g => g.Serialize())
            .Serialize())
        .Add("gold", Gold.Serialize())
        .Add("actionPoint", ActionPoint.Serialize());

    public ItemEnhancement7Result()
    {
    }

    public ItemEnhancement7Result(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        Id = d["id"].ToGuid();
        MaterialItemIdList = d["materialItemIdList"].ToList(StateExtensions.ToGuid);
        Gold = d["gold"].ToBigInteger();
        ActionPoint = d["actionPoint"].ToInteger();
    }
}
