using Libplanet.Crypto;
using Mimir.Models.Abstractions;

namespace Mimir.Models;

public record StateModel(Address Address) : IStateModel;
