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

    public static string? ExtractNCGAmount(string raw)
    {
        try
        {
            var decodedAction = Codec.Decode(Convert.FromHexString(raw));

            if (decodedAction is not Dictionary actionDict)
            {
                return null;
            }

            if (!actionDict.TryGetValue((Text)"values", out var valuesValue) || valuesValue is not Dictionary valuesDict)
            {
                return null;
            }

            if (!valuesDict.TryGetValue((Text)"amount", out var amountValue))
            {
                return null;
            }

            return ParseNCGAmount(amountValue);
        }
        catch
        {
            return null;
        }
    }

    public static string? ExtractRecipient(string raw)
    {
        try
        {
            var decodedAction = Codec.Decode(Convert.FromHexString(raw));

            if (decodedAction is not Dictionary actionDict)
            {
                return null;
            }

            if (!actionDict.TryGetValue((Text)"values", out var valuesValue) || valuesValue is not Dictionary valuesDict)
            {
                return null;
            }

            if (!valuesDict.TryGetValue((Text)"recipient", out var recipientValue))
            {
                return null;
            }

            return ParseAddress(recipientValue);
        }
        catch
        {
            return null;
        }
    }

    public static string? ExtractSender(string raw)
    {
        try
        {
            var decodedAction = Codec.Decode(Convert.FromHexString(raw));

            if (decodedAction is not Dictionary actionDict)
            {
                return null;
            }

            if (!actionDict.TryGetValue((Text)"values", out var valuesValue) || valuesValue is not Dictionary valuesDict)
            {
                return null;
            }

            if (!valuesDict.TryGetValue((Text)"sender", out var senderValue))
            {
                return null;
            }

            return ParseAddress(senderValue);
        }
        catch
        {
            return null;
        }
    }

    private static string? ParseNCGAmount(IValue amountValue)
    {
        if (amountValue is not List amountList || amountList.Count < 2)
        {
            return null;
        }

        var currencyInfo = amountList[0];
        var amount = amountList[1];

        if (currencyInfo is not Dictionary currencyDict || amount is not Integer amountInt)
        {
            return null;
        }

        if (!currencyDict.TryGetValue((Text)"ticker", out var tickerValue) || tickerValue is not Text ticker)
        {
            return null;
        }

        if (ticker.Value != "NCG")
        {
            return null;
        }

        if (!currencyDict.TryGetValue((Text)"decimalPlaces", out var decimalPlacesValue) || decimalPlacesValue is not Binary decimalPlacesBinary)
        {
            return null;
        }

        var decimalPlacesBytes = decimalPlacesBinary.ToByteArray();
        if (decimalPlacesBytes.Length == 0)
        {
            return null;
        }

        var decimalPlaces = decimalPlacesBytes[0];
        var divisor = Math.Pow(10, decimalPlaces);
        var result = (decimal)amountInt.Value / (decimal)divisor;

        return result.ToString("F" + decimalPlaces);
    }

    private static string? ParseAddress(IValue addressValue)
    {
        if (addressValue is Binary addressBinary)
        {
            return Convert.ToHexString(addressBinary.ToByteArray()).ToLower();
        }

        if (addressValue is Text addressText)
        {
            return addressText.Value;
        }

        return null;
    }
}
