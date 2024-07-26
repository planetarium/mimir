using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;
using static Lib9c.SerializeKeys;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Costume"/>
/// </summary>
public record Costume : ItemBase, IBencodable
{
    public bool Equipped { get; }
    public string SpineResourcePath { get; }
    public Guid ItemId { get; }
    public long RequiredBlockIndex { get; }

    public override IValue Bencoded
    {
        get
        {
            var d = ((Dictionary)base.Bencoded)
                .Add("equipped", Equipped.Serialize())
                .Add("spine_resource_path", SpineResourcePath.Serialize())
                .Add(LegacyCostumeItemIdKey, ItemId.Serialize());

            return RequiredBlockIndex > 0
                ? d.Add(RequiredBlockIndexKey, RequiredBlockIndex.Serialize())
                : d;
        }
    }

    public Costume(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        if (d.TryGetValue((Text)"equipped", out var toEquipped))
        {
            Equipped = toEquipped.ToBoolean();
        }

        if (d.TryGetValue((Text)"spine_resource_path", out var spineResourcePath))
        {
            SpineResourcePath = (Text)spineResourcePath;
        }

        ItemId = d[LegacyCostumeItemIdKey].ToGuid();

        if (d.ContainsKey(RequiredBlockIndexKey))
        {
            RequiredBlockIndex = d[RequiredBlockIndexKey].ToLong();
        }
    }
}
