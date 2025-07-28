using System;
using System.Text.Json;
using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Types;
using MongoDB.Bson;

namespace Lib9c.GraphQL.Types;

public class BsonValueType : ScalarType<BsonValue, StringValueNode>
{
    public BsonValueType() : base("BsonValue")
    {
    }

    public override IValueNode ParseResult(object? resultValue)
    {
        return resultValue switch
        {
            BsonValue value => ParseValue(value),
            string s => ParseValue(s),
            _ => throw new SerializationException(
                ErrorBuilder.New()
                    .SetMessage("Invalid runtime type. Expected: BsonValue or string.")
                    .SetCode(ErrorCodes.Scalars.InvalidRuntimeType)
                    .Build(),
                this)
        };
    }

    protected override BsonValue ParseLiteral(StringValueNode valueSyntax)
    {
        try
        {
            return BsonDocument.Parse(valueSyntax.Value);
        }
        catch (Exception ex)
        {
            throw new SerializationException(
                ErrorBuilder.New()
                    .SetMessage($"Invalid BSON format: {ex.Message}")
                    .SetCode("INVALID_SYNTAX")
                    .Build(),
                this);
        }
    }

    protected override StringValueNode ParseValue(BsonValue runtimeValue)
    {
        var jsonString = runtimeValue.ToJson();
        return new StringValueNode(jsonString);
    }

    public override bool TrySerialize(object? runtimeValue, out object? resultValue)
    {
        if (runtimeValue is BsonValue value)
        {
            resultValue = value.ToJson();
            return true;
        }

        resultValue = null;
        return false;
    }

    public override bool TryDeserialize(object? resultValue, out object? runtimeValue)
    {
        if (resultValue is string s)
        {
            try
            {
                runtimeValue = BsonDocument.Parse(s);
                return true;
            }
            catch
            {
                runtimeValue = null;
                return false;
            }
        }

        runtimeValue = null;
        return false;
    }
} 