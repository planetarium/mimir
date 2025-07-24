using Libplanet.Crypto;
using Mimir.Worker.Util;
using MongoDB.Bson;
using Xunit;

namespace Mimir.Worker.Tests.Util;

public class ActionParserTests
{
    [Fact]
    public void ParseAction_WithValidRawData_ReturnsCorrectParsedData()
    {
        var rawData =
            "6475373a747970655f69647531363a6861636b5f616e645f736c617368323275363a76616c756573647531323a617053746f6e65436f756e7475313a307531333a6176617461724164647265737332303ae06aa76fbc394bd5f0e97179d1d955de324f15be75383a636f7374756d65736c657531303a65717569706d656e74736c31363a89a665068cfc484782326d0a2398b9bd31363a122f0c7e14ce744c988d4c672bf6fea231363a8e4d47d398ab6147b56d6ee156cfd9de31363a6df8fcefbdf251478ef5606d3826a7b131363a4f0d5ffc9d52ca49ab0cc2764a058f956575353a666f6f64736c6575323a696431363abb800221e345034dae37a022e94a349c75313a726c6c75313a3075353a3330303031656575373a7374616765496475333a3234397531343a746f74616c506c6179436f756e7475313a3175373a776f726c64496475313a356565";

        var (typeId, values, parsedAction) = ActionParser.ParseAction(rawData);

        Assert.Equal("hack_and_slash22", typeId);
        Assert.NotNull(values);
        Assert.NotNull(parsedAction);

        Assert.Equal("hack_and_slash22", parsedAction["type_id"].AsString);
        Assert.Equal("0", values["apStoneCount"].AsString);
        Assert.Equal("e06aa76fbc394bd5f0e97179d1d955de324f15be", values["avatarAddress"].AsString);
    }

    [Fact]
    public void ExtractAvatarAddress_WithValidRawData_ReturnsCorrectData()
    {
        var rawData =
            "6475373a747970655f69647531363a6861636b5f616e645f736c617368323275363a76616c756573647531323a617053746f6e65436f756e7475313a307531333a6176617461724164647265737332303ae06aa76fbc394bd5f0e97179d1d955de324f15be75383a636f7374756d65736c657531303a65717569706d656e74736c31363a89a665068cfc484782326d0a2398b9bd31363a122f0c7e14ce744c988d4c672bf6fea231363a8e4d47d398ab6147b56d6ee156cfd9de31363a6df8fcefbdf251478ef5606d3826a7b131363a4f0d5ffc9d52ca49ab0cc2764a058f956575353a666f6f64736c6575323a696431363abb800221e345034dae37a022e94a349c75313a726c6c75313a3075353a3330303031656575373a7374616765496475333a3234397531343a746f74616c506c6179436f756e7475313a3175373a776f726c64496475313a356565";

        var avatarAddresses = ActionParser.ExtractAvatarAddress(rawData);

        Assert.Contains(new Address("0xe06aa76fbc394bd5f0e97179d1d955de324f15be"), avatarAddresses);
    }

    [Fact]
    public void ExtractNCGAmount_WithValidNCGAmount_ReturnsCorrectDecimalString()
    {
        var rawData = "6475373a747970655f69647531353a7472616e736665725f61737365743575363a76616c7565736475363a616d6f756e746c647531333a646563696d616c506c61636573313a0275373a6d696e746572736c32303a47d082a115c63e7b58b1532d20e631538eafadde6575363a7469636b657275333a4e43476569353530656575343a6d656d6f75303a75393a726563697069656e7432303acd01d2d7d7dd82da471ce7973205f0e7da0deb9e75363a73656e64657232303a491d9842ed8f1b5d291272cf9e7b66a7b7c90cda6565";
        var result = ActionParser.ExtractNCGAmount(rawData);
        Assert.Equal("5.50", result);
    }

