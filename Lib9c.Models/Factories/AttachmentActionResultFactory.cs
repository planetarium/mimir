using Bencodex.Types;
using Lib9c.Models.AttachmentActionResults;
using Lib9c.Models.Exceptions;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Factories;

public static class AttachmentActionResultFactory
{
    public static AttachmentActionResult Create(IValue bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        var typeId = ((Text)d["typeId"]).Value;
        return typeId switch
        {
            "buy.buyerResult" => new Buy7BuyerResult(d),
            "buy.sellerResult" => new Buy7SellerResult(d),
            "combination.result-model" => new CombinationConsumable5Result(d),
            "itemEnhancement.result" => new ItemEnhancement7Result(d),
            "item_enhancement9.result" => new ItemEnhancement9Result(d),
            "item_enhancement11.result" => new ItemEnhancement11Result(d),
            "item_enhancement12.result" => new ItemEnhancement12Result(d),
            "item_enhancement13.result" => new ItemEnhancement13Result(d),
            "sellCancellation.result" => new SellCancellationResult(d),
            "rapidCombination.result" => new RapidCombination0Result(d),
            "rapid_combination5.result" => new RapidCombination5Result(d),
            "dailyReward.dailyRewardResult" => new DailyReward2Result(d),
            "monsterCollection.result" => new MonsterCollectionResult(d),
            _ => throw new UnsupportedArgumentValueException<string>("typeId", typeId),
        };
    }
}
