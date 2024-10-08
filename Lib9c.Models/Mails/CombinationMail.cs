using Bencodex.Types;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.CombinationMail"/>
/// </summary>
[BsonIgnoreExtraElements]
public record CombinationMail : AttachmentMail
{
    public CombinationMail(IValue bencoded) : base(bencoded)
    {
    }
}
