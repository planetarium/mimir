using Bencodex.Types;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.MonsterCollectionMail"/>
/// </summary>
[BsonIgnoreExtraElements]
public record MonsterCollectionMail : AttachmentMail
{
    public MonsterCollectionMail(IValue bencoded) : base(bencoded)
    {
    }
}
