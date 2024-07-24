using Bencodex.Types;
using Libplanet.Types.Assets;
using Mimir.Models.Exceptions;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Mimir.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.GrindingMail"/>
/// </summary>
public record GrindingMail : Mail
{
    public int ItemCount { get; init; }
    public FungibleAssetValue Asset { get; init; }

    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add("ic", ItemCount.Serialize())
        .Add("a", Asset.Serialize());

    public GrindingMail(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                [ValueKind.Dictionary],
                bencoded.Kind);
        }

        ItemCount = d["ic"].ToInteger();
        Asset = d["a"].ToFungibleAssetValue();
    }
}
