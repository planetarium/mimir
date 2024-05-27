using System.Security.Cryptography;
using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Types;
using Libplanet.Common;

namespace Lib9c.GraphQL.Types;

public class HashDigestSHA256Type : ScalarType<HashDigest<SHA256>, StringValueNode>
{
    public HashDigestSHA256Type() : base("HashDigestSHA256")
    {
    }

    public override IValueNode ParseResult(object? resultValue)
    {
        return resultValue switch
        {
            HashDigest<SHA256> hashDigest => ParseValue(hashDigest),
            string s => ParseValue(s),
            _ => throw new SerializationException(
                ErrorBuilder.New()
                    .SetMessage("Invalid runtime type. Expected: HashDigest<SHA256> or string.")
                    .SetCode(ErrorCodes.Scalars.InvalidRuntimeType)
                    .Build(),
                this)
        };
    }

    protected override HashDigest<SHA256> ParseLiteral(StringValueNode valueSyntax) =>
        HashDigest<SHA256>.FromString(valueSyntax.Value);

    protected override StringValueNode ParseValue(HashDigest<SHA256> runtimeValue) =>
        new(runtimeValue.ToString());

    public override bool TrySerialize(object? runtimeValue, out object? resultValue)
    {
        if (runtimeValue is HashDigest<SHA256> hd)
        {
            resultValue = hd.ToString();
            return true;
        }

        resultValue = null;
        return false;
    }

    public override bool TryDeserialize(object? resultValue, out object? runtimeValue)
    {
        if (resultValue is string s)
        {
            runtimeValue = ParseValue(s);
            return true;
        }

        runtimeValue = null;
        return false;
    }
}
