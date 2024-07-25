using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.States;

public record ItemSlotState : IBencodable
{
    public BattleType BattleType { get; init; }
    public List<Guid> Costumes { get; init; }
    public List<Guid> Equipments { get; init; }

    public IValue Bencoded => List.Empty
        .Add(BattleType.Serialize())
        .Add(Costumes
            .OrderBy(x => x)
            .Select(x => x.Serialize())
            .Serialize())
        .Add(Equipments
            .OrderBy(x => x)
            .Select(x => x.Serialize())
            .Serialize());

    public ItemSlotState(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                [ValueKind.List],
                bencoded.Kind);
        }

        BattleType = l[0].ToEnum<BattleType>();
        Costumes = l[1].ToList(StateExtensions.ToGuid);
        Equipments = l[2].ToList(StateExtensions.ToGuid);
    }
}
