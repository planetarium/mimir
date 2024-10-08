using Bencodex.Types;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.ItemEnhanceMail"/>
/// </summary>
[BsonIgnoreExtraElements]
public record ItemEnhanceMail : AttachmentMail
{
    public ItemEnhanceMail(IValue bencoded) : base(bencoded)
    {
    }
}
