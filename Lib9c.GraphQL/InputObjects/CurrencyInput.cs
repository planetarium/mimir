using Lib9c.GraphQL.Enums;
using Libplanet.Crypto;

namespace Lib9c.GraphQL.InputObjects;

public record CurrencyInput(
    CurrencyMethodType CurrencyMethodType,
    string Ticker,
    byte DecimalPlaces,
    Address[]? Minters,
    bool TotalSupplyTrackable,
    long? MaximumSupplyMajor,
    long? MaximumSupplyMinor);
