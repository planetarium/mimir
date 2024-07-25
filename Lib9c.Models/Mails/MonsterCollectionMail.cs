using Bencodex.Types;

namespace Lib9c.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.MonsterCollectionMail"/>
/// </summary>
public record MonsterCollectionMail : AttachmentMail
{
    public MonsterCollectionMail(IValue bencoded) : base(bencoded)
    {
    }
}
