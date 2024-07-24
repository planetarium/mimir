// using System.Numerics;
// using Bencodex.Types;
// using Lib9c;
// using Libplanet.Crypto;
// using Mimir.Worker.Exceptions;
// using Mimir.Worker.Handler;
// using Mimir.Worker.Handler.AdventureBoss;
// using Mimir.Worker.Models.State.AdventureBoss;
// using Nekoyume.Model.AdventureBoss;

// namespace Mimir.Worker.Tests.Handler.AdventureBoss;

// public class ExploreBoardHandlerTests
// {
//     private readonly ExploreBoardHandler _handler = new();

//     [Theory]
//     [InlineData(0, false)]
//     [InlineData(int.MaxValue, true)]
//     public void ConvertToBencodable_ExploreBoard(long season, bool initOthers)
//     {
//         var address = new PrivateKey().Address;
//         var state = new ExploreBoard(season);
//         if (initOthers)
//         {
//             state.ExplorerCount = int.MaxValue;
//             state.UsedApPotion = long.MaxValue;
//             state.UsedGoldenDust = long.MaxValue;
//             state.UsedNcg = BigInteger.One;
//             state.TotalPoint = long.MaxValue;
//             state.FixedRewardItemId = int.MaxValue;
//             state.FixedRewardFavId = int.MaxValue;
//             state.RaffleWinner = new PrivateKey().Address;
//             state.RaffleWinnerName = "name";
//             state.RaffleReward = 1 * Currencies.Crystal;
//         }

//         var context = new StateDiffContext
//         {
//             Address = address,
//             RawState = state.Bencoded(),
//         };
//         var stateData = _handler.ConvertToStateData(context);

//         Assert.IsType<ExploreBoardState>(stateData.State);
//         var dataState = (ExploreBoardState)stateData.State;
//         var obj = dataState.Object;
//         Assert.Equal(season, obj.Season);
//         Assert.Equal(state.ExplorerCount, obj.ExplorerCount);
//         Assert.Equal(state.UsedApPotion, obj.UsedApPotion);
//         Assert.Equal(state.UsedGoldenDust, obj.UsedGoldenDust);
//         Assert.Equal(state.UsedNcg, obj.UsedNcg);
//         Assert.Equal(state.TotalPoint, obj.TotalPoint);
//         Assert.Equal(state.FixedRewardItemId, obj.FixedRewardItemId);
//         Assert.Equal(state.FixedRewardFavId, obj.FixedRewardFavId);
//         Assert.Equal(state.RaffleWinner, obj.RaffleWinner);
//         Assert.Equal(state.RaffleWinnerName, obj.RaffleWinnerName);
//         Assert.Equal(state.RaffleReward, obj.RaffleReward);
//     }

//     [Theory]
//     [InlineData(0, false)]
//     [InlineData(int.MaxValue, true)]
//     public void ConvertToBencodable_Explorer(long season, bool initOthers)
//     {
//         var address = new PrivateKey().Address;
//         var state = new Explorer(
//             avatarAddress: new PrivateKey().Address,
//             name: "name");
//         if (initOthers)
//         {
//             state.Score = int.MaxValue;
//             state.Floor = int.MaxValue;
//             state.MaxFloor = int.MaxValue;
//             state.UsedApPotion = int.MaxValue;
//             state.UsedGoldenDust = int.MaxValue;
//             state.UsedNcg = int.MaxValue;
//             state.Claimed = true;
//         }

//         var context = new StateDiffContext
//         {
//             Address = address,
//             RawState = state.Bencoded,
//         };
//         var stateData = _handler.ConvertToStateData(context);

//         Assert.IsType<ExplorerState>(stateData.State);
//         var dataState = (ExplorerState)stateData.State;
//         var obj = dataState.Object;
//         Assert.Equal(state.AvatarAddress, obj.AvatarAddress);
//         Assert.Equal(state.Name, obj.Name);
//         Assert.Equal(state.Score, obj.Score);
//         Assert.Equal(state.Floor, obj.Floor);
//         Assert.Equal(state.MaxFloor, obj.MaxFloor);
//         Assert.Equal(state.UsedApPotion, obj.UsedApPotion);
//         Assert.Equal(state.UsedGoldenDust, obj.UsedGoldenDust);
//         Assert.Equal(state.UsedNcg, obj.UsedNcg);
//         Assert.Equal(state.Claimed, obj.Claimed);
//     }

//     [Fact]
//     public void ConvertToBencodable_Throw_UnexpectedStateException()
//     {
//         var context = new StateDiffContext
//         {
//             Address = new PrivateKey().Address,
//             RawState = List.Empty,
//         };
//         Assert.Throws<UnexpectedStateException>(() => _handler.ConvertToStateData(context));
//     }
// }
