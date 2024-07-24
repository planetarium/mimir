using Bencodex.Types;

namespace Mimir.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.BuyerMail"/>
/// </summary>
public record BuyerMail : AttachmentMail
{
    public BuyerMail(IValue bencoded) : base(bencoded)
    {
    }
}
