using System.Numerics;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Factories;
using Lib9c.Models.Items;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.AttachmentActionResults;

/// <summary>
/// <see cref="Nekoyume.Action.CombinationConsumable5.ResultModel"/>
/// </summary>
public record CombinationConsumable5Result : AttachmentActionResult
{
    private Dictionary<Material, int> Materials { get; init; }
    public Guid Id { get; init; }
    public BigInteger Gold { get; init; }
    public int ActionPoint { get; init; }
    public int RecipeId { get; init; }
    public int? SubRecipeId { get; init; }

    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add("materials", new List(Materials
            .OrderBy(kv => kv.Key.Id)
            .Select(pair => (IValue)Dictionary.Empty
                .Add("material", pair.Key.Bencoded)
                .Add("count", pair.Value.Serialize()))))
        .Add("id", Id.Serialize())
        .Add("gold", Gold.Serialize())
        .Add("actionPoint", ActionPoint.Serialize())
        .Add("recipeId", RecipeId.Serialize())
        .Add("subRecipeId", SubRecipeId.Serialize());

    public CombinationConsumable5Result(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        Materials = ((List)d["materials"])
            .Cast<Dictionary>()
            .ToDictionary(
                value => (Material)ItemFactory.Deserialize((Dictionary)value["material"]),
                value => value["count"].ToInteger());
        Id = d["id"].ToGuid();
        Gold = d["gold"].ToBigInteger();
        ActionPoint = d["actionPoint"].ToInteger();
        RecipeId = d["recipeId"].ToInteger();
        SubRecipeId = d["subRecipeId"].ToNullableInteger();
    }
}
