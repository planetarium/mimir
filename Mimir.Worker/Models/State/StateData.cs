using Libplanet.Crypto;
using Nekoyume.Model.State;

namespace Mimir.Worker.Models;

public record StateData(Address Address, IState State) : BaseData;