    [Fact]
    public void ExtractRecipient_WithValidRecipient_ReturnsCorrectAddress()
    {
        var rawData = "6475373a747970655f69647531353a7472616e736665725f61737365743575363a76616c7565736475363a616d6f756e746c647531333a646563696d616c506c61636573313a0275373a6d696e746572736c32303a47d082a115c63e7b58b1532d20e631538eafadde6575363a7469636b657275333a4e43476569353530656575343a6d656d6f75303a75393a726563697069656e7432303acd01d2d7d7dd82da471ce7973205f0e7da0deb9e75363a73656e64657232303a491d9842ed8f1b5d291272cf9e7b66a7b7c90cda6565";
        var result = ActionParser.ExtractRecipient(rawData);
        Assert.Equal(new Address("0xcd01d2d7d7dd82da471ce7973205f0e7da0deb9e"), result);
    }

    [Fact]
    public void ExtractSender_WithValidSender_ReturnsCorrectAddress()
    {
        var rawData = "6475373a747970655f69647531353a7472616e736665725f61737365743575363a76616c7565736475363a616d6f756e746c647531333a646563696d616c506c61636573313a0275373a6d696e746572736c32303a47d082a115c63e7b58b1532d20e631538eafadde6575363a7469636b657275333a4e43476569353530656575343a6d656d6f75303a75393a726563697069656e7432303acd01d2d7d7dd82da471ce7973205f0e7da0deb9e75363a73656e64657232303a491d9842ed8f1b5d291272cf9e7b66a7b7c90cda6565";
        var result = ActionParser.ExtractSender(rawData);
        Assert.Equal(new Address("0x491d9842ed8f1b5d291272cf9e7b66a7b7c90cda"), result);
    }

    [Fact]
    public void ExtractRecipient_WithMissingRecipient_ReturnsNull()
    {
        var rawData = "6475373a747970655f69647531353a7472616e736665725f61737365743575363a76616c7565736475363a616d6f756e746c647531333a646563696d616c506c61636573313a0275373a6d696e746572736c32303a47d082a115c63e7b58b1532d20e631538eafadde6575363a7469636b657275333a4e43476569353530656575343a6d656d6f75303a75363a73656e64657232303a491d9842ed8f1b5d291272cf9e7b66a7b7c90cda6565";
        var result = ActionParser.ExtractRecipient(rawData);
        Assert.Null(result);
    }

    [Fact]
    public void ExtractSender_WithMissingSender_ReturnsNull()
    {
        var rawData = "6475373a747970655f69647531353a7472616e736665725f61737365743575363a76616c7565736475363a616d6f756e746c647531333a646563696d616c506c61636573313a0275373a6d696e746572736c32303a47d082a115c63e7b58b1532d20e631538eafadde6575363a7469636b657275333a4e43476569353530656575343a6d656d6f75303a75393a726563697069656e7432303acd01d2d7d7dd82da471ce7973205f0e7da0deb9e6565";
        var result = ActionParser.ExtractSender(rawData);
        Assert.Null(result);
    }

    [Fact]
    public void ExtractRecipient_WithInvalidRawData_ReturnsNull()
    {
        var invalidRawData = "invalid_hex_data";
        var result = ActionParser.ExtractRecipient(invalidRawData);
        Assert.Null(result);
    }

    [Fact]
    public void ExtractSender_WithInvalidRawData_ReturnsNull()
    {
        var invalidRawData = "invalid_hex_data";
        var result = ActionParser.ExtractSender(invalidRawData);
        Assert.Null(result);
    }

    [Fact]
    public void ParseAction_WithInvalidRawData_ThrowsException()
    {
        var invalidRawData = "invalid_hex_data";

        Assert.Throws<InvalidOperationException>(() => ActionParser.ParseAction(invalidRawData));
    }

    [Fact]
    public void ParseAction_WithNonDictionaryData_ThrowsException()
    {
        var rawData = "6c68656c6c6f"; // "hello" as hex

        Assert.Throws<InvalidOperationException>(() => ActionParser.ParseAction(rawData));
    }
}
