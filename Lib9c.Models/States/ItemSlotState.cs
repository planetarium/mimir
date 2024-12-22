using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using MongoDB.Bson.Serialization.Attributes;
using Nekoyume.Model.EnumType;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.States;

/// <summary>
/// <see cref="Nekoyume.Model.State.ItemSlotState"/>
/// </summary>
[BsonIgnoreExtraElements]
public record ItemSlotState : IBencodable
{
    public BattleType BattleType { get; init; }
    public List<Guid> Costumes { get; init; }
    public List<Guid> Equipments { get; init; }

    public ItemSlotState() { }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public IValue Bencoded =>
        List
            .Empty.Add(BattleType.Serialize())
            .Add(Costumes.OrderBy(x => x).Select(x => x.Serialize()).Serialize())
            .Add(Equipments.OrderBy(x => x).Select(x => x.Serialize()).Serialize());

    public ItemSlotState(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind
            );
        }

        BattleType = l[0].ToEnum<BattleType>();
        Costumes = l[1].ToList(StateExtensions.ToGuid);
        Equipments = l[2].ToList(StateExtensions.ToGuid);
    }
}
