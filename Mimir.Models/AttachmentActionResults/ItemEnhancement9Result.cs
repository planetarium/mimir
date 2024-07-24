using System.Numerics;
using Bencodex.Types;
using Mimir.Models.Exceptions;
using Mimir.Models.Factories;
using Mimir.Models.Item;
using Nekoyume.Action;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Mimir.Models.AttachmentActionResults;

/// <summary>
/// <see cref="Nekoyume.Action.ItemEnhancement9.ResultModel"/>
/// </summary>
public record ItemEnhancement9Result : AttachmentActionResult
{
    public Guid Id { get; init; }
    public IEnumerable<Guid> MaterialItemIdList { get; init; }
    public BigInteger Gold { get; init; }
    public int ActionPoint { get; init; }
    public ItemEnhancement9.EnhancementResult EnhancementResult { get; init; }
    public ItemUsable? PreItemUsable { get; init; }

    public ItemEnhancement9Result(IValue bencoded) : base(bencoded)
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
        EnhancementResult = d["enhancementResult"].ToEnum<ItemEnhancement9.EnhancementResult>();
        PreItemUsable = d.ContainsKey("preItemUsable")
            ? (ItemUsable)ItemFactory.Deserialize((Dictionary)d["preItemUsable"])
            : null;
    }

    public override IValue Bencoded
    {
        get
        {
            var d = ((Dictionary)base.Bencoded)
                .Add("id", Id.Serialize())
                .Add("materialItemIdList", MaterialItemIdList
                    .OrderBy(i => i)
                    .Select(g => g.Serialize())
                    .Serialize())
                .Add("gold", Gold.Serialize())
                .Add("actionPoint", ActionPoint.Serialize())
                .Add("enhancementResult", EnhancementResult.Serialize());
            if (PreItemUsable is not null)
            {
                d = d.Add("preItemUsable", PreItemUsable.Bencoded);
            }

            return d;
        }
    }
}
