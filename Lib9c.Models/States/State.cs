using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Libplanet.Crypto;
using ValueKind = Bencodex.Types.ValueKind;
using static Lib9c.SerializeKeys;
using Lib9c.Models.Extensions;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.States;

/// <summary>
/// <see cref="Nekoyume.Model.State.State"/>
/// </summary>
[BsonIgnoreExtraElements]
public record State : IBencodable
{
    public Address Address { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public virtual IValue Bencoded => BencodedAsList;

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public virtual IValue BencodedAsList => new List(Address.Serialize());

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public virtual IValue BencodedAsDictionaryV1 => Dictionary.Empty.SetItem(LegacyAddressKey, Address.Serialize());

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public virtual IValue BencodedAsDictionaryV2 => Dictionary.Empty.SetItem(AddressKey, Address.Serialize());

    public State()
    {
    }

    public State(IValue bencoded)
    {
        switch (bencoded)
        {
            case List l:
                Address = l[0].ToAddress();
                return;
            case Dictionary d:
                Address = d.ContainsKey(LegacyAddressKey)
                    ? d[LegacyAddressKey].ToAddress()
                    : d[AddressKey].ToAddress();
                return;
            default:
                throw new UnsupportedArgumentTypeException<ValueKind>(
                    nameof(bencoded),
                    new[] { ValueKind.List, ValueKind.Dictionary },
                    bencoded.Kind);
        }
    }
}
