using Bencodex.Types;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.SellerMail"/>
/// </summary>
[BsonIgnoreExtraElements]
public record SellerMail : AttachmentMail
{
    public SellerMail(IValue bencoded) : base(bencoded)
    {
    }
}
