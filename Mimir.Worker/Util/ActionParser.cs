using System.Globalization;
using System.Text.Json;
using Bencodex;
using Bencodex.Types;
using Lib9c;
using Libplanet.Crypto;
using MongoDB.Bson;

namespace Mimir.Worker.Util;

public static class ActionParser
{
    private static readonly Codec Codec = new();

    public static (string TypeId, BsonDocument Values, BsonDocument ParsedAction) ParseAction(
        string raw
    )
    {
        try
        {
            var decodedAction = Codec.Decode(Convert.FromHexString(raw));

            if (decodedAction is not Dictionary actionDict)
            {
                throw new InvalidCastException(
                    $"Invalid action type. Expected Dictionary, got {decodedAction.GetType().Name}."
                );
            }

            var typeId = "";
            if (actionDict.TryGetValue((Text)"type_id", out var typeIdValue))
            {
                typeId = typeIdValue switch
                {
                    Text text => text.Value,
                    Integer integer => integer.Value.ToString(),
                    _ => "",
                };
            }

            var values = new BsonDocument();
            if (actionDict.TryGetValue((Text)"values", out var valuesValue))
            {
                var parsedValue = ParseBencodexValue(valuesValue);
                if (parsedValue is BsonDocument doc)
                {
                    values = doc;
                }
            }

            var parsedAction =
                ParseBencodexValue(decodedAction) as BsonDocument ?? new BsonDocument();

            return (typeId, values, parsedAction);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse action: {ex.Message}", ex);
        }
    }

    private static BsonValue ParseBencodexValue(IValue value)
    {
        return value switch
        {
            Dictionary dict => ParseDictionary(dict),
            List list => ParseList(list),
            Text text => new BsonString(text.Value),
            Integer integer => new BsonString(integer.Value.ToString(CultureInfo.InvariantCulture)),
            Binary binary => new BsonString(Convert.ToHexString(binary.ToByteArray()).ToLower()),
            Bencodex.Types.Boolean boolean => new BsonBoolean(boolean.Value),
            Null => BsonNull.Value,
            _ => new BsonString(value.ToString()),
        };
    }

    private static BsonDocument ParseDictionary(Dictionary dict)
    {
        var bsonDoc = new BsonDocument();
        foreach (var kvp in dict)
        {
            string key;
            if (kvp.Key is Text text)
            {
                key = text.Value;
            }
            else if (kvp.Key is Integer)
            {
                key = kvp.Key.ToString();
            }
            else if (kvp.Key is Binary binary)
            {
                key = Convert.ToHexString(binary.ToByteArray()).ToLower();
            }
            else
            {
                key = kvp.Key.ToString();
            }

            bsonDoc.Add(key, ParseBencodexValue(kvp.Value));
        }
        return bsonDoc;
    }

    private static BsonArray ParseList(List list)
    {
        var bsonArray = new BsonArray();
        foreach (var item in list)
        {
            bsonArray.Add(ParseBencodexValue(item));
        }
        return bsonArray;
    }

    public static List<Address> ExtractAvatarAddress(string raw)
    {
        var avatarKeys = new[]
        {
            SerializeKeys.EnemyAvatarAddressKey,
            SerializeKeys.MyAvatarAddressKey,
            SerializeKeys.AvatarAddressKey,
            SerializeKeys.BuyerAvatarAddressKey,
            SerializeKeys.LegacySellerAvatarAddressKey,
            SerializeKeys.SellerAvatarAddressKey,
            "avatarAddress",
        };

        var decodedAction = Codec.Decode(Convert.FromHexString(raw));

        if (decodedAction is not Dictionary actionDict)
        {
            throw new InvalidCastException(
                $"Invalid action type. Expected Dictionary, got {decodedAction.GetType().Name}."
            );
        }

        var avatarAddresses = new List<Address>();
        if (actionDict.TryGetValue((Text)"values", out var valuesValue))
        {
            if (valuesValue is not Dictionary valuesDict)
            {
                return avatarAddresses;
            }

            foreach (var key in avatarKeys)
            {
                if (valuesDict.ContainsKey((Text)key))
                {
                    var avatarAddress = new Address(valuesDict[key]);

                    avatarAddresses.Add(avatarAddress);
                }
            }
        }

        return avatarAddresses;
    }
}
