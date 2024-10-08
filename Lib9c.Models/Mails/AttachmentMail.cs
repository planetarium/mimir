using Bencodex.Types;
using Lib9c.Models.AttachmentActionResults;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Factories;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.AttachmentMail"/>
/// </summary>
[BsonIgnoreExtraElements]
public record AttachmentMail : Mail
{
    public AttachmentActionResult Attachment { get; init; }

    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add("attachment", Attachment.Bencoded);

    public AttachmentMail(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        Attachment = AttachmentActionResultFactory.Create(d["attachment"]);
    }
}
