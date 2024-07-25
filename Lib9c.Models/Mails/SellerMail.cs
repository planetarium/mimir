using Bencodex.Types;

namespace Lib9c.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.SellerMail"/>
/// </summary>
public record SellerMail : AttachmentMail
{
    public SellerMail(IValue bencoded) : base(bencoded)
    {
    }
}
