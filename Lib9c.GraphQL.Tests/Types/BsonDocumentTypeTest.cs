using System;
using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Execution;
using HotChocolate.Execution.Serialization;
using Lib9c.GraphQL.Types;
using MongoDB.Bson;
using Xunit;

namespace Lib9c.GraphQL.Tests.Types;

public class BsonDocumentTypeTest
{
    private readonly BsonDocumentType _type = new();

    [Fact]
    public void BsonDocumentType_Should_Have_Correct_Name()
    {
        Assert.Equal("BsonDocument", _type.Name);
    }

    [Fact]
    public void TrySerialize_With_BsonDocument_Should_Return_True()
    {
        var doc = new BsonDocument
        {
            { "key1", "value1" },
            { "key2", 123 }
        };

        var result = _type.TrySerialize(doc, out var serialized);

        Assert.True(result);
        Assert.NotNull(serialized);
        Assert.IsType<string>(serialized);
        
        var parsed = BsonDocument.Parse((string)serialized);
        Assert.Equal("value1", parsed["key1"].AsString);
        Assert.Equal(123, parsed["key2"].AsInt32);
    }

    [Fact]
    public void TrySerialize_With_Non_BsonDocument_Should_Return_False()
    {
        var result = _type.TrySerialize("not a bson document", out var serialized);

        Assert.False(result);
        Assert.Null(serialized);
    }

    [Fact]
    public void TryDeserialize_With_Valid_Json_String_Should_Return_True()
    {
        var jsonString = @"{""key1"": ""value1"", ""key2"": 123}";

        var result = _type.TryDeserialize(jsonString, out var deserialized);

        Assert.True(result);
        Assert.NotNull(deserialized);
        Assert.IsType<BsonDocument>(deserialized);
        
        var doc = (BsonDocument)deserialized;
        Assert.Equal("value1", doc["key1"].AsString);
        Assert.Equal(123, doc["key2"].AsInt32);
    }

    [Fact]
    public void TryDeserialize_With_Invalid_Json_String_Should_Return_False()
    {
        var invalidJson = "{ invalid json }";

        var result = _type.TryDeserialize(invalidJson, out var deserialized);

        Assert.False(result);
        Assert.Null(deserialized);
    }

    [Fact]
    public void ParseValue_Should_Return_StringValueNode()
    {
        var doc = new BsonDocument
        {
            { "key1", "value1" }
        };

        var result = _type.ParseValue(doc);

        Assert.IsType<StringValueNode>(result);
        var stringNode = (StringValueNode)result;
        Assert.NotNull(stringNode.Value);
        
        var parsed = BsonDocument.Parse(stringNode.Value);
        Assert.Equal("value1", parsed["key1"].AsString);
    }

    [Fact]
    public void ParseLiteral_With_Valid_Json_Should_Return_BsonDocument()
    {
        var jsonString = @"{""key1"": ""value1""}";
        var literal = new StringValueNode(jsonString);

        var result = _type.ParseLiteral(literal);

        Assert.IsType<BsonDocument>(result);
        var doc = (BsonDocument)result;
        Assert.Equal("value1", doc["key1"].AsString);
    }

} 