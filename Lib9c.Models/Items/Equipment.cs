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

    [BsonIgnore, GraphQLIgnore]
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
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        if (d.TryGetValue((Text)LegacyEquippedKey, out var value))
        {
            Equipped = value.ToBoolean();
        }

        if (d.TryGetValue((Text)LegacyLevelKey, out value))
        {
            try
            {
                Level = value.ToInteger();
            }
            catch (InvalidCastException)
            {
                Level = (int)((Integer)value).Value;
            }
        }

        if (d.TryGetValue((Text)EquipmentExpKey, out value))
        {
            try
            {
                Exp = value.ToLong();
            }
            catch (InvalidCastException)
            {
                Exp = (long)((Integer)value).Value;
            }
        }
        else
        {
            Exp = 0L;
        }

        if (d.TryGetValue((Text)LegacyStatKey, out value))
        {
            Stat = new DecimalStat(value);
        }

        if (d.TryGetValue((Text)LegacySetIdKey, out value))
        {
            SetId = value.ToInteger();
        }

        if (d.TryGetValue((Text)LegacySpineResourcePathKey, out value))
        {
            SpineResourcePath = (Text)value;
        }

        if (d.TryGetValue((Text)OptionCountFromCombinationKey, out value))
        {
            OptionCountFromCombination = value.ToInteger();
        }

        if (d.TryGetValue((Text)MadeWithMimisbrunnrRecipeKey, out value))
        {
            MadeWithMimisbrunnrRecipe = value.ToBoolean();
        }
    }
}
