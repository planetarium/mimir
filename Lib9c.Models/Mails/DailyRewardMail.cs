using Bencodex.Types;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.DailyRewardMail"/>
/// </summary>
[BsonIgnoreExtraElements]
public record DailyRewardMail : AttachmentMail
{
    public DailyRewardMail(IValue bencoded) : base(bencoded)
    {
    }
}
