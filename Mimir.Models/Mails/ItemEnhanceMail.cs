using Bencodex.Types;

namespace Mimir.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.ItemEnhanceMail"/>
/// </summary>
public record ItemEnhanceMail : AttachmentMail
{
    public ItemEnhanceMail(IValue bencoded) : base(bencoded)
    {
    }
}
