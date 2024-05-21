using Libplanet.Types.Assets;

namespace Mimir.Models.Assets;

public class Balance
{
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
}
