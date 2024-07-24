using Bencodex.Types;
using Libplanet.Types.Assets;
using Mimir.Models.Exceptions;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Mimir.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.AdventureBossRaffleWinnerMail"/>
/// </summary>
public record AdventureBossRaffleWinnerMail : AttachmentMail
{
    public long Season { get; init; }
    public FungibleAssetValue Reward { get; init; }

    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add("s", Season)
        .Add("r", Reward.Serialize());

    public AdventureBossRaffleWinnerMail(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                [ValueKind.Dictionary],
                bencoded.Kind);
        }

        Season = (Integer)d["s"];
        Reward = d["r"].ToFungibleAssetValue();
    }
}
