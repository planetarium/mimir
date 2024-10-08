using Bencodex.Types;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.SellCancelMail"/>
/// </summary>
[BsonIgnoreExtraElements]
public record SellCancelMail : AttachmentMail
{
    public SellCancelMail(IValue bencoded) : base(bencoded)
    {
    }
}
