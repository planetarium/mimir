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
                [ValueKind.Dictionary],
                bencoded.Kind);
        }

        var typeId = ((Text)d["typeId"]).Value;
        return typeId switch
        {
            "buyerMail" => new BuyerMail(d),
            "combinationMail" => new CombinationMail(d),
            "sellCancel" => new SellCancelMail(d),
            "seller" => new SellerMail(d),
            "itemEnhance" => new ItemEnhanceMail(d),
            "dailyRewardMail" => new DailyRewardMail(d),
            "monsterCollectionMail" => new MonsterCollectionMail(d),
            nameof(OrderExpirationMail) => new OrderExpirationMail(d),
            nameof(CancelOrderMail) => new CancelOrderMail(d),
            nameof(OrderBuyerMail) => new OrderBuyerMail(d),
            nameof(OrderSellerMail) => new OrderSellerMail(d),
            nameof(GrindingMail) => new GrindingMail(d),
            nameof(MaterialCraftMail) => new MaterialCraftMail(d),
            nameof(ProductBuyerMail) => new ProductBuyerMail(d),
            nameof(ProductSellerMail) => new ProductSellerMail(d),
            nameof(ProductCancelMail) => new ProductCancelMail(d),
            nameof(UnloadFromMyGaragesRecipientMail) => new UnloadFromMyGaragesRecipientMail(d),
            nameof(ClaimItemsMail) => new ClaimItemsMail(d),
            nameof(AdventureBossRaffleWinnerMail) => new AdventureBossRaffleWinnerMail(d),
            _ => throw new UnsupportedArgumentValueException<string>("typeId", typeId),
        };
    }
}
