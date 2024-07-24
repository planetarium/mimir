using Bencodex.Types;

namespace Mimir.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.SellCancelMail"/>
/// </summary>
public record SellCancelMail : AttachmentMail
{
    public SellCancelMail(IValue bencoded) : base(bencoded)
    {
    }
}
