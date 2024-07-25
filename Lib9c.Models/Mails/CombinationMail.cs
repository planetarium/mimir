using Bencodex.Types;

namespace Lib9c.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.CombinationMail"/>
/// </summary>
public record CombinationMail : AttachmentMail
{
    public CombinationMail(IValue bencoded) : base(bencoded)
    {
    }
}
