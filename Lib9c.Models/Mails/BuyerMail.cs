using Bencodex.Types;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.BuyerMail"/>
/// </summary>
[BsonIgnoreExtraElements]
public record BuyerMail : AttachmentMail
{
    public BuyerMail(IValue bencoded) : base(bencoded)
    {
    }
}
