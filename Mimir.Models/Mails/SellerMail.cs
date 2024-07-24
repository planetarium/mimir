using Bencodex.Types;

namespace Mimir.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.SellerMail"/>
/// </summary>
public record SellerMail : AttachmentMail
{
    public SellerMail(IValue bencoded) : base(bencoded)
    {
    }
}
