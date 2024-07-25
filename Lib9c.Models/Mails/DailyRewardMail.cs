using Bencodex.Types;

namespace Lib9c.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.DailyRewardMail"/>
/// </summary>
public record DailyRewardMail : AttachmentMail
{
    public DailyRewardMail(IValue bencoded) : base(bencoded)
    {
    }
}
