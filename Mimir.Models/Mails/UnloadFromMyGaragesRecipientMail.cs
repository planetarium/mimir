using System.Security.Cryptography;
using Bencodex.Types;
using Libplanet.Common;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Mimir.Models.Exceptions;
using Nekoyume.Action.Garages;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Mimir.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.UnloadFromMyGaragesRecipientMail"/>
/// </summary>
public record UnloadFromMyGaragesRecipientMail : AttachmentMail
{
    public IOrderedEnumerable<(Address balanceAddr, FungibleAssetValue value)>?
        FungibleAssetValues { get; init; }

    public IOrderedEnumerable<(HashDigest<SHA256> fungibleId, int count)>?
        FungibleIdAndCounts { get; init; }

    public string? Memo { get; init; }

    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add("l", new List(
            FungibleAssetValues is null
                ? Null.Value
                : new List(FungibleAssetValues.Select(tuple => new List(
                    tuple.balanceAddr.Serialize(),
                    tuple.value.Serialize()))),
            FungibleIdAndCounts is null
                ? Null.Value
                : new List(FungibleIdAndCounts.Select(tuple => new List(
                    tuple.fungibleId.Serialize(),
                    (Integer)tuple.count))),
            Memo is null
                ? Null.Value
                : (Text)Memo));

    public UnloadFromMyGaragesRecipientMail(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                [ValueKind.Dictionary],
                bencoded.Kind);
        }

        var list = (List)d["l"];
        var fungibleAssetValues = list[0].Kind == ValueKind.Null
            ? null
            : ((List)list[0]).Select(e =>
            {
                var l2 = (List)e;
                return (
                    l2[0].ToAddress(),
                    l2[1].ToFungibleAssetValue()
                );
            });
        FungibleAssetValues = GarageUtils.MergeAndSort(fungibleAssetValues);
        var fungibleIdAndCounts = list[1].Kind == ValueKind.Null
            ? null
            : ((List)list[1]).Select(e =>
            {
                var l2 = (List)e;
                return (
                    l2[0].ToItemId(),
                    (int)((Integer)l2[1]).Value);
            });
        FungibleIdAndCounts = GarageUtils.MergeAndSort(fungibleIdAndCounts);
        Memo = list[2].Kind == ValueKind.Null
            ? null
            : (string)(Text)list[2];
    }
}
