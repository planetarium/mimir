// using Lib9c;
// using Libplanet.Crypto;
// using Mimir.Worker.Handler;
// using Mimir.Worker.Handler.AdventureBoss;
// using Mimir.Worker.Models.State.AdventureBoss;
// using Nekoyume.Model.AdventureBoss;

// namespace Mimir.Worker.Tests.Handler.AdventureBoss;

// public class BountyBoardHandlerTests
// {
//     private readonly BountyBoardHandler _handler = new();

//     [Theory]
//     [InlineData(0, false)]
//     [InlineData(int.MaxValue, true)]
//     public void ConvertToStateData(long season, bool initOthers)
//     {
//         var address = new PrivateKey().Address;
//         var state = new BountyBoard(season);
//         if (initOthers)
//         {
//             state.Investors.Add(new Investor(
//                 avatarAddress: new PrivateKey().Address,
//                 name: "name",
//                 price: 1 * Currencies.Crystal));
//             state.FixedRewardItemId = int.MaxValue;
//             state.FixedRewardFavId = int.MaxValue;
//             state.RandomRewardItemId = int.MaxValue;
//             state.RandomRewardFavId = int.MaxValue;
//         }

//         var context = new StateDiffContext
//         {
//             Address = address,
//             RawState = state.Bencoded,
//         };
//         var stateData = _handler.ConvertToStateData(context);

//         Assert.IsType<BountyBoardState>(stateData.State);
//         var dataState = (BountyBoardState)stateData.State;
//         var obj = dataState.Object;
//         Assert.Equal(season, obj.Season);
//         if (initOthers)
//         {
//             Assert.Single(obj.Investors);
//             var expectedInvestor = state.Investors[0];
//             var actualInvestor = obj.Investors[0];
//             Assert.Equal(expectedInvestor.AvatarAddress, actualInvestor.AvatarAddress);
//             Assert.Equal(expectedInvestor.Name, actualInvestor.Name);
//             Assert.Equal(expectedInvestor.Price, actualInvestor.Price);
//         }
//         else
//         {
//             Assert.Empty(obj.Investors);
//         }

//         Assert.Equal(state.FixedRewardItemId, obj.FixedRewardItemId);
//         Assert.Equal(state.FixedRewardFavId, obj.FixedRewardFavId);
//         Assert.Equal(state.RandomRewardItemId, obj.RandomRewardItemId);
//         Assert.Equal(state.RandomRewardFavId, obj.RandomRewardFavId);
//     }
// }
