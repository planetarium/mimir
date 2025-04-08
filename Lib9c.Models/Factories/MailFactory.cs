using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Mails;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Factories;

public static class MailFactory
{
    public static Mail Create(IValue bencoded)
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
            // Write in alphabetical order.
            nameof(AdventureBossRaffleWinnerMail) => new AdventureBossRaffleWinnerMail(d),
            "buyerMail" => new BuyerMail(d),
            nameof(CancelOrderMail) => new CancelOrderMail(d),
            nameof(ClaimItemsMail) => new ClaimItemsMail(d),
            "combinationMail" => new CombinationMail(d),
            nameof(CustomCraftMail) => new CustomCraftMail(d),
            "dailyRewardMail" => new DailyRewardMail(d),
            nameof(GrindingMail) => new GrindingMail(d),
            "itemEnhance" => new ItemEnhanceMail(d),
            nameof(MaterialCraftMail) => new MaterialCraftMail(d),
            "monsterCollectionMail" => new MonsterCollectionMail(d),
            nameof(OrderBuyerMail) => new OrderBuyerMail(d),
            nameof(OrderExpirationMail) => new OrderExpirationMail(d),
            nameof(OrderSellerMail) => new OrderSellerMail(d),
            nameof(ProductBuyerMail) => new ProductBuyerMail(d),
            nameof(ProductCancelMail) => new ProductCancelMail(d),
            nameof(ProductSellerMail) => new ProductSellerMail(d),
            "sellCancel" => new SellCancelMail(d),
            "seller" => new SellerMail(d),
            nameof(UnloadFromMyGaragesRecipientMail) => new UnloadFromMyGaragesRecipientMail(d),
            nameof(PatrolRewardMail) => new PatrolRewardMail(d),
            nameof(WorldBossRewardMail) => new WorldBossRewardMail(d),
            _ => throw new UnsupportedArgumentValueException<string>("typeId", typeId),
        };
    }
}
