using System.Text.Json.Serialization;
using System.Reflection;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Stats;
using ValueKind = Bencodex.Types.ValueKind;
using static Lib9c.SerializeKeys;
using Lib9c.Models.Extensions;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Equipment"/>
/// </summary>
[BsonIgnoreExtraElements]
public record Equipment : ItemUsable
{
    public bool Equipped { get; init; }
    public int Level { get; init; }
    public long Exp { get; init; }
    public DecimalStat Stat { get; init; }
    public int SetId { get; init; }
    public string SpineResourcePath { get; init; }
    public int OptionCountFromCombination { get; init; }
    public bool MadeWithMimisbrunnrRecipe { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public override IValue Bencoded
    {
        get
        {
            var d = ((Dictionary)base.Bencoded)
                .Add(LegacyEquippedKey, Equipped.Serialize())
                .Add(LegacyLevelKey, Level.Serialize())
                .Add(LegacyStatKey, Stat.BencodedAsLegacy)
                .Add(LegacySetIdKey, SetId.Serialize())
                .Add(LegacySpineResourcePathKey, SpineResourcePath.Serialize());

            if (OptionCountFromCombination > 0)
            {
                d = d.SetItem(OptionCountFromCombinationKey, OptionCountFromCombination.Serialize());
            }

            if (MadeWithMimisbrunnrRecipe)
            {
                d = d.SetItem(MadeWithMimisbrunnrRecipeKey, MadeWithMimisbrunnrRecipe.Serialize());
            }

            if (Exp > 0)
            {
                d = d.SetItem(EquipmentExpKey, Exp.Serialize());
            }

            return d;
        }
    }

    public Equipment()
    {
    }

    public Equipment(IValue bencoded) : base(bencoded)
    {
        try
        {
            var equipment = (Nekoyume.Model.Item.Equipment)Nekoyume.Model.Item.ItemFactory.Deserialize(bencoded);
            Equipped = equipment.Equipped;
            Level = equipment.level;
            Exp = equipment.Exp;
            Stat = new DecimalStat(equipment.Stat.Serialize());
            SetId = equipment.SetId;
            SpineResourcePath = equipment.SpineResourcePath;
            OptionCountFromCombination = equipment.optionCountFromCombination;
            MadeWithMimisbrunnrRecipe = equipment.MadeWithMimisbrunnrRecipe;
        }
        catch (ArgumentException)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary, ValueKind.List },
                bencoded.Kind);
        }
    }
}
