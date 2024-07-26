using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Types;
using Libplanet.Crypto;

namespace Lib9c.GraphQL.Types;

public class AddressType : ScalarType<Address, StringValueNode>
{
    public AddressType() : base("LibplanetAddress")
    {
    }

    public override IValueNode ParseResult(object? resultValue)
    {
        return resultValue switch
        {
            Address address => ParseValue(address),
            string s => ParseValue(s),
            _ => throw new SerializationException(
                ErrorBuilder.New()
                    .SetMessage("Invalid runtime type. Expected: Address or string.")
                    .SetCode(ErrorCodes.Scalars.InvalidRuntimeType)
                    .Build(),
                this)
        };
    }

    protected override Address ParseLiteral(StringValueNode valueSyntax) =>
        new(valueSyntax.Value);

    protected override StringValueNode ParseValue(Address runtimeValue) =>
        new(runtimeValue.ToString());

    public override bool TrySerialize(object? runtimeValue, out object? resultValue)
    {
        if (runtimeValue is Address address)
        {
            resultValue = address.ToString();
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
