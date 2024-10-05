using System.Numerics;
using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Types;
using System.Globalization;

namespace Lib9c.GraphQL.Types;

public class BigIntegerType : ScalarType<BigInteger, StringValueNode>
{
    public BigIntegerType() : base("BigInteger")
    {
    }

    public override IValueNode ParseResult(object? resultValue)
    {
        return resultValue switch
        {
            BigInteger bigInteger => ParseValue(bigInteger),
            string s => ParseValue(s),
            _ => throw new SerializationException(
                ErrorBuilder.New()
                    .SetMessage("Invalid runtime type. Expected: BigInteger or string.")
                    .SetCode(ErrorCodes.Scalars.InvalidRuntimeType)
                    .Build(),
                this)
        };
    }

    protected override BigInteger ParseLiteral(StringValueNode valueSyntax) =>
        BigInteger.Parse(valueSyntax.Value, CultureInfo.InvariantCulture);

    protected override StringValueNode ParseValue(BigInteger runtimeValue) =>
        new(runtimeValue.ToString(CultureInfo.InvariantCulture));

    public override bool TrySerialize(object? runtimeValue, out object? resultValue)
    {
        if (runtimeValue is BigInteger bigInteger)
        {
            resultValue = bigInteger.ToString(CultureInfo.InvariantCulture);
            return true;
        }

        resultValue = null;
        return false;
    }

    public override bool TryDeserialize(object? resultValue, out object? runtimeValue)
    {
        if (resultValue is string s)
        {
            if (BigInteger.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out BigInteger bigInteger))
            {
                runtimeValue = bigInteger;
                return true;
            }
        }

        runtimeValue = null;
        return false;
    }
}
