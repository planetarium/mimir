// using Bencodex.Types;
// using Libplanet.Crypto;
// using Nekoyume.Model.Market;

// namespace Mimir.MongoDB.Bson;

// public record ProductDocument : IMimirBsonDocument(Address)
// {
//     public Address AvatarAddress { get; init; }
//     public Address ProductsStateAddress { get; init; }
//     public Product Object { get; init; }
//     public int? CombatPoint { get; init; }
//     public decimal? UnitPrice { get; init; }
//     public int? Crystal { get; init; }
//     public int? CrystalPerPrice { get; init; }

//     public ProductDocument(
//         Address avatarAddress,
//         Address productsStateAddress,
//         Product product)
//     {
//         Object = product;
//         AvatarAddress = avatarAddress;
//         ProductsStateAddress = productsStateAddress;
//     }

//     public ProductDocument(
//         Address avatarAddress,
//         Address productsStateAddress,
//         Product product,
//         decimal unitPrice,
//         int? combatPoint,
//         int? crystal,
//         int? crystalPerPrice)
//     {
//         Object = product;
//         AvatarAddress = avatarAddress;
//         ProductsStateAddress = productsStateAddress;
//         CombatPoint = combatPoint;
//         UnitPrice = unitPrice;
//         Crystal = crystal;
//         CrystalPerPrice = crystalPerPrice;
//     }

//     public IValue Bencoded => Object.Serialize();
// }
