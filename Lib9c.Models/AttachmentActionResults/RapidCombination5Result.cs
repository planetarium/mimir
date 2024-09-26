using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Lib9c.Models.Factories;
using Lib9c.Models.Items;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.AttachmentActionResults;

/// <summary>
/// <see cref="Nekoyume.Action.RapidCombination5.ResultModel"/>
/// </summary>
public record RapidCombination5Result : AttachmentActionResult
{
    public Guid Id { get; init; }
    public Dictionary<Material, int> Cost { get; init; }

    [BsonIgnore, GraphQLIgnore]
    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add("id", Id.Serialize())
        .Add("cost", new List(Cost
            .OrderBy(kv => kv.Key.Id)
            .Select(pair => (IValue)Dictionary.Empty
                .Add("material", pair.Key.Bencoded)
                .Add("count", pair.Value.Serialize()))));

    public RapidCombination5Result()
    {
    }

    public RapidCombination5Result(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        Id = d["id"].ToGuid();
        Cost = ((List)d["cost"])
            .Cast<Dictionary>()
            .ToDictionary(
                value => (Material)ItemFactory.Deserialize(value["material"]),
                value => value["count"].ToInteger());
    }
}
