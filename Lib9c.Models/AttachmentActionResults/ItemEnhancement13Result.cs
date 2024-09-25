using System.Numerics;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Lib9c.Models.Factories;
using Lib9c.Models.Items;
using Libplanet.Types.Assets;
using Nekoyume.Action;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.AttachmentActionResults;

/// <summary>
/// <see cref="Nekoyume.Action.ItemEnhancement13.ResultModel"/>
/// </summary>
public record ItemEnhancement13Result : AttachmentActionResult
{
    public Guid Id { get; init; }
    public IEnumerable<Guid> MaterialItemIdList { get; init; }
    public BigInteger Gold { get; init; }
    public int ActionPoint { get; init; }
    public ItemEnhancement9.EnhancementResult EnhancementResult { get; init; }
    public ItemUsable? PreItemUsable { get; init; }
    public FungibleAssetValue Crystal { get; init; }

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

            return d.Add("c", Crystal.Serialize());
        }
    }

    public ItemEnhancement13Result(IValue bencoded) : base(bencoded)
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
        EnhancementResult = d["enhancementResult"].ToEnum<ItemEnhancement9.EnhancementResult>();
        PreItemUsable = d.ContainsKey("preItemUsable")
            ? (ItemUsable)ItemFactory.Deserialize(d["preItemUsable"])
            : null;
        Crystal = new FungibleAssetValue(d["c"]);
    }
}
