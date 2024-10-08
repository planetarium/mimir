using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Market;

/// <summary>
/// <see cref="Nekoyume.Model.Market.ProductsState"/>
/// </summary>
[BsonIgnoreExtraElements]
public record ProductsState : IBencodable
{
    public List<Guid> ProductIds { get; init; } = new();

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public IValue Bencoded => new List(ProductIds.Select(e => e.Serialize()));

    public ProductsState()
    {
    }

    public ProductsState(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind);
        }

        ProductIds = l
            .Select(e => e.ToGuid())
            .ToList();
    }
}
