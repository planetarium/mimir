using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Libplanet.Types.Assets;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.ClaimItemsMail"/>
/// </summary>
public record ClaimItemsMail : Mail
{
    public List<FungibleAssetValue> FungibleAssetValues { get; init; }
    public List<(int id, int count)> Items { get; init; }
    public string? Memo { get; init; }

    public override IValue Bencoded
    {
        get
        {
            var d = (Dictionary)base.Bencoded;
            if (FungibleAssetValues.Count != 0)
            {
                d = d.SetItem("f", new List(FungibleAssetValues
                    .Select(f => f.Serialize())));
            }

            if (Items.Count != 0)
            {
                d = d.SetItem("i", new List(Items
                    .Select(tuple => List.Empty.Add(tuple.id).Add(tuple.count))));
            }

            if (!string.IsNullOrEmpty(Memo))
            {
                d = d.SetItem("m", Memo);
            }

            return d;
        }
    }

    public ClaimItemsMail(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        FungibleAssetValues = d.ContainsKey("f")
            ? d["f"].ToList(StateExtensions.ToFungibleAssetValue)
            : new List<FungibleAssetValue>();
        Items = d.ContainsKey("i")
            ? d["i"].ToList<(int, int)>(v =>
            {
                var list = (List)v;
                return ((Integer)list[0], (Integer)list[1]);
            })
            : new List<(int id, int count)>();
        Memo = d.ContainsKey("m")
            ? ((Text)d["m"]).Value
            : null;
    }
}
