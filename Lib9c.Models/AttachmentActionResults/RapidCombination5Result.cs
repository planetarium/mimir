using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Factories;
using Lib9c.Models.Items;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.AttachmentActionResults;

/// <summary>
/// <see cref="Nekoyume.Action.RapidCombination5.ResultModel"/>
/// </summary>
public record RapidCombination5Result : AttachmentActionResult
{
    public Guid Id { get; init; }
    private Dictionary<Material, int> Cost { get; init; }

    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add("id", Id.Serialize())
        .Add("cost", new List(Cost
            .OrderBy(kv => kv.Key.Id)
            .Select(pair => (IValue)Dictionary.Empty
                .Add("material", pair.Key.Bencoded)
                .Add("count", pair.Value.Serialize()))));

    public RapidCombination5Result(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                [ValueKind.Dictionary],
                bencoded.Kind);
        }

        Id = d["id"].ToGuid();
        Cost = ((List)d["cost"])
            .Cast<Dictionary>()
            .ToDictionary(
                value => (Material)ItemFactory.Deserialize((Dictionary)value["material"]),
                value => value["count"].ToInteger());
    }
}
