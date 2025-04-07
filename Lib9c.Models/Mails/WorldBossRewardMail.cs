using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Libplanet.Types.Assets;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.WorldBossRewardMail"/>
/// </summary>
[BsonIgnoreExtraElements]
public record WorldBossRewardMail : Mail
{
    public List<(int id, int count)> Items { get; init; }
    public List<FungibleAssetValue> FungibleAssetValues { get; init; }
    public override IValue Bencoded
    {
        get
        {
            var dict = (Dictionary)base.Bencoded;
            if (FungibleAssetValues?.Any() == true)
            {
                dict = dict.Add("f", new List(FungibleAssetValues.Select(f => f.Serialize())));
            }

            if (Items?.Any() == true)
            {
                dict = dict.Add("i",
                    new List(Items.Select(tuple => List.Empty.Add(tuple.id).Add(tuple.count))));
            }

            return dict;
        }
    }

    public WorldBossRewardMail(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        if (d.ContainsKey("i"))
        {
            Items = d["i"].ToList<(int, int)>(v =>
            {
                var list = (List)v;
                return ((Integer)list[0], (Integer)list[1]);
            });
        }

        if (d.ContainsKey("f"))
        {
            FungibleAssetValues = d["f"].ToList(StateExtensions.ToFungibleAssetValue);
        }
    }
}
