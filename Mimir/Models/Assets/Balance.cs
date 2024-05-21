using System.Security.Cryptography;
using Libplanet.Common;
using Libplanet.Types.Assets;

namespace Mimir.Models.Assets;

public class Balance
{
    private static readonly Dictionary<HashDigest<SHA1>, Balance> EmptyBalances = new();

    public Currency Currency { get; set; }
    public string Quantity { get; set; }

    public Balance(Currency currency, string quantity)
    {
        Currency = currency;
        Quantity = quantity;
    }

    // TODO: Specify the BSON schema for the Balance class.
    // TODO: Implement the following constructor.
    // public Balance(BsonDocument balance)
    // {
    //     Currency = new Currency(balance["Currency"].AsBsonDocument);
    //     Quantity = balance["Quantity"].AsString;
    // }

    public static Balance Empty(Currency currency)
    {
        if (!EmptyBalances.TryGetValue(currency.Hash, out var value))
        {
            value = new Balance(currency, "0");
            EmptyBalances[currency.Hash] = value;
        }

        return value;
    }
}
