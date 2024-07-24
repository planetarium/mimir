using System.Numerics;
using Bencodex.Types;
using Mimir.Models.Exceptions;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Mimir.Models.AttachmentActionResults;

/// <summary>
/// <see cref="Nekoyume.Action.ItemEnhancement7.ResultModel"/>
/// </summary>
public record ItemEnhancement7Result : AttachmentActionResult
{
    public Guid Id { get; init; }
    public IEnumerable<Guid> MaterialItemIdList { get; init; }
    public BigInteger Gold { get; init; }
    public int ActionPoint { get; init; }

    public ItemEnhancement7Result(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                [ValueKind.Dictionary],
                bencoded.Kind);
        }

        Id = d["id"].ToGuid();
        MaterialItemIdList = d["materialItemIdList"].ToList(StateExtensions.ToGuid);
        Gold = d["gold"].ToBigInteger();
        ActionPoint = d["actionPoint"].ToInteger();
    }

    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add("id", Id.Serialize())
        .Add("materialItemIdList", MaterialItemIdList
            .OrderBy(i => i)
            .Select(g => g.Serialize())
            .Serialize())
        .Add("gold", Gold.Serialize())
        .Add("actionPoint", ActionPoint.Serialize());
}
