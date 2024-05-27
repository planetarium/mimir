using System;
using System.Globalization;
using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Types;

namespace Lib9c.GraphQL.Types;

public class GuidType : ScalarType<Guid, StringValueNode>
{
    public GuidType() : base("Guid")
    {
    }

    public override IValueNode ParseResult(object? resultValue)
    {
        return resultValue switch
        {
            Guid guid => ParseValue(guid),
            string s => ParseValue(s),
            _ => throw new SerializationException(
                ErrorBuilder.New()
                    .SetMessage("Invalid runtime type. Expected: Guid or string.")
                    .SetCode(ErrorCodes.Scalars.InvalidRuntimeType)
                    .Build(),
                this)
        };
    }

    protected override Guid ParseLiteral(StringValueNode valueSyntax) =>
        Guid.Parse(valueSyntax.Value);

    protected override StringValueNode ParseValue(Guid runtimeValue) =>
        new(runtimeValue.ToString("D", CultureInfo.InvariantCulture));

    public override bool TrySerialize(object? runtimeValue, out object? resultValue)
    {
        if (runtimeValue is Guid guid)
        {
            resultValue = guid.ToString("D", CultureInfo.InvariantCulture);
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
